using System;
using System.IO;
using System.Linq;

namespace rename
{
    internal class Program
    {
        // Why REN do not support UNICODE!!!
        private static void Main(string[] args)
        {
            if (args.Length != 1)
            {
                Console.WriteLine("rename <dir>");
                return;
            }

            Directory.GetFiles(args[0], "*", SearchOption.AllDirectories).ToList().ForEach(y =>
            {
                var x = new Uri(y);
                var name = x.Segments.Last();
                var dir = x.AbsoluteUri.Remove(x.AbsoluteUri.Length - name.Length);
                name = name.Substring(name.Length - 7);
                var src = x.LocalPath;
                var dst = new Uri(dir + name).LocalPath;
                if (src != dst) { File.Move(src, dst); }
            });
        }
    }
}