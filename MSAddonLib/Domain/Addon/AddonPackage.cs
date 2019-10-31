using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;
using System.Xml.Serialization;
using MSAddonLib.Domain.AssetFiles;
using MSAddonLib.Util;
using MSAddonLib.Util.Persistence;
using SevenZip;


namespace MSAddonLib.Domain.Addon
{
    /// <summary>
    /// Detailed information about a addon package
    /// </summary>
    [Serializable]
    public sealed class AddonPackage
    {
        /// <summary>
        /// Name of the signature file of the addon. It contains information about:<br/>
        /// - An encrypted signature<br/>
        /// - Name of the addon<br/>
        /// - Name of the account of the published<br/>
        /// - Description of the addon<br/>
        /// - Flag indicating whether it's either a free addon or requires a license<br/>
        /// - A full list of every file inside addon - NOTE: only files in the list are recognized by Moviestorm at run-time.
        /// </summary>
        public const string SignatureFilename = ".Addon";

        /// <summary>
        /// Asset data archive. It contains:<br/>
        /// - the Manifest file<br/>
        /// - the description files for props and body parts (DESCRIPTOR, .template, .parts and .bodyparts) - NOTE: only files inside the Asset data archive are recognized by Moviestorm at run-time. Files in the Data folder are ignored (and can be deleted)
        /// </summary>
        public const string AssetDataFilename = "assetData.jar";

        /// <summary>
        /// Name of the Manifesto file (included in the asset data archive) 
        /// </summary>
        private const string ManifestFilename = "ASSET_DATA.MF";

        /// <summary>
        /// Name of the Version info file of the addon
        /// </summary>
        public const string VersionInfoFilename = "version.xml";

        /// <summary>
        /// Name of the properties file of the addon
        /// </summary>
        public const string PropertiesFilename = ".properties";

        /// <summary>
        /// Name of the unofficial notes file
        /// </summary>
        public const string NotesFilename = "notes.txt";

        /// <summary>
        /// Name of the mesh data file of the addon. It contains the meshes for all the props and body parts in the addon, in a compact (and possibly encrypted) format.<br/>
        /// NOTE: only meshes inside this file (and its associate index file) are recognized by Moviestorm at run-time. Cal3D mesh files (.cmf) in the Data folder are ignored (and can be deleted)
        /// </summary>
        public const string MeshDataFilename = "meshData.data";

        /// <summary>
        /// Name of the thumbnail image file of the addon
        /// </summary>
        public const string ThumbnailFilename = "thumbnail.jpg";

        private const double BytesPerMegabyte = 1024.0 * 1024.0;


        private const string ScenarioPrefix = "data\\scenario\\";
        private readonly int ScenarioPrefixLen = ScenarioPrefix.Length;

        private const string CurrentRegistryVersion = "1.7";


        // ------------------------------------------------------------------------------------------------------------------------------------

        public AddonPackageSource Source { get; set; }

        /// <summary>
        /// Name of the addon
        /// </summary>
        [XmlIgnore]
        public string Name => AddonSignature.Name;

        /// <summary>
        /// Friendly name of the addon
        /// </summary>
        public string FriendlyName
        {
            get { return string.IsNullOrEmpty(_friendlyName) ? AddonSignature.Name : _friendlyName; }
            set { _friendlyName = value; }
        }
        private string _friendlyName;

        /// <summary>
        /// Name of the account of the published
        /// </summary>
        [XmlIgnore]
        public string Publisher => !string.IsNullOrEmpty(Notes?.OriginalPublisher) ? Notes.OriginalPublisher : AddonSignature.Publisher;


        /// <summary>
        /// Name of the republisher
        /// </summary>
        [XmlIgnore]
        public string RepublishedBy => !string.IsNullOrEmpty(Notes?.OriginalPublisher) ? AddonSignature.Publisher : null;


        /// <summary>
        /// Qualified name of the addon
        /// </summary>
        [XmlIgnore]
        public string QualifiedName => $"{Publisher}.{Name}";

        /// <summary>
        /// Presentation format of the addon
        /// </summary>
        public AddonPackageFormat AddonFormat { get; set; } = AddonPackageFormat.Unknown;


        /// <summary>
        /// The addon is free, not requiring a license for its use in Moviestorm
        /// </summary>
        [XmlIgnore]
        public bool Free => AddonSignature.Free;

        /// <summary>
        /// Description of the addon
        /// </summary>
        [XmlIgnore]
        public string Description => AddonSignature.Description;

        /// <summary>
        /// Descriptive blurb for the addon
        /// </summary>
        public string Blurb { get; set; }

        /// <summary>
        /// Revision number of the addon
        /// </summary>
        public string Revision { get; set; }

        /// <summary>
        /// Path to the source file/folder
        /// </summary>
        [XmlIgnore]
        public string Location => Source.ArchivedPath ?? Source.SourcePath;

        /// <summary>
        /// Datetime of last modification of signature file
        /// </summary>
        public DateTime? LastCompiled { get; set; }

        /// <summary>
        /// Can be recompiled
        /// </summary>
        [XmlIgnore]
        public bool Recompilable => (MeshDataSizeMbytes == null) || (MeshDataSizeMbytes == 0.0) || HasCal3DMeshFiles;

        /// <summary>
        /// Addon signature file of the addon
        /// </summary>
        public AddonSignatureFile AddonSignature { get; set; }

        /// <summary>
        /// Asset manifest contents
        /// </summary>
        public AssetManifest AssetManifest { get; set; }


        /// <summary>
        /// Unofficial notes file
        /// </summary>
        public AddonNotes Notes { get; set; }


        /// <summary>
        /// Size of the mesh data file in megabytes
        /// </summary>
        public double? MeshDataSizeMbytes { get; set; }

        /// <summary>
        /// The addon contains Cal3D mesh files (.cmf)
        /// </summary>
        public bool HasCal3DMeshFiles { get; set; }

        /// <summary>
        /// The addon contains a signature file (required)
        /// </summary>
        public bool HasSignatureFile { get; set; }

        /// <summary>
        /// The addon contains an asset data archive (required)
        /// </summary>
        public bool HasAssetDataArchive { get; set; }

        /// <summary>
        /// The addon has a data folder (required)
        /// </summary>
        public bool HasDataFolder { get; set; }

        /// <summary>
        /// Summary information about the files in the addon
        /// </summary>
        public AddonFileSummaryInfo FileSummaryInfo { get; set; }

        /// <summary>
        /// List of demo (starter) movies included
        /// </summary>
        [XmlArrayItem("Movie")]
        public List<string> DemoMovies { get; set; }

        /// <summary>
        /// List of stock assets
        /// </summary>
        [XmlArrayItem("Stock")]
        public List<string> StockAssets { get; set; }

        /// <summary>
        /// The addon has a thumbnail image file
        /// </summary>
        public bool HasThumbnail { get; set; }

        /// <summary>
        /// The addon has a machine state file
        /// </summary>
        public bool HasStateMachine { get; set; }

        /// <summary>
        /// The addon has a verb definition file
        /// </summary>
        public bool HasVerbs { get; set; }

        public AddonAssetSummary AssetSummary { get; set; }

        /// <summary>
        /// Summary info about body models
        /// </summary>
        public BodyModelsSummary BodyModelsSummary { get; set; }

