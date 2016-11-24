//-----------------------------------------------------------------------
// <copyright file="GeometricNetworkValveIsolation.cs" company="Studio A&T s.r.l.">
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
    /// class GeometricNetworkValveIsolation
    /// </summary>
    [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "-")]
    internal sealed class GeometricNetworkValveIsolation : GeometricNetwork, IGeometricNetworkValveIsolation
    {
        /// <summary>
        /// Initializes a new instance of the GeometricNetworkValveIsolation class
        /// </summary>
        /// <param name="geometricNetwork">object geometricNetwork</param>
        /// <param name="geometricNetworkParameters">object IGeometricNetworkParameters</param>
        /// <param name="valve">feature class valve</param>
        /// <param name="station">feature class station</param>
        public GeometricNetworkValveIsolation(ESRI.ArcGIS.Geodatabase.IGeometricNetwork geometricNetwork, IGeometricNetworkParameters geometricNetworkParameters, IFeatureClass valve, IFeatureClass station)
            : base(geometricNetwork, geometricNetworkParameters)
        {
            this.Valve = valve;
            this.Station = station;
        }

        /// <summary>
        /// Gets or sets feature class Station for Isolation Valve
        /// </summary>
        public IFeatureClass Station
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets feature class Valve for Isolation Valve
        /// </summary>
        public IFeatureClass Valve
        {
            get;
            set;
        }
        
        /// <summary>
        /// solve isolation
        /// </summary>
        /// <returns>object JsonObject</returns>
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
                        
                        return error;
                    }
                }
                
                INetwork network = this.geometricNetwork.Network;

                ITraceFlowSolverGEN traceFlowSolver = new TraceFlowSolverClass() as ITraceFlowSolverGEN;
                INetSolver netSolver = traceFlowSolver as INetSolver;
                netSolver.SourceNetwork = network;

                this.SetFlagsOrigin(ref traceFlowSolver);

                ////now create barries based on valves
                INetElementBarriersGEN netElementBarriersGEN = new NetElementBarriersClass() as INetElementBarriersGEN;
                netElementBarriersGEN.ElementType = esriElementType.esriETJunction;
                netElementBarriersGEN.Network = network;
                int[] userIds = Helper.GetOIDs(this.Valve);
                netElementBarriersGEN.SetBarriers(this.Valve.FeatureClassID, ref userIds);
                INetElementBarriers netElementBarriers = netElementBarriersGEN as INetElementBarriers;
                netSolver.set_ElementBarriers(esriElementType.esriETJunction, netElementBarriers);

                IEnumNetEID junctionEIDs;
                IEnumNetEID edgeEIDs;
                traceFlowSolver.FindFlowEndElements(esriFlowMethod.esriFMConnected, esriFlowElements.esriFEJunctionsAndEdges, out junctionEIDs, out edgeEIDs);

                IEIDHelper eidHelper = new EIDHelperClass();
                eidHelper.GeometricNetwork = this.geometricNetwork;
                eidHelper.ReturnFeatures = true;

                List<IEIDInfo> valveEIDInfoHT = Helper.GetEIDInfoListByFeatureClass(this.Valve.FeatureClassID, junctionEIDs, eidHelper);

                traceFlowSolver = new TraceFlowSolverClass() as ITraceFlowSolverGEN;
                netSolver = traceFlowSolver as INetSolver;
                netSolver.SourceNetwork = network;

                this.SetFlagsOrigin(ref traceFlowSolver);

                netSolver.DisableElementClass(this.Station.FeatureClassID);
                traceFlowSolver.FindFlowEndElements(esriFlowMethod.esriFMConnected, esriFlowElements.esriFEJunctions, out junctionEIDs, out edgeEIDs);

                List<IEIDInfo> sourceEIDInfoHT = Helper.GetEIDInfoListByFeatureClass(this.Station.FeatureClassID, junctionEIDs, eidHelper);

                ////set up trace to find directly reachable sources
                traceFlowSolver = new TraceFlowSolverClass() as ITraceFlowSolverGEN;
                netSolver = traceFlowSolver as INetSolver;
                netSolver.SourceNetwork = network;

                this.SetFlagsOrigin(ref traceFlowSolver);

                ////set barriers in the network based on the saved valves
                ISelectionSetBarriers netElementBarrier = new SelectionSetBarriersClass();
                foreach (IEIDInfo eidInfo in valveEIDInfoHT)
                {
                    netElementBarrier.Add(this.Valve.FeatureClassID, eidInfo.Feature.OID);
                }

                netSolver.SelectionSetBarriers = netElementBarrier;

                ////run trace to find directly reachable sources(without passing valve)
                traceFlowSolver.FindFlowElements(esriFlowMethod.esriFMConnected, esriFlowElements.esriFEJunctionsAndEdges, out junctionEIDs, out edgeEIDs);
                List<IEIDInfo> sourceDirectEIDInfoHT = Helper.GetEIDInfoListByFeatureClass(this.Station.FeatureClassID, junctionEIDs, eidHelper);

                ////remove directly reachable sources from source array
                foreach (IEIDInfo eidInfo in sourceDirectEIDInfoHT)
                {
                    sourceEIDInfoHT.RemoveAll(x => x.Feature.OID == eidInfo.Feature.OID);
                }

                List<IEIDInfo> noSourceValveHT = new List<IEIDInfo>();
                List<IEIDInfo> hasSourceValveHT = new List<IEIDInfo>();

                List<int> listBarrierIds;
                bool foundSource;

                foreach (IEIDInfo valve in valveEIDInfoHT)
                {
                    foundSource = false;

                    ////create array of all isolation valve excluding the current one
                    listBarrierIds = new List<int>();
                    if (valveEIDInfoHT.Count > 1)
                    {
                        listBarrierIds.AddRange(valveEIDInfoHT.ConvertAll<int>(i => i.Feature.OID));
                        if (listBarrierIds.Contains(valve.Feature.OID))
                        {
                            listBarrierIds.Remove(valve.Feature.OID);
                        }
                    }

                    ////for each source attempt to trace
                    INetFlag netFlag1;
                    INetFlag netFlag2;
                    foreach (IEIDInfo source in sourceEIDInfoHT)
                    {
                        ////setup trace to test each valve
                        traceFlowSolver = new TraceFlowSolverClass() as ITraceFlowSolverGEN;
                        netSolver = traceFlowSolver as INetSolver;
                        netSolver.SourceNetwork = network;

                        ////set the first junction flag for path finding based this current valve
                        netFlag1 = new JunctionFlagClass();
                        netFlag1.UserClassID = valve.Feature.Class.ObjectClassID;
                        netFlag1.UserID = valve.Feature.OID;
                        netFlag1.UserSubID = 0;
                        netFlag1.Label = "Origin";

                        ////set the second and last trace flag at this source
                        netFlag2 = new JunctionFlagClass();
                        netFlag2.UserClassID = source.Feature.Class.ObjectClassID;
                        netFlag2.UserID = source.Feature.OID;
                        netFlag2.UserSubID = 0;
                        netFlag2.Label = "Destination";

                        Helper.AddTwoJunctionFlagsToTraceSolver(ref traceFlowSolver, netFlag1, netFlag2);

                        if (listBarrierIds.Count > 0)
                        {
                            netElementBarriersGEN = new NetElementBarriersClass() as INetElementBarriersGEN;
                            netElementBarriersGEN.ElementType = esriElementType.esriETJunction;
                            netElementBarriersGEN.Network = network;
                            int[] barrierIds = listBarrierIds.ToArray();
                            netElementBarriersGEN.SetBarriers(this.Valve.FeatureClassID, ref barrierIds);
                            netElementBarriers = netElementBarriersGEN as INetElementBarriers;
                            netSolver.set_ElementBarriers(esriElementType.esriETJunction, netElementBarriers);
                        }

                        ////run trace
                        object[] segCosts = new object[1];
                        segCosts[0] = new object();
                        edgeEIDs = null;
                        traceFlowSolver.FindPath(esriFlowMethod.esriFMConnected, esriShortestPathObjFn.esriSPObjFnMinSum, out junctionEIDs, out edgeEIDs, 1, ref segCosts);
                        if (edgeEIDs != null && edgeEIDs.Count > 0)
                        {
                            foundSource = true;
                            break;
                        }
                    }

                    if (foundSource)
                    {
                        hasSourceValveHT.Add(valve);
                    }
                    else
                    {
                        noSourceValveHT.Add(valve);
                    }
                }

                traceFlowSolver = new TraceFlowSolverClass() as ITraceFlowSolverGEN;
                netSolver = traceFlowSolver as INetSolver;
                netSolver.SourceNetwork = network;

                this.SetFlagsOrigin(ref traceFlowSolver);

                ////set the barrier in the network based on the saved valves
                netElementBarrier = new SelectionSetBarriersClass();
                foreach (IEIDInfo eidInfo in hasSourceValveHT)
                {
                    netElementBarrier.Add(this.Valve.FeatureClassID, eidInfo.Feature.OID);
                }

                netSolver.SelectionSetBarriers = netElementBarrier;

                ////run last trace
                traceFlowSolver.FindFlowElements(esriFlowMethod.esriFMConnected, this.FlowElements, out junctionEIDs, out edgeEIDs);

                ////deal with out put juncEIDs and edgeEIDs
                JsonObject objectJson = new JsonObject();
                this.SetResults(ref objectJson, edgeEIDs, junctionEIDs);
                objectJson.AddArray("flagsNotFound", Helper.GetListJsonObjects(this.flagNotFound));

                ////valves
                if (hasSourceValveHT.Count > 0)
                {
                    JsonObject[] featureSetValves;
                    this.GetTrace(Helper.GetEnumNetEID(hasSourceValveHT, esriElementType.esriETJunction), out featureSetValves);
                    objectJson.AddJsonObject("valves", featureSetValves[0]);
                }

                return objectJson;
            }
            catch (Exception e)
            {
                return (new ObjectError(e.Message)).ToJsonObject();
            }
        }
    }
}
