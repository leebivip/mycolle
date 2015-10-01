
// 两个文本相似时，使用机器来复制译文
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;
using System.Text.RegularExpressions;

namespace duptext
{
    struct TextBlock
    {
        public int order;
        public string s_hdr;
        public string s_txt;
        public string t_hdr;
        public string t_txt;
    }

    class TBComparer : IEqualityComparer<TextBlock>
    {
        public bool Equals(TextBlock x, TextBlock y)
        {
            if (object.ReferenceEquals(x, y)) return true;
            if (object.ReferenceEquals(x, null) || object.ReferenceEquals(y, null)) return false;
            return x.s_txt == y.s_txt;
        }

        public int GetHashCode(TextBlock obj)
        {
            if (object.ReferenceEquals(obj, null)) return 0;
            return obj.s_txt.GetHashCode();
        }
    }

    class Program
    {
        static TextBlock[] ReadFile(string p)
        {
            var blocks = new List<TextBlock>();

            try
            {
                using (var sr = new StreamReader(p, Encoding.Unicode, false))
                {
                    int status = 0;
                    TextBlock tb = new TextBlock();

                    for (int i = 0; ;)
                    {
                        var raw = sr.ReadLine();
                        
                        switch (status)
                        {
                            case 0:
                                if (Regex.IsMatch(raw, @"^○\d{4}○.*$"))
                                {
                                    tb.s_hdr = raw;
                                    status = 1;
                                }
                                if (Regex.IsMatch(raw, @"^●\d{4}●.*$"))
                                {
                                    tb.t_hdr = raw;
                                    status = 2;
                                }
                                break;
                            case 1:
                                tb.s_txt = raw;
                                status = 0;
                                break;
                            case 2:
                                tb.t_txt = raw;
                                status = 0;
                                tb.order = ++i;
                                blocks.Add(tb);
                                break;
                            default:
                                break;
                        }
                        if (sr.EndOfStream) break;
                    }
                }
            }
            catch { Console.WriteLine("! " + p); }

            return blocks.ToArray();
        }

        static void Main(string[] args)
        {
            if (args.Length != 2)
            {
                Console.WriteLine("duptext <src> <dst>");
                return;
            }

            var srcf = ReadFile(args[0]);
            var dstf = ReadFile(args[1]);

            ////////////////////////////////////////

            // 神 TM 没有 API 活不下去, diff 算法难成狗
            var sameL_r = srcf.Intersect(dstf, new TBComparer());
            var sameR_r = dstf.Intersect(srcf, new TBComparer());
            var dstdiff = dstf.Except(sameR_r); // API 按有序集计算, 所以要提前算好差集

            var sameL = sameL_r.OrderBy(x => x.order).OrderBy(x => x.s_txt).ToArray(); // 算出来的集合顺序不一样你敢信?
            var sameR = sameR_r.OrderBy(x => x.order).OrderBy(x => x.s_txt).ToArray();

            int match = 0;
            int copy = 0;
            for (int i = 0, k = 0; i < sameL.Length; ++i)
            {
                for (int j = k; j < sameR.Length; ++j)
                {
                    if(sameR[j].s_txt == sameL[i].s_txt)
                    {
                        ++match;
                        if (sameR[j].t_txt != sameL[i].t_txt)
                        {
                            sameR[j].t_txt = sameL[i].t_txt;
                            ++copy;
                        }
                        k = j;
                        break;
                    }
                }
            }

            dstf = dstdiff.Union(sameR).OrderBy(x => x.order).ToArray();
            ////////////////////////////////////////

            var sb = new StringBuilder();
            foreach (var b in dstf)
            {
                sb.AppendLine(b.s_hdr).AppendLine(b.s_txt)
                    .AppendLine(b.t_hdr).AppendLine(b.t_txt)
                    .AppendLine();
            }
            try
            {
                using (var sw = new StreamWriter(args[1], false, Encoding.Unicode))
                { sw.Write(sb.ToString()); }
            }
            catch { Console.WriteLine("! " + args[1]); }

            Console.WriteLine("setL: " + sameL.Length + " setR: " + sameR.Length + " match: " + match + " sync: " + copy);
        }
    }
}
