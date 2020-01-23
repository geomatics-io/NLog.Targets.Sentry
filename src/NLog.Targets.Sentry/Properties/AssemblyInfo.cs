﻿#region

using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#endregion

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("NLog.Targets.Sentry")]
[assembly:
    AssemblyDescription("Custom target for NLog enabling you to send logging messages to the Sentry logging service.")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("Bjarne Riis, Dillon Buchanan, Christian Junk")]
[assembly: AssemblyProduct("NLog.Targets.Sentry")]
[assembly: AssemblyCopyright("Copyright © 2016-2018 Bjarne Riis, Christian Junk")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("e14fa010-29e4-48a5-9268-a26215202a72")]

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
[assembly: AssemblyVersion("2.0.0")]
[assembly: AssemblyFileVersion("2.0.0")]
[assembly: InternalsVisibleTo("NLog.Targets.Sentry.UnitTests")]