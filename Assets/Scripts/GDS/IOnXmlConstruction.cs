using System.Xml;

namespace GDS
{
    /// <summary>
    /// Classes that implement this interface will have the <see cref="OnConstructedFromXml(XmlNode)"/>
    /// method called after this object has been constructed from an xml node.
    /// </summary>
    public interface IOnXmlConstruction
    {
        /// <summary>
        /// Called after this object has been constructed from an xml node.
        /// </summary>
        /// <param name="node">The root node this object was constructed from.</param>
        void OnConstructedFromXml(XmlNode node);
    }
}
