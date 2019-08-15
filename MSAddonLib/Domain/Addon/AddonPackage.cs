using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using MSAddonLib.Domain.AssetFiles;
using MSAddonLib.Util.Persistence;
using SevenZip;


namespace MSAddonLib.Domain.Addon
{
    /// <summary>
    /// Detailed information about a addon package
    /// </summary>
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


        public AddonPackageSource SourceType { get; private set; }

        public string Source { get; private set; }


        /// <summary>
        /// Name of the addon
        /// </summary>
        public string Name { get { return AddonSignature.Name; } }

        /// <summary>
        /// Friendly name of the addon
        /// </summary>
        public string FriendlyName { get { return string.IsNullOrEmpty(_friendlyName) ? AddonSignature.Name : _friendlyName; } }
        private string _friendlyName = "";

        /// <summary>
        /// Name of the account of the published
        /// </summary>
        public string Publisher { get { return AddonSignature.Publisher; } }

        /// <summary>
        /// Qualified name of the addon
        /// </summary>
        public string QualifiedName
        {
            get { return Publisher + "." + Name; }
        }


        /// <summary>
        /// The addon is free, not requiring a license for its use in Moviestorm
        /// </summary>
        public bool Free { get { return AddonSignature.Free; } }

        /// <summary>
        /// Description of the addon
        /// </summary>
        public string Description { get { return AddonSignature.Description; } }

        /// <summary>
        /// Descriptive blurb for the addon
        /// </summary>
        public string Blurb { get; private set; } = "";

        /// <summary>
        /// Revision number of the addon
        /// </summary>
        public string Revision { get; private set; } = "";

        /// <summary>
        /// Addon signature file of the addon
        /// </summary>
        public AddonSignatureFile AddonSignature { get; private set; }

        /// <summary>
        /// Asset manifest contents
        /// </summary>
        public AssetManifest AssetManifest { get; private set; } = null;

        /// <summary>
        /// Size of the mesh data file in megabytes
        /// </summary>
        public double? MeshDataSizeMbytes { get; private set; } = null;

        /// <summary>
        /// The addon contains Cal3D mesh files (.cmf)
        /// </summary>
        public bool HasCal3DMeshFiles { get; private set; }

        /// <summary>
        /// The addon contains a signature file (required)
        /// </summary>
        public bool HasSignatureFile { get; private set; } = false;

        /// <summary>
        /// The addon contains an asset data archive (required)
        /// </summary>
        public bool HasAssetDataArchive { get; private set; } = false;

        /// <summary>
        /// The addon has a data folder (required)
        /// </summary>
        public bool HasDataFolder { get; private set; } = false;

        /// <summary>
        /// List of demo (starter) movies included
        /// </summary>
        public List<string> DemoMovies { get; private set; }

        /// <summary>
        /// List of stock assets
        /// </summary>
        public List<string> StockAssets { get; private set; }

        /// <summary>
        /// The addon has a thumbnail image file
        /// </summary>
        public bool HasThumbnail { get; private set; } = false;

        /// <summary>
        /// The addon has a machine state file
        /// </summary>
        public bool HasStateMachine { get; set; } = false;

        /// <summary>
        /// The addon has a verb definition file
        /// </summary>
        public bool HasVerbs { get; set; } = false;


        /// <summary>
        /// Summary info about body models
        /// </summary>
        public BodyModelsSummary BodyModelsSummary { get; private set; }

        /// <summary>
        /// Summary info about prop models 
        /// </summary>
        public PropModelsSummary PropModelsSummary { get; private set; }

        /// <summary>
        /// Summary info about animations
        /// </summary>
        public VerbsSummary VerbsSummary { get; set; }

        /// <summary>
        /// Sound files in the addon
        /// </summary>
        public List<string> Sounds { get; private set; }

        /// <summary>
        /// Filters in the addon
        /// </summary>
        public List<string> Filters { get; private set; }

        /// <summary>
        /// Special effects in the addon
        /// </summary>
        public List<string> SpecialEffects { get; private set; }

        /// <summary>
        /// Materials in the addon
        /// </summary>
        public List<string> Materials { get; private set; }

        /// <summary>
        /// Sky textures in the addon
        /// </summary>
        public List<string> SkyTextures { get; private set; }

        // --------------------------

        /// <summary>
        /// The addon has some issue
        /// </summary>
        public bool HasIssues => !string.IsNullOrEmpty(_issuesStringBuilder.ToString());

        /// <summary>
        /// Text of the issues
        /// </summary>
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

        private SevenZipArchiver Archiver { get; set; }


