//Josip Medved <jmedved@jmedved.com>   www.medo64.com

//2012-02-22: First version.


using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Xml;

namespace Medo.Xml {

    /// <summary>
    /// Resolving external XML resources within Manifest resources.
    /// </summary>
    [ComVisible(false)]
    public class XmlResourceResolver : XmlResolver {

        /// <summary>
        /// Create new instance.
        /// </summary>
        /// <param name="resourceNamePrefix">Prefix of manifest resources part. E.g. "MyApp.Resources".</param>
        public XmlResourceResolver(string resourceNamePrefix) {
            this.ResourceNamePrefix = resourceNamePrefix;
        }

        /// <summary>
        /// Gets resource prefix.
        /// It is used internally GetManifestResourceStream.
        /// </summary>
        public string ResourceNamePrefix { get; private set; }

        /// <summary>
        /// Sets the credentials used to authenticate Web requests.
        /// Not supported.
        /// </summary>
        public override System.Net.ICredentials Credentials {
            set { throw new NotImplementedException("Setting credentials in not supported."); }
        }

        /// <summary>
        /// Maps an URI to an object containing the actual resource.
        /// </summary>
        /// <param name="absoluteUri">The URI returned from System.Xml.XmlResolver.ResolveUri.</param>
        /// <param name="role">The current version does not use this parameter when resolving URIs. This is provided for future extensibility purposes. For example, this can be mapped to the xlink:role and used as an implementation specific argument in other scenarios.</param>
        /// <param name="ofObjectToReturn">The type of object to return. The current version only returns System.IO.Stream objects.</param>
        /// <exception cref="System.ArgumentNullException">Argument cannot be null.</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">Argument must contain segments.</exception>
        public override object GetEntity(Uri absoluteUri, string role, Type ofObjectToReturn) {
            if (absoluteUri == null) { throw new ArgumentNullException("absoluteUri", "Argument cannot be null."); }
            if (absoluteUri.Segments.Length == 0) { throw new ArgumentOutOfRangeException("absoluteUri", "Argument must contain segments."); }
            if (ofObjectToReturn == null) { throw new ArgumentNullException("ofObjectToReturn", "Argument cannot be null."); }

            if (ofObjectToReturn.Equals(typeof(Stream))) {
                var fileName = absoluteUri.Segments[absoluteUri.Segments.Length - 1];
                var resourceName = this.ResourceNamePrefix + "." + fileName;
                var resourceStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName);
                return resourceStream;
            } else {
                throw new NotImplementedException("Returning object other than Stream is not supported."); //The current version does not use this parameter when resolving URIs. This is provided for future extensibility purposes. For example, this can be mapped to the xlink:role and used as an implementation specific argument in other scenarios. (http://msdn.microsoft.com/en-us/library/system.xml.xmlresolver.getentity.aspx)
            }
        }
    }
}
