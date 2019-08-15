using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MSAddonLib.Domain.AssetFiles;
using MSAddonLib.Util.Persistence;
using SevenZip;

namespace MSAddonLib.Domain.Addon
{
    public sealed class VerbsSummary
    {

        public const string VerbsFilename = "Data\\Verbs";

        public VerbCollection Verbs { get; private set; }

        private SevenZipArchiver Archiver { get; set; }


        private bool _initialized = false;

        // ------------------------------------------------------------------------------------------------------

        public VerbsSummary()
        {

        }

        public VerbsSummary(SevenZipArchiver pArchiver)
        {

            Archiver = pArchiver;
        }


        // ---------------------------------------------------------------------------------------------------------


        public bool PopulateSummary(out string pErrorText, SevenZipArchiver pArchiver = null)
        {
            pErrorText = null;
            if (_initialized)
                return true;

            if (!PopulateSummaryPreChecks(ref pArchiver, out pErrorText))
                return false;

            bool isOk = true;
            try
            {
                VerbCollection verbCollection = new VerbCollection();
                verbCollection = CreateVerbsSummary(pArchiver, verbCollection, out pErrorText);
                Verbs = CreateVerbsSummaryFromStateMachine(pArchiver, verbCollection);
            }
            catch (Exception exception)
            {
                pErrorText = $"VerbsSummary.PopulateSummary() EXCEPTION: {exception.Message}";
                isOk = false;
            }


            _initialized = isOk;

            return isOk;
        }



        private bool PopulateSummaryPreChecks(ref SevenZipArchiver pArchiver, out string pErrorText)
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

            return true;
        }


        private VerbCollection CreateVerbsSummary(SevenZipArchiver pArchiver, VerbCollection pVerbCollection, out string pErrorText)
        {
            pErrorText = null;
            
            string verbsContents = pArchiver.ExtractArchivedFileToString(VerbsFilename);
            if (verbsContents == null)
            {
                if (!pArchiver.FileExists(VerbsFilename))
                {
                    return pVerbCollection;
                }

                pErrorText = pArchiver.LastErrorText;
                return pVerbCollection;
            }

            Verbs verbsDescriptor = AssetFiles.Verbs.LoadFromString(verbsContents, out pErrorText);
            if (verbsDescriptor == null)
                return pVerbCollection;

            AppendPuppetSoloVerbs(pVerbCollection, verbsDescriptor.SoloVerb);
            AppendPropSoloVerbs(pVerbCollection, verbsDescriptor.mscopethingsverbsSoloPropAnimVerb);
            AppendHeldVerbs(pVerbCollection, verbsDescriptor.HeldPropVerb);
            AppendInteractivePropVerbs(pVerbCollection, verbsDescriptor.PropVerb);
            AppendPuppetMutualVerbs(pVerbCollection, verbsDescriptor.MutualVerb, verbsDescriptor.MutualStemVerb);

            return pVerbCollection;
        }



        private void AppendPuppetSoloVerbs(VerbCollection pVerbCollection, vectorSoloVerb[] pVerbs)
        {
            if ((pVerbs == null) || (pVerbs.Length == 0))
                return;

            List<VerbSummaryItem> puppetSoloVerbs = new List<VerbSummaryItem>();
            foreach (vectorSoloVerb verbDescriptor in pVerbs)
            {
                VerbSummaryItem verb = new VerbSummaryItem();
                verb.VerbName = verbDescriptor.name;
                verb.VerbType = "Puppet Solo";
                verb.ModelA = verbDescriptor.model;
                verb.SetSortKey(verb.ModelA, verb.VerbName);
                VerbSummaryItem oldItem = SearchEntry(puppetSoloVerbs, verb.SortKey);
                if(oldItem == null)
                    puppetSoloVerbs.Add(verb);
                else
                    oldItem.Iterations++;
                
            }
            pVerbCollection.PuppetSoloVerbs = puppetSoloVerbs.OrderBy(o => o.SortKey).ToList();
        }

        private VerbSummaryItem SearchEntry(List<VerbSummaryItem> pVerbsList, string pVerbKey)
        {
            foreach(VerbSummaryItem verb in pVerbsList)
                if (verb.SortKey == pVerbKey)
                    return verb;
            return null;
        }

