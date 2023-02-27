#tool nuget:?package=GitVersion.CommandLine&version=5.6.3
#tool nuget:?package=GitReleaseManager&version=0.12.1
#tool nuget:?package=TestCentric.GuiRunner&version=2.0.0-alpha7

//#load nuget:?package=TestCentric.Cake.Recipe&version=1.0.0-dev00030
#load cake/build-settings.cake

var target = Argument("target", Argument("t", "Default"));

//////////////////////////////////////////////////////////////////////
// SETUP
//////////////////////////////////////////////////////////////////////

Setup<BuildSettings>((context) =>
{
	var settings = BuildSettings.Initialize
	(
		context: context,
		title: "Net80PluggableAgent",
		solutionFile: "net80-pluggable-agent.sln",
		unitTest: "net80-agent-launcher.tests.exe",
		guiVersion: "2.0.0-alpha7",
		githubOwner: "TestCentric",
		githubRepository: "net80-pluggable-agent",
		copyright: "Copyright (c) Charlie Poole and TestCentric Engine contributors.",
		packages: new PackageDefinition[] { NuGetAgentPackage, ChocolateyAgentPackage },
		packageTests: new PackageTest[] {
			NetCore11PackageTest, NetCore21PackageTest, NetCore31PackageTest,
			Net50PackageTest, Net60PackageTest, Net70PackageTest, Net80PackageTest }
	);

	Information($"Net80PluggableAgent {settings.Configuration} version {settings.PackageVersion}");

	if (BuildSystem.IsRunningOnAppVeyor)
		AppVeyor.UpdateBuildVersion(settings.PackageVersion + "-" + AppVeyor.Environment.Build.Number);

	return settings;
});

var NuGetAgentPackage = new NuGetPackage(
		id: "NUnit.Extension.Net80PluggableAgent",
		source: "nuget/Net80PluggableAgent.nuspec",
		checks: new PackageCheck[] {
			HasFiles("LICENSE.txt"),
			HasDirectory("tools").WithFiles("net80-agent-launcher.dll", "nunit.engine.api.dll"),
			HasDirectory("tools/agent").WithFiles(
				"net80-pluggable-agent.dll", "net80-pluggable-agent.dll.config",
				"nunit.engine.api.dll", "testcentric.engine.core.dll",
				"testcentric.engine.metadata.dll", "testcentric.extensibility.dll")
		});

var ChocolateyAgentPackage = new ChocolateyPackage(
		id: "nunit-extension-net80-pluggable-agent",
		source: "choco/net80-pluggable-agent.nuspec",
		checks: new PackageCheck[] {
			HasDirectory("tools").WithFiles("net80-agent-launcher.dll", "nunit.engine.api.dll")
				.WithFiles("LICENSE.txt", "VERIFICATION.txt"),
			HasDirectory("tools/agent").WithFiles(
				"net80-pluggable-agent.dll", "net80-pluggable-agent.dll.config",
				"nunit.engine.api.dll", "testcentric.engine.core.dll",
				"testcentric.engine.metadata.dll", "testcentric.extensibility.dll")
		});

var NetCore11PackageTest = new PackageTest(
	1, "NetCore11PackageTest", "Run mock-assembly.dll targeting .NET Core 1.1", GUI_RUNNER,
	"tests/netcoreapp1.1/mock-assembly.dll", MockAssemblyResult);

var NetCore21PackageTest = new PackageTest(
	1, "NetCore21PackageTest", "Run mock-assembly.dll targeting .NET Core 2.1", GUI_RUNNER,
	"tests/netcoreapp2.1/mock-assembly.dll", MockAssemblyResult);

var NetCore31PackageTest = new PackageTest(
	1, "NetCore31PackageTest", "Run mock-assembly.dll targeting .NET Core 3.1", GUI_RUNNER,
	"tests/netcoreapp3.1/mock-assembly.dll", MockAssemblyResult);

var Net50PackageTest = new PackageTest(
	1, "Net50PackageTest", "Run mock-assembly.dll targeting .NET 5.0", GUI_RUNNER,
	"tests/net5.0/mock-assembly.dll", MockAssemblyResult);

var Net60PackageTest = new PackageTest(
	1, "Net60PackageTest", "Run mock-assembly.dll targeting .NET 6.0", GUI_RUNNER,
	"tests/net6.0/mock-assembly.dll", MockAssemblyResult);

var Net70PackageTest = new PackageTest(
	1, "Net70PackageTest", "Run mock-assembly.dll targeting .NET 7.0", GUI_RUNNER,
	"tests/net7.0/mock-assembly.dll", MockAssemblyResult);

var Net80PackageTest = new PackageTest(
	1, "Net80PackageTest", "Run mock-assembly.dll targeting .NET 8.0", GUI_RUNNER,
	"tests/net8.0/mock-assembly.dll", MockAssemblyResult);

static readonly string GUI_RUNNER = "tools/TestCentric.GuiRunner.2.0.0-alpha7/tools/testcentric.exe";

ExpectedResult MockAssemblyResult => new ExpectedResult("Failed")
{
	Total = 36,
	Passed = 23,
	Failed = 5,
	Warnings = 1,
	Inconclusive = 1,
	Skipped = 7,
	Assemblies = new ExpectedAssemblyResult[]
	{
		new ExpectedAssemblyResult("mock-assembly.dll", "Net80AgentLauncher")
	}
};

//////////////////////////////////////////////////////////////////////
// TASK TARGETS
//////////////////////////////////////////////////////////////////////

Task("Appveyor")
	.IsDependentOn("BuildTestAndPackage")
	.IsDependentOn("Publish");

Task("BuildTestAndPackage")
	.IsDependentOn("Build")
	.IsDependentOn("Test")
	.IsDependentOn("Package");

//Task("Travis")
//	.IsDependentOn("Build")
//	.IsDependentOn("Test");

Task("Default")
    .IsDependentOn("Build");

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

RunTarget(target);
