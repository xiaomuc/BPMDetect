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
        Dictionary<int, double> BPM { get; }
        Dictionary<int, double> Peaks { get; }
        Dictionary<int, double> TopPeaks { get; }
        BPMDetectorConfig Config { get; }
    }
}