        private void AppendPropSoloVerbs(VerbCollection pVerbCollection, vectorMscopethingsverbsSoloPropAnimVerb[] pVerbs)
        {
            if ((pVerbs == null) || (pVerbs.Length == 0))
                return;

            List<VerbSummaryItem> propSoloVerbs = new List<VerbSummaryItem>();
            foreach (vectorMscopethingsverbsSoloPropAnimVerb verbDescriptor in pVerbs)
            {
                VerbSummaryItem verb = new VerbSummaryItem();
                verb.VerbName = verbDescriptor.name;
                verb.VerbType = "Prop Solo";
                verb.ModelA = verbDescriptor.model;
                verb.SetSortKey(verb.ModelA, verb.VerbName);
                VerbSummaryItem oldItem = SearchEntry(propSoloVerbs, verb.SortKey);
                if (oldItem == null)
                    propSoloVerbs.Add(verb);
                else
                    oldItem.Iterations++;


            }
            pVerbCollection.PropSoloVerbs = propSoloVerbs.OrderBy(o => o.SortKey).ToList();
        }

        private void AppendHeldVerbs(VerbCollection pVerbCollection, vectorHeldPropVerb[] pVerbs)
        {
            if ((pVerbs == null) || (pVerbs.Length == 0))
                return;

            List<VerbSummaryItem> heldPropsVerbs = new List<VerbSummaryItem>();
            foreach (vectorHeldPropVerb verbDescriptor in pVerbs)
            {
                VerbSummaryItem verb = new VerbSummaryItem();
                int elementCount = verbDescriptor.ItemsElementName.Length;
                for (int index = 0; index < elementCount; ++index)
                {
                    string value = verbDescriptor.Items[index];
                    switch (verbDescriptor.ItemsElementName[index])
                    {
                        case ItemsChoiceType.name: verb.VerbName = value; break;
                        case ItemsChoiceType.modelA: verb.ModelA = value; break;
                        case ItemsChoiceType.modelB: verb.ModelB = value; break;
                    }
                }
                verb.VerbType = "Held Prop";
                verb.SetSortKey(verb.ModelB, verb.ModelA, verb.VerbName);

                VerbSummaryItem oldItem = SearchEntry(heldPropsVerbs, verb.SortKey);
                if (oldItem == null)
                    heldPropsVerbs.Add(verb);
                else
                    oldItem.Iterations++;
            }
            pVerbCollection.HeldPropsVerbs = heldPropsVerbs.OrderBy(o => o.SortKey).ToList();
        }

        private void AppendInteractivePropVerbs(VerbCollection pVerbCollection, vectorPropVerb[] pVerbs)
        {
            if ((pVerbs == null) || (pVerbs.Length == 0))
                return;

            List<VerbSummaryItem> interactivePropsVerbs = new List<VerbSummaryItem>();
            foreach (vectorPropVerb verbDescriptor in pVerbs)
            {
                VerbSummaryItem verb = new VerbSummaryItem();
                verb.VerbName = verbDescriptor.name;
                verb.VerbType = "Interactive Prop";
                verb.ModelA = verbDescriptor.modelA;
                verb.ModelB = verbDescriptor.modelB;
                verb.SetSortKey(verb.ModelB, verb.ModelA, verb.VerbName);

                VerbSummaryItem oldItem = SearchEntry(interactivePropsVerbs, verb.SortKey);
                if (oldItem == null)
                    interactivePropsVerbs.Add(verb);
                else
                    oldItem.Iterations++;
            }
            pVerbCollection.InteractivePropsVerbs = interactivePropsVerbs.OrderBy(o => o.SortKey).ToList();
        }

