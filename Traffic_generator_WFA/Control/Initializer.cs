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

        public string infuraKey = "t67uC80Gaq8IsJboDBpO";
        public string passwd = "dp1";
        private string _getAddress = "./geth.ipc";
        private string RPC = "http://localhost:8545";
        private Nethereum.JsonRpc.IpcClient.IpcClient ipcClient;
        public Web3 web3;
        public string abi = @"[
	{
		""constant"": false,
		""inputs"": [
			{
				""name"": ""spender"",
				""type"": ""address""
			},
			{
				""name"": ""value"",
				""type"": ""uint256""
			}
		],
		""name"": ""approve"",
		""outputs"": [
			{
				""name"": """",
				""type"": ""bool""
			}
		],
		""payable"": false,
		""stateMutability"": ""nonpayable"",
		""type"": ""function""
	},
	{
		""constant"": true,
		""inputs"": [],
		""name"": ""totalSupply"",
		""outputs"": [
			{
				""name"": """",
				""type"": ""uint256""
			}
		],
		""payable"": false,
		""stateMutability"": ""view"",
		""type"": ""function""
	},
	{
		""constant"": false,
		""inputs"": [
			{
				""name"": ""from"",
				""type"": ""address""
			},
			{
				""name"": ""to"",
				""type"": ""address""
			},
			{
				""name"": ""value"",
				""type"": ""uint256""
			}
		],
		""name"": ""transferFrom"",
		""outputs"": [
			{
				""name"": """",
				""type"": ""bool""
			}
		],
		""payable"": false,
		""stateMutability"": ""nonpayable"",
		""type"": ""function""
	},
	{
		""constant"": false,
		""inputs"": [
			{
				""name"": ""spender"",
				""type"": ""address""
			},
			{
				""name"": ""addedValue"",
				""type"": ""uint256""
			}
		],
		""name"": ""increaseAllowance"",
		""outputs"": [
			{
				""name"": """",
				""type"": ""bool""
			}
		],
		""payable"": false,
		""stateMutability"": ""nonpayable"",
		""type"": ""function""
	},
	{
		""constant"": true,
		""inputs"": [
			{
				""name"": ""owner"",
				""type"": ""address""
			}
		],
		""name"": ""balanceOf"",
		""outputs"": [
			{
				""name"": """",
				""type"": ""uint256""
			}
		],
		""payable"": false,
		""stateMutability"": ""view"",
		""type"": ""function""
	},
	{
		""constant"": false,
		""inputs"": [
			{
				""name"": ""spender"",
				""type"": ""address""
			},
			{
				""name"": ""subtractedValue"",
				""type"": ""uint256""
			}
		],
		""name"": ""decreaseAllowance"",
		""outputs"": [
			{
				""name"": """",
				""type"": ""bool""
			}
		],
		""payable"": false,
		""stateMutability"": ""nonpayable"",
		""type"": ""function""
	},
	{
		""constant"": false,
		""inputs"": [
			{
				""name"": ""to"",
				""type"": ""address""
			},
			{
				""name"": ""value"",
				""type"": ""uint256""
			}
		],
		""name"": ""transfer"",
		""outputs"": [
			{
				""name"": """",
				""type"": ""bool""
			}
		],
		""payable"": false,
		""stateMutability"": ""nonpayable"",
		""type"": ""function""
	},
	{
		""constant"": true,
		""inputs"": [
			{
				""name"": ""owner"",
				""type"": ""address""
			},
			{
				""name"": ""spender"",
				""type"": ""address""
			}
		],
		""name"": ""allowance"",
		""outputs"": [
			{
				""name"": """",
				""type"": ""uint256""
			}
		],
		""payable"": false,
		""stateMutability"": ""view"",
		""type"": ""function""
	},
	{
		""anonymous"": false,
		""inputs"": [
			{
				""indexed"": true,
				""name"": ""from"",
				""type"": ""address""
			},
			{
				""indexed"": true,
				""name"": ""to"",
				""type"": ""address""
			},
			{
				""indexed"": false,
				""name"": ""value"",
				""type"": ""uint256""
			}
		],
		""name"": ""Transfer"",
		""type"": ""event""
	},
	{
		""anonymous"": false,
		""inputs"": [
			{
				""indexed"": true,
				""name"": ""owner"",
				""type"": ""address""
			},
			{
				""indexed"": true,
				""name"": ""spender"",
				""type"": ""address""
			},
			{
				""indexed"": false,
				""name"": ""value"",
				""type"": ""uint256""
			}
		],
		""name"": ""Approval"",
		""type"": ""event""
	}
]";
        public string bytecode = "";

        public MongoAccount masterAcc;
        public Contract walletContract;
        public Thread transactions;
        public bool appClose = false;

        public async void CreateAccountsAsync(int NoOfAccounts, string address)
        {
            fc = new FaucetControl();

            mongoClient = new MongoClient(connectionString);
            web3 = new Web3();
            //TransactionController.GetMongoTransaction();
            await CreateMasterSmartContractWalletAsync();
            CreateTrafficAccounts(NoOfAccounts);

            tc.trafficInitialized = true;
            mw.UpdateView(mw.tagNum);
            
            transactions = new Thread(() => tc.TransactionSendingAsync(walletContract, web3, masterAcc,walletContract, passwd));
            transactions.Start();

            //if (tc.GetMongoTransaction(address) == 0)
            //{
            //    tc.trafficInitialized = true;
            //    mw.UpdateView(mw.tagNum);
            //    Thread transactions = new Thread(() => tc.TransactionSendingAsync(walletContract));
            //    transactions.Start();
            //}

            
        }

        public void CreateTrafficAccounts(int count)
        {
            var db = mongoClient.GetDatabase("transaction_data");
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

        public async Task CreateMasterSmartContractWalletAsync()
        {
            var db = mongoClient.GetDatabase("transaction_data");
            masterAcc = db.GetCollection<MongoAccount>("masterAccounts").Find(_ => true).FirstOrDefault();
            var smartContract = db.GetCollection<MongoAccount>("smartContracts").Find(_ => true).FirstOrDefault();

            if (masterAcc == null)
            {
                for (int x = 0; x < 2; x++)
                {
                    var account = await web3.Personal.NewAccount.SendRequestAsync(passwd);
                    await db.GetCollection<MongoAccount>("masterAccounts").InsertOneAsync( new MongoAccount(account));
                    masterAcc = new MongoAccount(account);
                }
            }
            
            Thread faucet = new Thread(() => fc.Donate(masterAcc));
            

            if(smartContract == null)
            {
                try
                {
                    var unlockResult = await web3.Personal.UnlockAccount.SendRequestAsync(masterAcc.Address, passwd, 180);
                    var accValue = await web3.Eth.GetBalance.SendRequestAsync(masterAcc.Address);
                    var transactionHash = await web3.Eth.DeployContract.SendRequestAsync(abi, bytecode, masterAcc.Address);
                    var receipt = await web3.Eth.Transactions.GetTransactionReceipt.SendRequestAsync(transactionHash);
                    while (receipt == null)
                    {
                        Thread.Sleep(5000);
                        receipt = await web3.Eth.Transactions.GetTransactionReceipt.SendRequestAsync(transactionHash);
                    }
                    smartContract = new MongoAccount(receipt.ContractAddress);
                    walletContract = web3.Eth.GetContract(abi, smartContract.Address);
                    await db.GetCollection<MongoAccount>("smartContracts").InsertOneAsync(smartContract);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.InnerException);
                }
            }
            else
            {
                walletContract = web3.Eth.GetContract(abi, smartContract.Address);
            }
                
        }
    }
}
