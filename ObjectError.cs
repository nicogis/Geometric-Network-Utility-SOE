//-----------------------------------------------------------------------
// <copyright file="ObjectError.cs" company="Studio A&T s.r.l.">
//     Copyright (c) Studio A&T s.r.l. All rights reserved.
// </copyright>
// <author>Nicogis</author>
namespace Studioat.ArcGis.Soe.Rest
{
    using ESRI.ArcGIS.SOESupport;

    /// <summary>
    /// class for create a json
    /// </summary>
    internal sealed class ObjectError : ObjectJson
    {
        /// <summary>
        /// Initializes a new instance of the ObjectError class.
        /// </summary>
        /// <param name="message">text of the message</param>
        internal ObjectError(string message)
        {
            this.Message = message;
        }

        /// <summary>
        /// Prevents a default instance of the ObjectError class from being created.
        /// </summary>
        private ObjectError()
        {
        }

        /// <summary>
        /// Gets or sets of the property Message
        /// </summary>
        public string Message
        {
            get;
            set;
        }

        #region IJsonObject Members
        /// <summary>
        /// return a object JsonObject
        /// </summary>
        /// <returns>object Json Object</returns>
        public override JsonObject ToJsonObject()
        {
            JsonObject result = base.ToJsonObject();
            result.AddString("errorDescription", this.Message);
            result.AddBoolean("hasError", true);
            return result;
        }

        #endregion
    }
}
