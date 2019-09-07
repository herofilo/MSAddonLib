using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
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
        /// Name of the mesh data file of the addon. It contains the meshes for all the props and body parts in the addon, in a compact (and possibly encrypted) format.<br/>
        /// NOTE: only meshes inside this file (and its associate index file) are recognized by Moviestorm at run-time. Cal3D mesh files (.cmf) in the Data folder are ignored (and can be deleted)
        /// </summary>
        public const string MeshDataFilename = "meshData.data";

        /// <summary>
        /// Name of the thumbnail image file of the addon
        /// </summary>
        public const string ThumbnailFilename = "thumbnail.jpg";

        private const double BytesPerMegabyte = 1024.0 * 1024.0;

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
        public string Publisher => AddonSignature.Publisher;

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
        /// Datetime of last modification of signations file
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

        /// <summary>
        /// Sound files in the addon
        /// </summary>
        [XmlArrayItem("SoundFile")]
        public List<string> Sounds { get; set; }

        /// <summary>
        /// Filters in the addon
        /// </summary>
        [XmlArrayItem("Filter")]
        public List<string> Filters { get; set; }

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

        // --------------------------

        /// <summary>
        /// The addon has some issue
        /// </summary>
        [XmlIgnore]
        public bool HasIssues => !string.IsNullOrEmpty(_issuesStringBuilder.ToString());

        /// <summary>
        /// Text of the issues
        /// </summary>
        [XmlIgnore]
        public string Issues { get { return _issuesStringBuilder.ToString(); } }


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

            LastCompiled = GetLastCompiled();


            HasVerbs = contentsSummary.HasVerbs;

            RetrieveAddonSignatureInfo();

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

            MeshDataSizeMbytes = GetMeshDataSize();

            HasCal3DMeshFiles = contentsSummary.HasCal3DMeshFiles;

            HasDataFolder = contentsSummary.HasDataContents;
            DemoMovies = contentsSummary.DemoMovies;
            StockAssets = contentsSummary.StockAssets;

            HasThumbnail = contentsSummary.HasThumbnail;

            HasStateMachine = contentsSummary.HasStateMachine;

            if (HasVerbs || HasStateMachine)
            {
                string errorText;
                VerbsSummary = new VerbsSummary(Source, pProcessingFlags.HasFlag(ProcessingFlags.ListCompactDupVerbsByName));

                if (!(_verbSummaryPopulationOk = VerbsSummary.PopulateSummary(out errorText)))
                {
                    _issuesStringBuilder.AppendLine($"VerbsSummary: {errorText}");
                }
            }

            Sounds = GetSounds(contentsSummary.SoundFiles);

            Filters = GetFilters(contentsSummary.FilterFiles);

            SpecialEffects = GetSpecialEffects(contentsSummary.SpecialEffects);

            Materials = GetMaterials(Source, contentsSummary.MaterialsFiles);

            SkyTextures = GetSkies(contentsSummary.SkyFiles);

            return true;
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
            catch
            {

            }

            return null;
        }



        private CheckContentsInFileResult CheckContentsInFileList(List<ArchiveFileInfo> pFileList)
        {
            CheckContentsInFileResult result = new CheckContentsInFileResult();

            foreach (ArchiveFileInfo item in pFileList)
            {
                string filename = item.FileName.ToLower();

                if (filename == ".addon")
                {
                    result.HasAddonSignatureFile = true;
                    continue;
                }

                if (filename == "assetdata.jar")
                {
                    result.HasAssetDataFile = true;
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

                if (filename == "thumbnail.jpg")
                {
                    result.HasThumbnail = true;
                    continue;
                }

                if (!result.HasDataContents && filename.StartsWith("data\\") && !filename.EndsWith("data\\"))
                    result.HasDataContents = true;

                if (filename.EndsWith("verbs"))
                {
                    result.HasVerbs = true;
                    continue;
                }
                if (filename.EndsWith("statemachine"))
                {
                    result.HasStateMachine = true;
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

                if (filename.StartsWith(@"stock\"))
                {
                    string[] parts = item.FileName.Split("\\".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length > 1)
                    {
                        string stockName = parts[1];
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

                if (filename.StartsWith("data\\cuttingroom\\filters\\") && !item.IsDirectory)
                {
                    string path = Path.GetDirectoryName(item.FileName);
                    if (result.FilterFiles == null)
                        result.FilterFiles = new List<string>();
                    if (!result.FilterFiles.Contains(path))
                        result.FilterFiles.Add(path);
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

            }

            return result;
        }


        private CheckContentsInFileResult CheckContentsInFileList(string pRootPath)
        {
            CheckContentsInFileResult result = new CheckContentsInFileResult();

            string prefix = pRootPath.ToLower();
            if (!prefix.EndsWith("\\"))
                prefix += "\\";
            int prefixLen = prefix.Length;

            foreach (string fileName in Directory.EnumerateFiles(pRootPath, "*", SearchOption.AllDirectories))
            {
                string relativePath = fileName;
                string filenameLower = fileName.ToLower();
                if (filenameLower.StartsWith(prefix))
                {
                    filenameLower = filenameLower.Remove(0, prefixLen);
                    relativePath = relativePath.Remove(0, prefixLen);
                }


                if (filenameLower == ".addon")
                {
                    result.HasAddonSignatureFile = true;
                    continue;
                }

                if (filenameLower == "assetdata.jar")
                {
                    result.HasAssetDataFile = true;
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

                if (!result.HasDataContents && filenameLower.StartsWith("data\\") && !filenameLower.EndsWith("data\\"))
                    result.HasDataContents = true;

                if (filenameLower.EndsWith("verbs"))
                {
                    result.HasVerbs = true;
                    continue;
                }
                if (filenameLower.EndsWith("statemachine"))
                {
                    result.HasStateMachine = true;
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

                if (filenameLower.StartsWith(@"stock\"))
                {
                    string[] parts = relativePath.Split("\\".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length > 1)
                    {
                        string stockName = parts[1];
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

                if (filenameLower.StartsWith("data\\cuttingroom\\filters\\") /* && !fileName.IsDirectory */)
                {
                    string path = Path.GetDirectoryName(relativePath);
                    if (result.FilterFiles == null)
                        result.FilterFiles = new List<string>();
                    if (!result.FilterFiles.Contains(path))
                        result.FilterFiles.Add(path);
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

            }

            return result;
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
                    if(addonFileContents == null)
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

            if(deleteTempAssetDataFile)
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
            foreach (string item in pSkyFiles)
            {
                skies.Add(item.Remove(0, "Data\\Sky\\".Length));
            }

            return skies;
        }


        private List<string> GetSounds(List<string> pSoundFiles)
        {
            if ((pSoundFiles == null) || (pSoundFiles.Count == 0))
                return null;

            List<string> sounds = new List<string>();
            foreach (string item in pSoundFiles)
            {
                sounds.Add(item.Remove(0, "Data\\Sound\\".Length).Replace("\\", "/"));
            }

            return sounds;
        }

        private List<string> GetFilters(List<string> pFilterFiles)
        {
            if ((pFilterFiles == null) || (pFilterFiles.Count == 0))
                return null;

            List<string> filters = new List<string>();
            foreach (string item in pFilterFiles)
            {
                filters.Add(item.Remove(0, "Data/CuttingRoom/Filters/".Length).Replace("\\", "/"));
            }

            return filters;
        }



        private List<string> GetSpecialEffects(List<string> pSpecialEffects)
        {
            if ((pSpecialEffects == null) || (pSpecialEffects.Count == 0))
                return null;

            List<string> specialEffects = new List<string>();
            foreach (string item in pSpecialEffects)
            {
                specialEffects.Add(item.Replace("Data\\", "").Replace("\\", "/").Replace(".mps", ""));
            }

            return specialEffects;
        }

        // --------------------------------------------------------------------------------------------------------


        private double? GetMeshDataSize()
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
            // summary.AppendLine(string.Format("FriendlyName: {0}", _friendlyName));
            summary.AppendLine(string.Format("    Publisher: {0}", Publisher));
            summary.AppendLine(string.Format("    Free: {0}", Free));
            if(LastCompiled.HasValue)
                summary.AppendLine(string.Format($"    Last compiled: {LastCompiled.Value:u}"));
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
            if(!string.IsNullOrEmpty(Revision))
                summary.AppendLine(string.Format("    Revision: {0}", Revision));
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

            AppendMiscList(summary, Filters, "Filters");

            AppendMiscList(summary, SpecialEffects, "Special Effects");

            AppendMiscList(summary, Materials, "Materials");

            AppendMiscList(summary, SkyTextures, "Sky Textures");

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


    sealed class CheckContentsInFileResult
    {
        public bool HasAddonSignatureFile { get; set; }

        public bool HasAssetDataFile { get; set; }

        public bool HasVersionFile { get; set; }

        public bool HasPropertiesFile { get; set; }

        public bool HasThumbnail { get; set; }

        public List<string> DemoMovies { get; set; }

        public List<string> StockAssets { get; set; }

        public bool HasDataContents { get; set; }

        public bool HasStateMachine { get; set; }

        public bool HasVerbs { get; set; }

        public bool HasCal3DMeshFiles { get; set; }

        public List<string> MaterialsFiles { get; set; }

        public List<string> SkyFiles { get; set; }
        // public List<string> PropAnimations { get; set; }
        public List<string> SoundFiles { get; set; }
        public List<string> FilterFiles { get; set; }
        public List<string> SpecialEffects { get; set; }
    }

}
