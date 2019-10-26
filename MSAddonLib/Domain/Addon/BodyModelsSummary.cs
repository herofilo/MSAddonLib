using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using MSAddonLib.Domain.AssetFiles;
using MSAddonLib.Util.Persistence;
using SevenZip;

namespace MSAddonLib.Domain.Addon
{
    /// <summary>
    /// Compiles a summary of all information regarding to Body models in the addon
    /// </summary>
    [Serializable]
    public sealed class BodyModelsSummary
    {
        public BodyModelSumPuppets Puppets { get;  set; }

        [XmlIgnore]
        public AddonPackageSource AddonSource { get; private set; }

        // private SevenZipArchiver Archiver { get; set; }

        private string TempFolderPath { get; set; }

        private List<BodyModelItem> BodyModels { get; set; }

        private bool LoadAnimations { get; set; }

        private bool ListGestureGaitsAnimations { get; set; }

        private bool _initialized = false;

        private readonly List<string> _imageExtensionList = new List<string>()
        {
            ".dds", ".jpg", ".png", ".bmp"
        };




        // ------------------------------------------------------------------------------------------------------

        public BodyModelsSummary()
        {

        }

        public BodyModelsSummary(AddonPackageSource pSource, string pTempFolderPath, List<BodyModelItem> pBodyModels, bool pLoadAnimations = true, bool pListGestureGaitsAnimations = true)
        {
            AddonSource = pSource;
            TempFolderPath = pTempFolderPath?.Trim();
            BodyModels = pBodyModels;
            LoadAnimations = pLoadAnimations;
            ListGestureGaitsAnimations = pListGestureGaitsAnimations;
        }


        // ---------------------------------------------------------------------------------------------------------


        public bool PopulateSummary(out string pErrorText, AddonPackageSource pSource = null, string pTempFolderPath = null, List<BodyModelItem> pBodyModels = null)
        {
            pErrorText = null;
            if (_initialized)
                return true;

            if (!PopulateSummaryPreChecks(ref pSource, ref pTempFolderPath, ref pBodyModels, out pErrorText))
                return false;

            bool isOk = true;
            string assetDataFile = null;
            bool deleteTempAssetDataFile = false;
            try
            {
                if (pSource.SourceType == AddonPackageSourceType.Folder)
                    assetDataFile = Path.Combine(pSource.SourcePath, AddonPackage.AssetDataFilename);
                else
                {
                    assetDataFile = Path.Combine(pTempFolderPath, AddonPackage.AssetDataFilename);
                    pSource.Archiver.ArchivedFilesExtract(pTempFolderPath, new List<string>() { AddonPackage.AssetDataFilename });
                    deleteTempAssetDataFile = true;
                }

                List<string> puppetTextureList = GetPuppetTextures(pSource);

                SevenZipArchiver mftArchiver = new SevenZipArchiver(assetDataFile);

                foreach (BodyModelItem item in pBodyModels)
                {
                    if (Puppets == null)
                        Puppets = new BodyModelSumPuppets(mftArchiver);
                    if (!Puppets.AppendBodyModelItem(item, puppetTextureList, LoadAnimations, out pErrorText))
                    {
                        isOk = false;
                        break;
                    }
                }
            }
            catch (Exception exception)
            {
                pErrorText = $"BodyModelsSummary.PopulateSummary() EXCEPTION: {exception.Message}";
                isOk = false;
            }
            finally
            {
                if(deleteTempAssetDataFile)
                    File.Delete(assetDataFile);
            }

            _initialized = isOk;

            return isOk;
        }




        private bool PopulateSummaryPreChecks(ref AddonPackageSource pSource, ref string pTempFolderPath, ref List<BodyModelItem> pBodyModels, out string pErrorText)
        {
            pErrorText = null;
            if (pSource == null)
            {
                if (AddonSource == null)
                {
                    pErrorText = "No addon source specification";
                    return false;
                }

                pSource = AddonSource;
            }
            else
                AddonSource = pSource;

            if (AddonSource.SourceType == AddonPackageSourceType.Invalid)
            {
                pErrorText = "Invalid addon source";
                return false;
            }


            pTempFolderPath = pTempFolderPath?.Trim();
            if (string.IsNullOrEmpty(pTempFolderPath))
            {
                if (string.IsNullOrEmpty(TempFolderPath))
                {
                    pErrorText = "No temporary folder specification";
                    return false;
                }

                pTempFolderPath = TempFolderPath;
            }
            else
                TempFolderPath = pTempFolderPath;

            if (!Directory.Exists(pTempFolderPath))
            {
                pErrorText = "Temporary folder not found";
                return false;
            }


            if (pBodyModels == null)
            {
                if (BodyModels == null)
                {
                    pErrorText = "No body models specification";
                    return false;
                }

                pBodyModels = BodyModels;
            }
            else
                BodyModels = pBodyModels;

            return true;
        }


