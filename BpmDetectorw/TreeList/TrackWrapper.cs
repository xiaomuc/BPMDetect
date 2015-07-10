using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using iTunesLib;
using SoundAnalyzeLib;

namespace BpmDetector.TreeList
{
    /// <summary>
    /// IITTrackのラッパー
    /// </summary>
    public class TrackWrapper : INotifyPropertyChanged
    {
        /// <summary>
        /// このラッパーをアイテムとして持つコレクション
        /// </summary>
        TrackCollectionWrapper _owner;

        /// <summary>
        /// iTunesのIITTrackオブジェクトを内包
        /// </summary>
        IITTrack _track;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="track"></param>
        /// <param name="owner"></param>
        public TrackWrapper(IITTrack track, TrackCollectionWrapper owner)
        {
            this._owner = owner;
            this._track = track;
        }

        /// <summary>
        /// iTunesのIITTrackオブジェクト
        /// </summary>
        public IITTrack Track
        {
            get { return _track; }
        }

        /// <summary>
        /// BPM分析オブジェクト
        /// </summary>
        public IBpmDetector Detector
        {
            get
            {
                return _owner.getDetector(this.Track);
            }
            set
            {
                _owner.setDetector(_track, value);
                notifyPropertyChanged();
            }
        }

        /// <summary>
        /// コレクション内のインデックス
        /// </summary>
        public int Index
        {
            get
            {
                return _track.Index;
            }
        }

        /// <summary>
        /// プロパティ変更イベント
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// プロパティ変更イベントハンドラ
        /// </summary>
        /// <param name="propertyName"></param>
        public void notifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
