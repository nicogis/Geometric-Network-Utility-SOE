# Geometric Network Utility SOE Rest


This solution (developed in c#) creates a SOE Rest in arcgis server 10.4.1 or superior for these operations:

- TraceNetwork 
- ValveIsolation
- TraceNetworkPosAlong

Installation:

a) upload file Studioat.ArcGis.Soe.Rest.GeometricNetworkUtility.soe (see http://resources.arcgis.com/en/help/arcobjects-net/conceptualhelp/0001/000100000nvz000000.htm)

b) create a service map and enable in capabilities the extension. In your mxd you must have at least a geometric Network

c) from service directory you can see all your Geometric Network
   http://hostname/instanceags/rest/services/yourservice/MapServer/exts/GeometricNetworkUtility

d) TraceNetwork, IsolateValve and TraceNetworkPosAlong operation for Geometry Network (example with id=1. To know id use request c)
http://hostname/instanceags/rest/services/yourservice/MapServer/exts/GeometricNetworkUtility/GeometricNetworks/1/TraceNetwork
http://hostname/instanceags/rest/services/yourservice/MapServer/exts/GeometricNetworkUtility/GeometricNetworks/1/IsolateValve
To see weights:
http://hostname/instanceags/rest/services/yourservice/MapServer/exts/GeometricNetworkUtility/GeometricNetworks/1


###### Help

[Live](http://sit2.sistemigis.it/sit/rest/services/Demo/GeometricNetwork/MapServer/exts/GeometricNetworkUtility/Help)

###### Video

[Live](https://www.youtube.com/watch?v=b3D0G68waL8)

###### Geometric Network

[Live demo](http://sit2.sistemigis.it/js/GeometricNetwork)


###### Valve isolation

[Live demo](http://sit2.sistemigis.it/js/valveisolation/)


###### FindLongest
 
[Live demo](http://sit2.sistemigis.it/js/GeometricNetworkStream)


###### Pos Along

The geometric network must be simple edge with flow direction in same digitized direction of edges

[Live demo](http://sit2.sistemigis.it/js/GeometricNetworkStreamPosAlong)


###### Samples

All samples are in folder data. You have projects and data for publish service and in client you have web apps js consume services):
1) you create services with enabled capabilites Geometric Network.
2) open config.js and set config.host, config.instance and config.operationalLayers = {GNLayer:'yourService'};

In ArcCatalog in capabilities (operations allowed) you can allow these operations (TraceNetwork, IsolateValve, PosAlong).

The solutions are checked 100% with stylecop and fxcop.  


# FAQ
#### Question:
I have this error when you publish the service: 'ClassFactory cannot supply requested class'
#### Answer: 
The problem is that you didn't check the ".NET extension support" when you have installed ArcGIS Server

#### Question: 
Can I use this soe in ArcGIS Server installed on Linux
#### Answer: 
No, you need porting this code in java if you need install on Linux