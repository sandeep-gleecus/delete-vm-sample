using System.Reflection;
using System.Runtime.CompilerServices;
using System.Web.UI;

//
// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
//
[assembly: AssemblyTitle("SpiraTeam Web Form Code-Behind Classes")]
[assembly: AssemblyDescription("Provides the presentation logic for SpiraTeam")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("Inflectra Corporation")]
[assembly: AssemblyProduct("SpiraTeam")]
[assembly: AssemblyCopyright("Copyright (C) 2006-2021 Inflectra Corporation")]
[assembly: AssemblyTrademark("Inflectra, SpiraTest, SpiraPlan and SpiraTeam are either registered trademarks or trademarks of Inflectra Corporation in the U.S. and/or other countries.")]
[assembly: AssemblyCulture("")]		

//
// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version 
//      Build Number
//      Revision
//
// You can specify all the values or you can default the Revision and Build Numbers 
// by using the '*' as shown below:

[assembly: AssemblyVersion(Inflectra.SpiraTest.Common.Global.VERSION_STRING_FULL)]
[assembly: AssemblyFileVersion(Inflectra.SpiraTest.Common.Global.VERSION_STRING_FULL)]

//
// In order to sign your assembly you must specify a key to use. Refer to the 
// Microsoft .NET Framework documentation for more information on assembly signing.
//
// Use the attributes below to control which key is used for signing. 
//
// Notes: 
//   (*) If no key is specified, the assembly is not signed.
//   (*) KeyName refers to a key that has been installed in the Crypto Service
//       Provider (CSP) on your machine. KeyFile refers to a file which contains
//       a key.
//   (*) If the KeyFile and the KeyName values are both specified, the 
//       following processing occurs:
//       (1) If the KeyName can be found in the CSP, that key is used.
//       (2) If the KeyName does not exist and the KeyFile does exist, the key 
//           in the KeyFile is installed into the CSP and used.
//   (*) In order to create a KeyFile, you can use the sn.exe (Strong Name) utility.
//        When specifying the KeyFile, the location of the KeyFile should be
//        relative to the "project output directory". The location of the project output
//        directory is dependent on whether you are working with a local or web project.
//        For local projects, the project output directory is defined as
//       <Project Directory>\obj\<Configuration>. For example, if your KeyFile is
//       located in the project directory, you would specify the AssemblyKeyFile 
//       attribute as [assembly: AssemblyKeyFile("..\\..\\mykey.snk")]
//        For web projects, the project output directory is defined as
//       %HOMEPATH%\VSWebCache\<Machine Name>\<Project Directory>\obj\<Configuration>.
//   (*) Delay Signing is an advanced option - see the Microsoft .NET Framework
//       documentation for more information on this.
//
[assembly: AssemblyDelaySign(false)]
[assembly: AssemblyKeyFile("")]
[assembly: AssemblyKeyName("")]

//Client Side Global Libraries and Resources
[assembly: WebResource("Inflectra.SpiraTest.Web.ClientScripts.GlobalFunctions.js", "text/javascript")]
[assembly: WebResource("Inflectra.SpiraTest.Web.ClientScripts.AjaxExtensions.js", "text/javascript")]
[assembly: WebResource("Inflectra.SpiraTest.Web.ClientScripts.Timer.Timer.js", "text/javascript")]
[assembly: WebResource("Inflectra.SpiraTest.Web.ClientScripts.Validation.js", "text/javascript")]
[assembly: WebResource("Inflectra.SpiraTest.Web.ClientScripts.BtnToolbarFixedPosition.js", "text/javascript")]
[assembly: WebResource("Inflectra.SpiraTest.Web.ClientScripts.Dashboard.js", "text/javascript")]

//Html2Canvas
[assembly: WebResource("Inflectra.SpiraTest.Web.ClientScripts.html2canvas.min.js", "text/javascript")]
[assembly: WebResource("Inflectra.SpiraTest.Web.ClientScripts.html2canvas.js", "text/javascript")]
[assembly: WebResource("Inflectra.SpiraTest.Web.ClientScripts.html2canvas.svg.min.js", "text/javascript")]
[assembly: WebResource("Inflectra.SpiraTest.Web.ClientScripts.html2canvas.svg.js", "text/javascript")]

//jQuery
[assembly: WebResource("Inflectra.SpiraTest.Web.ClientScripts.jQuery.js", "text/javascript")]
[assembly: WebResource("Inflectra.SpiraTest.Web.ClientScripts.jQuery-Plugins.js", "text/javascript")]

