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

        public List<Range> ranges = new List<Range>();
        public List<Range> blockRanges = new List<Range>();
        public double blockPercentage = 0;

        public bool trafficInitialized = false;
        public HexBigInteger pendingFilter;

        public async Task TransactionSendingAsync(Contract contract, Web3 web3, MongoAccount masterAcc, Contract walletContract, string passwd)
        {
            Random rand = new Random();
            int latestBlock = int.Parse(web3.Eth.Blocks.GetBlockNumber.SendRequestAsync().GetAwaiter().GetResult().Value.ToString());
            var mainAccount = new ManagedAccount(masterAcc.Address, passwd);

            #region Traffic
            BigInteger balance = 0;
            pendingFilter = await web3.Eth.Filters.NewPendingTransactionFilter.SendRequestAsync();

            while (!Program.init.appClose)
            {
                var newLatestBlock = int.Parse(web3.Eth.Blocks.GetBlockNumber.SendRequestAsync().GetAwaiter().GetResult().Value.ToString());    //get latest block
                if (newLatestBlock != latestBlock)                                                                                              //if new block already kicked in...
                {
                    if (rand.NextDouble() >= blockPercentage)
                    {
                        var blockCount = GetRandomBlockValue();
                        for (int i = 0; i < blockCount; i++)
                        {
                            var tx = GetRandomVolumeValue();
                            Console.WriteLine(tx + " Eth");

                            string firstAddress = null;
                            List<string> accs = accList.ToList();

                            do
                            {
                                if (accs.Count == 0)
                                {
                                    firstAddress = masterAcc.Address;
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
                                    balance = await balanceHandler.QueryAsync<BigInteger>(walletContract.Address, balanceOfFunctionMessage);
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

                        latestBlock = newLatestBlock;
                    }
                }
                else
                    Thread.Sleep(500);
            }
            #endregion
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
            var deviation = CalculateStdDev(transactionList, totalTx);      //need to repair, if we go back to deviation

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

                var generatorBin = new Range(bin.FromValue, bin.ToValue, bin.Avg, bin.Probability, bin.CDFProbability);
                generatedTransactionHistogram.Add(generatorBin);
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

                var generatorBin = new Range(bin.FromValue, bin.ToValue, bin.Avg, bin.Probability, bin.CDFProbability);
                generatedTransactionHistogram.Add(generatorBin);
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

        public double GetRandomBlockValue()
        {
            bool isDouble = false;

            Random rand = new Random();
            double randVal = rand.NextDouble();

            Range range = null;
            double sumProb = 0;
            foreach (var item in blockRanges)
            {
                sumProb += item.Probability;
                range = item;
                if (sumProb >= randVal)
                    break;
            }

            range.Count++;
            return GetRandomVolume(range.FromValue, range.ToValue, isDouble);
        }

        public double GetRandomVolumeValue()
        {
            bool isDouble = true;
            Random formatChooser = new Random();
            double formatVal = formatChooser.NextDouble();
            if (formatVal <= 0.35)
                isDouble = false;

            Random rand = new Random();
            double randVal = rand.NextDouble();

            Range range = null;
            double sumProb = 0;
            foreach (var item in ranges)
            {
                sumProb += item.Probability;
                range = item;
                if (sumProb >= randVal)
                    break;
                    
            }

            range.Count++;
            return GetRandomVolume(range.FromValue, range.ToValue, isDouble);
        }

        public double GetRandomVolume(double minimum, double maximum, bool isDouble)
        {
            Random random = new Random();
            double value = random.NextDouble() * (maximum - minimum) + minimum;
            if (isDouble || value < 1)
                return value;
            else
                return Math.Ceiling(value);
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
                    Gas = 8000000
                };
                transferHandler.SendRequestAsync(Program.init.walletContract.Address, transfer).GetAwaiter().GetResult();
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
                balance = balanceHandler.QueryAsync<BigInteger>(Program.init.walletContract.Address, balanceOfFunctionMessage).GetAwaiter().GetResult();

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
                    transferHandler.SendRequestAsync(Program.init.walletContract.Address, transfer).GetAwaiter().GetResult();
                }
            }
            catch (Exception ex)
            {
                if (ex.Message.Equals("insufficient funds for gas * price + value"))
                {
                    SendGasFromMain(address);
                    SendTokensToMain(address);
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
