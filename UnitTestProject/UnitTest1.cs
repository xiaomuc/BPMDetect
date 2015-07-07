using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using iTunesLib;

namespace UnitTestProject
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestClearBPM()
        {
            iTunesApp app = new iTunesApp();
            foreach (IITFileOrCDTrack track in app.LibraryPlaylist.Tracks)
            {
                track.BPM = 10;
            }
        }
    }
}