        private List<string> GetPuppetTextures(AddonPackageSource pSource)
        {

            List<string> candidateFiles = new List<string>();

            switch (pSource.SourceType)
            {
                case AddonPackageSourceType.Archiver:
                    List<ArchiveFileInfo> filesInfo;
                    pSource.Archiver.ArchivedFileList(out filesInfo);
                    foreach (ArchiveFileInfo fileInfo in filesInfo)
                    {
                        candidateFiles.Add(fileInfo.FileName?.Trim().ToLower());
                    }
                    break;
                case AddonPackageSourceType.Folder:
                    string rootFolder = pSource.SourcePath.ToLower();
                    if (!rootFolder.EndsWith("\\"))
                        rootFolder += "\\";
                    // int rootFolderLength = rootFolder.Length + (rootFolder.EndsWith("\\") ? 0 : 1);
                    foreach (string fileName in Directory.EnumerateFiles(pSource.SourcePath, "*",
                        SearchOption.AllDirectories))
                    {
                        string file = fileName.ToLower().Replace(rootFolder, "");
                        candidateFiles.Add(file);
                    }

                    break;
            }

            List<string> puppetTextures = new List<string>();
            foreach (string filePath in candidateFiles)
            {
                if(filePath.StartsWith("data\\puppets\\") && !filePath.Contains("\\thumbs\\") && !filePath.Contains("\\collections\\"))
                {
                    string extension = Path.GetExtension(filePath);
                    if (!string.IsNullOrEmpty(extension) && _imageExtensionList.Contains(extension))
                    {
                        puppetTextures.Add(filePath.Replace("\\", "/"));
                    }
                }
            }

            puppetTextures.Sort();
            return puppetTextures;
        }



        public override string ToString()
        {
            if ((Puppets == null) || (Puppets?.Puppets.Count == 0))
                return "";

            StringBuilder textBuilder = new StringBuilder();
            foreach (BodyModelSumPuppet puppet in Puppets.Puppets)
            {
                textBuilder.AppendLine($"{puppet.PuppetName}:");
                if (puppet.BodyParts != null)
                {
                    textBuilder.AppendLine("    BodyParts:");
                    foreach (BodyModelSumBodyPart part in puppet.BodyParts)
                    {
                        string text = $"{part.BodyPartName} ({part.BodyPartType})";
                        if (part.Morphable)
                            text += " [MORPHABLE]";
                        textBuilder.AppendLine($"       {text}");
                    }
                }

                bool hasDecals = ((puppet.Decals != null) && (puppet.Decals.Count > 0));
                if (hasDecals || puppet.ExternDecalReferenced)
                {
                    if (hasDecals)
                    {
                        textBuilder.AppendLine("    Decals:");
                        foreach (BodyModelSumDecal decal in puppet.Decals)
                        {
                            textBuilder.AppendLine($"       {decal.DecalName} ({decal.Group})");
                        }
                        // if(puppet.ExternDecalReferenced)
                        //    textBuilder.AppendLine($"   Extern Decals Referenced");
                    }
                    else
                    {
                        textBuilder.AppendLine("    Decals: Only extern Decals referenced");
                    }
                }


                if ((puppet.Animations == null) && ((puppet.GestureAndGaitsAnimations + puppet.OtherAnimations) <= 0))
                    continue;
                if (LoadAnimations)
                {
                    if (puppet.Animations != null)
                    {
                        textBuilder.AppendLine("    Animations:");
                        foreach (string animation in puppet.Animations)
                        {
                            textBuilder.AppendLine($"       {animation}");
                        }
                    }
                }
                else
                {
                    if (!ListGestureGaitsAnimations)
                    {
                        int gestureAnimations = puppet.Animations?.Count ?? 0;
                        textBuilder.AppendLine(
                            $"    Animations: Gesture/Gaits: {gestureAnimations}   Other: {puppet.OtherAnimations} - See Verbs");
                        continue;
                    }

                    if (puppet.Animations != null)
                    {
                        textBuilder.AppendLine("    Animations:");
                        foreach (string animation in puppet.Animations)
                        {
                            textBuilder.AppendLine($"       {animation}");
                        }

                        if (puppet.OtherAnimations > 0)
                        {
                            textBuilder.AppendLine(
                                $"       Non-gesture/gaits animations: {puppet.OtherAnimations} - See Verbs");
                        }
                    }
                    else
                    {
                        if (puppet.OtherAnimations > 0)
                        {
                            textBuilder.AppendLine(
                                $"    Animations (non-gesture/gaits): {puppet.OtherAnimations} - See Verbs");
                        }
                    }
                }
            }
            return textBuilder.ToString();
        }
    }


