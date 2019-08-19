﻿//------------------------------------------------------------------------------
// <auto-generated>
//     Este código fue generado por una herramienta.
//     Versión de runtime:4.0.30319.42000
//
//     Los cambios en este archivo podrían causar un comportamiento incorrecto y se perderán si
//     se vuelve a generar el código.
// </auto-generated>
//------------------------------------------------------------------------------

using System;
using System.IO;
using System.Xml.Serialization;

// 
// Este código fuente fue generado automáticamente por xsd, Versión=4.6.1055.0.
// 

namespace MSAddonLib.Domain.AssetFiles
{


    /// <comentarios/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.6.1055.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false, ElementName = "vector")]
    public partial class Verbs
    {

        private vectorMscopethingsverbsSoloPropAnimVerb[] mscopethingsverbsSoloPropAnimVerbField;

        private vectorHeldPropVerb[] heldPropVerbField;

        private vectorPropVerb[] propVerbField;

        private vectorSoloVerb[] soloVerbField;

        private vectorMutualStemVerb[] mutualStemVerbField;

        private vectorMutualVerb[] mutualVerbField;

        /// <comentarios/>
        [System.Xml.Serialization.XmlElementAttribute("mscope.things.verbs.SoloPropAnimVerb")]
        public vectorMscopethingsverbsSoloPropAnimVerb[] mscopethingsverbsSoloPropAnimVerb
        {
            get { return this.mscopethingsverbsSoloPropAnimVerbField; }
            set { this.mscopethingsverbsSoloPropAnimVerbField = value; }
        }

        /// <comentarios/>
        [System.Xml.Serialization.XmlElementAttribute("HeldPropVerb")]
        public vectorHeldPropVerb[] HeldPropVerb
        {
            get { return this.heldPropVerbField; }
            set { this.heldPropVerbField = value; }
        }

        /// <comentarios/>
        [System.Xml.Serialization.XmlElementAttribute("PropVerb")]
        public vectorPropVerb[] PropVerb
        {
            get { return this.propVerbField; }
            set { this.propVerbField = value; }
        }

        /// <comentarios/>
        [System.Xml.Serialization.XmlElementAttribute("SoloVerb")]
        public vectorSoloVerb[] SoloVerb
        {
            get { return this.soloVerbField; }
            set { this.soloVerbField = value; }
        }

        /// <comentarios/>
        [System.Xml.Serialization.XmlElementAttribute("MutualStemVerb")]
        public vectorMutualStemVerb[] MutualStemVerb
        {
            get { return this.mutualStemVerbField; }
            set { this.mutualStemVerbField = value; }
        }

        /// <comentarios/>
        [System.Xml.Serialization.XmlElementAttribute("MutualVerb")]
        public vectorMutualVerb[] MutualVerb
        {
            get { return this.mutualVerbField; }
            set { this.mutualVerbField = value; }
        }


        /// <summary>
        /// Creates a Verbs instance from an existent Verbs file
        /// </summary>
        /// <param name="pFilename">Path to the Verbs file</param>
        /// <param name="pErrorText">Text of error, if any</param>
        /// <returns>Verbs instance, or null if error</returns>
        public static Verbs Load(string pFilename, out string pErrorText)
        {
            pErrorText = null;
            if (!File.Exists(pFilename))
            {
                pErrorText = "Verbs.Load(): File not found";
                return null;
            }

            Verbs verbs = new Verbs();

            try
            {
                XmlSerializer serializer = new XmlSerializer(verbs.GetType());
                using (StreamReader reader = new StreamReader(pFilename))
                {
                    verbs = (Verbs)serializer.Deserialize(reader);
                    reader.Close();
                }
            }
            catch (Exception exception)
            {
                pErrorText = $"EXCEPTION while deserializing Verbs: {exception.Message} - {exception.InnerException?.Message}";
                verbs = null;
            }

            return verbs;
        }


