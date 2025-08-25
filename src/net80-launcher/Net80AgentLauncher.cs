// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.Versioning;
using System.Text;
using TestCentric.Engine.Extensibility;
using TestCentric.Engine.Internal;
using TestCentric.Extensibility;

namespace TestCentric.Engine.Services
{
    [Extension(Description = "Pluggable agent for running tests under .NET 8.0", EngineVersion = "2.0.0")]
    [ExtensionProperty("AgentType", "LocalProcess")]
    [ExtensionProperty("TargetFramework", ".NETCoreApp,Version=8.0")]
    public class Net80AgentLauncher : IAgentLauncher
    {
        private string AGENT_NAME = "testcentric-net80-agent.dll";
        private const string RUNTIME_IDENTIFIER = ".NETCoreApp";
        private static readonly Version RUNTIME_VERSION = new Version(8, 0, 0);
        private static readonly string TARGET_FRAMEWORK = new FrameworkName(RUNTIME_IDENTIFIER, RUNTIME_VERSION).ToString();

        public TestAgentInfo AgentInfo => new TestAgentInfo(
            GetType().Name,
            TestAgentType.LocalProcess,
            TARGET_FRAMEWORK);

        public bool CanCreateProcess(TestPackage package)
        {
            // Get target runtime
            string runtimeSetting = package.Settings.GetValueOrDefault(SettingDefinitions.TargetRuntimeFramework);
            return runtimeSetting.Length > 8 && runtimeSetting.StartsWith("netcore-") && runtimeSetting[8] <= '8';
        }

        public Process CreateProcess(TestPackage package)
        {
            return CreateProcess(Guid.Empty, null, package);
        }

        public Process CreateProcess(Guid agentId, string agencyUrl, TestPackage package)
        {
            // Should not be called unless runtime is one we can handle
            if (!CanCreateProcess(package))
                return null;

            bool runUnderAgency = !string.IsNullOrEmpty(agencyUrl);

            var sb = new StringBuilder();
            if (runUnderAgency)
                sb.Append($"--agentId={agentId} --agencyUrl={agencyUrl} --pid={Process.GetCurrentProcess().Id}");
            else
                sb.Append(package.FullName);

            // Access other package settings
            bool runAsX86 = package.Settings.GetValueOrDefault(SettingDefinitions.RunAsX86);
            bool debugTests = package.Settings.GetValueOrDefault(SettingDefinitions.DebugTests);
            bool debugAgent = package.Settings.GetValueOrDefault(SettingDefinitions.DebugAgent);
            string traceLevel = package.Settings.GetValueOrDefault(SettingDefinitions.InternalTraceLevel);
            bool loadUserProfile = package.Settings.GetValueOrDefault(SettingDefinitions.LoadUserProfile);
            string workDirectory = package.Settings.GetValueOrDefault(SettingDefinitions.WorkDirectory);

            // Set options that need to be in effect before the package
            // is loaded by using the command line.
            if (traceLevel != "Off")
                sb.Append(" --trace=").EscapeProcessArgument(traceLevel);
            if (debugAgent)
                sb.Append(" --debug-agent");
            if (workDirectory != string.Empty)
                sb.Append($" --work=").EscapeProcessArgument(workDirectory);

            var agentDir = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "agent");
            var agentPath = Path.Combine(agentDir, AGENT_NAME);
            var agentArgs = sb.ToString();

            var process = new Process();
            process.EnableRaisingEvents = true;

            var startInfo = process.StartInfo;
            startInfo.UseShellExecute = false;
            startInfo.CreateNoWindow = runUnderAgency;
            startInfo.WorkingDirectory = Environment.CurrentDirectory;
            startInfo.LoadUserProfile = loadUserProfile;

            startInfo.FileName = "dotnet";
            startInfo.Arguments = $"{agentPath} {agentArgs}";

            return process;
        }
    }
}