    // -------------------------------------------------------------------------------------------------------------------------------------------------------------------

    [Serializable]
    public sealed class BodyModelSumPuppets
    {
        [XmlArrayItem("Puppet")]
        public List<BodyModelSumPuppet> Puppets { get; set; }

        [XmlIgnore]
        public SevenZipArchiver Archiver { get; private set; }


        // ------------------------------------------------------------------------------------

        public BodyModelSumPuppets()
        {

        }


        public BodyModelSumPuppets(SevenZipArchiver pArchiver)
        {
            Archiver = pArchiver;
        }



        public BodyModelSumPuppet SearchPuppet(string pName)
        {
            if (string.IsNullOrEmpty(pName = pName?.Trim().ToLower()))
            {
                return null;
            }

            if ((Puppets == null) || (Puppets.Count == 0))
                return null;

            foreach (BodyModelSumPuppet item in Puppets)
                if (item.PuppetName.Trim().ToLower() == pName)
                    return item;

            return null;
        }

        public bool AppendBodyModelItem(BodyModelItem pBodyModelItem, List<string> pPuppetTextures, bool pLoadAnimations, out string pErrorText)
        {
            // string puppetName = pBodyModelItem.PuppetName;
            BodyModelSumPuppet puppet = null;
            if (Puppets != null)
            {
                puppet = SearchPuppet(pBodyModelItem.PuppetName);
            }
            else
                Puppets = new List<BodyModelSumPuppet>();

            if (puppet == null)
            {
                puppet = new BodyModelSumPuppet(pBodyModelItem.PuppetName, Archiver);
                Puppets.Add(puppet);
            }

            return puppet.AppendData(pBodyModelItem, pPuppetTextures, pLoadAnimations, out pErrorText);
        }
    }


    // ---------------------------------------------------------------------------------------------------------------------

    [Serializable]
    public sealed class BodyModelSumPuppet
    {
        public string PuppetName { get; set; }

        [XmlArrayItem("BodyPart")]
        public List<BodyModelSumBodyPart> BodyParts { get; set; }

        [XmlArrayItem("Animation")]
        public List<string> Animations { get; set; }

        // public List<string> GestureAndGaitsVerbs { get; set; }

        public int GestureAndGaitsAnimations { get; set; } = 0;

        public int OtherAnimations { get; set; } = 0;

        [XmlArrayItem("Decal")]
        public List<BodyModelSumDecal> Decals { get; set; }

        public bool ExternDecalReferenced { get; set; }

        public string PuppetPath => $"Data/Puppets/{PuppetName}".ToLower();

        private string _RootPath;

        private int _RootPathLength;

        // private bool _initialData = false;

        private SevenZipArchiver _Archiver;




        // ---------------------------------------------------------------------------------

        public BodyModelSumPuppet()
        {

        }


        public BodyModelSumPuppet(string pName, SevenZipArchiver pArchiver)
        {
            _Archiver = pArchiver;
            PuppetName = pName?.Trim();
            _RootPath = $"data/puppets/{PuppetName.ToLower()}/";
            _RootPathLength = _RootPath.Length;
        }


        public bool AppendData(BodyModelItem pBodyModelItem, List<string> pAddonTextures, bool pLoadAnimations, out string pErrorText)
        {
            pErrorText = null;

            bool onlyGestures = !pLoadAnimations;
            if (!InsertAnimations(pBodyModelItem.Animations, onlyGestures, out pErrorText))
                return false;

            List<string> puppetTextures = new List<string>();
            foreach (string addonTextureFile in pAddonTextures)
            {
                if(addonTextureFile.StartsWith(PuppetPath))
                    puppetTextures.Add(addonTextureFile);
            }

            return InsertParts(pBodyModelItem.Parts, puppetTextures, out pErrorText);
        }



