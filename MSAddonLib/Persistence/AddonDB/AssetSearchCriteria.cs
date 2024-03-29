﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms.VisualStyles;
using MSAddonLib.Domain.Addon;

namespace MSAddonLib.Persistence.AddonDB
{
    public sealed class AssetSearchCriteria : SearchCriteriaBase
    {
        public static string ContentPacksPath
        {
            get { return _contentPacksPath; }
            set { _contentPacksPath = value?.ToLower(); }
        }
        private static string _contentPacksPath;


        public string Name { get; private set; }

        private readonly Regex _nameRegex = null;

        public AddonAssetType AssetType { get; private set; }

        public string AssetSubTypes { get; private set; }

        private Regex _assetSubTypesRegex = null;

        public string Tags { get; private set; }

        private Regex _tagsRegex = null;

        public string ExtraInfo { get; private set; }

        private Regex _assetExtraInfoRegex = null;

        private const string ExtraInfoNull = "{N*0@M_A?T*C=H}";


        // ----------------------------------------------------------------------------------------------------


        public AssetSearchCriteria(string pName, AddonAssetType pAssetType, string pAssetSubTypes, string pTags, string pExtraInfo)
        {
            Name = pName;
            _nameRegex = CreateMultiValuedRegex(Name);

            AssetType = pAssetType;

            AssetSubTypes = pAssetSubTypes;
            _assetSubTypesRegex = CreateMultiValuedRegex(AssetSubTypes);

            Tags = pTags;
            _tagsRegex = CreateMultiValuedRegex(Tags);

            ExtraInfo = pExtraInfo;
            _assetExtraInfoRegex = CreateMultiValuedRegex(ExtraInfo);
        }
        


        public List<AssetSearchResultItem> SearchAsset(AddonPackage pPackage)
        {
            List<AssetSearchResultItem> found = new List<AssetSearchResultItem>();

            AssetSearchResultItem baseResultItem = new AssetSearchResultItem()
            {
                AddonName = pPackage.Name,
                AddonPublisher = pPackage.Publisher,
                Free = pPackage.Free,
                Location = pPackage.Location
            };
            baseResultItem.Installed = (pPackage.AddonFormat == AddonPackageFormat.InstalledFolder);
            baseResultItem.ContentPack = pPackage.Location.ToLower().StartsWith(ContentPacksPath);

            if ((pPackage.BodyModelsSummary?.Puppets?.Puppets.Count ?? 0) > 0)
            {
                if (AssetType.HasFlag(AddonAssetType.BodyPart))
                {
                    SearchBodyParts(pPackage.BodyModelsSummary.Puppets.Puppets, baseResultItem, found);
                }

                if (AssetType.HasFlag(AddonAssetType.Decal))
                {
                    SearchDecals(pPackage.BodyModelsSummary.Puppets.Puppets, baseResultItem, found);
                }
            }

            if (AssetType.HasFlag(AddonAssetType.Prop) && ((pPackage.PropModelsSummary.Props?.Props.Count ?? 0) > 0))
            {
                SearchProps(pPackage.PropModelsSummary.Props.Props, baseResultItem, found);
            }

            if (AssetType.HasFlag(AddonAssetType.Verb) && (pPackage.VerbsSummary?.Verbs.HasData ?? false))
            {
                SearchVerbs(pPackage.VerbsSummary.Verbs, baseResultItem, found);
            }


            if (AssetType.HasFlag(AddonAssetType.Animation))
            {
                SearchAnimations(pPackage.AssetManifest, baseResultItem, found);
            }

            if (AssetType.HasFlag(AddonAssetType.CuttingRoomAsset))
            {
                SearchCuttingRoomAssets(pPackage.CuttingRoomAssetsSummary, baseResultItem, found);
            }

            if (AssetType.HasFlag(AddonAssetType.Material) && (((pPackage.Materials?.Count ?? 0) > 0)))
            {
                SearchCommon(pPackage.Materials, AddonAssetType.Material, baseResultItem, found);
            }


            if (AssetType.HasFlag(AddonAssetType.Sound) && (((pPackage.Sounds?.Count ?? 0) > 0)))
            {
                SearchCommon(pPackage.Sounds, AddonAssetType.Sound, baseResultItem, found);
            }

            if (AssetType.HasFlag(AddonAssetType.SpecialEffect) && (((pPackage.SpecialEffects?.Count ?? 0) > 0)))
            {
                SearchCommon(pPackage.SpecialEffects, AddonAssetType.SpecialEffect, baseResultItem, found);
            }

            /*
            if (AssetType.HasFlag(AddonAssetType.CuttingRoomAsset) && (((pPackage.Filters?.Count ?? 0) > 0)))
            {
                SearchCommon(pPackage.Filters, AddonAssetType.CuttingRoomAsset, baseResultItem, found);
            }
            */

            if (AssetType.HasFlag(AddonAssetType.SkyTexture) && (((pPackage.SkyTextures?.Count ?? 0) > 0)))
            {
                SearchCommon(pPackage.SkyTextures, AddonAssetType.SkyTexture, baseResultItem, found);
            }

            if (AssetType.HasFlag(AddonAssetType.OtherAsset) && (((pPackage.OtherAssets?.Count ?? 0) > 0)))
            {
                SearchCommon(pPackage.OtherAssets, AddonAssetType.OtherAsset, baseResultItem, found);
            }



            if (AssetType.HasFlag(AddonAssetType.Stock) && (((pPackage.StockAssets?.Count ?? 0) > 0)))
            {
                SearchCommon(pPackage.StockAssets, AddonAssetType.Stock, baseResultItem, found);
            }

            if (AssetType.HasFlag(AddonAssetType.StartMovie) && (((pPackage.DemoMovies?.Count ?? 0) > 0)))
            {
                SearchCommon2(pPackage.DemoMovies, AddonAssetType.StartMovie, baseResultItem, found);
            }


            return found.Count > 0 ? found : null;
        }




