﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using MSAddonLib.Domain.Addon;

namespace MSAddonLib.Persistence.AddonDB
{
    public sealed class AssetSearchCriteria
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

        public List<string> Tags { get; private set; }


        public AssetSearchCriteria(string pName, AddonAssetType pAssetType, string pTags)
        {
            Name = pName?.Trim().ToLower();
            if (!string.IsNullOrEmpty(Name))
                _nameRegex = new Regex($"{Name}*", RegexOptions.IgnoreCase);
            AssetType = pAssetType;
            Tags = pTags?.Trim().ToLower().Split(" ,;".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).ToList();
        }


        public List<AssetSearchResultItem> SearchAsset(AddonPackage pPackage)
        {
            List<AssetSearchResultItem> found = new List<AssetSearchResultItem>();

            AssetSearchResultItem baseResultItem = new AssetSearchResultItem()
            {
                AddonName = pPackage.Name, AddonPublisher = pPackage.Publisher,
                Free = pPackage.Free, Location = pPackage.Location
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

            if (AssetType.HasFlag(AddonAssetType.Filter) && (((pPackage.Filters?.Count ?? 0) > 0)))
            {
                SearchCommon(pPackage.Filters, AddonAssetType.Filter, baseResultItem, found);
            }

            if (AssetType.HasFlag(AddonAssetType.SkyTexture) && (((pPackage.SkyTextures?.Count ?? 0) > 0)))
            {
                SearchCommon(pPackage.SkyTextures, AddonAssetType.SkyTexture, baseResultItem, found);
            }

            return found.Count > 0 ? found : null;
        }



        private void SearchCommon(List<string> pAssets, AddonAssetType pAssetType, AssetSearchResultItem pBaseResultItem, List<AssetSearchResultItem> pFound)
        {
            foreach (string asset in pAssets)
            {

                Tuple<string, string> parts = Split(asset);
                if (parts == null)
                    continue;

                if ((_nameRegex != null) && (!_nameRegex.IsMatch(parts.Item1)))
                    continue;
                
                AssetSearchResultItem item = (AssetSearchResultItem)pBaseResultItem.Clone();
                item.AssetType = pAssetType;
                item.AssetSubtype = parts.Item2 ?? pAssetType.ToString();
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
                    if((_nameRegex != null) && (!_nameRegex.IsMatch(part.BodyPartName)))
                        continue;

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

                    AssetSearchResultItem item = (AssetSearchResultItem) pBaseResultItem.Clone();
                    item.AssetType = AddonAssetType.BodyPart;
                    item.AssetSubtype = part.BodyPartType;
                    item.Name = $"{puppetName} : {part.BodyPartName}";
                    item.Tags = StringListToString(part.Tags);
                    item.ExtraInfo = $"{part.Covers}"; 
                    if (part.Morphable)
                        item.ExtraInfo += " [Morphable]";
                    pFound.Add(item);
                }
            }
        }

        private void SearchDecals(List<BodyModelSumPuppet> pPuppets, AssetSearchResultItem pBaseResultItem, List<AssetSearchResultItem> pFound)
        {
            foreach (BodyModelSumPuppet puppet in pPuppets)
            {
                if (puppet.Decals == null)
                    continue;

                string puppetName = puppet.PuppetName;

                foreach (BodyModelSumDecal decal in puppet.Decals)
                {
                    if ((_nameRegex != null) && (!_nameRegex.IsMatch(decal.DecalName)))
                        continue;

                    AssetSearchResultItem item = (AssetSearchResultItem)pBaseResultItem.Clone();
                    item.AssetType = AddonAssetType.Decal;
                    item.AssetSubtype = decal.Group;
                    item.Name = $"{puppetName} : {decal.DecalName}";
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

                AssetSearchResultItem item = (AssetSearchResultItem)pBaseResultItem.Clone();
                item.AssetType = AddonAssetType.Prop;
                item.AssetSubtype = prop.PropType;
                item.Name = prop.PropName;
                item.Tags = StringListToString(prop.Tags);
                item.ExtraInfo = prop.AttributesString;
                if ((prop.Variants?.Count ?? 0) > 1)
                    item.ExtraInfo += $"  ({prop.Variants.Count} Variants)";
                if (prop.MultiPart)
                    item.ExtraInfo += " [Multipart]";
                pFound.Add(item);
            }
        }


        private void SearchVerbs(VerbCollection pVerbs, AssetSearchResultItem pBaseResultItem, List<AssetSearchResultItem> pFound)
        {
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

                AssetSearchResultItem item = (AssetSearchResultItem)pBaseResultItem.Clone();
                item.AssetType = AddonAssetType.Verb;
                item.AssetSubtype = verb.VerbType;
                item.Name = verb.VerbName;
                item.ExtraInfo = GetVerbExtraInfo(verb, pVerbType);
                /*
                item.ExtraInfo = $"[{verb.ModelA}]";
                if(verb.ModelB != null)
                    item.ExtraInfo += $"[{verb.ModelB}]";
                    */
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



        private string StringListToString(List<string> pList)
        {
            if (pList == null)
                return null;

            StringBuilder builder = new StringBuilder();
            foreach (string item in pList)
                builder.Append($"{item} ");

            return builder.ToString().Trim();
        }

        private Tuple<string, string> Split(string pString)
        {
            if (string.IsNullOrEmpty(pString = pString.Trim()))
                return null;
            int index = pString.LastIndexOf("/");
            if (index < 0)
                return new Tuple<string, string>(pString, null);

            return new Tuple<string, string>(pString.Substring(index + 1), pString.Substring(0, index));
        }



    }
}