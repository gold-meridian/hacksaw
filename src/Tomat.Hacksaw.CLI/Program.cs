using System;
using System.Diagnostics;
using System.IO;

using Tomat.Hacksaw.Metadata.Image;

namespace Tomat.Hacksaw.CLI;

internal static class Program
{
    public static void Main(string[] args)
    {
        var bytes = File.ReadAllBytes(args[0]);
        var sw = new Stopwatch();

        var total = 0L;

        for (var i = 0; i < 10; i++)
        {
            sw.Restart();
            {
                HlImage.Read(
                    bytes,
                    new HlImage.ReadSettings(
                        StoreDebugInfo: false,
                        StoreFunctionAssigns: false
                    )
                );
            }
            sw.Stop();

            Console.WriteLine($"{i}: " + sw.ElapsedMilliseconds);
            total += sw.ElapsedMilliseconds;
            GC.Collect();
        }

        Console.WriteLine($"Average: {total / 10}");
    }
}