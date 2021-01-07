//Code adapted from https://github.com/xXxTheDarkprogramerxXx/PS4_Tools

using System;

namespace PS4_Tools.LibOrbis
{
    public enum DrmType : int
    {
        NONE = 0x0,
        PS4 = 0xF,
    };

    public enum ContentType : int
    {
        /* pkg_ps4_app, pkg_ps4_patch, pkg_ps4_remaster */
        GD = 0x1A,
        /* pkg_ps4_ac_data, pkg_ps4_sf_theme, pkg_ps4_theme */
        AC = 0x1B,
        /* pkg_ps4_ac_nodata */
        AL = 0x1C,
        /* pkg_ps4_delta_patch */
        DP = 0x1E,
    };

    public enum IROTag : int
    {
        None = 0,
        /* SHAREfactory theme */
        SF_THEME = 0x1,
        /* System Software theme */
        SS_THEME = 0x2,
    };

    [Flags]
    public enum PKGFlags : uint
    {
        Unknown = 0x01,
        VER_1 = 0x01000000,
        VER_2 = 0x02000000,
        INTERNAL = 0x40000000,
        FINALIZED = 0x80000000,
    }

    [Flags]
    public enum ContentFlags : uint
    {
        FIRST_PATCH = 0x00100000,
        PATCHGO = 0x00200000,
        REMASTER = 0x00400000,
        PS_CLOUD = 0x00800000,
        GD_AC = 0x02000000,
        NON_GAME = 0x04000000,
        Unk_x8000000 = 0x08000000, /* has data? */
        SUBSEQUENT_PATCH = 0x40000000,
        DELTA_PATCH = 0x41000000,
        CUMULATIVE_PATCH = 0x60000000,
    }

    public enum EntryId : uint
    {
        METAS = 0x00000100,
        LICENSE_DAT = 0x00000400,
        PARAM_SFO = 0x00001000,
    };
}
