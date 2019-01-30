using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using DevExpress.XtraCharts;
using Traffic_generator_WFA.Models;

namespace Traffic_generator_WFA.Forms
{
    public partial class HomeControl : UserControl
    {
        public HomeControl()
        {
            InitializeComponent();

            
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

        public void SetSeriesActual(List<Models.Range> histogram)
        {
            try
            {
                if (histogram != null)
                {
                    chartControl2.Series.BeginUpdate();
                    chartControl2.Series["Generated"].DataSource = histogram;
                    chartControl2.Series["Generated"].ArgumentDataMember = "FromValue";
                    chartControl2.Series["Generated"].ValueDataMembers.AddRange(new string[] { "Count" });
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

        public void SetSeriesOriginalDist(List<Models.HistogramRecord> ranges)
        {
            try
            {
                chartControl2.Series.BeginUpdate();
                chartControl2.Series["Original"].DataSource = ranges;
                chartControl2.Series["Original"].ArgumentDataMember = "Value";
                chartControl2.Series["Original"].ValueDataMembers.AddRange(new string[] { "Count" });
            }
            finally
            {
                chartControl2.Series.EndUpdate();
            }
        }

        public void SetSeriesPDF(List<Models.Range> ranges)
        {
            try
            {
                chartControl1.Series.BeginUpdate();
                chartControl1.Series["PDF"].DataSource = ranges;
                chartControl1.Series["PDF"].ArgumentDataMember = "Avg";
                chartControl1.Series["PDF"].ValueDataMembers.AddRange(new string[] { "Probability" });

                XYDiagram diagram = (XYDiagram)chartControl1.Diagram;
                double min = Convert.ToDouble(diagram.AxisX.WholeRange.MinValue);
                double max = Convert.ToDouble(diagram.AxisX.WholeRange.MaxValue);
                diagram.AxisX.WholeRange.SetMinMaxValues(min, max);
                diagram.AxisX.WholeRange.AutoSideMargins = false;
                //diagram.AxisX.WholeRange.SideMarginsValue = 0;

            }
            finally
            {
                chartControl1.Series.EndUpdate();
            }
        }

        public void SetSeriesCDF(List<Models.Range> ranges)
        {
            try
            {
                chartControl1.Series.BeginUpdate();
                chartControl1.Series["CDF"].DataSource = ranges;
                chartControl1.Series["CDF"].ArgumentDataMember = "Avg";
                chartControl1.Series["CDF"].ValueDataMembers.AddRange(new string[] { "CDFProbability" });

            }
            finally
            {
                chartControl1.Series.EndUpdate();
            }
        }
    }
}
