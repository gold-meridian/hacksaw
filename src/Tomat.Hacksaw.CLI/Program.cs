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
        using var ms = new MemoryStream(bytes);
        var sw = Stopwatch.StartNew();
        var image = HlImage.Read(ms);
        sw.Stop();
        Console.WriteLine(sw.ElapsedMilliseconds);
    }
}