using System.Runtime.InteropServices;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;

namespace FastStructArray;

[MemoryDiagnoser]
public class Program
{
    private static readonly StructArray<ITestable, Testables> StructArray;
    private static readonly StructArray<ITestable, Testables> LargeStructArray;
    private static readonly ITestable[] NormalArray = new ITestable[10];
    private static readonly ITestable[] LargeNormalArray = new ITestable[10_000];

    static Program()
    {
        StructArray = new StructArray<ITestable, Testables>(10);

        LargeStructArray = new StructArray<ITestable, Testables>(10_000);
        for (var i = 0; i < 10_000; i++)
        {
            if (Random.Shared.Next() > 0)
            {
                StructArray.Set(i,
                    new Box3d
                    {
                        Box = new Box
                        {
                            Height = 20,
                            Width = 22,
                        },
                        Depth = 321
                    });
            }
            else
            {
                StructArray.Set(i,
                    new Box
                    {
                        Height = 20,
                        Width = 22,
                    });
            }
        }

        for (var i = 0; i < NormalArray.Length; i++)
        {
            NormalArray[i] = new Box();
        }

        for (var i = 0; i < LargeNormalArray.Length; i++)
        {
            if (Random.Shared.Next() > 0)
            {
                LargeNormalArray[i] = new Box3d
                {
                    Box = new Box
                    {
                        Height = 20,
                        Width = 22,
                    },
                    Depth = 321
                };
            }
            else
            {
                LargeNormalArray[i] = new Box();
            }

        }
    }

    [GlobalSetup]
    public void GlobalSetup()
    {
        CustomSet();
    }

    public static void Main()
    {
        BenchmarkRunner.Run<Program>();
    }

    [Benchmark]
    public void NormalSet()
    {
        NormalArray[0] = new Box3d
        {
            Box = new Box
            {
                Height = 20,
                Width = 22,
            },
            Depth = 321
        };
    }

    [Benchmark]
    public void NormalGet()
    {
        NormalArray[0].Execute();
    }

    [Benchmark]
    public void NormalGetLarge()
    {
        for (var i = 0; i < 10_000; i++)
        {
            NormalArray[i].Execute();
        }
    }
    
    [Benchmark]
    public void CustomSet()
    {
        StructArray.Set(0,
            new Box3d
            {
                Box = new Box
                {
                    Height = 20,
                    Width = 22,
                },
                Depth = 321
            });
    }

    [Benchmark]
    public void CustomGet()
    {
        switch ((Testables)StructArray.GetType(0))
        {
            case Testables.Box:
                StructArray.Get<Box>(0).Execute();
                break;
            case Testables.Box3d:
                StructArray.Get<Box3d>(0).Execute();
                break;
        }
    }

    [Benchmark]
    public void CustomGetLarge()
    {
        for (var i = 0; i < 10_000; i++)
        {
            switch ((Testables)StructArray.GetType(i))
            {
                case Testables.Box:
                    StructArray.Get<Box>(i).Execute();
                    break;
                case Testables.Box3d:
                    StructArray.Get<Box3d>(i).Execute();
                    break;
            }
        }
    }
}

interface ITestable
{
    void Execute();
}

[StructLayout(LayoutKind.Sequential)]
public struct Box : ITestable
{
    public int Width;
    public int Height;

    public void Execute()
    {
        Width += 1;
    }
}

[StructLayout(LayoutKind.Sequential)]
public struct Box3d : ITestable
{
    public Box Box;
    public int Depth;
    public void Execute()
    {
        Depth += 1;
    }
}

public enum Testables
{
    Box,
    Box3d
}
