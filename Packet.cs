using System;
using System.Buffers;
using System.Text;

namespace LiteNetLib_Referrer;

public class Packet
{
    public byte[] ByteArray
    {
        get
        {
            return byteList.ToArray();
        }
    }

    public byte[] UnreadByteArray
    {
        get
        {
            return byteList.GetRange(CurrentIndex, UnreadLength).ToArray();
        }
    }

    public int Length
    {
        get
        {
            return byteList.Count;
        }
    }

    public int UnreadLength
    {
        get
        {
            return Length - CurrentIndex;
        }
    }

    public int CurrentIndex { get; set; }

    private List<byte> byteList;

    public NetPacket()
    {
        CurrentIndex = 0;
        byteList = new List<byte>();
    }

    public NetPacket(byte[] data) : this()
    {
        Write(data);
    }

    public void Clear()
    {
        byteList.Clear();
        CurrentIndex = 0;
    }

    public void Remove(int offset, int count)
    {
        byteList.RemoveRange(offset, count);
        CurrentIndex = CurrentIndex > offset ? CurrentIndex - count : CurrentIndex;
    }

    public void InsertLength()
    {
        InsertLength(0);
    }

    public void InsertLength(int offset)
    {
        int length = byteList.Count - offset;
        byteList.InsertRange(offset, ToProperEndian(BitConverter.GetBytes(length)));
    }

    public void InsertAtStart(byte value) { byteList.Insert(0, value); }
    public void InsertAtStart(bool value) { byteList.InsertRange(0, ToProperEndian(BitConverter.GetBytes(value))); }
    public void InsertAtStart(char value) { byteList.InsertRange(0, ToProperEndian(BitConverter.GetBytes(value))); }
    public void InsertAtStart(double value) { byteList.InsertRange(0, ToProperEndian(BitConverter.GetBytes(value))); }
    public void InsertAtStart(float value) { byteList.InsertRange(0, ToProperEndian(BitConverter.GetBytes(value))); }
    public void InsertAtStart(int value) { byteList.InsertRange(0, ToProperEndian(BitConverter.GetBytes(value))); }
    public void InsertAtStart(long value) { byteList.InsertRange(0, ToProperEndian(BitConverter.GetBytes(value))); }
    public void InsertAtStart(short value) { byteList.InsertRange(0, ToProperEndian(BitConverter.GetBytes(value))); }
    public void InsertAtStart(uint value) { byteList.InsertRange(0, ToProperEndian(BitConverter.GetBytes(value))); }
    public void InsertAtStart(ulong value) { byteList.InsertRange(0, ToProperEndian(BitConverter.GetBytes(value))); }
    public void InsertAtStart(ushort value) { byteList.InsertRange(0, ToProperEndian(BitConverter.GetBytes(value))); }
    public void InsertAtStart(string value)
    {
        InsertAtStart(value.Length);
        byteList.InsertRange(sizeof(int), Encoding.UTF8.GetBytes(value));
    }

    // Unity Addons
    public void InsertAtStart(Vector3 value)
    {
        InsertAtStart(value.z);
        InsertAtStart(value.y);
        InsertAtStart(value.x);
    }
    public void InsertAtStart(Quaternion value)
    {
        InsertAtStart(value.w);
        InsertAtStart(value.z);
        InsertAtStart(value.y);
        InsertAtStart(value.x);
    }

    public void Write(byte value) { byteList.Add(value); }
    private void WriteInternal(byte[] value) { byteList.AddRange(value); }
    public void Write(byte[] value)
    {
        Write(value.Length);
        WriteInternal(value);
    }

    public void Write(bool value)
    {
        byteList.AddRange(ToProperEndian(BitConverter.GetBytes(value)));
    }

    public void Write(bool[] value)
    {
        Write(value.Length);
        foreach (var item in value)
        {
            Write(item);
        }
    }

    public void Write(char value)
    {
        byteList.AddRange(ToProperEndian(BitConverter.GetBytes(value)));
    }

    public void Write(char[] value)
    {
        Write(value.Length);
        foreach (var item in value)
        {
            Write(item);
        }
    }

    public void Write(double value)
    {
        byteList.AddRange(ToProperEndian(BitConverter.GetBytes(value)));
    }

    public void Write(double[] value)
    {
        Write(value.Length);
        foreach (var item in value)
        {
            Write(item);
        }
    }

    public void Write(float value)
    {
        byteList.AddRange(ToProperEndian(BitConverter.GetBytes(value)));
    }

    public void Write(float[] value)
    {
        Write(value.Length);
        foreach (var item in value)
        {
            Write(item);
        }
    }

    public void Write(int value)
    {
        byteList.AddRange(ToProperEndian(BitConverter.GetBytes(value)));
    }

    public void Write(int[] value)
    {
        Write(value.Length);
        foreach (var item in value)
        {
            Write(item);
        }
    }

