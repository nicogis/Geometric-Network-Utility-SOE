//-----------------------------------------------------------------------
// <copyright file="IGeometricNetworkValveIsolation.cs" company="Studio A&T s.r.l.">
//     Copyright (c) Studio A&T s.r.l. All rights reserved.
// </copyright>
// <author>Nicogis</author>
namespace Studioat.ArcGis.Soe.Rest
{
    using ESRI.ArcGIS.Geodatabase;

    /// <summary>
    /// interface IGeometricNetworkValveIsolation
    /// </summary>
    internal interface IGeometricNetworkValveIsolation : IGeometricNetwork
    {
        /// <summary>
        /// Gets or sets station for valve isolation
        /// </summary>
        IFeatureClass Station
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets valve for valve isolation
        /// </summary>
        IFeatureClass Valve
        {
            get;
            set;
        }
    }
}
