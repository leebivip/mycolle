using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.IO;

using Newtonsoft.Json;

namespace hanairolink
{
    class _LayerContent
    {
        public Dictionary<string, List<List<string>>> core = new Dictionary<string, List<List<string>>>();
    }
    class _FileLayer
    {
        _LayerContent content = new _LayerContent();
        public void Add(IEnumerable<string> pathlist)
        {
            foreach (var file in pathlist)
            {
                try
                {
                    var lines = file.ReadLinesAsFile().ToArray();
                    var fn = Path.GetFileName(file);
                    var packed = lines.Select(x => { var r = new List<string>(); r.Add(x); return r; }).ToList();

                    try
                    {
                        content.core.Add(fn, packed);
                    }
                    catch
                    {
                        packed = content.core[fn];
                        for (int i = 0; i < packed.Count; ++i)
                        {
                            packed[i].Add(lines[i]);
                        }
                        content.core[fn] = packed;
                    }
                }
                catch
                {
                    Console.WriteLine(file);
                }
            }
        }
        public void Save(string path)
        {
            var json = JsonConvert.SerializeObject(content, Formatting.Indented);
            json.WriteToFile(path);
        }
        public void Load(string path)
        {
            var json = path.ReadAsFile(65001);
            content = JsonConvert.DeserializeObject<_LayerContent>(json);
        }
        public IEnumerable<KeyValuePair<string, string>> GetPairs(string rawname)
        {
            var ret = new List<KeyValuePair<string, string>>();
            content.core[rawname].ForEach(x =>
            {
                // 从后往前选取第一个非空行 注意文本的排放顺序 高质量的放在最后
                for (int i = x.Count - 1; i >= 0; --i)
                {
                    if (!string.IsNullOrWhiteSpace(x[i]))
                    {
                        ret.Add(new KeyValuePair<string, string>(x[0], x[i]));
                        break;
                    }
                }
                // 如果原文也是空白 那就没有必要添加了
            });
            return ret;
        }
        public string Setup(string dstname)
        {
            var dstlines = dstname.ReadLinesAsFile(65001);
            var srcpairs = GetPairs(Path.GetFileName(dstname).Replace("_BinOrder", "")).ToList();
            var dstpairs = new List<KeyValuePair<int, string>>();

            string pattern = @"^\[0x(?<label>.{8})\](?<text>.*)$";

            foreach (var str in dstlines)
            {
                var g = Regex.Match(str, pattern).Groups;
                if (g.Count == 3) dstpairs.Add(new KeyValuePair<int, string>(int.Parse(g["label"].Value, System.Globalization.NumberStyles.HexNumber), g["text"].Value));
            }

            //////////////////////////////////
            var srcpairsOrdered = new List<KeyValuePair<int, string>>();
            for (int i = 0; i < srcpairs.Count; ++i)
                srcpairsOrdered.Add(new KeyValuePair<int, string>(i, srcpairs[i].Key));
            var srcmatched = srcpairsOrdered.Intersect(dstpairs, new PairsComp()).OrderBy(x => x.Value).ToList();
            var dstmatched = dstpairs.Intersect(srcpairsOrdered, new PairsComp()).OrderBy(x => x.Value).ToList();

            var dstother = dstpairs.Except(dstmatched).ToList();

            for (int i = 0, k = 0; i < srcmatched.Count; ++i)
            {
                for (int j = k; j < dstmatched.Count; ++j)
                {
                    if (dstmatched[j].Value == srcmatched[i].Value)
                    {
                        dstmatched[j] = new KeyValuePair<int, string>(dstmatched[j].Key, srcpairs[srcmatched[i].Key].Value);
                        k = j;
                        break;
                    }
                }
            }

            var newpairs = dstother.Union(dstmatched).OrderBy(x => x.Key).ToList();

            ///////////////////////////////////
            if (dstpairs.Count != newpairs.Count)
                Console.WriteLine(dstname + "match error");

            var sb = new StringBuilder();

            for(int i = 0; i < dstpairs.Count; ++i)
            {
                sb.AppendLine(string.Format("[0x{0:x8}]{1}", dstpairs[i].Key, dstpairs[i].Value));
                sb.AppendLine(string.Format(";[0x{0:x8}]{1}", newpairs[i].Key, newpairs[i].Value));
                sb.AppendLine();
            }

            return sb.ToString();
        }
    }

    class PairsComp : IEqualityComparer<KeyValuePair<int, string>>
    {
        public bool Equals(KeyValuePair<int, string> x, KeyValuePair<int, string> y)
        {
            if (object.ReferenceEquals(x, y)) return true;
            if (object.ReferenceEquals(x, null) || object.ReferenceEquals(y, null)) return false;
            return x.Value == y.Value;
        }

        public int GetHashCode(KeyValuePair<int, string> obj)
        {
            if (object.ReferenceEquals(obj, null)) return 0;
            return obj.Value.GetHashCode();
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 2)
            {
                Console.WriteLine("hanairolink <target> <base> [<step1> ... <stepN>]");
                return;
            }

            var inlist = new List<string>();
            for (int i = 1; i < args.Length; ++i)
            { inlist.Add(args[i]); }

            var layer = new _FileLayer();

            layer.Add(inlist);

            var str = layer.Setup(args[0]);
        }
    }

    public static class Extensions
    {
        public static string ReadAsFile(this string path, int codepage = 1200)
        {
            var sr = new StreamReader(path, Encoding.GetEncoding(codepage), true);
            var ret = sr.ReadToEnd();
            sr.Dispose();
            return ret;
        }
        public static IEnumerable<string> ReadLinesAsFile(this string path, int codepage = 1200)
        {
            var sr = new StreamReader(path, Encoding.GetEncoding(codepage), true);
            var lines = new List<string>();
            do { lines.Add(sr.ReadLine()); } while (!sr.EndOfStream);
            sr.Dispose();
            return lines;
        }
        public static void WriteToFile(this string content, string path)
        {
            var sw = new StreamWriter(path, false, Encoding.UTF8);
            sw.Write(content);
            sw.Dispose();
        }
    }
}
