//-----------------------------------------------------------------------
// <copyright file="Helper.cs" company="Studio A&T s.r.l.">
//  Copyright (c) Studio A&T s.r.l. All rights reserved.
// </copyright>
// <author>Nicogis</author>
//-----------------------------------------------------------------------
namespace Studioat.ArcGis.Soe.Rest
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using ESRI.ArcGIS.Geodatabase;
    using ESRI.ArcGIS.Geometry;
    using ESRI.ArcGIS.NetworkAnalysis;
    using ESRI.ArcGIS.SOESupport;

    /// <summary>
    /// Find method on a network
    /// </summary>
    internal enum TraceSolverType
    {
        /// <summary>
        /// Finds the total cost of all reachable network elements based on the specified flow method
        /// </summary>
        FindAccumulation,

        /// <summary>
        /// Finds all reachable network elements that are parts of closed circuits in the network
        /// </summary>
        FindCircuits,

        /// <summary>
        /// Finds all reachable network elements that are upstream from all the specified origins
        /// </summary>
        FindCommonAncestors,

        /// <summary>
        /// Finds all reachable network elements based on the specified flow method
        /// </summary>
        FindFlowElements,

        /// <summary>
        /// Finds all reachable network end elements based on the specified flow method
        /// </summary>
        FindFlowEndElements,

        /// <summary>
        /// Finds all unreachable network elements based on the flow method
        /// </summary>
        FindFlowUnreachedElements,

        /// <summary>
        /// Finds a path between the specified origins in the network
        /// </summary>
        FindPath,

        /// <summary>
        /// Finds a path upstream to a source or downstream to a sink, depending on the specified flow method
        /// </summary>
        FindSource,

        /// <summary>
        /// Finds the longest path upstream from a junction
        /// </summary>
        FindLongest
    }

    /// <summary>
    /// class of Helper
    /// </summary>
    internal static class Helper
    {
        /// <summary>
        /// convert FeatureClass in Recordset
        /// </summary>
        /// <param name="featureClass">feature class input</param>
        /// <param name="queryFilter">query filter</param>
        /// <returns>return Recordset</returns>
        internal static IRecordSet2 ConvertToRecordset(IFeatureClass featureClass, IQueryFilter2 queryFilter)
        {
            IRecordSet recordSet = new RecordSetClass();
            IRecordSetInit recordSetInit = recordSet as IRecordSetInit;
            recordSetInit.SetSourceTable(featureClass as ITable, queryFilter);

            return (IRecordSet2)recordSetInit;
        }

        /// <summary>
        /// Get list EIDInfos by FeatureClass
        /// </summary>
        /// <param name="featureClassId">Id FeatureClass</param>
        /// <param name="junctionEIDs">object junctionEIDs</param>
        /// <param name="eidHelper">object eidHelper</param>
        /// <returns>List of IEIDInfo</returns>
        internal static List<IEIDInfo> GetEIDInfoListByFeatureClass(int featureClassId, IEnumNetEID junctionEIDs, IEIDHelper eidHelper)
        {
            List<IEIDInfo> outputEIDInfoHT = new List<IEIDInfo>();

            IEnumEIDInfo allEnumEidInfo = eidHelper.CreateEnumEIDInfo(junctionEIDs);
            IEIDInfo eidInfo = allEnumEidInfo.Next();
            while (eidInfo != null)
            {
                if (eidInfo.Feature.Class.ObjectClassID == featureClassId)
                {
                    outputEIDInfoHT.Add(eidInfo);
                }

                eidInfo = allEnumEidInfo.Next();
            }

            return outputEIDInfoHT;
        }

        /// <summary>
        /// Add two junction flags to TraceSolver
        /// </summary>
        /// <param name="traceFlowSolver">object TraceSolver</param>
        /// <param name="netflag1">1° junction flag</param>
        /// <param name="netflag2">2° junction flag</param>
        internal static void AddTwoJunctionFlagsToTraceSolver(ref ITraceFlowSolverGEN traceFlowSolver, INetFlag netflag1, INetFlag netflag2)
        {
            IJunctionFlag junctionflag1 = netflag1 as IJunctionFlag;
            IJunctionFlag junctionflag2 = netflag2 as IJunctionFlag;
            if (junctionflag1 != null && junctionflag2 != null)
            {
                IJunctionFlag[] junctionflags = new IJunctionFlag[2];
                junctionflags[0] = junctionflag1 as IJunctionFlag;
                junctionflags[1] = junctionflag2 as IJunctionFlag;
                traceFlowSolver.PutJunctionOrigins(ref junctionflags);
            }
        }

        /// <summary>
        /// convert list IGeometry to array of JsonObjects
        /// </summary>
        /// <param name="geometries">list of IGeometry</param>
        /// <returns>array of JsonObject</returns>
        internal static object[] GetListJsonObjects(List<IGeometry> geometries)
        {
            List<JsonObject> jsonObjects = new List<JsonObject>();
            geometries.ForEach(g =>
            {
                jsonObjects.Add(Conversion.ToJsonObject(g));
            });

            return jsonObjects.ToArray();
        }

        /// <summary>
        /// get OID Valve
        /// </summary>
        /// <param name="featureClass">feature class valve</param>
        /// <returns>list of OIDs valve</returns>
        internal static int[] GetOIDs(IFeatureClass featureClass)
        {
            IQueryFilter queryFilter = new QueryFilterClass();
            queryFilter.SubFields = featureClass.OIDFieldName;
            List<int> inputIds = new List<int>();
            //// using (ComReleaser comReleaser = new ComReleaser())
            //// {
            IFeatureCursor featureCursor = null;
            try
            {
                featureCursor = featureClass.Search(queryFilter, true);
                //// comReleaser.ManageLifetime(featureCursor);
                IFeature feature = featureCursor.NextFeature();
                while (feature != null)
                {
                    inputIds.Add(feature.OID);
                    feature = featureCursor.NextFeature();
                }
                ////}
            }
            catch
            {
                throw;
            }
            finally
            {
                if (featureCursor != null)
                {
                    Marshal.ReleaseComObject(featureCursor);
                }
            }

            return inputIds.ToArray();
        }

        /// <summary>
        /// search the eid nearest from point
        /// </summary>
        /// <param name="geometricNetwork">geometric Network</param>
        /// <param name="searchTolerance">tolerance for search</param>
        /// <param name="point">point input</param>
        /// <param name="elementType">type of element</param>
        /// <returns>return eid</returns>
        internal static int GetEIDFromPoint(ESRI.ArcGIS.Geodatabase.IGeometricNetwork geometricNetwork, double searchTolerance, IPoint point, esriElementType elementType)
        {
            IEnumFeatureClass enumFeatureClassSimple = null;
            IEnumFeatureClass enumFeatureClassComlex = null;
            if (elementType == esriElementType.esriETEdge)
            {
                enumFeatureClassSimple = geometricNetwork.get_ClassesByType(esriFeatureType.esriFTSimpleEdge);
                enumFeatureClassComlex = geometricNetwork.get_ClassesByType(esriFeatureType.esriFTComplexEdge);
            }
            else if (elementType == esriElementType.esriETJunction)
            {
                enumFeatureClassSimple = geometricNetwork.get_ClassesByType(esriFeatureType.esriFTSimpleJunction);
                enumFeatureClassComlex = geometricNetwork.get_ClassesByType(esriFeatureType.esriFTComplexJunction);
            }

            double distance = double.PositiveInfinity;
            int featureClassID = -1;
            IGeometry featureGeometry = null;
            Helper.FindNearestDistance(enumFeatureClassSimple, point, searchTolerance, ref distance, ref featureClassID, ref featureGeometry);
            Helper.FindNearestDistance(enumFeatureClassComlex, point, searchTolerance, ref distance, ref featureClassID, ref featureGeometry);

            if (featureClassID == -1)
            {
                return -1;
            }

            IProximityOperator proximityPoint = featureGeometry as IProximityOperator;
            IPoint p = proximityPoint.ReturnNearestPoint(point, esriSegmentExtension.esriNoExtension);
            if (elementType == esriElementType.esriETEdge)
            {
                return geometricNetwork.get_EdgeElement(p);
            }
            else if (elementType == esriElementType.esriETJunction)
            {
                return geometricNetwork.get_JunctionElement(p);
            }

            return -1;
        }

        /// <summary>
        /// object polyline. Distance negative -> upstream
        /// </summary>
        /// <param name="geometricNetwork">object geometricNetwork</param>
        /// <param name="resultEdges">objects resultEdges</param>
        /// <param name="distance">value of distance</param>
        /// <param name="point">object point</param>
        /// <param name="offset">offset of polyline</param>
        /// <param name="messageInfo">info on result</param>
        /// <returns>object IGeometry (polyline or point)</returns>
        internal static IGeometry GetPolylinePosAlong(ESRI.ArcGIS.Geodatabase.IGeometricNetwork geometricNetwork, IEnumNetEID resultEdges, double distance, IPoint point, double? offset, ref string messageInfo)
        {
            IGeometry geometryBag = new GeometryBagClass();
            geometryBag.SpatialReference = point.SpatialReference;
            IGeometryCollection geometryCollection = geometryBag as IGeometryCollection;

            IEIDHelper eidHelper = new EIDHelperClass();
            eidHelper.GeometricNetwork = geometricNetwork;
            eidHelper.ReturnGeometries = true;
            eidHelper.ReturnFeatures = false;

            IEnumEIDInfo enumEIDinfo = eidHelper.CreateEnumEIDInfo(resultEdges);
            enumEIDinfo.Reset();
            IEIDInfo eidInfo = enumEIDinfo.Next();
            while (eidInfo != null)
            {
                IGeometry geometry = eidInfo.Geometry;
                geometryCollection.AddGeometry(geometry);
                eidInfo = enumEIDinfo.Next();
            }

            ITopologicalOperator unionedPolyline = new PolylineClass();
            unionedPolyline.ConstructUnion(geometryBag as IEnumGeometry);

            IPolyline pl = unionedPolyline as IPolyline;
            if (distance < 0)
            {
                pl.ReverseOrientation();
                distance = Math.Abs(distance);
            }

            IMAware mAware = pl as IMAware;
            mAware.MAware = true;
            IMSegmentation3 mSegmentation = unionedPolyline as IMSegmentation3;
            mSegmentation.SetMsAsDistance(false);

            IPoint ptTmp = new PointClass();
            double distanceAlong = 0;
            double distanceFromCurve = 0;
            bool rightSide = false;

            pl.QueryPointAndDistance(esriSegmentExtension.esriNoExtension, point, false, ptTmp, ref distanceAlong, ref distanceFromCurve, ref rightSide);
            object mStartArray = mSegmentation.GetMsAtDistance(distanceAlong, false);
            double[] mStart = mStartArray as double[];
            double distanceDownStream = distanceAlong + distance;
            IPolyline resultPolyline = mSegmentation.GetSubcurveBetweenMs(mStart[0], distanceDownStream) as IPolyline;

            if (resultPolyline.IsEmpty)
            {
                return point;
            }

            if (mSegmentation.MMax < distanceDownStream)
            {
                messageInfo = "The set distance exceeds the length of network";
            }

            return Helper.ConstructOffset(resultPolyline, offset);
        }

        /// <summary>
        /// Find Nearest Distance
        /// </summary>
        /// <param name="enumFeatureClass">object enumFeatureClass</param>
        /// <param name="point">point source</param>
        /// <param name="searchTolerance">search tolerance</param>
        /// <param name="distance">distance found</param>
        /// <param name="featureClassID">featureClassID found</param>
        /// <param name="featureGeometry">featureGeometry found</param>
        internal static void FindNearestDistance(IEnumFeatureClass enumFeatureClass, IPoint point, double searchTolerance, ref double distance, ref int featureClassID, ref IGeometry featureGeometry)
        {
            enumFeatureClass.Reset();
            IFeatureClass featureClass = enumFeatureClass.Next();
            while (featureClass != null)
            {
                string shapeFieldName = featureClass.ShapeFieldName;
                ITopologicalOperator topologicalOperator = point as ITopologicalOperator;
                ISpatialFilter spatialFilter = new SpatialFilterClass();
                spatialFilter.Geometry = topologicalOperator.Buffer(searchTolerance);
                spatialFilter.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;
                spatialFilter.GeometryField = shapeFieldName;
                IFeatureCursor featureCursor = null;
                try
                {
                    ////using (ComReleaser comReleaser = new ComReleaser())
                    ////{
                    featureCursor = featureClass.Search(spatialFilter, true);
                    ////comReleaser.ManageLifetime(featureCursor);
                    IFeature feature = featureCursor.NextFeature();
                    while (feature != null)
                    {
                        IProximityOperator proximityOperator = feature.ShapeCopy as IProximityOperator;
                        double distanceCurrent = proximityOperator.ReturnDistance(point);
                        if (distance > distanceCurrent)
                        {
                            distance = distanceCurrent;
                            featureClassID = featureClass.FeatureClassID;
                            featureGeometry = feature.ShapeCopy;
                        }

                        feature = featureCursor.NextFeature();
                    }
                    ////}
                }
                catch
                {
                    throw;
                }
                finally
                {
                    if (featureCursor != null)
                    {
                        Marshal.ReleaseComObject(featureCursor);
                    }
                }

                featureClass = enumFeatureClass.Next();
            }
        }

        /// <summary>
        /// Feature class from ID
        /// </summary>
        /// <param name="featureDataset">object featureDataset</param>
        /// <param name="id">ID feature Class</param>
        /// <returns>object feature class</returns>
        internal static IFeatureClass GetFeatureClassFromID(IFeatureDataset featureDataset, int id)
        {
            IEnumDataset datasets = featureDataset.Subsets;
            IDataset dataset = null;
            IFeatureClass fc = null;
            while ((dataset = datasets.Next()) != null)
            {
                if ((dataset as IFeatureClass).FeatureClassID == id)
                {
                    fc = dataset as IFeatureClass;
                    break;
                }
            }

            return fc;
        }

        /// <summary>
        /// Construct Offset
        /// </summary>
        /// <param name="polyline">object polyline</param>
        /// <param name="point">value of offset</param>
        /// <param name="offset">offset point</param>
        /// <returns>point with offset</returns>
        internal static IPoint ConstructOffset(IPolyline polyline, IPoint point, double? offset)
        {
            if (polyline == null || polyline.IsEmpty)
            {
                return null;
            }

            if (point == null || point.IsEmpty)
            {
                return null;
            }

            if ((!offset.HasValue) || double.IsNaN(offset.Value) || offset.Value == 0.0)
            {
                return point;
            }

            double distanceAlongCurve = 0;
            double distanceFromCurve = 0;
            bool rightSide = false;
            IPoint outPoint = new PointClass();
            polyline.QueryPointAndDistance(esriSegmentExtension.esriNoExtension, point, false, outPoint, ref distanceAlongCurve, ref distanceFromCurve, ref rightSide); 
            IConstructPoint constructPoint = outPoint as IConstructPoint;
            constructPoint.ConstructOffset(polyline, esriSegmentExtension.esriNoExtension, distanceAlongCurve, false, offset.Value);
            return constructPoint as IPoint;
        }

        /// <summary>
        /// Construct Offset
        /// </summary>
        /// <param name="polyline">object polyline</param>
        /// <param name="offset">offset polyline</param>
        /// <returns>polyline with offset</returns>
        internal static IPolyline ConstructOffset(IPolyline polyline, double? offset)
        {
            if (polyline == null || polyline.IsEmpty)
            {
                return null;
            }

            if ((!offset.HasValue) || double.IsNaN(offset.Value) || offset.Value == 0.0)
            {
                return polyline;
            }

            object missing = Type.Missing;
            IConstructCurve constructCurve = new PolylineClass();
            constructCurve.ConstructOffset(polyline, offset.Value, ref missing, ref missing);
            return constructCurve as IPolyline;
        }

        /// <summary>
        /// Trace Geometry Network
        /// </summary>
        /// <param name="geometricNetwork">Geometric Network</param>
        /// <param name="traceSolverType">trace Solver Type</param>
        /// <param name="flowElements">flow Elements</param>
        /// <param name="edgeFlags">edge Flags</param>
        /// <param name="junctionFlags">junction Flags</param>
        /// <param name="edgeBarriers">edge Barriers</param>
        /// <param name="junctionBarriers">junction Barriers</param>
        /// <param name="flowMethod">method flow</param>
        /// <param name="outFields">output fields</param>
        /// <param name="maxFeatures">max Features</param>
        /// <param name="tolerance">parameter tolerance</param>
        /// <param name="traceIndeterminateFlow">trace Indeterminate Flow</param>
        /// <param name="shortestPathObjFn">shortest Path Obj Fn</param>
        /// <param name="disableLayers">list of disable layers</param>
        /// <param name="netJunctionWeight">Junction Weight</param>
        /// <param name="netFromToEdgeWeight">FromTo Edge Weight</param>
        /// <param name="netToFromEdgeWeight">ToFrom Edge Weight</param>
        /// <param name="netJunctionFilterWeight">Junction Filter Weight</param>
        /// <param name="junctionFilterRangesFrom">junction Filter Ranges From</param>
        /// <param name="junctionFilterRangesTo">junction Filter Ranges To</param>
        /// <param name="junctionFilterNotOperator">junction Filter Not Operator</param>
        /// <param name="netFromToEdgeFilterWeight">FromTo Edge Filter Weight</param>
        /// <param name="netToFromEdgeFilterWeight">ToFrom Edge Filter Weight</param>
        /// <param name="edgeFilterRangesFrom">edge Filter Ranges From</param>
        /// <param name="edgeFilterRangesTo">edge Filter Ranges To</param>
        /// <param name="edgeFilterNotOperator">edge Filter Not Operator</param>
        /// <returns>Trace Geometric Network</returns>
        internal static byte[] GetTraceGeometryNetwork(ESRI.ArcGIS.Geodatabase.IGeometricNetwork geometricNetwork, TraceSolverType traceSolverType, esriFlowElements flowElements, List<IPoint> edgeFlags, List<IPoint> junctionFlags, List<IPoint> edgeBarriers, List<IPoint> junctionBarriers, esriFlowMethod flowMethod, string[] outFields, int maxFeatures, double tolerance, bool traceIndeterminateFlow, esriShortestPathObjFn shortestPathObjFn, List<int> disableLayers, INetWeight netJunctionWeight, INetWeight netFromToEdgeWeight, INetWeight netToFromEdgeWeight, INetWeight netJunctionFilterWeight, List<object> junctionFilterRangesFrom, List<object> junctionFilterRangesTo, bool junctionFilterNotOperator, INetWeight netFromToEdgeFilterWeight, INetWeight netToFromEdgeFilterWeight, List<object> edgeFilterRangesFrom, List<object> edgeFilterRangesTo, bool edgeFilterNotOperator)
        {
            IGeometricNetworkParameters geometricNetworkParameters = new GeometricNetworkParameters();
            geometricNetworkParameters.EdgeFlags = edgeFlags;
            geometricNetworkParameters.JunctionFlags = junctionFlags;
            geometricNetworkParameters.EdgeBarriers = edgeBarriers;
            geometricNetworkParameters.JunctionBarriers = junctionBarriers;
            geometricNetworkParameters.MaxFeatures = maxFeatures;
            geometricNetworkParameters.Tolerance = tolerance;
            geometricNetworkParameters.FlowElements = flowElements;

            IGeometricNetworkTracer geometricNetworkTracer = new GeometricNetworkTracer(geometricNetwork, geometricNetworkParameters, traceSolverType, flowMethod, traceIndeterminateFlow);
            geometricNetworkTracer.OutFields = outFields;
            geometricNetworkTracer.DisableLayers = disableLayers;
            geometricNetworkTracer.JunctionWeight = netJunctionWeight;
            geometricNetworkTracer.FromToEdgeWeight = netFromToEdgeWeight;
            geometricNetworkTracer.ToFromEdgeWeight = netToFromEdgeWeight;
            geometricNetworkTracer.JunctionFilterWeight = netJunctionFilterWeight;
            geometricNetworkTracer.JunctionFilterRangesFrom = junctionFilterRangesFrom;
            geometricNetworkTracer.JunctionFilterRangesTo = junctionFilterRangesTo;
            geometricNetworkTracer.JunctionFilterNotOperator = junctionFilterNotOperator;
            geometricNetworkTracer.FromToEdgeFilterWeight = netFromToEdgeFilterWeight;
            geometricNetworkTracer.ToFromEdgeFilterWeight = netToFromEdgeFilterWeight;
            geometricNetworkTracer.EdgeFilterRangesFrom = edgeFilterRangesFrom;
            geometricNetworkTracer.EdgeFilterRangesTo = edgeFilterRangesTo;
            geometricNetworkTracer.EdgeFilterNotOperator = edgeFilterNotOperator;

            if ((traceSolverType == TraceSolverType.FindPath) || (traceSolverType == TraceSolverType.FindSource))
            {
                geometricNetworkTracer.ShortestPathObjFn = shortestPathObjFn;
            }

            return geometricNetworkTracer.Solve().JsonByte();
        }

        /// <summary>
        /// Get IEnumNetEID from List of IEIDInfo
        /// </summary>
        /// <param name="eidInfos">list of IEIDInfo</param>
        /// <param name="elementType">element type of eid</param>
        /// <returns>object IEnumNetEID</returns>
        internal static IEnumNetEID GetEnumNetEID(List<IEIDInfo> eidInfos, esriElementType elementType)
        {
            IEnumNetEIDBuilderGEN enumNetEIDBuilder = new EnumNetEIDArrayClass();
            enumNetEIDBuilder.ElementType = elementType;
            foreach (IEIDInfo eidInfo in eidInfos)
            {
                enumNetEIDBuilder.Add(eidInfo.EID);
            }

            return enumNetEIDBuilder as IEnumNetEID;
        }
    }
}
