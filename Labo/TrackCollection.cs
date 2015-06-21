using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using iTunesLib;
using SoundAnalyzeLib;

namespace Labo
{
    public class WrapTrack : INotifyPropertyChanged
    {
        IITTrack _track;
        BpmDetector _detector;
        public WrapTrack(IITTrack track)
        {
            this._track = track;
        }
        public IITTrack Track
        {
            get { return _track; }
        }
        public BpmDetector Detector
        {
            get { return _detector; }
            set
            {
                this._detector = value;
                notifyPropertyChanged();
            }
        }
        int detectedBPM = 10;
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
    public class WrapCollection : IEnumerable
    {
        IITTrackCollection _trackCollection;
        WrapEneumerator _enumerator;
        public WrapCollection(IITTrackCollection trackCollection)
        {
            this._trackCollection = trackCollection;
            this._enumerator = new WrapEneumerator(trackCollection.GetEnumerator());
        }

        public IEnumerator GetEnumerator()
        {
            return this._enumerator;
        }
    }
    class WrapEneumerator : IEnumerator
    {
        IEnumerator _enumerator;
        public WrapEneumerator(IEnumerator enumerator)
        {
            this._enumerator = enumerator;
        }

        public object Current
        {
            get { return new WrapTrack(_enumerator.Current as IITTrack); }
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