//Bootstrap
[assembly: WebResource("Inflectra.SpiraTest.Web.ClientScripts.Bootstrap.min.js", "text/javascript")]

//Hopscotch (for guided tours in app)
[assembly: WebResource("Inflectra.SpiraTest.Web.ClientScripts.hopscotch.min.js", "text/javascript")]

//Knockout
[assembly: WebResource("Inflectra.SpiraTest.Web.ClientScripts.knockout.min.js", "text/javascript")]
[assembly: WebResource("Inflectra.SpiraTest.Web.ClientScripts.knockout.mapping.js", "text/javascript")]

//React
[assembly: WebResource("Inflectra.SpiraTest.Web.ClientScripts.react.min.js", "text/javascript")]
[assembly: WebResource("Inflectra.SpiraTest.Web.ClientScripts.react-dom.min.js", "text/javascript")]

//moustreap (for assiging keypresses to js functions (no dependencies))
[assembly: WebResource("Inflectra.SpiraTest.Web.ClientScripts.mousetrap.js", "text/javascript")]

//mustache (for rendering simple data where knockout or react is too heavy (no dependencies))
[assembly: WebResource("Inflectra.SpiraTest.Web.ClientScripts.mustache.js", "text/javascript")]

//prism (for displaying source code (no dependencies))
[assembly: WebResource("Inflectra.SpiraTest.Web.ClientScripts.prism.js", "text/javascript")]

//Moments for date/time
[assembly: WebResource("Inflectra.SpiraTest.Web.ClientScripts.moment-with-locales.js", "text/javascript")]
[assembly: WebResource("Inflectra.SpiraTest.Web.ClientScripts.moment-with-locales.min.js", "text/javascript")]

//C3 and D3 libraries for charting and data visualization
[assembly: WebResource("Inflectra.SpiraTest.Web.ClientScripts.c3.min.js", "text/javascript")]
[assembly: WebResource("Inflectra.SpiraTest.Web.ClientScripts.d3.min.js", "text/javascript")]

//GraphViz libraries for mind maps and use case diagram support
[assembly: WebResource("Inflectra.SpiraTest.Web.ClientScripts.graphlib-dot.js", "text/javascript")]
[assembly: WebResource("Inflectra.SpiraTest.Web.ClientScripts.dagre-d3.js", "text/javascript")]

//XSS Santization Library
[assembly: WebResource("Inflectra.SpiraTest.Web.ClientScripts.xss.min.js", "text/javascript")]

//Gantt Chart Library
[assembly: WebResource("Inflectra.SpiraTest.Web.ClientScripts.dhtmlx-gantt.js", "text/javascript")]

//Diagram Library
[assembly: WebResource("Inflectra.SpiraTest.Web.ClientScripts.dhtmlx-diagramWithEditor.js", "text/javascript")]
[assembly: WebResource("Inflectra.SpiraTest.Web.ClientScripts.dhtmlx-diagram.js", "text/javascript")]

//QR Code Generator
[assembly: WebResource("Inflectra.SpiraTest.Web.ClientScripts.qrcode.js", "text/javascript")]

//The ASP.NET 4.0 Preview version of the ASP.NET AJAX library
[assembly: WebResource("Inflectra.SpiraTest.Web.ClientScripts.MicrosoftAjaxCore.js", "text/javascript")]
[assembly: WebResource("Inflectra.SpiraTest.Web.ClientScripts.MicrosoftAjaxGlobalization.js", "text/javascript")]
[assembly: WebResource("Inflectra.SpiraTest.Web.ClientScripts.MicrosoftAjaxSerialization.js", "text/javascript")]
[assembly: WebResource("Inflectra.SpiraTest.Web.ClientScripts.MicrosoftAjaxComponentModel.js", "text/javascript")]
[assembly: WebResource("Inflectra.SpiraTest.Web.ClientScripts.MicrosoftAjaxHistory.js", "text/javascript")]
[assembly: WebResource("Inflectra.SpiraTest.Web.ClientScripts.MicrosoftAjaxNetwork.js", "text/javascript")]
[assembly: WebResource("Inflectra.SpiraTest.Web.ClientScripts.MicrosoftAjaxWebServices.js", "text/javascript")]
[assembly: WebResource("Inflectra.SpiraTest.Web.ClientScripts.MicrosoftAjaxApplicationServices.js", "text/javascript")]
[assembly: WebResource("Inflectra.SpiraTest.Web.ClientScripts.MicrosoftAjaxWebForms.js", "text/javascript")]
[assembly: WebResource("Inflectra.SpiraTest.Web.ClientScripts.MicrosoftAjaxScriptLoader.js", "text/javascript")]
[assembly: WebResource("Inflectra.SpiraTest.Web.ClientScripts.Common.Common.js", "text/javascript")]
[assembly: WebResource("Inflectra.SpiraTest.Web.ClientScripts.Animations.js", "text/javascript")]
[assembly: WebResource("Inflectra.SpiraTest.Web.ClientScripts.BaseScripts.js", "text/javascript")]

