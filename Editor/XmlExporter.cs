using System.IO;
using System.Text;
using System.Xml.Serialization;

namespace Editor
{
    public static class XmlExporter
    {
        /// <summary>
        /// 构造HierarchyData对象并导出为完整XML树结构
        /// </summary>
        /// <param name="data">HierarchyData</param>
        /// <returns>string</returns>
        public static string Export(HierarchyData data)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(HierarchyData));

            using (UTF8StringWriter writer = new UTF8StringWriter())
            {
                serializer.Serialize(writer, data);
                return writer.ToString();
            }
        }
    }
    
    // 自定义StringWriter以确保输出为UTF-8编码
    public class UTF8StringWriter : StringWriter
    {
        public override Encoding Encoding => Encoding.UTF8;
    }
}