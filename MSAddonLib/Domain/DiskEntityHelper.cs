using MSAddonLib.Persistence;

namespace MSAddonLib.Domain
{
    public static class DiskEntityHelper
    {
        // ----------------------------------------------------------------------------------------------

        public static IDiskEntity GetEntity(string pEntityPath, bool pInsideArchive, IReportWriter pReportWriter)
        {
            IDiskEntity diskEntity = null;

            if(pReportWriter == null)
                pReportWriter = new NullReportWriter();

            switch (DiskEntityBase.GetEntityType(pEntityPath))
            {
                case DiskEntityType.Folder:
                    diskEntity = new DiskEntityFolder(pEntityPath, pInsideArchive, pReportWriter);                   
                    break;
                case DiskEntityType.AddonFolder:
                    diskEntity = new DiskEntityAddonFolder(pEntityPath, pInsideArchive, pReportWriter);
                    break;
                case DiskEntityType.Archive:
                    diskEntity = new DiskEntityArchive(pEntityPath, pInsideArchive, pReportWriter);                    
                    break;
                case DiskEntityType.SketchupFile:
                    diskEntity = new DiskEntitySketchup(pEntityPath, pInsideArchive, pReportWriter);
                    break;
                case DiskEntityType.AddonFile:
                    diskEntity = new DiskEntityAddon(pEntityPath, pInsideArchive, pReportWriter);                    
                    break;
            }

            return diskEntity;
        }


    }
}
