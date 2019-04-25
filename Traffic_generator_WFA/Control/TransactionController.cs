using DevExpress.XtraEditors;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using Nethereum.Contracts;
using Nethereum.Hex.HexTypes;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.RPC.Eth.Filters;
using Nethereum.StandardTokenEIP20.ContractDefinition;
using Nethereum.Util;
using Nethereum.Web3;
using Nethereum.Web3.Accounts.Managed;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Sockets;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Traffic_generator_WFA.Models;

namespace Traffic_generator_WFA.Control
{
    public class TransactionController
    {
        public List<string> accList = new List<string>();
        public List<string> walletList = new List<string>();
        public List<TransactionReceipt> receipts = new List<TransactionReceipt>();
        public List<Token> tokenList = new List<Token>();
        public List<MongoTransactionCount> blockList = new List<MongoTransactionCount>();
        public List<MongoTransactionCount> transactionList = new List<MongoTransactionCount>();

        public List<HistogramRecord> blockRangeHistogram = new List<HistogramRecord>();
        public List<HistogramRecord> transactionHistogram = new List<HistogramRecord>();
        public List<HistogramRecord> countHistogram = new List<HistogramRecord>();
        public List<Range> generatedTransactionHistogram = new List<Range>();
        public List<Range> generatedBlockHistogram = new List<Range>();
        public List<Range> originalTransactionHistogram = new List<Range>();
        public List<Range> originalBlockHistogram = new List<Range>();

        public List<Range> ranges = new List<Range>();
        public List<Range> blockRanges = new List<Range>();
        public double blockPercentage = 0;
        
        public HexBigInteger pendingFilter;

        public Random randVolume = new Random();
        public Random randVolumeValue = new Random();
        public Random randBlock = new Random();
        public Random randBlockValue = new Random();

        public void TransactionSendingAsync(Web3 web3, string masterAcc, string walletContract, string passwd)
        {
            //for(var x = 0; x <= 150000; x++)
            //{
            //    var blockCount = GetRandomBlockValue();
            //    AddBlockToHistogram(blockCount);
            //}

            //for (var x = 0; x <= 200000; x++)
            //{
            //    var blockCount = GetRandomVolumeValue();
            //    AddTransactionToHistogram(blockCount);
            //}

            Random rand = new Random();
            int latestBlock = int.Parse(web3.Eth.Blocks.GetBlockNumber.SendRequestAsync().GetAwaiter().GetResult().Value.ToString());
            var mainAccount = new ManagedAccount(masterAcc, passwd);

            #region Traffic
            BigInteger balance = 0;
            pendingFilter = web3.Eth.Filters.NewPendingTransactionFilter.SendRequestAsync().GetAwaiter().GetResult();

            while (!Program.init.mw.hc.trafficStop)
            {
                if (Program.init.mw.hc.trafficPlay)
                {
                    var newLatestBlock = int.Parse(web3.Eth.Blocks.GetBlockNumber.SendRequestAsync().GetAwaiter().GetResult().Value.ToString());    //get latest block
                    if (newLatestBlock != latestBlock)                                                                                              //if new block already kicked in...
                    {
                        if (rand.NextDouble() <= 0.5) //blockPercentage)
                        {
                            var blockCount = GetRandomBlockValue();
                            AddBlockToHistogram(blockCount);
                            for (int i = 0; i < blockCount; i++)
                            {
                                var tx = GetRandomVolumeValue();
                                Console.WriteLine(tx + " Tokens");

                                string firstAddress = null;
                                List<string> accs = accList.ToList();

                                do
                                {
                                    if (accs.Count == 0)
                                    {
                                        firstAddress = masterAcc;
                                        break;
                                    }

                                    firstAddress = accs[rand.Next(accs.Count)];  //take random address
                                    accs.Remove(firstAddress);
                                    try
                                    {

                                        var balanceOfFunctionMessage = new BalanceOfFunction()      //get balance and check, if accound has enough tokens
                                        {
                                            Owner = firstAddress,
                                        };

                                        var balanceHandler = web3.Eth.GetContractQueryHandler<BalanceOfFunction>();
                                        balance = balanceHandler.QueryAsync<BigInteger>(walletContract, balanceOfFunctionMessage).GetAwaiter().GetResult();
                                    }
                                    catch (Exception e)
                                    {
                                        throw e;
                                    }
                                } while (balance < new BigInteger(tx));

                                var secondAddress = accList[rand.Next(accList.Count)];
                                do
                                {
                                    secondAddress = accList[rand.Next(accList.Count)];    //take random address

                                } while (firstAddress == secondAddress);                      //eliminate sending to same address
                                SendTokens(firstAddress, secondAddress, tx);
                            }
                        }
                        latestBlock = newLatestBlock;
                    }
                }
                else
                    Thread.Sleep(200);
            }
            #endregion

            Program.init.mw.trafficInitialized = false;
        }

