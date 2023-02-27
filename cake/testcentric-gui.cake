/// <summary>
/// Class that knows how to install and run the TestCentric GUI,
/// using either the NuGet or the Chocolatey package.
/// </summary>
public class GuiRunner
{
	public const string NuGetId = "TestCentric.GuiRunner";
	public const string ChocoId = "testcentric-gui";
	
	private const string RUNNER_EXE = "testcentric.exe";

	private BuildSettings _settings;

	public GuiRunner(BuildSettings settings, string packageId)
	{
		if (packageId != null && packageId != NuGetId && packageId != ChocoId)
			throw new System.Exception($"Package Id invalid: {packageId}");

		_settings = settings;

		PackageId = packageId;
		Version = settings.GuiVersion;
	}

	public string PackageId { get; }
	public string Version { get; }
	public string InstallPath => _settings.PackageTestDirectory;
	public string ExecutablePath =>
		$"{InstallPath}{PackageId}.{Version}/tools/{RUNNER_EXE}";
	public bool IsInstalled => System.IO.File.Exists(ExecutablePath);

	public int RunUnattended(string arguments)
	{
		if (!arguments.Contains(" --run"))
			arguments += " --run";
		if (!arguments.Contains(" --unattended"))
			arguments += " --unattended";

		return Run(arguments);
	}

	public int Run(string arguments)
	{
		Console.WriteLine(ExecutablePath);
		Console.WriteLine(arguments);
		Console.WriteLine();
		return _settings.Context.StartProcess(ExecutablePath, new ProcessSettings()
		{
			Arguments = arguments,
			WorkingDirectory = _settings.OutputDirectory
		});
	}

	static readonly string[] PACKAGE_SOURCES = {
		"https://www.nuget.org/api/v2",
		"https://www.myget.org/F/testcentric/api/v2"
	};

	public void InstallRunner()
    {
		if (!System.IO.Directory.Exists(InstallPath))
			throw new System.Exception($"Directory does not exist: {InstallPath}");

		_settings.Context.NuGetInstall(PackageId,
			new NuGetInstallSettings()
			{
				Version = Version,
				Source = PACKAGE_SOURCES,
				Prerelease = true,
				OutputDirectory = InstallPath
			});
    }
}
