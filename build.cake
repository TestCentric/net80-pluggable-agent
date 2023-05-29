#tool NuGet.CommandLine&version=6.0.0

// Load the recipe
#load nuget:?package=TestCentric.Cake.Recipe&version=1.0.1-dev00025
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

BuildSettings.Packages.AddRange(new PluggableAgentFactory(".NetCoreApp, Version=8.0").Packages);

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
