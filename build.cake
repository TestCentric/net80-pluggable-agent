// Load the recipe
#load nuget:?package=TestCentric.Cake.Recipe&version=1.4.1-dev00001
// Comment out above line and uncomment below for local tests of recipe changes
//#load ../TestCentric.Cake.Recipe/recipe/*.cake

const string AGENT_NAME = "testcentric-net80-agent";

BuildSettings.Initialize
(
	context: Context,
	title: "Net80PluggableAgent",
	solutionFile: "net80-pluggable-agent.sln",
	unitTests: "**/*.tests.exe",
	githubOwner: "TestCentric",
	githubRepository: "net80-pluggable-agent" 
);

var MockAssemblyResult1 = new ExpectedResult("Failed")
{
	Total = 36, Passed = 23, Failed = 5, Warnings = 1, Inconclusive = 1, Skipped = 7,
	Assemblies = new ExpectedAssemblyResult[] { new ExpectedAssemblyResult("mock-assembly.dll") }
};

var MockAssemblyResult2 = new ExpectedResult("Failed")
{
	Total = 37, Passed = 23, Failed = 5, Warnings = 1, Inconclusive = 1, Skipped = 7,
	Assemblies = new ExpectedAssemblyResult[] { new ExpectedAssemblyResult("mock-assembly.dll") }
};

var AspNetCoreResult = new ExpectedResult("Passed")
{
	Total = 2, Passed = 2, Failed = 0, Warnings = 0, Inconclusive = 0, Skipped = 0,
	Assemblies = new ExpectedAssemblyResult[] { new ExpectedAssemblyResult("aspnetcore-test.dll") }
};

var WindowsFormsResult = new ExpectedResult("Passed")
{
	Total = 2, Passed = 2, Failed = 0, Warnings = 0, Inconclusive = 0, Skipped = 0,
	Assemblies = new ExpectedAssemblyResult[] {	new ExpectedAssemblyResult("windows-forms-test.dll") }
};

var PackageTests = new PackageTest[] {
	new PackageTest(
		1, "NetCore21PackageTest", "Run mock-assembly.dll targeting .NET Core 2.1",
		"tests/netcoreapp2.1/mock-assembly.dll", MockAssemblyResult2),
	new PackageTest(
		1, "NetCore31PackageTest", "Run mock-assembly.dll targeting .NET Core 3.1",
		"tests/netcoreapp3.1/mock-assembly.dll", MockAssemblyResult2),
	new PackageTest(
		1, "Net50PackageTest", "Run mock-assembly.dll targeting .NET 5.0",
		"tests/net5.0/mock-assembly.dll", MockAssemblyResult2),
	new PackageTest(
		1, "Net60PackageTest", "Run mock-assembly.dll targeting .NET 6.0",
		"tests/net6.0/mock-assembly.dll", MockAssemblyResult2),
	new PackageTest(
		1, "Net70PackageTest", "Run mock-assembly.dll targeting .NET 7.0",
		"tests/net7.0/mock-assembly.dll", MockAssemblyResult2),
	new PackageTest(
		1, "Net80PackageTest", "Run mock-assembly.dll targeting .NET 8.0",
		"tests/net8.0/mock-assembly.dll", MockAssemblyResult2),
	new PackageTest(
		1, $"AspNetCore8.0Test", $"Run test using AspNetCore targeting .NET 8.0",
		$"tests/net8.0/aspnetcore-test.dll", AspNetCoreResult),
	new PackageTest(
		1, "Net80WindowsFormsTest", $"Run test using windows forms under .NET 8.0",
		"tests/net8.0-windows/windows-forms-test.dll", WindowsFormsResult)
};

BuildSettings.Packages.Add(new NuGetPackage(
	"TestCentric.Extension.Net80PluggableAgent",
	title: ".NET 8.0 Pluggable Agent",
	description: "TestCentric engine extension for running tests under .NET 8.0",
	tags: new [] { "testcentric", "pluggable", "agent", "net80" },
	packageContent: new PackageContent()
		.WithRootFiles("../../LICENSE.txt", "../../README.md", "../../testcentric.png")
		.WithDirectories(
			new DirectoryContent("tools").WithFiles(
				"net80-agent-launcher.dll", "net80-agent-launcher.pdb",
				"TestCentric.Extensibility.Api.dll", "TestCentric.Engine.Api.dll" ),
			new DirectoryContent("tools/agent").WithFiles(
				$"agent/{AGENT_NAME}.dll", $"agent/{AGENT_NAME}.pdb", $"agent/{AGENT_NAME}.dll.config",
				$"agent/{AGENT_NAME}.deps.json", $"agent/{AGENT_NAME}.runtimeconfig.json",
				"agent/TestCentric.InternalTrace.dll", "agent/TestCentric.Metadata.dll", "agent/TestCentric.Agent.Core.dll",
                "agent/TestCentric.Extensibility.dll", "agent/TestCentric.Extensibility.Api.dll",
                "agent/testcentric.engine.api.dll", "agent/Microsoft.Extensions.DependencyModel.dll") ),
	testRunner: new AgentRunner($"{BuildSettings.NuGetTestDirectory}TestCentric.Extension.Net80PluggableAgent.{BuildSettings.PackageVersion}/tools/agent/{AGENT_NAME}.dll"),
	tests: PackageTests) );
	
BuildSettings.Packages.Add(new ChocolateyPackage(
	"testcentric-extension-net80-pluggable-agent",
	title: "TestCentric Extension - .NET 80 Pluggable Agent",
	description: "TestCentric engine extension for running tests under .NET 8.0",
	tags: new [] { "testcentric", "pluggable", "agent", "net80" },
	packageContent: new PackageContent()
		.WithRootFiles("../../testcentric.png")
		.WithDirectories(
			new DirectoryContent("tools").WithFiles(
				"../../LICENSE.txt", "../../README.md", "../../VERIFICATION.txt",
				"net80-agent-launcher.dll", "net80-agent-launcher.pdb",
				"TestCentric.Extensibility.Api.dll", "TestCentric.Engine.Api.dll" ),
			new DirectoryContent("tools/agent").WithFiles(
				$"agent/{AGENT_NAME}.dll", $"agent/{AGENT_NAME}.pdb", $"agent/{AGENT_NAME}.dll.config",
				$"agent/{AGENT_NAME}.deps.json", $"agent/{AGENT_NAME}.runtimeconfig.json",
				"agent/TestCentric.InternalTrace.dll", "agent/TestCentric.Metadata.dll", "agent/TestCentric.Agent.Core.dll",
                "agent/TestCentric.Extensibility.dll", "agent/TestCentric.Extensibility.Api.dll",
				"agent/TestCentric.Engine.Api.dll", "agent/Microsoft.Extensions.DependencyModel.dll") ),
	testRunner: new AgentRunner($"{BuildSettings.ChocolateyTestDirectory}testcentric-extension-net80-pluggable-agent.{BuildSettings.PackageVersion}/tools/agent/{AGENT_NAME}.dll"),
	tests: PackageTests) );

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

Build.Run();
