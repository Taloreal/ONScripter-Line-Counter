using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Text.Encodings;

namespace NSLineCounter {

    public class Program {

        public static Dictionary<string, NameTag> Tags = new Dictionary<string, NameTag>() {
            { "", new NameTag("", "Misidentified") },
            { "ALL", new NameTag("ALL", "ALL") },
            { "`", new NameTag("`", "Narrator") },
        };

        public static Bitmap Results = new Bitmap(10, 10);
        public static string Alpha = "abcdefghijklmnopqrstuvwxyz;.!?- *\t";
        public static List<string> Lines = new List<string>();
        public static Dictionary<string, int> Occurences = new Dictionary<string, int>();

        public static void Main(string[] args) {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            if (!File.Exists("0.txt")) { return; }
            ReadScript();
            GetLineCounts();
            DisplayStats();
            SaveOccurences();
            Console.ReadLine();
        }

        public static void SaveOccurences() {
            List<Tuple<string, int>> occur = DictToListTuple(Occurences);
            occur.Sort(CompareTuples);
            StreamWriter sw = new StreamWriter("occur.txt", false, Encoding.GetEncoding(932));
            occur.ForEach(t => {
                string woln = t.Item1.Replace("\r", "").Replace("\n", "");
                sw.WriteLine(t.Item2.ToString() + " x " + woln);
            });
            sw.Close();
        }

        public static int CompareTuples(Tuple<string, int> t1, Tuple<string, int> t2) {
            if (t1.Item2 > t2.Item2) { return -1; }
            if (t1.Item2 == t2.Item2) { return 0; }
            else { return 1; }
        }

        public static List<Tuple<T1, T2>> DictToListTuple<T1, T2>(Dictionary<T1, T2> dic) {
            List<Tuple<T1, T2>> temp = new List<Tuple<T1, T2>>();
            foreach (T1 key in dic.Keys) {
                temp.Add(new Tuple<T1, T2>(key, dic[key]));
            }
            return temp;
        }

        public static void DisplayStats() {
            int count = 0;
            InitializeDraw();
            int[] length = GetLongest();
            StreamWriter sw = new StreamWriter(@"results.txt");
            foreach (NameTag person in Tags.Values) {
                if (person.Name == "Misidentified") { continue; }
                string str = person.Name + MakePad(length[0] - person.Name.Length, ' ');
                str += ":  " + MakePad(length[1] - GetDigits(person.EN_Instances), '0');
                str += person.EN_Instances.ToString() + "  ";
                str += "|  " + MakePad(length[2] - GetDigits(person.JP_Instances), '0');
                str += person.JP_Instances.ToString() + "  ";
                int combine = person.EN_Instances + person.JP_Instances;
                str += "|  " + MakePad(length[3] - GetDigits(combine), '0');
                str += combine.ToString() + "  ";
                DrawString(str.Substring(0, length[0]), count, 0);
                DrawString(str.Substring(length[0] + 2), count, 75);
                Console.WriteLine(str);
                sw.WriteLine(str);
                count++;
            }
            sw.Close();
            SaveImage();
        }

        public static void InitializeDraw() {
            int math = Tags.Count * 25 + 10;
            Results = new Bitmap(210, math);
            Graphics gx = Graphics.FromImage(Results);
            gx.FillRectangle(Brushes.White, 0, 0, 210, math);
            gx.Dispose();
        }

        public static void DrawString(string str, int inst, int offset) {
            Graphics gx = Graphics.FromImage(Results);
            PointF pos = new PointF(15 + offset, inst * 25 + 10);
            gx.DrawString(str, SystemFonts.DefaultFont, Brushes.Black, pos);
            gx.Dispose();
        }

        public static void SaveImage() {
            Results.Save("progress.bmp");
        }

        public static string MakePad(int chars, char c) {
            string val = "";
            while (val.Length < chars) {
                val += c;
            }
            return val;
        }

        public static int[] GetLongest() {
            int[] longest = { 0, 0, 0, 0 };
            foreach (NameTag tag in Tags.Values) {
                if (tag.Name == "Misidentified") { continue; }
                int en_digits = GetDigits(tag.EN_Instances);
                int jp_digits = GetDigits(tag.JP_Instances);
                int combine = GetDigits(tag.EN_Instances + tag.JP_Instances);
                if (tag.Name.Length > longest[0]) { longest[0] = tag.Name.Length; }
                if (en_digits > longest[1]) { longest[1] = en_digits; }
                if (jp_digits > longest[2]) { longest[2] = jp_digits; }
                if (combine > longest[3]) { longest[3] = combine; }
            }
            return longest;
        }

        public static int GetDigits(int num) {
            int digits = 1;
            while (num > 9) {
                num /= 10;
                digits++;
            }
            return digits;
        }

        public static void GetLineCounts() {
            bool other = false;
            NameTag Other = new NameTag("OTHER", "OTHER");
            foreach (string l in Lines) {
                if (l == "") { continue; }
                if (Alpha.Contains(l[0])) { continue; }
                string tag = NameTag.GetTag(l);
                string name = tag.Replace("【", "").Replace("】", "");
                other = name != "" && tag != "`" && !AlphaContains(name[0]);
                if (!Tags.ContainsKey(tag) && !other) {
                    Tags.Add(tag, new NameTag(tag, name));
                }
                NameTag nt = (other) ? Other : Tags[tag];
                string inside = l.Replace("`", "").Replace("「", "");
                if (tag != "") { inside = inside.Replace(tag, ""); }
                if (AlphaContains(inside[0])) {
                    nt.EN_Instances++;
                    Tags["ALL"].EN_Instances++;
                }
                else { 
                    nt.JP_Instances++;
                    Tags["ALL"].JP_Instances++;
                    if (!Occurences.ContainsKey(inside)) { 
                        Occurences.Add(inside, 0); 
                    }
                    Occurences[inside]++;
                }
            }
            Tags.Add("OTHER", Other);
        }

        public static bool AlphaContains(char c) {
            return Alpha.ToUpper().Contains(c) || Alpha.ToLower().Contains(c);
        }

        public static void ReadScript() {
            StreamReader sr = new StreamReader(@"0.txt", Encoding.GetEncoding(932));
            while (!sr.EndOfStream) {
                string str = sr.ReadLine();
                Lines.Add(str);
            }
            sr.Close();
        }
    }
}
