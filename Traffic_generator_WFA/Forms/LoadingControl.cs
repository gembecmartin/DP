﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Forms;
using Traffic_generator_WFA.Control;
using MongoDB.Driver;
using Nethereum.Web3;
using System.Threading;

namespace Traffic_generator_WFA.Forms
{
    public partial class LoadingControl : DevExpress.XtraEditors.XtraUserControl
    {
        public int status = 0;
        public LoadingControl()
        {
            Program.init.mw.loadingProc = true;
            InitializeComponent();

            backgroundWorker1.DoWork += backgroundWorker1_DoWork;
            backgroundWorker1.RunWorkerCompleted += backgroundWorker1_RunWorkerCompleted;
            backgroundWorker1.ProgressChanged += backgroundWorker1_ReportProgress;
            backgroundWorker1.WorkerReportsProgress = true;
            backgroundWorker1.RunWorkerAsync();
        }

        private void backgroundWorker1_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            var connectionString = "mongodb://localhost:27017";

            var initializer = Program.init;
            initializer.fc = new FaucetControl();

            try
            {
                backgroundWorker1.ReportProgress(0);
                initializer.mongoClient = new MongoClient(connectionString);

                backgroundWorker1.ReportProgress(3);
                initializer.CreateTrafficAccounts(Program.init.accNo);

                initializer.tc.GetMongoBlocks(initializer.contractProperties.OriginalTokenAddress, initializer.web3, backgroundWorker1);

                initializer.tc.GetMongoTransaction(initializer.contractProperties.OriginalTokenAddress, backgroundWorker1);

                backgroundWorker1.ReportProgress(99);
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        private void backgroundWorker1_ReportProgress(object sender, System.ComponentModel.ProgressChangedEventArgs e)
        {
            progressBarControl1.EditValue = e.ProgressPercentage;
            switch (e.ProgressPercentage)
            {
                case 0:
                    labelControl2.Text = "Initializing connection...";
                    break;
                case 1:
                    labelControl2.Text = "Syncing node...";
                    break;
                case 3:
                    labelControl2.Text = "Setting up traffic accounts..."; 
                    break;
                case 4:
                    labelControl2.Text = "Fetching block informations..."; 
                    break;
                case 25:
                    labelControl2.Text = "Generating block probability histogram...";
                    break;
                case 50:
                    labelControl2.Text = "Fetching transaction informations...";
                    break;
                case 75:
                    labelControl2.Text = "Generating transaction value histogram..."; 
                    break;
            }
        }
        private void backgroundWorker1_RunWorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {
            Program.init.mw.loadingProc = false;
            if (e.Error != null)
            {
                MessageBox.Show("Unknown error occured.");
            }
            else
            {
                Program.init.mw.trafficInitialized = true;
                Program.init.loading = false;
                Program.init.mw.UpdateView(Program.init.mw.tagNum);
                //var transactions = new Thread( () => Program.init.tc.TransactionSendingAsync(Program.init.web3, Program.init.contractProperties.Master, Program.init.contractProperties.Address, Program.init.passwd));
                //transactions.Start();
               
            }
        }

        private void LoadingControl_Load(object sender, EventArgs e)
        {

        }

        private void progressBar2_Click(object sender, EventArgs e)
        {

        }

        private void marqueeProgressBarControl2_EditValueChanged(object sender, EventArgs e)
        {

        }
    }
}