        private void SearchCommon2(List<string> pAssets, AddonAssetType pAssetType, AssetSearchResultItem pBaseResultItem, List<AssetSearchResultItem> pFound)
        {
            if ((_tagsRegex != null) || (_assetExtraInfoRegex != null))
                return;

            foreach (string asset in pAssets)
            {
                if ((_nameRegex != null) && (!_nameRegex.IsMatch(asset)))
                    continue;

                string subType = pAssetType.ToString();
                if (_assetSubTypesRegex != null)
                    if (!_assetSubTypesRegex.IsMatch(subType))
                        continue;

                AssetSearchResultItem item = (AssetSearchResultItem)pBaseResultItem.Clone();
                item.AssetType = pAssetType;
                item.AssetSubtype = subType;
                item.Name = asset;
                pFound.Add(item);
            }
        }


        private void SearchCommon(List<string> pAssets, AddonAssetType pAssetType, AssetSearchResultItem pBaseResultItem, List<AssetSearchResultItem> pFound)
        {
            if ((_tagsRegex != null) || (_assetExtraInfoRegex != null))
                return;

            foreach (string asset in pAssets)
            {
                string splitChar = ((pAssetType == AddonAssetType.Stock) || (pAssetType == AddonAssetType.OtherAsset)) ? ":" : "/";
                Tuple<string, string> parts = Split(asset, splitChar);
                if (parts == null)
                    continue;

                if ((_nameRegex != null) && (!_nameRegex.IsMatch(parts.Item1)))
                    continue;

                string subType = parts.Item2 ?? pAssetType.ToString();
                if (_assetSubTypesRegex != null)
                    if (!_assetSubTypesRegex.IsMatch(subType))
                        continue;

                AssetSearchResultItem item = (AssetSearchResultItem)pBaseResultItem.Clone();
                item.AssetType = pAssetType;
                item.AssetSubtype = subType;

                item.Name = parts.Item1;
                pFound.Add(item);
            }
        }


