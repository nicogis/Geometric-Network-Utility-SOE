//-----------------------------------------------------------------------
// <copyright file="GeometricNetwork.cs" company="Studio A&T s.r.l.">
// Copyright (c) Studio A&T s.r.l. All rights reserved.
// </copyright>
// <author>Nicogis</author>
//-----------------------------------------------------------------------
namespace Studioat.ArcGis.Soe.Rest
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Text;
    using ESRI.ArcGIS.Geodatabase;
    using ESRI.ArcGIS.Geometry;
    using ESRI.ArcGIS.NetworkAnalysis;
    using ESRI.ArcGIS.SOESupport;

    /// <summary>
    /// Class for geometry network tracer
    /// </summary>
    [SuppressMessage("Microsoft.StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "-")]
    internal abstract class GeometricNetwork : IGeometricNetwork
    {
        /// <summary>
        /// Gets or sets Geometry Network
        /// </summary>
        protected ESRI.ArcGIS.Geodatabase.IGeometricNetwork geometricNetwork;

        /// <summary>
        /// Gets or sets flag Not Found
        /// </summary>
        protected List<IGeometry> flagNotFound;

        /// <summary>
        /// Gets or sets barrier Not Found
        /// </summary>
        protected List<IGeometry> barrierNotFound;   

        /// <summary>
        /// list flag edge found
        /// </summary>
        protected IEdgeFlag[] edgeFlags;
        
        /// <summary>
        /// list flag junction found
        /// </summary>
        protected IJunctionFlag[] junctionFlags;
 
        /// <summary>
        /// list barrier edge found
        /// </summary>
        protected int[] edgeBarriers;

        /// <summary>
        /// list barrier junction found
        /// </summary>
        protected int[] junctionBarriers;

        /// <summary>
        /// Initializes a new instance of the GeometricNetwork class
        /// </summary>
        /// <param name="geometricNetwork">object geometricNetwork</param>
        /// <param name="geometricNetworkParameters">object IGeometricNetworkParameters</param>
        public GeometricNetwork(ESRI.ArcGIS.Geodatabase.IGeometricNetwork geometricNetwork, IGeometricNetworkParameters geometricNetworkParameters)
        {
            this.geometricNetwork = geometricNetwork;
            this.EdgeFlags = geometricNetworkParameters.EdgeFlags;
            this.JunctionFlags = geometricNetworkParameters.JunctionFlags;
            this.EdgeBarriers = geometricNetworkParameters.EdgeBarriers;
            this.JunctionBarriers = geometricNetworkParameters.JunctionBarriers;
            this.MaxFeatures = geometricNetworkParameters.MaxFeatures;
            this.Tolerance = geometricNetworkParameters.Tolerance;
            this.FlowElements = geometricNetworkParameters.FlowElements;
            this.OutFields = new string[] { "*" };

            this.edgeFlags = new EdgeFlag[] { };
            this.junctionFlags = new JunctionFlag[] { };
            this.edgeBarriers = new int[] { };
            this.junctionBarriers = new int[] { };

            this.SetFlagsGeometricNetwork();
            this.SetBarriersGeometricNetwork();
        }

        /// <summary>
        /// Gets or sets Output fields
        /// </summary>
        public string[] OutFields
        {
            get;
            set;
        }

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

        /// <summary>
        /// Gets or sets ShortestPathObjFn 
        /// </summary>
        public esriShortestPathObjFn ShortestPathObjFn
        {
            get;
            set;
        }

        /// <summary>
        /// method solve
        /// </summary>
        /// <returns>result of solve</returns>
        public abstract JsonObject Solve();

        /// <summary>
        /// return a featureSet from EIDs
        /// </summary>
        /// <param name="eids">object EIDs</param>
        /// <param name="featureSets">array JsonObject featureSet</param>
        protected void GetTrace(IEnumNetEID eids, out JsonObject[] featureSets)
        {
            IEIDHelper eidHelper = new EIDHelperClass();
            eidHelper.GeometricNetwork = this.geometricNetwork;
            eidHelper.ReturnGeometries = true;
            eidHelper.ReturnFeatures = true;

            Dictionary<int, IPair<List<int>, IFeatureClass>> dictionary = new Dictionary<int, IPair<List<int>, IFeatureClass>>();
            IEnumEIDInfo enumEIDinfo = eidHelper.CreateEnumEIDInfo(eids);
            enumEIDinfo.Reset();
            IEIDInfo eidInfo = enumEIDinfo.Next();
            while (eidInfo != null)
            {
                IFeatureClass featureClass = eidInfo.Feature.Class as IFeatureClass;
                IFeature feature = eidInfo.Feature;
                int featureClassID = featureClass.FeatureClassID;
                if (!dictionary.ContainsKey(featureClassID))
                {
                    IPair<List<int>, IFeatureClass> pair = new Pair<List<int>, IFeatureClass>(new List<int>(), featureClass);
                    dictionary.Add(featureClassID, pair);
                }

                dictionary[featureClassID].First.Add(feature.OID);
                eidInfo = enumEIDinfo.Next();
            }

            List<JsonObject> jsonObjects = new List<JsonObject>();
            foreach (int i in dictionary.Keys)
            {
                IPair<List<int>, IFeatureClass> pair = dictionary[i];
                List<int> listOIDs = pair.First;
                IFeatureClass featureClass = pair.Second;

                IQueryFilter2 queryFilter = new QueryFilterClass();
                if (this.OutFields[0] != "*")
                {
                    List<string> listFields = new List<string>();
                    Array.ForEach(
                        this.OutFields,
                        s =>
                        {
                            if (featureClass.Fields.FindField(s) != -1)
                            {
                                listFields.Add(s);
                            }
                        });

                    if (listFields.Count == 0)
                    {
                        queryFilter.SubFields = featureClass.OIDFieldName;
                    }
                    else
                    {
                        listFields.Each((s, index) =>
                        {
                            if (index == 0)
                            {
                                queryFilter.SubFields = s;
                            }
                            else
                            {
                                queryFilter.AddField(s);
                            }
                        });

                        if (!Array.Exists(this.OutFields, s => s == featureClass.OIDFieldName))
                        {
                            queryFilter.AddField(featureClass.OIDFieldName);
                        }

                        if (!Array.Exists(this.OutFields, s => s == featureClass.ShapeFieldName))
                        {
                            queryFilter.AddField(featureClass.ShapeFieldName);
                        }
                    }
                }

                queryFilter.WhereClause = featureClass.OIDFieldName + " IN (" + string.Join(",", Array.ConvertAll<int, string>(listOIDs.ToArray(), s => s.ToString(CultureInfo.InvariantCulture))) + ")";

                IRecordSet recordset = Helper.ConvertToRecordset(featureClass, queryFilter);
                jsonObjects.Add(new JsonObject(Encoding.UTF8.GetString(Conversion.ToJson(recordset))));
            }

            featureSets = jsonObjects.ToArray();
        }

        /// <summary>
        /// set edge and junction for results
        /// </summary>
        /// <param name="objectJson">object JsonObject</param>
        /// <param name="edgeEIDs">object IEnumNetEID for edge</param>
        /// <param name="junctionEIDs">object IEnumNetEID for junction</param>
        protected void SetResults(ref JsonObject objectJson, IEnumNetEID edgeEIDs, IEnumNetEID junctionEIDs)
        {
            if ((this.FlowElements == esriFlowElements.esriFEEdges) || (this.FlowElements == esriFlowElements.esriFEJunctionsAndEdges))
            {
                if (edgeEIDs == null)
                {
                    throw new GeometricNetworkException("No traced edges found");
                }
                else
                {
                    if (edgeEIDs.Count > this.MaxFeatures)
                    {
                        throw new GeometricNetworkException(edgeEIDs.Count.ToString(CultureInfo.InvariantCulture) + " features were traced which exceeds the limit of " + this.MaxFeatures);
                    }
                }
            }

            if ((this.FlowElements == esriFlowElements.esriFEJunctions) || (this.FlowElements == esriFlowElements.esriFEJunctionsAndEdges))
            {
                if (junctionEIDs == null)
                {
                    throw new GeometricNetworkException("No traced junctions found");
                }
                else
                {
                    if (junctionEIDs.Count > this.MaxFeatures)
                    {
                        throw new GeometricNetworkException(junctionEIDs.Count.ToString(CultureInfo.InvariantCulture) + " features were traced which exceeds the limit of " + this.MaxFeatures);
                    }
                }
            }

            if (this.FlowElements == esriFlowElements.esriFEEdges || this.FlowElements == esriFlowElements.esriFEJunctionsAndEdges)
            {
                if (edgeEIDs.Count == 0)
                {
                    objectJson.AddArray("edges", (new List<JsonObject>()).ToArray());
                }
                else
                {
                    JsonObject[] featureSet;
                    this.GetTrace(edgeEIDs, out featureSet);
                    objectJson.AddArray("edges", featureSet);
                }
            }

            if (this.FlowElements == esriFlowElements.esriFEJunctions || this.FlowElements == esriFlowElements.esriFEJunctionsAndEdges)
            {
                if (junctionEIDs.Count == 0)
                {
                    objectJson.AddArray("junctions", (new List<JsonObject>()).ToArray());
                }
                else
                {
                    JsonObject[] featureSet;
                    this.GetTrace(junctionEIDs, out featureSet);
                    objectJson.AddArray("junctions", featureSet);
                }
            }
        }

        /// <summary>
        /// Set Barriers GeometricNetwork
        /// </summary>
        protected void SetBarriersGeometricNetwork()
        {
            this.barrierNotFound = new List<IGeometry>();
            ////barriers edge
            if (this.EdgeBarriers.Count > 0)
            {
                List<int> eidEdgeBarriers = new List<int>();
                foreach (IPoint point in this.EdgeBarriers)
                {
                    int eid = this.GetEIDFromPoint(this.Tolerance, point, esriElementType.esriETEdge);
                    if (eid < 1)
                    {
                        this.barrierNotFound.Add(point);
                        continue;
                    }

                    eidEdgeBarriers.Add(eid);
                }

                this.edgeBarriers = eidEdgeBarriers.ToArray();
            }

            ////barriers junction
            if (this.JunctionBarriers.Count > 0)
            {
                List<int> eidJunctionBarriers = new List<int>();
                foreach (IPoint point in this.JunctionBarriers)
                {
                    int eid = this.GetEIDFromPoint(this.Tolerance, point, esriElementType.esriETJunction);
                    if (eid < 1)
                    {
                        this.barrierNotFound.Add(point);
                        continue;
                    }

                    eidJunctionBarriers.Add(eid);
                }

                this.junctionBarriers = eidJunctionBarriers.ToArray();
            }
        }

        /// <summary>
        /// set barrier for traceFlowSolver
        /// </summary>
        /// <param name="traceFlowSolver">object ITraceFlowSolverGEN</param>
        protected void SetBarriers(ref ITraceFlowSolverGEN traceFlowSolver)
        {
            INetSolver netSolver = traceFlowSolver as INetSolver;

            if (this.edgeBarriers.Length > 0)
            {
                INetElementBarriersGEN netElementBarriersGEN = new NetElementBarriersClass() as INetElementBarriersGEN;
                netElementBarriersGEN.ElementType = esriElementType.esriETEdge;
                netElementBarriersGEN.Network = this.geometricNetwork.Network;
                netElementBarriersGEN.SetBarriersByEID(ref this.edgeBarriers);
                INetElementBarriers netElementBarriers = netElementBarriersGEN as INetElementBarriers;
                netSolver.set_ElementBarriers(esriElementType.esriETEdge, netElementBarriers);
            }

            if (this.junctionBarriers.Length > 0)
            {
                INetElementBarriersGEN netElementBarriersGEN = new NetElementBarriersClass() as INetElementBarriersGEN;
                netElementBarriersGEN.ElementType = esriElementType.esriETJunction;
                netElementBarriersGEN.Network = this.geometricNetwork.Network;
                netElementBarriersGEN.SetBarriersByEID(ref this.junctionBarriers);
                INetElementBarriers netElementBarriers = netElementBarriersGEN as INetElementBarriers;
                netSolver.set_ElementBarriers(esriElementType.esriETJunction, netElementBarriers);
            }
        }

        /// <summary>
        /// set origin flag for traceFlowSolver
        /// </summary>
        /// <param name="traceFlowSolver">object ITraceFlowSolverGEN</param>
        protected void SetFlagsOrigin(ref ITraceFlowSolverGEN traceFlowSolver)
        {
            if (this.edgeFlags.Length > 0)
            {
                traceFlowSolver.PutEdgeOrigins(ref this.edgeFlags);
            }

            if (this.junctionFlags.Length > 0)
            {
                traceFlowSolver.PutJunctionOrigins(ref this.junctionFlags);
            }
        }

        /// <summary>
        /// Set Flags Geometric Network
        /// </summary>
        protected void SetFlagsGeometricNetwork()
        {
            //// edge Flags
            List<INetFlag> edgeFlagList = new List<INetFlag>();
            this.flagNotFound = new List<IGeometry>();

            if (this.EdgeFlags.Count > 0)
            {
                foreach (IPoint point in this.EdgeFlags)
                {
                    int eid = this.GetEIDFromPoint(this.Tolerance, point, esriElementType.esriETEdge);

                    if (eid < 1)
                    {
                        this.flagNotFound.Add(point);
                        continue;
                    }

                    INetElements netElements = this.geometricNetwork.Network as INetElements;
                    int featureClassID, featureID, subID;
                    netElements.QueryIDs(eid, esriElementType.esriETEdge, out featureClassID, out featureID, out subID);

                    INetFlag netFlag = new EdgeFlagClass();
                    netFlag.UserClassID = featureClassID;
                    netFlag.UserID = featureID;
                    netFlag.UserSubID = subID;
                    edgeFlagList.Add(netFlag);
                }

                this.edgeFlags = new IEdgeFlag[edgeFlagList.Count];
                edgeFlagList.Each((i, index) =>
                {
                    this.edgeFlags[index] = i as IEdgeFlag;
                });
            }

            //// junction Flags
            List<INetFlag> junctionFlagList = new List<INetFlag>();
            if (this.JunctionFlags.Count > 0)
            {
                foreach (IPoint point in this.JunctionFlags)
                {
                    int eid = this.GetEIDFromPoint(this.Tolerance, point, esriElementType.esriETJunction);

                    if (eid < 1)
                    {
                        this.flagNotFound.Add(point);
                        continue;
                    }

                    INetElements netElements = this.geometricNetwork.Network as INetElements;
                    int featureClassID, featureID, subID;
                    netElements.QueryIDs(eid, esriElementType.esriETJunction, out featureClassID, out featureID, out subID);

                    INetFlag netFlag = new JunctionFlagClass();
                    netFlag.UserClassID = featureClassID;
                    netFlag.UserID = featureID;
                    netFlag.UserSubID = subID;
                    junctionFlagList.Add(netFlag);
                }

                this.junctionFlags = new IJunctionFlag[junctionFlagList.Count];
                junctionFlagList.Each((i, index) =>
                {
                    this.junctionFlags[index] = i as JunctionFlag;
                });
            }
        }

        /// <summary>
        /// search the eid nearest from point
        /// </summary>
        /// <param name="searchTolerance">tolerance for search</param>
        /// <param name="point">point input</param>
        /// <param name="elementType">type of element</param>
        /// <returns>return eid</returns>
        private int GetEIDFromPoint(double searchTolerance, IPoint point, esriElementType elementType)
        {
            return Helper.GetEIDFromPoint(this.geometricNetwork, searchTolerance, point, elementType);
        }
    }
}
