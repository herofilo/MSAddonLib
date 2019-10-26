using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using MSAddonLib.Util.Persistence;
using SevenZip;

namespace MSAddonLib.Domain.Addon
{
    [Serializable]
    public sealed class AddonPackageSource
    {
        public AddonPackageSourceType SourceType { get; set; } = AddonPackageSourceType.Invalid;

        [XmlIgnore]
        public SevenZipArchiver Archiver { get; private set; }

        public string SourcePath { get; set; }

        public string ArchivedPath { get; set; }

        // -----------------------------------------------------------------------------------------------------------------------------------------------


        public AddonPackageSource()
        {

        }


        public AddonPackageSource(string pSourcePath, string pArchivedPath = null)
        {
            if (string.IsNullOrEmpty(pSourcePath = pSourcePath.Trim()))
                throw new Exception("Not a valid path specification");

            pSourcePath = Path.GetFullPath(pSourcePath);

            ArchivedPath = pArchivedPath;
            if (Directory.Exists(pSourcePath))
            {
                SourceType = AddonPackageSourceType.Folder;
                SourcePath = pSourcePath;
                return;
            }

            SevenZipArchiver archiver = new SevenZipArchiver(pSourcePath);
            List<ArchiveFileInfo> files;
            if (archiver.ArchivedFileList(out files) < 0)
            {
                string errorText = string.IsNullOrEmpty(archiver.LastErrorText)
                    ? "Not a valid archive"
                    : archiver.LastErrorText;
                throw new Exception(errorText);
            }

            Archiver = archiver;
            SourceType = AddonPackageSourceType.Archiver;
            SourcePath = pSourcePath;
        }


        public AddonPackageSource(SevenZipArchiver pArchiver, string pArchivedPath = null)
        {
            if (pArchiver == null)
                throw new Exception("Not a valid Archiver specification");

            Archiver = pArchiver;
            SourceType = AddonPackageSourceType.Archiver;
            if (pArchiver.Source == SevenZipArchiverSource.File)
                SourcePath = pArchiver.ArchiveName;
            ArchivedPath = pArchivedPath;
        }
    }



    public enum AddonPackageSourceType
    {
        Invalid,
        Folder,
        Archiver
    }


}