        private void SearchBodyParts(List<BodyModelSumPuppet> pPuppets, AssetSearchResultItem pBaseResultItem, List<AssetSearchResultItem> pFound)
        {
            foreach (BodyModelSumPuppet puppet in pPuppets)
            {
                if (puppet.BodyParts == null)
                    continue;

                string puppetName = puppet.PuppetName;

                foreach (BodyModelSumBodyPart part in puppet.BodyParts)
                {
                    if ((_nameRegex != null) && (!_nameRegex.IsMatch(part.BodyPartName)))
                        continue;
                    /*
                    if ((Tags != null) && (Tags.Count > 0))
                    {
                        if ((part.Tags == null) || (part.Tags.Count == 0))
                            continue;
                        bool gotTag = false;
                        foreach (string tagSearched in Tags)
                        {
                            if (part.Tags.Contains(tagSearched))
                            {
                                gotTag = true;
                                break;
                            }
                        }

                        if (!gotTag)
                            continue;
                    }
                    */
                    if (!TagFilterOk(part.Tags))
                        continue;

                    if (_assetSubTypesRegex != null)
                        if (!_assetSubTypesRegex.IsMatch(part.BodyPartType))
                            continue;

                    string extraInfo = $"{part.Covers}";
                    if (part.Morphable)
                        extraInfo += " [Morphable]";
                    if ((_assetExtraInfoRegex != null) && !_assetExtraInfoRegex.IsMatch(extraInfo ?? ExtraInfoNull))
                         continue;

                    AssetSearchResultItem item = (AssetSearchResultItem)pBaseResultItem.Clone();
                    item.AssetType = AddonAssetType.BodyPart;
                    item.AssetSubtype = part.BodyPartType;
                    item.Name = $"{puppetName} : {part.BodyPartName}";
                    item.Tags = StringListToString(part.Tags);
                    item.ExtraInfo = extraInfo;

                    pFound.Add(item);
                }
            }
        }

        private void SearchDecals(List<BodyModelSumPuppet> pPuppets, AssetSearchResultItem pBaseResultItem, List<AssetSearchResultItem> pFound)
        {
            if ((_tagsRegex != null) || (_assetExtraInfoRegex != null))
                return;

            foreach (BodyModelSumPuppet puppet in pPuppets)
            {
                if (puppet.Decals == null)
                    continue;

                string puppetName = puppet.PuppetName;
                bool foundAnyDecal = false;
                foreach (BodyModelSumDecal decal in puppet.Decals)
                {
                    if ((_nameRegex != null) && (!_nameRegex.IsMatch(decal.DecalName)))
                        continue;

                    if (_assetSubTypesRegex != null)
                        if (!_assetSubTypesRegex.IsMatch(decal.Group))
                            continue;

                    AssetSearchResultItem item = (AssetSearchResultItem)pBaseResultItem.Clone();
                    item.AssetType = AddonAssetType.Decal;
                    item.AssetSubtype = decal.Group;
                    item.Name = $"{puppetName} : {decal.DecalName}";
                    pFound.Add(item);
                    foundAnyDecal = true;
                }

                if (puppet.ExternDecalReferenced && !foundAnyDecal)
                {
                    if ((_nameRegex != null) && (!_nameRegex.IsMatch("Extern")))
                        continue;

                    if ((_assetSubTypesRegex != null) && (!_assetSubTypesRegex.IsMatch("?")))
                        continue;

                    AssetSearchResultItem item = (AssetSearchResultItem)pBaseResultItem.Clone();
                    item.AssetType = AddonAssetType.Decal;
                    item.AssetSubtype = "?";
                    item.Name = $"{puppetName} : Extern Decal Referenced";
                    pFound.Add(item);
                }
            }
        }



        private void SearchProps(List<PropModelSumProp> pProps, AssetSearchResultItem pBaseResultItem, List<AssetSearchResultItem> pFound)
        {

            foreach (PropModelSumProp prop in pProps)
            {
                if ((_nameRegex != null) && (!_nameRegex.IsMatch(prop.PropName)))
                    continue;
                /*
                if ((Tags != null) && (Tags.Count > 0))
                {
                    if ((prop.Tags == null) || (prop.Tags.Count == 0))
                        continue;
                    bool gotTag = false;
                    foreach (string tagSearched in Tags)
                    {
                        if (prop.Tags.Contains(tagSearched))
                        {
                            gotTag = true;
                            break;
                        }
                    }

                    if (!gotTag)
                        continue;
                }
                */

                if (!TagFilterOk(prop.Tags))
                    continue;

                if (_assetSubTypesRegex != null)
                    if (!_assetSubTypesRegex.IsMatch(prop.PropType))
                        continue;

                string extraInfo = prop.AttributesString;
                if ((prop.Variants?.Count ?? 0) > 1)
                    extraInfo += PropVariantsInfo(prop.Variants);
                if (prop.MultiPart)
                    extraInfo += " [Multipart]";
                if ((_assetExtraInfoRegex != null) && !_assetExtraInfoRegex.IsMatch(extraInfo ?? ExtraInfoNull))
                    continue;


                AssetSearchResultItem item = (AssetSearchResultItem)pBaseResultItem.Clone();
                item.AssetType = AddonAssetType.Prop;
                item.AssetSubtype = prop.PropType;
                item.Name = prop.PropName;
                item.Tags = StringListToString(prop.Tags);
                item.ExtraInfo = extraInfo;

                pFound.Add(item);
            }
        }


