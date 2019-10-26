using MSAddonLib.Persistence;

namespace MSAddonLib.Domain
{
    public static class DiskEntityHelper
    {
        // ----------------------------------------------------------------------------------------------

        public static IDiskEntity GetEntity(string pEntityPath, string pArchivedPath, IReportWriter pReportWriter)
        {
            IDiskEntity diskEntity = null;

            if(pReportWriter == null)
                pReportWriter = new NullReportWriter();

            switch (DiskEntityBase.GetEntityType(pEntityPath))
            {
                case DiskEntityType.Folder:
                    diskEntity = new DiskEntityFolder(pEntityPath, pArchivedPath, pReportWriter);                   
                    break;
                case DiskEntityType.AddonFolder:
                    diskEntity = new DiskEntityAddonFolder(pEntityPath, pArchivedPath, pReportWriter);
                    break;
                case DiskEntityType.Archive:
                    diskEntity = new DiskEntityArchive(pEntityPath, pArchivedPath, pReportWriter);                    
                    break;
                case DiskEntityType.SketchupFile:
                    diskEntity = new DiskEntitySketchup(pEntityPath, pArchivedPath, pReportWriter);
                    break;
                case DiskEntityType.AddonFile:
                    diskEntity = new DiskEntityAddon(pEntityPath, pArchivedPath, pReportWriter);                    
                    break;
            }

            return diskEntity;
        }


    }
}
