namespace GDS
{
    /// <summary>
    /// A class that implements this interface will be automatically added to a list of references
    /// when parsed in xml.
    /// </summary>
    public interface IXmlReference
    {
        /// <summary>
        /// The unique id of this reference.
        /// </summary>
        string XmlReferenceID { get; }
    }
}
