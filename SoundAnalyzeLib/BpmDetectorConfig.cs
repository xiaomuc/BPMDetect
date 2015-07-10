
namespace SoundAnalyzeLib
{
    /// <summary>
    /// BPM分析に使用する各種設定値を保持する
    /// </summary>
    public class BPMDetectorConfig
    {
        /// <summary>
        /// 音量抽出に使用する1フレームのサンプル数
        /// </summary>
        public int FrameSize { get; set; }

        /// <summary>
        /// 検出するBPMの最小値
        /// </summary>
        public int BPMLow { get; set; }

        /// <summary>
        /// 検出するBPMの最大値
        /// </summary>
        public int BPMHigh { get; set; }

        /// <summary>
        /// 優先するBPMの最小値
        /// </summary>
        public int PriorityBPMLow { get; set; }

        /// <summary>
        /// 優先するBPMの最大値
        /// </summary>
        public int PriorityBPMHigh { get; set; }

        /// <summary>
        /// BPMのピーク値検出する際のしきい値
        /// </summary>
        public double PeakThreshold { get; set; }

        /// <summary>
        /// ピーク検出に使用する幅（新しいほうでは未使用)
        /// </summary>
        public int PeakWidth { get; set; }

        /// <summary>
        /// 自己相関を計算する数
        /// </summary>
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
