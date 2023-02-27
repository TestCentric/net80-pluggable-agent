// Representation of a single test to be run against a pre-built package.
// Each test has a Level, with the following values defined...
//  0 Do not run - used for temporarily disabling a test
//  1 Run for all CI tests - that is every time we test packages
//  2 Run only on PRs, dev builds and when publishing
//  3 Run only when publishing
public struct PackageTest
{
	public int Level;
	public string Description;
	public string Runner;
	public string Arguments;
	public ExpectedResult ExpectedResult;
	public string[] ExtensionsNeeded;
	
	public PackageTest(int level, string description, string runner, string arguments, ExpectedResult expectedResult, params string[] extensionsNeeded)
	{
		if (description == null)
			throw new ArgumentNullException(nameof(description));
		if (runner == null)
			throw new ArgumentNullException(nameof(runner));
		if (arguments == null)
			throw new ArgumentNullException(nameof(arguments));
		if (expectedResult == null)
			throw new ArgumentNullException(nameof(expectedResult));

		Level = level;
		Description = description;
		Runner = runner;
		Arguments = arguments;
		ExpectedResult = expectedResult;
		ExtensionsNeeded = extensionsNeeded;
	}
}

const string DEFAULT_TEST_RESULT_FILE = "TestResult.xml";

// Abstract base for all package testers.
public abstract class PackageTester
{
	protected BuildSettings _settings;
	protected PackageDefinition _package;
	private ICakeContext _context;

	public PackageTester(BuildSettings settings, PackageDefinition package)
	{
		_settings = settings;
		_package = package;
		_context = settings.Context;
    }

	protected abstract string PackageName { get; }
	protected abstract FilePath PackageUnderTest { get; }
	protected abstract string PackageTestDirectory { get; }
	protected abstract string PackageTestBinDirectory { get; }
	protected abstract string ExtensionInstallDirectory { get; }

	protected virtual string NUnitV2Driver => "NUnit.Extension.NUnitV2Driver";
	protected virtual string NUnitProjectLoader => "NUnit.Extension.NUnitProjectLoader";
	protected virtual string Net20PluggableAgent => "NUnit.Extension.Net20PluggableAgent";

	private List<string> InstalledExtensions { get; } = new List<string>();

	public void RunAllTests()
	{
		Console.WriteLine("Testing package " + PackageName);

		RunPackageTests(_settings.PackageTestLevel);

		//CheckTestErrors(ref ErrorDetail);
	}

	private void ClearAllExtensions()
    {
		// Ensure we start out each package with no extensions installed.
		// If any package test installs an extension, it remains available
		// for subsequent tests of the same package only.
		foreach (var dirPath in _context.GetDirectories(ExtensionInstallDirectory + "*"))
		{
			string dirName = dirPath.GetDirectoryName();
			if (dirName.StartsWith("NUnit.Extension.") || dirName.StartsWith("nunit-extension-"))
			{
				_context.DeleteDirectory(dirPath, new DeleteDirectorySettings() { Recursive = true });
				Console.WriteLine("Deleted directory " + dirName);
			}
		}
	}

	private void CheckExtensionIsInstalled(string extension)
	{
		bool alreadyInstalled = _context.GetDirectories($"{ExtensionInstallDirectory}{extension}.*").Count > 0;

		if (!alreadyInstalled)
		{
			DisplayBanner($"Installing {extension}");
			InstallEngineExtension(extension);
			InstalledExtensions.Add(extension);
		}
	}

	protected abstract void InstallEngineExtension(string extension);

	private void RunPackageTests(int testLevel)
	{
		var runner = new GuiRunner(_settings, GuiRunner.NuGetId);
		var reporter = new ResultReporter(PackageName);

		//ClearAllExtensions();

		foreach (var packageTest in _package.PackageTests)
		{
            if (packageTest.Level > 0 && packageTest.Level <= testLevel)
            {
                foreach (string extension in packageTest.ExtensionsNeeded)
					CheckExtensionIsInstalled(extension);

				var resultFile = _settings.OutputDirectory + DEFAULT_TEST_RESULT_FILE;
				// Delete result file ahead of time so we don't mistakenly
				// read a left-over file from another test run. Leave the
				// file after the run in case we need it to debug a failure.
				if (_context.FileExists(resultFile))
					_context.DeleteFile(resultFile);
				
				DisplayBanner(packageTest.Description);
				DisplayTestEnvironment(packageTest);

                runner.RunUnattended(packageTest.Arguments);

                try
                {
					var result = new ActualResult(resultFile);
					var report = new PackageTestReport(packageTest, result);
					reporter.AddReport(report);

					Console.WriteLine(report.Errors.Count == 0
						? "\nSUCCESS: Test Result matches expected result!"
						: "\nERROR: Test Result not as expected!");
				}
				catch (Exception ex)
                {
					reporter.AddReport(new PackageTestReport(packageTest, ex));

					Console.WriteLine("\nERROR: Failed to generate Report!");
					Console.WriteLine(ex.ToString());
				}
			}
		}

		bool anyErrors = reporter.ReportResults();
		Console.WriteLine();

		// All package tests are run even if one of them fails. If there are
		// any errors,  we stop the run at this point.
		if (anyErrors)
			throw new Exception("One or more package tests had errors!");
	}

