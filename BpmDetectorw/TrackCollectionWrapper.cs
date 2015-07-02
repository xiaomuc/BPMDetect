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

namespace BpmDetectorw
{
    /// <summary>
    /// リストビュー表示用にIITPlaylistCollectionのラッパークラス
    /// </summary>
    public class TrackCollectionWrapper : IEnumerable
    {
        IITTrackCollection _trackCollection;
        Dictionary<int, IBpmDetector> _detectorDictionary;
        public TrackCollectionWrapper(IITTrackCollection trackCollection, Dictionary<int, IBpmDetector> detectorDictionary)
        {
            this._trackCollection = trackCollection;
            this._detectorDictionary = detectorDictionary;
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
            get { return new TrackWrapper(_trackCollection[index], this); }
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
                if (_owner.DetectorDictionary.ContainsKey(this.Track.TrackDatabaseID))
                {
                    return _owner.DetectorDictionary[this._track.TrackDatabaseID];
                }
                else
                {
                    return null;
                }
            }
            set
            {
                if (_owner.DetectorDictionary.ContainsKey(this.Track.TrackDatabaseID))
                {
                    _owner.DetectorDictionary[this.Track.TrackDatabaseID] = value;
                }
                else
                {
                    _owner.DetectorDictionary.Add(this.Track.TrackDatabaseID, value);
                }
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
            get { return new TrackWrapper(_enumerator.Current as IITTrack, _owner); }
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
