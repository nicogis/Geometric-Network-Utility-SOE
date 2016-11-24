//-----------------------------------------------------------------------
// <copyright file="GeometricNetworkInfo.cs" company="Studio A&T s.r.l.">
//     Copyright (c) Studio A&T s.r.l. All rights reserved.
// </copyright>
// <author>Nicogis</author>
//-----------------------------------------------------------------------
namespace Studioat.ArcGis.Soe.Rest
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using ESRI.ArcGIS.Geodatabase;
    using ESRI.ArcGIS.Geometry;
    using ESRI.ArcGIS.SOESupport;

    /// <summary>
    /// class of GeometricNetworkInfo
    /// </summary>
    internal class GeometricNetworkInfo
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GeometricNetworkInfo"/> class
        /// </summary>
        /// <param name="id">id of layer</param>
        /// <param name="name">name of layer</param>
        /// <param name="geometricNetwork">object IGeometricNetwork</param>
        public GeometricNetworkInfo(int id, string name, ESRI.ArcGIS.Geodatabase.IGeometricNetwork geometricNetwork)
        {
            this.Name = name;
            this.ID = id;
            IGeoDataset geodataset = geometricNetwork as IGeoDataset;
            this.SpatialReference = geodataset.SpatialReference;
            this.Extent = geodataset.Extent;
            this.GeometricNetwork = geometricNetwork;
        }

        /// <summary>
        /// Gets or sets extent
        /// </summary>
        public IEnvelope Extent
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets id
        /// </summary>
        public int ID
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets name
        /// </summary>
        public string Name
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets geometricNetwork
        /// </summary>
        public ESRI.ArcGIS.Geodatabase.IGeometricNetwork GeometricNetwork
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets spatialReference
        /// </summary>
        public ISpatialReference SpatialReference
        {
            get;
            set;
        }

        /// <summary>
        /// convert the current instance in JsonObject
        /// </summary>
        /// <returns>object JsonObject</returns>
        public JsonObject ToJsonObject()
        {
            byte[] bytes = Conversion.ToJson(this.Extent);
            JsonObject jo = new JsonObject(Encoding.UTF8.GetString(bytes));

            if (!jo.Exists("spatialReference"))
            {
                if (this.SpatialReference.FactoryCode == 0)
                {
                    jo.AddString("spatialReference", this.SpatialReference.Name);
                }
                else
                {
                    jo.AddLong("spatialReference", (long)this.SpatialReference.FactoryCode);
                }
            }

            JsonObject jsonObjectGeometricNetworkInfo = new JsonObject();
            jsonObjectGeometricNetworkInfo.AddString("name", this.Name);
            jsonObjectGeometricNetworkInfo.AddLong("id", (long)this.ID);
            jsonObjectGeometricNetworkInfo.AddJsonObject("extent", jo);

            INetSchema netSchema = (INetSchema)this.GeometricNetwork.Network; 
            INetWeight netWeight;
            List<JsonObject> weights = new List<JsonObject>();
            for (int i = 0; i < netSchema.WeightCount; i++) 
            { 
                netWeight = netSchema.get_Weight(i);
                JsonObject weight = new JsonObject();
                weight.AddLong("id", (long)netWeight.WeightID);
                weight.AddString("name", netWeight.WeightName);
                weight.AddString("type", Enum.GetName(typeof(esriWeightType), netWeight.WeightType));
                weight.AddLong("bitGateSize", (long)netWeight.BitGateSize);
                
                List<JsonObject> weightAssociation = new List<JsonObject>();
                IEnumNetWeightAssociation enumNetWeightAssociation = netSchema.get_WeightAssociations(netWeight.WeightID);
                INetWeightAssociation netWeightAssociation = enumNetWeightAssociation.Next();
                while (netWeightAssociation != null)
                {
                    JsonObject jsonObjectNetWeightAssociation = new JsonObject();
                    jsonObjectNetWeightAssociation.AddString("fieldName", netWeightAssociation.FieldName);
                    jsonObjectNetWeightAssociation.AddString("tableName", netWeightAssociation.TableName);
                    weightAssociation.Add(jsonObjectNetWeightAssociation);
                    netWeightAssociation = enumNetWeightAssociation.Next();
                }

                weight.AddArray("weightAssociation", weightAssociation.ToArray());
                weights.Add(weight);
            }

            jsonObjectGeometricNetworkInfo.AddArray("weights", weights.ToArray());

            return jsonObjectGeometricNetworkInfo;
        }
    }
}