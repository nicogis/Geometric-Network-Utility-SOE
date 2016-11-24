//-----------------------------------------------------------------------
// <copyright file="Pair.cs" company="Studio A&T s.r.l.">
//     Copyright (c) Studio A&T s.r.l. All rights reserved.
// </copyright>
// <author>Nicogis</author>
namespace Studioat.ArcGis.Soe.Rest
{
    /// <summary>
    /// class implement Pair
    /// </summary>
    /// <typeparam name="A">first type</typeparam>
    /// <typeparam name="B">second type</typeparam>
    internal class Pair<A, B> : IPair<A, B>
    {
        /// <summary>
        /// The first value
        /// </summary>
        private readonly A first;

        /// <summary>
        /// The second value
        /// </summary>
        private readonly B second;

        /// <summary>
        /// Initializes a new instance of the Pair class
        /// </summary>
        /// <param name="first">The first value</param>
        /// <param name="second">The second value</param>
        public Pair(A first, B second)
        {
            this.first = first;
            this.second = second;
        }

        /// <summary>
        /// Gets first value
        /// </summary>
        A IPair<A, B>.First
        {
            get { return this.first; }
        }

        /// <summary>
        /// Gets second value
        /// </summary>
        B IPair<A, B>.Second
        {
            get { return this.second; }
        }
    }
}
