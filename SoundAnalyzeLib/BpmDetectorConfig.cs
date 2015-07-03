using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoundAnalyzeLib
{
    public class BPMDetectorConfig
    {
        public int FrameSize { get; set; }
        public int BPMLow { get; set; }
        public int BPMHigh { get; set; }
        public int PriorityBPMLow { get; set; }
        public int PriorityBPMHigh { get; set; }
        public double PeakThreshold { get; set; }
        public int PeakWidth { get; set; }
        public int AutoCorrelationSize { get; set; }

        public BPMDetectorConfig()
        {
            FrameSize = 4410;
            BPMLow = 50;
            BPMHigh = 250;
            PriorityBPMLow = 80;
            PriorityBPMHigh = 159;
            PeakThreshold = 0.6;
            PeakWidth = 3;
            AutoCorrelationSize = 50;
        }
    }
}
