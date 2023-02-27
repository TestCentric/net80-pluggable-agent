//////////////////////////////////////////////////////////////////////
// PACKAGING TARGETS
//////////////////////////////////////////////////////////////////////

Task("Package")
	.IsDependentOn("Build")
	.IsDependentOn("BuildPackages")
	.IsDependentOn("VerifyPackages")
	.IsDependentOn("TestPackages");

//////////////////////////////////////////////////////////////////////
// BUILD PACKAGES
//////////////////////////////////////////////////////////////////////

Task("BuildPackages")
	.Does<BuildSettings>((settings) =>
	{
		foreach (var package in settings.Packages)
			if (package.IsNuGetPackage)
			{
				CreateDirectory(settings.PackageDirectory);
				NuGetPack(package.PackageSource, new NuGetPackSettings()
				{
					Version = settings.PackageVersion,
					OutputDirectory = settings.PackageDirectory,
					NoPackageAnalysis = true
				});
			}
			else if (package.IsChocolateyPackage)
			{
                CreateDirectory(settings.PackageDirectory);
                ChocolateyPack(package.PackageSource, new ChocolateyPackSettings()
                {
                    Version = settings.PackageVersion,
                    OutputDirectory = settings.PackageDirectory
                });
			}
	});

//////////////////////////////////////////////////////////////////////
// INSTALL PACKAGES
//////////////////////////////////////////////////////////////////////

Task("InstallPackages")
	.IsDependentOn("BuildPackages")
	.Does<BuildSettings>((settings) =>
	{
		foreach (var package in settings.Packages)
		{
			var packageName = $"{package.PackageId}.{settings.PackageVersion}.nupkg";
			var testDirectory = settings.PackageTestDirectory + package.PackageId;

			if (System.IO.Directory.Exists(testDirectory))
				DeleteDirectory(testDirectory,
					new DeleteDirectorySettings()
					{
						Recursive = true
					});

			CreateDirectory(testDirectory);

			Unzip(settings.PackageDirectory + packageName, testDirectory);

			Information($"  Installed {packageName}");
			Information($"    at {testDirectory}");
		}
	});

//////////////////////////////////////////////////////////////////////
// CHECK PACKAGE CONTENT
//////////////////////////////////////////////////////////////////////

Task("VerifyPackages")
	.IsDependentOn("InstallPackages")
	.Does<BuildSettings>((settings) =>
	{
		foreach (var package in settings.Packages)
		{
			var packageName = $"{package.PackageId}.{settings.PackageVersion}.nupkg";
			Information($"Verifying package {packageName}");
			var testDirectory = settings.PackageTestDirectory + package.PackageId;
			Check.That(testDirectory, package.PackageChecks);
			Information("  SUCCESS: All checks were successful");
		}
	});

//////////////////////////////////////////////////////////////////////
// TEST PACKAGES
//////////////////////////////////////////////////////////////////////

Task("TestPackages")
	.IsDependentOn("InstallPackages")
	.Does<BuildSettings>((settings) =>
	{
		foreach (var package in settings.Packages)
			if (package.IsNuGetPackage)
				new NuGetPackageTester(settings, package).RunAllTests();
			else if (package.IsChocolateyPackage)
				new ChocolateyPackageTester(settings, package).RunAllTests();
	});
