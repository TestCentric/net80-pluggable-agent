public abstract class PackageDefinition
{
	protected enum PackageType
	{
		NuGet,
		Chocolatey
	}

	private PackageType _packageType;

	protected PackageDefinition(PackageType packageType, string id, string source, PackageCheck[] checks)
	{
		_packageType = packageType;

		PackageId = id;
		PackageSource = source;
		PackageChecks = checks;
	}

	public string PackageId { get; }
	public string PackageSource { get; }
	public PackageCheck[] PackageChecks { get; }

	public bool IsNuGetPackage => _packageType == PackageType.NuGet;
	public bool IsChocolateyPackage => _packageType == PackageType.Chocolatey;

	public IList<PackageTest> PackageTests { get; set; } = new PackageTest[0];
	public PackageDefinition WithTests(params PackageTest[] tests)
    {
		PackageTests = tests;
		return this;
    }
}

// Users may only instantiate the derived classes, which avoids
// exposing PackageType and makes it impossible to create a
// PackageDefinitin with an unknown package type.
public class NuGetPackage : PackageDefinition
{
    public NuGetPackage(string id, string source, params PackageCheck[] checks)
        : base(PackageType.NuGet, id, source, checks) { }
}

public class ChocolateyPackage : PackageDefinition
{
    public ChocolateyPackage(string id, string source, params PackageCheck[] checks)
        : base(PackageType.Chocolatey, id, source, checks) { }
}
