using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Traffic_generator_WFA.Models
{
    public class Range
    {
        public double FromValue { get; set; }
        public double ToValue { get; set; }
        public double Avg { get; set; }
        public double Probability { get; set; }
        public double CDFProbability { get; set; }
        public int Count { get; set; } = 0;

        public Range(double FromValue, double ToValue, double Avg, double Probability, double CDFProbability)
        {
            this.FromValue = FromValue;
            this.ToValue = ToValue;
            this.Avg = Avg;
            this.Probability = Probability;
            this.CDFProbability = CDFProbability;
        }

        public Range()
        {

        }
    }
}
