using MSAddonLib.Persistence;

namespace MSAddonLib.Domain
{
    public static class AssetHelper
    {
        // ----------------------------------------------------------------------------------------------

        public static IAsset GetAsset(string pAssetPath, IReportWriter pReportWriter)
        {
            IAsset asset = null;

            if(pReportWriter == null)
                pReportWriter = new NullReportWriter();

            switch (AssetBase.GetAssetType(pAssetPath))
            {
                case AssetType.Folder:
                    asset = new AssetFolder(pAssetPath, pReportWriter);                   
                    break;
                case AssetType.Archive:
                    asset = new AssetArchive(pAssetPath, pReportWriter);                    
                    break;
                case AssetType.SketchupFile:
                    asset = new AssetSketchup(pAssetPath, pReportWriter);
                    break;
                case AssetType.AddonFile:
                    asset = new AssetAddon(pAssetPath, pReportWriter);                    
                    break;
            }

            return asset;
        }


    }
}