        /// <summary>
        /// Summary info about prop models 
        /// </summary>
        public PropModelsSummary PropModelsSummary { get; set; }

        /// <summary>
        /// Summary info about animations
        /// </summary>
        public VerbsSummary VerbsSummary { get; set; }


        public bool HasCuttingRoomAssets { get; set; }

        public CuttingRoomAssetsSummary CuttingRoomAssetsSummary { get; set; }

        /// <summary>
        /// Sound files in the addon
        /// </summary>
        [XmlArrayItem("SoundFile")]
        public List<string> Sounds { get; set; }


        /// <summary>
        /// Special effects in the addon
        /// </summary>       
        [XmlArrayItem("SpecialEffect")]
        public List<string> SpecialEffects { get; set; }

        /// <summary>
        /// Materials in the addon
        /// </summary>
        [XmlArrayItem("Material")]
        public List<string> Materials { get; set; }

        /// <summary>
        /// Sky textures in the addon
        /// </summary>
        [XmlArrayItem("Sky")]
        public List<string> SkyTextures { get; set; }

        [XmlArrayItem("Asset")]
        public List<string> OtherAssets { get; set; }


        /// <summary>
        /// Fingerprint of the addon
        /// </summary>
        public string FingerPrint { get; set; }


        public DateTime? GeneratedDateTime { get; set; }

        public string RegistryVersion { get; set; } = CurrentRegistryVersion;

        // --------------------------

        /// <summary>
        /// The addon has some issue
        /// </summary>
        [XmlIgnore]
        public bool HasIssues => !string.IsNullOrEmpty(Issues);

        /// <summary>
        /// Text of the issues
        /// </summary>
        public string Issues { get; set; }


        private readonly StringBuilder _issuesStringBuilder = new StringBuilder();

        // ------------------------------

        /// <summary>
        /// Info about the files included in the addon archive
        /// </summary>
        private readonly List<ArchiveFileInfo> _addonFileList = null;

        private string ReportText { get; set; }

        /// <summary>
        /// Forces listing of all animation files (.caf) in the addon.<br/>
        /// By default, it only lists the name of the animation files for the gestures and gaits, for they are only defined in the StateMachine file and determining what puppets they apply to is a complex task.
        /// </summary>
        private bool ListAllAnimationFiles { get; set; }

        private bool ListGestureGaitsAnimationFiles { get; set; }

        private bool ListWeirdGestureGaitVerbs { get; set; }

        private bool _verbSummaryPopulationOk = false;


        // -----------------------------------------------------------------------------------------------

        /// <summary>
        /// Constructor required for serialization
        /// </summary>
        public AddonPackage()
        {

        }


        /// <summary>
        /// Full information about the addon
        /// </summary>
        /// <param name="pArchiver">Archiver for accessing the addon archive contents</param>
        /// <param name="pProcessingFlags">Processing flags</param>
        /// <param name="pTemporaryFolder">Path to the root temporary folder</param>
        /// <param name="pArchivedPath">Path of archived file</param>
        public AddonPackage(SevenZipArchiver pArchiver, ProcessingFlags pProcessingFlags, string pTemporaryFolder = null, string pArchivedPath = null)
        {
            if (pArchiver == null)
            {
                throw new Exception("Not a valid archiver");
            }

            if (pArchiver.ArchivedFileList(out _addonFileList) < 0)
            {
                throw new Exception($"Error extracting the list of archived files: {pArchiver.LastErrorText}");
            }

            Source = new AddonPackageSource(pArchiver, pArchivedPath);

            AddonFormat = AddonPackageFormat.PackageFile;

            LoadAddonPackage(pProcessingFlags, pTemporaryFolder);
        }


        /// <summary>
        /// Full information about the addon
        /// </summary>
        /// <param name="pPath">Path of the folder/file containing the addon</param>
        /// <param name="pProcessingFlags">Processing flags</param>
        /// <param name="pTemporaryFolder">Path to the root temporary folder</param>
        public AddonPackage(string pPath, ProcessingFlags pProcessingFlags, string pTemporaryFolder = null)
        {
            if (string.IsNullOrEmpty(pPath = pPath.Trim()))
            {
                throw new Exception("Not a valid path specification");
            }

            string path = (Path.IsPathRooted(pPath)) ? pPath : Path.GetFullPath(pPath);
            if (path.ToLower().EndsWith(".addon"))
            {
                if (!File.Exists(path))
                {
                    throw new Exception("File not found");
                }
                SevenZipArchiver archiver = new SevenZipArchiver(path);
                if (archiver.ArchivedFileList(out _addonFileList) < 0)
                {
                    throw new Exception($"Error extracting the list of archived files: {archiver.LastErrorText}");
                }
                Source = new AddonPackageSource(archiver);
                AddonFormat = AddonPackageFormat.PackageFile;
            }
            else
            {
                if (!Directory.Exists(path))
                {
                    throw new Exception("Folder not found");
                }
                Source = new AddonPackageSource(path);
                AddonFormat = AddonPackageFormat.InstalledFolder;
            }

            LoadAddonPackage(pProcessingFlags, pTemporaryFolder);
        }


        // ----------------------------------------------------------------------------------------------------------------------------------------------------


