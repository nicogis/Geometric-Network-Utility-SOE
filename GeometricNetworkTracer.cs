//-----------------------------------------------------------------------
// <copyright file="GeometricNetworkTracer.cs" company="Studio A&T s.r.l.">
// Copyright (c) Studio A&T s.r.l. All rights reserved.
// </copyright>
// <author>Nicogis</author>
//-----------------------------------------------------------------------
namespace Studioat.ArcGis.Soe.Rest
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using ESRI.ArcGIS.Geodatabase;
    using ESRI.ArcGIS.NetworkAnalysis;
    using ESRI.ArcGIS.SOESupport;

    /// <summary>
    /// Class GeometricNetworkTracer
    /// </summary>
    [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "-")]
    internal sealed class GeometricNetworkTracer : GeometricNetwork, IGeometricNetworkTracer
    {
        /// <summary>
        /// Initializes a new instance of the GeometricNetworkTracer class
        /// </summary>
        /// <param name="geometricNetwork">object geometricNetwork</param>
        /// <param name="geometricNetworkParameters">object IGeometricNetworkParameters</param>
        /// <param name="traceSolverType">trace Solver Type</param>
        /// <param name="flowMethod">flow method</param>
        /// <param name="traceIndeterminateFlow">trace Indeterminate Flow</param>
        public GeometricNetworkTracer(ESRI.ArcGIS.Geodatabase.IGeometricNetwork geometricNetwork, IGeometricNetworkParameters geometricNetworkParameters, TraceSolverType traceSolverType, esriFlowMethod flowMethod, bool traceIndeterminateFlow)
            : base(geometricNetwork, geometricNetworkParameters)
        {
            this.TraceSolverType = traceSolverType;
            this.FlowMethod = flowMethod;
            this.TraceIndeterminateFlow = traceIndeterminateFlow;
            this.DisableLayers = new List<int>();
        }

        /// <summary>
        /// Gets or sets a value indicating whether trace Indeterminate Flow
        /// </summary>
        public bool TraceIndeterminateFlow
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets Type Trace Solver 
        /// </summary>
        public TraceSolverType TraceSolverType
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets flow Method
        /// </summary>
        public esriFlowMethod FlowMethod
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets disable layers
        /// </summary>
        public List<int> DisableLayers
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets Junction Weight
        /// </summary>
        public INetWeight JunctionWeight
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets From To Edge Weight
        /// </summary>
        public INetWeight FromToEdgeWeight
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets To From Edge Weight
        /// </summary>
        public INetWeight ToFromEdgeWeight
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets Junction Filter Weight
        /// </summary>
        public INetWeight JunctionFilterWeight
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets Junction Filter Ranges From
        /// </summary>
        public List<object> JunctionFilterRangesFrom
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets Junction Filter Ranges To
        /// </summary>
        public List<object> JunctionFilterRangesTo
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether Junction Filter Not Operator
        /// </summary>
        public bool JunctionFilterNotOperator
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets From To Edge Filter Weight
        /// </summary>
        public INetWeight FromToEdgeFilterWeight
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets To From Edge Filter Weight
        /// </summary>
        public INetWeight ToFromEdgeFilterWeight
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets Edge Filter Ranges From
        /// </summary>
        public List<object> EdgeFilterRangesFrom
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets Edge Filter Ranges To
        /// </summary>
        public List<object> EdgeFilterRangesTo
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether Edge Filter Not Operator
        /// </summary>
        public bool EdgeFilterNotOperator
        {
            get;
            set;
        }

        /// <summary>
        /// operation solve trace
        /// </summary>
        /// <returns>solve trace</returns>
        public override JsonObject Solve()
        {
            try
            {
                if ((this.edgeFlags.Length == 0) && (this.junctionFlags.Length == 0))
                {
                    string message = "No input valid flags found"; 
                    if (this.flagNotFound.Count == 0)
                    {
                        throw new GeometricNetworkException(message);
                    }
                    else
                    {
                        JsonObject error = (new ObjectError(message)).ToJsonObject();
                        error.AddArray("flagsNotFound", Helper.GetListJsonObjects(this.flagNotFound));
                        if (this.barrierNotFound.Count > 0)
                        {
                            error.AddArray("barriersNotFound", Helper.GetListJsonObjects(this.barrierNotFound));
                        }

                        return error;
                    }
                }

                ITraceFlowSolverGEN traceFlowSolver = new TraceFlowSolverClass() as ITraceFlowSolverGEN;
                INetSolver netSolver = traceFlowSolver as INetSolver;
                INetwork network = this.geometricNetwork.Network;
                netSolver.SourceNetwork = network;

                // flag origin
                this.SetFlagsOrigin(ref traceFlowSolver);

                // barrier 
                this.SetBarriers(ref traceFlowSolver);

                // set disabled layers
                foreach (int i in this.DisableLayers)
                {
                    netSolver.DisableElementClass(i);
                }

                // set weight
                this.SetWeights(ref traceFlowSolver);
                
                if ((this.TraceSolverType != TraceSolverType.FindCircuits) && (this.TraceSolverType != TraceSolverType.FindCommonAncestors))
                {
                    // The TraceIndeterminateFlow property affects the trace results of trace solvers whose FlowMethod parameter is esriFMUpstream or esriFMDownstream
                    if ((this.FlowMethod == esriFlowMethod.esriFMDownstream) || (this.FlowMethod == esriFlowMethod.esriFMUpstream))
                    {
                        traceFlowSolver.TraceIndeterminateFlow = this.TraceIndeterminateFlow;
                    }
                }

                IEnumNetEID junctionEIDs = null;
                IEnumNetEID edgeEIDs = null;
                object totalCost;
                int count;
                object[] segmentCosts;

                JsonObject objectJson = new JsonObject();
                switch (this.TraceSolverType)
                {
                    case TraceSolverType.FindAccumulation:
                        traceFlowSolver.FindAccumulation(this.FlowMethod, this.FlowElements, out junctionEIDs, out edgeEIDs, out totalCost);
                        objectJson.AddDouble("totalCost", (double)totalCost);
                        break;
                    case TraceSolverType.FindCircuits:
                        traceFlowSolver.FindCircuits(this.FlowElements, out junctionEIDs, out edgeEIDs);
                        break;
                    case TraceSolverType.FindCommonAncestors:
                        traceFlowSolver.FindCommonAncestors(this.FlowElements, out junctionEIDs, out edgeEIDs);
                        break;
                    case TraceSolverType.FindFlowElements:
                        traceFlowSolver.FindFlowElements(this.FlowMethod, this.FlowElements, out junctionEIDs, out edgeEIDs);
                        break;
                    case TraceSolverType.FindFlowEndElements:
                        traceFlowSolver.FindFlowEndElements(this.FlowMethod, this.FlowElements, out junctionEIDs, out edgeEIDs);
                        break;
                    case TraceSolverType.FindFlowUnreachedElements:
                        traceFlowSolver.FindFlowUnreachedElements(this.FlowMethod, this.FlowElements, out junctionEIDs, out edgeEIDs);
                        break;
                    case TraceSolverType.FindPath:
                        count = Math.Max(this.junctionFlags.Length, this.edgeFlags.Length);
                        if (count < 2)
                        {
                            throw new GeometricNetworkException("Edge or junction found < 2!");
                        }

                        --count;
                        segmentCosts = new object[count];
                        traceFlowSolver.FindPath(this.FlowMethod, this.ShortestPathObjFn, out junctionEIDs, out edgeEIDs, count, ref segmentCosts);
                        objectJson.AddArray("segmentCosts", segmentCosts);
                        break;
                    case TraceSolverType.FindSource:
                        count = this.junctionFlags.Length + this.edgeFlags.Length;
                        segmentCosts = new object[count];
                        traceFlowSolver.FindSource(this.FlowMethod, this.ShortestPathObjFn, out junctionEIDs, out edgeEIDs, count, ref segmentCosts);
                        objectJson.AddArray("segmentCosts", segmentCosts);
                        break;
                    case TraceSolverType.FindLongest:
                        // get end junctions in upstream
                        IEnumNetEID junctionEIDsFindLongest = null;
                        IEnumNetEID edgeEIDsFindLongest = null;
                        traceFlowSolver.FindFlowEndElements(esriFlowMethod.esriFMUpstream, esriFlowElements.esriFEJunctions, out junctionEIDsFindLongest, out edgeEIDsFindLongest);
                        
                        int? eidLongest = null;
                        double eidLongestLenght = double.MinValue;

                        if (junctionEIDsFindLongest.Count > 0)
                        {
                            double eidLongestLenghtCurrent = double.NaN;
                            for (int i = 0; i < junctionEIDsFindLongest.Count; i++)
                            {
                                int netEIDCurrent = junctionEIDsFindLongest.Next();
                                object[] segmentLenghts = new object[1];
                                IEnumNetEID junctionEIDsLongest = null;
                                IEnumNetEID edgeEIDsLongest = null;

                                INetFlag netFlag = new JunctionFlagClass();
                                INetElements netElements = this.geometricNetwork.Network as INetElements;
                                int featureClassID, featureID, subID;
                                netElements.QueryIDs(netEIDCurrent, esriElementType.esriETJunction, out featureClassID, out featureID, out subID);

                                netFlag.UserClassID = featureClassID;
                                netFlag.UserID = featureID;
                                netFlag.UserSubID = subID;

                                IJunctionFlag[] junctionFlags = new IJunctionFlag[] { this.junctionFlags[0], netFlag as IJunctionFlag };
                                traceFlowSolver.PutJunctionOrigins(ref junctionFlags);

                                traceFlowSolver.FindPath(esriFlowMethod.esriFMUpstream, esriShortestPathObjFn.esriSPObjFnMinMax, out junctionEIDsLongest, out edgeEIDsLongest, 1, ref segmentLenghts);
                                if (segmentLenghts[0] != null)
                                {
                                    eidLongestLenghtCurrent = Convert.ToDouble(segmentLenghts[0]);
                                    if (eidLongestLenghtCurrent > eidLongestLenght)
                                    {
                                        eidLongestLenght = eidLongestLenghtCurrent;
                                        eidLongest = netEIDCurrent;
                                        edgeEIDs = edgeEIDsLongest;
                                        junctionEIDs = junctionEIDsLongest;
                                    }
                                }
                            }
                        }
                        else
                        {
                            throw new GeometricNetworkException("Junction end not found!");
                        }

                        if (eidLongest.HasValue)
                        {
                            objectJson.AddDouble("totalCost", eidLongestLenght);
                        }
                        else 
                        {
                            throw new GeometricNetworkException("EID longest not found!");
                        }

                        break;
                    default:
                        throw new GeometricNetworkException("Trace solver type not found");
                }

                this.SetResults(ref objectJson, edgeEIDs, junctionEIDs);
                objectJson.AddArray("flagsNotFound", Helper.GetListJsonObjects(this.flagNotFound));
                objectJson.AddArray("barriersNotFound", Helper.GetListJsonObjects(this.barrierNotFound));
                return objectJson;
            }
            catch (Exception e)
            {
                return (new ObjectError(e.Message)).ToJsonObject();
            }
        }

        /// <summary>
        /// set weights in traceFlowSolver
        /// </summary>
        /// <param name="traceFlowSolver">object traceFlowSolver</param>
        private void SetWeights(ref ITraceFlowSolverGEN traceFlowSolver)
        {
            INetSolverWeightsGEN netSolverWeights = traceFlowSolver as INetSolverWeightsGEN;

            // Junction Weight
            if (this.JunctionWeight != null)
            {
                netSolverWeights.JunctionWeight = this.JunctionWeight;
            }

            // FromTo Edge Weight
            if (this.FromToEdgeWeight != null)
            {
                netSolverWeights.FromToEdgeWeight = this.FromToEdgeWeight;
            }

            // ToFrom Edge Weight
            if (this.ToFromEdgeWeight != null)
            {
                netSolverWeights.ToFromEdgeWeight = this.ToFromEdgeWeight;
            }

            // Junction Filter Weight
            if (this.JunctionFilterWeight != null)
            {
                netSolverWeights.JunctionFilterWeight = this.JunctionFilterWeight;
                netSolverWeights.SetFilterType(esriElementType.esriETJunction, esriWeightFilterType.esriWFRange, this.JunctionFilterNotOperator);
                object[] junctionFilterRangesFrom = this.JunctionFilterRangesFrom.ToArray();
                object[] junctionFilterRangesTo = this.JunctionFilterRangesTo.ToArray();
                netSolverWeights.SetFilterRanges(esriElementType.esriETJunction, ref junctionFilterRangesFrom, ref junctionFilterRangesTo);
            }

            // FromTo ToFrom Edge Filter Weight
            if ((this.FromToEdgeFilterWeight != null) && (this.ToFromEdgeFilterWeight != null))
            {
                netSolverWeights.FromToEdgeFilterWeight = this.FromToEdgeFilterWeight;
                netSolverWeights.ToFromEdgeFilterWeight = this.ToFromEdgeFilterWeight;
                netSolverWeights.SetFilterType(esriElementType.esriETEdge, esriWeightFilterType.esriWFRange, this.EdgeFilterNotOperator);
                object[] edgeFilterRangesFrom = this.EdgeFilterRangesFrom.ToArray();
                object[] edgeFilterRangesTo = this.EdgeFilterRangesTo.ToArray();
                netSolverWeights.SetFilterRanges(esriElementType.esriETEdge, ref edgeFilterRangesFrom, ref edgeFilterRangesTo);
            }
        }
    }
}
