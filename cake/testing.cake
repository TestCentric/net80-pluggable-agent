//////////////////////////////////////////////////////////////////////
// UNIT TESTS
//////////////////////////////////////////////////////////////////////

Task("Test")
	.IsDependentOn("Build")
	.Does<BuildSettings>((settings) =>
	{
		int rc = StartProcess(settings.OutputDirectory + settings.UnitTest);
		if (rc != 0)
			throw new System.Exception($"Unit Test Failure, rc = {rc}");
	});

