//-----------------------------------------------------------------------
// <copyright file="GeometricNetworkUtility.cs" company="Studio A&T s.r.l.">
//     Copyright (c) Studio A&T s.r.l. All rights reserved.
// </copyright>
// <author>Nicogis</author>
//-----------------------------------------------------------------------
namespace Studioat.ArcGis.Soe.Rest
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Text;
    using ESRI.ArcGIS.Carto;
    using ESRI.ArcGIS.esriSystem;
    using ESRI.ArcGIS.Geodatabase;
    using ESRI.ArcGIS.Geometry;
    using ESRI.ArcGIS.NetworkAnalysis;
    using ESRI.ArcGIS.Server;
    using ESRI.ArcGIS.SOESupport;

    /// <summary>
    /// Class SOE GeometryNetworkUtility
    /// </summary>
    [ComVisible(true)]
    [Guid("3de75f06-31f0-4fa2-8322-df8965bb3d68")]    
    [ClassInterface(ClassInterfaceType.None)]
    [ServerObjectExtension("MapServer",
        AllCapabilities = "Trace network,Isolate valve,Position along",
        DefaultCapabilities = "Trace network,Isolate valve,Position along",
        Description = "Geometric Network Utility",
        DisplayName = "Geometric Network Utility",
        Properties = "",
        SupportsREST = true,
        SupportsSOAP = false)]
    [SuppressMessage("Microsoft.StyleCop.CSharp.NamingRules", "SA1306:FieldNamesMustBeginWithLowerCaseLetter", Justification = "Warning FxCop - Error Code ESRI - Capabilities")]
    [SuppressMessage("Microsoft.StyleCop.CSharp.NamingRules", "SA1305:FieldNamesMustNotUseHungarianNotation", Justification = "Warning FxCop - Error Code ESRI - pSOH")]
    [SuppressMessage("Microsoft.Usage", "CA2201:DoNotRaiseReservedExceptionTypes", Justification = "-")]
    public class GeometricNetworkUtility : IServerObjectExtension, IObjectConstruct, IRESTRequestHandler
    {
        /// <summary>
        /// name of soe
        /// </summary>
        private string soeName;

        /// <summary>
        /// object serverObjectHelper
        /// </summary>
        private IServerObjectHelper serverObjectHelper;

        /// <summary>
        /// object logger
        /// </summary>
        private ServerLogger logger;

        /// <summary>
        /// request Handler 
        /// </summary>
        private IRESTRequestHandler requestHandler;

        /// <summary>
        /// List of GeometricNetworkInfo
        /// </summary>
        private List<GeometricNetworkInfo> geometricNetworkInfos;

        /// <summary>
        /// separator string for Weight Ranges 
        /// </summary>
        private string separatorWeightRanges;

        /// <summary>
        /// Initializes a new instance of the GeometricNetworkUtility class.
        /// </summary>
        public GeometricNetworkUtility()
        {
            this.soeName = this.GetType().Name;
            this.logger = new ServerLogger();
            this.requestHandler = new SoeRestImpl(this.soeName, this.CreateRestSchema()) as IRESTRequestHandler;
        }

        #region IServerObjectExtension Members

        /// <summary>
        /// Event Init SOE
        /// </summary>
        /// <param name="pSOH">object IServerObjectHelper</param>
        public void Init(IServerObjectHelper pSOH)
        {
            this.serverObjectHelper = pSOH;
        }

        /// <summary>
        /// Event Shutdown SOE
        /// </summary>
        public void Shutdown()
        {
        }

        #endregion

        #region IObjectConstruct Members

        /// <summary>
        /// Event Construct SOE
        /// </summary>
        /// <param name="props">properties of SOE</param>
        public void Construct(IPropertySet props)
        {
            AutoTimer timer = new AutoTimer();
            this.logger.LogMessage(ServerLogger.msgType.infoSimple, "Construct", -1, this.soeName + " Construct has started.");
            this.GetGeometricNetworkInfos();
            NumberFormatInfo numericFormatInfo = CultureInfo.CurrentCulture.NumberFormat;
            this.separatorWeightRanges = (numericFormatInfo.NumberDecimalSeparator == ".") ? "," : ";";
            this.logger.LogMessage(ServerLogger.msgType.infoSimple, "Construct", -1, timer.Elapsed, this.soeName + " Construct has completed.");
        }

        #endregion

        #region IRESTRequestHandler Members

        /// <summary>
        /// Get schema of SOE
        /// </summary>
        /// <returns>schema of SOE</returns>
        public string GetSchema()
        {
            return this.requestHandler.GetSchema();
        }

        /// <summary>
        /// Handle REST Request
        /// </summary>
        /// <param name="Capabilities">capabilities soe</param>
        /// <param name="resourceName">name of resource</param>
        /// <param name="operationName">name of operation</param>
        /// <param name="operationInput">operation Input</param>
        /// <param name="outputFormat">output Format</param>
        /// <param name="requestProperties">request Properties</param>
        /// <param name="responseProperties">response Properties</param>
        /// <returns>object byte[]</returns>
        public byte[] HandleRESTRequest(string Capabilities, string resourceName, string operationName, string operationInput, string outputFormat, string requestProperties, out string responseProperties)
        {
            return this.requestHandler.HandleRESTRequest(Capabilities, resourceName, operationName, operationInput, outputFormat, requestProperties, out responseProperties);
        }

        #endregion

        /// <summary>
        /// Create schema SOE
        /// </summary>
        /// <returns>Rest Resource</returns>
        private RestResource CreateRestSchema()
        {
            ////resource root
            RestResource rootResource = new RestResource(this.soeName, false, this.RootResourceHandler);

            RestResource infoResource = new RestResource("Info", false, this.InfoResHandler);
            rootResource.resources.Add(infoResource);

            RestResource helpResource = new RestResource("Help", false, this.HelpResHandler);
            rootResource.resources.Add(helpResource);

            ////resource geometricNetworks
            RestResource geometricNetworksResource = new RestResource("GeometricNetworks", true, new ResourceHandler(this.GeometricNeworkFeatureClass));
            ////operation TraceNetwork
            RestOperation traceNetworkOperation = new RestOperation("TraceNetwork", new string[] { "traceSolverType", "flowMethod", "flowElements", "edgeFlags", "junctionFlags", "edgeBarriers", "junctionBarriers", "outFields", "maxTracedFeatures", "tolerance", "traceIndeterminateFlow", "shortestPathObjFn", "disableLayers", "junctionWeight", "fromToEdgeWeight", "toFromEdgeWeight", "junctionFilterWeight", "junctionFilterRanges", "junctionFilterNotOperator", "fromToEdgeFilterWeight", "toFromEdgeFilterWeight", "edgeFilterRanges", "edgeFilterNotOperator" }, new string[] { "json" }, this.TraceGeometryNetwork, "Trace network");
            ////operation IsolateValve
            RestOperation isolateValveOperation = new RestOperation("IsolateValve", new string[] { "stationLayerId", "valveLayerId", "flowElements", "edgeFlags", "junctionFlags", "edgeBarriers", "junctionBarriers", "outFields", "maxTracedFeatures", "tolerance" }, new string[] { "json" }, this.IsolateValve, "Isolate valve");
            ////operation PosAlong
            RestOperation traceNetworkPosAlongOperation = new RestOperation("TraceNetworkPosAlong", new string[] { "edgeFlags", "length", "fieldLevel", "offset", "tolerance", "sameOrder" }, new string[] { "json" }, this.TraceGeometryNetworkPosAlong, "Position along");

            geometricNetworksResource.operations.Add(traceNetworkOperation);
            geometricNetworksResource.operations.Add(isolateValveOperation);
            geometricNetworksResource.operations.Add(traceNetworkPosAlongOperation);
            rootResource.resources.Add(geometricNetworksResource);
            return rootResource;
        }

        /// <summary>
        /// Handler Root Resource
        /// </summary>
        /// <param name="boundVariables">bound Variables</param>
        /// <param name="outputFormat">output Format</param>
        /// <param name="requestProperties">request Properties</param>
        /// <param name="responseProperties">response Properties</param>
        /// <returns>object byte[]</returns>
        private byte[] RootResourceHandler(NameValueCollection boundVariables, string outputFormat, string requestProperties, out string responseProperties)
        {
            responseProperties = null;

            List<GeometricNetworkInfo> layerInfos = this.geometricNetworkInfos;
            JsonObject[] objectArray = System.Array.ConvertAll(layerInfos.ToArray(), i => i.ToJsonObject());
            JsonObject jsonObject = new JsonObject();
            jsonObject.AddArray("GeometricNetworks", objectArray);
            return Encoding.UTF8.GetBytes(jsonObject.ToJson());
        }

        /// <summary>
        /// Returns JSON representation of Help resource. This resource is not a collection.
        /// </summary>
        /// <param name="boundVariables">list of variables bound</param>
        /// <param name="outputFormat">format of output</param>
        /// <param name="requestProperties">list of request properties</param>
        /// <param name="responseProperties">list of response properties </param>
        /// <returns>String JSON representation of Help resource.</returns>
        private byte[] HelpResHandler(NameValueCollection boundVariables, string outputFormat, string requestProperties, out string responseProperties)
        {
            responseProperties = "{\"Content-Type\" : \"application/json\"}";

            JsonObject result = new JsonObject();

            JsonObject soeResources = new JsonObject();
            soeResources.AddString("GeometricNetworks", "A list of geometric network in the map. Operations return 'hasError' = true and 'errorDescription' (string) if there is an error.");
            result.AddJsonObject("Resources", soeResources);

            JsonObject getTraceNetworkInputs = new JsonObject();
            getTraceNetworkInputs.AddString("traceSolverType", "(string) FindAccumulation or FindCircuits or FindCommonAncestors or FindFlowElements or FindFlowEndElements or FindFlowUnreachedElements or FindPath or FindSource or FindLongest");
            getTraceNetworkInputs.AddString("flowMethod", "(string) enum arcobjects esriFlowMethod");
            getTraceNetworkInputs.AddString("flowElements", "(string) enum arcobjects esriFlowElements");
            getTraceNetworkInputs.AddString("edgeFlags", "array of geometries(point) (see rest esri point)");
            getTraceNetworkInputs.AddString("junctionFlags", "array of geometries(point) (see rest esri point)");
            getTraceNetworkInputs.AddString("edgeBarriers", "array of geometries(point) (see rest esri point)");
            getTraceNetworkInputs.AddString("junctionBarriers", "array of geometries(point) (see rest esri point)");
            getTraceNetworkInputs.AddString("maxTracedFeatures", "max number of edge or junction that can be returned");
            getTraceNetworkInputs.AddString("tolerance", "in map units to search for flag or barrier");
            getTraceNetworkInputs.AddString("traceIndeterminateFlow", "booleran (optional)");
            getTraceNetworkInputs.AddString("shortestPathObjFn", "required only for FindPath or FindSource (optional)");
            getTraceNetworkInputs.AddString("disableLayers", "array of int. Id of layers (optional)");
            getTraceNetworkInputs.AddString("outFields", "list of fields in result ('*' for all fields). This list is common for all feature class in geometric network.");
            getTraceNetworkInputs.AddString("junctionWeight", "(optional)");
            getTraceNetworkInputs.AddString("fromToEdgeWeight", "(optional)");
            getTraceNetworkInputs.AddString("toFromEdgeWeight", "(optional)");
            getTraceNetworkInputs.AddString("junctionFilterWeight", "(optional)");
            getTraceNetworkInputs.AddString("junctionFilterRanges", "(optional)");
            getTraceNetworkInputs.AddString("junctionFilterNotOperator", "boolean (optional)");
            getTraceNetworkInputs.AddString("fromToEdgeFilterWeight", "(optional)");
            getTraceNetworkInputs.AddString("toFromEdgeFilterWeight", "(optional)");
            getTraceNetworkInputs.AddString("edgeFilterRanges", "(optional)");
            getTraceNetworkInputs.AddString("edgeFilterNotOperator", "boolean (optional)");

            JsonObject getTraceNetworkOutput = new JsonObject();
            getTraceNetworkOutput.AddString("barriersNotFound", "array of geometries(point) (see rest esri point)");
            getTraceNetworkOutput.AddString("flagsNotFound", "array of geometries(point) (see rest esri point)");
            getTraceNetworkOutput.AddString("edges", "array of feature (see rest esri feature)");
            getTraceNetworkOutput.AddString("junctions", "array of feature (see rest esri feature)");

            JsonObject getTraceNetworkParams = new JsonObject();
            getTraceNetworkParams.AddString("Info", "Trace Network. To learn more about formatting the input geometries, input geometry, please visit the 'Geometry Objects' section of the ArcGIS Server REST documentation.");
            getTraceNetworkParams.AddJsonObject("Inputs", getTraceNetworkInputs);
            getTraceNetworkParams.AddJsonObject("Outputs", getTraceNetworkOutput);

            JsonObject getIsolateValveInputs = new JsonObject();
            getIsolateValveInputs.AddString("stationLayerId", "(int) Id of station layer");
            getIsolateValveInputs.AddString("valveLayerId", "(int) Id of valve layer");
            getIsolateValveInputs.AddString("flowElements", "enum arcobjects esriFlowElements");
            getIsolateValveInputs.AddString("edgeFlags", "array of geometries(point) (see rest esri point)");
            getIsolateValveInputs.AddString("junctionFlags", "array of geometries(point) (see rest esri point)");
            getIsolateValveInputs.AddString("edgeBarriers", "not used");
            getIsolateValveInputs.AddString("junctionBarriers", "not used");
            getIsolateValveInputs.AddString("maxTracedFeatures", "max number of edge or junction that can be returned");
            getIsolateValveInputs.AddString("tolerance", "in map units to search for flag");
            getIsolateValveInputs.AddString("outFields", "list of fields in result ('*' for all fields). This list is common for all feature class in geometric network.");
            
            JsonObject getIsolateValveOutput = new JsonObject();
            getIsolateValveOutput.AddString("flagsNotFound", "array of geometries(point) (see rest esri point)");
            getIsolateValveOutput.AddString("edges", "array of feature (see rest esri feature)");
            getIsolateValveOutput.AddString("junctions", "array of feature (see rest esri feature)");
            getIsolateValveOutput.AddString("valves", "array of feature (see rest esri feature)");

            JsonObject getIsolateValveParams = new JsonObject();
            getIsolateValveParams.AddString("Info", "Isolate valve. To learn more about formatting the input geometries, input geometry, please visit the 'Geometry Objects' section of the ArcGIS Server REST documentation.");
            getIsolateValveParams.AddJsonObject("Inputs", getIsolateValveInputs);
            getIsolateValveParams.AddJsonObject("Outputs", getIsolateValveOutput);

            JsonObject getTraceNetworkPosAlongInputs = new JsonObject();
            getTraceNetworkPosAlongInputs.AddString("edgeFlags", "array of geometries(point) (see rest esri point). For now used only the first edge flag.");
            getTraceNetworkPosAlongInputs.AddString("length", "(double) distance from edgeFlags along geometric network. Negative value along upstream, positive value along downstream");
            getTraceNetworkPosAlongInputs.AddString("fieldLevel", "(string) field with Strahler stream order. It is read in edge. Used if you set parameter sameOrder = true");
            getTraceNetworkPosAlongInputs.AddString("offset", "double (optional) offset from geometric network");
            getTraceNetworkPosAlongInputs.AddString("tolerance", "in map units to search for flag");
            getTraceNetworkPosAlongInputs.AddString("sameOrder", "(bool) optional default = false. If you set true the trace stop when the start Strahler stream order change.");

            JsonObject getTraceNetworkPosAlongOutput = new JsonObject();
            getTraceNetworkPosAlongOutput.AddString("geometry", "(geometry) polyline or point (see rest esri polyline or point)");
            getTraceNetworkPosAlongOutput.AddString("message", "(string) message if length exceed stream");

            JsonObject getTraceNetworkPosAlongParams = new JsonObject();
            getTraceNetworkPosAlongParams.AddString("Info", "Position along geometric network. Requirements: simple egde, flow defined and digitalized in same direction of flow");
            getTraceNetworkPosAlongParams.AddJsonObject("Inputs", getTraceNetworkPosAlongInputs);
            getTraceNetworkPosAlongParams.AddJsonObject("Outputs", getTraceNetworkPosAlongOutput);

            JsonObject soeOperations = new JsonObject();
            soeOperations.AddJsonObject("TraceNetwork", getTraceNetworkParams);
            soeOperations.AddJsonObject("IsolateValve", getIsolateValveParams);
            soeOperations.AddJsonObject("TraceNetworkPosAlong", getTraceNetworkPosAlongParams);

            result.AddJsonObject("Operations", soeOperations);

            return result.JsonByte();
        }

        /// <summary>
        /// Returns JSON representation of Info resource. This resource is not a collection.
        /// </summary>
        /// <param name="boundVariables">list of variables bound</param>
        /// <param name="outputFormat">format of output</param>
        /// <param name="requestProperties">list of request properties</param>
        /// <param name="responseProperties">list of response properties </param>
        /// <returns>String JSON representation of Info resource.</returns>
        private byte[] InfoResHandler(NameValueCollection boundVariables, string outputFormat, string requestProperties, out string responseProperties)
        {
            responseProperties = "{\"Content-Type\" : \"application/json\"}";

            JsonObject result = new JsonObject();
            AddInPackageAttribute addInPackage = (AddInPackageAttribute)Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AddInPackageAttribute), false)[0];
            result.AddString("agsVersion", addInPackage.TargetVersion);
            result.AddString("soeVersion", addInPackage.Version);
            result.AddString("author", addInPackage.Author);
            result.AddString("company", addInPackage.Company);

            return Encoding.UTF8.GetBytes(result.ToJson());
        }

        /// <summary>
        /// resource Geometry Network
        /// </summary>
        /// <param name="boundVariables">list of variables bound</param>
        /// <param name="outputFormat">format of output</param>
        /// <param name="requestProperties">list of request properties</param>
        /// <param name="responseProperties">list of response properties </param>
        /// <returns>resource in byte</returns>
        private byte[] GeometricNeworkFeatureClass(NameValueCollection boundVariables, string outputFormat, string requestProperties, out string responseProperties)
        {
            responseProperties = null;
            if (boundVariables["GeometricNetworksID"] == null)
            {
                List<GeometricNetworkInfo> layerInfos = this.geometricNetworkInfos;
                JsonObject[] objectArray = System.Array.ConvertAll(layerInfos.ToArray(), i => i.ToJsonObject());
                JsonObject jsonObject = new JsonObject();
                jsonObject.AddArray("GeometricNetworks", objectArray);
                return Encoding.UTF8.GetBytes(jsonObject.ToJson());
            }
            else
            {
                int id = Convert.ToInt32(boundVariables["GeometricNetworksID"], CultureInfo.InvariantCulture);
                string s = this.geometricNetworkInfos.Find(i => i.ID == id).ToJsonObject().ToJson();
                return Encoding.UTF8.GetBytes(s);
            }
        }

        /// <summary>
        /// From service return list of layer with Geometry Network
        /// </summary>
        private void GetGeometricNetworkInfos()
        {
            IMapServer3 serverObject = this.GetMapServer();
            IMapLayerInfos mapLayerInfos = serverObject.GetServerInfo(serverObject.DefaultMapName).MapLayerInfos;

            this.geometricNetworkInfos = new List<GeometricNetworkInfo>();
            for (int i = 0; i < mapLayerInfos.Count; i++)
            {
                IMapLayerInfo mapLayerInfo = mapLayerInfos.get_Element(i);
                if (mapLayerInfo.IsFeatureLayer)
                {
                    IFeatureClass featureClass = this.GetFeatureClass(mapLayerInfo.ID);
                    IFeatureDataset featureDataset = featureClass.FeatureDataset;
                    if ((featureDataset != null) && ((featureClass.FeatureType == esriFeatureType.esriFTSimpleJunction) || (featureClass.FeatureType == esriFeatureType.esriFTSimpleEdge) || (featureClass.FeatureType == esriFeatureType.esriFTComplexJunction) || (featureClass.FeatureType == esriFeatureType.esriFTComplexEdge)))
                    {
                        INetworkCollection networkCollection = featureDataset as INetworkCollection;
                        if (networkCollection != null && networkCollection.GeometricNetworkCount > 0)
                        {
                            for (int j = 0; j < networkCollection.GeometricNetworkCount; j++)
                            {
                                ESRI.ArcGIS.Geodatabase.IGeometricNetwork geometricNetwork = networkCollection.GeometricNetwork[j];
                                IFeatureClassContainer featureClassContainer = geometricNetwork as IFeatureClassContainer;
                                if (featureClassContainer.get_ClassByID(featureClass.FeatureClassID) != null)
                                {
                                    if (!this.geometricNetworkInfos.Exists(gn => gn.GeometricNetwork == geometricNetwork))
                                    {
                                        IDataset dataset = geometricNetwork as IDataset;
                                        this.geometricNetworkInfos.Add(new GeometricNetworkInfo(this.geometricNetworkInfos.Count + 1, dataset.Name, geometricNetwork));
                                    }

                                    break;
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Operation Isolate Valve
        /// </summary>
        /// <param name="boundVariables">bound Variables</param>
        /// <param name="operationInput">operation Input</param>
        /// <param name="outputFormat">output Format</param>
        /// <param name="requestProperties">request Properties</param>
        /// <param name="responseProperties">response Properties</param>
        /// <returns>Isolate Valve</returns>
        private byte[] IsolateValve(NameValueCollection boundVariables, JsonObject operationInput, string outputFormat, string requestProperties, out string responseProperties)
        {
            responseProperties = null;

            string methodName = MethodBase.GetCurrentMethod().Name;

            ////geometric Network id
            int id;
            try
            {
                id = Convert.ToInt32(boundVariables["GeometricNetworksID"], CultureInfo.InvariantCulture);
            }
            catch
            {
                throw new ArgumentException("geometric Network id not valid", methodName);
            }

            GeometricNetworkInfo geometricNetworkInfo = this.geometricNetworkInfos.Find(i => i.ID == id);
            if (geometricNetworkInfo == null)
            {
                throw new ArgumentException("geometric Network id not found", methodName);
            }

            ESRI.ArcGIS.Geodatabase.IGeometricNetwork geometricNetwork = geometricNetworkInfo.GeometricNetwork;

            ////station id
            long? stationId;
            bool found = operationInput.TryGetAsLong("stationLayerId", out stationId);
            if (!found || !stationId.HasValue)
            {
                throw new ArgumentException("stationLayerId not specified", methodName);
            }

            if (stationId.Value > int.MaxValue)
            {
                throw new ArgumentException("stationLayerId not valid", methodName);
            }

            ////valve id
            long? valveId;
            found = operationInput.TryGetAsLong("valveLayerId", out valveId);
            if (!found || !valveId.HasValue)
            {
                throw new ArgumentException("valveLayerId not specified", methodName);
            }

            if (valveId.Value > int.MaxValue)
            {
                throw new ArgumentException("valveLayerId not valid", methodName);
            }

            ////flowElements
            string flowElementsString;
            found = operationInput.TryGetString("flowElements", out flowElementsString);
            if (!found || string.IsNullOrEmpty(flowElementsString))
            {
                throw new ArgumentException("flowElements not specified", methodName);
            }

            esriFlowElements flowElements;
            try
            {
                flowElements = (esriFlowElements)Enum.Parse(typeof(esriFlowElements), flowElementsString);
            }
            catch
            {
                throw new ArgumentException("flowElements not valid", methodName);
            }

            ////edge flags
            object[] jsonEdgeFlags;
            List<IPoint> edgeFlags = new List<IPoint>();
            if (operationInput.TryGetArray("edgeFlags", out jsonEdgeFlags))
            {
                JsonObject[] joEdgeFlags = null;
                try
                {
                    joEdgeFlags = jsonEdgeFlags.Cast<JsonObject>().ToArray();
                }
                catch
                {
                    throw new ArgumentException("invalid edge flag", methodName);
                }

                foreach (JsonObject jo in joEdgeFlags)
                {
                    IPoint location = Conversion.ToGeometry(jo, esriGeometryType.esriGeometryPoint) as IPoint;
                    if (location == null)
                    {
                        throw new ArgumentException("invalid edgeFlags", methodName);
                    }

                    edgeFlags.Add(location);
                }
            }

            //// junction Flags
            object[] jsonJunctionFlags;
            List<IPoint> junctionFlags = new List<IPoint>();
            if (operationInput.TryGetArray("junctionFlags", out jsonJunctionFlags))
            {
                JsonObject[] joJunctionFlags = null;
                try
                {
                    joJunctionFlags = jsonJunctionFlags.Cast<JsonObject>().ToArray();
                }
                catch
                {
                    throw new ArgumentException("invalid junctionFlags", methodName);
                }

                foreach (JsonObject jo in joJunctionFlags)
                {
                    IPoint location = Conversion.ToGeometry(jo, esriGeometryType.esriGeometryPoint) as IPoint;
                    if (location == null)
                    {
                        throw new ArgumentException("invalid junctionFlags", methodName);
                    }

                    junctionFlags.Add(location);
                }
            }

            if ((edgeFlags.Count == 0) && (junctionFlags.Count == 0))
            {
                throw new ArgumentException("edgeFlags and/or junctionFlags not found", methodName);
            }

            //// edge Barriers
            object[] jsonEdgeBarriers;
            List<IPoint> edgeBarriers = new List<IPoint>();
            if (operationInput.TryGetArray("edgeBarriers", out jsonEdgeBarriers))
            {
                JsonObject[] joEdgeBarriers = null;
                try
                {
                    joEdgeBarriers = jsonEdgeBarriers.Cast<JsonObject>().ToArray();
                }
                catch
                {
                    throw new ArgumentException("invalid edgeBarriers", methodName);
                }

                foreach (JsonObject jo in joEdgeBarriers)
                {
                    IPoint location = Conversion.ToGeometry(jo, esriGeometryType.esriGeometryPoint) as IPoint;
                    if (location == null)
                    {
                        throw new ArgumentException("invalid edgeBarriers", methodName);
                    }

                    edgeBarriers.Add(location);
                }
            }

            //// junction Barriers
            object[] jsonJunctionBarriers;
            List<IPoint> junctionBarriers = new List<IPoint>();
            if (operationInput.TryGetArray("junctionBarriers", out jsonJunctionBarriers))
            {
                JsonObject[] joJunctionBarriers = null;
                try
                {
                    joJunctionBarriers = jsonJunctionBarriers.Cast<JsonObject>().ToArray();
                }
                catch
                {
                    throw new ArgumentException("invalid junctionBarriers", methodName);
                }

                foreach (JsonObject jo in joJunctionBarriers)
                {
                    IPoint location = Conversion.ToGeometry(jo, esriGeometryType.esriGeometryPoint) as IPoint;
                    if (location == null)
                    {
                        throw new ArgumentException("invalid junctionBarriers", methodName);
                    }

                    junctionBarriers.Add(location);
                }
            }

            ////outFields
            string outFields;
            found = operationInput.TryGetString("outFields", out outFields);
            if (!found || string.IsNullOrEmpty(outFields))
            {
                throw new ArgumentException("invalid outFields", methodName);
            }

            string[] fields = outFields.Split(',');

            ////maxFeatures
            long? maxFeatures;
            found = operationInput.TryGetAsLong("maxTracedFeatures", out maxFeatures);
            if (!found || !maxFeatures.HasValue)
            {
                throw new ArgumentException("invalid maxTracedFeatures", methodName);
            }

            if (maxFeatures.Value > int.MaxValue)
            {
                throw new ArgumentException("invalid maxTracedFeatures", methodName);
            }

            ////tolerance
            double? tolerance;
            found = operationInput.TryGetAsDouble("tolerance", out tolerance);
            if (!found || !tolerance.HasValue)
            {
                throw new ArgumentException("invalid tolerance", methodName);
            }

            if (tolerance.Value < 0)
            {
                throw new ArgumentException("invalid tolerance", methodName);
            }

            return this.GetIsolateValve(geometricNetwork, (int)stationId.Value, (int)valveId.Value, flowElements, edgeFlags, junctionFlags, edgeBarriers, junctionBarriers, fields, (int)maxFeatures.Value, tolerance.Value);
        }

        /// <summary>
        /// operation Trace Geometry Network Position Along
        /// </summary>
        /// <param name="boundVariables">bound Variables</param>
        /// <param name="operationInput">operation Input</param>
        /// <param name="outputFormat">output Format</param>
        /// <param name="requestProperties">request Properties</param>
        /// <param name="responseProperties">response Properties</param>
        /// <returns>Trace Geometry Network</returns>
        private byte[] TraceGeometryNetworkPosAlong(NameValueCollection boundVariables, JsonObject operationInput, string outputFormat, string requestProperties, out string responseProperties)
        {
            responseProperties = null;
            string methodName = MethodBase.GetCurrentMethod().Name;

            // geometric Network id
            int id;
            try
            {
                id = Convert.ToInt32(boundVariables["GeometricNetworksID"], CultureInfo.InvariantCulture);
            }
            catch
            {
                throw new ArgumentException("geometric Network id not valid", methodName);
            }

            GeometricNetworkInfo geometricNetworkInfo = this.geometricNetworkInfos.Find(i => i.ID == id);
            if (geometricNetworkInfo == null)
            {
                throw new ArgumentException("geometric Network id not found", methodName);
            }

            ESRI.ArcGIS.Geodatabase.IGeometricNetwork geometricNetwork = geometricNetworkInfo.GeometricNetwork;

            // edge flags
            object[] jsonEdgeFlags;
            List<IPoint> edgeFlags = new List<IPoint>();
            if (operationInput.TryGetArray("edgeFlags", out jsonEdgeFlags))
            {
                JsonObject[] joEdgeFlags = null;
                try
                {
                    joEdgeFlags = jsonEdgeFlags.Cast<JsonObject>().ToArray();
                }
                catch
                {
                    throw new ArgumentException("invalid edge flag", methodName);
                }

                foreach (JsonObject jo in joEdgeFlags)
                {
                    IPoint location = Conversion.ToGeometry(jo, esriGeometryType.esriGeometryPoint) as IPoint;
                    if (location == null)
                    {
                        throw new ArgumentException("invalid edgeFlags", methodName);
                    }

                    edgeFlags.Add(location);
                }
            }

            //// junction Flags
            //// object[] jsonJunctionFlags;
            //// List<IPoint> junctionFlags = new List<IPoint>();
            //// if (operationInput.TryGetArray("junctionFlags", out jsonJunctionFlags))
            //// {
            ////    JsonObject[] joJunctionFlags = null;
            ////    try
            ////    {
            ////        joJunctionFlags = jsonJunctionFlags.Cast<JsonObject>().ToArray();
            ////    }
            ////    catch
            ////    {
            ////        throw new ArgumentException("invalid junctionFlags", methodName);
            ////    }

            ////    foreach (JsonObject jo in joJunctionFlags)
            ////    {
            ////        IPoint location = Conversion.ToGeometry(jo, esriGeometryType.esriGeometryPoint) as IPoint;
            ////        if (location == null)
            ////        {
            ////            throw new ArgumentException("invalid junctionFlags", methodName);
            ////        }

            ////        junctionFlags.Add(location);
            ////    }
            ////}

            ////if ((edgeFlags.Count == 0) && (junctionFlags.Count == 0))
            ////{
            ////    throw new ArgumentException("edgeFlags and/or junctionFlags not found", methodName);
            ////}

            if (edgeFlags.Count != 1)
            {
                throw new ArgumentException("edgeFlags != 1", methodName);
            }

            // length (downstream positive value - upstream negative value)
            double? lengthValue;
            bool found = operationInput.TryGetAsDouble("length", out lengthValue);
            if (!found || !lengthValue.HasValue)
            {
                throw new ArgumentException("distance not specified", methodName);
            }

            double length = lengthValue.Value;

            if (length == 0)
            {
                throw new ArgumentException("distance not valid", methodName);
            }

            bool sameStream = false;
            bool? sameStreamValue;
            found = operationInput.TryGetAsBoolean("sameOrder", out sameStreamValue);
            if (found)
            {
                sameStream = sameStreamValue.Value;
            }

            string fieldLevel = null;
            if ((lengthValue.Value < 0) || ((lengthValue.Value > 0) && sameStream))
            {
                found = operationInput.TryGetString("fieldLevel", out fieldLevel);
                if (!found || string.IsNullOrEmpty(fieldLevel))
                {
                    throw new ArgumentException("fieldLevel not specified", methodName);
                } 
            }

            // offset
            double? offsetValue = double.NaN;
            operationInput.TryGetAsDouble("offset", out offsetValue);

            // tolerance
            double? toleranceValue;
            found = operationInput.TryGetAsDouble("tolerance", out toleranceValue);
            if (!found || !toleranceValue.HasValue)
            {
                throw new ArgumentException("invalid tolerance", methodName);
            }

            if (toleranceValue.Value < 0)
            {
                throw new ArgumentException("invalid tolerance", methodName);
            }

            double tolerance = toleranceValue.Value;

            try
            {
                List<IGeometry> flagNotFound = new List<IGeometry>();
                IPoint point = edgeFlags[0];
                int eid = Helper.GetEIDFromPoint(geometricNetwork, tolerance, point, esriElementType.esriETEdge);

                if (eid < 1)
                {
                    flagNotFound.Add(point);
                    JsonObject result = new JsonObject();
                    result.AddArray("flagsNotFound", Helper.GetListJsonObjects(flagNotFound));
                    return result.JsonByte();
                }
                
                INetElements networkElements = (INetElements)geometricNetwork.Network;
                esriFlowElements flowElements = esriFlowElements.esriFEEdges;

                ITraceFlowSolverGEN traceFlowSolver = new TraceFlowSolverClass() as ITraceFlowSolverGEN;
                INetSolver netSolver = traceFlowSolver as INetSolver;
                netSolver.SourceNetwork = geometricNetwork.Network;
                traceFlowSolver.TraceIndeterminateFlow = false;

                IFeatureDataset featureDataset = geometricNetwork.FeatureDataset;

                IGeometry geometry = null;
                string messageInfo = null;

                //// downstream
                if (length > 0)
                {
                    //// edge Flags
                    ////if (edgeFlags.Count == 1)
                    ////{
                        
                        INetElements netElements = geometricNetwork.Network as INetElements;
                        int featureClassID, featureID, subID;
                        netElements.QueryIDs(eid, esriElementType.esriETEdge, out featureClassID, out featureID, out subID);

                        INetFlag netFlag = new EdgeFlagClass();
                        netFlag.UserClassID = featureClassID;
                        netFlag.UserID = featureID;
                        netFlag.UserSubID = subID;
                        IEdgeFlag[] efs = new IEdgeFlag[1];
                        efs[0] = netFlag as IEdgeFlag;
                        traceFlowSolver.PutEdgeOrigins(ref efs);

                    if (sameStream)
                    {
                        bool error = false;
                        bool result = false;
                        IEnumNetEIDBuilderGEN eids = new EnumNetEIDArrayClass();
                        eids.ElementType = esriElementType.esriETEdge;
                        eids.Add(eid);
                        IFeature f = Helper.GetFeatureClassFromID(featureDataset, featureClassID).GetFeature(featureID);
                        int idxLivello = f.Fields.FindField(fieldLevel);
                        int livelloCurrent = int.Parse(f.get_Value(idxLivello).ToString());
                        while (true)
                        {
                            int fromEIDJunction, toEIDJunction;
                            INetTopologyEditGEN netTopology = networkElements as INetTopologyEditGEN;
                            netTopology.GetFromToJunctionEIDs(eid, out fromEIDJunction, out toEIDJunction);

                            int edgeCount = netTopology.GetAdjacentEdgeCount(toEIDJunction);

                            ////check if all edges into junction
                            int inEdges = 0;

                            int eidCurrent = -1;
                            for (int i = 0; i < edgeCount; i++)
                            {
                                bool reverseOrientation;
                                int adjacentEdge;
                                netTopology.GetAdjacentEdge(toEIDJunction, i, out adjacentEdge, out reverseOrientation);
                                ////exit at junction
                                if (reverseOrientation == false) 
                                {
                                    int userClassId, userId, userSubId;
                                    networkElements.QueryIDs(adjacentEdge, esriElementType.esriETEdge, out userClassId, out userId, out userSubId);
                                    f = Helper.GetFeatureClassFromID(featureDataset, userClassId).GetFeature(userId);
                                    idxLivello = f.Fields.FindField(fieldLevel);
                                    int livello = int.Parse(f.get_Value(idxLivello).ToString());
                                    if (livello <= livelloCurrent)
                                    {
                                        eidCurrent = adjacentEdge;
                                    }
                                    else
                                    {
                                        result = true;
                                    }

                                    ////only one exit
                                    break; 
                                }
                                else
                                {
                                    ++inEdges;
                                }
                            }

                            if ((inEdges == edgeCount) || result)
                            {
                                break;
                            }

                            if (eidCurrent == -1)
                            {
                                error = true;
                                break;
                            }

                            eids.Add(eidCurrent);
                            eid = eidCurrent;
                        }

                        if (error || (eids as IEnumNetEID).Count == 0)
                        {
                            throw new GeometricNetworkException("Error in find of downstream. Check level in geometric network");
                        }
                        else
                        {
                            geometry = Helper.GetPolylinePosAlong(geometricNetwork, eids as IEnumNetEID, length, point, offsetValue, ref messageInfo);
                        }
                    }
                    else
                    {
                        IEnumNetEID resultJunctions, resultEdges;
                        traceFlowSolver.FindFlowElements(esriFlowMethod.esriFMDownstream, flowElements, out resultJunctions, out resultEdges);

                        if ((resultEdges != null) || (resultEdges.Count != 0))
                        {
                            geometry = Helper.GetPolylinePosAlong(geometricNetwork, resultEdges as IEnumNetEID, length, point, offsetValue, ref messageInfo);
                        }
                        else
                        {
                            throw new GeometricNetworkException("Result not found!");
                        }
                    }

                    ////}
                }
                else if (lengthValue < 0)
                {
                    ////if (edgeFlags.Count == 1)
                    ////{
                        bool error = false;

                        IEnumNetEIDBuilderGEN eids = new EnumNetEIDArrayClass();
                        eids.ElementType = esriElementType.esriETEdge;
                        eids.Add(eid);
                        int numEdges = geometricNetwork.Network.EdgeCount;
                        int countLoop = 0;
                        int levelCurrent = -1;
                        while (true)
                        {
                            int fromEIDJunction, toEIDJunction;
                            INetTopologyEditGEN netTopology = networkElements as INetTopologyEditGEN;
                            netTopology.GetFromToJunctionEIDs(eid, out fromEIDJunction, out toEIDJunction);

                            int edgeCount = netTopology.GetAdjacentEdgeCount(fromEIDJunction);
                            if (edgeCount == 1)
                            {
                                break;
                            }

                            if (sameStream)
                            {
                                int userClassID, userID, userSubID;
                                networkElements.QueryIDs(eid, esriElementType.esriETEdge, out userClassID, out userID, out userSubID);
                                IFeature f = Helper.GetFeatureClassFromID(featureDataset, userClassID).GetFeature(userID);
                                int idxOrder = f.Fields.FindField(fieldLevel);
                                levelCurrent = int.Parse(f.get_Value(idxOrder).ToString());
                            }

                            int outEdges = 0;
                            bool reverseOrientation;
                            int adjacentEdge;
                            int order = -1;
                            int eidCurrent = -1;
                            for (int i = 0; i < edgeCount; i++)
                            {
                                netTopology.GetAdjacentEdge(fromEIDJunction, i, out adjacentEdge, out reverseOrientation);
                                ////enter in junction
                                if (reverseOrientation)
                                {
                                    int userClassID, userID, userSubID;
                                    networkElements.QueryIDs(adjacentEdge, esriElementType.esriETEdge, out userClassID, out userID, out userSubID);
                                    IFeature f = Helper.GetFeatureClassFromID(featureDataset, userClassID).GetFeature(userID);
                                    int idxOrder = f.Fields.FindField(fieldLevel);

                                    ////strahler order of stream
                                    int orderTmp = int.Parse(f.get_Value(idxOrder).ToString());

                                    if (sameStream)
                                    {
                                        if (levelCurrent == orderTmp)
                                        {
                                            eidCurrent = adjacentEdge;
                                            break;
                                        }
                                    }
                                    else
                                    {
                                        if (orderTmp > order)
                                        {
                                            order = orderTmp;
                                            eidCurrent = adjacentEdge;
                                        }
                                    }
                                }
                                else
                                {
                                    ++outEdges;
                                }
                            }

                            if (outEdges == edgeCount)
                            {
                                break;
                            }

                            if (eidCurrent == -1)
                            {
                                if (!sameStream)
                                {
                                    error = true;
                                }

                                break;
                            }

                            eids.Add(eidCurrent);
                            eid = eidCurrent;

                            ++countLoop;

                            if (countLoop > numEdges)
                            {
                                throw new GeometricNetworkException("Error in find of upstream. Check geometric network");
                            }
                        }

                        if (error || ((eids as IEnumNetEID).Count == 0))
                        {
                            throw new GeometricNetworkException("Error in find of upstream. Check level in geometric network");
                        }
                        else
                        {
                            geometry = Helper.GetPolylinePosAlong(geometricNetwork, eids as IEnumNetEID, length, point, offsetValue, ref messageInfo);
                        }
                    ////}
                }

                JsonObject objectJson = new JsonObject();
                objectJson.AddJsonObject("geometry", ESRI.ArcGIS.SOESupport.Conversion.ToJsonObject(geometry));
                if (messageInfo != null)
                {
                    objectJson.AddString("message", messageInfo);
                }

                return objectJson.JsonByte();
            }
            catch (Exception ex)
            {
                return (new ObjectError(ex.Message)).ToJsonObject().JsonByte();
            }
        }

        /// <summary>
        /// operation Trace Geometry Network
        /// </summary>
        /// <param name="boundVariables">bound Variables</param>
        /// <param name="operationInput">operation Input</param>
        /// <param name="outputFormat">output Format</param>
        /// <param name="requestProperties">request Properties</param>
        /// <param name="responseProperties">response Properties</param>
        /// <returns>Trace Geometry Network</returns>
        private byte[] TraceGeometryNetwork(NameValueCollection boundVariables, JsonObject operationInput, string outputFormat, string requestProperties, out string responseProperties)
        {
            responseProperties = null;
            string methodName = MethodBase.GetCurrentMethod().Name;

            // geometric Network id
            int id;
            try
            {
                id = Convert.ToInt32(boundVariables["GeometricNetworksID"], CultureInfo.InvariantCulture);
            }
            catch
            {
                throw new ArgumentException("geometric Network id not valid", methodName);
            }

            GeometricNetworkInfo geometricNetworkInfo = this.geometricNetworkInfos.Find(i => i.ID == id);
            if (geometricNetworkInfo == null)
            {
                throw new ArgumentException("geometric Network id not found", methodName);
            }

            ESRI.ArcGIS.Geodatabase.IGeometricNetwork geometricNetwork = geometricNetworkInfo.GeometricNetwork;

            // traceSolverType
            string traceSolverTypeString;
            bool found = operationInput.TryGetString("traceSolverType", out traceSolverTypeString);
            if (!found || string.IsNullOrEmpty(traceSolverTypeString))
            {
                throw new ArgumentException("traceSolverType not specified", methodName);
            }

            TraceSolverType traceSolverType;
            try
            {
                traceSolverType = (TraceSolverType)Enum.Parse(typeof(TraceSolverType), traceSolverTypeString);
            }
            catch
            {
                throw new ArgumentException("traceSolverType not valid", methodName);
            }

            // flowMethod
            esriFlowMethod flowMethod = esriFlowMethod.esriFMConnected;
            if ((traceSolverType != TraceSolverType.FindCircuits) && (traceSolverType != TraceSolverType.FindCommonAncestors) && (traceSolverType != TraceSolverType.FindCommonAncestors) && (traceSolverType != TraceSolverType.FindLongest))
            {
                string flowMethodString;
                found = operationInput.TryGetString("flowMethod", out flowMethodString);
                if (!found || string.IsNullOrEmpty(flowMethodString))
                {
                    throw new ArgumentException("flowMethod not specified", methodName);
                }

                try
                {
                    flowMethod = (esriFlowMethod)Enum.Parse(typeof(esriFlowMethod), flowMethodString);
                }
                catch
                {
                    throw new ArgumentException("flowMethod not valid", methodName);
                }
            }

            // flowElements
            string flowElementsString;
            found = operationInput.TryGetString("flowElements", out flowElementsString);
            if (!found || string.IsNullOrEmpty(flowElementsString))
            {
                throw new ArgumentException("flowElements not specified", methodName);
            }

            esriFlowElements flowElements;
            try
            {
                flowElements = (esriFlowElements)Enum.Parse(typeof(esriFlowElements), flowElementsString);
            }
            catch
            {
                throw new ArgumentException("flowElements not valid", methodName);
            }

            // edge flags
            object[] jsonEdgeFlags;
            List<IPoint> edgeFlags = new List<IPoint>();
            if (operationInput.TryGetArray("edgeFlags", out jsonEdgeFlags))
            {
                JsonObject[] joEdgeFlags = null;
                try
                {
                    joEdgeFlags = jsonEdgeFlags.Cast<JsonObject>().ToArray();
                }
                catch
                {
                    throw new ArgumentException("invalid edge flag", methodName);
                }

                foreach (JsonObject jo in joEdgeFlags)
                {
                    IPoint location = Conversion.ToGeometry(jo, esriGeometryType.esriGeometryPoint) as IPoint;
                    if (location == null)
                    {
                        throw new ArgumentException("invalid edgeFlags", methodName);
                    }

                    edgeFlags.Add(location);
                }
            }

            // junction Flags
            object[] jsonJunctionFlags;
            List<IPoint> junctionFlags = new List<IPoint>();
            if (operationInput.TryGetArray("junctionFlags", out jsonJunctionFlags))
            {
                JsonObject[] joJunctionFlags = null;
                try
                {
                    joJunctionFlags = jsonJunctionFlags.Cast<JsonObject>().ToArray();
                }
                catch
                {
                    throw new ArgumentException("invalid junctionFlags", methodName);
                }

                foreach (JsonObject jo in joJunctionFlags)
                {
                    IPoint location = Conversion.ToGeometry(jo, esriGeometryType.esriGeometryPoint) as IPoint;
                    if (location == null)
                    {
                        throw new ArgumentException("invalid junctionFlags", methodName);
                    }

                    junctionFlags.Add(location);
                }
            }

            if ((edgeFlags.Count == 0) && (junctionFlags.Count == 0))
            {
                throw new ArgumentException("edgeFlags and/or junctionFlags not found", methodName);
            }

            if (traceSolverType == TraceSolverType.FindPath)
            {
                if ((edgeFlags.Count != 0) && (junctionFlags.Count != 0))
                {
                    throw new ArgumentException("FindPath: the flags you place on the network must be either all edge or all junction. You cannot find a path among a mixture of edge and junction origins", methodName);
                }

                if (edgeFlags.Count == 1)
                {
                    throw new ArgumentException("FindPath: the flags edge you place on the network must be > 1.", methodName);
                }

                if (junctionFlags.Count == 1)
                {
                    throw new ArgumentException("FindPath: the flags junction you place on the network must be > 1.", methodName);
                }
            }

            if (traceSolverType == TraceSolverType.FindLongest)
            {
                if ((junctionFlags.Count != 1) || (edgeFlags.Count != 0))
                {
                    throw new ArgumentException("FindLongest: the flag junction you place on the network must be = 1. No edges.", methodName);
                }
            }

            // edge Barriers
            object[] jsonEdgeBarriers;
            List<IPoint> edgeBarriers = new List<IPoint>();
            if (operationInput.TryGetArray("edgeBarriers", out jsonEdgeBarriers))
            {
                JsonObject[] joEdgeBarriers = null;
                try
                {
                    joEdgeBarriers = jsonEdgeBarriers.Cast<JsonObject>().ToArray();
                }
                catch
                {
                    throw new ArgumentException("invalid edgeBarriers", methodName);
                }

                foreach (JsonObject jo in joEdgeBarriers)
                {
                    IPoint location = Conversion.ToGeometry(jo, esriGeometryType.esriGeometryPoint) as IPoint;
                    if (location == null)
                    {
                        throw new ArgumentException("invalid edgeBarriers", methodName);
                    }

                    edgeBarriers.Add(location);
                }
            }

            // junction Barriers
            object[] jsonJunctionBarriers;
            List<IPoint> junctionBarriers = new List<IPoint>();
            if (operationInput.TryGetArray("junctionBarriers", out jsonJunctionBarriers))
            {
                JsonObject[] joJunctionBarriers = null;
                try
                {
                    joJunctionBarriers = jsonJunctionBarriers.Cast<JsonObject>().ToArray();
                }
                catch
                {
                    throw new ArgumentException("invalid junctionBarriers", methodName);
                }

                foreach (JsonObject jo in joJunctionBarriers)
                {
                    IPoint location = Conversion.ToGeometry(jo, esriGeometryType.esriGeometryPoint) as IPoint;
                    if (location == null)
                    {
                        throw new ArgumentException("invalid junctionBarriers", methodName);
                    }

                    junctionBarriers.Add(location);
                }
            }

            // outFields
            string outFieldsString;
            found = operationInput.TryGetString("outFields", out outFieldsString);
            string[] outFields;
            if (!found || string.IsNullOrEmpty(outFieldsString))
            {
                outFields = new string[] { "*" };
            }
            else
            {
                outFields = outFieldsString.Split(',');
            }

            // maxFeatures
            long? maxFeatures;
            found = operationInput.TryGetAsLong("maxTracedFeatures", out maxFeatures);
            if (!found || !maxFeatures.HasValue)
            {
                throw new ArgumentException("invalid maxTracedFeatures", methodName);
            }

            if (maxFeatures.Value > int.MaxValue)
            {
                throw new ArgumentException("invalid maxTracedFeatures", methodName);
            }

            // tolerance
            double? tolerance;
            found = operationInput.TryGetAsDouble("tolerance", out tolerance);
            if (!found || !tolerance.HasValue)
            {
                throw new ArgumentException("invalid tolerance", methodName);
            }

            if (tolerance.Value < 0)
            {
                throw new ArgumentException("invalid tolerance", methodName);
            }

            // traceIndeterminateFlow
            bool? traceIndeterminateFlow;
            found = operationInput.TryGetAsBoolean("traceIndeterminateFlow", out traceIndeterminateFlow);
            if (!found || !traceIndeterminateFlow.HasValue)
            {
                traceIndeterminateFlow = false;
            }

            // shortestPathObjFn
            esriShortestPathObjFn shortestPathObjFn = esriShortestPathObjFn.esriSPObjFnMinMax;
            if (traceSolverType == TraceSolverType.FindSource)
            {
                string shortestPathObjFnString;
                found = operationInput.TryGetString("shortestPathObjFn", out shortestPathObjFnString);
                if (!found || string.IsNullOrEmpty(shortestPathObjFnString))
                {
                    throw new ArgumentException("shortestPathObjFn not specified", methodName);
                }

                try
                {
                    shortestPathObjFn = (esriShortestPathObjFn)Enum.Parse(typeof(esriShortestPathObjFn), shortestPathObjFnString);
                }
                catch
                {
                    throw new ArgumentException("shortestPathObjFn not valid", methodName);
                }
            }

            // disable layers     
            object[] disableLayersInput;
            List<int> disableLayers = new List<int>();
            if (operationInput.TryGetArray("disableLayers", out disableLayersInput))
            {
                try
                {
                    foreach (int i in disableLayersInput.Cast<int>().ToList())
                    {
                        disableLayers.Add(this.GetFeatureClass(i).FeatureClassID);
                    }
                }
                catch
                {
                    throw new ArgumentException("invalid disableLayers", methodName);
                }
            }

            // weight
            // Junction Weight
            INetSchema netSchema = geometricNetwork.Network as INetSchema;
            string junctionWeight;
            INetWeight netJunctionWeight = null;
            found = operationInput.TryGetString("junctionWeight", out junctionWeight);
            if (found && !string.IsNullOrEmpty(junctionWeight))
            {
                netJunctionWeight = netSchema.get_WeightByName(junctionWeight);
                if (netJunctionWeight == null)
                {
                    throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "{0} not found", junctionWeight), methodName);
                }
            }

            // from to edge Weight
            string fromToEdgeWeight;
            INetWeight netFromToEdgeWeight = null;
            found = operationInput.TryGetString("fromToEdgeWeight", out fromToEdgeWeight);
            if (found && !string.IsNullOrEmpty(fromToEdgeWeight))
            {
                netFromToEdgeWeight = netSchema.get_WeightByName(fromToEdgeWeight);
                if (netFromToEdgeWeight == null)
                {
                    throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "{0} not found", fromToEdgeWeight), methodName);
                }
            }

            // to from edge Weight
            string toFromEdgeWeight;
            INetWeight netToFromEdgeWeight = null;
            found = operationInput.TryGetString("toFromEdgeWeight", out toFromEdgeWeight);
            if (found && !string.IsNullOrEmpty(toFromEdgeWeight))
            {
                netToFromEdgeWeight = netSchema.get_WeightByName(toFromEdgeWeight);
                if (netToFromEdgeWeight == null)
                {
                    throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "{0} not found", toFromEdgeWeight), methodName);
                }
            }

            // filter junction Weight
            string junctionFilterWeight;
            INetWeight netJunctionFilterWeight = null;
            List<object> junctionFilterRangesFrom = null;
            List<object> junctionFilterRangesTo = null;
            bool junctionFilterNotOperator = false;
            found = operationInput.TryGetString("junctionFilterWeight", out junctionFilterWeight);
            if (found && !string.IsNullOrEmpty(junctionFilterWeight))
            {
                netJunctionFilterWeight = netSchema.get_WeightByName(junctionFilterWeight);
                if (netJunctionFilterWeight == null)
                {
                    throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "{0} not found", junctionFilterWeight), methodName);
                }

                junctionFilterRangesFrom = new List<object>();
                junctionFilterRangesTo = new List<object>();

                esriWeightType weightType = netJunctionFilterWeight.WeightType;
                string junctionFilterRanges;
                found = operationInput.TryGetString("junctionFilterRanges", out junctionFilterRanges);
                if (found && !string.IsNullOrEmpty(junctionFilterRanges))
                {
                    string[] arrayJunctionFilterRanges = junctionFilterRanges.Split(new string[] { this.separatorWeightRanges }, StringSplitOptions.RemoveEmptyEntries);

                    foreach (string s in arrayJunctionFilterRanges)
                    {
                        string[] range = s.Split(new char[] { '-' });
                        if (range.Length > 2)
                        {
                            throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "{0} not valid", junctionFilterRanges), methodName);
                        }

                        for (int j = 0; j < range.Length; j++)
                        {
                            string n = range[j];
                            if (weightType == esriWeightType.esriWTDouble)
                            {
                                double valueWeight;
                                if (double.TryParse(n, out valueWeight))
                                {
                                    if (range.Length == 1)
                                    {
                                        junctionFilterRangesFrom.Add(valueWeight);
                                        junctionFilterRangesTo.Add(valueWeight);
                                        break;
                                    }
                                    else
                                    {
                                        if (j == 0)
                                        {
                                            junctionFilterRangesFrom.Add(valueWeight);
                                        }
                                        else if (j == 1)
                                        {
                                            junctionFilterRangesTo.Add(valueWeight);
                                            if ((double)junctionFilterRangesFrom[junctionFilterRangesFrom.Count - 1] > (double)junctionFilterRangesTo[junctionFilterRangesTo.Count - 1])
                                            {
                                                throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "{0} not valid (from > to)", junctionFilterRanges), methodName);
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "{0} not valid", junctionFilterRanges), methodName);
                                }
                            }
                            else if ((weightType == esriWeightType.esriWTInteger) || (weightType == esriWeightType.esriWTBitGate))
                            {
                                int valueWeight;
                                if (int.TryParse(n, out valueWeight))
                                {
                                    if ((weightType == esriWeightType.esriWTBitGate) && (Convert.ToString(valueWeight, 2).Length > netJunctionFilterWeight.BitGateSize))
                                    {
                                        throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "{0} not valid (BitGateSize)", junctionFilterRanges), methodName);
                                    }

                                    if (range.Length == 1)
                                    {
                                        junctionFilterRangesFrom.Add(valueWeight);
                                        junctionFilterRangesTo.Add(valueWeight);
                                        break;
                                    }
                                    else
                                    {
                                        if (j == 0)
                                        {
                                            junctionFilterRangesFrom.Add(valueWeight);
                                        }
                                        else if (j == 1)
                                        {
                                            junctionFilterRangesTo.Add(valueWeight);
                                            if ((int)junctionFilterRangesFrom[junctionFilterRangesFrom.Count - 1] > (int)junctionFilterRangesTo[junctionFilterRangesTo.Count - 1])
                                            {
                                                throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "{0} not valid (from > to)", junctionFilterRanges), methodName);
                                            }
                                        }
                                    }
                                }
                            }
                            else if (weightType == esriWeightType.esriWTSingle)
                            {
                                float valueWeight;
                                if (float.TryParse(n, out valueWeight))
                                {
                                    if (range.Length == 1)
                                    {
                                        junctionFilterRangesFrom.Add(valueWeight);
                                        junctionFilterRangesTo.Add(valueWeight);
                                        break;
                                    }
                                    else
                                    {
                                        if (j == 0)
                                        {
                                            junctionFilterRangesFrom.Add(valueWeight);
                                        }
                                        else if (j == 1)
                                        {
                                            junctionFilterRangesTo.Add(valueWeight);
                                            if ((float)junctionFilterRangesFrom[junctionFilterRangesFrom.Count - 1] > (float)junctionFilterRangesTo[junctionFilterRangesTo.Count - 1])
                                            {
                                                throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "{0} not valid (from > to)", junctionFilterRanges), methodName);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                bool? junctionNotOperator;
                found = operationInput.TryGetAsBoolean("junctionFilterNotOperator", out junctionNotOperator);
                if (!found || !junctionNotOperator.HasValue)
                {
                    junctionFilterNotOperator = false;
                }
                else
                {
                    junctionFilterNotOperator = junctionNotOperator.Value;
                }
            }

            INetWeight netFromToEdgeFilterWeight = null;
            INetWeight netToFromEdgeFilterWeight = null;
            List<object> edgeFilterRangesFrom = null;
            List<object> edgeFilterRangesTo = null;
            bool edgeFilterNotOperator = false;

            string fromToEdgeFilterWeight;
            bool foundFT = operationInput.TryGetString("fromToEdgeFilterWeight", out fromToEdgeFilterWeight);
            string toFromEdgeFilterWeight;
            bool foundTF = operationInput.TryGetString("toFromEdgeFilterWeight", out toFromEdgeFilterWeight);

            if (foundFT ^ foundTF)
            {
                throw new ArgumentException("Set fromToEdgeFilterWeight and toFromEdgeFilterWeight!", methodName);
            }
            else
            {
                if (foundFT && foundTF && !string.IsNullOrEmpty(fromToEdgeFilterWeight) && !string.IsNullOrEmpty(toFromEdgeFilterWeight))
                {
                    netFromToEdgeFilterWeight = netSchema.get_WeightByName(fromToEdgeFilterWeight);
                    if (netFromToEdgeFilterWeight == null)
                    {
                        throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "{0} not found", fromToEdgeFilterWeight), methodName);
                    }

                    netToFromEdgeFilterWeight = netSchema.get_WeightByName(toFromEdgeFilterWeight);
                    if (netToFromEdgeFilterWeight == null)
                    {
                        throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "{0} not found", toFromEdgeFilterWeight), methodName);
                    }

                    esriWeightType weightType = netFromToEdgeFilterWeight.WeightType;
                    if (weightType != netToFromEdgeFilterWeight.WeightType)
                    {
                        throw new ArgumentException("Different weight type (EdgeFilterWeight)!", methodName);
                    }

                    edgeFilterRangesFrom = new List<object>();
                    edgeFilterRangesTo = new List<object>();

                    string edgeFilterRanges;
                    found = operationInput.TryGetString("edgeFilterRanges", out edgeFilterRanges);
                    if (found && !string.IsNullOrEmpty(edgeFilterRanges))
                    {
                        string[] arrayEdgeFilterRanges = edgeFilterRanges.Split(new string[] { this.separatorWeightRanges }, StringSplitOptions.RemoveEmptyEntries);

                        foreach (string s in arrayEdgeFilterRanges)
                        {
                            string[] range = s.Split(new char[] { '-' });
                            if (range.Length > 2)
                            {
                                throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "{0} not valid", edgeFilterRanges), methodName);
                            }

                            for (int j = 0; j < range.Length; j++)
                            {
                                string n = range[j];
                                if (weightType == esriWeightType.esriWTDouble)
                                {
                                    double valueWeight;
                                    if (double.TryParse(n, out valueWeight))
                                    {
                                        if (range.Length == 1)
                                        {
                                            edgeFilterRangesFrom.Add(valueWeight);
                                            edgeFilterRangesTo.Add(valueWeight);
                                            break;
                                        }
                                        else
                                        {
                                            if (j == 0)
                                            {
                                                edgeFilterRangesFrom.Add(valueWeight);
                                            }
                                            else if (j == 1)
                                            {
                                                edgeFilterRangesTo.Add(valueWeight);
                                                if ((double)edgeFilterRangesFrom[edgeFilterRangesFrom.Count - 1] > (double)edgeFilterRangesTo[edgeFilterRangesTo.Count - 1])
                                                {
                                                    throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "{0} not valid (from > to)", edgeFilterRanges), methodName);
                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "{0} not valid", edgeFilterRanges), methodName);
                                    }
                                }
                                else if ((weightType == esriWeightType.esriWTInteger) || (weightType == esriWeightType.esriWTBitGate))
                                {
                                    int valueWeight;
                                    if (int.TryParse(n, out valueWeight))
                                    {
                                        if (range.Length == 1)
                                        {
                                            if ((weightType == esriWeightType.esriWTBitGate) && ((Convert.ToString(valueWeight, 2).Length > netFromToEdgeFilterWeight.BitGateSize) || (Convert.ToString(valueWeight, 2).Length > netToFromEdgeFilterWeight.BitGateSize)))
                                            {
                                                throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "{0} not valid (BitGateSize)", edgeFilterRanges), methodName);
                                            }

                                            edgeFilterRangesFrom.Add(valueWeight);
                                            edgeFilterRangesTo.Add(valueWeight);
                                            break;
                                        }
                                        else
                                        {
                                            if (j == 0)
                                            {
                                                edgeFilterRangesFrom.Add(valueWeight);
                                            }
                                            else if (j == 1)
                                            {
                                                edgeFilterRangesTo.Add(valueWeight);
                                                int fromEdge = (int)edgeFilterRangesFrom[edgeFilterRangesFrom.Count - 1];
                                                int toEdge = (int)edgeFilterRangesTo[edgeFilterRangesTo.Count - 1];
                                                if ((weightType == esriWeightType.esriWTBitGate) && ((Convert.ToString(fromEdge, 2).Length > netFromToEdgeFilterWeight.BitGateSize) || (Convert.ToString(toEdge, 2).Length > netToFromEdgeFilterWeight.BitGateSize)))
                                                {
                                                    throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "{0} not valid (BitGateSize)", edgeFilterRanges), methodName);
                                                }

                                                if (fromEdge > toEdge)
                                                {
                                                    throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "{0} not valid (from > to)", edgeFilterRanges), methodName);
                                                }
                                            }
                                        }
                                    }
                                }
                                else if (weightType == esriWeightType.esriWTSingle)
                                {
                                    float valueWeight;
                                    if (float.TryParse(n, out valueWeight))
                                    {
                                        if (range.Length == 1)
                                        {
                                            edgeFilterRangesFrom.Add(valueWeight);
                                            edgeFilterRangesTo.Add(valueWeight);
                                            break;
                                        }
                                        else
                                        {
                                            if (j == 0)
                                            {
                                                edgeFilterRangesFrom.Add(valueWeight);
                                            }
                                            else if (j == 1)
                                            {
                                                edgeFilterRangesTo.Add(valueWeight);
                                                if ((float)edgeFilterRangesFrom[edgeFilterRangesFrom.Count - 1] > (float)edgeFilterRangesTo[edgeFilterRangesTo.Count - 1])
                                                {
                                                    throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "{0} not valid (from > to)", edgeFilterRanges), methodName);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }

                    bool? edgeNotOperator;
                    found = operationInput.TryGetAsBoolean("edgeFilterNotOperator", out edgeNotOperator);
                    if (!found || !edgeNotOperator.HasValue)
                    {
                        edgeFilterNotOperator = false;
                    }
                    else
                    {
                        edgeFilterNotOperator = edgeNotOperator.Value;
                    }
                }
            }

            return Helper.GetTraceGeometryNetwork(geometricNetwork, traceSolverType, flowElements, edgeFlags, junctionFlags, edgeBarriers, junctionBarriers, flowMethod, outFields, (int)maxFeatures.Value, tolerance.Value, traceIndeterminateFlow.Value, shortestPathObjFn, disableLayers, netJunctionWeight, netFromToEdgeWeight, netToFromEdgeWeight, netJunctionFilterWeight, junctionFilterRangesFrom, junctionFilterRangesTo, junctionFilterNotOperator, netFromToEdgeFilterWeight, netToFromEdgeFilterWeight, edgeFilterRangesFrom, edgeFilterRangesTo, edgeFilterNotOperator);
        }

        /// <summary>
        /// Isolate Valve
        /// </summary>
        /// <param name="geometricNetwork">Geometric Network</param>
        /// <param name="stationId">id of station</param>
        /// <param name="valveId">id of valve</param>
        /// <param name="flowElements">flow elements</param>
        /// <param name="edgeFlags">edge Flags</param>
        /// <param name="junctionFlags">junction Flags</param>
        /// <param name="edgeBarriers">edge Barriers</param>
        /// <param name="junctionBarriers">junction Barriers</param>
        /// <param name="outFields">output fields</param>
        /// <param name="maxFeatures">max features</param>
        /// <param name="tolerance">tolerance spatial</param>
        /// <returns>isolate valve</returns>
        private byte[] GetIsolateValve(ESRI.ArcGIS.Geodatabase.IGeometricNetwork geometricNetwork, int stationId, int valveId, esriFlowElements flowElements, List<IPoint> edgeFlags, List<IPoint> junctionFlags, List<IPoint> edgeBarriers, List<IPoint> junctionBarriers, string[] outFields, int maxFeatures, double tolerance)
        {
            IFeatureClass stationFeatureClass = this.GetFeatureClass(stationId);
            if (stationFeatureClass == null)
            {
                return (new ObjectError("Unable to find station layer")).ToJsonObject().JsonByte();
            }

            IFeatureClass valveFeatureClass = this.GetFeatureClass(valveId);
            if (valveFeatureClass == null)
            {
                return (new ObjectError("Unable to find valve layer")).ToJsonObject().JsonByte();
            }

            IGeometricNetworkParameters geometricNetworkParameters = new GeometricNetworkParameters();
            geometricNetworkParameters.EdgeFlags = edgeFlags;
            geometricNetworkParameters.JunctionFlags = junctionFlags;
            geometricNetworkParameters.EdgeBarriers = edgeBarriers;
            geometricNetworkParameters.JunctionBarriers = junctionBarriers;
            geometricNetworkParameters.MaxFeatures = maxFeatures;
            geometricNetworkParameters.Tolerance = tolerance;
            geometricNetworkParameters.FlowElements = flowElements;

            IGeometricNetworkValveIsolation valveIsolation = new GeometricNetworkValveIsolation(geometricNetwork, geometricNetworkParameters, valveFeatureClass, stationFeatureClass);

            valveIsolation.OutFields = outFields;

            return valveIsolation.Solve().JsonByte();
        }

        /// <summary>
        /// feature class from Layer Id
        /// </summary>
        /// <param name="layerId">Id of layer</param>
        /// <returns>feature class</returns>
        private IFeatureClass GetFeatureClass(int layerId)
        {
            IMapServer3 mapServer = this.GetMapServer();
            IMapServerDataAccess dataAccess = (IMapServerDataAccess)mapServer;
            return (IFeatureClass)dataAccess.GetDataSource(mapServer.DefaultMapName, layerId);
        }

        /// <summary>
        /// Get object MapServer of ServerObject 
        /// </summary>
        /// <returns>object MapServer</returns>
        private IMapServer3 GetMapServer()
        {
            IMapServer3 mapServer = this.serverObjectHelper.ServerObject as IMapServer3;
            if (mapServer == null)
            {
                throw new Exception("Unable to access the map server.");
            }

            return mapServer;
        }
    }
}
