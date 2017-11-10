//-----------------------------------------------------------------------
// <copyright file="AssemblyInfo.cs" company="Studio A&T s.r.l.">
//     Copyright (c) Studio A&T s.r.l. All rights reserved.
// </copyright>
// <author>Nicogis</author>

using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.InteropServices;
using ESRI.ArcGIS.SOESupport;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("GeometricNetworkUtility")]
[assembly: AssemblyDescription("Geometric Network Utility")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("Studio A&T s.r.l.")]
[assembly: AssemblyProduct("GeometricNetworkUtility")]
[assembly: AssemblyCopyright("Copyright © Studio A&T s.r.l. 2017")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("be6abc9b-3a09-4e39-92d8-abe06861256b")]

// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version 
//      Build Number
//      Revision
//
// You can specify all the values or you can default the Build and Revision Numbers 
// by using the '*' as shown below:
// [assembly: AssemblyVersion("1.0.*")]
[assembly: AssemblyVersion("1.3.0.0")]
[assembly: AssemblyFileVersion("1.3.0.0")]

[assembly: AddInPackage("Studioat.ArcGis.Soe.Rest.GeometricNetworkUtility", "f15446f6-4217-4c50-a970-078c0b64a20a",
    Author = "nicogis",
    Company = "Studio A&T s.r.l.",
    Description = "Geometric Network Utility",
    TargetProduct = "Server",
    TargetVersion = "10.5",
    Version = "1.3")]

[module: SuppressMessage("Microsoft.Design", "CA1014:MarkAssembliesWithClsCompliant", Justification = "-")]
[module: SuppressMessage("Microsoft.Design", "CA1020:AvoidNamespacesWithFewTypes", Justification = "-", Scope = "namespace", Target = "Studioat.ArcGis.Soe.Rest")]
[module: SuppressMessage("Microsoft.Performance", "CA1809:AvoidExcessiveLocals", Justification = "-", Scope = "member", Target = "Studioat.ArcGis.Soe.Rest.GeometricNetworkUtility.#TraceGeometryNetwork(System.Collections.Specialized.NameValueCollection,ESRI.ArcGIS.SOESupport.JsonObject,System.String,System.String,System.String&)")]