        private double CalculateStdDev(List<MongoTransactionCount> values, int totalTx)
        {
            double totalSum = 0;
            foreach (var item in values)
            {
                totalSum += item.Amount * item.Count;
            }
            double avg = totalSum / totalTx;
            totalSum = 0; 

            foreach (var item in values)
            {
                totalSum += Math.Pow((item.Amount - avg), 2) * item.Count;
            }

            return Math.Sqrt(totalSum / (totalTx - 1));
        }

        public int GetMongoTransaction(string address, BackgroundWorker bw)
        {
            var filter = new BsonDocument { { "tokenContract", address } };
            var connectionString = "mongodb://localhost:27017";
            bw.ReportProgress(50);
            /**************************************** Mongo Query ******************************************/
            //
            //      db.getCollection('transactions').aggregate(
            //      [
            //          { $match: { tokenContract: "0xd26114cd6EE289AccF82350c8d8487fedB8A0C07"} },
            //          { $group: { _id: "$amount", count: { $sum: 1 } } },
            //          { $sort: { _id: 1 } }   
            //      ], { allowDiskUse: true })
            //
            /***********************************************************************************************/

            try
            {
                MongoClient client = new MongoClient(connectionString);
                var db = client.GetDatabase("DP");

                var match = new BsonDocument { { "$match", new BsonDocument { { "tokenContract",  address } } } };
                var group = new BsonDocument { { "$group", new BsonDocument { { "_id", "$amount" }, { "count", new BsonDocument { { "$sum", 1} } } } } };
                var sort = new BsonDocument { { "$sort", new BsonDocument { { "_id", 1} } } };
                var project = new BsonDocument { { "$project", new BsonDocument { { "amount", "$_id" }, { "count", "$count"} } } };

                var pipeline = new[] { match, group, sort, project };
                var coll = db.GetCollection<BsonDocument>("transactions");
                var result = coll.Aggregate<BsonDocument>(pipeline, new AggregateOptions { AllowDiskUse = true });

                transactionList = result.ToList().Select(x => BsonSerializer.Deserialize<MongoTransactionCount>(x)).Where(x => x.Amount > 0).ToList();
                var temp = transactionList.Last();

                bw.ReportProgress(75);
                CreateTransactionHistogram();
                return 0;
            }
            catch (Exception e)
            {
                if (e is MongoConnectionException || e is MongoConnectionClosedException || e is SocketException || e is TimeoutException)
                {
                    System.Windows.Forms.MessageBox.Show("A problem occured when trying to connectiong to Mongo database.");
                    return 1;
                }
                else
                {
                    System.Windows.Forms.MessageBox.Show("A problem occured while fetching transactions.");
                    return 1;
                }
            }
        }

