using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using MSAddonLib.Domain;
using MSAddonLib.Domain.Addon;
using MSAddonLib.Util;

namespace SerializeAddonPackage
{
    class Program
    {
        static void Main(string[] args)
        {
            string errorText;
            Utils.ResetTempFolder(out errorText);

            DoSerializeAddonPackage();
        }



        private static void DoSerializeAddonPackage()
        {

            AddonPackage package = new AddonPackage(@"C:\Program Files (x86)\Moviestorm\AddOn\1AD", ProcessingFlags.ListCompactDupVerbsByName);



            string errorText = null;


            StringBuilder writerStringBuilder = new StringBuilder();
            try
            {
                XmlSerializer serializer = new XmlSerializer(package.GetType());


                using (StringWriter writer = new StringWriter(writerStringBuilder))
                {
                    serializer.Serialize(writer, package);
                    writer.Close();
                }
            }
            catch (Exception exception)
            {
                errorText = Utils.GetExceptionExtendedMessage(exception);
            }


            string serializedPackage = writerStringBuilder.ToString();


        }
    }
}
