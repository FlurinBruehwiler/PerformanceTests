using System.Collections.Frozen;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Algorithms;

public class StructArray<T, TEnum> where TEnum : unmanaged, Enum, IConvertible
{
    private const int TypeInfoOffset = sizeof(byte);

    private readonly byte[] _data;

    private readonly int _elementSize;
    private readonly FrozenDictionary<int, TEnum> _types;

    public StructArray(int capacity)
    {
        if (!typeof(T).IsInterface)
            throw new ArgumentException($"{typeof(T).Name} must be an interface");

        var types = typeof(T).Assembly.GetExportedTypes();

        Dictionary<int, TEnum> typeDict = new();

        for (var i = 0; i < types.Length; i++)
        {
            var possibleType = types[i];

            if(!possibleType.IsAssignableTo(typeof(T)))
                continue;

            if (!possibleType.IsValueType)
                throw new InvalidOperationException($"All types implementing {typeof(T).Name} have to be structs, {possibleType.Name} is not.");

            var size = Marshal.SizeOf(possibleType);
            if (size > _elementSize)
            {
                _elementSize = size;
            }

            var type = Enum.Parse<TEnum>(possibleType.Name);
            typeDict[possibleType.GetHashCode()] = type;
        }

        _types = typeDict.ToFrozenDictionary();

        //because we add an extra infobyte at the start
        _elementSize += TypeInfoOffset;

        _data = new byte[capacity * _elementSize];

        Clear();
    }

    public void Clear()
    {
        for (var i = 0; i < _data.Length; i++)
        {
            _data[i] = 0;
        }
    }

    public unsafe void Set<TImplementation>(int idx, TImplementation x) where TImplementation : unmanaged, T
    {
        var typeInfo = _types[typeof(TImplementation).GetHashCode()];

        fixed (byte* bytesPtr = &_data[idx])
        {
            bytesPtr[0] = Unsafe.As<TEnum, byte>(ref typeInfo);

            *(TImplementation*)(bytesPtr + TypeInfoOffset) = x;
        }
    }

    public int GetType(int index)
    {
        return _data[index * _elementSize];
    }

    public unsafe TImplementation Get<TImplementation>(int index) where TImplementation : unmanaged, T
    {
        TImplementation a;

        fixed (byte* bytesPtr = &_data[index * _elementSize + TypeInfoOffset]) // + 1 because of the info byte at the start
        {
            a = *(TImplementation*)bytesPtr;
        }

        return a;
    }
}
