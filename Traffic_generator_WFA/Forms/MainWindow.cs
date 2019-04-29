using DevExpress.XtraCharts;
using DevExpress.XtraEditors;
using DevExpress.XtraSplashScreen;
using Nethereum.StandardTokenEIP20.ContractDefinition;
using Nethereum.Web3;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Traffic_generator_WFA.Control;
using Traffic_generator_WFA.Forms.Items;

namespace Traffic_generator_WFA.Forms
{
    public partial class MainWindow : XtraForm
    {
        public HomeControl hc = null;
        private LoadingControl lc = null;
        Thread updater = null;
        public int tagNum = 1;
        public bool trafficInitialized = false;
        public bool loadingProc = false;
        public bool chartRefreshing = false;
        private bool chartUpdater = false;

        public delegate BindingList<Models.Range> GetTx();
        public delegate BindingList<Models.Range> GetBlocks();
        public delegate void Hists(BindingList<Models.Range> tx_hist, BindingList<Models.Range> block_hist);

        public BindingList<Models.Range> th = new BindingList<Models.Range>();
        public BindingList<Models.Range> bh = new BindingList<Models.Range>();
        

        public MainWindow()
        {
            var mainT = Thread.CurrentThread;

            InitializeComponent();
            Program.init.web3 = new Web3();
            Program.init.CreateMasterAccount();
            tileBar1.SelectedItem = tileBarItem1;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var generatorForm = new GeneratorCreationDialogue();
            generatorForm.Show(this);
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void tileBar1_Click(object sender, EventArgs e)
        {
        }

        private void UpdateChart()
        {
            GetTx txGetter = Program.init.tc.returnTx;
            GetBlocks blockGetter = Program.init.tc.returnBlocks;

            IAsyncResult resTx = txGetter.BeginInvoke(null, null);
            IAsyncResult resBlock = blockGetter.BeginInvoke(null, null);

            while (!hc.trafficStop)
            {
                if (hc != null)
                {
                    resTx = txGetter.BeginInvoke(null, null);
                    resBlock = blockGetter.BeginInvoke(null, null);

                    var nth = txGetter.EndInvoke(resTx);
                    var nbh = blockGetter.EndInvoke(resBlock);

                    BeginInvoke(new MethodInvoker(delegate {
                        var thr = Thread.CurrentThread;
                        hc.th = new BindingList<Models.Range>(nth);
                        hc.bh = new BindingList<Models.Range>(nbh);

                        hc.SetSeriesActual();
                    }));
                }
                Thread.Sleep(5000);
            }
        }

        private void tileBar1_SelectedItemChanged(object sender, TileItemEventArgs e)
        {
            int tag = short.Parse(e.Item.Tag.ToString());
            UpdateView(tag);
        }

        public void UpdateView(int tag)
        {
            tagNum = tag;
            
            if (!trafficInitialized)
            {
                groupControl1.Controls.Remove(panelControl1);
                groupControl1.Controls.Remove(flowLayoutPanel1);
                panelControl1 = new PanelControl();
                groupControl1.Controls.Add(panelControl1);
                panelControl1.Dock = DockStyle.Fill;
                panelControl1.AutoScroll = true;
                panelControl1.AutoSize = true;
                panelControl1.Location = new Point(0, 103);
                panelControl1.Padding = new Padding(0, 103, 0, 0);
                if (loadingProc)
                {
                    groupControl1.Controls.Remove(panelControl1);
                    groupControl1.Controls.Remove(flowLayoutPanel1);
                    panelControl1 = new PanelControl();
                    groupControl1.Controls.Add(panelControl1);
                    panelControl1.Dock = DockStyle.Fill;
                    panelControl1.AutoSize = true;
                    panelControl1.Location = new Point(0, 103);
                    panelControl1.Padding = new Padding(0, 103, 0, 0);
                    lc.Dock = DockStyle.Fill;
                    panelControl1.Controls.Add(lc);
                    lc.Location = new Point(0, 0);
                }
                else if (Program.init.loading)
                {
                    groupControl1.Controls.Remove(panelControl1);
                    groupControl1.Controls.Remove(flowLayoutPanel1);
                    panelControl1 = new PanelControl();
                    groupControl1.Controls.Add(panelControl1);
                    panelControl1.Dock = DockStyle.Fill;
                    panelControl1.AutoSize = true;
                    panelControl1.Location = new Point(0, 103);
                    panelControl1.Padding = new Padding(0, 103, 0, 0);
                    lc = new LoadingControl();
                    lc.Dock = DockStyle.Fill;
                    panelControl1.Controls.Add(lc);
                    lc.Location = new Point(0, 0);
                }
                else
                {
                    NoTraffic nt = new NoTraffic();
                    nt.Dock = DockStyle.Fill;
                    panelControl1.Controls.Add(nt);
                    nt.Location = new Point(0, 0);
                }
            }
            else
            {
                switch (tag)
                {
                    case 1:
                        groupControl1.Controls.Remove(panelControl1);
                        groupControl1.Controls.Remove(flowLayoutPanel1);
                        panelControl1 = new PanelControl();
                        groupControl1.Controls.Add(panelControl1);
                        panelControl1.Dock = DockStyle.Fill;
                        panelControl1.AutoSize = true;
                        panelControl1.Location = new Point(0, 103);
                        panelControl1.Padding = new Padding(0, 103, 0, 0);
                        hc = new HomeControl();
                        panelControl1.Controls.Add(hc);

                        if (!chartUpdater)
                        {
                            var thr = Thread.CurrentThread;
                            GetTx txGetter = Program.init.tc.returnTx;
                            GetBlocks blockGetter = Program.init.tc.returnBlocks;

                            IAsyncResult resTx = txGetter.BeginInvoke(null, null);
                            IAsyncResult resBlock = blockGetter.BeginInvoke(null, null);

                            th = txGetter.EndInvoke(resTx);
                            bh = blockGetter.EndInvoke(resBlock);

                            hc.InitSeries(th, bh);

                            updater = new Thread(UpdateChart);
                            updater.Start();
                            chartUpdater = true;
                        }
                        
                        hc.SetSeriesOriginalDist(Program.init.tc.ranges);
                        hc.SetSeriesOriginalBlockDist(Program.init.tc.blockRanges);
                        hc.Location = new Point(0, 0);
                        hc.Dock = DockStyle.Fill;
                        break;
                    case 2:
                        groupControl1.Controls.Remove(panelControl1);
                        groupControl1.Controls.Remove(flowLayoutPanel1);
                        flowLayoutPanel1 = new FlowLayoutPanel();
                        groupControl1.Controls.Add(flowLayoutPanel1);
                        flowLayoutPanel1.AutoScroll = true;
                        flowLayoutPanel1.Padding = new Padding(0, 103, 0, 0);
                        flowLayoutPanel1.Dock = DockStyle.Fill;
                        //foreach (var address in Program.init.tc.accList)
                        //{
                        //    AccountItem ai = new AccountItem(address);
                        //    flowLayoutPanel1.Controls.Add(ai);
                        //}
                        break;
                    case 3:
                        groupControl1.Controls.Remove(panelControl1);
                        groupControl1.Controls.Remove(flowLayoutPanel1);
                        flowLayoutPanel1 = new FlowLayoutPanel();
                        groupControl1.Controls.Add(flowLayoutPanel1);
                        flowLayoutPanel1.AutoScroll = true;
                        flowLayoutPanel1.Dock = DockStyle.Fill;
                        flowLayoutPanel1.Padding = new Padding(0, 103, 0, 0);
                        //for (int i = 0; i < 100; i++)
                        //{
                        //    WalletItem wi = new WalletItem();
                        //    flowLayoutPanel1.Controls.Add(wi);
                        //}
                        break;

                }
            }
        }

        private void MainWindow_FormClosed(object sender, FormClosedEventArgs e)
        {

        }

        private void MainWindow_FormClosing(object sender, FormClosingEventArgs e)
        {
            hc.trafficStop = true;

            try {
                var mongoProcesses = Process.GetProcessesByName("mongod");
                foreach (var p in mongoProcesses)
                {
                    p.Kill();
                    p.WaitForExit();
                }
                var gethProcesses = Process.GetProcessesByName("geth");
                foreach (var p in gethProcesses)
                {
                    p.Kill();
                    p.WaitForExit();
                }
            }
            catch(Exception ex)
            {
                throw ex;
            }

            Application.Exit();
            
        }
    }
}