        private bool InsertAnimations(List<string> pAnimations, bool pOnlyGestures, out string pErrorText)
        {
            pErrorText = null;
            if ((pAnimations == null) || (pAnimations.Count == 0))
                return true;

            string prefix = "animations/";
            int animationsPrefixLen = prefix.Length;

            bool insertOk = false;
            try
            {
                foreach (string item in pAnimations)
                {
                    string animation = item.ToLower().StartsWith(prefix) ? item.Remove(0, animationsPrefixLen) : item;
                    int dotIndex = animation.LastIndexOf('.');

                    string animationsLower = animation.ToLower();
                    animation = animation.Remove(dotIndex);
                    if (animationsLower.StartsWith("gestures/") || animationsLower.StartsWith("gaits/"))
                    {
                        GestureAndGaitsAnimations++;
                    }
                    else
                    {
                        OtherAnimations++;
                        if(pOnlyGestures)
                            continue;
                    }
                    
                    if (Animations == null)
                        Animations = new List<string>();

                    if (!Animations.Contains(animation))
                        Animations.Add(animation);
                }

                insertOk = true;
            }
            catch (Exception exception)
            {
                pErrorText = $"BodyModelSumPuppet.InsertAnimations(), EXCEPTION: {exception.Message}";
            }

            return insertOk;
        }


        // ------------------------------------------------------------------------------------------------------------------------------------------------------------


        private bool InsertParts(List<BodyPart> pBodyParts, List<string> pPuppetTextures, out string pErrorText)
        {
            pErrorText = null;
            if ((pBodyParts == null) || (pBodyParts.Count == 0))
                return true;

            bool insertOk = false;
            
            try
            {
                List<BodyModelSumDecal> puppetDecals = null;
                bool externDecalsReferenced = false;
                foreach (BodyPart item in pBodyParts)
                {
                    BodyModelSumBodyPart part = new BodyModelSumBodyPart(_Archiver, PuppetName);

                    bool addNewPart;
                    List<BodyModelSumDecal> decals;
                    if (!part.ExtractData(item, pPuppetTextures, out addNewPart, out decals, out externDecalsReferenced, out pErrorText))
                    {
                        return false;
                    }

                    if (addNewPart)
                    {
                        if (BodyParts == null)
                            BodyParts = new List<BodyModelSumBodyPart>();

                        BodyParts.Add(part);
                    }

                    if (decals != null)
                    {
                        if (puppetDecals == null)
                            puppetDecals = decals;
                        else
                        {
                            foreach (BodyModelSumDecal decal in puppetDecals)
                            {
                                if(!DecalAlreadyListed(decal))
                                    puppetDecals.Add(decal);
                            }
                        }
                    }
                }

                if ((puppetDecals != null) && (puppetDecals.Count > 0))
                    Decals = puppetDecals.OrderBy(o => o.Group + o.DecalName).ToList();
                ExternDecalReferenced = externDecalsReferenced;

                insertOk = true;
            }
            catch (Exception exception)
            {
                pErrorText = $"BodyModelSumPuppet.InsertParts(), EXCEPTION: {exception.Message}";
            }

            return insertOk;
        }

        private bool DecalAlreadyListed(BodyModelSumDecal pDecal)
        {
            if ((pDecal == null) || (Decals == null))
                return true;

            foreach (BodyModelSumDecal listedDecal in Decals)
            {
                if (listedDecal.DecalName == pDecal.DecalName)
                    return true;
            }

            return false;
        }
    }

    // ---------------------------------------------------------------------------------------------------------------------

    [Serializable]
    public sealed class BodyModelSumBodyPart
    {
        private SevenZipArchiver _Archiver;

        public string BodyPartName { get; set; }

        public string Description { get; set; }


        public string FilePath { get; set; }

        public string BodyPartType { get; set; }

        public string Covers { get; set; }

        public bool HasMeshes { get; set; }

        public bool Morphable { get; set; }

        public List<string> Tags { get; set; }

        public string PuppetName { get; set; }


        // ------------------------------------------------------------------------------------------

        public BodyModelSumBodyPart()
        {

        }


        public BodyModelSumBodyPart(SevenZipArchiver pArchiver, string pPuppetName)
        {
            _Archiver = pArchiver;
            PuppetName = pPuppetName;
        }