        public int GetMongoBlocks(string address, Web3 web3, BackgroundWorker bw)
        {
            var connectionString = "mongodb://localhost:27017";
            bw.ReportProgress(4);
            /**********************************************Mongo Query****************************************************/
            //
            //      db.getCollection('transactions').aggregate(
            //          [
            //              { $match: { tokenContract: "0xd26114cd6EE289AccF82350c8d8487fedB8A0C07"} },
            //              { $group: { _id: "$blockNumber", transactions: { $sum: 1 } } },
            //              { $group: { _id: "$transactions", count: { $sum: 1}}},
            //              { $sort: { _id: 1 } }   
            //          ], { allowDiskUse: true })
            //
            /************************************************************************************************************/

            try
            {
                MongoClient client = new MongoClient(connectionString);
                var db = client.GetDatabase("DP");

                var match = new BsonDocument { { "$match", new BsonDocument { { "tokenContract", address } } } };
                var group = new BsonDocument { { "$group", new BsonDocument { { "_id", "$blockNumber" }, { "transactions", new BsonDocument { { "$sum", 1 } } } } } };
                var additionalGroup = new BsonDocument { { "$group", new BsonDocument { { "_id", "$transactions" }, { "count", new BsonDocument { { "$sum", 1 } } } } } };
                var sort = new BsonDocument { { "$sort", new BsonDocument { { "_id", 1 } } } };
                var project = new BsonDocument { { "$project", new BsonDocument { { "amount", "$_id" }, { "count", "$count" } } } };
                var count = new BsonDocument { { "$count", "transactions"} };

                var pipeline = new[] { match, group, additionalGroup, sort, project };
                var coll = db.GetCollection<BsonDocument>("transactions");
                var result = coll.Aggregate<BsonDocument>(pipeline, new AggregateOptions { AllowDiskUse = true });
                blockList = result.ToList().Select(x => BsonSerializer.Deserialize<MongoTransactionCount>(x)).Where(x => x.Amount > 0).ToList();

                pipeline = new[] { match, group, count };
                var blockCount = coll.Aggregate<BsonDocument>(pipeline, new AggregateOptions { AllowDiskUse = true });
                int bc = blockCount.First().GetValue(0).AsInt32;

                int latestBlock = int.Parse(web3.Eth.Blocks.GetBlockNumber.SendRequestAsync().GetAwaiter().GetResult().Value.ToString());

                var coll2 = db.GetCollection<MongoTransaction>("transactions");
                var firstIcoBlock = coll2.Find(x => x.TokenContract == address).SortBy(x => x.BlockNumber).FirstOrDefault().BlockNumber;

                blockPercentage = bc / (double)(latestBlock - firstIcoBlock);

                bw.ReportProgress(25);
                CreateBlockHistogram();
                return 0;
            }
            catch (Exception e)
            {
                return 1;
            }
        }

        public void CreateTransactionHistogram()
        {
            double totalProb = 0;

            int totalTx = transactionList.Sum(x => x.Count);
            double firstQuartile = 0;
            double thirdQuartile = 0;

            int count = 0;
            foreach (var value in transactionList)
            {
                double searchQuartileBelow = count / (double)totalTx;
                double searchQuartileAbove = (count + value.Count) / (double)totalTx;

                if (searchQuartileBelow <= 0.25 && searchQuartileAbove >= 0.25)
                    firstQuartile = value.Amount;
                if (searchQuartileBelow <= 0.75 && searchQuartileAbove >= 0.75)
                    thirdQuartile = value.Amount;
                count += value.Count;
            }

            transactionHistogram = new List<HistogramRecord>();

            var interQuartile = thirdQuartile - firstQuartile;

            var transactionRange = transactionList.Last().Amount - transactionList.First().Amount;
            var deviation = CalculateStdDev(transactionList, totalTx);

            var binNumber2 = Math.Ceiling(transactionRange / (2 * interQuartile / Math.Pow(totalTx, 1 / 3.0)));      //Freedman–Diaconis`s rule
            var binNumber = Math.Ceiling(transactionRange / (3.5 * deviation / Math.Pow(totalTx, 1 / 3.0)));      //Scott`s rule
            var binNumber3 = Math.Ceiling(transactionRange / (1.06 * deviation / Math.Pow(totalTx, 1 / 5.0)));

            for (int i = 0; i < binNumber; i++)
            {
                var bin = new Range();
                bin.FromValue = (transactionRange / binNumber) * i;
                bin.ToValue = (transactionRange / binNumber) * (i + 1);
                bin.Avg = (bin.FromValue + bin.ToValue) / 2;

                ranges.Add(bin);
            }

            foreach (var transaction in transactionList)
            {
                foreach (var bin in ranges) {
                    if (transaction.Amount > bin.FromValue && transaction.Amount <= bin.ToValue)
                        bin.Count += transaction.Count;
                } }

            for (int i = 0; i < ranges.Count; i++)
            {
                ranges[i].Probability = (double)ranges[i].Count / (double)totalTx;
                ranges[i].CDFProbability = totalProb + ranges[i].Probability;
                totalProb = ranges[i].CDFProbability;
            }

            var newRanges = new List<Range>();
            foreach (var range in ranges)
            {
                if (range.Count != 0)
                {
                    newRanges.Add(range);
                    var generatorBin = new Range(range.FromValue, range.ToValue, range.Avg, range.Probability, range.CDFProbability);
                    generatedTransactionHistogram.Add(generatorBin);
                }
            }

            ranges = newRanges.ToList();
        }

