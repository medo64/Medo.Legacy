/* Josip Medved <jmedved@jmedved.com> http://www.jmedved.com */

//2008-11-07: First version.


using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;

namespace Medo.Localization
{

    /// <summary>
    /// Class for searching through post office data.
    /// Data needs to be valid XML in following format:
    /// <example>
    /// &lt;PostOffices&gt;
    ///     &lt;PostOffice code=&quot;31000&quot; name=&quot;Osijek&quot; region=&quot;Osječko-baranjska županija&quot; isCentral=&quot;true&quot; /&gt;
    ///     &lt;PostOffice code=&quot;31222&quot; name=&quot;Bizovac&quot; region=&quot;Osječko-baranjska županija&quot; isCentral=&quot;true&quot; /&gt;
    ///     &lt;PostOffice code=&quot;31216&quot; name=&quot;Ivanovac&quot; region=&quot;Osječko-baranjska županija&quot; isCentral=&quot;false&quot; /&gt;
    /// &lt;/PostOffices&gt;
    /// </example>
    /// </summary>
    public class PostOffices : IDisposable
    {

        private XElement _document;

        /// <summary>
        /// Creates new instance.
        /// </summary>
        /// <param name="xmlReader">XmlReader which contains post offices.</param>
        public PostOffices(XmlReader xmlReader)
        {
            _document = XElement.Load(xmlReader);
        }


        /// <summary>
        /// Return post offices that are exact match to given code.
        /// </summary>
        /// <param name="code">Postal code.</param>
        public IList<PostOffice> GetByExactCode(string code)
        {
            IEnumerable<XElement> items =
                from element in this._document.Elements("PostOffice")
                where element.Attribute("code").Value == code
                select element;

            List<PostOffice> list = GetList(items);
            list.Sort(ComparisionByCode);
            return list.AsReadOnly();
        }

        /// <summary>
        /// Return post offices that match start of given code.
        /// </summary>
        /// <param name="code">Postal code.</param>
        public IList<PostOffice> GetByPartialCode(string code)
        {
            IEnumerable<XElement> items =
                from element in this._document.Elements("PostOffice")
                where element.Attribute("code").Value.StartsWith(code, StringComparison.CurrentCultureIgnoreCase)
                select element;

            List<PostOffice> list = GetList(items);
            list.Sort(ComparisionByCode);
            return list.AsReadOnly();
        }

        /// <summary>
        /// Return post offices that match start of given name.
        /// </summary>
        /// <param name="name">Name.</param>
        public IList<PostOffice> GetByPartialName(string name)
        {
            IEnumerable<XElement> items =
                from element in this._document.Elements("PostOffice")
                where element.Attribute("name").Value.StartsWith(name, StringComparison.CurrentCultureIgnoreCase)
                select element;

            List<PostOffice> list = GetList(items);
            list.Sort(ComparisionByName);
            return list.AsReadOnly();
        }


        private static List<PostOffice> GetList(IEnumerable<XElement> items)
        {
            List<PostOffice> filteredList = new List<PostOffice>();
            foreach (XElement iItem in items)
            {
                string iCode = (string)iItem.Attribute("code");
                string iName = (string)iItem.Attribute("name");
                string iRegion = (string)iItem.Attribute("region");
                bool iIsCentral = (bool)iItem.Attribute("isCentral");
                filteredList.Add(new PostOffice(iCode, iName, iRegion, iIsCentral));
            }
            return filteredList;
        }


        private static int ComparisionByCode(PostOffice first, PostOffice second)
        {
            int cCode = string.Compare(first.Code, second.Code, StringComparison.CurrentCultureIgnoreCase);
            if (cCode != 0) { return cCode; }

            if (first.IsCentral && !second.IsCentral)
            {
                return -1;
            }
            else if (!first.IsCentral && second.IsCentral)
            {
                return 1;
            }

            return string.Compare(first.Name, second.Name, StringComparison.CurrentCultureIgnoreCase);
        }


        private static int ComparisionByName(PostOffice first, PostOffice second)
        {
            int cName = string.Compare(first.Name, second.Name, StringComparison.CurrentCultureIgnoreCase);
            if (cName != 0) { return cName; }

            int cCode = string.Compare(first.Code, second.Code, StringComparison.CurrentCultureIgnoreCase);
            if (cCode != 0) { return cCode; }

            if (first.IsCentral && !second.IsCentral)
            {
                return -1;
            }
            else if (!first.IsCentral && second.IsCentral)
            {
                return 1;
            }

            return 0;
        }


        #region "IDisposable Support"

        /// <summary>
        /// Releases the unmanaged resources and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">True to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
        }

        /// <summary>
        /// Releases all resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion

    }


    /// <summary>
    /// Structure for keeping post office data.
    /// </summary>
    public struct PostOffice
    {

        /// <summary>
        /// Create new instance.
        /// </summary>
        /// <param name="code">Postal code of post office.</param>
        /// <param name="name">Name.</param>
        /// <param name="region">Region.</param>
        /// <param name="isCentral">True if this is main post office.</param>
        public PostOffice(string code, string name, string region, bool isCentral)
            : this()
        {
            this.Code = code;
            this.Name = name;
            this.Region = region;
            this.IsCentral = isCentral;
        }

        /// <summary>
        /// Gets postal code.
        /// </summary>
        public string Code { get; private set; }

        /// <summary>
        /// Gets post office name.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Gets post office region.
        /// </summary>
        public string Region { get; private set; }

        /// <summary>
        /// Gets whether this is central post office.
        /// </summary>
        public bool IsCentral { get; private set; }


        /// <summary>
        /// Returns a value indicating whether this instance is equal to the other.
        /// </summary>
        /// <param name="obj">A object to compare to this instance.</param>
        public override bool Equals(object obj)
        {
            if (obj is PostOffice)
            {
                PostOffice other = (PostOffice)obj;
                return (string.Compare(this.Code, other.Code, StringComparison.Ordinal) == 0) && (string.Compare(this.Name, other.Name, StringComparison.Ordinal) == 0);
            }
            return false;
        }

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>A 32-bit signed integer hash code.</returns>
        public override int GetHashCode()
        {
            return this.Code.GetHashCode();
        }

        /// <summary>
        /// Converts the value of this instance to its equivalent string representation.
        /// </summary>
        /// <returns>String that represents this instance.</returns>
        public override string ToString()
        {
            return this.Name;
        }



        /// <summary>
        /// Returns true if both objects are equal.
        /// </summary>
        /// <param name="objA">First object.</param>
        /// <param name="objB">Second object.</param>
        public static bool operator ==(PostOffice objA, PostOffice objB)
        {
            return objB.Equals(objA);
        }

        /// <summary>
        /// Returns true if both objects are no equal.
        /// </summary>
        /// <param name="objA">First object.</param>
        /// <param name="objB">Second object.</param>
        public static bool operator !=(PostOffice objA, PostOffice objB)
        {
            return !objA.Equals(objB);
        }

    }

}
