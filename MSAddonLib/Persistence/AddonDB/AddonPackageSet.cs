using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml.Serialization;
using MSAddonLib.Domain;
using MSAddonLib.Domain.Addon;
using MSAddonLib.Util;
using MSAddonLib.Util.Persistence;

namespace MSAddonLib.Persistence.AddonDB
{
    [Serializable]
    public sealed class AddonPackageSet
    {

        public const string DefaultAddonPackageSetFileName = "AddonPackageSet.scat";

        public const string CurrentCatalogueVersion = "1.0";


        public MoviestormPaths MoviestormPaths
        {
            get { return _moviestormPaths; }
            set
            {
                _moviestormPaths = value;
                AddonSearchCriteria.ContentPacksPath = _moviestormPaths.ContentPacksPath;
                AssetSearchCriteria.ContentPacksPath = _moviestormPaths.ContentPacksPath;
            }
        }
        private MoviestormPaths _moviestormPaths;

        public string CatalogueVersion { get; set; } = CurrentCatalogueVersion;

        public List<string> AddonSources { get; set; }


        public List<AddonPackage> Addons { get; set; }

        public DateTime LastUpdate { get; set; }

        // ----------------------------------------------------------------------------------------------------------------

        public AddonPackageSet()
        {

        }


        public AddonPackageSet(List<string> pSources)
        {
            Initialize(null, pSources);
        }


        public AddonPackageSet(MoviestormPaths pMoviestormPaths, List<string> pSources)
        {
            Initialize(pMoviestormPaths, pSources);
        }


        private void Initialize(MoviestormPaths pMoviestormPaths, List<string> pSources)
        {
            string errorText;
            MoviestormPaths = pMoviestormPaths ?? AddonPersistenceUtils.GetMoviestormPaths(out errorText);
            AddonSources = pSources;
            LastUpdate = DateTime.Now;
        }

        // --------------------------------------------------------------------------------------------------------------------------------

        public static AddonPackageSet Load(out string pErrorText, string pFilename = null)
        {
            pErrorText = null;
            if (string.IsNullOrEmpty(pFilename = pFilename?.Trim()))
                pFilename = DefaultAddonPackageSetFileName;
            if (!File.Exists(pFilename))
            {
                return null;
            }

            AddonPackageSet addonCatalogue = new AddonPackageSet();

            try
            {
                XmlSerializer serializer = new XmlSerializer(addonCatalogue.GetType());
                using (StreamReader reader = new StreamReader(pFilename))
                {
                    addonCatalogue = (AddonPackageSet)serializer.Deserialize(reader);
                    reader.Close();
                }
            }
            catch (Exception exception)
            {
                pErrorText = Utils.GetExceptionExtendedMessage(exception);
                addonCatalogue = null;
            }
            return addonCatalogue;
        }





        /// <summary>
        /// Saves AddonPackageSet file
        /// </summary>
        /// <param name="pErrorText">Text of error, if any</param>
        /// <returns>Result of operation</returns>
        public bool Save(out string pErrorText, string pFilename = null)
        {
            pErrorText = null;
            if (string.IsNullOrEmpty(pFilename = pFilename?.Trim()))
                pFilename = DefaultAddonPackageSetFileName;
            try
            {
                XmlSerializer serializer = new XmlSerializer(this.GetType());
                using (StreamWriter writer = new StreamWriter(pFilename, false, Encoding.UTF8))
                {
                    serializer.Serialize(writer, this);
                    writer.Close();
                }
            }
            catch (Exception exception)
            {
                pErrorText = Utils.GetExceptionExtendedMessage(exception);
                return false;
            }
            return true;

        }


        // ------------------------------------------------------------------------------------------------------------------------------


        public bool AppendSource(string pSource)
        {
            return false;
        }


        public bool Append(AddonPackage pPackage, bool pRefresh = true)
        {
            if (Addons == null)
            {
                Addons = new List<AddonPackage>();
                Addons.Add(pPackage);
                LastUpdate = DateTime.Now;
                return true;
            }

            int index = GetIndexByLocation(pPackage.Location);
            if (index >= 0)
            {
                if (!pRefresh)
                    return false;
                Addons.RemoveAt(index);
            }
            Addons.Add(pPackage);
            LastUpdate = DateTime.Now;

            return true;
        }



        public bool DeleteByName(string pName, string pPublisher = null)
        {
            int index = GetIndexByName(pName, pPublisher);
            if (index >= 0)
            {
                Addons.RemoveAt(index);
                LastUpdate = DateTime.Now;
                return true;
            }

            return false;
        }



        public bool DeleteByLocation(string pLocation)
        {
            int index = GetIndexByLocation(pLocation);
            if (index >= 0)
            {
                Addons.RemoveAt(index);
                LastUpdate = DateTime.Now;
                return true;
            }

            return false;
        }




        public int DeletePath(string pPath)
        {
            if (string.IsNullOrEmpty(pPath = pPath.Trim().ToLower()) || (Addons == null))
                return 0;

            List<AddonPackage> addons = new List<AddonPackage>();
            foreach (AddonPackage addon in Addons)
            {
                if(!addon.Location.StartsWith(pPath, StringComparison.InvariantCultureIgnoreCase))
                    addons.Add(addon);
            }

            Addons = addons;
            LastUpdate = DateTime.Now;

            return 0;
        }