        /// <summary>
        /// Creates a Verbs instance from a XML string
        /// </summary>
        /// <param name="pText">XML string</param>
        /// <param name="pErrorText">Text of error, if any</param>
        /// <returns>Verbs instance, or null if error</returns>
        public static Verbs LoadFromString(string pText, out string pErrorText)
        {
            pErrorText = null;
            if (string.IsNullOrEmpty(pText = pText?.Trim()))
            {
                pErrorText = "Verbs.LoadFromString(): No input text";
                return null;
            }

            Verbs verbs = new Verbs();

            try
            {
                XmlSerializer serializer = new XmlSerializer(verbs.GetType());
                using (TextReader reader = new StringReader(pText))
                {
                    verbs = (Verbs)serializer.Deserialize(reader);
                    reader.Close();
                }
            }
            catch (Exception exception)
            {
                pErrorText = $"EXCEPTION while deserializing Verbs: {exception.Message} - {exception.InnerException?.Message}";
                verbs = null;
            }

            return verbs;
        }




    }

    /// <comentarios/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.6.1055.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class vectorMscopethingsverbsSoloPropAnimVerb
    {

        private string modelField;

        private string nameField;

        private string objectClassField;

        private string animField;

        /// <comentarios/>
        public string model
        {
            get { return this.modelField; }
            set { this.modelField = value; }
        }

        /// <comentarios/>
        public string name
        {
            get { return this.nameField; }
            set { this.nameField = value; }
        }

        /// <comentarios/>
        public string objectClass
        {
            get { return this.objectClassField; }
            set { this.objectClassField = value; }
        }

        /// <comentarios/>
        public string anim
        {
            get { return this.animField; }
            set { this.animField = value; }
        }
    }

    /// <comentarios/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.6.1055.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class vectorHeldPropVerb
    {

        private string[] itemsField;

        private ItemsChoiceType[] itemsElementNameField;

        /// <comentarios/>
        [System.Xml.Serialization.XmlElementAttribute("activityClass", typeof(string))]
        [System.Xml.Serialization.XmlElementAttribute("animA", typeof(string))]
        [System.Xml.Serialization.XmlElementAttribute("modelA", typeof(string))]
        [System.Xml.Serialization.XmlElementAttribute("modelB", typeof(string))]
        [System.Xml.Serialization.XmlElementAttribute("name", typeof(string))]
        [System.Xml.Serialization.XmlElementAttribute("objectClass", typeof(string))]
        [System.Xml.Serialization.XmlElementAttribute("subjectClass", typeof(string))]
        [System.Xml.Serialization.XmlChoiceIdentifierAttribute("ItemsElementName")]
        public string[] Items
        {
            get { return this.itemsField; }
            set { this.itemsField = value; }
        }

        /// <comentarios/>
        [System.Xml.Serialization.XmlElementAttribute("ItemsElementName")]
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public ItemsChoiceType[] ItemsElementName
        {
            get { return this.itemsElementNameField; }
            set { this.itemsElementNameField = value; }
        }
    }

    /// <comentarios/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.6.1055.0")]
    [System.SerializableAttribute()]
    [System.Xml.Serialization.XmlTypeAttribute(IncludeInSchema = false)]
    public enum ItemsChoiceType
    {

        /// <comentarios/>
        activityClass,

        /// <comentarios/>
        animA,

        /// <comentarios/>
        modelA,

        /// <comentarios/>
        modelB,

        /// <comentarios/>
        name,

        /// <comentarios/>
        objectClass,

        /// <comentarios/>
        subjectClass,
    }

