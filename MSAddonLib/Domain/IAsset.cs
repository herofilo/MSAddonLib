namespace MSAddonLib.Domain
{
    public interface IAsset
    {

        AssetType AssetType { get; }

        string AssetPath { get; }


        string AbsolutePath { get; }

        string Name { get; }

        // --------------------------------------------------------------------------

        bool CheckAsset(ProcessingFlags pProcessingFlags, string pNamePrinted = null);

    }
}
