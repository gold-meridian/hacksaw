using System;
using System.IO;

using Tomat.Hacksaw.Metadata.Image;

namespace Tomat.Hacksaw.CLI;

internal static class Program
{
    public static void Main(string[] args)
    {
        using var fs = File.OpenRead(args[0]);
        var image = HlImage.Read(fs);
        Console.WriteLine(image);
    }
}