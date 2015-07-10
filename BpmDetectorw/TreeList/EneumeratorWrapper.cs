using System.Collections;
using iTunesLib;

namespace BpmDetector.TreeList
{
    /// <summary>
    /// TrackCollectionWrapperクラスが使用するIEnumerator。
    /// 中身はiTunesのPlaylist.TracksのEnumeratorをラップしたもの
    /// </summary>
    public class EneumeratorWrapper : IEnumerator
    {
        /// <summary>
        /// iTunesのPlaylist.TrackのEnumerator
        /// </summary>
        IEnumerator _enumerator;

        /// <summary>
        /// オーナーとなるコレクションクラス
        /// </summary>
        TrackCollectionWrapper _owner;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="enumerator"></param>
        /// <param name="owner"></param>
        public EneumeratorWrapper(IEnumerator enumerator, TrackCollectionWrapper owner)
        {
            this._owner = owner;
            this._enumerator = enumerator;
        }

        /// <summary>
        /// カレント
        /// </summary>
        public object Current
        {
            get { return _owner.get(_enumerator.Current as IITTrack); }
        }

        /// <summary>
        /// 次へ
        /// </summary>
        /// <returns></returns>
        public bool MoveNext()
        {
            return _enumerator.MoveNext();
        }

        /// <summary>
        /// リセットは必要だけど使われないみたい
        /// </summary>
        public void Reset()
        {
            _enumerator.Reset();
        }
    }
}
