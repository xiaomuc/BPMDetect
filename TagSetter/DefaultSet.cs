using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Collections;

namespace TagSetter
{
    public class DefaultSet
    {
        public ArrayList list;

        public DefaultSet()
        {
            list = new ArrayList();
        }
        Type[] et = new Type[] { typeof(SettingItem) };

        public void saveToFile(String fileName)
        {
            //ArrayListに追加されているオブジェクトの型の配列を作成

            //XMLファイルに保存する
            System.Xml.Serialization.XmlSerializer serializer1 =
                new System.Xml.Serialization.XmlSerializer(typeof(ArrayList), et);
            System.IO.StreamWriter sw = new System.IO.StreamWriter(
                fileName, false, new System.Text.UTF8Encoding(false));
            serializer1.Serialize(sw, list);
            sw.Close();
        }

        public void loadFromFile(String fileName)
        {
            //保存した内容を復元する
            System.Xml.Serialization.XmlSerializer serializer2 =
                                new System.Xml.Serialization.XmlSerializer(typeof(ArrayList), et);
            System.IO.StreamReader sr = new System.IO.StreamReader(
                fileName, new System.Text.UTF8Encoding(false));
            list = (ArrayList)serializer2.Deserialize(sr);
            sr.Close();
        }
    }
    public class SettingItem
    {
        public const String VO_MAN = "M";
        public const String VO_WOMAN = "W";
        public const String REJECT = "R";
        public const String LANG = "l";
        public const String TAG_MAN = "[" + VO_MAN + "]";
        public const String TAG_WOMAN = "[" + VO_WOMAN + "]";
        public const String TAG_REJECT = "[" + REJECT + "]";
        public const String TAG_LANG = "[lang:";

        public String Kind { get; set; }
        public String Artist { get; set; }
        public String Album { get; set; }
        public bool VoWoman { get; set; }
        public bool VoMan { get; set; }
        public String Lang { get; set; }
        public bool Reject { get; set; }
    }
}