        // -----------------------------------------------------------------------------------------------
        /// <summary>
        /// Full information about the addon
        /// </summary>
        /// <param name="pProcessingFlags">Processing flags</param>
        /// <param name="pArchiver">Archiver for accessing the addon archive contents</param>
        /// <param name="pTemporaryFolder">Path to the root temporary folder</param>
        public AddonPackage(ProcessingFlags pProcessingFlags, SevenZipArchiver pArchiver, string pTemporaryFolder)
        {
            if (pArchiver == null)
            {
                throw new Exception("Not a valid archiver");
            }

            SourceType = AddonPackageSource.Archiver;
            if (pArchiver.Source == SevenZipArchiverSource.File)
                Source = pArchiver.ArchiveName;

            if (pArchiver.ArchivedFileList(out _addonFileList) < 0)
            {
                throw new Exception($"Error extracting the list of archived files: {pArchiver.LastErrorText}");
            }

            Archiver = pArchiver;

            LoadAddonPackage(pProcessingFlags, pTemporaryFolder);

            CheckContentsInFileResult contentsSummary = CheckContentsInFileList(_addonFileList);

            ListAllAnimationFiles = pProcessingFlags.HasFlag(ProcessingFlags.ListAllAnimationFiles);
            ListGestureGaitsAnimationFiles = pProcessingFlags.HasFlag(ProcessingFlags.ListGestureGaitsAnimations);
            ListWeirdGestureGaitVerbs = pProcessingFlags.HasFlag(ProcessingFlags.ListWeirdGestureGaitsVerbs);

            if (!contentsSummary.HasAddonSignatureFile)
            {
                throw new Exception("No Addon Signature file (.AddOn)");
            }
            if (!contentsSummary.HasAssetDataFile)
            {
                throw new Exception("No Addon AssetData file (assetData.jar)");
            }


            RetrieveAddonSignatureInfo(pArchiver);

            HasVerbs = contentsSummary.HasVerbs;

            try
            {
                // RetrieveAssetManifest(pAddonPath, pTemporaryFolder);
                RetrieveAssetManifest(pArchiver, pTemporaryFolder);
            }
            catch (Exception exception)
            {
                _issuesStringBuilder.AppendLine(exception.Message);
            }

            RetrieveVersionInfo(pArchiver);
            RetrievePropertiesInfo(pArchiver);

            MeshDataSizeMbytes = GetMeshDataSize();

            HasCal3DMeshFiles = contentsSummary.HasCal3DMeshFiles;

            HasDataFolder = contentsSummary.HasDataContents;
            DemoMovies = contentsSummary.DemoMovies;
            StockAssets = contentsSummary.StockAssets;

            HasThumbnail = (GetArchivedFileFullName(ThumbnailFilename) != null);

            HasStateMachine = contentsSummary.HasStateMachine;

            if (HasVerbs || HasStateMachine)
            {
                string errorText;
                VerbsSummary = new VerbsSummary(pArchiver);
                if (!VerbsSummary.PopulateSummary(out errorText))
                {
                    // Ignore at this moment
                    // throw new Exception(errorText);
                }
            }


            Sounds = GetSounds(contentsSummary.SoundFiles);

            Filters = GetFilters(contentsSummary.FilterFiles);

            SpecialEffects = GetSpecialEffects(contentsSummary.SpecialEffects);

            Materials = GetMaterials(pArchiver, contentsSummary.MaterialsFiles);

            SkyTextures = GetSkies(contentsSummary.SkyFiles);
        }



        /// <summary>
        /// Full information about the addon
        /// </summary>
        /// <param name="pProcessingFlags">Processing flags</param>
        /// <param name="pFolderPath">Path of the folder containing the addon</param>
        /// <param name="pTemporaryFolder">Path to the root temporary folder</param>
        public AddonPackage(ProcessingFlags pProcessingFlags, string pFolderPath, string pTemporaryFolder)
        {
            if (string.IsNullOrEmpty(pFolderPath = pFolderPath.Trim()))
            {
                throw new Exception("Not a valid folder specification");
            }

            string folderPath = (Path.IsPathRooted(pFolderPath)) ? pFolderPath : Path.GetFullPath(pFolderPath);
            if (!Directory.Exists(folderPath))
            {
                throw new Exception("Folder not found");
            }

            SourceType = AddonPackageSource.Folder;
            Source = folderPath;

            LoadAddonPackage(pProcessingFlags, pTemporaryFolder);



        }


