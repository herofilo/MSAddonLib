using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MSAddonLib.Persistence.AddonDB
{
    public sealed class AssetSearchResultItem : ICloneable
    {
        public string AddonName { get; set; }

        public string AddonPublisher { get; set; }

        public bool Free { get; set; }

        public bool Installed { get; set; }

        public bool ContentPack { get; set; }

        public string Location { get; set; }

        // ........................

        public AddonAssetType AssetType
        {
            get { return _assetType; }
            set
            {
                _assetType = value;
                ulong typeValue = (ulong) _assetType;
                _assetTypeString = _assetType.ToString("X");
            }
        }

        private string _assetTypeString;
        private AddonAssetType _assetType;

        /*
         * Props: blank  pyroprop vehicle lamp auto etc
         * Bodypart: body head accessory
         * Verb: tipo
         * Decal: group
         * Material: floor ceil wall
         */
        public string AssetSubtype { get; set; }    

        public string Name { get; set; }

        public string Tags { get; set; }

        /*
         * Bodyparts: morphable
         * Props: flags variants
         *
         */
        public string ExtraInfo { get; set; }       // verbs: 

        public string SortKey => $"{AddonName}^{_assetTypeString}^{AssetSubtype}^{Name}";



        // --------------------------------------------------------------------------------------------------------------

        public object Clone()
        {
            return MemberwiseClone();
        }
    }
}
