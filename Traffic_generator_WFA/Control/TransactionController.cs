using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using Nethereum.Contracts;
using Nethereum.Hex.HexTypes;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.StandardTokenEIP20.ContractDefinition;
using Nethereum.Util;
using Nethereum.Web3;
using Nethereum.Web3.Accounts.Managed;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using Traffic_generator_WFA.Models;

namespace Traffic_generator_WFA.Control
{
    public class TransactionController
    {
        public List<string> accList = new List<string>();
        public List<string> walletList = new List<string>();
        public List<TransactionReceipt> receipts = new List<TransactionReceipt>();
        public List<Token> tokenList = new List<Token>();
        public List<MongoTransaction> transactionList = new List<MongoTransaction>();
        public List<HistogramRecord> transactionHistogram = new List<HistogramRecord>();
        public List<HistogramRecord> countHistogram = new List<HistogramRecord>();
        public List<Range> generatedTransactionHistogram = new List<Range>();
        public List<Range> ranges = new List<Range>();
        public double rangePercentage = 0.5;

        public bool trafficInitialized = false;

        public async Task TransactionSendingAsync(Contract contract, Web3 web3, MongoAccount masterAcc, Contract walletContract, string passwd)
        {
            int scale = 5;//86400 / 86400;   //one day has 86400 seconds
            Random rand = new Random();
            DateTimeOffset nextTransactionTIme = DateTimeOffset.UtcNow.AddSeconds(GetRandomVolume(scale - scale / 2, scale + scale / 2, false));
            var mainAccount = new ManagedAccount(masterAcc.Address, passwd);
            var unlockResult = await web3.Personal.UnlockAccount.SendRequestAsync(mainAccount.Address, mainAccount.Password, 180);

            #region Initialization
            HexBigInteger pendingFilter = await web3.Eth.Filters.NewPendingTransactionFilter.SendRequestAsync();        // filter for pending transactions
            try
            {
                foreach (var acc in accList)        //each account gets partial number of tokens from main account
                {
                    var transferHandler = web3.Eth.GetContractTransactionHandler<TransferFunction>();
                    var transfer = new TransferFunction()       //transaction construction
                    {
                        FromAddress = mainAccount.Address,
                        To = acc,
                        Value = 10,
                        Gas = 8000000
                    };
                    await transferHandler.SendRequestAsync(walletContract.Address, transfer);       //transaction execution on token contract    
                }

                //FilterLog[] filterChanges = null;
                //do
                //{
                //    filterChanges = await web3.Eth.Filters.GetFilterChangesForEthNewFilter.SendRequestAsync(pendingFilter);  //checking, if all initialization transactions are mined
                //} while (filterChanges.Length > 0);
            }
            catch (Exception e)
            {
                throw e;
            }
            #endregion

            #region Traffic
            BigInteger balance = 0;
            while (!Program.init.appClose)
            {
                if (DateTimeOffset.UtcNow.CompareTo(nextTransactionTIme) > 0)
                {
                    //var tx = GetRandomVolumeValue();
                    //AddTransactionToHistogram(tx);
                    //Console.WriteLine(tx + " Eth");

                    string firstAddress = null;

                    do {
                        firstAddress = accList[rand.Next(accList.Count)];  //take random address
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
                    } while (balance < new BigInteger(10));

                    var secondAddress = accList[rand.Next(accList.Count)];
                    do
                    {
                        secondAddress = accList[rand.Next(accList.Count)];    //take random address
                                               
                    } while (firstAddress == secondAddress);                      //eliminate sending to same address

                    nextTransactionTIme = nextTransactionTIme.AddSeconds(scale);               

                    try
                    {
                        unlockResult = Program.init.web3.Personal.UnlockAccount.SendRequestAsync(firstAddress, Program.init.passwd, 180).GetAwaiter().GetResult();
                        var transferHandler = web3.Eth.GetContractTransactionHandler<TransferFunction>();
                        var transfer = new TransferFunction()
                        {
                            FromAddress = firstAddress,
                            To = secondAddress,
                            Value = 10,
                            Gas = 8000000
                        };
                        await transferHandler.SendRequestAsync(walletContract.Address, transfer);
                        AddTransactionToHistogram(10);
                    }
                    catch (Exception ex)
                    {
                        if (ex.Message.Equals("insufficient funds for gas * price + value"))
                        {
                            SendGasFromMain(firstAddress);
                            continue;
                        }
                        else throw ex;
                    }
                }
            }
            #endregion
        }

        private double CalculateStdDev(IEnumerable<double> values)
        {
            double avg = values.Average();
            return Math.Sqrt(values.Average(v => Math.Pow(v - avg, 2)));
        }

        public int GetMongoTransaction(string address)
        {
            var filter = new BsonDocument { { "tokenContract", address } };
            var connectionString = "mongodb://localhost:27017";

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
                var db = client.GetDatabase("transaction_data");

                var match = new BsonDocument { { "$match", new BsonDocument { { "tokenContract", address } } } };
                var group = new BsonDocument { { "$group", new BsonDocument { { "_id", "$amount" }, { "count", new BsonDocument { { "$sum", 1} } } } } };
                var sort = new BsonDocument { { "$sort", new BsonDocument { { "_id", 1} } } };
                var project = new BsonDocument { { "$project", new BsonDocument { { "amount", "$_id" }, { "count", "$count"} } } };

                var pipeline = new[] { match, group, sort, project };
                var coll = db.GetCollection<BsonDocument>("transactions");
                var result = coll.Aggregate<BsonDocument>(pipeline, new AggregateOptions { AllowDiskUse = true });

                transactionList = result.ToList().Select(x => BsonSerializer.Deserialize<MongoTransaction>(x)).ToList();

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
                    System.Windows.Forms.MessageBox.Show("A problem occured when trying to fetch transactions.");
                    return 1;
                }
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
                if (count / totalTx <= 0.25 && (count + value.Count) / totalTx >= 0.25)
                    firstQuartile = value.Amount;
                if (count / totalTx <= 0.75 && (count + value.Count) / totalTx >= 0.75)
                    thirdQuartile = value.Amount;
                count += value.Count;
            }

            transactionHistogram = new List<HistogramRecord>();
            //var firstQuartile = transactionList.ElementAt(totalTx / 4).Amount;
            //var thirdQuartile = transactionList.ElementAt((totalTx * 3) / 4).Amount;

            var interQuartile = thirdQuartile - firstQuartile;          //range between first and third quartile

            var transactionRange = transactionList.Last().Amount - transactionList.First().Amount;
            //var deviation = CalculateStdDev(transactionList.Select(x => x.Amount).ToList());      //need to repair, if we go back to deviation

            var binNumber = Math.Ceiling(transactionRange / (2 * interQuartile / Math.Pow(totalTx, 1 / 3)));      //Freedman–Diaconis`s rule
            //var binNumber = Math.Ceiling(transactionRange / (3.5 * deviation / Math.Pow(totalTx, 1 / 3)));      //Scott`s rule

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
                        bin.Count++;
                } }

            for (int i = 0; i < ranges.Count; i++)
            {
                ranges[i].Probability = (double)ranges[i].Count / (double)totalTx;
                ranges[i].CDFProbability = totalProb + ranges[i].Probability;
                totalProb = ranges[i].CDFProbability;
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
                return (int)value;
        }


        public void SendFundsToMain(string address)
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
                    SendFundsToMain(address);
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
