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
    public class TrackCollectionWrapper : IEnumerable
    {
        IITTrackCollection _trackCollection;
        EneumeratorWrapper _enumerator;
        Dictionary<int, BpmDetector> _detectorDictionary;
        public TrackCollectionWrapper(IITTrackCollection trackCollection, Dictionary<int, BpmDetector> detectorDictionary)
        {
            this._trackCollection = trackCollection;
            this._enumerator = new EneumeratorWrapper(trackCollection.GetEnumerator(), this);
            this._detectorDictionary = detectorDictionary;
        }

        public IEnumerator GetEnumerator()
        {
            return this._enumerator;
        }
        public Dictionary<int, BpmDetector> DetectorDictionary
        {
            get { return this._detectorDictionary; }
        }
        public int Count { get { return _trackCollection.Count; } }

        public TrackWrapper this[int index]
        {
            get { return new TrackWrapper(_trackCollection[index], this); }
        }
    }

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
        public BpmDetector Detector
        {
            get
            {
                if (_owner.DetectorDictionary.ContainsKey(this.Track.trackID))
                {
                    return _owner.DetectorDictionary[this._track.trackID];
                }
                else
                {
                    return null;
                }
            }
            set
            {
                if (_owner.DetectorDictionary.ContainsKey(this.Track.trackID))
                {
                    _owner.DetectorDictionary[this.Track.trackID] = value;
                }
                else
                {
                    _owner.DetectorDictionary.Add(this.Track.trackID, value);
                }
                notifyPropertyChanged();
            }
        }
        int detectedBPM = 1;
        public int DetectedBPM
        {
            get
            {
                return detectedBPM;
            }
            set
            {
                this.detectedBPM = value;
                notifyPropertyChanged();
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
