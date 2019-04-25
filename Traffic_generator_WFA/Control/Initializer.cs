using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO;
using Nethereum.Web3;
using System.Threading;
using Nethereum.Contracts;
using Traffic_generator_WFA.Models;
using Traffic_generator_WFA.Forms;
using MongoDB.Driver;
using MongoDB.Bson;
using Traffic_generator_WFA.Models.Smart_Contract;
using Nethereum.Util;
using System.Numerics;

namespace Traffic_generator_WFA.Control
{
    public class Initializer
    {
        public MainWindow mw;
        public TransactionController tc;
        public FaucetControl fc;
        public bool loading = false;

        private string connectionString = "mongodb://localhost:27017";
        public MongoClient mongoClient;

        public string passwd = "dp1";
        private string _getAddress = "./geth.ipc";
        private string RPC = "http://localhost:8545";
        private Nethereum.JsonRpc.IpcClient.IpcClient ipcClient;
        public Web3 web3;

        public MongoAccount masterAcc;
        public TokenContractProperties contractProperties;
        public int accNo;
        public string trafficICO = "";

        public async void CreateAccountsAsync(int NoOfAccounts, string address, string selectedAbi)
        {
         
        }

        public void CreateTrafficAccounts(int count)
        {
            var db = mongoClient.GetDatabase("DP");
            var accounts = db.GetCollection<MongoAccount>("accounts").Find(_ => true).ToList();
            
            if (accounts.Count != 0)
            {
                foreach (var account in accounts)
                {
                    tc.accList.Add(account.Address);
                    Thread faucet = new Thread(() => fc.Donate(account));
                }
                
            }

            if (accounts.Count < count)
            {
                Parallel.For(0, count - tc.accList.Count, async i =>
                {
                    var account = await web3.Personal.NewAccount.SendRequestAsync(passwd);
                    await db.GetCollection<MongoAccount>("accounts").InsertOneAsync(new MongoAccount(account));
                    tc.accList.Add(account);
                    Thread faucet = new Thread(() => fc.Donate(new MongoAccount(account)));
                });
            }
        }

        public void CreateMasterAccount()
        {
            var connectionString = "mongodb://localhost:27017";
            MongoClient client = new MongoClient(connectionString);
            var db = client.GetDatabase("DP");
            masterAcc = db.GetCollection<MongoAccount>("masterAccounts").Find(_ => true).FirstOrDefault();

            if (masterAcc == null)
            {
                var account = web3.Personal.NewAccount.SendRequestAsync(passwd).GetAwaiter().GetResult();
                db.GetCollection<MongoAccount>("masterAccounts").InsertOneAsync(new MongoAccount(account));
                masterAcc = new MongoAccount(account);
            }
        }

        public TokenContract CreateNewToken(string abi, string bytecode, string code, string name, TokenProperties properties)
        {
            var sup = (properties.Supply * Math.Pow(10, properties.Decimals));

            var mainAccount = web3.Personal.NewAccount.SendRequestAsync(passwd).GetAwaiter().GetResult();
            Program.init.tc.SendGasFromMain(mainAccount);

            var accValue = web3.Eth.GetBalance.SendRequestAsync(mainAccount).GetAwaiter().GetResult();
            while (accValue.Value == 0)
            {
                accValue = web3.Eth.GetBalance.SendRequestAsync(mainAccount).GetAwaiter().GetResult();
                Thread.Sleep(1000);
            }

            var unlockResult = web3.Personal.UnlockAccount.SendRequestAsync(mainAccount, passwd, 180).GetAwaiter().GetResult();
            try
            {
                StandardTokenDeployment.BYTECODE = bytecode;
                var tokenDeployResult = new StandardTokenDeployment()
                {
                   FromAddress = mainAccount,
                   TotalSupply = new BigInteger(properties.Supply * Math.Pow(10, properties.Decimals)),
                   Code = code,
                   Name = name,
                   Decimals = properties.Decimals,
                   Gas = 2000000
                };

                var deploymentHandler = web3.Eth.GetContractDeploymentHandler<StandardTokenDeployment>();
                var transactionReceipt = deploymentHandler.SendRequestAndWaitForReceiptAsync(tokenDeployResult).GetAwaiter().GetResult();
                var contractAddress = transactionReceipt.ContractAddress;

                var contract = new TokenContract();
                var props = new TokenContractProperties();
                contract.Name = name;
                props.Address = contractAddress;
                props.Code = code;
                props.Decimals = properties.Decimals;
                props.Master = mainAccount;
                props.Supply = properties.Supply;
                props.OriginalTokenAddress = properties.Address;
                contract.Properties = props;

                var connectionString = "mongodb://localhost:27017";
                MongoClient client = new MongoClient(connectionString);
                var db = client.GetDatabase("DP");
                db.GetCollection<TokenContract>("smartContracts").InsertOne(contract);

                return contract;
            }
            catch (Exception ex)
            {
                if (ex.Message.Equals("insufficient funds for gas * price + value"))
                {
                    Program.init.tc.SendGasFromMain(mainAccount);
                    return null;
                }
                else throw ex;
            }
        }
    }
}
