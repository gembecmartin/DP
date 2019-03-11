using CsvHelper;
using MongoDB.Driver;
using Nethereum.Web3;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Traffic_generator_WFA.Control;
using Traffic_generator_WFA.Models;

namespace Traffic_generator_WFA.Forms
{
    public partial class GeneratorCreationDialogue : Form
    {
        public GeneratorCreationDialogue()
        {
            try
            {
                Program.init.web3 = new Web3();

                var connectionString = "mongodb://localhost:27017";
                MongoClient client = new MongoClient(connectionString);
                var db = client.GetDatabase("DP");
                var tokenRecords = db.GetCollection<Token>("tokens").Find(_ => true).ToList();

                Program.init.tc = new TransactionController();
                foreach (var token in tokenRecords)
                {
                    Program.init.tc.tokenList.Add(token);
                }

                /*----------------------------------------------------------*/
                InitializeComponent();
                backgroundWorker1.DoWork += backgroundWorker1_DoWork;
                backgroundWorker1.RunWorkerCompleted += backgroundWorker1_RunWorkerCompleted;
                backgroundWorker1.RunWorkerAsync();

                comboBox1.DataSource = Program.init.tc.tokenList;

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
                Program.init.contractAddress = comboBox1.SelectedValue.ToString();
                Program.init.accNo = int.Parse(noAccount.Text);

                Program.init.loading = true;
                Program.init.mw.UpdateView(Program.init.mw.tagNum);

                //Program.init.CreateAccountsAsync(Int32.Parse(noAccount.Text), comboBox1.SelectedValue.ToString());

            }
            else
                MessageBox.Show("One or more parameters are not filled!\n" +
                    "Fill all parameters and repeat your request.", "Generator missing parameters", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void backgroundWorker1_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            var web3 = Program.init.web3;
            var syncing = web3.Eth.Syncing.SendRequestAsync().GetAwaiter().GetResult();

            while (syncing.IsSyncing)
            {
                labelControl1.Text = "Node not synced! " + (syncing.HighestBlock.Value - syncing.CurrentBlock.Value) + " blocks remaining";
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
                confirm.Enabled = false;
            }
        }

        private void cancel_Click(object sender, EventArgs e)
        {
            this.Hide();
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
    }
}
