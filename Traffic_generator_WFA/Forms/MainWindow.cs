using DevExpress.XtraCharts;
using DevExpress.XtraEditors;
using DevExpress.XtraSplashScreen;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
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

        private HomeControl hc = null;
        Thread updater = null;
        public int tagNum = 1;

        public MainWindow()
        {
            InitializeComponent();
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
            while (true)
            {
                if (hc != null)                    
                    hc.SetSeriesActual(Program.init.tc.generatedTransactionHistogram);
                Thread.Sleep(1000);
            }
        }

        private void tileBar1_SelectedItemChanged(object sender, TileItemEventArgs e)
        {
            int tag = Int16.Parse(e.Item.Tag.ToString());
            UpdateView(tag);
        }

        public void UpdateView(int tag)
        {
            tagNum = tag;
            
            if (!Program.init.tc.trafficInitialized)
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
                if (Program.init.loading)
                {
                    LoadingControl lc = new LoadingControl();
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
                        if (updater == null)
                        {
                            updater = new Thread(UpdateChart);
                            updater.Start();
                        }
                        hc.SetSeriesOriginalDist(Program.init.tc.countHistogram);
                        hc.SetSeriesPDF(Program.init.tc.ranges);
                        hc.SetSeriesCDF(Program.init.tc.ranges);
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
                        //for (int i = 0; i < 100; i++)
                        //{
                        //    AccountItem ai = new AccountItem();
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
    }
}
