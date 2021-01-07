//Code adapted from https://github.com/xXxTheDarkprogramerxXx/PS4_Tools

using System.Collections.Generic;
using System.IO;
using PS4_Tools.LibOrbis.Util;

namespace PS4_Tools.LibOrbis.PKG
{
    // Represents the data of an entry
    public abstract class Entry
    {
        public abstract EntryId Id { get; }
        public abstract uint Length { get; }
        public abstract string Name { get; }
        //public abstract void Write(Stream s);
        public MetaEntry meta;
    }

    /// <summary>
    /// The representation of an entry in the PKG entry table.
    /// </summary>
    public class MetaEntry
    {
        public EntryId id;
        public uint NameTableOffset;
        public uint Flags1;
        public uint Flags2;
        public uint DataOffset;
        public uint DataSize;
        // public ulong Pad; // zero-pad

        public static MetaEntry Read(Stream s)
        {
            var ret = new MetaEntry();
            ret.id = (EntryId)s.ReadUInt32BE();
            ret.NameTableOffset = s.ReadUInt32BE();
            ret.Flags1 = s.ReadUInt32BE();
            ret.Flags2 = s.ReadUInt32BE();
            ret.DataOffset = s.ReadUInt32BE();
            ret.DataSize = s.ReadUInt32BE();
            s.Position += 8;
            return ret;
        }
    }

    public class SfoEntry : Entry
    {
        public readonly SFO.ParamSfo ParamSfo;
        public SfoEntry(SFO.ParamSfo paramSfo)
        {
            ParamSfo = paramSfo;
        }
        public override EntryId Id => EntryId.PARAM_SFO;
        public override string Name => "param.sfo";
        public override uint Length => (uint)ParamSfo.FileSize;
    }


    public class MetasEntry : Entry
    {
        public List<MetaEntry> Metas = new List<MetaEntry>();
        public override EntryId Id => EntryId.METAS;
        public override uint Length => (uint)Metas.Count * 32;
        public override string Name => null;
    }

}
