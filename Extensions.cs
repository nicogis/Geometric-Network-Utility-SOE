//-----------------------------------------------------------------------
// <copyright file="Extensions.cs" company="Studio A&T s.r.l.">
//     Copyright (c) Studio A&T s.r.l. All rights reserved.
// </copyright>
// <author>Nicogis</author>
//-----------------------------------------------------------------------
namespace Studioat.ArcGis.Soe.Rest
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using ESRI.ArcGIS.SOESupport;

        /// <summary>
        /// class for extension
        /// </summary>
        internal static class Extensions
        {
            /// <summary>
            /// return the json in byte UTF8 from a object jsonObject
            /// </summary>
            /// <param name="jsonObject">object Json Object</param>
            /// <returns>json in byte UTF8</returns>
            internal static byte[] JsonByte(this JsonObject jsonObject)
            {
                string json = jsonObject.ToJson();
                return Encoding.UTF8.GetBytes(json);
            }

            /// <summary>
            /// each with index
            /// </summary>
            /// <typeparam name="T">type of object enumerable</typeparam>
            /// <param name="enumerable">object enumerable</param>
            /// <param name="action">action on items of enumerable object</param>
            internal static void Each<T>(this IEnumerable<T> enumerable, Action<T, int> action) 
            { 
                var i = 0;
                foreach (var e in enumerable)
                {
                    action(e, i++);
                }
            } 
        }
}
