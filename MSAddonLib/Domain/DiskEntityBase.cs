using System;
using System.IO;
using MSAddonLib.Domain.Addon;
using MSAddonLib.Persistence;

namespace MSAddonLib.Domain
{
    public class DiskEntityBase // : IDiskEntity
    {

        public const string ErrorTokenString = "#!?";

        public DiskEntityType DiskEntityType { get; protected set; } = DiskEntityType.Unknown;


        public string EntityPath { get; protected set; }

        public string AbsolutePath { get; protected set; }

        public string Name { get; protected set; }

        public bool InsideArchive { get; protected set; }

        protected IReportWriter ReportWriter = null;


        // -----------------------------------------------------------------------------------------

        public DiskEntityBase(string pEntityPath, bool pInsideArchive, IReportWriter pReportWriter)
        {
            string absolutePath, name;
            DiskEntityType = GetEntityType(pEntityPath, out absolutePath, out name);
            EntityPath = pEntityPath;
            if (DiskEntityType != DiskEntityType.Unknown)
            {
                AbsolutePath = absolutePath;
                Name = name;
            }

            InsideArchive = pInsideArchive;
            ReportWriter = pReportWriter ?? new NullReportWriter();
        }




        // ----------------------------------------------------------------------------------------------

        public static DiskEntityType GetEntityType(string pEntityPath)
        {
            string absolutePath, name;
            return GetEntityType(pEntityPath, out absolutePath, out name);
        }


        protected static DiskEntityType GetEntityType(string pEntityPath, out string pAbsolutePath, out string pName)
        {
            DiskEntityType diskEntityType = DiskEntityType.Unknown;
            pAbsolutePath = pName = null;

            try
            {
                if (File.Exists(pEntityPath))
                {
                    string extension = Path.GetExtension(pEntityPath);
                    switch (extension?.Trim().ToLower())
                    {
                        case ".addon":
                            diskEntityType = DiskEntityType.AddonFile;
                            break;
                        case ".zip":
                        case ".rar":
                        case ".7z":
                            diskEntityType = DiskEntityType.Archive;
                            break;
                        case ".skp":
                            diskEntityType = DiskEntityType.SketchupFile;
                            break;
                        default:
                            return DiskEntityType.Unknown;
                    }
                    
                } else if (Directory.Exists(pEntityPath))
                {
                    diskEntityType = 
                        DiskEntityFolder.IsAddonFolder(pEntityPath) 
                            ? DiskEntityType.AddonFolder 
                            : DiskEntityType.Folder;
                }
                else
                {
                    return DiskEntityType.Unknown;
                }

                pAbsolutePath = Path.GetFullPath(pEntityPath);
                pName = Path.GetFileName(pEntityPath);

            }
            catch (Exception exception)
            {
                diskEntityType = DiskEntityType.Unknown;
            }

            return diskEntityType;
        }


        public static bool IsAddonFolder(string pEntityPath)
        {
            return File.Exists(Path.Combine(pEntityPath, AddonPackage.SignatureFilename)) &&
                   File.Exists(Path.Combine(pEntityPath, AddonPackage.AssetDataFilename));
            // && Directory.Exists(Path.Combine(pEntityPath, "Data"));
        }

    }
}
