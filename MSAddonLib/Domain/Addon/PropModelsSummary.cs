using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using MSAddonLib.Domain.AssetFiles;
using MSAddonLib.Util.Persistence;

namespace MSAddonLib.Domain.Addon
{
    public sealed class PropModelsSummary
    {
        public PropModelSumProps Props { get; private set; }

        private SevenZipArchiver Archiver { get; set; }

        private string TempFolderPath { get; set; }

        private List<PropModelItem> PropModelItems { get; set; }

        private bool LoadAnimations { get; set; }

        private bool _initialized = false;




        // ------------------------------------------------------------------------------------------------------

        public PropModelsSummary()
        {

        }

        public PropModelsSummary(SevenZipArchiver pArchiver, string pTempFolderPath, List<PropModelItem> pPropModelItems, bool pLoadAnimations = true)
        {

            Archiver = pArchiver;
            TempFolderPath = pTempFolderPath?.Trim();
            PropModelItems = pPropModelItems;
            LoadAnimations = pLoadAnimations;
        }


        // ---------------------------------------------------------------------------------------------------------


        public bool PopulateSummary(out string pErrorText, SevenZipArchiver pArchiver = null, string pTempFolderPath = null, List<PropModelItem> pPropModelItems = null)
        {
            pErrorText = null;
            if (_initialized)
                return true;

            if (!PopulateSummaryPreChecks(ref pArchiver, ref pTempFolderPath, ref pPropModelItems, out pErrorText))
                return false;

            bool isOk = true;
            string mftFile = Path.Combine(pTempFolderPath, AddonPackage.AssetDataFilename);
            try
            {
                pArchiver.ArchivedFilesExtract(pTempFolderPath, new List<string>() { AddonPackage.AssetDataFilename });

                SevenZipArchiver mftArchiver = new SevenZipArchiver(mftFile);

                foreach (PropModelItem item in pPropModelItems)
                {
                    if (Props == null)
                        Props = new PropModelSumProps(mftArchiver);

                    if (!Props.AppendBodyModelItem(item, out pErrorText))
                    {
                        isOk = false;
                        break;
                    }
                }
            }
            catch (Exception exception)
            {
                pErrorText = $"PropModelsSummary.PopulateSummary() EXCEPTION: {exception.Message}";
                isOk = false;
            }
            finally
            {
                File.Delete(mftFile);
            }

            _initialized = isOk;

            return isOk;
        }


        private bool PopulateSummaryPreChecks(ref SevenZipArchiver pArchiver, ref string pTempFolderPath, ref List<PropModelItem> pPropModelItems, out string pErrorText)
        {
            pErrorText = null;
            if (pArchiver == null)
            {
                if (Archiver == null)
                {
                    pErrorText = "No archiver specification";
                    return false;
                }

                pArchiver = Archiver;
            }
            else
                Archiver = pArchiver;

            pTempFolderPath = pTempFolderPath?.Trim();
            if (string.IsNullOrEmpty(pTempFolderPath))
            {
                if (string.IsNullOrEmpty(TempFolderPath))
                {
                    pErrorText = "No temporary folder specification";
                    return false;
                }

                pTempFolderPath = TempFolderPath;
            }
            else
                TempFolderPath = pTempFolderPath;

            if (!Directory.Exists(pTempFolderPath))
            {
                pErrorText = "Temporary folder not found";
                return false;
            }


            if (pPropModelItems == null)
            {
                if (PropModelItems == null)
                {
                    pErrorText = "No prop models specification";
                    return false;
                }

                pPropModelItems = PropModelItems;
            }
            else
                PropModelItems = pPropModelItems;

            return true;
        }


        public override string ToString()
        {
            if (!_initialized || (Props == null) || (Props?.Props.Count == 0))
                return "";


            StringBuilder textBuilder = new StringBuilder();
            foreach (PropModelSumProp prop in Props.Props)
            {
                string propName = prop.PropName;
                if (prop.PropType.ToLower() != "prop")
                    propName += $" ({prop.PropType})";

                if (prop.AttributesString != null)
                    propName += $" [{prop.AttributesString}]";
                if (prop.MultiPart)
                    propName += " [Multipart]";
                textBuilder.AppendLine($"{propName}");
                if (prop.Variants != null)
                {
                    textBuilder.AppendLine($"    Variants (default: {prop.DefaultVariant}):");
                    foreach (string variant in prop.Variants)
                        textBuilder.AppendLine($"        {variant}");
                }

                if (!string.IsNullOrEmpty(prop.AutoAnimation))
                    textBuilder.AppendLine($"    Autoanimation: {prop.AutoAnimation}");
                else
                {
                    if (prop.Animations != null)
                    {
                        if (LoadAnimations)
                        {
                            textBuilder.AppendLine("    Animations:");
                            foreach (string animation in prop.Animations)
                            {
                                textBuilder.AppendLine($"        {animation}");
                            }
                        }
                        else
                        {
                            textBuilder.AppendLine($"    Animations: {prop.Animations.Count} - See Verbs");
                        }
                    }
                }
            }
            return textBuilder.ToString();
        }

    }


    // -------------------------------------------------------------------------------------------------------------------------------


    public sealed class PropModelSumProps
    {
        public List<PropModelSumProp> Props { get; private set; }

        public SevenZipArchiver Archiver { get; private set; }


        public PropModelSumProps(SevenZipArchiver pArchiver)
        {
            Archiver = pArchiver;
        }



