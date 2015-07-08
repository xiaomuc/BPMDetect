using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using iTunesLib;
using SoundAnalyzeLib;
using ComUtils;

namespace BpmDetectorw
{
    /// <summary>
    /// リストビュー表示用にIITPlaylistCollectionのラッパークラス
    /// </summary>
    public class TrackCollectionWrapper : IEnumerable
    {
        IITTrackCollection _trackCollection;
        Dictionary<int, IBpmDetector> _detectorDictionary;
        Dictionary<int, TrackWrapper> _items;
        string _dataPath;
        string _ext;

        public TrackCollectionWrapper(IITTrackCollection trackCollection, Dictionary<int, IBpmDetector> detectorDictionary, string dataPath,string ext)
        {
            this._trackCollection = trackCollection;
            this._detectorDictionary = detectorDictionary;
            _items = new Dictionary<int, TrackWrapper>();
            _dataPath = dataPath;
            _ext = ext;
        }

        public IEnumerator GetEnumerator()
        {
            return new EneumeratorWrapper(_trackCollection.GetEnumerator(), this);
        }
        public Dictionary<int, IBpmDetector> DetectorDictionary
        {
            get { return this._detectorDictionary; }
        }
        public int Count { get { return _trackCollection.Count; } }

        public TrackWrapper this[int index]
        {
            get { return getByIndex(index); }
        }

        public TrackWrapper get(IITTrack track)
        {
            if (!_items.ContainsKey(track.TrackDatabaseID))
            {
                _items.Add(track.TrackDatabaseID, new TrackWrapper(track, this));
            }
            return _items[track.TrackDatabaseID];
        }

        public TrackWrapper getByIndex(int index)
        {
            IITTrack track = _trackCollection[index];
            return get(track);
        }
        
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
        public void setDetector(IITTrack track, IBpmDetector detector)
        {
            string fileName = BpmUtils.getDataFileName(_dataPath, track,_ext);
            detector.saveToFile(fileName);
            _detectorDictionary[track.TrackDatabaseID] = detector;
        }
    }

    /// <summary>
    /// IITTrackのラッパー
    /// </summary>
    public class TrackWrapper : INotifyPropertyChanged
    {
        TrackCollectionWrapper _owner;
        IITTrack _track;
        public TrackWrapper(IITTrack track, TrackCollectionWrapper owner)
        {
            this._owner = owner;
            this._track = track;
        }
        public IITTrack Track
        {
            get { return _track; }
        }
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
        public int Index
        {
            get
            {
                return _track.Index;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void notifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }

    public class EneumeratorWrapper : IEnumerator
    {
        IEnumerator _enumerator;
        TrackCollectionWrapper _owner;
        public EneumeratorWrapper(IEnumerator enumerator, TrackCollectionWrapper owner)
        {
            this._owner = owner;
            this._enumerator = enumerator;
        }

        public object Current
        {
            get { return _owner.get(_enumerator.Current as IITTrack); }
        }

        public bool MoveNext()
        {
            return _enumerator.MoveNext();
        }

        public void Reset()
        {
            _enumerator.Reset();
        }
    }
}