        private void AppendPuppetMutualVerbs(VerbCollection pVerbCollection, vectorMutualVerb[] pVerbs, vectorMutualStemVerb[] pStemVerbs)
        {
            bool anyMutualVerb = (pVerbs != null) && (pVerbs.Length > 0);
            bool anyMutualStemVerb = ((pStemVerbs != null) && (pStemVerbs.Length > 0));
            if (!anyMutualVerb && !anyMutualStemVerb)
                return;
            List<VerbSummaryItem> puppetMutualVerbs = null;
            if (anyMutualVerb)
            {
                puppetMutualVerbs = new List<VerbSummaryItem>();
                foreach (vectorMutualVerb verbDescriptor in pVerbs)
                {
                    VerbSummaryItem verb = new VerbSummaryItem();
                    verb.VerbName = verbDescriptor.name;
                    verb.VerbType = "Puppets Interaction";
                    verb.ModelA = verbDescriptor.modelA;
                    verb.ModelB = verbDescriptor.modelB;
                    verb.SetSortKey(verb.ModelA, verb.ModelB, verb.VerbName);

                    VerbSummaryItem oldItem = SearchEntry(puppetMutualVerbs, verb.SortKey);
                    if (oldItem == null)
                        puppetMutualVerbs.Add(verb);
                    else
                        oldItem.Iterations++;
                }
            }

            if (anyMutualStemVerb)
            {

                if (puppetMutualVerbs == null)
                    puppetMutualVerbs = new List<VerbSummaryItem>();
                foreach (vectorMutualStemVerb verbDescriptor in pStemVerbs)
                {
                    VerbSummaryItem verb = new VerbSummaryItem();
                    verb.VerbName = verbDescriptor.name;
                    verb.VerbType = "Puppets Interaction";
                    verb.SetSortKey(null, null, verb.VerbName);

                    VerbSummaryItem oldItem = SearchEntry(puppetMutualVerbs, verb.SortKey);
                    if (oldItem == null)
                        puppetMutualVerbs.Add(verb);
                    else
                        oldItem.Iterations++;
                }
            }

            pVerbCollection.PuppetMutualVerbs = puppetMutualVerbs.OrderBy(o => o.SortKey).ToList();
        }


        // - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - 

        private VerbCollection CreateVerbsSummaryFromStateMachine(SevenZipArchiver pArchiver, VerbCollection pVerbCollection)
        {
            string stateMachineFilePath = "Data\\StateMachine";
            string stateMachineContents = pArchiver.ExtractArchivedFileToString(stateMachineFilePath);
            if (stateMachineContents == null)
            {
                return pVerbCollection;
            }

            string errorText;
            StateMachine stateMachine = StateMachine.LoadFromString(stateMachineContents, out errorText);
            if (stateMachine == null)
                return pVerbCollection;

            const string gaitPrefix = "gaits/";
            int gaitPrefixLen = gaitPrefix.Length;
            const string gesturePrefix = "gestures/";
            int gesturePrefixLen = gesturePrefix.Length;

            List<Tuple<string, string>> animationFiles = null;
            List<VerbSummaryItem> gaits = null;
            List<VerbSummaryItem> gestures = null;

            foreach (StateMachineNameMap nameMap in stateMachine.NameMap)
            {
                if (nameMap.Items == null)
                    continue;

                foreach (object item in nameMap.Items)
                {
                    Type transitionType = item.GetType();
                    if (transitionType == typeof(StateMachineNameMapState))
                    {
                        break;
                    }

                    if (transitionType == typeof(StateMachineNameMapAnimTransition))
                    {
                        StateMachineNameMapAnimTransition transition = (StateMachineNameMapAnimTransition)item;
                        VerbSummaryItem gaitAnimation = new VerbSummaryItem();
                        string animationFilepath = null;
                        for (int index = 0; index < transition.ItemsElementName.Length; ++index)
                        {
                            switch (transition.ItemsElementName[index])
                            {
                                case TransitionsItemsChoiceType.menuPath: gaitAnimation.VerbName = (string)transition.Items[index]; break;
                                case TransitionsItemsChoiceType.name: animationFilepath = ((string)transition.Items[index])?.ToLower(); break;
                            }
                        }

                        if ((gaitAnimation.VerbName?.ToLower() ?? "").StartsWith(gaitPrefix))
                        {
                            gaitAnimation.VerbName = gaitAnimation.VerbName?.Remove(0, gaitPrefixLen);
                            gaitAnimation.VerbType = "Gait Animation";

                            if (animationFiles == null)
                                animationFiles = GetAnimationFiles(pArchiver);
                            gaitAnimation.ModelA = GetAnimationPuppetModel(animationFilepath, animationFiles);


                            gaitAnimation.SetSortKey(gaitAnimation.VerbName);
                            if (gaits == null)
                                gaits = new List<VerbSummaryItem>();

                            VerbSummaryItem oldItem = SearchEntry(gaits, gaitAnimation.SortKey);
                            if (oldItem == null)
                                gaits.Add(gaitAnimation);
                            else
                                oldItem.Iterations++;
                        }

                        continue;
                    }
                    if (transitionType == typeof(StateMachineNameMapGestureTransition))
                    {
                        StateMachineNameMapGestureTransition transition = (StateMachineNameMapGestureTransition)item;
                        string name = transition.menuPath;
                        if ((transition.menuPath?.ToLower() ?? "").StartsWith(gesturePrefix))
                        {
                            name = name.Remove(0, gesturePrefixLen);
                        }

                        VerbSummaryItem gestureAnimation = new VerbSummaryItem()
                        {
                            VerbName = name,
                            VerbType = "Gesture Animation"
                        };

                        string animationFilepath = transition.name;
                        if (animationFiles == null)
                            animationFiles = GetAnimationFiles(pArchiver);
                        gestureAnimation.ModelA = GetAnimationPuppetModel(animationFilepath, animationFiles);

                        gestureAnimation.SetSortKey(gestureAnimation.VerbName);
                        if (gestures == null)
                            gestures = new List<VerbSummaryItem>();

                        VerbSummaryItem oldItem = SearchEntry(gestures, gestureAnimation.SortKey);
                        if (oldItem == null)
                            gestures.Add(gestureAnimation);
                        else
                            oldItem.Iterations++;
                    }
                }
            }

            if(gaits != null)
                pVerbCollection.Gaits = gaits.OrderBy(o => o.SortKey).ToList();
            if (gestures != null)
                pVerbCollection.Gestures = gestures.OrderBy(o => o.SortKey).ToList();
            return pVerbCollection;
        }