        private bool LoadAddonPackage(ProcessingFlags pProcessingFlags, string pTemporaryFolder)
        {
            if (Source.SourceType == AddonPackageSourceType.Invalid)
                throw new Exception("Invalid source type for the addon");

            // Validate and check temporary folder (mandatory)
            if (string.IsNullOrEmpty(pTemporaryFolder = pTemporaryFolder?.Trim()))
                pTemporaryFolder = Utils.GetTempDirectory();
            else
                pTemporaryFolder = (Path.IsPathRooted(pTemporaryFolder)) ? pTemporaryFolder : Path.GetFullPath(pTemporaryFolder);

            if (string.IsNullOrEmpty(pTemporaryFolder = pTemporaryFolder.Trim()) || !Directory.Exists(pTemporaryFolder))
                throw new Exception("No temporary folder found");

            // Location = string.IsNullOrEmpty(Source.ArchivedPath) ? Source.SourcePath : Source.ArchivedPath;

            // bool isFolderAddon = Source.SourceType == AddonPackageSourceType.Folder;

            ListAllAnimationFiles = pProcessingFlags.HasFlag(ProcessingFlags.ListAllAnimationFiles);
            ListGestureGaitsAnimationFiles = pProcessingFlags.HasFlag(ProcessingFlags.ListGestureGaitsAnimations);
            ListWeirdGestureGaitVerbs = pProcessingFlags.HasFlag(ProcessingFlags.ListWeirdGestureGaitsVerbs);

            CheckContentsInFileResult contentsSummary =
                Source.SourceType == AddonPackageSourceType.Folder
                    ? CheckContentsInFileList(Source.SourcePath)
                    : CheckContentsInFileList(_addonFileList);

            if (!contentsSummary.HasAddonSignatureFile)
            {
                throw new Exception("No Addon Signature file (.AddOn)");
            }
            if (!contentsSummary.HasAssetDataFile)
            {
                throw new Exception("No Addon AssetData file (assetData.jar)");
            }

            FileSummaryInfo = contentsSummary.FileSummaryInfo;

            LastCompiled = GetLastCompiled();


            HasVerbs = contentsSummary.HasVerbs;

            HasCuttingRoomAssets = contentsSummary.HasCuttingRoomAssets;

            try
            {
                RetrieveAddonSignatureInfo();
            }
            catch (Exception exception)
            {
                _issuesStringBuilder.AppendLine(exception.Message);
            }

            try
            {
                RetrieveAssetManifest(pTemporaryFolder);
            }
            catch (Exception exception)
            {
                _issuesStringBuilder.AppendLine(exception.Message);
            }

            RetrieveVersionInfo();
            RetrievePropertiesInfo();
            if (contentsSummary.HasNotesFile)
                RetrieveNotesFile();


            MeshDataSizeMbytes = GetMeshDataSize();

            HasCal3DMeshFiles = contentsSummary.HasCal3DMeshFiles;

            HasDataFolder = contentsSummary.HasDataContents;
            DemoMovies = contentsSummary.DemoMovies;
            StockAssets = contentsSummary.StockAssets;

            HasThumbnail = contentsSummary.HasThumbnail;

            HasStateMachine = contentsSummary.HasStateMachine;

            string errorText;
            if (HasVerbs || HasStateMachine)
            {
                VerbsSummary = new VerbsSummary(Source, pProcessingFlags.HasFlag(ProcessingFlags.ListCompactDupVerbsByName));

                if (!(_verbSummaryPopulationOk = VerbsSummary.PopulateSummary(out errorText)))
                {
                    _issuesStringBuilder.AppendLine($"VerbsSummary: {errorText}");
                }
            }

            if (HasCuttingRoomAssets)
            {
                CuttingRoomAssetsSummary = new CuttingRoomAssetsSummary(Source);
                if (!CuttingRoomAssetsSummary.PopulateSummary(out errorText))
                {
                    _issuesStringBuilder.AppendLine($"CuttingRoomAssetsSummary: {errorText}");
                }
            }

            Sounds = GetSounds(contentsSummary.SoundFiles);

            // Filters = GetFilters(contentsSummary.FilterFiles);

            SpecialEffects = GetSpecialEffects(contentsSummary.SpecialEffects);

            Materials = GetMaterials(Source, contentsSummary.MaterialsFiles);

            SkyTextures = GetSkies(contentsSummary.SkyFiles);

            OtherAssets = contentsSummary.OtherAssets;

            Issues = _issuesStringBuilder.ToString()?.Trim();

            UpdateAssetSummary();

            FingerPrint = CalculateFingerPrint(contentsSummary.FingerPrintData);

            GeneratedDateTime = DateTime.Now;

            return true;
        }




        public void UpdateAssetSummary()
        {
            AssetSummary = new AddonAssetSummary();

            if ((BodyModelsSummary?.Puppets?.Puppets?.Count ?? 0) > 0)
            {
                foreach (BodyModelSumPuppet puppet in BodyModelsSummary.Puppets.Puppets)
                {
                    AssetSummary.Bodyparts += puppet.BodyParts?.Count ?? 0;
                    AssetSummary.Decals += puppet.Decals?.Count ?? 0;
                }
            }

            AssetSummary.Animations = UpdateAnimationsCount();

            if (PropModelsSummary?.Props?.Props != null)
            { 
                AssetSummary.Props = PropModelsSummary.Props.Props.Count;
                int variants = 0;
                foreach (PropModelSumProp prop in PropModelsSummary.Props.Props)
                {
                    if (prop.Variants == null)
                        variants++;
                    else
                        variants += prop.Variants.Count;
                }
                AssetSummary.PropVariants = variants;
            }

            AssetSummary.Verbs =
                (VerbsSummary?.Verbs?.Gaits?.Count ?? 0)
                + (VerbsSummary?.Verbs?.Gestures?.Count ?? 0)
                + (VerbsSummary?.Verbs?.PuppetSoloVerbs?.Count ?? 0)
                + (VerbsSummary?.Verbs?.PuppetMutualVerbs?.Count ?? 0)
                + (VerbsSummary?.Verbs?.PropSoloVerbs?.Count ?? 0)
                + (VerbsSummary?.Verbs?.HeldPropsVerbs?.Count ?? 0)
                + (VerbsSummary?.Verbs?.InteractivePropsVerbs?.Count ?? 0);

            AssetSummary.CuttingRoomAssets =
                (CuttingRoomAssetsSummary?.Assets?.Filters?.Count ?? 0)
                + (CuttingRoomAssetsSummary?.Assets?.TextStyles?.Count ?? 0)
                + (CuttingRoomAssetsSummary?.Assets?.Transitions?.Count ?? 0);
            
            AssetSummary.Sounds = Sounds?.Count ?? 0;
            AssetSummary.SpecialEffects = SpecialEffects?.Count ?? 0;
            AssetSummary.Materials = Materials?.Count ?? 0;
            AssetSummary.SkyTextures = SkyTextures?.Count ?? 0;
            AssetSummary.OtherAssets = OtherAssets?.Count ?? 0;

            AssetSummary.Stocks = StockAssets?.Count ?? 0;
            AssetSummary.StartMovies = DemoMovies?.Count ?? 0;
        }

        private int UpdateAnimationsCount()
        {
            if (AssetManifest == null)
                return 0;

            int count = 0;
            if ((AssetManifest.BodyModels != null) && (AssetManifest.BodyModels.Count > 0))
            {
                foreach (BodyModelItem puppet in AssetManifest.BodyModels)
                {
                    count += puppet.Animations?.Count ?? 0;
                }
            }
            if ((AssetManifest.PropModels != null) && (AssetManifest.PropModels.Count > 0))
            {
                foreach (PropModelItem prop in AssetManifest.PropModels)
                {
                    count += prop.Animations?.Count ?? 0;
                }
            }

            return count;
        }


        private DateTime? GetLastCompiled()
        {
            if (Source.SourceType == AddonPackageSourceType.Archiver)
            {
                ArchiveFileInfo? archivedFileInfo = Source.Archiver.GetFileInfo(SignatureFilename);
                return archivedFileInfo?.LastWriteTime;
            }

            try
            {
                FileInfo fileInfo = new FileInfo(Path.Combine(Source.SourcePath, SignatureFilename));
                return fileInfo?.LastWriteTime;
            }
            catch (Exception exception)
            {
                _issuesStringBuilder.AppendLine(
                    $"ERROR while getting datetime of last compilation: {exception.Message}");
            }

            return null;
        }



