using System;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;

namespace MSAddonLib.Util
{
    public static class Utils
    {

        /// <summary>
        /// If set to true, uses the home executable folder as root for the temporary folders (by default). Otherwise, uses the user local data temp folder.
        /// </summary>
        /// <remarks>The value can't be changed once has been set for the first time, either directly or indirectly</remarks>
        public static bool UsesApplicationTempFolder
        {
            get
            {
                if (!_usesApplicationTempFolder.HasValue)
                    _usesApplicationTempFolder = true;
                return _usesApplicationTempFolder.Value;
            }
            set
            {
                if (!_usesApplicationTempFolder.HasValue)
                    _usesApplicationTempFolder = value;
            }
        }
        private static bool? _usesApplicationTempFolder = null;

        private static string TemporaryFoldersRoot => _temporaryFoldersRoot ?? (_temporaryFoldersRoot = 
                                                            UsesApplicationTempFolder
                                                          ? GetExecutableDirectory()
                                                          : $"{Path.Combine(Path.GetTempPath(), Path.GetFileNameWithoutExtension(Assembly.GetEntryAssembly().Location))}");

        private static string _temporaryFoldersRoot = null;

        private static string _versionString = null;

        private static string _executableDirectory = null;

        private static string _tempDirectory = null;

        private static string _backupDirectory = null;


        // ------------------------------------------------------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Gets a string containing the version of the executable (major + minor + build)
        /// </summary>
        /// <returns>Version string of the executable</returns>
        public static string GetExecutableVersion()
        {
            if (_versionString != null)
                return _versionString;

            Version version =
                Assembly.GetEntryAssembly()?.GetName().Version;

            string revision = (version.Revision > 0) ? $".{version.Revision}" : "";

            
            return (_versionString = (version == null) ? "?" : $"{version.Major}.{version.Minor}.{version.Build}{revision}");
        }


        /// <summary>
        /// Gets the executable home folder
        /// </summary>
        /// <returns>Path to the executable home folder</returns>
        public static string GetExecutableDirectory()
        {
            return _executableDirectory ?? (_executableDirectory = Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location));
        }

        /// <summary>
        /// Gets the executable full path
        /// </summary>
        /// <returns>Path to the executable file</returns>
        public static string GetExecutableFullPath()
        {
            return System.Reflection.Assembly.GetEntryAssembly().Location;
        }


        /// <summary>
        /// Gets the temporary folder
        /// </summary>
        /// <returns>Path to the temporary folder</returns>
        public static string GetTempDirectory()
        {
            return _tempDirectory ?? (_tempDirectory = $"{TemporaryFoldersRoot}\\Temp");
        }

        /// <summary>
        /// Gets the backup folder
        /// </summary>
        /// <returns>Path to the backup folder</returns>
        public static string GetBackupDirectory()
        {
            return _backupDirectory ?? (_backupDirectory = $"{TemporaryFoldersRoot}\\Backup");
        }

        // -------------------------------------------------------------------------------------------------------------------------------------------------


        /// <summary>
        /// Reset the temporary and backup folders
        /// </summary>
        /// <param name="pErrorText">Text of error, if any</param>
        /// <returns>Result of the operation</returns>
        public static bool ResetTemporaryFolders(out string pErrorText)
        {
            return ResetFolder(GetTempDirectory(), out pErrorText) &&
                ResetFolder(GetBackupDirectory(), out pErrorText);
        }


        /// <summary>
        /// Reset the temporary folder
        /// </summary>
        /// <param name="pErrorText">Text of error, if any</param>
        /// <returns>Result of the operation</returns>
        public static bool ResetTempFolder(out string pErrorText)
        {
            return ResetFolder(GetTempDirectory(), out pErrorText);
        }


        /// <summary>
        /// Delete folder recursively
        /// </summary>
        /// <param name="pPath"></param>
        /// <param name="pErrorText">Text of error, if any</param>
        /// <returns>Result of the operation</returns>
        public static bool DeleteTempFolder(string pPath, out string pErrorText)
        {
            pErrorText = null;
            if (string.IsNullOrEmpty(pPath?.Trim()))
            {
                pErrorText = "Folder specification blank";
                return false;
            }

            string rootTempFolder = GetTempDirectory()?.ToLower() ?? "--";
            string pathLower = pPath.ToLower();
            if (!pathLower.StartsWith(rootTempFolder))
            {
                pErrorText = "Path is not part of the temporary folder hierarchy";
                return false;
            }
            
            bool gotDeleted = false;
            try
            {
                _DeleteDirectory(pPath);
                Thread.Sleep(1000);
                if (!Directory.Exists(pPath))
                    gotDeleted = true;
                else
                    pErrorText = $"Folder '{pPath}' couldn't be deleted";
            }
            catch (Exception exception)
            {
                pErrorText = exception.Message;
            }
            
            return gotDeleted;
        }



