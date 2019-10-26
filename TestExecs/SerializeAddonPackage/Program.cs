using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Xml.Serialization;
using MSAddonLib.Domain;
using MSAddonLib.Domain.Addon;
using MSAddonLib.Persistence.AddonDB;
using MSAddonLib.Util;

namespace SerializeAddonPackage
{
    class Program
    {
        static void Main(string[] args)
        {

            // Test01();
            // return;

            string errorText;
            Utils.ResetTempFolder(out errorText);

            DoSerializeAddonPackage();
        }

        private static void Test01()
        {
            Exception exception3 = new Exception("Inner3 exception");
            Exception exception2 = new Exception("Inner2 exception", exception3);
            Exception exception1 = new Exception("Inner1 exception", exception2);
            Exception exception = new Exception("Exception level 0", exception1);

            string fullMessage = Utils.GetExceptionFullMessage(exception);

            string extendedMessage = Utils.GetExceptionExtendedMessage(exception);

        }


        private static void DoSerializeAddonPackage()
        {

            AddonPackage package = new AddonPackage(@"C:\Program Files (x86)\Moviestorm\AddOn\1AD", ProcessingFlags.ListCompactDupVerbsByName);
            /*
            if (pProcessingFlags.HasFlag(ProcessingFlags.AppendToAddonPackageSet) && (AddonPackageSet != null) &&
                (package != null) && (!package.HasIssues))
                AddonPackageSet.Append(package, true);
                */
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
                errorText = Utils.GetExceptionFullMessage(exception);
            }


            string serializedPackage = writerStringBuilder.ToString();


        }
    }
}
