using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using SevenZip;

namespace MSAddonLib.Util.Persistence
{
    public static class PersistenceUtils
    {
        /// <summary>
        /// Restore an addon 
        /// </summary>
        /// <param name="pArchiver"></param>
        /// <param name="pDeleteSource">Delete source file if ok</param>
        /// <param name="pRootFolder"></param>
        /// <param name="pErrorText">Text of error, if any</param>
        /// <returns>Name of the addon file, or null if error</returns>
        public static string RestoreAddonFile(SevenZipArchiver pArchiver, bool pDeleteSource, string pRootFolder, out string pErrorText)
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
                pErrorText = $"RestoreAddonFile(), Exception: {exception.Message}";
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

    }
}
