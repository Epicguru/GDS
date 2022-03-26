using GDS;
using GDS.Refs;
using System.Xml;

namespace Defs
{
    /// <summary>
    /// The base def class. A def is a data container that is loaded from an XML file.
    /// A def should generally not have behaviour code.
    /// The order of callback methods is as follows:
    /// <list type="number">
    /// <item><see cref="OnConstructedFromXml(XmlNode)"/>. Only override for advanced customization. Always call the base method if you do.</item>
    /// <item><see cref="PreResolveReferences"/>. Called before the <see cref="DefDatabase"/> is populated, and obviously before refs are resolved. Override when you want to manually request references.</item>
    /// <item><see cref="GetConfigErrors(DefConfigErrors)"/>. Called once all references have been resolved, and the <see cref="DefDatabase"/> has been populated. Override to check for errors in the def, such as missing data.</item>
    /// <item><see cref="PostLoad"/>. Do any extra initialization you may require here.</item>
    /// </list>
    /// </summary>
    public abstract class Def : IXmlReference, IOnXmlConstruction
    {
        public string XmlReferenceID => DefID;

        /// <summary>
        /// The name of the parent def, as specified in XML.
        /// The parent may be abstract, in which case it will not be in the <see cref="DefDatabase"/>.
        /// This field should probably only be used for debugging purposes.
        /// </summary>
        public string ParentDefName;

        /// <summary> 
        /// Unique Id of this def. Set automatically from the xml node name.
        /// Should not be modified at runtime.
        /// </summary>
        public string DefID;

        /// <summary>
        /// User-facing display name. Depending on the def type, this may or may not be set.
        /// </summary>
        public string Label;

        /// <summary>
        /// User-facing description. Depending on the def type, this may or may not be set.
        /// </summary>
        public string Description;

        /// <summary>
        /// Called when the def is first constructed from it's XML node.
        /// By the time this is called, all it's fields will also be initialized, but reference will not be resolved.
        /// If overriding, always call the base method.
        /// </summary>
        /// <param name="node">The node that this def was constructed from.</param>
        public virtual void OnConstructedFromXml(XmlNode node)
        {
            DefID = node.Name;
            ParentDefName = node.TryGetAttr("ParentName");
        }

        /// <summary>
        /// Called before references are resolved. Override to register add reference requests using <see cref="GDSParser.AddReferenceRequest(ReferenceRequest)"/>,
        /// which will then be resolved.
        /// </summary>
        public virtual void PreResolveReferences(GDSParser parser)
        {

        }

        /// <summary>
        /// Called after references are resolved, but before <see cref="PostLoad"/>.
        /// Here you should check for any errors in the data of this def, such as missing fields or invalid setups.
        /// Report errors and warnings using the <paramref name="report"/> object.  
        /// See: <see cref="DefConfigErrors.Error(string, string)"/>,
        /// <see cref="DefConfigErrors.Warn(string, string)"/>.
        /// </summary>
        public virtual void GetConfigErrors(DefConfigErrors report)
        {

        }

        /// <summary>
        /// Called after references are resolved and the <see cref="DefDatabase"/> has been populated. Do any final initialization here.
        /// Generally you should avoid using this, as Defs should not have any behaviour in them.
        /// </summary>
        public virtual void PostLoad()
        {

        }

        public override string ToString()
        {
            if (Label == null)
                return $"[{GetType().Name}] {DefID}";

            return $"[{GetType().Name}] {DefID} '{Label}'";
        }
    }
}