        private List<Tuple<string, string>> GetAnimationFiles(SevenZipArchiver pArchiver)
        {
            List<ArchiveFileInfo> archiveFileInfos;
            pArchiver.ArchivedFileList(out archiveFileInfos);
            if ((archiveFileInfos == null) || (archiveFileInfos.Count == 0))
                return null;

            const string prefixPuppets = "data\\puppets\\";
            int prefixPuppetsLength = prefixPuppets.Length;

            const string prefixProps = "data\\props\\";
            int prefixPropsLength = prefixProps.Length;

            const string prefixAnimations = "animations\\";

            // List<string> animationFiles = new List<string>();

            List<Tuple<string, string>> animationFiles = new List<Tuple<string, string>>();
            foreach (ArchiveFileInfo item in archiveFileInfos)
            {
                string fileNameLower = item.FileName.ToLower();
                if (fileNameLower.EndsWith(".caf"))
                {
                    string puppet;
                    int index;
                    bool weird = false;
                    if (fileNameLower.StartsWith(prefixPuppets))
                    {
                        puppet = item.FileName.Remove(0, prefixPuppetsLength);
                        index = puppet.IndexOf("\\");
                    }
                    else if (fileNameLower.StartsWith(prefixProps))
                    {
                        puppet = item.FileName.Remove(0, prefixPropsLength);
                        index = puppet.IndexOf("\\animations", StringComparison.InvariantCultureIgnoreCase);
                        weird = true;
                    }
                    else
                        continue;
                    puppet = puppet.Remove(index) + (weird ? "!?" : "");
                    index = fileNameLower.IndexOf(prefixAnimations);
                    string relativePath = fileNameLower.Remove(0, index + prefixAnimations.Length).Replace("\\", "/").Replace(".caf", "");
                    animationFiles.Add(new Tuple<string, string>(relativePath, puppet.Replace("\\", "/")));
                }
            }

            if (animationFiles.Count == 0)
                return null;

            return animationFiles.OrderBy(o => o.Item1).ToList();
        }

        private string GetAnimationPuppetModel(string pAnimationFilepath, List<Tuple<string, string>> pAnimationFiles)
        {
            if (string.IsNullOrEmpty(pAnimationFilepath = pAnimationFilepath?.Trim().ToLower()) || (pAnimationFiles == null))
                return "^";

            List<string> puppets = new List<string>();
            bool firsTry = true;
            while (true)
            {
                foreach (Tuple<string, string> file in pAnimationFiles)
                {
                    int comparison = string.Compare(file.Item1, pAnimationFilepath, StringComparison.InvariantCulture);
                    if (comparison > 0)
                        break;
                    if (comparison == 0)
                    {
                        puppets.Add(file.Item2);
                    }
                }

                if (puppets.Count == 0)
                {
                    if (!pAnimationFilepath.EndsWith("_reverse") || !firsTry)
                        return "?";
                    pAnimationFilepath = pAnimationFilepath.Replace("_reverse", "");
                    firsTry = false;
                }
                else
                    break;
            }

            if (puppets.Count == 1)
                return puppets[0];
            if (puppets.Count == 2)
            {
                if(((puppets[0] == "Male01") && (puppets[1] == "Female01")) ||
                    ((puppets[1] == "Male01") && (puppets[0] == "Female01")))
                    return "*";
            }

            StringBuilder puppetsStringBuilder = new StringBuilder();
            foreach (string item in puppets)
                puppetsStringBuilder.Append($"{item} ");

            return puppetsStringBuilder.ToString().Trim();
        }



