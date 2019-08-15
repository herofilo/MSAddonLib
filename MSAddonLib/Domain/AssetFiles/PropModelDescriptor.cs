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
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false, ElementName = "ModelDescriptor")]
    public partial class PropModelDescriptor
    {

        private string nameField;

        private string modelField;

        private string autoAnimField;

        private string typeField;

        private string[] tagsField;

        private string defaultVariantField;

        private ModelDescriptorEntry[] attributesField;

        private object sitBonesField;

        /// <comentarios/>
        public string name
        {
            get { return this.nameField; }
            set { this.nameField = value; }
        }

        /// <comentarios/>
        public string model
        {
            get { return this.modelField; }
            set { this.modelField = value; }
        }

        /// <comentarios/>
        public string autoAnim
        {
            get { return this.autoAnimField; }
            set { this.autoAnimField = value; }
        }

        /// <comentarios/>
        public string type
        {
            get { return this.typeField; }
            set { this.typeField = value; }
        }

        /// <comentarios/>
        [System.Xml.Serialization.XmlArrayItemAttribute(IsNullable = false)]
        public string[] tags
        {
            get { return this.tagsField; }
            set { this.tagsField = value; }
        }

        /// <comentarios/>
        public string defaultVariant
        {
            get { return this.defaultVariantField; }
            set { this.defaultVariantField = value; }
        }

        /// <comentarios/>
        [System.Xml.Serialization.XmlArrayItemAttribute("entry", IsNullable = false)]
        public ModelDescriptorEntry[] attributes
        {
            get { return this.attributesField; }
            set { this.attributesField = value; }
        }

        /// <comentarios/>
        public object sitBones
        {
            get { return this.sitBonesField; }
            set { this.sitBonesField = value; }
        }


        /// <summary>
        /// Creates a PropModelDescriptor instance from a XML string
        /// </summary>
        /// <param name="pText">XML string</param>
        /// <returns>PropModelDescriptor instance, or null if error</returns>
        public static PropModelDescriptor LoadFromString(string pText, out string pErrorText)
        {
            pErrorText = null;
            if (string.IsNullOrEmpty(pText = pText?.Trim()))
            {
                pErrorText = "PropModelDescriptor.LoadFromString(): No input text";
                return null;
            }

            PropModelDescriptor modelDescriptor = new PropModelDescriptor();

            try
            {
                XmlSerializer serializer = new XmlSerializer(modelDescriptor.GetType());
                using (TextReader reader = new StringReader(pText))
                {
                    modelDescriptor = (PropModelDescriptor)serializer.Deserialize(reader);
                    reader.Close();
                }
            }
            catch (Exception exception)
            {
                pErrorText = $"EXCEPTION while deserializing PropModelDescriptor: {exception.Message} - {exception.InnerException?.Message}";
                modelDescriptor = null;
            }

            return modelDescriptor;
        }
    }


    // >>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>

    /// <comentarios/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.6.1055.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class ModelDescriptorEntry
    {

        private string[] stringField;

        private bool booleanField;

        private bool booleanFieldSpecified;

        /// <comentarios/>
        [System.Xml.Serialization.XmlElementAttribute("string")]
        public string[] @string
        {
            get { return this.stringField; }
            set { this.stringField = value; }
        }

        /// <comentarios/>
        public bool boolean
        {
            get { return this.booleanField; }
            set { this.booleanField = value; }
        }

        /// <comentarios/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool booleanSpecified
        {
            get { return this.booleanFieldSpecified; }
            set { this.booleanFieldSpecified = value; }
        }
    }

    // >>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>

    }