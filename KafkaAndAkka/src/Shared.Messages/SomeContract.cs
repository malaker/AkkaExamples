using System;
using System.Xml;

namespace Shared.Messages
{
    [Serializable]
    public class SomeContract
    {
        public long Id { get; set; }

        public DateTime Timestamp { get; set; }

        public string Content { get; set; }

        public XmlDocument MyXML
        {
            get
            {
                var doc = new XmlDocument();
                doc.LoadXml(Content);
                doc.FirstChild.InnerText = "version=\"1.0\" encoding=\"utf-16\"";
                return doc;
            }
        }
    }
}