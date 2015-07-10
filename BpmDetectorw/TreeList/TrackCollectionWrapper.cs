using System.Collections;
using System.Collections.Generic;
using iTunesLib;
using SoundAnalyzeLib;
using ComUtils;

namespace BpmDetector.TreeList
{
    /// <summary>
    /// リストビュー表示用にIITPlaylistCollectionのラッパークラス
    /// </summary>
    public class TrackCollectionWrapper : IEnumerable
    {
        /// <summary>
        /// ラップするiTunesのPlaylist.Tracks
        /// </summary>
        IITTrackCollection _trackCollection;

        /// <summary>
        /// 分析済みのIBPMDetectorを格納しておくディ区所なり
        /// </summary>
        Dictionary<int, IBpmDetector> _detectorDictionary;
        
        /// <summary>
        /// 子アイテムとなるTrackWrapperオブジェクトも保存しておく
        /// </summary>
        Dictionary<int, TrackWrapper> _items;

        /// <summary>
        /// 分析済みBPM情報を保存するフォルダ
        /// </summary>
        string _dataPath;

        /// <summary>
        /// 分析済みBPM情報の拡張子
        /// </summary>
        string _ext;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="trackCollection"></param>
        /// <param name="detectorDictionary"></param>
        /// <param name="dataPath"></param>
        /// <param name="ext"></param>
        public TrackCollectionWrapper(IITTrackCollection trackCollection, Dictionary<int, IBpmDetector> detectorDictionary, string dataPath,string ext)
        {
            this._trackCollection = trackCollection;
            this._detectorDictionary = detectorDictionary;
            _items = new Dictionary<int, TrackWrapper>();
            _dataPath = dataPath;
            _ext = ext;
        }

        /// <summary>
        /// Enumerator取得。
        /// </summary>
        /// <remarks>
        /// 保持してたのを返すと変なので、毎回生成して返すようにした。
        /// </remarks>
        /// <returns></returns>
        public IEnumerator GetEnumerator()
        {
            return new EneumeratorWrapper(_trackCollection.GetEnumerator(), this);
        }

        /// <summary>
        /// Detectorを保存するディクショナリオブジェクト
        /// </summary>
        public Dictionary<int, IBpmDetector> DetectorDictionary
        {
            get { return this._detectorDictionary; }
        }

        /// <summary>
        /// 保持しているアイテム数=iTunesのPlaylistのトラック数
        /// </summary>
        public int Count { get { return _trackCollection.Count; } }

        /// <summary>
        /// アイテム=トラックのラッパー
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public TrackWrapper this[int index]
        {
            get { return getByIndex(index); }
        }

        /// <summary>
        /// IITTrackからアイテムを見つける
        /// </summary>
        /// <param name="track"></param>
        /// <returns></returns>
        public TrackWrapper get(IITTrack track)
        {
            if (!_items.ContainsKey(track.TrackDatabaseID))
            {
                _items.Add(track.TrackDatabaseID, new TrackWrapper(track, this));
            }
            return _items[track.TrackDatabaseID];
        }

        /// <summary>
        /// インデックスからアイテムを見つける
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public TrackWrapper getByIndex(int index)
        {
            IITTrack track = _trackCollection[index];
            return get(track);
        }
        
        /// <summary>
        /// トラックから保存したDetectorを検索
        /// </summary>
        /// <param name="track"></param>
        /// <returns></returns>
        public IBpmDetector getDetector(IITTrack track)
        {
            if (_detectorDictionary.ContainsKey(track.TrackDatabaseID))
            {
                return _detectorDictionary[track.TrackDatabaseID];
            }
            else
            {
                string fileName = BpmUtils.getDataFileName(_dataPath,track,_ext);
                if (System.IO.File.Exists(fileName))
                {
                    IBpmDetector detector = new BPMVolumeAutoCorrelation();
                    detector.loadFromFile(fileName);
                    _detectorDictionary.Add(track.TrackDatabaseID, detector);
                    return detector;
                }
            }
            return null;
        }

        /// <summary>
        /// Detectorを設定
        /// </summary>
        /// <param name="track"></param>
        /// <param name="detector"></param>
        public void setDetector(IITTrack track, IBpmDetector detector)
        {
            string fileName = BpmUtils.getDataFileName(_dataPath, track,_ext);
            detector.saveToFile(fileName);
            _detectorDictionary[track.TrackDatabaseID] = detector;
        }
    }
}
