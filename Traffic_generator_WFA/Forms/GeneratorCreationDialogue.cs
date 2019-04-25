using CsvHelper;
using DevExpress.XtraEditors;
using MongoDB.Driver;
using Nethereum.JsonRpc.Client;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.Web3;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Traffic_generator_WFA.Control;
using Traffic_generator_WFA.Models;

namespace Traffic_generator_WFA.Forms
{
    public partial class GeneratorCreationDialogue : Form
    {
        public string selectedAbi = "";
        private List<TokenContract> contractList = new List<TokenContract>();
        public GeneratorCreationDialogue()
        {
            try
            {
                Program.init.web3 = new Web3();

                var connectionString = "mongodb://localhost:27017";
                MongoClient client = new MongoClient(connectionString);
                var db = client.GetDatabase("DP");
                var tokenRecords = db.GetCollection<TokenContract>("smartContracts").Find(_ => true).ToList();

                Program.init.tc = new TransactionController();
                foreach (var token in tokenRecords)
                {
                    contractList.Add(token);
                }

                /*----------------------------------------------------------*/
                InitializeComponent();
                backgroundWorker1.DoWork += backgroundWorker1_DoWork;
                backgroundWorker1.RunWorkerCompleted += backgroundWorker1_RunWorkerCompleted;
                backgroundWorker1.ProgressChanged += backgroundWorker1_ReportProgress;
                backgroundWorker1.WorkerReportsProgress = true;
                backgroundWorker1.RunWorkerAsync();

                comboBox1.DataSource = contractList;

                labelControl1.Text = "Node not synced! Syncing in progress.";
                labelControl1.ForeColor = Color.Red;
                confirm.Enabled = false;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        private async void Confirm_ClickAsync(object sender, EventArgs e)
        {

            if (noAccount.Text != "")
            {
                Hide();
                var token = (TokenContract)comboBox1.SelectedItem;
                Program.init.contractProperties = token.Properties;
                Program.init.accNo = int.Parse(noAccount.Text);
                Program.init.trafficICO = token.Name;

                Program.init.loading = true;
                Program.init.mw.UpdateView(Program.init.mw.tagNum);
            }
            else
                MessageBox.Show("One or more parameters are not filled!\n" +
                    "Fill all parameters and repeat your request.", "Generator missing parameters", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void backgroundWorker1_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            SyncingOutput syncing = null;
            int latest = 0;
            Web3 web3 = Program.init.web3;
            latest = int.Parse(web3.Eth.Blocks.GetBlockNumber.SendRequestAsync().GetAwaiter().GetResult().Value.ToString());
            syncing = web3.Eth.Syncing.SendRequestAsync().GetAwaiter().GetResult();

            try
            {
                while (syncing.IsSyncing || latest == 0)
                {
                    syncing = web3.Eth.Syncing.SendRequestAsync().GetAwaiter().GetResult();
                    if (syncing.IsSyncing)
                        backgroundWorker1.ReportProgress((int)syncing.HighestBlock.Value - (int)syncing.CurrentBlock.Value);
                    else
                        backgroundWorker1.ReportProgress(0);
                    Thread.Sleep(200);
                }
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        private void backgroundWorker1_ReportProgress(object sender, System.ComponentModel.ProgressChangedEventArgs e)
        {
            switch (e.ProgressPercentage)
            {
                case 0:
                    labelControl1.Text = "Node not synced! \nWaiting for peers";
                    break;
                default:
                    labelControl1.Text = "Node not synced! \n" + e.ProgressPercentage + " blocks remaining";
                    break;
            }
        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                MessageBox.Show("Unknown error occured.");
            }
            else {
                labelControl1.Text = "Node synced!";
                labelControl1.ForeColor = Color.Green;
                confirm.Enabled = true;
            }
        }

        private void cancel_Click(object sender, EventArgs e)
        {
            Hide();
        }

        private void fontDialog1_Apply(object sender, EventArgs e)
        {

        }

        private void volume_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) &&
                (e.KeyChar != '.'))
            {
                e.Handled = true;
            }

            // only allow one decimal point
            if ((e.KeyChar == '.') && ((sender as TextBox).Text.IndexOf('.') > -1))
            {
                e.Handled = true;
            }
        }

        private void noTrasactions_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }
        }

        private void noAccount_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }
        }

        private void label4_Click(object sender, EventArgs e)
        {

        }

        private void labelControl1_Click(object sender, EventArgs e)
        {

        }

        private void GeneratorCreationDialogue_Load(object sender, EventArgs e)
        {

        }

        private void simpleButton1_Click(object sender, EventArgs e)
        {
            var addIco = new AddIcoDIalogue(comboBox1);
            addIco.ShowDialog();
        }
    }
}