        public PropModelSumProp SearchProp(string pName)
        {
            if (string.IsNullOrEmpty(pName = pName?.Trim().ToLower()))
            {
                return null;
            }

            if ((Props == null) || (Props.Count == 0))
                return null;

            foreach (PropModelSumProp item in Props)
                if (item.PropName.Trim().ToLower() == pName)
                    return item;

            return null;
        }

        public bool AppendBodyModelItem(PropModelItem pPropModelItem, out string pErrorText)
        {
            // string puppetName = pBodyModelItem.PuppetName;
            PropModelSumProp prop = null;
            if (Props != null)
            {
                prop = SearchProp(pPropModelItem.Name);
            }
            else
                Props = new List<PropModelSumProp>();

            if (prop == null)
            {
                prop = new PropModelSumProp(pPropModelItem.Name, Archiver);
                Props.Add(prop);
            }

            return prop.AppendData(pPropModelItem, out pErrorText);
        }
    }


    // ---------------------------------------------------------------------------------------------------------------------

    public sealed class PropModelSumProp
    {
        public string PropName { get; set; }

        public string PropType { get; set; } = "Prop";

        public string Description { get; set; }

        public string AutoAnimation { get; set; }

        public string DefaultVariant { get; set; }

        public List<string> Variants { get; set; }

        public bool MultiPart { get; set; }

        public PropModelAttributes Attributes { get; set; }

        public string AttributesString { get; set; }

        public List<string> Animations { get; set; }


        private string _RootPath;

        private int _RootPathLength;

        private bool _initialData = false;

        private SevenZipArchiver _Archiver;


        // ---------------------------------------------------------------------------------

        public PropModelSumProp(string pName, SevenZipArchiver pArchiver)
        {
            _Archiver = pArchiver;
            PropName = pName?.Trim();
            // ************ CAVEAT ************
            _RootPath = $"data/props/{PropName.ToLower()}/";
            _RootPathLength = _RootPath.Length;
        }


        public bool AppendData(PropModelItem pPropModelItem, out string pErrorText)
        {
            pErrorText = null;
            MultiPart = (pPropModelItem.Parts != null) && (pPropModelItem.Parts.Count > 1);
            if ((pPropModelItem.Templates != null) && (pPropModelItem.Templates.Count > 1))
            {
                Variants = new List<string>();
                foreach (Template item in pPropModelItem.Templates)
                {
                    string variantName = item.Name.Remove(0, _RootPathLength);
                    int dotIndex = variantName.LastIndexOf('.');
                    variantName = variantName.Remove(dotIndex);
                    Variants.Add(variantName);
                }
            }

            if (!InsertAnimations(pPropModelItem.Animations, out pErrorText))
                return false;

            return ReadPropDescription(out pErrorText);
        }




        private bool InsertAnimations(List<string> pAnimations, out string pErrorText)
        {
            pErrorText = null;
            if ((pAnimations == null) || (pAnimations.Count == 0))
                return true;
            if (Animations == null)
                Animations = new List<string>();

            string prefix = "animations/";
            int animationsPrefixLen = prefix.Length;

            bool insertOk = false;
            try
            {
                foreach (string item in pAnimations)
                {
                    string animation = (item.ToLower().StartsWith(prefix)) ? item.Remove(0, animationsPrefixLen) : item;
                    int dotIndex = animation.LastIndexOf('.');
                    animation = animation.Remove(dotIndex);
                    if (!Animations.Contains(animation))
                        Animations.Add(animation);
                }

                insertOk = true;
            }
            catch (Exception exception)
            {
                pErrorText = $"PropModelSumProp.InsertAnimations(), EXCEPTION: {exception.Message}";
            }

            return insertOk;
        }



        private bool ReadPropDescription(out string pErrorText)
        {
            pErrorText = null;
            string descriptorFile = Path.Combine(_RootPath.Replace("/", "\\"), "DESCRIPTOR");

            string descriptorContents = _Archiver.ExtractArchivedFileToString(descriptorFile);
            if (descriptorContents == null)
            {
                if (!_Archiver.FileExists(descriptorFile))
                {
                    return true;
                }

                pErrorText = _Archiver.LastErrorText;
                return false;
            }

            PropModelDescriptor modelDescriptor =
                PropModelDescriptor.LoadFromString(descriptorContents, out pErrorText);
            if (modelDescriptor == null)
                return false;

            PropType = modelDescriptor.type;
            AutoAnimation = modelDescriptor.autoAnim;

            if (Variants != null)
                DefaultVariant = modelDescriptor.defaultVariant;


            if (modelDescriptor.attributes != null)
            {
                AttributesString = "";
                foreach (ModelDescriptorEntry entry in modelDescriptor.attributes)
                {
                    SetAttribute(entry);
                }

                AttributesString = (AttributesString.Length == 0) ? null : AttributesString.Trim();
            }

            return true;
        }

        private void SetAttribute(ModelDescriptorEntry pEntry)
        {
            // excludes bone_held
            if (!pEntry.booleanSpecified || !pEntry.boolean)
                return;

            string attributeName = pEntry.@string[0].Trim().ToLower();
            switch (attributeName)
            {
                case "navigable": Attributes |= PropModelAttributes.Navigable; AttributesString += "Navigable "; break;
                case "mobile": Attributes |= PropModelAttributes.Mobile; AttributesString += "Mobile "; break;
                case "holdable": Attributes |= PropModelAttributes.Held; AttributesString += "Held "; break;
            }
        }


        [Flags]
        public enum PropModelAttributes
        {
            Held = 1,
            Mobile = 2,
            Navigable = 4
        }



    }
}
