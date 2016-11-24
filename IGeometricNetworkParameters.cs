//-----------------------------------------------------------------------
// <copyright file="IGeometricNetworkParameters.cs" company="Studio A&T s.r.l.">
//     Copyright (c) Studio A&T s.r.l. All rights reserved.
// </copyright>
// <author>Nicogis</author>
namespace Studioat.ArcGis.Soe.Rest
{
    using System.Collections.Generic;
    using ESRI.ArcGIS.Geometry;
    using ESRI.ArcGIS.NetworkAnalysis;

    /// <summary>
    /// interface IGeometricNetworkParameters
    /// </summary>
    internal interface IGeometricNetworkParameters
    {
        /// <summary>
        /// Gets or sets Edge Flags
        /// </summary>
        List<IPoint> EdgeFlags
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets Junction Flags
        /// </summary>
        List<IPoint> JunctionFlags
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets Edge Barriers
        /// </summary>
        List<IPoint> EdgeBarriers
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets Junction Barriers
        /// </summary>
        List<IPoint> JunctionBarriers
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets max Features
        /// </summary>
        int MaxFeatures
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets max Features
        /// </summary>
        double Tolerance
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets Flow Elements 
        /// </summary>
        esriFlowElements FlowElements
        {
            get;
            set;
        }
    }
}