        public void Clear()
        {
            Addons = null;
            LastUpdate = DateTime.Now;

        }



        public void InitializeDatabase()
        {
            Addons = null;

            List<string> files = new List<string>();

            foreach (string folder in Directory.EnumerateDirectories(_moviestormPaths.ContentPacksPath, "*",
                SearchOption.TopDirectoryOnly))
            {
                files.Add(folder);
            }

            string moddersWorkshop = Path.Combine(_moviestormPaths.AddonsPath, "ModdersWorkshop").ToLower();
            foreach (string folder in Directory.EnumerateDirectories(_moviestormPaths.AddonsPath, "*",
                SearchOption.TopDirectoryOnly))
            {
                if (folder.ToLower() != moddersWorkshop)
                    files.Add(folder);
            }

            ProcessingFlags processingFlags = ProcessingFlags.AppendToAddonPackageSet |
                                              ProcessingFlags.AppendToAddonPackageSetForceRefresh;

            foreach (string argument in files)
            {
                IDiskEntity asset = DiskEntityHelper.GetEntity(argument, null, new NullReportWriter());

                if (asset == null)
                {
                    continue;
                }
                asset.CheckEntity(processingFlags);
            }

            LastUpdate = DateTime.Now;
        }



        // ----------------------------------------------------------------------------------------------------------------------

        public AddonPackage FindByLocation(string pLocation)
        {
            if (string.IsNullOrEmpty(pLocation = pLocation?.Trim().ToLower()) || (Addons == null))
                return null;

            foreach(AddonPackage addon in Addons)
                if (addon.Location.ToLower() == pLocation)
                    return addon;

            return null;
        }


        public AddonPackage FindByName(string pName, string pPublisher = null)
        {
            if (string.IsNullOrEmpty(pName = pName?.Trim().ToLower()) || (Addons == null))
                return null;

            pPublisher = pPublisher?.Trim().ToLower();

            if (!string.IsNullOrEmpty(pPublisher))
            {
                string qualifiedName = $"{pPublisher}.{pName}";
                foreach(AddonPackage addon in Addons)
                    if (addon.QualifiedName.ToLower() == qualifiedName)
                        return addon;

                return null;
            }

            foreach (AddonPackage addon in Addons)
                if (addon.Name.ToLower() == pName)
                    return addon;

            return null;
        }



        public int GetIndexByLocation(string pLocation)
        {
            if (string.IsNullOrEmpty(pLocation = pLocation?.Trim().ToLower()) || (Addons == null))
                return -1;

            for(int index = 0; index < Addons.Count; ++index)
                if (Addons[index].Location.ToLower() == pLocation)
                    return index;
            return -1;
        }



        public int GetIndexByName(string pName, string pPublisher = null)
        {
            if (string.IsNullOrEmpty(pName = pName?.Trim().ToLower()) || (Addons == null))
                return -1;

            pPublisher = pPublisher?.Trim().ToLower();

            if (!string.IsNullOrEmpty(pPublisher))
            {
                string qualifiedName = $"{pPublisher}.{pName}";
                for (int index = 0; index < Addons.Count; ++index)
                    if (Addons[index].QualifiedName.ToLower() == qualifiedName)
                        return index;

                return -1;
            }

            for (int index = 0; index < Addons.Count; ++index)
                if (Addons[index].Name.ToLower() == pName)
                    return index;

            return -1;
        }


        // ----------------------------------------------------------------------------------------------------------------


        public List<AssetSearchResultItem> Search(AddonSearchCriteria pAddonSearchCriteria, AssetSearchCriteria pAssetSearchCriteria)
        {
            List<AddonPackage> subSet = pAddonSearchCriteria == null ? Addons : SelectPackages(pAddonSearchCriteria);

            if(pAssetSearchCriteria == null)
                pAssetSearchCriteria = new AssetSearchCriteria(null, AddonAssetType.Any, null);

            // if (pAssetSearchCriteria != null)
                return SearchAsset(subSet, pAssetSearchCriteria);
            /*
            return (pAddonSearchCriteria == null)
                ? null
                : BuildAddonSearchResult(subSet);
                */
        }


        /*
        private List<AssetSearchResultItem> BuildAddonSearchResult(List<AddonPackage> subSet)
        {
            throw new NotImplementedException();
        }
        */


        public List<AddonPackage> SelectPackages(AddonSearchCriteria pCriteria)
        {
            if (Addons == null)
                return null;

            if (pCriteria == null)
                return Addons;

            List<AddonPackage> found = new List<AddonPackage>();
            foreach (AddonPackage addon in Addons)
            {
                if (pCriteria.IsMatch(addon))
                    found.Add(addon);
            }

            return found.Count == 0 ? null : found;
        }


        public List<AssetSearchResultItem> SearchAsset(List<AddonPackage> pPackages, AssetSearchCriteria pCriteria)
        {
            if ((pPackages == null) || (pPackages.Count == 0))
                return null;

            if (pCriteria == null)
                pCriteria = new AssetSearchCriteria(null, AddonAssetType.Any, null);

            List<AssetSearchResultItem> items = new List<AssetSearchResultItem>();

            foreach (AddonPackage package in pPackages)
            {
                List<AssetSearchResultItem> found = pCriteria.SearchAsset(package);
                if(found != null)
                    items.AddRange(found);
            }

            return items.Count > 0 ? items : null;
        }

    }
}
