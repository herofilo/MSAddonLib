using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using SevenZip;

namespace MSAddonLib.Util.Persistence
{

    /// <summary>
    /// Wrapper class for the SevenZipSharp library
    /// </summary>
    public class SevenZipArchiver
    {
        /// <summary>
        /// Text of the last error for any operation. It's reset every time any function is called
        /// </summary>
        public string LastErrorText { get; private set; }

        /// <summary>
        /// Path to the archive file
        /// </summary>
        public string ArchiveName { get; private set; }


        /// <summary>
        /// Stream of the archive
        /// </summary>
        public Stream ArchiveStream { get; private set; }


        /// <summary>
        /// Source of the archive
        /// </summary>
        public SevenZipArchiverSource Source { get; private set; } = SevenZipArchiverSource.Undefined;


        /// <summary>
        /// Password (optional)
        /// </summary>
        public string Password { get; set; } = null;

        /// <summary>
        /// Encrypt Headers of the archive
        /// </summary>
        public bool EncryptHeaders { get; set; } = false;

        /// <summary>
        /// Format of the archive
        /// </summary>
        public OutArchiveFormat ArchiveFormat { get; set; } = OutArchiveFormat.Zip;

        /// <summary>
        /// Level of compression
        /// </summary>
        public CompressionLevel CompressionLevel { get; set; } = CompressionLevel.Normal;

        /// <summary>
        /// Method of compression
        /// </summary>
        public CompressionMethod CompressionMethod { get; set; } = CompressionMethod.Deflate;




        // ------------------------------------------------------------------------------------------

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="pArchiveName">Path to the archive file</param>
        /// <param name="pPassword">Password (optional)</param>
        /// <param name="pEncryptHeaders">Encrypt headers of the archive</param>
        /// <param name="pArchiveFormat">Format of the archive</param>
        /// <param name="pCompressionLevel">Level of compression</param>
        /// <param name="pCompressionMethod">Compression method</param>        
        public SevenZipArchiver(string pArchiveName,
            string pPassword = null, bool pEncryptHeaders = false,
            OutArchiveFormat pArchiveFormat = OutArchiveFormat.Zip,
            CompressionLevel pCompressionLevel = CompressionLevel.Normal,
            CompressionMethod pCompressionMethod = CompressionMethod.Default)
        {
            ArchiveName = pArchiveName?.Trim();
            Password = pPassword;
            EncryptHeaders = pEncryptHeaders;
            ArchiveFormat = pArchiveFormat;
            CompressionLevel = pCompressionLevel;
            CompressionMethod = pCompressionMethod;
            Source = SevenZipArchiverSource.File;
        }



        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="pArchiveStream">Stream for the archive file</param>
        /// <param name="pPassword">Password (optional)</param>
        /// <param name="pEncryptHeaders">Encrypt headers of the archive</param>
        /// <param name="pArchiveFormat">Format of the archive</param>
        /// <param name="pCompressionLevel">Level of compression</param>
        /// <param name="pCompressionMethod">Compression method</param>        
        public SevenZipArchiver(Stream pArchiveStream,
            string pPassword = null, bool pEncryptHeaders = false,
            OutArchiveFormat pArchiveFormat = OutArchiveFormat.Zip,
            CompressionLevel pCompressionLevel = CompressionLevel.Normal,
            CompressionMethod pCompressionMethod = CompressionMethod.Default)
        {
            ArchiveStream = pArchiveStream;
            Password = pPassword;
            EncryptHeaders = pEncryptHeaders;
            ArchiveFormat = pArchiveFormat;
            CompressionLevel = pCompressionLevel;
            CompressionMethod = pCompressionMethod;
            Source = SevenZipArchiverSource.Stream;
        }



        // ----------------------------------------------------------------------------------------

        // TODO : ArchivedFileList() - support for encrypted archives
        /// <summary>
        /// Returns the contents of the archive, as a list of strings
        /// </summary>
        /// <param name="pEntryList">List of files in the archive</param>
        /// <returns>Number of files in the archie (-1==error)</returns>
        /// <remarks>CAVEAT: it does not support encrypted archives so far</remarks>
        public int ArchivedFileList(out List<ArchiveFileInfo> pEntryList)
        {
            LastErrorText = null;
            pEntryList = null;
            try
            {
                SevenZipExtractor extractor = GetExtractor();
                if (extractor == null)
                    return -1;

                using (extractor)
                {
                    pEntryList = new List<ArchiveFileInfo>();
                    foreach (ArchiveFileInfo item in extractor.ArchiveFileData)
                    {
                        pEntryList.Add(item);
                    }
                }

            }
            catch (Exception exception)
            {
                LastErrorText = $"EXCEPTION: {exception.Message}";
                pEntryList = null;
                return -1;
            }
            return pEntryList.Count;
        }



