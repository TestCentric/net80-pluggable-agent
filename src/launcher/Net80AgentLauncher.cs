// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric Engine contributors.
// Licensed under the MIT License. See LICENSE.txt in root directory.
// ***********************************************************************

using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using NUnit.Engine;
using NUnit.Engine.Extensibility;
using TestCentric.Engine.Extensibility;
using TestCentric.Engine.Internal;

namespace TestCentric.Engine.Services
{
    [Extension]
    public class Net80AgentLauncher : IAgentLauncher
    {
        public TestAgentInfo AgentInfo => new TestAgentInfo(
            GetType().Name,
            TestAgentType.LocalProcess);

        public bool CanCreateProcess(TestPackage package)
        {
            // Get target runtime
            string runtimeSetting = package.GetSetting("TargetRuntimeFramework", "");
            return runtimeSetting.Length > 8 && runtimeSetting.StartsWith("netcore-") && runtimeSetting[8] <= '8';
        }

        public Process CreateProcess(Guid agentId, string agencyUrl, TestPackage package)
        {
            // Should not be called unless runtime is one we can handle
            if (!CanCreateProcess(package))
                return null;

            // Access other package settings
            bool runAsX86 = package.GetSetting("RunAsX86", false);
            bool debugTests = package.GetSetting("DebugTests", false);
            bool debugAgent = package.GetSetting("DebugAgent", false);
            string traceLevel = package.GetSetting("InternalTraceLevel", "Off");
            bool loadUserProfile = package.GetSetting("LoadUserProfile", false);
            string workDirectory = package.GetSetting("WorkDirectory", string.Empty);

            var sb = new StringBuilder($"--agentId={agentId} --agencyUrl={agencyUrl} --pid={Process.GetCurrentProcess().Id}");

            // Set options that need to be in effect before the package
            // is loaded by using the command line.
            if (traceLevel != "Off")
                sb.Append(" --trace=").EscapeProcessArgument(traceLevel);
            if (debugAgent)
                sb.Append(" --debug-agent");
            if (workDirectory != string.Empty)
                sb.Append($" --work=").EscapeProcessArgument(workDirectory);

            var agentName = "net80-pluggable-agent.dll";
            var agentDir = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "agent");
            var agentPath = Path.Combine(agentDir, agentName);
            var agentArgs = sb.ToString();

            var process = new Process();
            process.EnableRaisingEvents = true;

            var startInfo = process.StartInfo;
            startInfo.UseShellExecute = false;
            startInfo.CreateNoWindow = true;
            startInfo.WorkingDirectory = Environment.CurrentDirectory;
            startInfo.LoadUserProfile = loadUserProfile;

            startInfo.FileName = "dotnet";
            startInfo.Arguments = $"{agentPath} {agentArgs}";

            return process;
        }
    }
}