        public void CreateBlockHistogram()
        {
            double totalProb = 0;
            int totalTx = blockList.Sum(x => x.Count);
            double firstQuartile = 0;
            double thirdQuartile = 0;

            int count = 0;
            foreach (var value in blockList)
            {
                double searchQuartileBelow = count / (double)totalTx;
                double searchQuartileAbove = (count + value.Count) / (double)totalTx;

                if (searchQuartileBelow <= 0.25 && searchQuartileAbove >= 0.25)
                    firstQuartile = value.Amount;
                if (searchQuartileBelow <= 0.75 && searchQuartileAbove >= 0.75)
                    thirdQuartile = value.Amount;
                count += value.Count;
            }

            blockRangeHistogram = new List<HistogramRecord>();
            var interQuartile = thirdQuartile - firstQuartile;

            var totalBlockRange = blockList.Last().Amount - blockList.First().Amount;
            var deviation = CalculateStdDev(blockList, totalTx);                                                    //need to repair, if we go back to deviation

            var binNumber2 = Math.Ceiling(totalBlockRange / (2 * interQuartile / Math.Pow(totalTx, 1 / 3.0)));      //Freedman–Diaconis`s rule
            var binNumber = Math.Ceiling(totalBlockRange / (3.5 * deviation / Math.Pow(totalTx, 1 / 3.0)));         //Scott`s rule
            var binNumber3 = Math.Ceiling(totalBlockRange / (1.06 * deviation / Math.Pow(totalTx, 1 / 5.0)));

            if (binNumber > totalBlockRange)    //more bin numbers thatn total range of blocks will generate bins with 0 probability
                binNumber = totalBlockRange;

            for (int i = 0; i < binNumber; i++)
            {
                var bin = new Range();
                bin.FromValue = Math.Round((totalBlockRange / binNumber) * i);
                bin.ToValue = Math.Round((totalBlockRange / binNumber) * (i + 1));
                bin.Avg = (bin.FromValue + bin.ToValue) / 2;
                blockRanges.Add(bin);
            }

            foreach (var transaction in blockList)
            {
                foreach (var bin in blockRanges)
                {
                    if (transaction.Amount > bin.FromValue && transaction.Amount <= bin.ToValue)
                        bin.Count += transaction.Count;
                }
            }

            for (int i = 0; i < blockRanges.Count; i++)
            {
                blockRanges[i].Probability = (double)blockRanges[i].Count / (double)totalTx;
                blockRanges[i].CDFProbability = totalProb + blockRanges[i].Probability;
                totalProb = blockRanges[i].CDFProbability;
            }

            var newBlockRanges = new List<Range>();
            foreach (var range in blockRanges)
            {
                if (range.Count != 0)
                {
                    newBlockRanges.Add(range);
                    var generatorBin = new Range(range.FromValue, range.ToValue, range.Avg, range.Probability, range.CDFProbability);
                    generatedBlockHistogram.Add(generatorBin);
                }
            }

            blockRanges = newBlockRanges.ToList();
        }

        public void AddTransactionToHistogram(double value)
        {
            foreach (var bin in generatedTransactionHistogram)
            {
                if (value > bin.FromValue && value <= bin.ToValue)
                {
                    bin.Count++;
                    break;
                }
            }         
        }

