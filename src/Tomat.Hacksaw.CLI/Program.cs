using System;
using System.Diagnostics;
using System.IO;

using Tomat.Hacksaw.Metadata.Image;

namespace Tomat.Hacksaw.CLI;

internal static class Program
{
    public static void Main(string[] args)
    {
        //using var fs = File.OpenRead(args[0]);
        var bytes = File.ReadAllBytes(args[0]);
        var sw = new Stopwatch();

        for (var i = 0; i < 10; i++)
        {
            using var ms = new MemoryStream(bytes);

            sw.Restart();
            {
                HlImage.Read(ms);
            }
            sw.Stop();

            Console.WriteLine($"{i}: " + sw.ElapsedMilliseconds);
        }
    }
}