        private string PropVariantsInfo(List<string> pVariants)
        {
            if ((pVariants == null) || (pVariants.Count < 2))
                return "";

            StringBuilder variantBuilder = new StringBuilder();
            foreach (string variant in pVariants)
                variantBuilder.Append($"'{variant}',");

            string variantInfo = variantBuilder.ToString();
            variantInfo = variantInfo.Substring(0, variantInfo.Length - 1);

            return $"  ({pVariants.Count} Variants: {variantInfo})";
        }


        private bool TagFilterOk(List<string> pAssetTags)
        {
            if (_tagsRegex == null)
                return true;

            if ((pAssetTags == null) || (pAssetTags.Count == 0))
                return false;

            bool gotTag = false;
            foreach(string tag in pAssetTags)
                if (_tagsRegex.IsMatch(tag))
                {
                    gotTag = true;
                    break;
                }

            return gotTag;
        }



        private void SearchVerbs(VerbCollection pVerbs, AssetSearchResultItem pBaseResultItem, List<AssetSearchResultItem> pFound)
        {
            if (_tagsRegex != null)
                return;

            if (pVerbs.Flags.HasFlag(VerbCollectionFlags.Gaits))
                SearchVerbsKind(pVerbs.Gaits, VerbCollectionFlags.Gaits, pBaseResultItem, pFound);
            if (pVerbs.Flags.HasFlag(VerbCollectionFlags.Gestures))
                SearchVerbsKind(pVerbs.Gestures, VerbCollectionFlags.Gestures, pBaseResultItem, pFound);
            if (pVerbs.Flags.HasFlag(VerbCollectionFlags.PuppetSoloVerbs))
                SearchVerbsKind(pVerbs.PuppetSoloVerbs, VerbCollectionFlags.PuppetSoloVerbs, pBaseResultItem, pFound);
            if (pVerbs.Flags.HasFlag(VerbCollectionFlags.PuppetMutualVerbs))
                SearchVerbsKind(pVerbs.PuppetMutualVerbs, VerbCollectionFlags.PuppetMutualVerbs, pBaseResultItem, pFound);

            if (pVerbs.Flags.HasFlag(VerbCollectionFlags.PropSoloVerbs))
                SearchVerbsKind(pVerbs.PropSoloVerbs, VerbCollectionFlags.PropSoloVerbs, pBaseResultItem, pFound);
            if (pVerbs.Flags.HasFlag(VerbCollectionFlags.HeldPropsVerbs))
                SearchVerbsKind(pVerbs.HeldPropsVerbs, VerbCollectionFlags.HeldPropsVerbs, pBaseResultItem, pFound);
            if (pVerbs.Flags.HasFlag(VerbCollectionFlags.InteractivePropsVerbs))
                SearchVerbsKind(pVerbs.InteractivePropsVerbs, VerbCollectionFlags.InteractivePropsVerbs, pBaseResultItem, pFound);
        }


        private void SearchVerbsKind(List<VerbSummaryItem> pVerbs, VerbCollectionFlags pVerbType, AssetSearchResultItem pBaseResultItem, List<AssetSearchResultItem> pFound)
        {
            foreach (VerbSummaryItem verb in pVerbs)
            {
                if ((_nameRegex != null) && (!_nameRegex.IsMatch(verb.VerbName)))
                    continue;

                if (_assetSubTypesRegex != null)
                    if (!_assetSubTypesRegex.IsMatch(verb.VerbType))
                        continue;

                string extraInfo = GetVerbExtraInfo(verb, pVerbType);
                if ((_assetExtraInfoRegex != null) && !_assetExtraInfoRegex.IsMatch(extraInfo ?? ExtraInfoNull))
                    continue;

                AssetSearchResultItem item = (AssetSearchResultItem)pBaseResultItem.Clone();
                item.AssetType = AddonAssetType.Verb;
                item.AssetSubtype = verb.VerbType;
                item.Name = verb.VerbName;
                item.ExtraInfo = extraInfo;

                pFound.Add(item);
            }
        }


