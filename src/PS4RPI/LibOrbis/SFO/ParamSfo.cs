//Code adapted from https://github.com/xXxTheDarkprogramerxXx/PS4_Tools

using PS4_Tools.LibOrbis.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace PS4_Tools.LibOrbis.SFO
{
    public class ParamSfo
    {
        //public Value this[string name]
        //{
        //    get { return GetValueByName(name); }
        //    set
        //    {
        //        if (GetValueByName(name) is Value)
        //{

        //            Value v = GetValueByName(name);
        //            Values.Remove(v);
        //        }
        //        Values.Add(value);
        //        Values.Sort((v1, v2) => v1.Name.CompareTo(v2.Name));
        //    }
        //}
        public List<Value> Values;
        public ParamSfo()
        {
            Values = new List<Value>();
        }
        //public Value GetValueByName(string name)
        //{
        //    foreach (var v in Values)
        //    {
        //        if (v.Name == name) return v;
        //    }
        //    return null;
        //}

        public static ParamSfo FromStream(Stream s)
        {
            var ret = new ParamSfo();
            var start = s.Position;
            s.Position = start + 8;
            var keyTableStart = s.ReadInt32LE();
            var dataTableStart = s.ReadInt32LE();
            var numValues = s.ReadInt32LE();
            for (int value = 0; value < numValues; value++)
            {
                s.Position = value * 0x10 + 0x14 + start;
                var keyOffset = s.ReadUInt16LE();
                var format = (SfoEntryType)s.ReadUInt16LE();
                var len = s.ReadInt32LE();
                var maxLen = s.ReadInt32LE();
                var dataOffset = s.ReadUInt32LE();
                s.Position = start + keyTableStart + keyOffset;
                var name = s.ReadASCIINullTerminated();
                s.Position = start + dataTableStart + dataOffset;
                switch (format)
                {
                    case SfoEntryType.Integer:
                        ret.Values.Add(new IntegerValue(name, s.ReadInt32LE()));
                        break;
                    case SfoEntryType.Utf8:
                        ret.Values.Add(new Utf8Value(name, Encoding.UTF8.GetString(s.ReadBytes(len > 0 ? len - 1 : len)), maxLen));
                        break;
                    case SfoEntryType.Utf8Special:
                        ret.Values.Add(new IntegerValue(name, s.ReadInt32LE()));
                        break;
                    default:
                        throw new Exception($"Unknown SFO type: {(ushort)format:X4}");
                }
            }
            return ret;
        }
        int keyTableOffset => 0x14 + (Values.Count * 0x10);

        public int FileSize => CalcSize().Item2;

        /// <summary>
        /// 
        /// </summary>
        /// <returns>A tuple containing the offset of the data table and the total file size.</returns>
        private Tuple<int, int> CalcSize()
        {
            int keyTableSize = 0x0;
            int dataSize = 0x0;
            Values.Sort((v1, v2) => v1.Name.CompareTo(v2.Name));
            foreach (var v in Values)
            {
                keyTableSize += v.Name.Length + 1;
                dataSize += v.MaxLength;
            }
            int dataTableOffset = keyTableOffset + keyTableSize;
            if (dataTableOffset % 4 != 0) dataTableOffset += 4 - (dataTableOffset % 4);
            return Tuple.Create(dataTableOffset, dataSize + dataTableOffset);
        }

        public static ParamSfo DefaultAC = new ParamSfo()
        {
            Values = new List<Value>
            {
                new IntegerValue("ATTRIBUTE", 0),
                new Utf8Value("CATEGORY", "ac", 4),
                new Utf8Value("CONTENT_ID", "AAXXXX-BBBBYYYYY_00-ZZZZZZZZZZZZZZZZ", 48),
                new Utf8Value("FORMAT", "obs", 4),
                new Utf8Value("TITLE", "Title", 128),
                new Utf8Value("TITLE_ID", "BBBBYYYYY", 12),
                new Utf8Value("VERSION", "01.00", 8),
            }
        };
        public static ParamSfo DefaultGD = new ParamSfo()
        {
            Values = new List<Value>
            {

            }
        };
    }

    public enum SfoEntryType : ushort
    {
        Utf8Special = 0x4,
        Utf8 = 0x204,
        Integer = 0x404
    };
    public abstract class Value
    {
        public Value(string name, SfoEntryType type)
        {
            Name = name; Type = type;
        }
        public SfoEntryType Type;
        public string Name;
        public abstract int Length { get; }
        public abstract int MaxLength { get; }
        public abstract byte[] ToByteArray();
    }
    
    public class Utf8Value : Value
    {
        public Utf8Value(string name, string value, int maxLength)
          : base(name, SfoEntryType.Utf8)
        {
            Type = SfoEntryType.Utf8;
            MaxLength = maxLength;
            Value = value;
        }
        public override int Length => Encoding.UTF8.GetByteCount(Value) + 1;
        public override int MaxLength { get; }
        public string Value;
        public override byte[] ToByteArray() => Encoding.UTF8.GetBytes(Value);
        public override string ToString()
        {
            return Value;
        }
    }
    public class IntegerValue : Value
    {
        public IntegerValue(string name, int value)
          : base(name, SfoEntryType.Integer)
        {
            Type = SfoEntryType.Integer;
            Value = value;
        }
        public override int Length => 4;
        public override int MaxLength => 4;
        public int Value;
        public override byte[] ToByteArray() => BitConverter.GetBytes(Value);
        public override string ToString()
        {
            return $"0x{Value:x8}";
        }
    }
}
