using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using SevenZip;

namespace MSAddonLib.Util.Persistence
{
    public static class AddonPersistenceUtils
    {
        private static bool _gotMoviestormPaths = false;

        private static MoviestormPaths _moviestormPaths = null;

        private static string _moviestormPathsError = null;



        // ---------------------------------------------------------------------------------------------------

        /// <summary>
        /// Restore an addon file disguised as an archive
        /// </summary>
        /// <param name="pArchiver"></param>
        /// <param name="pDeleteSource">Delete source archive if restoration succeeds</param>
        /// <param name="pRootFolder">Root folder, in the case of a rooted addon</param>
        /// <param name="pErrorText">Text of error, if any</param>
        /// <returns>Name of the addon file, or null if error</returns>
        public static string CorrectAddonFile(SevenZipArchiver pArchiver, bool pDeleteSource, string pRootFolder, out string pErrorText)
        {
            pErrorText = null;


            string sourceFile = pArchiver.ArchiveName;
            string extension = Path.GetExtension(sourceFile);
            string destFile = pRootFolder == null 
                ? sourceFile.Replace(extension, ".addon") 
                : Path.Combine(Path.GetDirectoryName(sourceFile) ?? "", pRootFolder + ".addon");

            string tempPath = Utils.GetTempDirectory();
            string destFolder = null;

            bool processOk = false;
            try
            {
                SevenZipExtractor extractor = pArchiver.GetExtractor();
                if (extractor == null)
                {
                    pErrorText = "Couldn't determine archive format";
                    return null;
                }

                bool isZipArchive = (extractor.Format == InArchiveFormat.Zip);
                bool isRooted = (pRootFolder != null);
                if (!isRooted && isZipArchive)
                {
                    if (File.Exists(destFile))
                    {
                        pDeleteSource = false;
                        return destFile;
                    }

                    File.Copy(sourceFile, destFile);
                    if (!File.Exists(destFile))
                    {
                        pErrorText = "Couldn't create addon file";
                        return null;
                    }

                    processOk = true;
                    return destFile;
                }

                if (!isRooted)
                {
                    destFolder = Path.Combine(tempPath, Path.GetFileNameWithoutExtension(sourceFile));
                    Directory.CreateDirectory(destFolder);
                    pArchiver.ArchivedFilesExtract(destFolder, null);
                }
                else
                {
                    destFolder = Path.Combine(tempPath, pRootFolder);
                    pArchiver.ArchivedFilesExtract(tempPath, null);
                }

                SevenZipArchiver newArchiver = new SevenZipArchiver(destFile);

                newArchiver.ArchiveFolder(destFolder);
                if (!File.Exists(destFile))
                {
                    pErrorText = newArchiver.LastErrorText ?? "???";
                    return null;
                }

                processOk = true;
                return destFile;
            }
            catch (Exception exception)
            {
                pErrorText = $"CorrectAddonFile(), Exception: {exception.Message}";
            }
            finally
            {
                if (destFolder != null)
                {
                    string errorText;
                    Utils.DeleteTempFolder(destFolder, out errorText);
                }

                if (processOk)
                {
                    if ((pDeleteSource) && File.Exists(sourceFile))
                        File.Delete(sourceFile);
                }
                else
                {
                    if (File.Exists(destFile))
                        File.Delete(destFile);
                }
            }
            return null;
        }

        // -------------------------------------------------------------------------------------------------


        public static MoviestormPaths GetMoviestormPaths(out string pErrorText)
        {
            if (_gotMoviestormPaths)
            {
                pErrorText = _moviestormPathsError;
                return _moviestormPaths;
            }

            _gotMoviestormPaths = true;
            pErrorText = null;
            string installPath;
            string temptativePath =
                Environment.GetFolderPath((Environment.Is64BitOperatingSystem)
                    ? Environment.SpecialFolder.ProgramFilesX86
                    : Environment.SpecialFolder.ProgramFiles) + @"\Moviestorm";

            if (Directory.Exists(temptativePath) && File.Exists(temptativePath + @"\moviestorm.exe"))
                installPath = temptativePath;
            else
            {
                _moviestormPathsError = pErrorText = "Moviestorm installation path coudn't be automatically determined";
                return null;
            }

            string contentPacksPath = Path.Combine(installPath, "AddOn");
            if (!Directory.Exists(contentPacksPath))
            {
                _moviestormPathsError = pErrorText = "Moviestorm content packs path not found";
                return null;
            }

            string userDataPath = null;
            temptativePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Moviestorm");
            if (Directory.Exists(temptativePath) && File.Exists(temptativePath + @"\machinimascope.properties"))
                userDataPath = temptativePath;

            if (userDataPath == null)
            {
                temptativePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Moviestorm");
                if (Directory.Exists(temptativePath) && File.Exists(temptativePath + @"\machinimascope.properties"))
                    userDataPath = temptativePath;
                else
                {
                    _moviestormPathsError = pErrorText = "Moviestorm user data path coudn't be automatically determined";
                    return null;
                }
            }

            _moviestormPaths = new MoviestormPaths(installPath, userDataPath);

            return _moviestormPaths;
        }



    }


    public class MoviestormPaths
    {
        public string InstallationPath { get; private set; }

        public string ContentPacksPath { get; private set; }

        public string PreviewsPath { get; private set; }

        public string UserDataPath { get; private set; }

        public string AddonsPath { get; private set; }

        public string MoviesPath { get; private set; }


        public MoviestormPaths(string pInstallationPath, string pUserDataPath)
        {
            InstallationPath = pInstallationPath;
            ContentPacksPath = Path.Combine(InstallationPath, "AddOn");
            PreviewsPath = Path.Combine(InstallationPath, "Previews");
            UserDataPath = pUserDataPath;
            AddonsPath = Path.Combine(UserDataPath, "AddOn");
            MoviesPath = Path.Combine(UserDataPath, "Movies");
        }
    }


}