    public void Write(long value)
    {
        byteList.AddRange(ToProperEndian(BitConverter.GetBytes(value)));
    }

    public void Write(long[] value)
    {
        Write(value.Length);
        foreach (var item in value)
        {
            Write(item);
        }
    }

    public void Write(short value)
    {
        byteList.AddRange(ToProperEndian(BitConverter.GetBytes(value)));
    }

    public void Write(short[] value)
    {
        Write(value.Length);
        foreach (var item in value)
        {
            Write(item);
        }
    }

    public void Write(uint value)
    {
        byteList.AddRange(ToProperEndian(BitConverter.GetBytes(value)));
    }

    public void Write(uint[] value)
    {
        Write(value.Length);
        foreach (var item in value)
        {
            Write(item);
        }
    }

    public void Write(ulong value)
    {
        byteList.AddRange(ToProperEndian(BitConverter.GetBytes(value)));
    }

    public void Write(ulong[] value)
    {
        Write(value.Length);
        foreach (var item in value)
        {
            Write(item);
        }
    }

    public void Write(ushort value)
    {
        byteList.AddRange(ToProperEndian(BitConverter.GetBytes(value)));
    }

    public void Write(ushort[] value)
    {
        Write(value.Length);
        foreach (var item in value)
        {
            Write(item);
        }
    }

    public void Write(string value)
    {
        Write(value.Length);
        byteList.AddRange(Encoding.UTF8.GetBytes(value));
    }

    public void Write(string[] value)
    {
        Write(value.Length);
        foreach (var item in value)
        {
            Write(item);
        }
    }

    private byte[] ToProperEndian(byte[] value)
    {
        if (BitConverter.IsLittleEndian)
        {
            Array.Reverse(value);
        }
        return value;
    }

    public byte ReadByte(bool moveIndexPosition = true)
    {
        int typeSize = 1;
        var value = byteList[CurrentIndex];
        CurrentIndex += moveIndexPosition ? typeSize : 0;
        return value;
    }

    public byte[] ReadBytes(int length, bool moveIndexPosition = true)
    {
        int typeSize = length;
        var value = byteList.GetRange(CurrentIndex, length).ToArray();
        CurrentIndex += moveIndexPosition ? typeSize : 0;
        return value;
    }

    public bool ReadBool(bool moveIndexPosition = true)
    {
        int typeSize = sizeof(bool);
        var value = BitConverter.ToBoolean(new byte[] { byteList[CurrentIndex] }, 0);
        CurrentIndex += moveIndexPosition ? typeSize : 0;
        return value;
    }

    public bool ReadBools(bool moveIndexPosition = true)
    {
        int length = ReadInt();
        int typeSize = length * sizeof(bool) + sizeof(int);
        var value = new bool[length];
        for (int i = 0; i < length; i++)
            value[i] = ReadBool();
        CurrentIndex -= moveIndexPosition ? 0 : typeSize;
        return value;
    }

    public char ReadChar(bool moveIndexPosition = true)
    {
        int typeSize = sizeof(char);
        var value = BitConverter.ToChar(new byte[] { byteList[CurrentIndex] }, 0);
        CurrentIndex += moveIndexPosition ? typeSize : 0;
        return value;
    }

    public char[] ReadChars(bool moveIndexPosition = true)
    {
        int length = ReadInt();
        int typeSize = length * sizeof(char) + sizeof(int);
        var value = new char[length];
        for (int i = 0; i < length; i++)
            value[i] = ReadChar();
        CurrentIndex -= moveIndexPosition ? 0 : typeSize;
        return value;
    }

    public double ReadDouble(bool moveIndexPosition = true)
    {
        int typeSize = sizeof(double);
        var value = BitConverter.ToDouble(ToProperEndian(byteList.GetRange(CurrentIndex, typeSize).ToArray()), 0);
        CurrentIndex += moveIndexPosition ? typeSize : 0;
        return value;
    }

    public double[] ReadDoubles(bool moveIndexPosition = true)
    {
        int length = ReadInt();
        int typeSize = length * sizeof(double) + sizeof(int);
        var value = new double[length];
        for (int i = 0; i < length; i++)
            value[i] = ReadDouble();
        CurrentIndex -= moveIndexPosition ? 0 : typeSize;
        return value;
    }

    public float ReadFloat(bool moveIndexPosition = true)
    {
        int typeSize = sizeof(float);
        var value = BitConverter.ToSingle(ToProperEndian(byteList.GetRange(CurrentIndex, typeSize).ToArray()), 0);
        CurrentIndex += moveIndexPosition ? typeSize : 0;
        return value;
    }

    public float[] ReadFloats(bool moveIndexPosition = true)
    {
        int length = ReadInt();
        int typeSize = length * sizeof(float) + sizeof(int);
        var value = new float[length];
        for (int i = 0; i < length; i++)
            value[i] = ReadFloat();
        CurrentIndex -= moveIndexPosition ? 0 : typeSize;
        return value;
    }

