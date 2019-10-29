using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using SevenZip;

namespace MSAddonLib.Util.Persistence
{
    public static class AddonStrongHash
    {
        public static string GetStrongHash(string pLocation, out string pErrorText)
        {
            pErrorText = null;
            if (string.IsNullOrEmpty(pLocation = pLocation.Trim()))
                return null;

            if (Directory.Exists(pLocation))
                return GetStrongHashFolder(pLocation, out pErrorText);

            int sharpIndex = pLocation.LastIndexOf("#", StringComparison.InvariantCulture);
            if (sharpIndex > 0)
            {
                return GetStrongHashArchivedAddon(pLocation.Substring(0, sharpIndex), pLocation.Substring(sharpIndex + 1), out pErrorText);
            }

            return GetStrongHashFile(pLocation, out pErrorText);
        }


        private static string GetStrongHashFolder(string pRootFolder, out string pErrorText)
        {
            pErrorText = null;
            string hash = null;
            string fileNameLower = null;
            try
            {
                string prefix = pRootFolder.ToLower() + "\\";

                List<string> fileData = new List<string>();

                foreach (string fileName in Directory.EnumerateFiles(pRootFolder, "*", SearchOption.AllDirectories))
                {
                    fileNameLower = fileName.ToLower().Replace(prefix, "");
                    if ((fileNameLower == "assetdata.jar") || (fileNameLower == "meshdata.data"))
                    {
                        fileData.Add($"{fileNameLower}^{ComputeFileHash(fileName)}|");
                        continue;
                    }

                    if (!fileNameLower.StartsWith("data\\"))
                        continue;
                    string file = Path.GetFileName(fileNameLower);
                    string extension = Path.GetExtension(fileNameLower) ?? "";
                    if ((file == "descriptor") || (extension == ".bodypart") || (extension == ".template") ||
                        (extension == ".part") || (extension == ".cmf") || (extension == ".crf"))
                        continue;
                    fileData.Add($"{fileNameLower}^{ComputeFileHash(fileName)}|");
                }

                hash = CalcStrongHash(fileData);
            }
            catch (Exception exception)
            {
                pErrorText = $"GetStrongHashFolder():: [{fileNameLower}]\n  EXCEPTION: {exception.Message}\n{exception.StackTrace}";
            }

            return hash;
        }


        private static string ComputeFileHash(string pFileName)
        {
            FileInfo info = new FileInfo(pFileName);
            if (info.Length == 0)
                return "#ZERO-LENGTH";

            byte[] hash;
            using (FileStream stream = File.OpenRead(pFileName))
            {
                SHA512Cng sha512 = new SHA512Cng();
                hash = sha512.ComputeHash(stream);
            }

            return Utils.HexaBinString(hash);
        }


        private static string CalcStrongHash(List<string> pFileData)
        {
            pFileData.Sort();

            StringBuilder builder = new StringBuilder();
            foreach (string line in pFileData)
            {
                builder.Append(line);
            }

            string fingerprintText = builder.ToString();
            byte[] bytes = Encoding.UTF8.GetBytes(fingerprintText);

            SHA256 sha256 = SHA256.Create();
            byte[] hash = sha256.ComputeHash(bytes);

            return Utils.HexaBinString(hash);
        }


        private static string GetStrongHashFile(string pAddonFile, out string pErrorText)
        {
            pErrorText = null;
            if (!File.Exists(pAddonFile))
                return null;

            string hash = null;
            string fileNameLower = null;
            try
            {
                SevenZipArchiver archiver = new SevenZipArchiver(pAddonFile);

                List<ArchiveFileInfo> fileList;
                archiver.ArchivedFileList(out fileList);

                List<string> fileData = new List<string>();

                foreach (ArchiveFileInfo item in fileList)
                {
                    if (item.IsDirectory)
                        continue;
                    fileNameLower = item.FileName.ToLower();

                    if ((fileNameLower == "assetdata.jar") || (fileNameLower == "meshdata.data"))
                    {
                        fileData.Add(
                            $"{fileNameLower}^{ComputeArchivedFileHash(archiver, item)}");
                        continue;
                    }

                    if (!fileNameLower.StartsWith("data\\"))
                        continue;
                    string file = Path.GetFileName(fileNameLower);
                    string extension = Path.GetExtension(fileNameLower) ?? "";
                    if ((file == "descriptor") || (extension == ".bodypart") || (extension == ".template") ||
                        (extension == ".part") || (extension == ".cmf") || (extension == ".crf"))
                        continue;
                    fileData.Add(
                        $"{fileNameLower}^{ComputeArchivedFileHash(archiver, item)}");
                }

                hash = CalcStrongHash(fileData);
            }
            catch (Exception exception)
            {
                pErrorText = $"GetStrongHashFile(): [{fileNameLower}]\n  EXCEPTION: {exception.Message}\n{exception.StackTrace}";
            }

            return hash;
        }


        private static string ComputeArchivedFileHash(SevenZipArchiver pArchiver, ArchiveFileInfo pFile)
        {
            if(pFile.Size == 0)
                return "#ZERO-LENGTH";
            
            byte[] hash;
            using (Stream stream = pArchiver.ExtractArchivedFileToStream(pFile.FileName))
            {
                SHA512Cng sha512 = new SHA512Cng();
                hash = sha512.ComputeHash(stream);
            }

            return Utils.HexaBinString(hash);
        }


        private static string GetStrongHashArchivedAddon(string pArchive, string pAddonName, out string pErrorText)
        {
            pErrorText = null;
            if (!File.Exists(pArchive))
                return null;

            SevenZipArchiver archiver = new SevenZipArchiver(pArchive);
            if (!archiver.FileExists(pAddonName))
                return null;

            string tempFolder = Utils.GetTempDirectory();
            if (!Directory.Exists(tempFolder))
                return null;

            archiver.ArchivedFilesExtract(tempFolder, new List<string>() { pAddonName });

            string unarchAddon = Path.Combine(tempFolder, pAddonName);
            if (!File.Exists(unarchAddon))
                return null;

            string hash = GetStrongHashFile(unarchAddon, out pErrorText);

            File.Delete(unarchAddon);

            return hash;
        }


    }
}