        public bool ExtractData(BodyPart pItem, List<string> pPuppetTextures, out bool pAddNewPart, out List<BodyModelSumDecal> pDecals, out bool pExternDecalReferenced, out string pErrorText)
        {
            pAddNewPart = false;
            pErrorText = null;
            pDecals = null;
            pExternDecalReferenced = false;

            FilePath = pItem.DescriptionFilePath;
            BodyPartType = pItem.PartsCovered;
            // Tags = pItem.Tags;

            string fileContents = _Archiver.ExtractArchivedFileToString(FilePath.Replace("/", "\\"));
            if (fileContents == null)
            {
                pErrorText = _Archiver.LastErrorText;
                return false;
            }

            bool isBasicBodyPartFile = fileContents.Contains("<mscope.things.BodyPart>");

            // Populate
            // List<BodyModelSumDecal> tempDecals = null;
            bool fileProcessOk =
                isBasicBodyPartFile
                    ? ProcessBasicBodyPartFile(fileContents, out pErrorText)
                    : ProcessMorphBodyPartFile(fileContents, pPuppetTextures, out pDecals, out pExternDecalReferenced, out pErrorText);


            if (isBasicBodyPartFile)
                pAddNewPart = true;
            else
            {
                pAddNewPart = HasMeshes;
            }
            if (!fileProcessOk)
            {
                return false;
            }

            return true;
        }


        private bool ProcessBasicBodyPartFile(string pText, out string pErrorText)
        {
            AssetFiles.BodyPart bodyPart = AssetFiles.BodyPart.LoadFromString(pText, out pErrorText);
            if (bodyPart == null)
                return false;

            BodyPartName = bodyPart.name;
            Description = bodyPart.description;
            if ((bodyPart.tags != null) && (bodyPart.tags.Length > 0))
            {
                Tags = new List<string>();
                foreach(string tag in bodyPart.tags)
                    Tags.Add(tag);
            }

            return true;
        }


        private bool ProcessMorphBodyPartFile(string pText, List<string> pPuppetTextures, out List<BodyModelSumDecal> pDecals, out bool pExternDecalReferenced, out string pErrorText)
        {
            pDecals = null;
            pExternDecalReferenced = false;
            AssetFiles.BodyPartMorph bodyPart = AssetFiles.BodyPartMorph.LoadFromString(pText, out pErrorText);
            if (bodyPart == null)
                return false;

            BodyPartName = bodyPart.name;
            Description = bodyPart.description;
            HasMeshes = (bodyPart.meshes != null) && (bodyPart.meshes.Length > 0);
            if ((bodyPart.morphTargets != null) && (bodyPart.morphTargets.Length > 0))
                Morphable = true;

            if ((bodyPart.tags != null) && (bodyPart.tags.Length > 0))
            {
                Tags = new List<string>();
                foreach (string tag in bodyPart.tags)
                    Tags.Add(tag);
            }

            if ((bodyPart.decals != null) && (bodyPart.decals.Length > 0))
            {
                pDecals = new List<BodyModelSumDecal>();
                foreach (mscopethingsBodyPart_MorphPartMaterial decalItem in bodyPart.decals)
                {
                    if (!CheckDecalFileExists(decalItem.maps, pPuppetTextures))
                    {
                        pExternDecalReferenced = true;
                        continue;
                    }

                    BodyModelSumDecal decal = new BodyModelSumDecal();
                    decal.DecalName = decalItem.name.Trim();
                    decal.Group = GetDecalGroup(decalItem.parameters).Trim();
                    pDecals.Add(decal);
                }
            }

            if (!HasMeshes)
            {
                BodyPartType += " DECALS";
            }

            return true;
        }






        private bool CheckDecalFileExists(string[][] pDecalMaps, List<string> pPuppetTextures)
        {
            string imageFile = null;
            foreach (string[] map in pDecalMaps)
            {
                if (map[0]?.Trim().ToLower() == "diffusemap")
                {
                    imageFile = map[1]?.Trim().ToLower();
                    break;
                }
            }

            if (imageFile == null)
                return false;
            foreach (string puppetTextureFile in pPuppetTextures)
            {
                if (puppetTextureFile.EndsWith(imageFile))
                    return true;
            }

            return false;
        }


        private string GetDecalGroup(string[][] pDecalParameters)
        {
            if (pDecalParameters == null)
                return null;

            foreach (var entry in pDecalParameters)
            {
                if (entry.Length == 2)
                {
                    if (entry[0].ToLower().Contains("group"))
                        return entry[1];
                }
            }

            return null;
        }
    }

    // ---------------------------------------------------------------------------------------------------------------------

    [Serializable]
    public sealed class BodyModelSumDecal
    {
        public string DecalName { get; set; }

        public string Group { get; set; }

        // public string Path { get; set; }

    }

}