        private CheckContentsInFileResult CheckContentsInFileList(List<ArchiveFileInfo> pFileList)
        {
            CheckContentsInFileResult result = new CheckContentsInFileResult();

            result.FileSummaryInfo = new AddonFileSummaryInfo();
            result.FileSummaryInfo.TotalFiles = pFileList.Count;
            result.FingerPrintData = new List<string>();

            foreach (ArchiveFileInfo item in pFileList)
            {
                string filename = item.FileName.ToLower();
                
                if (filename == ".addon")
                {
                    result.HasAddonSignatureFile = true;
                    result.FileSummaryInfo.SignatureFile = new AddonFileInfo()
                    {
                        LastModified = item.LastWriteTime,
                        Size = item.Size
                    };
                    continue;
                }

                if (filename == "assetdata.jar")
                {
                    result.HasAssetDataFile = true;
                    result.FileSummaryInfo.ManifestArchive = new AddonFileInfo()
                    {
                        LastModified = item.LastWriteTime,
                        Size = item.Size
                    };
                    result.FingerPrintData.Add($"{filename}:{item.Size}:{item.LastWriteTime:s}^");
                    continue;
                }

                if (filename == "version.xml")
                {
                    result.HasVersionFile = true;
                    continue;
                }

                if (filename == ".properties")
                {
                    result.HasPropertiesFile = true;
                    continue;
                }

                if (filename == "notes.txt")
                {
                    result.HasNotesFile = true;
                    continue;
                }

                if (filename == "thumbnail.jpg")
                {
                    result.HasThumbnail = true;
                    continue;
                }

                if (filename == "meshdata.data")
                {
                    result.FingerPrintData.Add($"{filename}:{item.Size}:{item.LastWriteTime:s}^");
                    continue;
                }

                if (!result.HasDataContents && filename.StartsWith("data\\") && !filename.EndsWith("data\\"))
                    result.HasDataContents = true;

                if (filename.StartsWith("data\\") && !item.IsDirectory)
                {
                    result.FingerPrintData.Add($"{filename}:{item.Size}:{item.LastWriteTime:s}^");
                }

                if (filename.EndsWith("verbs"))
                {
                    result.HasVerbs = true;
                    result.FileSummaryInfo.VerbFile = new AddonFileInfo()
                    {
                        LastModified = item.LastWriteTime,
                        Size = item.Size
                    };
                    continue;
                }
                if (filename.EndsWith("statemachine"))
                {
                    result.HasStateMachine = true;
                    result.FileSummaryInfo.StateMachineFile = new AddonFileInfo()
                    {
                        LastModified = item.LastWriteTime,
                        Size = item.Size
                    };
                    continue;
                }

                if (filename.StartsWith(@"movies\"))
                {
                    string[] parts = item.FileName.Split("\\".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

                    if (parts.Length > 1)
                    {
                        string movieName = parts[1];
                        if (!string.IsNullOrEmpty(movieName))
                        {
                            if (result.DemoMovies == null)
                                result.DemoMovies = new List<string>();
                            else
                            {
                                if (result.DemoMovies.Contains(movieName))
                                    continue;
                            }

                            result.DemoMovies.Add(movieName);
                        }
                    }
                    continue;
                }

                if (filename.StartsWith(@"stock\") && filename.EndsWith(@"\object.xml"))
                {
                    string[] parts = item.FileName.Split("\\".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length > 1)
                    {
                        string stockText = Source.Archiver.ExtractArchivedFileToString(item.FileName)?.ToLower();
                        string stockSubtype = "";
                        if (stockText?.StartsWith("<set>") ?? false)
                            stockSubtype = "Set:";
                        else if (stockText?.StartsWith("<character>") ?? false)
                            stockSubtype = "Character:";
                        string stockName = $"{stockSubtype}{parts[1]}";
                        if (!string.IsNullOrEmpty(stockName))
                        {
                            if (result.StockAssets == null)
                                result.StockAssets = new List<string>();
                            else
                            {
                                if (result.StockAssets.Contains(stockName))
                                    continue;
                            }

                            result.StockAssets.Add(stockName);
                        }
                    }
                    continue;
                }

                if (filename.EndsWith(".cmf"))
                {
                    result.HasCal3DMeshFiles = true;
                    continue;
                }

                if (filename.StartsWith("data\\sound\\") && !item.IsDirectory)
                {
                    if (result.SoundFiles == null)
                        result.SoundFiles = new List<string>();
                    result.SoundFiles.Add(item.FileName);
                    continue;
                }

                if (!result.HasCuttingRoomAssets && filename.StartsWith("data\\cuttingroom") && filename.EndsWith("object.dat"))
                {
                    result.HasCuttingRoomAssets = true;
                    continue;
                }

                if (filename.StartsWith("data\\sky\\") && !item.IsDirectory && !filename.Contains("_preview"))
                {
                    if (result.SkyFiles == null)
                        result.SkyFiles = new List<string>();
                    result.SkyFiles.Add(item.FileName);
                    continue;
                }

                if (filename.EndsWith("materials.materials"))
                {
                    if (result.MaterialsFiles == null)
                        result.MaterialsFiles = new List<string>();
                    result.MaterialsFiles.Add(item.FileName);
                    continue;
                }

                if (filename.EndsWith(".mps"))
                {
                    if (result.SpecialEffects == null)
                        result.SpecialEffects = new List<string>();
                    result.SpecialEffects.Add(item.FileName);
                }

                if (filename.StartsWith("data\\terrain_masks") && filename.EndsWith(".png"))
                {
                    if(result.OtherAssets == null)
                        result.OtherAssets = new List<string>();
                    result.OtherAssets.Add("TerrainMask:" + Path.GetFileNameWithoutExtension(item.FileName));
                }

                if (filename.StartsWith("data\\scenario\\"))
                {
                    string scenarioName = GetScenarioName(item.FileName);

                    if (result.OtherAssets == null)
                        result.OtherAssets = new List<string>();
                    if (!result.OtherAssets.Contains(scenarioName))
                        result.OtherAssets.Add(scenarioName);
                }

            }

            return result;
        }


        private CheckContentsInFileResult CheckContentsInFileList(string pRootPath)
        {
            CheckContentsInFileResult result = new CheckContentsInFileResult();
            result.FileSummaryInfo = new AddonFileSummaryInfo();
            result.FingerPrintData = new List<string>();

            string prefix = pRootPath.ToLower();
            if (!prefix.EndsWith("\\"))
                prefix += "\\";
            int prefixLen = prefix.Length;

            List<string> addonFiles = Directory.EnumerateFiles(pRootPath, "*", SearchOption.AllDirectories).ToList();
            result.FileSummaryInfo.TotalFiles = addonFiles.Count;

            foreach (string fileName in addonFiles)
            {
                string relativePath = fileName;
                string filenameLower = fileName.ToLower();
                ulong fileSize;
                DateTime fileLastWriteTime;
                if (filenameLower.StartsWith(prefix))
                {
                    filenameLower = filenameLower.Remove(0, prefixLen);
                    relativePath = relativePath.Remove(0, prefixLen);
                }


                if (filenameLower == ".addon")
                {
                    result.HasAddonSignatureFile = true;
                    result.FileSummaryInfo.SignatureFile = GetFileSummaryInfo(pRootPath, fileName);
                    continue;
                }

                if (filenameLower == "assetdata.jar")
                {
                    result.HasAssetDataFile = true;
                    result.FileSummaryInfo.ManifestArchive = GetFileSummaryInfo(pRootPath, fileName);
                    GetFileData(fileName, out fileSize, out fileLastWriteTime);
                    result.FingerPrintData.Add($"{filenameLower}:{fileSize}:{fileLastWriteTime:s}^");
                    continue;
                }

                if (filenameLower == "version.xml")
                {
                    result.HasVersionFile = true;
                    continue;
                }

                if (filenameLower == ".properties")
                {
                    result.HasPropertiesFile = true;
                    continue;
                }

                if (filenameLower == "thumbnail.jpg")
                {
                    result.HasThumbnail = true;
                    continue;
                }

                if (filenameLower == "notes.txt")
                {
                    result.HasNotesFile = true;
                    continue;
                }

                if (filenameLower == "meshdata.data")
                {
                    GetFileData(fileName, out fileSize, out fileLastWriteTime);
                    result.FingerPrintData.Add($"{filenameLower}:{fileSize}:{fileLastWriteTime:s}^");
                    continue;
                }

                if (!result.HasDataContents && filenameLower.StartsWith("data\\") && !filenameLower.EndsWith("data\\"))
                    result.HasDataContents = true;


                if (filenameLower.StartsWith("data\\"))
                {
                    GetFileData(fileName, out fileSize, out fileLastWriteTime);
                    result.FingerPrintData.Add($"{filenameLower}:{fileSize}:{fileLastWriteTime:s}^");
                }

                if (filenameLower.EndsWith("verbs"))
                {
                    result.HasVerbs = true;
                    result.FileSummaryInfo.VerbFile = GetFileSummaryInfo(pRootPath, fileName);
                    continue;
                }
                if (filenameLower.EndsWith("statemachine"))
                {
                    result.HasStateMachine = true;
                    result.FileSummaryInfo.StateMachineFile = GetFileSummaryInfo(pRootPath, fileName);
                    continue;
                }

                /*
                if (filename.Contains("\\props\\") && filename.EndsWith(".caf"))
                {
                    if (result.PropAnimations == null)
                        result.PropAnimations = new List<string>();
                    result.PropAnimations.Add(item.FileName);
                    continue;
                }
                */

                if (filenameLower.StartsWith(@"movies\"))
                {
                    string[] parts = relativePath.Split("\\".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

                    if (parts.Length > 1)
                    {
                        string movieName = parts[1];
                        if (!string.IsNullOrEmpty(movieName))
                        {
                            if (result.DemoMovies == null)
                                result.DemoMovies = new List<string>();
                            else
                            {
                                if (result.DemoMovies.Contains(movieName))
                                    continue;
                            }

                            result.DemoMovies.Add(movieName);
                        }
                    }
                    continue;
                }

                if (filenameLower.StartsWith(@"stock\") && filenameLower.EndsWith(@"\object.xml"))
                {
                    string[] parts = relativePath.Split("\\".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length > 1)
                    {
                        string stockText = File.ReadAllText(Path.Combine(pRootPath, filenameLower))?.ToLower();
                        string stockSubtype = "";
                        if (stockText?.StartsWith("<set>") ?? false)
                            stockSubtype = "Set:";
                        else if (stockText?.StartsWith("<character>") ?? false)
                            stockSubtype = "Character:";
                        string stockName = $"{stockSubtype}{parts[1]}";
                        if (!string.IsNullOrEmpty(stockName))
                        {
                            if (result.StockAssets == null)
                                result.StockAssets = new List<string>();
                            else
                            {
                                if (result.StockAssets.Contains(stockName))
                                    continue;
                            }

                            result.StockAssets.Add(stockName);
                        }
                    }
                    continue;
                }

                if (filenameLower.EndsWith(".cmf"))
                {
                    result.HasCal3DMeshFiles = true;
                    continue;
                }

                if (filenameLower.StartsWith("data\\sound\\") /* && !fileName.IsDirectory */)
                {
                    if (result.SoundFiles == null)
                        result.SoundFiles = new List<string>();
                    result.SoundFiles.Add(relativePath);
                    continue;
                }

                if (!result.HasCuttingRoomAssets && filenameLower.StartsWith("data\\cuttingroom") && filenameLower.EndsWith("object.dat"))
                {
                    result.HasCuttingRoomAssets = true;
                    continue;
                }

                if (filenameLower.StartsWith("data\\sky\\") && /* !fileName.IsDirectory && */ !filenameLower.Contains("_preview"))
                {
                    if (result.SkyFiles == null)
                        result.SkyFiles = new List<string>();
                    result.SkyFiles.Add(relativePath);
                    continue;
                }

                if (filenameLower.EndsWith("materials.materials"))
                {
                    if (result.MaterialsFiles == null)
                        result.MaterialsFiles = new List<string>();
                    result.MaterialsFiles.Add(relativePath);
                    continue;
                }

                if (filenameLower.EndsWith(".mps"))
                {
                    if (result.SpecialEffects == null)
                        result.SpecialEffects = new List<string>();
                    result.SpecialEffects.Add(relativePath);
                }

                if (filenameLower.StartsWith("data\\terrain_masks") && filenameLower.EndsWith(".png"))
                {
                    if (result.OtherAssets == null)
                        result.OtherAssets = new List<string>();
                    result.OtherAssets.Add("TerrainMask:" + Path.GetFileNameWithoutExtension(relativePath));
                }

                if (filenameLower.StartsWith("data\\scenario\\"))
                {
                    string scenarioName = GetScenarioName(relativePath);

                    if (result.OtherAssets == null)
                        result.OtherAssets = new List<string>();
                    if(!result.OtherAssets.Contains(scenarioName))
                        result.OtherAssets.Add(scenarioName);
                }
            }

            return result;
        }

        private void GetFileData(string pFileName, out ulong pFileSize, out DateTime pFileLastWriteTime)
        {
            FileInfo info = new FileInfo(pFileName);
            pFileSize = (ulong) info.Length;
            pFileLastWriteTime = info.LastWriteTime;
        }


        private string CalculateFingerPrint(List<string> pFingerPrintData)
        {
            pFingerPrintData.Sort();
            
            StringBuilder builder = new StringBuilder();
            foreach (string line in pFingerPrintData)
            {
                builder.Append(line);
            }

            string fingerprintText = builder.ToString();
            byte[] bytes = Encoding.UTF8.GetBytes(fingerprintText);

            SHA256 sha256 = SHA256.Create();
            byte[] hash = sha256.ComputeHash(bytes);

            return Utils.HexaBinString(hash);
        }
        

        private string GetScenarioName(string pPath)
        {
            string name = pPath.Remove(0, ScenarioPrefixLen);
            int index = name.IndexOf("\\");

            if(index >= 0)
                name = name.Remove(index);

            return $"Scenario:{name}";
        }


        private AddonFileInfo GetFileSummaryInfo(string pRootPath, string pFileName)
        {
            AddonFileInfo summaryFileInfo = null;
            try
            {
                FileInfo info = new FileInfo(Path.Combine(pRootPath, pFileName));
                summaryFileInfo = new AddonFileInfo()
                {
                    LastModified = info.LastWriteTime,
                    Size = (ulong)info.Length
                };
            }
            catch { }

            return summaryFileInfo;
        }


        private string GetArchivedFileFullName(string pSearch)
        {
            if (string.IsNullOrEmpty(pSearch = pSearch?.Trim().ToLower()))
                return null;

            if (Source.SourceType == AddonPackageSourceType.Folder)
            {
                string fileFullName = Path.Combine(Source.SourcePath, pSearch);
                return File.Exists(fileFullName) ? fileFullName : null;
            }

            foreach (ArchiveFileInfo item in _addonFileList)
            {
                string filename = item.FileName.ToLower();
                if (filename == pSearch)
                    return item.FileName;
            }

            return null;
        }


        // ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Retrieves addon basic info from the signature file 
        /// </summary>
        private void RetrieveAddonSignatureInfo()
        {
            string addonSignatureFilename = null;
            byte[] addonFileContents = null;
            string errorText = null;
            switch (Source.SourceType)
            {
                case AddonPackageSourceType.Archiver:
                    addonSignatureFilename = GetArchivedFileFullName(SignatureFilename);
                    addonFileContents = Source.Archiver.ExtractArchivedFileToByte(addonSignatureFilename);
                    if (addonFileContents == null)
                        errorText = Source.Archiver.LastErrorText;
                    break;
                case AddonPackageSourceType.Folder:
                    addonSignatureFilename = Path.Combine(Source.SourcePath, SignatureFilename);
                    addonFileContents = File.ReadAllBytes(addonSignatureFilename);
                    break;
            }
            if (addonFileContents == null)
            {
                throw new Exception($"EXCEPTION recovering archived contents of Signature file: {errorText}");
            }

            AddonSignature = AddonSignatureFile.Load(addonFileContents);
            HasSignatureFile = true;
        }

        // --------------------------------------------------------------------------------------------------------------------------------------------------------------------


        /// <summary>
        /// Retrieves the asset manifest from the file inside the assetData.jar class library
        /// </summary>
        /// <param name="pTemporaryFolder">Path to temporary folder</param>
        private void RetrieveAssetManifest(string pTemporaryFolder)
        {
            // Previously, extract the Manifest file into the temp file
            string assetDataFile = null;
            bool deleteTempAssetDataFile = false;
            switch (Source.SourceType)
            {
                case AddonPackageSourceType.Archiver:
                    string assetDataArchivedFile = GetArchivedFileFullName(AssetDataFilename);
                    List<string> filesToExtract = new List<string>() { assetDataArchivedFile };
                    if (Source.Archiver.ArchivedFilesExtract(pTemporaryFolder, filesToExtract) < 0)
                    {
                        throw new Exception($"EXCEPTION extracting Manifest file {Source.Archiver.LastErrorText}");
                    }

                    assetDataFile = Path.Combine(pTemporaryFolder, AssetDataFilename);
                    deleteTempAssetDataFile = true;
                    break;
                case AddonPackageSourceType.Folder:
                    assetDataFile = Path.Combine(Source.SourcePath, AssetDataFilename);
                    break;
            }
            if (!File.Exists(assetDataFile))
                throw new Exception($"No {AssetDataFilename} file");


            SevenZipArchiver assetDataArchiver = new SevenZipArchiver(assetDataFile);

            string manifestContents = assetDataArchiver.ExtractArchivedFileToString(ManifestFilename, true);

            if (deleteTempAssetDataFile)
                File.Delete(assetDataFile);
            if (string.IsNullOrEmpty(manifestContents))
            {
                throw new Exception(
                    $"ERROR extracting {ManifestFilename} contents: {assetDataArchiver.LastErrorText}");
            }


            string errorText;
            AssetManifest assetManifest = AssetManifest.LoadFromString(manifestContents, out errorText);
            if (assetManifest == null)
                throw new Exception(errorText);

            BodyModelsSummary = new BodyModelsSummary(Source, pTemporaryFolder, assetManifest.BodyModels, ListAllAnimationFiles, ListGestureGaitsAnimationFiles);
            if (!BodyModelsSummary.PopulateSummary(out errorText))
            {
                throw new Exception(errorText);
            }

            bool loadAllAnimationFiles = ListAllAnimationFiles || !HasVerbs;
            PropModelsSummary = new PropModelsSummary(Source, pTemporaryFolder, assetManifest.PropModels, loadAllAnimationFiles);
            if (!PropModelsSummary.PopulateSummary(out errorText))
            {
                throw new Exception(errorText);
            }

            AssetManifest = assetManifest;
            HasAssetDataArchive = true;
        }

        // -------------------------------------------------------------------------------------------------------------------------------


        private void RetrieveVersionInfo()
        {
            string versionFileFullName;
            string versionFileContents = null;

            switch (Source.SourceType)
            {
                case AddonPackageSourceType.Archiver:
                    versionFileFullName = GetArchivedFileFullName(VersionInfoFilename);
                    if (string.IsNullOrEmpty(versionFileFullName))
                        return;

                    versionFileContents = Source.Archiver.ExtractArchivedFileToString(versionFileFullName);
                    break;
                case AddonPackageSourceType.Folder:
                    versionFileFullName = Path.Combine(Source.SourcePath, VersionInfoFilename);
                    if (!File.Exists(versionFileFullName))
                        return;
                    versionFileContents = File.ReadAllText(versionFileFullName);
                    break;
            }

            if (string.IsNullOrEmpty((versionFileContents = versionFileContents?.Trim())))
                return;

            string errorText;
            AddonVersionInfo addonVersionInfo = AddonVersionInfo.LoadFromString(versionFileContents, out errorText);
            if (addonVersionInfo != null)
                Revision = addonVersionInfo.Revision;
            else
            {
                if (errorText != null)
                    _issuesStringBuilder.AppendLine(errorText);
            }
        }

        // --------------------------------------------------------------------------------------------------------------------------------------------------------

        private void RetrievePropertiesInfo()
        {
            string propertiesFileFullName;
            string propertiesFileContents = null;

            switch (Source.SourceType)
            {
                case AddonPackageSourceType.Archiver:
                    propertiesFileFullName = GetArchivedFileFullName(PropertiesFilename);
                    if (string.IsNullOrEmpty(propertiesFileFullName))
                        return;

                    propertiesFileContents = Source.Archiver.ExtractArchivedFileToString(propertiesFileFullName);
                    break;
                case AddonPackageSourceType.Folder:
                    propertiesFileFullName = Path.Combine(Source.SourcePath, PropertiesFilename);
                    if (!File.Exists(propertiesFileFullName))
                        return;
                    propertiesFileContents = File.ReadAllText(propertiesFileFullName);
                    break;
            }

            if (string.IsNullOrEmpty((propertiesFileContents = propertiesFileContents?.Trim())))
                return;

            string errorText;
            AddonPropertiesInfo addonPropertiesInfo = AddonPropertiesInfo.LoadFromString(propertiesFileContents, out errorText);
            if (addonPropertiesInfo != null)
            {
                _friendlyName = addonPropertiesInfo.Name;
                Blurb = addonPropertiesInfo.Blurb;
            }
            else
            {
                _friendlyName = Name;
            }
        }

        // ----------------------------------------------------------------------------------------------------

        private void RetrieveNotesFile()
        {
            string notesFullFilename;
            string notesFileContents = null;

            switch (Source.SourceType)
            {
                case AddonPackageSourceType.Archiver:
                    notesFullFilename = GetArchivedFileFullName(NotesFilename);
                    if (string.IsNullOrEmpty(notesFullFilename))
                        return;

                    notesFileContents = Source.Archiver.ExtractArchivedFileToString(notesFullFilename);
                    break;
                case AddonPackageSourceType.Folder:
                    notesFullFilename = Path.Combine(Source.SourcePath, NotesFilename);
                    if (!File.Exists(notesFullFilename))
                        return;
                    notesFileContents = File.ReadAllText(notesFullFilename);
                    break;
            }

            if (string.IsNullOrEmpty((notesFileContents = notesFileContents?.Trim())))
                return;

            string errorText;
            Notes = AddonNotes.LoadFromString(notesFileContents, out errorText);
        }


        // ----------------------------------------------------------------------------------------------------

        private List<string> GetMaterials(AddonPackageSource pSource, List<string> pMaterialsFiles)
        {
            if ((pMaterialsFiles == null) || (pMaterialsFiles.Count == 0))
                return null;

            return pSource.SourceType == AddonPackageSourceType.Archiver
                ? GetMaterialsArchiver(pSource.Archiver, pMaterialsFiles)
                : GetMaterialsFolder(pSource.SourcePath, pMaterialsFiles);
        }

        private List<string> GetMaterialsArchiver(SevenZipArchiver pArchiver, List<string> pMaterialsFiles)
        {
            List<string> materials = new List<string>();
            foreach (string item in pMaterialsFiles)
            {
                string folder = Path.GetDirectoryName(item).Replace("Data\\Materials\\", "");

                string materialContents = pArchiver.ExtractArchivedFileToString(item);

                string errorText;
                Materials materialsVector = AssetFiles.Materials.LoadFromString(materialContents, out errorText);
                if (materialsVector == null)
                {
                    throw new Exception(errorText);
                }

                foreach (var material in materialsVector.material)
                {
                    materials.Add(Path.Combine(folder, material.name).Replace("\\", "/"));
                }
            }

            return materials;
        }

        private List<string> GetMaterialsFolder(string pFolderPath, List<string> pMaterialsFiles)
        {
            List<string> materials = new List<string>();

            foreach (string item in pMaterialsFiles)
            {
                string folder = Path.GetDirectoryName(item).Replace("Data\\Materials\\", "");

                string materialContents = File.ReadAllText(Path.Combine(pFolderPath, item));
                string errorText;
                Materials materialsVector = AssetFiles.Materials.LoadFromString(materialContents, out errorText);
                if (materialsVector == null)
                {
                    throw new Exception(errorText);
                }

                foreach (var material in materialsVector.material)
                {
                    materials.Add(Path.Combine(folder, material.name).Replace("\\", "/"));
                }

            }

            return materials;
        }

        // --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------


        private List<string> GetSkies(List<string> pSkyFiles)
        {
            if ((pSkyFiles == null) || (pSkyFiles.Count == 0))
                return null;

            List<string> skies = new List<string>();
            try
            {
                foreach (string item in pSkyFiles)
                {
                    skies.Add(item.Remove(0, "Data\\Sky\\".Length));
                }
            }
            catch (Exception exception)
            {
                _issuesStringBuilder.AppendLine($"Error while getting Sky Textures: {exception.Message}");
            }
            return skies;
        }


        private List<string> GetSounds(List<string> pSoundFiles)
        {
            if ((pSoundFiles == null) || (pSoundFiles.Count == 0))
                return null;

            List<string> sounds = new List<string>();
            try
            {
                foreach (string item in pSoundFiles)
                {
                    sounds.Add(item.Remove(0, "Data\\Sound\\".Length).Replace("\\", "/"));
                }
            }
            catch (Exception exception)
            {
                _issuesStringBuilder.AppendLine($"Error while getting Sounds: {exception.Message}");
            }

            return sounds;
        }

        /*
        private List<string> GetFilters(List<string> pFilterFiles)
        {
            if ((pFilterFiles == null) || (pFilterFiles.Count == 0))
                return null;

            List<string> filters = new List<string>();
            try
            {
                foreach (string item in pFilterFiles)
                {
                    filters.Add(item.Remove(0, "Data/CuttingRoom/Filters/".Length).Replace("\\", "/"));
                }
            }
            catch (Exception exception)
            {
                _issuesStringBuilder.AppendLine($"Error while getting Filters: {exception.Message}");
            }

            return filters;
        }
        */


        private List<string> GetSpecialEffects(List<string> pSpecialEffects)
        {
            if ((pSpecialEffects == null) || (pSpecialEffects.Count == 0))
                return null;

            List<string> specialEffects = new List<string>();
            try
            {
                foreach (string item in pSpecialEffects)
                {
                    specialEffects.Add(item.Replace("Data\\", "").Replace("\\", "/").Replace(".mps", ""));
                }
            }
            catch (Exception exception)
            {
                _issuesStringBuilder.AppendLine($"Error while getting SFX: {exception.Message}");
            }
            return specialEffects;
        }

        // --------------------------------------------------------------------------------------------------------


        private double? GetMeshDataSize()
        {
            try
            {
                switch (Source.SourceType)
                {
                    case AddonPackageSourceType.Archiver:
                        foreach (ArchiveFileInfo file in _addonFileList)
                        {
                            if (file.FileName.ToLower() == MeshDataFilename.ToLower())
                            {
                                return file.Size / BytesPerMegabyte;
                            }
                        }
                        break;
                    case AddonPackageSourceType.Folder:
                        string meshDataFile = Path.Combine(Source.SourcePath, MeshDataFilename);
                        if (File.Exists(meshDataFile))
                            return new FileInfo(meshDataFile).Length / BytesPerMegabyte;
                        break;
                }
            }
            catch (Exception exception)
            {
                _issuesStringBuilder.AppendLine($"Error while getting Mesh Data Size: {exception.Message}");
            }

            return null;
        }


        // -----------------------------------------------------------------------------------------------------

        public override string ToString()
        {
            /*
            if (ReportText != null)
                return ReportText;
                */

            StringBuilder summary = new StringBuilder();
            summary.AppendLine(string.Format("    Name: {0}", Name));
            if (HasIssues)
            {
                summary.AppendLine("   !Has Problems - refer to the end of the report...");
            }

            // summary.AppendLine(string.Format("FriendlyName: {0}", _friendlyName));
            summary.AppendLine(string.Format($"    Publisher: {Publisher}"));
            if(!string.IsNullOrEmpty(RepublishedBy))
                summary.AppendLine(string.Format($"    Republished by: {RepublishedBy}"));
            summary.AppendLine(string.Format($"    Free: {Free}"));
            if (LastCompiled.HasValue)
                summary.AppendLine(string.Format($"    Last compiled: {LastCompiled.Value:u}"));
            if (FileSummaryInfo != null)
                summary.AppendLine($"    Total files: {FileSummaryInfo.TotalFiles}");
            if (!string.IsNullOrEmpty(Description))
            {
                bool firstLine = true;
                foreach (string line in Description.Split("\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries))
                {

                    if (!string.IsNullOrEmpty(line?.Trim()))
                    {
                        if (firstLine)
                        {
                            summary.AppendLine($"    Description: {line}");
                            firstLine = false;
                        }
                        else
                        {
                            summary.AppendLine($"                 {line}");
                        }
                    }
                }
            }

            if (!string.IsNullOrEmpty(Blurb?.Trim()) &&
                (string.IsNullOrEmpty(Description?.Trim()) || (Blurb?.Trim() != Description?.Trim())))
            {
                bool firstLine = true;
                foreach (string line in Blurb.Split("\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries))
                {

                    if (!string.IsNullOrEmpty(line?.Trim()))
                    {
                        if (firstLine)
                        {
                            summary.AppendLine($"    Blurb: {line}");
                            firstLine = false;
                        }
                        else
                        {
                            summary.AppendLine($"           {line}");
                        }
                    }
                }
            }
            if (!string.IsNullOrEmpty(Revision))
                summary.AppendLine(string.Format("    Revision: {0}", Revision));

            if (!string.IsNullOrEmpty(FingerPrint))
                summary.AppendLine($"    * Fingerprint: {FingerPrint}");
            
            // summary.AppendLine(string.Format("Path: {0}", _addonPath));
            if (MeshDataSizeMbytes.HasValue && (MeshDataSizeMbytes.Value > 0))
            {
                summary.AppendLine(string.Format("    Mesh Data File size (MB): {0:#.###}", MeshDataSizeMbytes));
                if (!HasCal3DMeshFiles)
                    summary.AppendLine("       NOTE: Addon can't be republished (no Cal3D mesh files found)!");
            }
            else
                summary.AppendLine("    No meshes");
            if (HasThumbnail)
                summary.AppendLine("    Has Thumbnail image");
            if ((DemoMovies != null) && (DemoMovies.Count > 0))
            {
                summary.AppendLine("    * Includes Demo Movies:");
                foreach (string item in DemoMovies)
                    summary.AppendLine($"         '{item}'");
            }

            if ((StockAssets != null) && (StockAssets.Count > 0))
            {
                summary.AppendLine("    * Includes Stock assets:");
                foreach (string item in StockAssets)
                    summary.AppendLine($"         '{item}'");
            }

            if (BodyModelsSummary != null)
            {
                string bodyModelsSummaryText = BodyModelsSummary.ToString();
                if (!string.IsNullOrEmpty(bodyModelsSummaryText))
                {
                    summary.AppendLine("    Body parts:");
                    foreach (string line in bodyModelsSummaryText.Split("\n".ToCharArray()))
                    {
                        if (!string.IsNullOrEmpty(line.Trim()))
                            summary.AppendLine($"      {line}");
                    }
                }
            }

            if (PropModelsSummary != null)
            {
                string propModelsSummaryText = PropModelsSummary.ToString();
                if (!string.IsNullOrEmpty(propModelsSummaryText))
                {
                    summary.AppendLine("    Prop models:");
                    foreach (string line in propModelsSummaryText.Split("\n".ToCharArray()))
                    {
                        if (!string.IsNullOrEmpty(line.Trim()))
                            summary.AppendLine($"      {line}");
                    }
                }
            }

            if (HasVerbs || HasStateMachine)
            {
                if (HasVerbs && HasStateMachine)
                    summary.AppendLine("    Has StateMachine and Verbs files");
                else if (HasStateMachine)
                    summary.AppendLine("    Has StateMachine file");
                else
                    summary.AppendLine("    Has Verbs file");

                bool printHasVerbs = true;
                if ((VerbsSummary != null) && VerbsSummary.Verbs.HasData)
                {
                    string verbsSummaryText = VerbsSummary.WriteReport(ListWeirdGestureGaitVerbs);
                    if (!string.IsNullOrEmpty(verbsSummaryText))
                    {
                        summary.AppendLine("    Verbs:");
                        foreach (string line in verbsSummaryText.Split("\n".ToCharArray()))
                        {
                            if (!string.IsNullOrEmpty(line.Trim()))
                                summary.AppendLine($"      {line}");
                        }
                        printHasVerbs = false;
                    }
                }
                if (printHasVerbs && !_verbSummaryPopulationOk)
                    summary.AppendLine("        OOPS! Something went wrong while creating the verb list :(");
            }

            AppendMiscList(summary, Sounds, "Sounds");

            if (HasCuttingRoomAssets && CuttingRoomAssetsSummary.HasData)
            {
                string text = CuttingRoomAssetsSummary.ToString();
                if (!string.IsNullOrEmpty(text))
                {
                    summary.AppendLine("    Cutting Room Assets:");
                    foreach (string line in text.Split("\n".ToCharArray()))
                    {
                        if (!string.IsNullOrEmpty(line.Trim()))
                            summary.AppendLine($"      {line}");
                    }
                }
            }
            // AppendMiscList(summary, Filters, "Filters");

            AppendMiscList(summary, SpecialEffects, "Special Effects");

            AppendMiscList(summary, Materials, "Materials");

            AppendMiscList(summary, SkyTextures, "Sky Textures");

            AppendMiscList(summary, OtherAssets, "Other Assets");

            if (HasIssues)
            {
                summary.AppendLine("    !PROBLEMS:");
                foreach (string line in Issues.Split("\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries))
                    summary.AppendLine($"        {line}");
            }

            return (ReportText = summary.ToString());
        }


        private void AppendMiscList(StringBuilder pSummary, List<string> pMiscList, string pHeader)
        {
            if ((pMiscList == null) || (pMiscList.Count == 0))
                return;

            pSummary.AppendLine($"    {pHeader}:");
            foreach (string item in pMiscList)
                pSummary.AppendLine($"      {item}");

        }



    }

    /// <summary>
    /// Presentation format of the addon
    /// </summary>
    public enum AddonPackageFormat
    {
        Unknown,
        PackageFile,
        InstalledFolder
    }


    public class AddonAssetSummary
    {
        public int Bodyparts { get; set; }

        public int Decals { get; set; }

        public int Props { get; set; }

        public int PropVariants { get; set; }

        public int Animations { get; set; }

        public int Verbs { get; set; }

        public int Sounds { get; set; }

        public int CuttingRoomAssets { get; set; }

        public int SpecialEffects { get; set; }

        public int Materials { get; set; }

        public int SkyTextures { get; set; }

        public int OtherAssets { get; set; }

        public int Stocks { get; set; }

        public int StartMovies { get; set; }
    }


    public sealed class AddonFileSummaryInfo
    {
        public int TotalFiles { get; set; }

        public AddonFileInfo SignatureFile { get; set; }

        public AddonFileInfo ManifestArchive { get; set; }

        public AddonFileInfo StateMachineFile { get; set; }

        public AddonFileInfo VerbFile { get; set; }
    }


    public sealed class AddonFileInfo
    {
        public ulong Size { get; set; }

        public DateTime LastModified { get; set; }
    }


    sealed class CheckContentsInFileResult
    {
        public bool HasAddonSignatureFile { get; set; }

        public bool HasAssetDataFile { get; set; }

        public bool HasVersionFile { get; set; }

        public bool HasPropertiesFile { get; set; }

        public bool HasThumbnail { get; set; }

        public bool HasNotesFile { get; set; }

        public List<string> DemoMovies { get; set; }

        public List<string> StockAssets { get; set; }

        public bool HasDataContents { get; set; }

        public bool HasStateMachine { get; set; }

        public bool HasVerbs { get; set; }

        public bool HasCuttingRoomAssets { get; set; }

        public bool HasCal3DMeshFiles { get; set; }

        public List<string> MaterialsFiles { get; set; }

        public List<string> SkyFiles { get; set; }
        // public List<string> PropAnimations { get; set; }
        public List<string> SoundFiles { get; set; }

        // public List<string> FilterFiles { get; set; }

        public List<string> SpecialEffects { get; set; }

        public List<string> OtherAssets { get; set; }

        public AddonFileSummaryInfo FileSummaryInfo { get; set; }

        public List<string> FingerPrintData { get; set; }
    }

}
