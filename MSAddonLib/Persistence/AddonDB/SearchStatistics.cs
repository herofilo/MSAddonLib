using System.Collections.Generic;
using MSAddonLib.Domain.Addon;

namespace MSAddonLib.Persistence.AddonDB
{
    public sealed class SearchStatistics : AddonAssetSummary
    {
        public int Addons { get; set; }

        public int Publishers { get; set; }

        public int TotalAssets => Bodyparts + Decals + Props + Verbs + Animations
                                  + Materials + Sounds + CuttingRoomAssets + SpecialEffects
                                  + Stocks + StartMovies + SkyTextures + OtherAssets;

        public SearchStatistics(List<AssetSearchResultItem> pAssets)
        {
            if ((pAssets == null) || (pAssets.Count == 0))
                return;

            Bodyparts = pAssets.FindAll(o => o.AssetType == AddonAssetType.BodyPart).Count;
            Decals = pAssets.FindAll(o => o.AssetType == AddonAssetType.Decal).Count;
            Props = pAssets.FindAll(o => o.AssetType == AddonAssetType.Prop).Count;
            Verbs = pAssets.FindAll(o => o.AssetType == AddonAssetType.Verb).Count;
            Animations = pAssets.FindAll(o => o.AssetType == AddonAssetType.Animation).Count;
            Materials = pAssets.FindAll(o => o.AssetType == AddonAssetType.Material).Count;
            Sounds = pAssets.FindAll(o => o.AssetType == AddonAssetType.Sound).Count;
            CuttingRoomAssets = pAssets.FindAll(o => o.AssetType == AddonAssetType.CuttingRoomAsset).Count;
            SpecialEffects = pAssets.FindAll(o => o.AssetType == AddonAssetType.SpecialEffect).Count;
            Stocks = pAssets.FindAll(o => o.AssetType == AddonAssetType.Stock).Count;
            StartMovies = pAssets.FindAll(o => o.AssetType == AddonAssetType.StartMovie).Count;
            SkyTextures = pAssets.FindAll(o => o.AssetType == AddonAssetType.SkyTexture).Count;
            OtherAssets = pAssets.FindAll(o => o.AssetType == AddonAssetType.OtherAsset).Count;

            List<string> addons = new List<string>();
            List<string> publishers = new List<string>();

            foreach (AssetSearchResultItem asset in pAssets)
            {
                string publisher = asset.AddonPublisher.ToLower();
                if(!publishers.Contains(publisher))
                    publishers.Add(publisher);

                string addon = $"{publisher}::{asset.AddonName.ToLower()}";
                if(!addons.Contains(addon))
                    addons.Add(addon);
            }

            Addons = addons.Count;
            Publishers = publishers.Count;
        }

    }
}