        private bool LoadAddonPackage(ProcessingFlags pProcessingFlags, string pTemporaryFolder)
        {
            bool isFolderAddon = SourceType == AddonPackageSource.Folder;

            ListAllAnimationFiles = pProcessingFlags.HasFlag(ProcessingFlags.ListAllAnimationFiles);
            ListGestureGaitsAnimationFiles = pProcessingFlags.HasFlag(ProcessingFlags.ListGestureGaitsAnimations);
            ListWeirdGestureGaitVerbs = pProcessingFlags.HasFlag(ProcessingFlags.ListWeirdGestureGaitsVerbs);

            CheckContentsInFileResult contentsSummary =
                isFolderAddon
                    ? CheckContentsInFileList(Source)
                    : CheckContentsInFileList(_addonFileList);

            if (!contentsSummary.HasAddonSignatureFile)
            {
                throw new Exception("No Addon Signature file (.AddOn)");
            }
            if (!contentsSummary.HasAssetDataFile)
            {
                throw new Exception("No Addon AssetData file (assetData.jar)");
            }

            HasVerbs = contentsSummary.HasVerbs;

            if (isFolderAddon)
            {
                RetrieveAddonSignatureInfo(Source);

                try
                {
                    RetrieveAssetManifest(Source, pTemporaryFolder);
                }
                catch (Exception exception)
                {
                    _issuesStringBuilder.AppendLine(exception.Message);
                }

                RetrieveVersionInfo(Source);
                RetrievePropertiesInfo(Source);
            }
            else
            {
                RetrieveAddonSignatureInfo(Archiver);

                try
                {
                    RetrieveAssetManifest(Archiver, pTemporaryFolder);
                }
                catch (Exception exception)
                {
                    _issuesStringBuilder.AppendLine(exception.Message);
                }

                RetrieveVersionInfo(Archiver);
                RetrievePropertiesInfo(Archiver);
            }

            MeshDataSizeMbytes = GetMeshDataSize();

            HasCal3DMeshFiles = contentsSummary.HasCal3DMeshFiles;

            HasDataFolder = contentsSummary.HasDataContents;
            DemoMovies = contentsSummary.DemoMovies;
            StockAssets = contentsSummary.StockAssets;

            HasThumbnail = (GetArchivedFileFullName(ThumbnailFilename) != null);

            HasStateMachine = contentsSummary.HasStateMachine;

            if (HasVerbs || HasStateMachine)
            {
                string errorText;
                VerbsSummary = 
                    isFolderAddon ? new VerbsSummary(Source) : new VerbsSummary(Archiver);
                if (!VerbsSummary.PopulateSummary(out errorText))
                {
                    // Ignore at this moment
                    // throw new Exception(errorText);
                }
            }


            Sounds = GetSounds(contentsSummary.SoundFiles);

            Filters = GetFilters(contentsSummary.FilterFiles);

            SpecialEffects = GetSpecialEffects(contentsSummary.SpecialEffects);

            Materials = isFolderAddon 
                ? GetMaterials(Source, contentsSummary.MaterialsFiles)
                : GetMaterials(Archiver, contentsSummary.MaterialsFiles);

            SkyTextures = GetSkies(contentsSummary.SkyFiles);

            return true;
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

                /*
                if (filename.Contains("\\props\\") && filename.EndsWith(".caf"))
                {
                    if (result.PropAnimations == null)
                        result.PropAnimations = new List<string>();
                    result.PropAnimations.Add(item.FileName);
                    continue;
                }
                */

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
                    filenameLower = fileName.Remove(0, prefixLen);
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
        /// Retrieves addon basic info from .AddOn file in archive
        /// </summary>
        /// <param name="pArchiver">Archiver for retrieving data from archive</param>
        private void RetrieveAddonSignatureInfo(SevenZipArchiver pArchiver)
        {
            string addonSignatureFilename = GetArchivedFileFullName(SignatureFilename);
            byte[] addonFileContents = pArchiver.ExtractArchivedFileToByte(addonSignatureFilename);

            if (addonFileContents == null)
            {
                throw new Exception($"EXCEPTION recovering archived contents of Signature file: {pArchiver.LastErrorText}");
            }

            AddonSignature = AddonSignatureFile.Load(addonFileContents);
            HasSignatureFile = true;
        }

        // --------------------------------------------------------------------------------------------------------------------------------------------------------------------


        /// <summary>
        /// Retrieves the asset manifest from the file inside the assetData.jar class library
        /// </summary>
        /// <param name="pArchiver">Archiver to access the addon file contents</param>
        /// <param name="pTemporaryFolder">Path to temporary folder</param>
        private void RetrieveAssetManifest(SevenZipArchiver pArchiver, string pTemporaryFolder)
        {
            // Previously, extract the Manifest file into the temp file

            string assetDataArchivedFile = GetArchivedFileFullName(AssetDataFilename);
            List<string> filesToExtract = new List<string>() { assetDataArchivedFile };
            if (pArchiver.ArchivedFilesExtract(pTemporaryFolder, filesToExtract) < 0)
            {
                throw new Exception($"EXCEPTION extracting Manifest file {pArchiver.LastErrorText}");
            }

            string assetDataFile = Path.Combine(pTemporaryFolder, AssetDataFilename);
            if (!File.Exists(assetDataFile))
                throw new Exception($"No {AssetDataFilename} file");


            SevenZipArchiver assetDataArchiver = new SevenZipArchiver(assetDataFile);

            string manifestContents = assetDataArchiver.ExtractArchivedFileToString(ManifestFilename, true);

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

            // bool loadAllAnimationFiles = ListAllAnimationFiles || !HasVerbs;
            BodyModelsSummary = new BodyModelsSummary(pArchiver, pTemporaryFolder, assetManifest.BodyModels, ListAllAnimationFiles, ListGestureGaitsAnimationFiles);
            if (!BodyModelsSummary.PopulateSummary(out errorText))
            {
                throw new Exception(errorText);
            }

            bool loadAllAnimationFiles = ListAllAnimationFiles || !HasVerbs;
            PropModelsSummary = new PropModelsSummary(pArchiver, pTemporaryFolder, assetManifest.PropModels, loadAllAnimationFiles);
            if (!PropModelsSummary.PopulateSummary(out errorText))
            {
                throw new Exception(errorText);
            }

            AssetManifest = assetManifest;
            HasAssetDataArchive = true;
        }

        // -------------------------------------------------------------------------------------------------------------------------------


        private void RetrieveVersionInfo(SevenZipArchiver pArchiver)
        {
            string versionFileFullName = GetArchivedFileFullName(VersionInfoFilename);
            if (string.IsNullOrEmpty(versionFileFullName))
                return;

            string versionFileContents = pArchiver.ExtractArchivedFileToString(versionFileFullName);
            if (string.IsNullOrEmpty((versionFileContents = versionFileContents?.Trim())))
                return;

            string errorText;
            AddonVersionInfo addonVersionInfo = AddonVersionInfo.LoadFromString(versionFileContents, out errorText);
            if (addonVersionInfo != null)
                Revision = addonVersionInfo.Revision;
        }

        // --------------------------------------------------------------------------------------------------------------------------------------------------------

        private void RetrievePropertiesInfo(SevenZipArchiver pArchiver)
        {
            string propertiesFileFullName = GetArchivedFileFullName(PropertiesFilename);
            if (string.IsNullOrEmpty(propertiesFileFullName))
                return;

            string propertiesFileContents = pArchiver.ExtractArchivedFileToString(propertiesFileFullName);
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

        private List<string> GetMaterials(SevenZipArchiver pArchiver, List<string> pMaterialsFiles)
        {
            if ((pMaterialsFiles == null) || (pMaterialsFiles.Count == 0))
                return null;

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
            foreach (ArchiveFileInfo file in _addonFileList)
            {
                if (file.FileName.ToLower() == MeshDataFilename.ToLower())
                {
                    return file.Size / BytesPerMegabyte;
                }
            }

            return null;
        }


        // -----------------------------------------------------------------------------------------------------

        public override string ToString()
        {
            if (ReportText != null)
                return ReportText;

            StringBuilder summary = new StringBuilder();
            summary.AppendLine(string.Format("    Name: {0}", Name));
            // summary.AppendLine(string.Format("FriendlyName: {0}", _friendlyName));
            summary.AppendLine(string.Format("    Publisher: {0}", Publisher));
            summary.AppendLine(string.Format("    Free: {0}", Free));
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

            if (!string.IsNullOrEmpty(Blurb.Trim()) &&
                (string.IsNullOrEmpty(Description?.Trim()) || (Blurb.Trim() != Description?.Trim())))
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
            if (DemoMovies != null)
            {
                summary.AppendLine("    * Includes Demo Movies:");
                foreach (string item in DemoMovies)
                    summary.AppendLine($"         '{item}'");
            }

            if (StockAssets != null)
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
                if (VerbsSummary != null)
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
                if (printHasVerbs)
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


    public enum AddonPackageSource
    {
        Folder,
        Archiver
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
