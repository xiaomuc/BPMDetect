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
    public class TestTrack : IITTrack
    {
        IITTrack _track;
        TestCollection _collection;
        public TestTrack(IITTrack t, TestCollection collection)
        {
            _track = t;
            _collection = collection;
            bpm = 0;
        }
        int bpm;
        public int DetectedBPM
        {
            get { return bpm; }
            set
            {
                this.bpm = value;
                _collection.forcePropertyChange();
            }
        }

        public IITArtwork AddArtworkFromFile(string filePath)
        {
            return _track.AddArtworkFromFile(filePath);
        }

        public string Album
        {
            get
            {
                return _track.Album;
            }
            set
            {
                _track.Album = value;
            }
        }

        public string Artist
        {
            get
            {
                return _track.Artist;
            }
            set
            {
                _track.Artist = value;
            }
        }

        public IITArtworkCollection Artwork
        {
            get { throw new NotImplementedException(); }
        }

        public int BPM
        {
            get
            {
                return _track.BPM;
            }
            set
            {
                _track.BPM = value;
            }
        }

        public int BitRate
        {
            get { throw new NotImplementedException(); }
        }

        public string Comment
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public bool Compilation
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public string Composer
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public DateTime DateAdded
        {
            get { throw new NotImplementedException(); }
        }

        public void Delete()
        {
            throw new NotImplementedException();
        }

        public int DiscCount
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public int DiscNumber
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public int Duration
        {
            get { return _track.Duration; }
        }

        public string EQ
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public bool Enabled
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public int Finish
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public string Genre
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public void GetITObjectIDs(out int sourceID, out int playlistID, out int trackID, out int databaseID)
        {
            throw new NotImplementedException();
        }

        public string Grouping
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public int Index
        {
            get { return _track.Index; }
        }

        public ITTrackKind Kind
        {
            get { throw new NotImplementedException(); }
        }

        public string KindAsString
        {
            get { return _track.KindAsString; }
        }

        public DateTime ModificationDate
        {
            get { throw new NotImplementedException(); }
        }

        public string Name
        {
            get
            {
                return _track.Name;
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public void Play()
        {
            throw new NotImplementedException();
        }

        public int PlayOrderIndex
        {
            get { throw new NotImplementedException(); }
        }

        public int PlayedCount
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public DateTime PlayedDate
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public IITPlaylist Playlist
        {
            get { return _track.Playlist; }
        }

        public int Rating
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public int SampleRate
        {
            get { throw new NotImplementedException(); }
        }

        public int Size
        {
            get { throw new NotImplementedException(); }
        }

        public int Start
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public string Time
        {
            get { return _track.Time; }
        }

        public int TrackCount
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public int TrackDatabaseID
        {
            get { throw new NotImplementedException(); }
        }

        public int TrackNumber
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public int VolumeAdjustment
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public int Year
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public int playlistID
        {
            get { throw new NotImplementedException(); }
        }

        public int sourceID
        {
            get { throw new NotImplementedException(); }
        }

        public int trackID
        {
            get { return _track.trackID; }
        }
    }

    public class TestCollection : IITTrackCollection, INotifyPropertyChanged
    {
        IITTrackCollection _tracks;
        public TestCollection(IITTrackCollection t)
        {
            this._tracks = t;
        }

        public int Count
        {
            get { return _tracks.Count; }
        }

        public IEnumerator GetEnumerator()
        {
            return _tracks.GetEnumerator();
        }

        public IITTrack get_ItemByName(string Name)
        {
            return new TestTrack(_tracks.get_ItemByName(Name), this);
        }

        public IITTrack get_ItemByPersistentID(int highID, int lowID)
        {
            return new TestTrack(get_ItemByPersistentID(highID, lowID), this);
        }

        public IITTrack get_ItemByPlayOrder(int Index)
        {
            return new TestTrack(get_ItemByPlayOrder(Index), this);
        }

        public IITTrack this[int Index]
        {
            get { return new TestTrack(_tracks[Index], this); }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        public void forcePropertyChange()
        {
            NotifyPropertyChanged();
        }
    }

}
