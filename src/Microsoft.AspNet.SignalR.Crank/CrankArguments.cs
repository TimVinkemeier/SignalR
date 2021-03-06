﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using CmdLine;
using Microsoft.AspNet.SignalR.Client.Http;
using Microsoft.AspNet.SignalR.Client.Transports;

namespace Microsoft.AspNet.SignalR.Crank
{
    [CommandLineArguments(Program = "Crank")]
    internal class CrankArguments
    {
        internal const int ConnectionPollIntervalMS = 1000;
        internal const int ConnectionPollAttempts = 25;

        private string controller;
        private string server;

        [CommandLineParameter(Command = "?", Name = "Help", Default = false, Description = "Show Help", IsHelp = true)]
        public bool Help { get; set; }

        [CommandLineParameter(Command = "Url", Required = true, Description = "Server URL for SignalR connections")]
        public string Url { get; set; }

        [CommandLineParameter(Command = "Transport", Required = false, Default = "auto", Description = "Transport name. Default: auto")]
        public string Transport { get; set; }

        [CommandLineParameter(Command = "BatchSize", Required = false, Default = 1, Description = "(Connect phase) Batch size for parallel connections. Default: 1 (batch disabled)")]
        public int BatchSize { get; set; }

        [CommandLineParameter(Command = "ConnectInterval", Required = false, Default = 10, Description = "(Connect phase) Time in milliseconds between connection adds. Default: 10 ms")]
        public int ConnectInterval { get; set; }

        [CommandLineParameter(Command = "Connections", Required = false, Default = 100000, Description = "(Connect phase) Number of connections to open. Default: 100000")]
        public int Connections { get; set; }

        [CommandLineParameter(Command = "ConnectTimeout", Required = false, Default = 300, Description = "(Connect phase) Timeout in milliseconds. Default: 300 ms")]
        public int ConnectTimeout { get; set; }

        [CommandLineParameter(Command = "MinServerMBytes", Required = false, Default = 500, Description = "(Connect phase) Minimum server available MBytes to reach. Default: 500 MB")]
        public int MinServerMBytes { get; set; }

        [CommandLineParameter(Command = "SendBytes", Required = false, Default = 0, Description = "(Send phase) Payload size in bytes. Default: 0 bytes (idle)")]
        public int SendBytes { get; set; }

        [CommandLineParameter(Command = "SendInterval", Required = false, Default = 500, Description = "(Send phase) Time in milliseconds between sends. Default: 500 ms")]
        public int SendInterval { get; set; }

        [CommandLineParameter(Command = "SendTimeout", Required = false, Default = 300, Description = "(Send phase) Timeout in milliseconds. Default: 300 ms")]
        public int SendTimeout { get; set; }

        [CommandLineParameter(Command = "ControllerUrl", Required = false, Description = "Url where one client will host a controller hub. Default: no controller (single client)")]
        public string ControllerUrl { get; set; }

        [CommandLineParameter(Command = "NumClients", Required = false, Default = 1, Description = "Number of load clients connecting to the controller. Default: 1 (single client)")]
        public int NumClients { get; set; }

        [CommandLineParameter(Command = "LogFile", Required = false, Default = "crank.csv", Description = "CSV output file")]
        public string LogFile { get; set; }

        [CommandLineParameter(Command = "SampleInterval", Required = false, Default = 1000, Description = "(Connect, Send and Disconnect phases) Time in milliseconds between samples. Default: 1000 ms")]
        public int SampleInterval { get; set; }

        [CommandLineParameter(Command = "SignalRInstance", Required = false, Description = "Instance name for SignalR counters on the server. Defaults to using client connection states.")]
        public string SignalRInstance { get; set; }

        [CommandLineParameter(Command = "SkipPerformanceCounters", Required = false, Default = false, Description = "Skip performance counter connection. Performance counters are not available on Azure WebApps.")]
        public bool SkipPerformanceCounters { get; set; }

        [CommandLineParameter(Command = "UseADAuthentication", Required = false, Default = false, Description = "(ADAuthentication) Enable Azure Active Directory Authentication for the SignalR endpoint.")]
        public bool UseAadAwareConnectionFactory { get; set; }

        [CommandLineParameter(Command = "ADInstance", Required = false, Default = "https://login.windows.net/{0}", Description = "(ADAuthentication) The Active Directory instance URL. The default value is suitable for Azure Active Directory.")]
        public string AadInstance { get; set; }

        [CommandLineParameter(Command = "ADTenant", Required = false, Description = "(ADAuthentication) The name of the tenant, e.g. 'example.onmicrosoft.com'.")]
        public string Tenant { get; set; }

        [CommandLineParameter(Command = "ADClientId", Required = false, Description = "(ADAuthentication) The client id of the application registration for this app.")]
        public string ClientId { get; set; }

        [CommandLineParameter(Command = "ADRedirectUri", Required = false, Description = "(ADAuthentication) The redirect URI provided during application registration.")]
        public string RedirectUri { get; set; }

        [CommandLineParameter(Command = "ADResourceId", Required = false, Description = "(ADAuthentication) The ID of the resource to be accessed, e.g. the client id of another registered application.")]
        public string ResourceId { get; set; }

        [CommandLineParameter(Command = "ADUsername", Required = false, Description = "(ADAuthentication) The username to log in with. If not given, interactive login will be used. MSA or MFA accounts require interactive login.")]
        public string Username { get; set; }

        [CommandLineParameter(Command = "ADPassword", Required = false, Description = "(ADAuthentication) The password belonging to the username. If not given, interactive login will be used. MSA or MFA accounts require interactive login.")]
        public string Password { get; set; }

        public string Controller
        {
            get
            {
                if (controller == null)
                {
                    controller = String.IsNullOrEmpty(ControllerUrl) ? "localhost" : GetHostName(ControllerUrl);
                }
                return controller;
            }
        }

        public string Server
        {
            get
            {
                if (server == null)
                {
                    server = GetHostName(Url);
                }
                return server;
            }
        }

        public bool IsController
        {
            get
            {
                return Controller.Equals("localhost", StringComparison.OrdinalIgnoreCase) ||
                    Controller.Equals(Environment.MachineName, StringComparison.OrdinalIgnoreCase);
            }
        }

        public static CrankArguments Parse()
        {
            CrankArguments args = null;
            try
            {
                args = CommandLine.Parse<CrankArguments>();
            }
            catch (CommandLineException e)
            {
                Console.WriteLine(e.ArgumentHelp.Message);
                Console.WriteLine(e.ArgumentHelp.GetHelpText(Console.BufferWidth));
                Environment.Exit(1);
            }
            return args;
        }

        private static string GetHostName(string url)
        {
            if (!String.IsNullOrEmpty(url))
            {
                return new Uri(url).Host;
            }
            return String.Empty;
        }

        public IClientTransport GetTransport()
        {
            if (!String.IsNullOrEmpty(Transport))
            {
                var httpClient = new DefaultHttpClient();
                if (Transport.Equals("WebSockets", StringComparison.InvariantCultureIgnoreCase))
                {
                    return new WebSocketTransport(httpClient);
                }
                else if (Transport.Equals("ServerSentEvents", StringComparison.InvariantCultureIgnoreCase))
                {
                    return new ServerSentEventsTransport(httpClient);
                }
                else if (Transport.Equals("LongPolling", StringComparison.InvariantCultureIgnoreCase))
                {
                    return new LongPollingTransport(httpClient);
                }
                else if (Transport.Equals("Auto", StringComparison.InvariantCultureIgnoreCase))
                {
                    return new AutoTransport(httpClient);
                }
            }
            return null;
        }
    }
}