        private string GetVerbExtraInfo(VerbSummaryItem pVerb, VerbCollectionFlags pVerbType)
        {
            string modelA;

            switch (pVerbType)
            {
                case VerbCollectionFlags.Gaits:
                case VerbCollectionFlags.Gestures:
                    string extraInfo = $"[{pVerb.ModelA}]";
                    if (pVerb.ModelA.EndsWith("!?"))
                        extraInfo += " (WEIRD)";
                    if (pVerb.Iterations > 1)
                        extraInfo += $" (x{pVerb.Iterations} iter.)";
                    return extraInfo;
                case VerbCollectionFlags.PuppetSoloVerbs:
                    modelA = pVerb.ModelA ?? "*";
                    return (pVerb.Iterations == 1)
                        ? $"[{modelA}]"
                        : $"[{modelA}] (x{pVerb.Iterations} iter.)";
                case VerbCollectionFlags.PuppetMutualVerbs:
                    modelA = pVerb.ModelA ?? "*";
                    string modelB = pVerb.ModelB ?? "*";
                    return (pVerb.Iterations == 1)
                        ? $"[{modelA}]->[{modelB}]"
                        : $"[{modelA}]->[{modelB}] (x{pVerb.Iterations} iter.)";
                case VerbCollectionFlags.PropSoloVerbs:
                    return (pVerb.Iterations == 1)
                        ? $"[{pVerb.ModelA}]"
                        : $"[{pVerb.ModelA}] (x{pVerb.Iterations} iter.)";
                case VerbCollectionFlags.HeldPropsVerbs:
                case VerbCollectionFlags.InteractivePropsVerbs:
                    modelA = pVerb.ModelA ?? "*";
                    return (pVerb.Iterations == 1)
                        ? $"[{pVerb.ModelB}][{modelA}]"
                        : $"[{pVerb.ModelB}][{modelA}] (x{pVerb.Iterations} iter.)";
            }

            return null;
        }


        private void SearchCuttingRoomAssets(CuttingRoomAssetsSummary pCuttingRoomAssets, AssetSearchResultItem pBaseResultItem, List<AssetSearchResultItem> pFound)
        {
            if ((pCuttingRoomAssets?.Assets == null) || (!pCuttingRoomAssets.HasData))
                return;

            if (pCuttingRoomAssets.Assets.Flags.HasFlag(CuttingRoomAssetCollectionFlags.Filters))
                SearchCuttingRoomAssetsKind(pCuttingRoomAssets.Assets.Filters, pBaseResultItem, pFound);
            if (pCuttingRoomAssets.Assets.Flags.HasFlag(CuttingRoomAssetCollectionFlags.TextStyles))
                SearchCuttingRoomAssetsKind(pCuttingRoomAssets.Assets.TextStyles, pBaseResultItem, pFound);
            if (pCuttingRoomAssets.Assets.Flags.HasFlag(CuttingRoomAssetCollectionFlags.Transitions))
                SearchCuttingRoomAssetsKind(pCuttingRoomAssets.Assets.Transitions, pBaseResultItem, pFound);
        }