    /// <comentarios/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.6.1055.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class vectorPropVerb
    {

        private string modelAField;

        private string modelBField;

        private string animAField;

        private string animBField;

        private bool rot180SubjField;

        private bool rot180SubjFieldSpecified;

        private string nameField;

        private string subjectClassField;

        private string objectClassField;

        /// <comentarios/>
        public string modelA
        {
            get { return this.modelAField; }
            set { this.modelAField = value; }
        }

        /// <comentarios/>
        public string modelB
        {
            get { return this.modelBField; }
            set { this.modelBField = value; }
        }

        /// <comentarios/>
        public string animA
        {
            get { return this.animAField; }
            set { this.animAField = value; }
        }

        /// <comentarios/>
        public string animB
        {
            get { return this.animBField; }
            set { this.animBField = value; }
        }

        /// <comentarios/>
        public bool rot180Subj
        {
            get { return this.rot180SubjField; }
            set { this.rot180SubjField = value; }
        }

        /// <comentarios/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool rot180SubjSpecified
        {
            get { return this.rot180SubjFieldSpecified; }
            set { this.rot180SubjFieldSpecified = value; }
        }

        /// <comentarios/>
        public string name
        {
            get { return this.nameField; }
            set { this.nameField = value; }
        }

        /// <comentarios/>
        public string subjectClass
        {
            get { return this.subjectClassField; }
            set { this.subjectClassField = value; }
        }

        /// <comentarios/>
        public string objectClass
        {
            get { return this.objectClassField; }
            set { this.objectClassField = value; }
        }
    }

    /// <comentarios/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.6.1055.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class vectorSoloVerb
    {

        private string modelField;

        private string animField;

        private string nameField;

        private string subjectClassField;

        /// <comentarios/>
        public string model
        {
            get { return this.modelField; }
            set { this.modelField = value; }
        }

        /// <comentarios/>
        public string anim
        {
            get { return this.animField; }
            set { this.animField = value; }
        }

        /// <comentarios/>
        public string name
        {
            get { return this.nameField; }
            set { this.nameField = value; }
        }

        /// <comentarios/>
        public string subjectClass
        {
            get { return this.subjectClassField; }
            set { this.subjectClassField = value; }
        }
    }

    /// <comentarios/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.6.1055.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class vectorMutualStemVerb
    {

        private string animStemField;

        private bool sittingVerbField;

        private string nameField;

        private string subjectClassField;

        private string objectClassField;

        /// <comentarios/>
        public string animStem
        {
            get { return this.animStemField; }
            set { this.animStemField = value; }
        }

        /// <comentarios/>
        public bool sittingVerb
        {
            get { return this.sittingVerbField; }
            set { this.sittingVerbField = value; }
        }

        /// <comentarios/>
        public string name
        {
            get { return this.nameField; }
            set { this.nameField = value; }
        }

        /// <comentarios/>
        public string subjectClass
        {
            get { return this.subjectClassField; }
            set { this.subjectClassField = value; }
        }

        /// <comentarios/>
        public string objectClass
        {
            get { return this.objectClassField; }
            set { this.objectClassField = value; }
        }
    }

    /// <comentarios/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.6.1055.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class vectorMutualVerb
    {

        private string modelAField;

        private string modelBField;

        private string animAField;

        private string animBField;

        private string nameField;

        private string subjectClassField;

        private string objectClassField;

        /// <comentarios/>
        public string modelA
        {
            get { return this.modelAField; }
            set { this.modelAField = value; }
        }

        /// <comentarios/>
        public string modelB
        {
            get { return this.modelBField; }
            set { this.modelBField = value; }
        }

        /// <comentarios/>
        public string animA
        {
            get { return this.animAField; }
            set { this.animAField = value; }
        }

        /// <comentarios/>
        public string animB
        {
            get { return this.animBField; }
            set { this.animBField = value; }
        }

        /// <comentarios/>
        public string name
        {
            get { return this.nameField; }
            set { this.nameField = value; }
        }

        /// <comentarios/>
        public string subjectClass
        {
            get { return this.subjectClassField; }
            set { this.subjectClassField = value; }
        }

        /// <comentarios/>
        public string objectClass
        {
            get { return this.objectClassField; }
            set { this.objectClassField = value; }
        }
    }

}