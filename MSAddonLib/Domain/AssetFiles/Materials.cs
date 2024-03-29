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


namespace MSAddonLib.Domain.AssetFiles
{

    // 
    // Este código fuente fue generado automáticamente por xsd, Versión=4.6.1055.0.
    // 


    /// <comentarios/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.6.1055.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false, ElementName = "vector")]
    public partial class Materials
    {

        private vectorMaterial[] materialField;

        /// <comentarios/>
        [System.Xml.Serialization.XmlElementAttribute("material")]
        public vectorMaterial[] material
        {
            get { return this.materialField; }
            set { this.materialField = value; }
        }


        // ----------------------------------------------------------------------------------------------------------------------------------------------------------------


        /// <summary>
        /// Creates a Materials instance from an existent materials description file
        /// </summary>
        /// <param name="pFilename">Path to the materials description file</param>
        /// <param name="pErrorText">Text of error, if any</param>
        /// <returns>Materials instance, or null if error</returns>
        public static Materials Load(string pFilename, out string pErrorText)
        {
            pErrorText = null;
            if (!File.Exists(pFilename))
            {
                pErrorText = "Materials.Load(): File not found";
                return null;
            }

            Materials materials = new Materials();

            try
            {
                XmlSerializer serializer = new XmlSerializer(materials.GetType());
                using (StreamReader reader = new StreamReader(pFilename))
                {
                    materials = (Materials)serializer.Deserialize(reader);
                    reader.Close();
                }
            }
            catch (Exception exception)
            {
                pErrorText = $"EXCEPTION while deserializing Materials: {exception.Message} - {exception.InnerException?.Message}";
                materials = null;
            }

            return materials;
        }


        /// <summary>
        /// Creates a Materials instance from a XML string
        /// </summary>
        /// <param name="pText">XML string</param>
        /// <param name="pErrorText">Text of error, if any</param>
        /// <returns>Materials instance, or null if error</returns>
        public static Materials LoadFromString(string pText, out string pErrorText)
        {
            pErrorText = null;
            if (string.IsNullOrEmpty(pText = pText?.Trim()))
            {
                pErrorText = "Materials.LoadFromString(): No input text";
                return null;
            }

            Materials materials = new Materials();

            try
            {
                XmlSerializer serializer = new XmlSerializer(materials.GetType());
                using (TextReader reader = new StringReader(pText))
                {
                    materials = (Materials)serializer.Deserialize(reader);
                    reader.Close();
                }
            }
            catch (Exception exception)
            {
                pErrorText = $"EXCEPTION while deserializing Materials: {exception.Message} - {exception.InnerException?.Message}";
                materials = null;
            }

            return materials;
        }


    }



    /// <comentarios/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.6.1055.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
    public partial class vectorMaterial
    {

        private string nameField;

        private string ambientColorField;

        private string diffuseColorField;

        private string emissiveColorField;

        private string specularColorField;

        private decimal shininessField;

        private string[][] mapsField;

        private vectorMaterialFlags flagsField;

        private vectorMaterialParameters parametersField;

        /// <comentarios/>
        public string name
        {
            get { return this.nameField; }
            set { this.nameField = value; }
        }

        /// <comentarios/>
        public string ambientColor
        {
            get { return this.ambientColorField; }
            set { this.ambientColorField = value; }
        }

        /// <comentarios/>
        public string diffuseColor
        {
            get { return this.diffuseColorField; }
            set { this.diffuseColorField = value; }
        }

        /// <comentarios/>
        public string emissiveColor
        {
            get { return this.emissiveColorField; }
            set { this.emissiveColorField = value; }
        }

        /// <comentarios/>
        public string specularColor
        {
            get { return this.specularColorField; }
            set { this.specularColorField = value; }
        }

        /// <comentarios/>
        public decimal shininess
        {
            get { return this.shininessField; }
            set { this.shininessField = value; }
        }

        /// <comentarios/>
        [System.Xml.Serialization.XmlArrayItemAttribute("entry", IsNullable = false)]
        [System.Xml.Serialization.XmlArrayItemAttribute(IsNullable = false, NestingLevel = 1)]
        public string[][] maps
        {
            get { return this.mapsField; }
            set { this.mapsField = value; }
        }

        /// <comentarios/>
        public vectorMaterialFlags flags
        {
            get { return this.flagsField; }
            set { this.flagsField = value; }
        }

        /// <comentarios/>
        public vectorMaterialParameters parameters
        {
            get { return this.parametersField; }
            set { this.parametersField = value; }
        }
    }

    /// <comentarios/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.6.1055.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class vectorMaterialFlags
    {

        private string stringField;

        /// <comentarios/>
        public string @string
        {
            get { return this.stringField; }
            set { this.stringField = value; }
        }
    }

    /// <comentarios/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.6.1055.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class vectorMaterialParameters
    {

        private string[] entryField;

        /// <comentarios/>
        [System.Xml.Serialization.XmlArrayItemAttribute(IsNullable = false)]
        public string[] entry
        {
            get { return this.entryField; }
            set { this.entryField = value; }
        }
    }
}