        public void AddBlockToHistogram(double value)
        {
            foreach (var bin in generatedBlockHistogram)
            {
                if (value > bin.FromValue && value <= bin.ToValue)
                {
                    bin.Count++;
                    break;
                }
            }
        }

        public double GetRandomBlockValue()
        {
            double randVal = randBlockValue.NextDouble();

            Range range = null;
            double sumProb = 0;
            foreach (var item in blockRanges)
            {
                sumProb += item.Probability;
                range = item;
                if (sumProb >= randVal)
                    break;
            }

            //range.Count++;
            return GetRandomBlock(range.FromValue, range.ToValue);
        }

        public double GetRandomVolumeValue()
        {
            
            double randVal = randVolumeValue.NextDouble();

            Range range = null;
            double sumProb = 0;
            foreach (var item in ranges)
            {
                sumProb += item.Probability;
                range = item;
                if (sumProb >= randVal)
                    break; 
            }

            return GetRandomVolume(range.FromValue, range.ToValue);
        }

        public double GetRandomVolume(double minimum, double maximum)
        {
            double value = randVolume.NextDouble() * (maximum - minimum) + minimum;
  
            return Math.Ceiling(value);
        }

        public double GetRandomBlock(double minimum, double maximum)
        {
            
            double value = randBlock.NextDouble() * (maximum - minimum) + minimum;

            return Math.Round(value);
        }

        public void SendTokens(string from, string to, double value)
        {
            try
            {
                var unlockResult = Program.init.web3.Personal.UnlockAccount.SendRequestAsync(from, Program.init.passwd, 180).GetAwaiter().GetResult();
                
                var transferHandler = Program.init.web3.Eth.GetContractTransactionHandler<TransferFunction>();
                var transfer = new TransferFunction()
                {
                    FromAddress = from,
                    To = to,
                    Value = (int)value,
                    GasPrice = Web3.Convert.ToWei(20, UnitConversion.EthUnit.Gwei),
                    Gas = 80000
                };
                //transfer.Gas = transferHandler.EstimateGasAsync(from, transfer).GetAwaiter().GetResult().Value;
                transferHandler.SendRequestAsync(Program.init.contractProperties.Address, transfer);
                AddTransactionToHistogram(value);
            }
            catch (Exception ex)
            {
                if (ex.Message.Equals("insufficient funds for gas * price + value"))
                {
                    SendGasFromMain(from);
                }
                else throw ex;
            }
        }

        public void SendTokensToMain(string address)
        {
            BigInteger balance = 0;

            try
            {
                var balanceOfFunctionMessage = new BalanceOfFunction()
                {
                    Owner = address,
                };

                var balanceHandler = Program.init.web3.Eth.GetContractQueryHandler<BalanceOfFunction>();
                balance = balanceHandler.QueryAsync<BigInteger>(Program.init.contractProperties.Address, balanceOfFunctionMessage).GetAwaiter().GetResult();

                if (balance > 0)
                {
                    var unlockResult = Program.init.web3.Personal.UnlockAccount.SendRequestAsync(address, Program.init.passwd, 180).GetAwaiter().GetResult();
                    var transferHandler = Program.init.web3.Eth.GetContractTransactionHandler<TransferFunction>();
                    var transfer = new TransferFunction()
                    {
                        FromAddress = address,
                        To = Program.init.masterAcc.Address,
                        Value = balance,
                        Gas = 8000000
                        
                    };
                    transferHandler.SendRequestAsync(Program.init.contractProperties.Address, transfer).GetAwaiter().GetResult();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error!!");
                if (ex.Message.Equals("insufficient funds for gas * price + value"))
                {
                    SendGasFromMain(address);
                    //SendTokensToMain(address);
                }
                else throw ex;
            }
        }

        public void SendGasFromMain(string toAddress) {
            var web3 = new Web3(new ManagedAccount(Program.init.masterAcc.Address, Program.init.passwd));

            try
            {
                var transaction = web3.Eth.GetEtherTransferService()
                    .TransferEtherAndWaitForReceiptAsync(toAddress, 1).GetAwaiter().GetResult();
            }
            catch(Exception e)
            {
                throw e;
            }
        }
    }

}
