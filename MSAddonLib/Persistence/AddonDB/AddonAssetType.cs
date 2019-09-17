using System;

namespace MSAddonLib.Persistence.AddonDB
{
    [Flags]
    public enum AddonAssetType : ulong
    {
        Null = 0,
        BodyPart = 0x0001,
        Decal = 0x0002,
        Prop = 0x0010,
        Verb = 0x0020,
        Animation = 0x0040,
        Material = 0x0100,
        Sound = 0x0200,
        Filter = 0x0400,
        SkyTexture = 0x0800,
        SpecialEffect = 0x1000,
        Stock = 0x2000,
        StartMovie = 0x4000,
        Any = 0xffff
    }
}