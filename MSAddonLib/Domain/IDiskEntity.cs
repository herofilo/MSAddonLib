namespace MSAddonLib.Domain
{
    public interface IDiskEntity
    {
        /// <summary>
        /// Type of disk entity
        /// </summary>
        DiskEntityType DiskEntityType { get; }

        /// <summary>
        /// Path to the entity
        /// </summary>
        string EntityPath { get; }

        /// <summary>
        /// Absolute path to the entity
        /// </summary>
        string AbsolutePath { get; }

        /// <summary>
        /// Name of the entity
        /// </summary>
        string Name { get; }

        // --------------------------------------------------------------------------

        bool CheckEntity(ProcessingFlags pProcessingFlags, string pNamePrinted = null);

    }
}
