using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace RCNet.MiscTools
{
    /// <summary>
    /// Miscellaneous utility functions
    /// </summary>
    public static class Utils
    {
        /// <summary>
        /// Value equality deep comparer (but slow).
        /// Object type has to be serializable.
        /// </summary>
        /// <param name="obj1">Instance 1</param>
        /// <param name="obj2">Instance 2</param>
        public static bool SerializebleEquals(object obj1, object obj2)
        {
            if(obj1.GetType() != obj2.GetType())
            {
                return false;
            }
            XmlSerializer serializer = new XmlSerializer(obj1.GetType());
            string obj1Xml, obj2Xml;
            using (StringWriter serialized1 = new StringWriter())
            {
                serializer.Serialize(serialized1, obj1);
                obj1Xml = serialized1.ToString();
            }
            using (StringWriter serialized2 = new StringWriter())
            {
                serializer.Serialize(serialized2, obj2);
                obj2Xml = serialized2.ToString();
            }
            return (obj1Xml == obj2Xml);
        }


    }//Utils
}//Namespace