[assembly: WebResource("Inflectra.SpiraTest.Web.ClientScripts.MicrosoftAjax.js", "text/javascript")]
[assembly: WebResource("Inflectra.SpiraTest.Web.ClientScripts.MicrosoftAjaxDataContext.js", "text/javascript")]

//Global Script Resources
[assembly: ScriptResource("Inflectra.SpiraTest.Web.ClientScripts.GlobalFunctions.js", "Inflectra.SpiraTest.Web.App_GlobalResources.ClientScript", "Inflectra.SpiraTest.Web.GlobalResources")]
[assembly: ScriptResource("Inflectra.SpiraTest.Web.ClientScripts.Common.Common.js", "Inflectra.SpiraTest.Web.App_GlobalResources.MicrosoftAjax.ScriptResources", "Sys.Extended.UI.Resources")]
[assembly: ScriptResource("Inflectra.SpiraTest.Web.ClientScripts.MicrosoftAjaxDataContext.js", "Inflectra.SpiraTest.Web.App_GlobalResources.MicrosoftAjax.DataContext.Res", "Sys.Data.DataRes")]
[assembly: ScriptResource("Inflectra.SpiraTest.Web.ClientScripts.MicrosoftAjaxCore.js", "Inflectra.SpiraTest.Web.App_GlobalResources.MicrosoftAjax.Res", "Sys.Res")]
[assembly: ScriptResource("Inflectra.SpiraTest.Web.ClientScripts.MicrosoftAjaxWebForms.js", "Inflectra.SpiraTest.Web.App_GlobalResources.MicrosoftAjax.WebForms.Res", "Sys.WebForms.Res")]

