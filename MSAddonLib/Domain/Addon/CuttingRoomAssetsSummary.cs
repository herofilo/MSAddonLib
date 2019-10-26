using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using MSAddonLib.Domain.AssetFiles;
using MSAddonLib.Persistence.AddonDB;
using MSAddonLib.Util.Persistence;
using SevenZip;

namespace MSAddonLib.Domain.Addon
{
    public sealed class CuttingRoomAssetsSummary
    {
        public const string CuttingRoomAssetsPath = "data\\cuttingroom";

        public const string AssetInfoFilename = "asset.info";

        public const string ObjectDatFilename = "object.dat";


        public CuttingRoomAssetCollection Assets { get; set; }

        [XmlIgnore]
        public bool HasData => Assets?.HasData ?? false;

        [XmlIgnore]
        public AddonPackageSource AddonSource { get; private set; }

        private bool _initialized = false;


        // ------------------------------------------------------------------------------------------------------

        public CuttingRoomAssetsSummary()
        {

        }

        public CuttingRoomAssetsSummary(AddonPackageSource pSource)
        {
            AddonSource = pSource;
        }


        // ---------------------------------------------------------------------------------------------------------


        public bool PopulateSummary(out string pErrorText, AddonPackageSource pSource = null)
        {
            pErrorText = null;
            if (_initialized)
                return true;

            if (!PopulateSummaryPreChecks(ref pSource, out pErrorText))
                return false;

            bool isOk = true;
            try
            {
                Assets = AddonSource.SourceType == AddonPackageSourceType.Folder 
                    ? CreateAssetsSummary(AddonSource.SourcePath, out pErrorText)
                    : CreateAssetsSummary(AddonSource.Archiver, out pErrorText);

            }
            catch (Exception exception)
            {
                pErrorText = $"CuttingRoomAssetsSummary.PopulateSummary() EXCEPTION: {exception.Message}";
                isOk = false;
            }


            _initialized = isOk;

            return isOk;
        }




        private bool PopulateSummaryPreChecks(ref AddonPackageSource pSource, out string pErrorText)
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

            return true;
        }


        private CuttingRoomAssetCollection CreateAssetsSummary(string pRootPath, out string pErrorText)
        {
            pErrorText = null;

            string assetFolder = Path.Combine(pRootPath, CuttingRoomAssetsPath);
            if (!Directory.Exists(assetFolder))
                return null;

            string pathPrefix = $"{pRootPath.ToLower()}\\{CuttingRoomAssetCollection.CuttingRoomAssetsPathPrefix}";
            CuttingRoomAssetCollection assets = new CuttingRoomAssetCollection();
            foreach (string file in Directory.EnumerateFiles(assetFolder, ObjectDatFilename, SearchOption.AllDirectories))
            {
                if (!assets.AppendItem(pathPrefix, file, out pErrorText))
                {
                    return null;
                }
            }

            return assets.HasData ? assets : null;
        }


        private CuttingRoomAssetCollection CreateAssetsSummary(SevenZipArchiver pArchiver, out string pErrorText)
        {
            pErrorText = null;

            CuttingRoomAssetCollection assets = new CuttingRoomAssetCollection();

            List<ArchiveFileInfo> files;
            pArchiver.ArchivedFileList(out files);

            foreach (ArchiveFileInfo file in files)
            {
                string fileNameLower = file.FileName.ToLower();
                if (fileNameLower.StartsWith(CuttingRoomAssetsPath) && fileNameLower.EndsWith(ObjectDatFilename))
                {
                    if (!assets.AppendItem(pArchiver, file.FileName, out pErrorText))
                        return null;
                }
            }
            
            return assets.HasData ? assets : null;
        }


        public override string ToString()
        {
            if ((Assets == null) || (!Assets.HasData))
                return "";

            StringBuilder textBuilder = new StringBuilder();
            // textBuilder.AppendLine("Cutting Room Assets:");

            CuttingRoomAssetCollectionFlags flags = Assets.Flags;
            if (flags.HasFlag(CuttingRoomAssetCollectionFlags.Filters))
                BuildAssetListText(textBuilder, "Filters", Assets.Filters);

            if (flags.HasFlag(CuttingRoomAssetCollectionFlags.TextStyles))
                BuildAssetListText(textBuilder, "Text Styles", Assets.TextStyles);

            if (flags.HasFlag(CuttingRoomAssetCollectionFlags.Transitions))
                BuildAssetListText(textBuilder, "Transitions", Assets.Transitions);

            return textBuilder.ToString();
        }


