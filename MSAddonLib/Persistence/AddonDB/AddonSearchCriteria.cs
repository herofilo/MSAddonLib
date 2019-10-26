using System.Text.RegularExpressions;
using MSAddonLib.Domain.Addon;
using MSAddonLib.Util.Persistence;

namespace MSAddonLib.Persistence.AddonDB
{
    public sealed class AddonSearchCriteria : SearchCriteriaBase
    {
        public static string ContentPacksPath
        {
            get { return _contentPacksPath; }
            set { _contentPacksPath = value?.ToLower(); }
        }
        private static string _contentPacksPath;


        public string Name { get; private set; }

        private readonly Regex _nameRegex = null;

        public string Publisher { get; private set; }

        private readonly Regex _publisherRegex = null;

        public bool? Installed { get; private set; }

        public bool? ContentPack { get; private set; }

        public string Location { get; private set; }

        private readonly Regex _locationRegex = null;


        public AddonSearchCriteria(string pName, string pPublisher, bool? pInstalled, bool? pContentPack,
            string pLocation) 
        {
            Name = pName;
            _nameRegex = CreateMultiValuedRegex(Name);

            Publisher = pPublisher;
            _publisherRegex = CreateMultiValuedRegex(Publisher);

            Installed = pInstalled;
            ContentPack = pContentPack;
            Location = pLocation?.Trim().ToLower();
            if (!string.IsNullOrEmpty(Location))
                _locationRegex = new Regex($"{Location}", RegexOptions.IgnoreCase);

            if (ContentPacksPath == null)
            {
                string errorText;
                ContentPacksPath = AddonPersistenceUtils.GetMoviestormPaths(out errorText)?.ContentPacksPath;
            }

        }



        public bool IsMatch(AddonPackage pPackage)
        {
            if ((_nameRegex != null) && !_nameRegex.IsMatch(pPackage.Name))
                return false;

            if ((_publisherRegex != null) && !_publisherRegex.IsMatch(pPackage.Publisher))
                return false;

            if (Installed.HasValue)
            {
                if (((pPackage.AddonFormat == AddonPackageFormat.InstalledFolder) && !Installed.Value) ||
                    ((pPackage.AddonFormat == AddonPackageFormat.PackageFile && Installed.Value)))
                    return false;
            }

            if (ContentPack.HasValue)
            {
                bool isContentPack = pPackage.Location.ToLower().StartsWith(ContentPacksPath);
                if (isContentPack != ContentPack.Value)
                    return false;
            }

            if ((_locationRegex != null) && !_locationRegex.IsMatch(pPackage.Location))
                return false;

            return true;
        }
    }
}