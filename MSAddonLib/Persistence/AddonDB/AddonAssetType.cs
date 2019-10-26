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
        CuttingRoomAsset = 0x0400,
        SkyTexture = 0x0800,
        SpecialEffect = 0x1000,
        OtherAsset = 0x2000,
        Stock = 0x4000,
        StartMovie = 0x8000,
        Any = 0xffff
    }
}