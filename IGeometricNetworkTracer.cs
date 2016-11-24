//-----------------------------------------------------------------------
// <copyright file="IGeometricNetworkTracer.cs" company="Studio A&T s.r.l.">
//     Copyright (c) Studio A&T s.r.l. All rights reserved.
// </copyright>
// <author>Nicogis</author>
namespace Studioat.ArcGis.Soe.Rest
{
    using System.Collections.Generic;
    using ESRI.ArcGIS.Geodatabase;
    using ESRI.ArcGIS.NetworkAnalysis;

    /// <summary>
    /// interface IGeometricNetworkTracer
    /// </summary>
    internal interface IGeometricNetworkTracer : IGeometricNetwork
    {
        /// <summary>
        /// Gets or sets flow Method
        /// </summary>
        esriFlowMethod FlowMethod
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether trace Indeterminate Flow
        /// </summary>
        bool TraceIndeterminateFlow
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets Type Trace Solver 
        /// </summary>
        TraceSolverType TraceSolverType
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets ShortestPathObjFn 
        /// </summary>
        esriShortestPathObjFn ShortestPathObjFn
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets Disable layers 
        /// </summary>
        List<int> DisableLayers
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets Junction Weight
        /// </summary>
        INetWeight JunctionWeight
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets From To Edge Weight
        /// </summary>
        INetWeight FromToEdgeWeight
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets To From Edge Weight
        /// </summary>
        INetWeight ToFromEdgeWeight
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets Junction Filter Weight
        /// </summary>
        INetWeight JunctionFilterWeight
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets Junction Filter Ranges From
        /// </summary>
        List<object> JunctionFilterRangesFrom
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets Junction Filter Ranges To
        /// </summary>
        List<object> JunctionFilterRangesTo
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether Junction Filter Not Operator
        /// </summary>
        bool JunctionFilterNotOperator
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets From To Edge Filter Weight
        /// </summary>
        INetWeight FromToEdgeFilterWeight
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets To From Edge Filter Weight
        /// </summary>
        INetWeight ToFromEdgeFilterWeight
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets Edge Filter Ranges From
        /// </summary>
        List<object> EdgeFilterRangesFrom
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets Edge Filter Ranges To
        /// </summary>
        List<object> EdgeFilterRangesTo
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether Edge Filter Not Operator
        /// </summary>
        bool EdgeFilterNotOperator
        {
            get;
            set;
        }
    }
}
