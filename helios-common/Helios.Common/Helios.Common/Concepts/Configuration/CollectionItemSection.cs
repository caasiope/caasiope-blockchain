using System.Configuration;
using System.Xml;

namespace Helios.Common.Concepts.Configuration
{
    public abstract class CollectionItemSection<T> : IConfigurationSectionHandler where T : class, new()
    {
        public object Create(object parent, object configContext, XmlNode section)
        {
            var collectionData = new T();

            foreach (XmlNode childNode in section.ChildNodes)
            {
                if (childNode.Attributes == null) // most likely a comment
                    continue;
                Add(collectionData, childNode.Attributes);
            }
            return collectionData;
        }

        protected abstract void Add(T collectionData, XmlAttributeCollection attributes);
    }
}