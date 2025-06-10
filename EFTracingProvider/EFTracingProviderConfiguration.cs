// Copyright (c) Microsoft Corporation.  All rights reserved.

using System;
using System.Configuration;
using System.Data;
using EFProviderWrapperToolkit;

namespace EFTracingProvider
{
    /// <summary>
    /// Configuration of EFTracingProvider.
    /// </summary>
    /// <remarks>
    /// Default values for properties of this class are obtained by reading configuration file:
    /// <para>'EFTracingProvider.logToConsole' parameter supplies default value for <see cref="LogToConsole"/>.</para>
    /// <para>'EFTracingProvider.logToFile' parameter supplies default value for <see cref="LogToConsole"/>.</para>
    /// </remarks>
    public static class EFTracingProviderConfiguration
    {
        /// <summary>
        /// Initializes static members of the EFTracingProviderConfiguration class.
        /// </summary>
        static EFTracingProviderConfiguration()
        {
            DefaultWrappedProvider = ConfigurationManager.AppSettings["EFTracingProvider.wrappedProvider"];
        }

        /// <summary>
        /// Gets or sets the default wrapped provider.
        /// </summary>
        /// <value>The default wrapped provider.</value>
        public static string DefaultWrappedProvider { get; set; }

        /// <summary>
        /// Gets or sets the action to be executed before and after every command.
        /// </summary>
        /// <value>The log action.</value>
        public static Action<CommandExecutionEventArgs> LogAction { get; set; }

        /// <summary>
        /// Registers the provider factory 
        /// </summary>
        public static void RegisterProvider()
        {
            DBProviderFactoryBase.RegisterProvider("EF Tracing Data Provider", "EFTracingProvider", typeof(EFTracingProviderFactory));
        }
    }
}
