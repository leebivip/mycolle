
// 两个文本相似时，使用机器来复制译文
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace duptext
{
    struct TextBlock
    {
        public string src_hdr;
        public string src_txt;
        public string trans_hdr;
        public string trans_txt;
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

                    while (true)
                    {
                        var raw = sr.ReadLine();
                        switch (status)
                        {
                            case 0:
                                if (Regex.IsMatch(raw, @"^○\d{4}○.*$"))
                                {
                                    tb.src_hdr = raw;
                                    status = 1;
                                }
                                if (Regex.IsMatch(raw, @"^●\d{4}●.*$"))
                                {
                                    tb.trans_hdr = raw;
                                    status = 2;
                                }
                                break;
                            case 1:
                                tb.src_txt = raw;
                                status = 0;
                                break;
                            case 2:
                                tb.trans_txt = raw;
                                status = 0;
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

            // 两个文本中匹配相同的原文，并复制译文
            for (int i = 0, j = 0; i < dstf.Length; ++i)
            // i : dstf index
            // j : start index 必须从未匹配的地方开始，而不是从头开始。要保证文本的顺序
            {
                for (int k = j; k < srcf.Length; ++k)
                // k : srcf index
                // k = j : 重新匹配前方内容没有意义
                {
                    if (dstf[i].src_txt == srcf[k].src_txt)
                    {
                        dstf[i].trans_txt = srcf[k].trans_txt;
                        j = k; // 遇到匹配后更新下一组匹配的起始位置，否则继续从相同的地方开始搜索
                        break; // 避免前后内容冲突
                    }
                }
            }

            ////////////////////////////////////////

            var sb = new StringBuilder();
            foreach (var b in dstf)
            {
                sb.AppendLine(b.src_hdr).AppendLine(b.src_txt)
                    .AppendLine(b.trans_hdr).AppendLine(b.trans_txt)
                    .AppendLine();
            }
            try
            {
                using (var sw = new StreamWriter(args[1], false, Encoding.Unicode))
                { sw.Write(sb.ToString()); }
            }
            catch { Console.WriteLine("! " + args[1]); }
        }
    }
}
