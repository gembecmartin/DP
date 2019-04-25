using DevExpress.XtraBars.Docking2010;
using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Traffic_generator_WFA.Control;
using Traffic_generator_WFA.Models;

namespace Traffic_generator_WFA.Forms
{
    public partial class HomeControl : UserControl
    {
        public bool trafficPlay = true;
        public bool trafficStop = false;
        public HomeControl()
        {
            InitializeComponent();
            textEdit1.Text = Program.init.accNo.ToString();
            textEdit3.Text = Program.init.contractProperties.Supply.ToString();
            textEdit4.Text = Program.init.trafficICO;
            textEdit5.Text = Program.init.contractProperties.Code;
            textEdit2.Text = Program.init.contractProperties.Decimals.ToString();
            backgroundWorker1.RunWorkerAsync();
        }

        private void HomeControl_Load(object sender, EventArgs e)
        {

        }

        public void SetSeriesCDF(List<HistogramRecord> histogram)
        {
            try
            {
                chartControl2.Series.BeginUpdate();
                chartControl2.Series["CDF"].DataSource = histogram;
                chartControl2.Series["CDF"].ArgumentDataMember = "Value";
                chartControl2.Series["CDF"].ValueDataMembers.AddRange(new string[] { "Count" });
            }
            finally
            {
                chartControl2.Series.EndUpdate();
            }
        }

        public void SetSeriesActual(List<Models.Range> tx_histogram, List<Models.Range> block_histogram)
        {
            try
            {
                if (tx_histogram != null)
                {
                    chartControl2.Series.BeginUpdate();
                    chartControl2.Series["Generated"].DataSource = tx_histogram;
                    chartControl2.Series["Generated"].ArgumentDataMember = "FromValue";
                    chartControl2.Series["Generated"].ValueDataMembers.AddRange(new string[] { "Count" });
                }

                if (block_histogram != null)
                {
                    chartControl1.Series.BeginUpdate();
                    chartControl1.Series["Generated"].DataSource = block_histogram;
                    chartControl1.Series["Generated"].ArgumentDataMember = "ToValue";
                    chartControl1.Series["Generated"].ValueDataMembers.AddRange(new string[] { "Count" });
                }

            }
            finally
            {
                chartControl2.Series.EndUpdate();
            }
        }

        //public void SetSeriesOriginalDist(List<Models.Range> ranges)
        //{
        //    try
        //    {
        //        chartControl2.Series.BeginUpdate();
        //        chartControl2.Series["Original"].DataSource = ranges;
        //        chartControl2.Series["Original"].ArgumentDataMember = "FromValue";
        //        chartControl2.Series["Original"].ValueDataMembers.AddRange(new string[] { "Count" });
        //    }
        //    finally
        //    {
        //        chartControl2.Series.EndUpdate();
        //    }
        //}

        public void SetSeriesOriginalDist(List<Models.Range> ranges)
        {
            try
            {
                chartControl2.Series.BeginUpdate();
                chartControl2.Series["Original"].DataSource = ranges;
                chartControl2.Series["Original"].ArgumentDataMember = "FromValue";
                chartControl2.Series["Original"].ValueDataMembers.AddRange(new string[] { "Count" });
            }
            finally
            {
                chartControl2.Series.EndUpdate();
            }
        }

        public void SetSeriesOriginalBlockDist(List<Models.Range> ranges)
        {
            try
            {
                chartControl1.Series.BeginUpdate();
                chartControl1.Series["Original"].DataSource = ranges;
                chartControl1.Series["Original"].ArgumentDataMember = "ToValue";
                chartControl1.Series["Original"].ValueDataMembers.AddRange(new string[] { "Count" });
            }
            finally
            {
                chartControl1.Series.EndUpdate();
            }
        }

        public void SetSeriesPDF(List<Models.Range> ranges)
        {
            try
            {
                /*cartesianChart1.Series = new LiveCharts.SeriesCollection
                {
                    new LineSeries
                    {
                        Values = new ChartValues<double>(ranges.Select(x => x.Probability).ToList()),
                        Stroke = System.Windows.Media.Brushes.Red,
                        Fill = System.Windows.Media.Brushes.Red
                    },
                    new LineSeries
                     {
                        Values = new ChartValues<double>(ranges.Select(x => x.CDFProbability).ToList()),
                         Stroke = System.Windows.Media.Brushes.Blue,
                        Fill = System.Windows.Media.Brushes.Blue
                    },
                };
                /*chartControl1.Series.BeginUpdate();
                chartControl1.Series["PDF"].DataSource = ranges;
                chartControl1.Series["PDF"].ArgumentDataMember = "Avg";
                chartControl1.Series["PDF"].ValueDataMembers.AddRange(new string[] { "Probability" });

                XYDiagram diagram = (XYDiagram)chartControl1.Diagram;
                double min = Convert.ToDouble(diagram.AxisX.WholeRange.MinValue);
                double max = Convert.ToDouble(diagram.AxisX.WholeRange.MaxValue);
                diagram.AxisX.WholeRange.SetMinMaxValues(min, max);
                diagram.AxisX.WholeRange.AutoSideMargins = false;
                //diagram.AxisX.WholeRange.SideMarginsValue = 0;*/

            }
            finally
            {
                //chartControl1.Series.EndUpdate();
            }
        }

        public void SetSeriesCDF(List<Models.Range> ranges)
        {
            //try
            //{
            //    chartControl1.Series.BeginUpdate();
            //    chartControl1.Series["CDF"].DataSource = ranges;
            //    chartControl1.Series["CDF"].ArgumentDataMember = "Avg";
            //    chartControl1.Series["CDF"].ValueDataMembers.AddRange(new string[] { "CDFProbability" });

            //}
            //finally
            //{
            //    chartControl1.Series.EndUpdate();
            //}
        }

        private void cartesianChart1_ChildChanged(object sender, System.Windows.Forms.Integration.ChildChangedEventArgs e)
        {

        }

        private void windowsUIButtonPanel1_ButtonClick(object sender, ButtonEventArgs e)
        {
            string tag = ((WindowsUIButton)e.Button).Tag.ToString();
            switch (tag)
            {
                case "Play":
                    trafficPlay = true;
                    break;
                case "Pause":
                    trafficPlay = false;
                    break;
                case "Stop":
                    trafficStop = true;
                    break;
            }
        }

        private void windowsUIButtonPanel1_ButtonChecked(object sender, ButtonEventArgs e)
        {
            string tag = ((WindowsUIButton)e.Button).Tag.ToString();
            switch (tag)
            {
                case "Play":
                    trafficPlay = true;
                    break;
                case "Pause":
                    trafficPlay = false;
                    break;
                case "Stop":
                    trafficStop = true;
                    Program.init.tc = new TransactionController();
                    break;
            }
        }

        private void backgroundWorker1_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            Program.init.tc.TransactionSendingAsync(Program.init.web3, Program.init.contractProperties.Master, Program.init.contractProperties.Address, Program.init.passwd);
        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {
            Program.init.mw.UpdateView(Program.init.mw.tagNum);
        }

        private void simpleButton1_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("https://ropsten.etherscan.io/token/" + Program.init.contractProperties.Address);
        }

        private void simpleButton2_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("https://ropsten.etherscan.io/address/" + Program.init.contractProperties.Master);
        }

        private void simpleButton3_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("https://etherscan.io/token/" + Program.init.contractProperties.OriginalTokenAddress);
        }
    }
}
