#tool NuGet.CommandLine&version=6.0.0

// Load the recipe
#load nuget:?package=TestCentric.Cake.Recipe&version=1.0.1-dev00015
// Comment out above line and uncomment below for local tests of recipe changes
//#load ../TestCentric.Cake.Recipe/recipe/*.cake

var target = Argument("target", Argument("t", "Default"));

BuildSettings.Initialize
(
	context: Context,
	title: "Net80PluggableAgent",
	solutionFile: "net80-pluggable-agent.sln",
	msbuildAllowPreviewVersion: true,
	unitTests: "net80-agent-launcher.tests.exe",
	githubOwner: "TestCentric",
	githubRepository: "net80-pluggable-agent"
);

// Define Package Tests
//   Level 1 tests are run each time we build the packages
//   Level 2 tests are run for PRs and when packages will be published
//   Level 3 tests are run only when publishing a release

// Ensure that this agent is not used except for .NET 8.0 tests. Since
// this is the only extension installed, all tests built for .NET 6.0
// or lower will run using the .NET 6.0 agent.

var NetCore11PackageTest = new PackageTest(
	1, "NetCore11PackageTest", "Run mock-assembly.dll targeting .NET Core 1.1",
	"tests/netcoreapp1.1/mock-assembly.dll", MockAssemblyResult("Net60AgentLauncher"));

var NetCore21PackageTest = new PackageTest(
	1, "NetCore21PackageTest", "Run mock-assembly.dll targeting .NET Core 2.1",
	"tests/netcoreapp2.1/mock-assembly.dll", MockAssemblyResult("Net60AgentLauncher"));

var NetCore31PackageTest = new PackageTest(
	1, "NetCore31PackageTest", "Run mock-assembly.dll targeting .NET Core 3.1",
	"tests/netcoreapp3.1/mock-assembly.dll", MockAssemblyResult("Net60AgentLauncher"));

var Net50PackageTest = new PackageTest(
	1, "Net50PackageTest", "Run mock-assembly.dll targeting .NET 5.0",
	"tests/net5.0/mock-assembly.dll", MockAssemblyResult("Net60AgentLauncher"));

var Net60PackageTest = new PackageTest(
	1, "Net60PackageTest", "Run mock-assembly.dll targeting .NET 6.0",
	"tests/net6.0/mock-assembly.dll", MockAssemblyResult("Net60AgentLauncher"));

var Net70PackageTest = new PackageTest(
	1, "Net70PackageTest", "Run mock-assembly.dll targeting .NET 7.0",
	"tests/net7.0/mock-assembly.dll", MockAssemblyResult("Net70AgentLauncher"));

// Tests actually using this agent

var Net80PackageTest = new PackageTest(
	1, "Net80PackageTest", "Run mock-assembly.dll targeting .NET 8.0",
	"tests/net8.0/mock-assembly.dll", MockAssemblyResult("Net80AgentLauncher"));

var AspNetCore80Test = new PackageTest(
	1, "AspNetCore80Test", "Run test using AspNetCore targeting .NET 8.0",
	"tests/net8.0/aspnetcore-test.dll", AspNetCoreResult("Net80AgentLauncher"));

var Net80WindowsFormsTest = new PackageTest(
	1, "Net80WindowsFormsTest", "Run test using windows forms under .NET 8.0",
	"tests/net8.0-windows/windows-forms-test.dll", WindowsFormsResult("Net80AgentLauncher"));

var packageTests = new PackageTest[] {
	NetCore11PackageTest, NetCore21PackageTest, NetCore31PackageTest,
	Net50PackageTest, Net60PackageTest, Net70PackageTest,
	Net80PackageTest, AspNetCore80Test, Net80WindowsFormsTest };

