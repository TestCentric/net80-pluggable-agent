//////////////////////////////////////////////////////////////////////
// CLEAN
//////////////////////////////////////////////////////////////////////

Task("Clean")
	.WithCriteria<BuildSettings>((settings) => settings.SolutionFile != null)
	.Does<BuildSettings>((settings) =>
	{
		Information("Cleaning " + settings.OutputDirectory);
		CleanDirectory(settings.OutputDirectory);
	});

//////////////////////////////////////////////////////////////////////
// CLEAN AND DELETE ALL OBJ DIRECTORIES
//////////////////////////////////////////////////////////////////////

Task("CleanAll")
	.Description("Perform standard 'Clean' followed by deleting object directories")
	.IsDependentOn("Clean")
	.IsDependentOn("DeleteObjectDirectories");

//////////////////////////////////////////////////////////////////////
// DELETE ALL OBJ DIRECTORIES
//////////////////////////////////////////////////////////////////////

Task("DeleteObjectDirectories")
	.WithCriteria<BuildSettings>((settings) => settings.SolutionFile != null)
	.Does(() =>
	{
		Information("Deleting object directories");

		foreach (var dir in GetDirectories("src/**/obj/"))
			DeleteDirectory(dir, new DeleteDirectorySettings() { Recursive = true });
	});

//////////////////////////////////////////////////////////////////////
// INITIALIZE FOR BUILD
//////////////////////////////////////////////////////////////////////

static readonly string[] PACKAGE_SOURCES =
{
   "https://www.nuget.org/api/v2",
   "https://www.myget.org/F/nunit/api/v2",
   "https://www.myget.org/F/testcentric/api/v2"
};

Task("NuGetRestore")
	.WithCriteria<BuildSettings>((settings) => settings.SolutionFile != null)
	.Does<BuildSettings>((settings) =>
	{
		NuGetRestore(settings.SolutionFile, new NuGetRestoreSettings()
		{
			Source = PACKAGE_SOURCES,
			Verbosity = NuGetVerbosity.Detailed
		});
	});

//////////////////////////////////////////////////////////////////////
// CHECK FOR MISSING AND NON-STANDARD FILE HEADERS
//////////////////////////////////////////////////////////////////////

Task("CheckHeaders")
	.WithCriteria<BuildSettings>((settings) => System.IO.Directory.Exists(settings.SourceDirectory))
	.Does<BuildSettings>((settings) =>
	{
		new HeaderCheck(settings).CheckHeaders();
	});

//////////////////////////////////////////////////////////////////////
// BUILD
//////////////////////////////////////////////////////////////////////

Task("Build")
	.WithCriteria<BuildSettings>((settings) => settings.SolutionFile != null)
	.IsDependentOn("Clean")
	.IsDependentOn("NuGetRestore")
	.IsDependentOn("CheckHeaders")
	.Does<BuildSettings>((settings) =>
	{
		if (IsRunningOnWindows())
		{
			MSBuild(settings.SolutionFile, new MSBuildSettings() { AllowPreviewVersion = true }
				.SetConfiguration(settings.Configuration)
				.SetMSBuildPlatform(MSBuildPlatform.Automatic)
				.SetVerbosity(Verbosity.Minimal)
				.SetNodeReuse(false)
				.SetPlatformTarget(PlatformTarget.MSIL)
			);
		}
		else
		{
			XBuild(settings.SolutionFile, new XBuildSettings()
				.WithTarget("Build")
				.WithProperty("Configuration", settings.Configuration)
				.SetVerbosity(Verbosity.Minimal)
			);
		}
	});
