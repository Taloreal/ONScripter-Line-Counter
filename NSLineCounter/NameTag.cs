using System;
using System.Collections.Generic;
using System.Text;

namespace NSLineCounter {

    public class NameTag {

        public string Tag { get; private set; }
        public string Name { get; private set; }
        public int EN_Instances = 0;
        public int JP_Instances = 0;

        public NameTag(string tag, string name) {
            Tag = tag;
            Name = name;
        }

        public static string GetTag(string text) {
            if (text[0] == '`') { return "`"; }
            if (text[0] == '【' && text.Contains("】")) {
                return text.Substring(0, text.IndexOf('】') + 1);
            }
            return "";
        }
    }
}