var NuGetAgentPackage = new NuGetPackage(
	id: "NUnit.Extension.Net80PluggableAgent",
	title: "Net80 Pluggable Agent",
	description: "TestCentric engine extension for running tests under .NET 8.0",
	tags: new [] { "testcentric", "pluggable", "agent", "net8.0" },
	packageContent: new PackageContent(
		new FilePath[] { "../../LICENSE.txt", "../../README.md", "../../testcentric.png" },
		new DirectoryContent("tools").WithFiles(
			"net80-agent-launcher.dll", "net80-agent-launcher.pdb",
			"nunit.engine.api.dll", "testcentric.engine.api.dll"),
		new DirectoryContent("tools/agent").WithFiles(
			"agent/net80-pluggable-agent.dll", "agent/net80-pluggable-agent.pdb", "agent/net80-pluggable-agent.dll.config",
			"agent/net80-pluggable-agent.deps.json", "agent/net80-pluggable-agent.runtimeconfig.json",
			"agent/nunit.engine.api.dll", "agent/testcentric.engine.core.dll", "agent/testcentric.engine.metadata.dll", "agent/testcentric.extensibility.dll",
			"agent/Microsoft.Extensions.DependencyModel.dll")),
	testRunner: new GuiRunner("TestCentric.GuiRunner", "2.0.0-beta1"),
	checks: new PackageCheck[] {
		HasFiles("LICENSE.txt"),
		HasDirectory("tools").WithFiles("net80-agent-launcher.dll", "nunit.engine.api.dll"),
		HasDirectory("tools/agent").WithFiles(
			"net80-pluggable-agent.dll", "net80-pluggable-agent.dll.config",
			"nunit.engine.api.dll", "testcentric.engine.core.dll",
			"testcentric.engine.metadata.dll", "testcentric.extensibility.dll") },
	tests: packageTests);

var ChocolateyAgentPackage = new ChocolateyPackage(
	id: "nunit-extension-net80-pluggable-agent",
	title: "Net80 Pluggable Agent",
	description: "TestCentric engine extension for running tests under .NET 8.0",
	tags: new [] { "testcentric", "pluggable", "agent", "net8.0" },
	packageContent: new PackageContent(
		new FilePath[] { "../../testcentric.png" },
		new DirectoryContent("tools").WithFiles(
			"../../LICENSE.txt", "../../README.md", "../../VERIFICATION.txt",
			"net80-agent-launcher.dll", "net80-agent-launcher.pdb",
			"nunit.engine.api.dll", "testcentric.engine.api.dll"),
		new DirectoryContent("tools/agent").WithFiles(
			"agent/net80-pluggable-agent.dll", "agent/net80-pluggable-agent.pdb", "agent/net80-pluggable-agent.dll.config",
			"agent/net80-pluggable-agent.deps.json", "agent/net80-pluggable-agent.runtimeconfig.json",
			"agent/nunit.engine.api.dll", "agent/testcentric.engine.core.dll", "agent/testcentric.engine.metadata.dll", "agent/testcentric.extensibility.dll",
			"agent/Microsoft.Extensions.DependencyModel.dll")),
	testRunner: new GuiRunner("testcentric-gui", "2.0.0-beta1"),
	checks: new PackageCheck[] {
		HasDirectory("tools").WithFiles("net80-agent-launcher.dll", "nunit.engine.api.dll")
			.WithFiles("LICENSE.txt", "VERIFICATION.txt"),
		HasDirectory("tools/agent").WithFiles(
			"net80-pluggable-agent.dll", "net80-pluggable-agent.dll.config",
			"nunit.engine.api.dll", "testcentric.engine.core.dll",
			"testcentric.engine.metadata.dll", "testcentric.extensibility.dll") },
	tests: packageTests);

BuildSettings.Packages.AddRange(new PackageDefinition[] { NuGetAgentPackage, ChocolateyAgentPackage });

ExpectedResult MockAssemblyResult(string expectedAgent) => new ExpectedResult("Failed")
{
	Total = 36,
	Passed = 23,
	Failed = 5,
	Warnings = 1,
	Inconclusive = 1,
	Skipped = 7,
	Assemblies = new ExpectedAssemblyResult[]
	{
		new ExpectedAssemblyResult("mock-assembly.dll", expectedAgent)
	}
};

ExpectedResult AspNetCoreResult(string expectedAgent) => new ExpectedResult("Passed")
{
	Assemblies = new ExpectedAssemblyResult[]
	{
		new ExpectedAssemblyResult("aspnetcore-test.dll", expectedAgent)
	}
};

ExpectedResult WindowsFormsResult(string expectedAgent) => new ExpectedResult("Passed")
{
    Assemblies = new ExpectedAssemblyResult[]
	{
		new ExpectedAssemblyResult("windows-forms-test.dll", expectedAgent)
	}
};

//////////////////////////////////////////////////////////////////////
// TASK TARGETS
//////////////////////////////////////////////////////////////////////

Task("Appveyor")
	.IsDependentOn("BuildTestAndPackage")
	.IsDependentOn("Publish");

Task("Default")
    .IsDependentOn("Build");

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

RunTarget(target);
