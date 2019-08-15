using System;
using System.IO;
using MSAddonLib.Persistence;

namespace MSAddonLib.Domain
{
    public class AssetBase // : IAsset
    {

        public const string ErrorTokenString = "#!?";

        public AssetType AssetType { get; protected set; } = AssetType.Unknown;


        public string AssetPath { get; protected set; }

        public string AbsolutePath { get; protected set; }

        public string Name { get; protected set; }

        protected IReportWriter ReportWriter = null;


        // -----------------------------------------------------------------------------------------

        public AssetBase(string pAssetPath, IReportWriter pReportWriter)
        {
            string absolutePath, name;
            AssetType = GetAssetType(pAssetPath, out absolutePath, out name);
            AssetPath = pAssetPath;
            if (AssetType != AssetType.Unknown)
            {
                AbsolutePath = absolutePath;
                Name = name;
            }

            ReportWriter = pReportWriter ?? new NullReportWriter();
        }




        // ----------------------------------------------------------------------------------------------

        public static AssetType GetAssetType(string pAssetPath)
        {
            string absolutePath, name;
            return GetAssetType(pAssetPath, out absolutePath, out name);
        }


        protected static AssetType GetAssetType(string pAssetPath, out string pAbsolutePath, out string pName)
        {
            AssetType assetType = AssetType.Unknown;
            pAbsolutePath = pName = null;

            try
            {
                if (File.Exists(pAssetPath))
                {
                    string extension = Path.GetExtension(pAssetPath);
                    switch (extension?.Trim().ToLower())
                    {
                        case ".addon":
                            assetType = AssetType.AddonFile;
                            break;
                        case ".zip":
                        case ".rar":
                        case ".7z":
                            assetType = AssetType.Archive;
                            break;
                        case ".skp":
                            assetType = AssetType.SketchupFile;
                            break;
                        default:
                            return AssetType.Unknown;
                    }
                    
                } else if (Directory.Exists(pAssetPath))
                {
                    assetType = AssetType.Folder;
                }
                else
                {
                    return AssetType.Unknown;
                }

                pAbsolutePath = Path.GetFullPath(pAssetPath);
                pName = Path.GetFileName(pAssetPath);

            }
            catch (Exception exception)
            {
                assetType = AssetType.Unknown;
            }

            return assetType;
        }


        /*
        public virtual bool CheckAsset(ProcessingFlags pProcessingFlags, out string pReport)
        {
            pReport = null;
            return false;
        }
        */

    }
}
