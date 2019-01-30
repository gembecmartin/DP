using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Traffic_generator_WFA.Models
{
    public class HistogramRecord
    {
        public HistogramRecord(double Value, int Count)
        {
            this.Value = Value;
            this.Count = Count;
        }

        public HistogramRecord()
        {

        }

        public double Value { get; set; }
        public int Count { get; set; }
    }
}