        private void BuildAssetListText(StringBuilder pBuilder, string pHeader, List<CuttingRoomAssetItem> pList)
        {
            pBuilder.AppendLine($"{pHeader}:");
            foreach (CuttingRoomAssetItem item in pList)
            {
                pBuilder.AppendLine(string.IsNullOrEmpty(item.Description) ? $"  {item.Name}" : $"  {item.Name}  :  {item.Description}");
                if(!string.IsNullOrEmpty(item.TagsRaw))
                    pBuilder.AppendLine($"     Tags: {item.TagsRaw}");
            }
        }


    }


    public sealed class CuttingRoomAssetCollection
    {
        public static readonly string CuttingRoomAssetsPathPrefix = CuttingRoomAssetsSummary.CuttingRoomAssetsPath + "\\";

        public static readonly string ObjectDatFilenameSuffix = "\\" + CuttingRoomAssetsSummary.ObjectDatFilename;



        [XmlArrayItem("Filter")]
        public List<CuttingRoomAssetItem> Filters { get; set; }

        [XmlArrayItem("TextStyle")]
        public List<CuttingRoomAssetItem> TextStyles { get; set; }

        [XmlArrayItem("Transition")]
        public List<CuttingRoomAssetItem> Transitions { get; set; }

        public CuttingRoomAssetCollectionFlags Flags { get; set; } =
            CuttingRoomAssetCollectionFlags.None;

        [XmlIgnore]
        public bool HasData => Flags != CuttingRoomAssetCollectionFlags.None;

        // --------------------------------------------------------------------------------------------------------------------


        public bool AppendItem(string pPathPrefix, string pFile, out string pErrorText)
        {
            string directory = Path.GetDirectoryName(pFile);
            string folder = directory?.ToLower().Replace(pPathPrefix, "");

            string fileText = null;
            string name = null;
            string assetInfoFile = Path.Combine(directory, CuttingRoomAssetsSummary.AssetInfoFilename);
            if (File.Exists(assetInfoFile))
                fileText = File.ReadAllText(assetInfoFile);
            else
                name = GetAssetName(directory);

            return DoAppendItem(folder, fileText, name, out pErrorText);
        }

        

        public bool AppendItem(SevenZipArchiver pArchiver, string pFile, out string pErrorText)
        {
            string directory = Path.GetDirectoryName(pFile);
            string folder = pFile.ToLower().Replace(CuttingRoomAssetsPathPrefix, "").Replace(ObjectDatFilenameSuffix, "");

            string fileText = null;
            string name = null;
            string assetInfoFile = Path.Combine(directory, CuttingRoomAssetsSummary.AssetInfoFilename);
            if(pArchiver.FileExists(assetInfoFile))
                fileText = pArchiver.ExtractArchivedFileToString(assetInfoFile);
            else
                name = GetAssetName(directory);

            return DoAppendItem(folder, fileText, name, out pErrorText);
        }

        private string GetAssetName(string pDirectory)
        {
            int index = pDirectory.LastIndexOf("\\", StringComparison.InvariantCulture);
            return pDirectory.Substring(index + 1);
        }


        private bool DoAppendItem(string pFolder, string pFileText, string pName, out string pErrorText)
        {
            pErrorText = null;
            CuttingRoomAssetItem asset = null;
            if (pFileText == null)
            {
                asset = new CuttingRoomAssetItem()
                {
                    AssetType = AddonAssetType.CuttingRoomAsset,
                    Name = pName
                };
            }
            else
            {
                AssetInfo assetInfo = AssetInfo.LoadFromString(pFileText, out pErrorText);
                if (assetInfo == null)
                    return false;

                asset = new CuttingRoomAssetItem()
                {
                    AssetType = AddonAssetType.CuttingRoomAsset,
                    Name = assetInfo.Name,
                    Description = assetInfo.Description
                };


                if (!string.IsNullOrEmpty(assetInfo.TagList?.Trim()))
                {
                    asset.TagsRaw = assetInfo.TagList;
                    string[] tags = assetInfo.TagList.Split(",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                    if (tags.Length > 0)
                        asset.Tags = tags.ToList();
                }
            }

            if (pFolder.StartsWith("filters"))
            {
                asset.AssetSubtype = "Filter";
                if (Filters == null)
                {
                    Filters = new List<CuttingRoomAssetItem>();
                    Flags |= CuttingRoomAssetCollectionFlags.Filters;
                }
                Filters.Add(asset);
                
            } else if (pFolder.StartsWith("textstyles"))
            {
                asset.AssetSubtype = "TextStyle";
                if (TextStyles == null)
                {
                    TextStyles = new List<CuttingRoomAssetItem>();
                    Flags |= CuttingRoomAssetCollectionFlags.TextStyles;
                }
                TextStyles.Add(asset);
            }
            else if (pFolder.StartsWith("transitions"))
            {
                asset.AssetSubtype = "Transition";
                if (Transitions == null)
                {
                    Transitions = new List<CuttingRoomAssetItem>();
                    Flags |= CuttingRoomAssetCollectionFlags.Transitions;
                }
                Transitions.Add(asset);
            }
            return true;
        }


    }

    [Flags]
    public enum CuttingRoomAssetCollectionFlags
    {
        None = 0,
        Filters = 0x01,
        TextStyles = 0x02,
        Transitions = 0x04
    }


    public sealed class CuttingRoomAssetItem
    {
        public AddonAssetType AssetType { get; set; }

        public string AssetSubtype { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public List<string> Tags { get; set; }

        public string TagsRaw { get; set; }
    }

}