        // ------------------------------------------------------------------------------------------------------------------


        public string WriteReport(bool pListWeirdGestureGaits)
        {
            if (!_initialized || (Verbs == null) || !Verbs.HasData)
                return "";

            StringBuilder textBuilder = new StringBuilder();

            VerbCollectionFlags flags = Verbs.Flags;
            if (flags.HasFlag(VerbCollectionFlags.PuppetSoloVerbs))
            {
                textBuilder.AppendLine("Puppet Solo Verbs (Postures):");
                foreach (VerbSummaryItem verb in Verbs.PuppetSoloVerbs)
                {
                    string model = verb.ModelA ?? "*";
                    string text = (verb.Iterations == 1)
                        ? $"    {model} : {verb.VerbName}"
                        : $"    {model} : {verb.VerbName}    (x{verb.Iterations})";
                    textBuilder.AppendLine(text);
                }

            }

            if (flags.HasFlag(VerbCollectionFlags.Gestures))
            {
                textBuilder.AppendLine("Gestures:");
                int weirdVerbs = 0;
                foreach (VerbSummaryItem verb in Verbs.Gestures)
                {
                    if (!pListWeirdGestureGaits && verb.ModelA.EndsWith("!?"))
                    {
                        weirdVerbs++;
                        continue;
                    }

                    string text = (verb.Iterations == 1)
                        ? $"    [{verb.ModelA}] : {verb.VerbName}"
                        : $"    [{verb.ModelA}] : {verb.VerbName}    (x{verb.Iterations})";
                    textBuilder.AppendLine(text);
                }
                if (weirdVerbs > 0)
                    textBuilder.AppendLine($"    {weirdVerbs} Improper Gestures (for Props)");
            }

            if (flags.HasFlag(VerbCollectionFlags.Gaits))
            {
                textBuilder.AppendLine("Gaits:");
                int weirdVerbs = 0;
                foreach (VerbSummaryItem verb in Verbs.Gaits)
                {
                    if (!pListWeirdGestureGaits && verb.ModelA.EndsWith("!?"))
                    {
                        weirdVerbs++;
                        continue;
                    }

                    string text = (verb.Iterations == 1)
                        ? $"    [{verb.ModelA}] : {verb.VerbName}"
                        : $"    [{verb.ModelA}] : {verb.VerbName}    (x{verb.Iterations})";
                    textBuilder.AppendLine(text);
                }
                if (weirdVerbs > 0)
                    textBuilder.AppendLine($"    {weirdVerbs} Improper Gestures (for Props)");
            }

            if (flags.HasFlag(VerbCollectionFlags.PropSoloVerbs))
            {
                textBuilder.AppendLine("Prop Solo Verbs:");
                foreach (VerbSummaryItem verb in Verbs.PropSoloVerbs)
                {
                    string text = (verb.Iterations == 1)
                        ? $"    [{verb.ModelA}] : {verb.VerbName}"
                        : $"    [{verb.ModelA}] : {verb.VerbName}    (x{verb.Iterations})";
                    textBuilder.AppendLine(text);
                }
            }

            if (flags.HasFlag(VerbCollectionFlags.HeldPropsVerbs))
            {
                textBuilder.AppendLine("Held Prop Verbs:");
                foreach (VerbSummaryItem verb in Verbs.HeldPropsVerbs)
                {
                    string modelA = verb.ModelA ?? "*";
                    string text = (verb.Iterations == 1)
                        ? $"    {verb.ModelB} [{modelA}] : {verb.VerbName}"
                        : $"    {verb.ModelB} [{modelA}] : {verb.VerbName}    (x{verb.Iterations})";
                    textBuilder.AppendLine(text);
                }
            }

            if (flags.HasFlag(VerbCollectionFlags.InteractivePropsVerbs))
            {
                textBuilder.AppendLine("Interactive Prop Verbs:");
                foreach (VerbSummaryItem verb in Verbs.InteractivePropsVerbs)
                {
                    string modelA = verb.ModelA ?? "*";
                    string text = (verb.Iterations == 1)
                        ? $"    {verb.ModelB} [{modelA}] : {verb.VerbName}"
                        : $"    {verb.ModelB} [{modelA}] : {verb.VerbName}    (x{verb.Iterations})";
                    textBuilder.AppendLine(text);
                }
            }

            if (flags.HasFlag(VerbCollectionFlags.PuppetMutualVerbs))
            {
                textBuilder.AppendLine("Puppet Interaction Verbs:");
                foreach (VerbSummaryItem verb in Verbs.PuppetMutualVerbs)
                {
                    string modelA = verb.ModelA ?? "*";
                    string modelB = verb.ModelB ?? "*";
                    string text = (verb.Iterations == 1)
                        ? $"    {modelA} -> {modelB} : {verb.VerbName}"
                        : $"    {modelA} -> {modelB} : {verb.VerbName}    (x{verb.Iterations})";
                    textBuilder.AppendLine(text);
                }
            }

            return textBuilder.ToString();
        }

    }


    public sealed class VerbCollection
    {
        public List<VerbSummaryItem> PropSoloVerbs { get; set; }

        public List<VerbSummaryItem> PuppetSoloVerbs { get; set; }

        public List<VerbSummaryItem> HeldPropsVerbs { get; set; }

        public List<VerbSummaryItem> InteractivePropsVerbs { get; set; }

        public List<VerbSummaryItem> PuppetMutualVerbs { get; set; }

        public List<VerbSummaryItem> Gaits { get; set; }

        public List<VerbSummaryItem> Gestures { get; set; }


        public VerbCollectionFlags Flags
        {
            get
            {
                VerbCollectionFlags flags = VerbCollectionFlags.None;
                if ((PropSoloVerbs != null) && (PropSoloVerbs.Count > 0))
                    flags |= VerbCollectionFlags.PropSoloVerbs;

                if((PuppetSoloVerbs != null) && (PuppetSoloVerbs.Count > 0))
                    flags |= VerbCollectionFlags.PuppetSoloVerbs;

                if ((HeldPropsVerbs != null) && (HeldPropsVerbs.Count > 0))
                    flags |= VerbCollectionFlags.HeldPropsVerbs;

                if ((InteractivePropsVerbs != null) && (InteractivePropsVerbs.Count > 0))
                    flags |= VerbCollectionFlags.InteractivePropsVerbs;

                if ((PuppetMutualVerbs != null) && (PuppetMutualVerbs.Count > 0))
                    flags |= VerbCollectionFlags.PuppetMutualVerbs;

                if ((Gaits != null) && (Gaits.Count > 0))
                    flags |= VerbCollectionFlags.Gaits;

                if ((Gestures != null) && (Gestures.Count > 0))
                    flags |= VerbCollectionFlags.Gestures;

                return flags;
            }
        }


        public bool HasData => (Flags != VerbCollectionFlags.None);
    }


    [Flags]
    public enum VerbCollectionFlags
    {
        None = 0,
        PropSoloVerbs = 0x0001,
        PuppetSoloVerbs = 0x002,
        HeldPropsVerbs = 0x0004,
        InteractivePropsVerbs = 0x0008,
        PuppetMutualVerbs = 0x0010,
        Gaits = 0x0020,
        Gestures = 0x0040
    }



    public sealed class VerbSummaryItem
    {
        public string VerbName { get; set; }

        public string VerbType { get; set; }

        public string ModelA { get; set; }

        public string ModelB { get; set; }

        public int Iterations { get; set; } = 1;

        public string SortKey { get; set; }


        public void SetSortKey(string pString0, string pString1 = "xDont'Use", string pString2 = "xDont'Use")
        {
            string key = string.IsNullOrEmpty(pString0 = pString0?.Trim().ToLower()) ? "*^" : pString0 + "^";
            if (pString1 == "xDont'Use")
            {
                SortKey = key + "^^";
                return;
            }
            key += string.IsNullOrEmpty(pString1 = pString1?.Trim().ToLower()) ? "*^" : pString1 + "^";
            if (pString2 == "xDont'Use")
            {
                SortKey = key + "^";
                return;
            }
            SortKey = key + (string.IsNullOrEmpty(pString2 = pString2?.Trim().ToLower()) ? "*^" : pString2 + "^");
        }

    }



}
