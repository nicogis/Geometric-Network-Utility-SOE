//-----------------------------------------------------------------------
// <copyright file="GeometricNetworkParameters.cs" company="Studio A&T s.r.l.">
//     Copyright (c) Studio A&T s.r.l. All rights reserved.
// </copyright>
// <author>Nicogis</author>
namespace Studioat.ArcGis.Soe.Rest
{
    using System.Collections.Generic;
    using ESRI.ArcGIS.Geometry;
    using ESRI.ArcGIS.NetworkAnalysis;

    /// <summary>
    /// class GeometricNetworkParameters
    /// </summary>
    internal class GeometricNetworkParameters : IGeometricNetworkParameters
    {
        /// <summary>
        /// Gets or sets Edge Flags
        /// </summary>
        public List<IPoint> EdgeFlags
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets Junction Flags
        /// </summary>
        public List<IPoint> JunctionFlags
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets Edge Barriers
        /// </summary>
        public List<IPoint> EdgeBarriers
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets Junction Barriers
        /// </summary>
        public List<IPoint> JunctionBarriers
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets max Features
        /// </summary>
        public int MaxFeatures
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets max Features
        /// </summary>
        public double Tolerance
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets Flow Elements 
        /// </summary>
        public esriFlowElements FlowElements
        {
            get;
            set;
        }
    }
}
