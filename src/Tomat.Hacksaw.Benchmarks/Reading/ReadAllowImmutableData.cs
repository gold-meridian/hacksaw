using System.IO;

using BenchmarkDotNet.Attributes;

using Haxelink;

using Tomat.Hacksaw.Metadata.Image;

namespace Tomat.Hacksaw.Benchmarks.Reading;

[MemoryDiagnoser]
public class ReadAllowImmutableData
{
    [Params("binaries/hello.hl", "C:/Program Files (x86)/Steam/steamapps/common/Dead Cells/deadcells.exe", "C:/Program Files (x86)/Steam/steamapps/common/Dead Cells/HLBOOT.DAT")]
    public string ImagePath;

    private byte[] data;
    
    [GlobalSetup]
    public void Setup()
    {
        data = File.ReadAllBytes(ImagePath);
    }

    [Benchmark(Baseline = true)]
    public void HacksawRead()
    {
        _ = HlImage.Read(data);
    }

    [Benchmark]
    public void SharplinkRead()
    {
        _ = SharpLink.HlCode.FromBytes(data);
    }
    
    [Benchmark]
    public void HaxelinkRead()
    {
        _ = new Bytecode(ImagePath);
    }
    
    [Benchmark]
    public void DccmHashlinkNetRead()
    {
        _ = HashlinkNET.Bytecode.HlCode.FromBytes(data);
    }
    
    [Benchmark()]
    public void ZzzHacksawRead()
    {
        _ = HlImage.Read(data);
    }
}