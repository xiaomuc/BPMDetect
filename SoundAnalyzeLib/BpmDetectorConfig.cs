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
            FrameSize = 512;
            BPMLow = 60;
            BPMHigh = 240;
            PriorityBPMLow = 90;
            PriorityBPMHigh = 180;
            PeakThreshold = 0.5;
            PeakWidth = 3;
            AutoCorrelationSize = 500;
        }
    }
}