	private void DisplayBanner(string message)
	{
		Console.WriteLine("\n========================================");;
		Console.WriteLine(message);
		Console.WriteLine("========================================");
	}

	private void DisplayTestEnvironment(PackageTest test)
	{
		Console.WriteLine("Test Environment");
		Console.WriteLine($"   OS Version: {Environment.OSVersion.VersionString}");
		Console.WriteLine($"  CLR Version: {Environment.Version}");
		Console.WriteLine($"       Runner: {test.Runner}");
		Console.WriteLine($"    Arguments: {test.Arguments}");
		Console.WriteLine();
	}

    protected FileCheck HasFile(string file) => HasFiles(new [] { file });
    protected FileCheck HasFiles(params string[] files) => new FileCheck(files);  

    protected DirectoryCheck HasDirectory(string dir) => new DirectoryCheck(dir);
}

//public class ZipPackageTester : PackageTester
//{
//	public ZipPackageTester(BuildSettings settings) : base(settings) { }

//	protected override string PackageName => _settings.ZipPackageName;
//	protected override FilePath PackageUnderTest => _settings.ZipPackage;
//	protected override string PackageTestDirectory => _settings.ZipTestDirectory;
//	protected override string PackageTestBinDirectory => PackageTestDirectory + "bin/";
//	protected override string ExtensionInstallDirectory => PackageTestBinDirectory + "addins/";
	
//	protected override void InstallEngineExtension(string extension)
//	{
//		Console.WriteLine($"Installing {extension} to directory {ExtensionInstallDirectory}");

//		_settings.Context.NuGetInstall(extension,
//			new NuGetInstallSettings()
//			{
//				OutputDirectory = ExtensionInstallDirectory,
//				Prerelease = true
//			});
//	}
//}

public class NuGetPackageTester : PackageTester
{
	public NuGetPackageTester(BuildSettings settings, PackageDefinition package) : base(settings, package) { }

	protected override string PackageName => $"{_package.PackageId}.{_settings.PackageVersion}.nupkg";
	protected override FilePath PackageUnderTest => _settings.PackageTestDirectory + PackageName;
	protected override string PackageTestDirectory => _settings.NuGetTestDirectory;
	protected override string PackageTestBinDirectory => PackageTestDirectory + "tools/";
	protected override string ExtensionInstallDirectory => _settings.PackageTestDirectory;
	
	protected override void InstallEngineExtension(string extension)
	{
		_settings.Context.NuGetInstall(extension,
			new NuGetInstallSettings()
			{
				OutputDirectory = ExtensionInstallDirectory,
				Prerelease = true
			});
	}
}

public class ChocolateyPackageTester : PackageTester
{
	public ChocolateyPackageTester(BuildSettings settings, PackageDefinition package) : base(settings, package) { }

	protected override string PackageName => $"{_package.PackageId}.{_settings.PackageVersion}.nupkg";
	protected override FilePath PackageUnderTest => _settings.PackageTestDirectory + PackageName;
	protected override string PackageTestDirectory => _settings.ChocolateyTestDirectory;
	protected override string PackageTestBinDirectory => PackageTestDirectory + "tools/";
	protected override string ExtensionInstallDirectory => _settings.PackageTestDirectory;
	
	// Chocolatey packages have a different naming convention from NuGet
	protected override string NUnitV2Driver => "nunit-extension-nunit-v2-driver";
	protected override string NUnitProjectLoader => "nunit-extension-nunit-project-loader";
    protected override string Net20PluggableAgent => "nunit-extension-net20-pluggable-agent";

    protected override void InstallEngineExtension(string extension)
	{
		// Install with NuGet because choco requires administrator access
		_settings.Context.NuGetInstall(extension,
			new NuGetInstallSettings()
			{
				Source = new[] { "https://www.myget.org/F/testcentric/api/v3/index.json" },
				OutputDirectory = ExtensionInstallDirectory,
				Prerelease = true
			});
	}
}