        public SevenZipExtractor GetExtractor()
        {
            if (Source == SevenZipArchiverSource.File)
            {
                if (string.IsNullOrEmpty(ArchiveName) || !File.Exists(ArchiveName))
                {
                    LastErrorText = "Invalid archive file name specification/file not found";
                    return null;
                }

                return new SevenZipExtractor(ArchiveName);
            }

            if (Source == SevenZipArchiverSource.Stream)
            {
                if (ArchiveStream == null)
                {
                    LastErrorText = "Invalid Stream for archive";
                    return null;
                }
                return new SevenZipExtractor(ArchiveStream);
            }

            LastErrorText = "Invalid source for archive";
            return null;
        }


        // -----------------------------------------------------------------------------------------------------

        public bool FileExists(string pFilename)
        {
            LastErrorText = null;

            if (string.IsNullOrEmpty(pFilename = pFilename?.Trim().ToLower()))
            {
                LastErrorText = "Invalid file name";
                return false;
            }

            try
            {
                SevenZipExtractor extractor = GetExtractor();
                if (extractor == null)
                {
                    return false;
                }

                using (extractor)
                {
                    foreach (var fileInfo in extractor.ArchiveFileData)
                    {
                        string filename = fileInfo.FileName.ToLower();
                        if (filename == pFilename)
                        {
                            return true;
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                LastErrorText = $"EXCEPTION: {exception.Message}";
                return false;
            }

            return false;
        }



        // -----------------------------------------------------------------------------------------------------

        // TODO : ArchivedFilesExtract() - support for encrypted archives
        /// <summary>
        /// Extracts files in the archive
        /// </summary>
        /// <param name="pDestinationPath">Path to the folder of destination</param>
        /// <param name="pFileList">List with specifications of the files to extract. It can be a mask of files</param>
        /// <remarks>As of this date, it does not work the filtering by the list of name of files</remarks>
        /// <returns>Number of files extracted. -1 if error</returns>
        /// <remarks>CAVEAT: it does not support encrypted archives so far</remarks>
        public int ArchivedFilesExtract(string pDestinationPath, List<string> pFileList)
        {
            LastErrorText = null;
            int fileExtractedCount = 0;
            try
            {
                SevenZipExtractor extractor = GetExtractor();
                if (extractor == null)
                    return -1;

                using (extractor)
                {
                    if ((pFileList?.Count ?? -1) <= 0)
                    {
                        extractor.ExtractArchive(pDestinationPath);
                    }
                    else
                    {
                        List<string> lwrFileList = new List<string>();
                        foreach (string item in pFileList)
                            lwrFileList.Add(item.ToLower().Trim());
                        List<int> fileIndexes = new List<int>();
                        for (int index = 0; index < extractor.ArchiveFileData.Count; ++index)
                        {
                            string filename = extractor.ArchiveFileData[index].FileName;
                            if (lwrFileList.Contains(filename.ToLower().Trim()))
                                fileIndexes.Add(index);
                        }
                        fileExtractedCount = fileIndexes.Count;
                        if (fileExtractedCount > 0)
                        {
                            extractor.ExtractFiles(pDestinationPath, fileIndexes.ToArray());
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                LastErrorText = $"EXCEPTION: {exception.Message}";
                return -1;
            }

            return fileExtractedCount;

        }



        // TODO : ExtractArchivedFileToString() - support for encrypted archives
        /// <summary>
        /// Extracts an archived file as a string
        /// </summary>
        /// <param name="pFilename">Name of the file to extract</param>
        /// <param name="pAnsiString">Returns an ANSI string (default: UTF-8)</param>
        /// <returns>Text of the file</returns>
        /// <remarks>CAVEAT: it does not support encrypted archives so far</remarks>
        public string ExtractArchivedFileToString(string pFilename, bool pAnsiString = false)
        {
            byte[] bytes = ExtractArchivedFileToByte(pFilename);
            if (bytes == null)
                return null;

            return pAnsiString
                ? Encoding.ASCII.GetString(bytes, 0, bytes.Length)
                : Encoding.UTF8.GetString(bytes, 0, bytes.Length);
        }


        // TODO : ExtractArchivedFileToByte() - support for encrypted archives
        /// <summary>
        /// Extracts an archived file as a byte vector
        /// </summary>
        /// <param name="pFilename">Name of the file to extract</param>
        /// <returns>Content of the file as a byte vector</returns>
        /// <remarks>CAVEAT: it does not support encrypted archives so far</remarks>
        public byte[] ExtractArchivedFileToByte(string pFilename)
        {
            if (string.IsNullOrEmpty(pFilename = pFilename?.Trim().ToLower()))
            {
                LastErrorText = "Invalid file specification";
                return null;
            }

            LastErrorText = null;
            byte[] buffer = null;
            try
            {
                SevenZipExtractor extractor = GetExtractor();
                if (extractor == null)
                    return null;

                using (extractor)
                {
                    int fileIndex = -1;
                    for (int index = 0; index < extractor.ArchiveFileData.Count; ++index)
                    {
                        string filename = extractor.ArchiveFileData[index].FileName.ToLower();
                        if (filename == pFilename)
                        {
                            buffer = new byte[extractor.ArchiveFileData[index].Size];
                            fileIndex = index;
                            break;
                        }
                    }
                    if (fileIndex > -1)
                    {
                        using (MemoryStream stream = new MemoryStream(buffer))
                        {
                            extractor.ExtractFile(fileIndex, stream);
                        }
                    }
                    else
                    {
                        LastErrorText = "File not found in archive";
                    }
                }
            }
            catch (Exception exception)
            {
                LastErrorText = $"EXCEPTION: {exception.Message}";
                return null;
            }

            return buffer;
        }


        // TODO : ExtractArchivedFileToStream() - support for encrypted archives
        /// <summary>
        /// Extracts an archived file as a byte vector
        /// </summary>
        /// <param name="pFilename">Name of the file to extract</param>
        /// <returns>Content of the file as a byte vector</returns>
        /// <remarks>CAVEAT: it does not support encrypted archives so far</remarks>
        public Stream ExtractArchivedFileToStream(string pFilename)
        {
            if (string.IsNullOrEmpty(pFilename = pFilename?.Trim().ToLower()))
            {
                LastErrorText = "Invalid file specification";
                return null;
            }

            LastErrorText = null;
            byte[] buffer = null;
            MemoryStream outputStream = null;
            try
            {
                SevenZipExtractor extractor = GetExtractor();
                if (extractor == null)
                    return null;

                using (extractor)
                {
                    int fileIndex = -1;
                    for (int index = 0; index < extractor.ArchiveFileData.Count; ++index)
                    {
                        string filename = extractor.ArchiveFileData[index].FileName.ToLower();
                        if (filename == pFilename)
                        {
                            buffer = new byte[extractor.ArchiveFileData[index].Size];
                            fileIndex = index;
                            break;
                        }
                    }
                    if (fileIndex > -1)
                    {
                        outputStream = new MemoryStream(buffer, true);
                        extractor.ExtractFile(fileIndex, outputStream);
                        outputStream.Seek(0, SeekOrigin.Begin);
                    }
                    else
                    {
                        LastErrorText = "File not found in archive";
                    }
                }
            }
            catch (Exception exception)
            {
                LastErrorText = $"EXCEPTION: {exception.Message}";
                return null;
            }

            return outputStream;
        }



        // ----------------------------------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Creates an archive from a folder
        /// </summary>
        /// <param name="pFolderToCompress">Folder to compress</param>
        /// <returns>Result of the operation</returns>
        public bool ArchiveFolder(string pFolderToCompress)
        {
            return ArchiveFolderX(pFolderToCompress, Password, EncryptHeaders, ArchiveFormat, CompressionLevel, CompressionMethod);
        }




        /// <summary>
        /// Creates an archive from a folder
        /// </summary>
        /// <param name="pFolderToCompress">Folder to compress</param>
        /// <param name="pPassword">Password (optional)</param>
        /// <param name="pEncryptHeaders">Encrypt headers of the archive</param>
        /// <param name="pArchiveFormat">Format of the archive</param>
        /// <param name="pCompressionLevel">Level of compression</param>
        /// <param name="pCompressionMethod">Compression method</param>
        /// <returns>Result of the operation</returns>
        public bool ArchiveFolderX(string pFolderToCompress, string pPassword = null,
            bool pEncryptHeaders = false,
            OutArchiveFormat pArchiveFormat = OutArchiveFormat.Zip,
            CompressionLevel pCompressionLevel = CompressionLevel.Normal,
            CompressionMethod pCompressionMethod = CompressionMethod.Default
         )
        {

            LastErrorText = null;
            if (string.IsNullOrEmpty(ArchiveName))
            {
                LastErrorText = "CreateArchive(): Invalid archive specification";
                return false;
            }

            if (string.IsNullOrEmpty(pFolderToCompress = pFolderToCompress?.Trim()) || !Directory.Exists(pFolderToCompress))
            {
                LastErrorText = "CreateArchive(): Invalid folder specification or folder not found";
                return false;
            }

            bool archiveOk = false;
            string backupFile = null;
            try
            {
                if (File.Exists(ArchiveName))
                {
                    backupFile = ArchiveName + ".bak";
                    File.Move(ArchiveName, backupFile);
                }

                SevenZipCompressor archiver = new SevenZipCompressor();
                archiver.CompressionMode = CompressionMode.Create;
                archiver.ArchiveFormat = pArchiveFormat;
                archiver.CompressionLevel = pCompressionLevel;
                archiver.CompressionMethod = pCompressionMethod;

                if (!string.IsNullOrEmpty(pPassword = pPassword?.Trim()))
                {
                    archiver.EncryptHeaders = pEncryptHeaders;
                    archiver.CompressDirectory(pFolderToCompress, ArchiveName, true, pPassword);
                }
                else
                    archiver.CompressDirectory(pFolderToCompress, ArchiveName);

                archiveOk = File.Exists(ArchiveName);
            }
            catch (Exception exception)
            {
                LastErrorText = $"CreateArchive(): {exception.Message}";
            }
            finally
            {
                if (backupFile != null)
                {
                    if (archiveOk)
                        File.Delete(backupFile);
                    else
                        File.Move(backupFile, ArchiveName);
                }
            }
            return archiveOk;
        }

    }


    public enum SevenZipArchiverSource
    {
        Undefined,
        File,
        Stream
    }


}
