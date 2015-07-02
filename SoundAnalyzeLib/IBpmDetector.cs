using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoundAnalyzeLib
{
    public interface IBpmDetector
    {
        int detect(string fileName);
        Dictionary<int, double> BPMs { get; }
        Dictionary<int, double> Peaks { get; }
        Dictionary<int, double> TopPeaks { get; }
        BPMDetectorConfig Config { get; }
        int ID { get; set; }
        int BPM { get; set; }
    }
}