        // ----------------------------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Resets a folder by deleting all its contents recursively
        /// </summary>
        /// <param name="pPath">Path to the folder to be reset</param>
        /// <param name="pErrorText">Text of error, if any</param>
        /// <returns>Result of the operation</returns>
        public static bool ResetFolder(string pPath, out string pErrorText)
        {
            pErrorText = null;
            if (string.IsNullOrEmpty(pPath?.Trim()))
            {
                pErrorText = "Folder specification blank";
                return false;
            }

            bool gotOk = false;
            if (!Directory.Exists(pPath))
            {
                try
                {
                    Directory.CreateDirectory(pPath);
                    gotOk = true;
                }
                catch (Exception exception)
                {
                    pErrorText = $"{pPath}: {exception.Message}";
                }

                return gotOk;
            }

            // Folder already exists

            try
            {
                _DeleteDirectory(pPath);
                Thread.Sleep(1000);
                _CreateDirectory(pPath);
                gotOk = true;
            }
            catch (Exception exception)
            {
                pErrorText = $"{pPath}: {exception.Message}";
            }

            return gotOk;
        }


        private static void _DeleteDirectory(string pPath)
        {
            DirectoryInfo baseDirInfo = new DirectoryInfo(pPath);
            foreach (FileInfo file in baseDirInfo.EnumerateFiles("*", SearchOption.AllDirectories))
            {
                file.IsReadOnly = false;
                file.Delete();
            }

            Exception exception = null;
            for (int count = 0; count < 10;)
            {
                try
                {
                    Directory.Delete(pPath, true);
                    return;
                }
                catch (Exception e)
                {
                    exception = e;
                }
                count += 2;
                Thread.Sleep(1000 * (count / 2));
            }
            if(exception != null)
                throw exception;
        }

        private static void _CreateDirectory(string pPath)
        {

            Exception exception = null;
            for (int count = 0; count < 10;)
            {
                try
                {
                    Directory.CreateDirectory(pPath);
                    if(Directory.Exists(pPath))
                        return;
                }
                catch (Exception e)
                {
                    exception = e;
                }
                count += 2;
                Thread.Sleep(1000 * (count / 2));
            }
            if (exception != null)
                throw exception;
        }


        // -------------------------------------------------------------------------------------------------------


        /// <summary>
        /// Checks whether a filename fits to a given template (regular expression)
        /// </summary>
        /// <param name="pFilename">Name of the file</param>
        /// <param name="pFileMask">Mask of the file</param>
        /// <returns>Result of the check</returns>
        public static bool MatchesFileMask(string pFilename, string pFileMask)
        {
            // const string specialChars = ".[]()^$+=!{,}";
            const string specialChars = ".()^$+=!{,}";
            bool matchResult = false;
            try
            {
                StringBuilder fileMaskConverted = new StringBuilder(pFileMask);
                foreach (char specialChar in specialChars)
                {
                    fileMaskConverted.Replace(specialChar.ToString(), $@"\{specialChar}");
                }
                fileMaskConverted.Replace("*", ".*").Replace("?", ".");

                Regex mask = new Regex($"^{fileMaskConverted.ToString()}$",
                    RegexOptions.IgnoreCase);
                matchResult = mask.IsMatch(pFilename);
            }
            catch
            {
                matchResult = false;
            }
            return matchResult;
        }


        // -------------------------------------------------------------------------------------------------------

        public static string GetExceptionExtendedMessage(Exception pException)
        {
            string text = pException.Message;

            string innerExceptionText = null;

            while (pException.InnerException != null)
            {
                innerExceptionText = pException.InnerException.Message;
                pException = pException.InnerException;
            }

            if (innerExceptionText == null)
                return text;

            return $"{text} [{innerExceptionText}]";
        }


        public static string GetExceptionFullMessage(Exception pException)
        {
            return _GetExceptionFullMessage(pException, null);
        }


        private static string _GetExceptionFullMessage(Exception pException, string pText)
        {
            if (pException.InnerException == null)
                return pException.Message;

            string innerText = _GetExceptionFullMessage(pException.InnerException, null);

            return $"{pException.Message} [{innerText}]";
        }



        // -------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Extension method for Tooltips 
        /// </summary>
        /// <param name="pToolTip">tooltip</param>
        public static void SetDefaults(this ToolTip pToolTip)
        {
            // Set up the delays for the ToolTip.
            pToolTip.AutoPopDelay = 5000;
            pToolTip.InitialDelay = 1000;
            pToolTip.ReshowDelay = 500;
            // Force the ToolTip text to be displayed whether or not the form is active.
            pToolTip.ShowAlways = true;
        }


    }



    
}
