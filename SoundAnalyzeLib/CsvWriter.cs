using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using CSCore;
using CSCore.Codecs;
using CSCore.Streams;

namespace SoundAnalyzeLib
{
    public class CsvWriter
    {
        private static CsvWriter instance = null;
        public static CsvWriter getInstance()
        {
            if (instance == null)
            {
                instance = new CsvWriter();
            }
            return instance;
        }
        public void WaveSourceToCsv(string inputFileName, string outputFileName)
        {
            IWaveSource waveSource = CodecFactory.Instance.GetCodec(inputFileName);
            ISampleSource sampleSource = waveSource.ToSampleSource();
            using (StreamWriter writer = File.CreateText(outputFileName))
            {
                float[] buffer = new float[sampleSource.WaveFormat.Channels];
                for (int ch = 0; ch < waveSource.WaveFormat.Channels; ch++)
                {
                    if (ch > 0)
                    {
                        writer.Write(",");
                    }
                    writer.Write("CH_{0}", ch);
                }
                writer.WriteLine();
                int readCount = 0;
                int r ;
                while ((r=sampleSource.Read(buffer, 0, sampleSource.WaveFormat.Channels)) > 0)
                {
                    for (int ch = 0; ch < waveSource.WaveFormat.Channels; ch++)
                    {
                        if (ch > 0)
                        {
                            writer.Write(",");
                        }
                        writer.Write("{0}", buffer[ch]);
                    }
                    writer.WriteLine();
                    readCount++;
                    if (readCount > waveSource.WaveFormat.SampleRate * 10)
                    {
                        break;
                    }
                }
            }
        }
    }
}
