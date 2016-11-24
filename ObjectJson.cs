//-----------------------------------------------------------------------
// <copyright file="ObjectJson.cs" company="Studio A&T s.r.l.">
//     Copyright (c) Studio A&T s.r.l. All rights reserved.
// </copyright>
// <author>Nicogis</author>
namespace Studioat.ArcGis.Soe.Rest
{
    using ESRI.ArcGIS.SOESupport;

    /// <summary>
    /// class for create a json
    /// </summary>
    internal abstract class ObjectJson : IObjectJson
    {
        /// <summary>
        /// return a object JsonObject
        /// </summary>
        /// <returns>object Json Object</returns>
        public virtual JsonObject ToJsonObject()
        {
            JsonObject result = new JsonObject();
            return result;
        }
    }
}