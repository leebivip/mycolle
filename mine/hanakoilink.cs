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
                    Console.WriteLine("Read\t" + file);
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
        public string Generate(string dstname)
        {
            // 读取作为导入目标的文件
            var dstlines = dstname.ReadLinesAsFile(65001);
            var dstpairs = new List<KeyValuePair<int, string>>();

            string pattern = @"^\[0x(?<label>.{8})\](?<text>.*)$";

            try
            {
                foreach (var str in dstlines)
                {
                    var g = Regex.Match(str, pattern).Groups;
                    if (g.Count == 3) dstpairs.Add(new KeyValuePair<int, string>(int.Parse(g["label"].Value, System.Globalization.NumberStyles.HexNumber), g["text"].Value));
                }
            }
            catch
            {
                Console.WriteLine("Parse\t" + dstname);
                Console.WriteLine();
                return "";
            }

            // 获取最新版的译文
            var srcpairs = GetPairs(Path.GetFileName(dstname).Replace("_BinOrder", "")).ToList();
            var srcpairsLabeled = new List<KeyValuePair<int, string>>();
            for (int i = 0; i < srcpairs.Count; ++i)
                srcpairsLabeled.Add(new KeyValuePair<int, string>(i, srcpairs[i].Key));

            // 导入译文
            var srcmatched = srcpairsLabeled.Intersect(dstpairs, new PairsComp()).OrderBy(x => x.Value).ToList();
            var dstmatched = dstpairs.Intersect(srcpairsLabeled, new PairsComp()).OrderBy(x => x.Value).ToList();

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

            // 生成导入的文件
            if (dstpairs.Count != newpairs.Count)
                Console.WriteLine(dstname + "match error");

            var sb = new StringBuilder();
            var logsb = new StringBuilder();
           

            for (int i = 0; i < dstpairs.Count; ++i)
            {
                var dstval = dstpairs[i].Value;
                sb.AppendLine(string.Format("[0x{0:x8}]{1}", dstpairs[i].Key, dstval));
                var newval = newpairs[i].Value;
                sb.AppendLine(string.Format(";[0x{0:x8}]{1}", newpairs[i].Key, newval));
                sb.AppendLine();
                if (dstval == newval && !Regex.IsMatch(dstval, @"^[0-9a-zA-Z_]+$"))
                    logsb.AppendLine(string.Format("Check\t{0}\t{1:x8}\t{2}", Path.GetFileName(dstname), dstpairs[i].Key, dstval));
            }
            Console.WriteLine(logsb.ToString());

            return sb.ToString();
        }
    }
    
    class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.Unicode;
            if (args.Length < 3)
            {
                Console.WriteLine("hanairolink <input> <step0> [<step1> ... <stepN>] <output>");
                return;
            }

            var inlist = new List<string>();
            for (int i = 1; i < args.Length - 1; ++i)
            { inlist.Add(args[i]); }

            var layer = new _FileLayer();

            layer.Add(inlist);

            var str = layer.Generate(args[0]);
            str.WriteToFile(args.Last());
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