    public int ReadInt(bool moveIndexPosition = true)
    {
        int typeSize = sizeof(int);
        var value = BitConverter.ToInt32(ToProperEndian(byteList.GetRange(CurrentIndex, typeSize).ToArray()), 0);
        CurrentIndex += moveIndexPosition ? typeSize : 0;
        return value;
    }

    public int[] ReadInts(bool moveIndexPosition = true)
    {
        int length = ReadInt();
        int typeSize = length * sizeof(int) + sizeof(int);
        var value = new int[length];
        for (int i = 0; i < length; i++)
            value[i] = ReadInt();
        CurrentIndex -= moveIndexPosition ? 0 : typeSize;
        return value;
    }

    public long ReadLong(bool moveIndexPosition = true)
    {
        int typeSize = sizeof(long);
        var value = BitConverter.ToInt64(ToProperEndian(byteList.GetRange(CurrentIndex, typeSize).ToArray()), 0);
        CurrentIndex += moveIndexPosition ? typeSize : 0;
        return value;
    }

    public long[] ReadLongs(bool moveIndexPosition = true)
    {
        int length = ReadInt();
        int typeSize = length * sizeof(long) + sizeof(int);
        var value = new long[length];
        for (int i = 0; i < length; i++)
            value[i] = ReadLong();
        CurrentIndex -= moveIndexPosition ? 0 : typeSize;
        return value;
    }

    public short ReadShort(bool moveIndexPosition = true)
    {
        int typeSize = sizeof(short);
        var value = BitConverter.ToInt16(ToProperEndian(byteList.GetRange(CurrentIndex, typeSize).ToArray()), 0);
        CurrentIndex += moveIndexPosition ? typeSize : 0;
        return value;
    }

    public short[] ReadShorts(bool moveIndexPosition = true)
    {
        int length = ReadInt();
        int typeSize = length * sizeof(short) + sizeof(int);
        var value = new short[length];
        for (int i = 0; i < length; i++)
            value[i] = ReadShort();
        CurrentIndex -= moveIndexPosition ? 0 : typeSize;
        return value;
    }

    public uint ReadUInt(bool moveIndexPosition = true)
    {
        int typeSize = sizeof(uint);
        var value = BitConverter.ToUInt32(ToProperEndian(byteList.GetRange(CurrentIndex, typeSize).ToArray()), 0);
        CurrentIndex += moveIndexPosition ? typeSize : 0;
        return value;
    }

    public uint[] ReadUInts(bool moveIndexPosition = true)
    {
        int length = ReadInt();
        int typeSize = length * sizeof(uint) + sizeof(int);
        var value = new uint[length];
        for (int i = 0; i < length; i++)
            value[i] = ReadUInt();
        CurrentIndex -= moveIndexPosition ? 0 : typeSize;
        return value;
    }

    public ulong ReadULong(bool moveIndexPosition = true)
    {
        int typeSize = sizeof(ulong);
        var value = BitConverter.ToUInt64(ToProperEndian(byteList.GetRange(CurrentIndex, typeSize).ToArray()), 0);
        CurrentIndex += moveIndexPosition ? typeSize : 0;
        return value;
    }

    public ulong[] ReadULongs(bool moveIndexPosition = true)
    {
        int length = ReadInt();
        int typeSize = length * sizeof(ulong) + sizeof(int);
        var value = new ulong[length];
        for (int i = 0; i < length; i++)
            value[i] = ReadULong();
        CurrentIndex -= moveIndexPosition ? 0 : typeSize;
        return value;
    }

    public ushort ReadUShort(bool moveIndexPosition = true)
    {
        int typeSize = sizeof(short);
        var value = BitConverter.ToUInt16(ToProperEndian(byteList.GetRange(CurrentIndex, typeSize).ToArray()), 0);
        CurrentIndex += moveIndexPosition ? typeSize : 0;
        return value;
    }

    public ushort[] ReadUShorts(bool moveIndexPosition = true)
    {
        int length = ReadInt();
        int typeSize = length * sizeof(ushort) + sizeof(int);
        var value = new ushort[length];
        for (int i = 0; i < length; i++)
            value[i] = ReadUShort();
        CurrentIndex -= moveIndexPosition ? 0 : typeSize;
        return value;
    }

    public string ReadString(bool moveIndexPosition = true)
    {
        int strLen = ReadInt();
        var value = Encoding.UTF8.GetString(byteList.GetRange(CurrentIndex + 4, strLen).ToArray());
        CurrentIndex += moveIndexPosition ? strLen + 4 : 0;
        return value;
    }

    public string[] ReadStrings(bool moveIndexPosition = true)
    {
        int length = ReadInt();
        int typeSize = sizeof(int);
        var value = new string[length];
        for (int i = 0; i < length; i++)
            value[i] = ReadString();
        CurrentIndex -= moveIndexPosition ? 0 : typeSize;
        return value;
    }
}