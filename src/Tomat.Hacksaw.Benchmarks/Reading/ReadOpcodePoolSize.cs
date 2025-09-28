using System.IO;
using BenchmarkDotNet.Attributes;
using Tomat.Hacksaw.Metadata.Image;

namespace Tomat.Hacksaw.Benchmarks.Reading;

[MemoryDiagnoser]
public class ReadOpcodePoolSize
{
    [Params(
        // "binaries/hello.hl",
        // "C:/Program Files (x86)/Steam/steamapps/common/Dead Cells/deadcells.exe",
        "C:/Program Files (x86)/Steam/steamapps/common/Dead Cells/HLBOOT.DAT"
    )]
    public string ImagePath { get; set; } = null!;
    
    private byte[] data = null!;
    
    [GlobalSetup]
    public void Setup()
    {
        data = File.ReadAllBytes(ImagePath);
    }

    /*
    [Benchmark]
    public void Read10()
    {
        OpcodeReading.SetPoolSize(1 << 10);
        _ = HlImage.Read(
            data,
            new HlImage.ReadSettings(
                StoreDebugInfo: false,
                StoreFunctionAssigns: false
            )
        );
    }
    
    [Benchmark]
    public void Read12()
    {
        OpcodeReading.SetPoolSize(1 << 12);
        _ = HlImage.Read(
            data,
            new HlImage.ReadSettings(
                StoreDebugInfo: false,
                StoreFunctionAssigns: false
            )
        );
    }
    
    [Benchmark]
    public void Read16()
    {
        OpcodeReading.SetPoolSize(1 << 16);
        _ = HlImage.Read(
            data,
            new HlImage.ReadSettings(
                StoreDebugInfo: false,
                StoreFunctionAssigns: false
            )
        );
    }
    
    [Benchmark]
    public void Read20()
    {
        OpcodeReading.SetPoolSize(1 << 20);
        _ = HlImage.Read(
            data,
            new HlImage.ReadSettings(
                StoreDebugInfo: false,
                StoreFunctionAssigns: false
            )
        );
    }
    */
    
    /*
    [Benchmark]
    public void Read14()
    {
        _ = HlImage.Read(
            data,
            new HlImage.ReadSettings(
                StoreDebugInfo: false,
                StoreFunctionAssigns: false,
                OpcodeBytePoolSize: 1 << 14
            )
        );
    }
    
    [Benchmark]
    public void Read15()
    {
        _ = HlImage.Read(
            data,
            new HlImage.ReadSettings(
                StoreDebugInfo: false,
                StoreFunctionAssigns: false,
                OpcodeBytePoolSize: 1 << 15
            )
        );
    }
    
    [Benchmark]
    public void Read16()
    {
        _ = HlImage.Read(
            data,
            new HlImage.ReadSettings(
                StoreDebugInfo: false,
                StoreFunctionAssigns: false,
                OpcodeBytePoolSize: 1 << 16
            )
        );
    }
    */
    
    [Benchmark]
    public void Read17()
    {
        _ = HlImage.Read(
            data,
            new HlImage.ReadSettings(
                StoreDebugInfo: false,
                StoreFunctionAssigns: false,
                OpcodeBytePoolSize: 1 << 17
            )
        );
    }
    
    [Benchmark]
    public void Read18()
    {
        _ = HlImage.Read(
            data,
            new HlImage.ReadSettings(
                StoreDebugInfo: false,
                StoreFunctionAssigns: false,
                OpcodeBytePoolSize: 1 << 18
            )
        );
    }
    
    [Benchmark]
    public void ReadFileSize()
    {
        _ = HlImage.Read(
            data,
            new HlImage.ReadSettings(
                StoreDebugInfo: false,
                StoreFunctionAssigns: false,
                OpcodeBytePoolSize: data.Length
            )
        );
    }
}