// Server Controls Resources and Tag Prefixes
[assembly: TagPrefix("Inflectra.SpiraTest.Web.ServerControls", "tstsc")]
[assembly: WebResource("Inflectra.SpiraTest.Web.ServerControls.ClientScripts.Tooltip.js", "text/javascript")]
[assembly: WebResource("Inflectra.SpiraTest.Web.ServerControls.ClientScripts.DropMenu.js", "text/javascript")]
[assembly: WebResource("Inflectra.SpiraTest.Web.ServerControls.ClientScripts.HierarchicalGrid.js", "text/javascript")]
[assembly: WebResource("Inflectra.SpiraTest.Web.ServerControls.ClientScripts.DropDownList.js", "text/javascript")]
[assembly: WebResource("Inflectra.SpiraTest.Web.ServerControls.ClientScripts.DropDownUserList.js", "text/javascript")]
[assembly: WebResource("Inflectra.SpiraTest.Web.ServerControls.ClientScripts.DropDownHierarchy.js", "text/javascript")]
[assembly: WebResource("Inflectra.SpiraTest.Web.ServerControls.ClientScripts.DatePicker.js", "text/javascript")]
[assembly: WebResource("Inflectra.SpiraTest.Web.ServerControls.ClientScripts.DateTimePicker.js", "text/javascript")]
[assembly: WebResource("Inflectra.SpiraTest.Web.ServerControls.ClientScripts.SortedGrid.js", "text/javascript")]
[assembly: WebResource("Inflectra.SpiraTest.Web.ServerControls.ClientScripts.TreeView.js", "text/javascript")]
[assembly: WebResource("Inflectra.SpiraTest.Web.ServerControls.ClientScripts.WebParts.js", "text/javascript")]
[assembly: WebResource("Inflectra.SpiraTest.Web.ServerControls.ClientScripts.DragDrop.js", "text/javascript")]
[assembly: WebResource("Inflectra.SpiraTest.Web.ServerControls.ClientScripts.GridView.js", "text/javascript")]
[assembly: WebResource("Inflectra.SpiraTest.Web.ServerControls.ClientScripts.DataList.js", "text/javascript")]
[assembly: WebResource("Inflectra.SpiraTest.Web.ServerControls.ClientScripts.DateRangeFilter.js", "text/javascript")]
[assembly: WebResource("Inflectra.SpiraTest.Web.ServerControls.ClientScripts.TabControl.js", "text/javascript")]
[assembly: WebResource("Inflectra.SpiraTest.Web.ServerControls.ClientScripts.OrderedGrid.js", "text/javascript")]
[assembly: WebResource("Inflectra.SpiraTest.Web.ServerControls.ClientScripts.DiagramEditor.js", "text/javascript")]
[assembly: WebResource("Inflectra.SpiraTest.Web.ServerControls.ClientScripts.DialogBoxPanel.js", "text/javascript")]
[assembly: WebResource("Inflectra.SpiraTest.Web.ServerControls.ClientScripts.HierarchicalSelector.js", "text/javascript")]
[assembly: WebResource("Inflectra.SpiraTest.Web.ServerControls.ClientScripts.ItemSelector.js", "text/javascript")]
[assembly: WebResource("Inflectra.SpiraTest.Web.ServerControls.ClientScripts.ContextMenu.js", "text/javascript")]
[assembly: WebResource("Inflectra.SpiraTest.Web.ServerControls.ClientScripts.ExtenderBase.BaseScripts.js", "text/javascript")]
[assembly: WebResource("Inflectra.SpiraTest.Web.ServerControls.ClientScripts.ExtenderBase.BaseScripts.debug.js", "text/javascript")]
[assembly: WebResource("Inflectra.SpiraTest.Web.ServerControls.ClientScripts.NavigationBar.js", "text/javascript")]
[assembly: WebResource("Inflectra.SpiraTest.Web.ServerControls.ClientScripts.JqPlot.js", "text/javascript")]
[assembly: WebResource("Inflectra.SpiraTest.Web.ServerControls.ClientScripts.canvg.js", "text/javascript")]
[assembly: WebResource("Inflectra.SpiraTest.Web.ServerControls.ClientScripts.rgbcolor.js", "text/javascript")]
[assembly: WebResource("Inflectra.SpiraTest.Web.ServerControls.ClientScripts.StackBlur.js", "text/javascript")]
[assembly: WebResource("Inflectra.SpiraTest.Web.ServerControls.ClientScripts.PlanningBoard.js", "text/javascript")]
[assembly: WebResource("Inflectra.SpiraTest.Web.ServerControls.ClientScripts.BackgroundProcessManager.js", "text/javascript")]
[assembly: WebResource("Inflectra.SpiraTest.Web.ServerControls.ClientScripts.SidebarPanel.js", "text/javascript")]
[assembly: WebResource("Inflectra.SpiraTest.Web.ServerControls.ClientScripts.SearchResults.js", "text/javascript")]
[assembly: WebResource("Inflectra.SpiraTest.Web.ServerControls.ClientScripts.AjaxFormManager.js", "text/javascript")]
[assembly: WebResource("Inflectra.SpiraTest.Web.ServerControls.ClientScripts.WorkflowOperations.js", "text/javascript")]
[assembly: WebResource("Inflectra.SpiraTest.Web.ServerControls.ClientScripts.LabelEx.js", "text/javascript")]
[assembly: WebResource("Inflectra.SpiraTest.Web.ServerControls.ClientScripts.Equalizer.js", "text/javascript")]
[assembly: WebResource("Inflectra.SpiraTest.Web.ServerControls.ClientScripts.CommentList.js", "text/javascript")]
[assembly: WebResource("Inflectra.SpiraTest.Web.ServerControls.ClientScripts.StatusBox.js", "text/javascript")]
[assembly: WebResource("Inflectra.SpiraTest.Web.ServerControls.ClientScripts.NumberRangeFilter.js", "text/javascript")]
[assembly: WebResource("Inflectra.SpiraTest.Web.ServerControls.ClientScripts.ArtifactHyperLink.js", "text/javascript")]
[assembly: WebResource("Inflectra.SpiraTest.Web.ServerControls.ClientScripts.UserOnlineStatus.js", "text/javascript")]
[assembly: WebResource("Inflectra.SpiraTest.Web.ServerControls.ClientScripts.Messenger.js", "text/javascript")]
[assembly: WebResource("Inflectra.SpiraTest.Web.ServerControls.ClientScripts.ScreenshotCapture.js", "text/javascript")]
[assembly: WebResource("Inflectra.SpiraTest.Web.ServerControls.ClientScripts.bootstrap-switch.min.js", "text/javascript")]
[assembly: WebResource("Inflectra.SpiraTest.Web.ServerControls.ClientScripts.bootstrap-switch.js", "text/javascript")]
[assembly: WebResource("Inflectra.SpiraTest.Web.ServerControls.ClientScripts.Notify.js", "text/javascript")]
[assembly: WebResource("Inflectra.SpiraTest.Web.ServerControls.ClientScripts.bootstrap-datetimepicker.js", "text/javascript")]
