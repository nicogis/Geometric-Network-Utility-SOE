//-----------------------------------------------------------------------
// <copyright file="IPair.cs" company="Studio A&T s.r.l.">
//     Copyright (c) Studio A&T s.r.l. All rights reserved.
// </copyright>
// <author>Nicogis</author>
namespace Studioat.ArcGis.Soe.Rest
{
    /// <summary>
    /// Provides the interface for a Pair.
    /// </summary>
    /// <typeparam name="A">The first value</typeparam>
    /// <typeparam name="B">The second value</typeparam>
    internal interface IPair<A, B>
    {
        /// <summary>
        /// Gets the first value in the pair.
        /// </summary>
        A First
        {
            get;
        }

        /// <summary>
        /// Gets the second value in the pair.
        /// </summary>
        B Second
        {
            get;
        }
    }
}