        private void SearchCuttingRoomAssetsKind(List<CuttingRoomAssetItem> pAssets, AssetSearchResultItem pBaseResultItem, List<AssetSearchResultItem> pFound)
        {
            foreach (CuttingRoomAssetItem asset in pAssets)
            {
                if ((_nameRegex != null) && (!_nameRegex.IsMatch(asset.Name)))
                    continue;

                if (_assetSubTypesRegex != null)
                    if (!_assetSubTypesRegex.IsMatch(asset.AssetSubtype))
                        continue;

                if (!TagFilterOk(asset.Tags))
                    continue;
                string tags = null;
                if (!string.IsNullOrEmpty(asset.TagsRaw))
                    tags = asset.TagsRaw.Replace(",", " ");

                string extraInfo =
                    string.IsNullOrEmpty(asset.Description) ? null : asset.Description;
                if ((_assetExtraInfoRegex != null) && !_assetExtraInfoRegex.IsMatch(extraInfo ?? ExtraInfoNull))
                    continue;

                AssetSearchResultItem item = (AssetSearchResultItem)pBaseResultItem.Clone();
                item.AssetType = AddonAssetType.CuttingRoomAsset;
                item.AssetSubtype = asset.AssetSubtype;
                item.Name = asset.Name;
                item.Tags = tags;
                item.ExtraInfo = extraInfo;

                pFound.Add(item);
            }
        }


        private void SearchAnimations(AssetManifest pManifest, AssetSearchResultItem pBaseResultItem, List<AssetSearchResultItem> pFound)
        {
            if (_tagsRegex != null)
                return;

            const string PuppetAnimationToken = "PuppetAnimation";
            if ((_assetSubTypesRegex == null) || _assetSubTypesRegex.IsMatch(PuppetAnimationToken))
            {
                if ((pManifest.BodyModels != null) && (pManifest.BodyModels.Count > 0))
                {
                    foreach (BodyModelItem puppet in pManifest.BodyModels)
                    {
                        if ((puppet.Animations != null) && (puppet.Animations.Count > 0))
                            SearchAnimationsCommon(PuppetAnimationToken, puppet.PuppetName, puppet.Animations,
                                pBaseResultItem, pFound);
                    }
                }
            }

            const string PropAnimationToken = "PropAnimation";
            if ((_assetSubTypesRegex == null) || _assetSubTypesRegex.IsMatch(PropAnimationToken))
            {
                if ((pManifest.PropModels != null) && (pManifest.PropModels.Count > 0))
                {
                    foreach (PropModelItem prop in pManifest.PropModels)
                    {
                        if ((prop.Animations != null) && (prop.Animations.Count > 0))
                            SearchAnimationsCommon(PropAnimationToken, prop.Name, prop.Animations,
                                pBaseResultItem, pFound);
                    }
                }
            }
        }


        private void SearchAnimationsCommon(string pAssetSubType, string pOwnerName, List<string> pAnimations, AssetSearchResultItem pBaseResultItem, List<AssetSearchResultItem> pFound)
        {
            const string prefix = "animations/";
            const string extension = ".caf";
            foreach (string animation in pAnimations)
            {
                if((_nameRegex != null) && !_nameRegex.IsMatch(animation))
                    continue;

                string animationLower = animation?.Trim().ToLower().Replace("\\", "/");
                if(string.IsNullOrEmpty(animationLower))
                    continue;
                string animationName = animation.Trim().Replace("\\", "/");
                if (animationLower.EndsWith(extension))
                    animationName = animationName.Substring(0, animationName.Length - extension.Length);
                if (animationLower.StartsWith(prefix))
                    animationName = animationName.Remove(0, prefix.Length);

                string extraInfo = $"[{pOwnerName}]";
                if ((_assetExtraInfoRegex != null) && !_assetExtraInfoRegex.IsMatch(extraInfo ?? ExtraInfoNull))
                    continue;

                AssetSearchResultItem item = (AssetSearchResultItem)pBaseResultItem.Clone();
                item.AssetType = AddonAssetType.Animation;
                item.AssetSubtype = pAssetSubType;
                item.Name = animationName;
                item.ExtraInfo = extraInfo;

                pFound.Add(item);
            }
        }


        private string StringListToString(List<string> pList)
        {
            if (pList == null)
                return null;

            StringBuilder builder = new StringBuilder();
            foreach (string item in pList)
                builder.Append($"{item} ");

            return builder.ToString().Trim();
        }

        private Tuple<string, string> Split(string pString, string pSplitChar = "/")
        {
            if (string.IsNullOrEmpty(pString = pString.Trim()))
                return null;

            int index = pString.LastIndexOf(pSplitChar, StringComparison.InvariantCulture);
            if (index < 0)
                return new Tuple<string, string>(pString, null);

            return new Tuple<string, string>(pString.Substring(index + 1), pString.Substring(0, index));
        }



    }
}