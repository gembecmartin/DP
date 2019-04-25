using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Traffic_generator_WFA.Control;
using Traffic_generator_WFA.Forms;

namespace Traffic_generator_WFA
{
    public class Program
    {
        public static Initializer init;
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            var appFolder = AppDomain.CurrentDomain.BaseDirectory;
            Console.WriteLine(appFolder);

            Process mongo = new Process();
            ProcessStartInfo mongoInfo = new ProcessStartInfo();
            mongoInfo.FileName = "cmd.exe";
            mongoInfo.Arguments = "/C C:\\MongoDB\\mongod.exe --dbpath " + appFolder + "mongo_db\\data";
            mongo.StartInfo = mongoInfo;
            mongo.Start();

            Process geth = new Process();
            ProcessStartInfo gethInfo = new ProcessStartInfo();
            gethInfo.FileName = "cmd.exe";
            gethInfo.Arguments = "/C geth --testnet --cache=2048 --syncmode \"light\" --rpc --rpcaddr 127.0.0.1 --rpcport 8545 --rpcapi=\"db,eth,net,web3,personal,debug\" --rpccorsdomain \" * \"";
            geth.StartInfo = gethInfo;
            geth.Start();

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            init = new Initializer();
            init.tc = new TransactionController();
            init.mw = new MainWindow();
            Application.Run(init.mw);
        }
    }
}
