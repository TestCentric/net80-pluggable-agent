//////////////////////////////////////////////////////////////////////
// CREATE A DRAFT RELEASE
//////////////////////////////////////////////////////////////////////

Task("CreateDraftRelease")
	.Does<BuildSettings>((settings) =>
	{
		if (settings.BuildVersion.IsReleaseBranch)
		{
			// NOTE: Since this is a release branch, the pre-release label
			// is "pre", which we don't want to use for the draft release.
			// The branch name contains the full information to be used
			// for both the name of the draft release and the milestone,
			// i.e. release-2.0.0, release-2.0.0-beta2, etc.
			string milestone = settings.BranchName.Substring(8);
			string releaseName = $"{settings.Title} {milestone}";

			Information($"Creating draft release for {releaseName}");

			try
			{
				GitReleaseManagerCreate(settings.GitHubAccessToken, settings.GitHubOwner, settings.GitHubRepository, new GitReleaseManagerCreateSettings()
				{
					Name = releaseName,
					Milestone = milestone
				});
			}
			catch
			{
				Error($"Unable to create draft release for {releaseName}.");
				Error($"Check that there is a {milestone} milestone with at least one closed issue.");
				Error("");
				throw;
			}
		}
		else
		{
			Information("Skipping Release creation because this is not a release branch");
		}
	});

//////////////////////////////////////////////////////////////////////
// CREATE A PRODUCTION RELEASE
//////////////////////////////////////////////////////////////////////

Task("CreateProductionRelease")
	.Does<BuildSettings>((settings) =>
	{
		if (settings.IsProductionRelease)
		{
			string token = settings.GitHubAccessToken;
			string owner = settings.GitHubOwner;
			string repository = settings.GitHubRepository;
			string tagName = settings.PackageVersion;
            //string assets = IsRunningOnWindows()
            //	? $"\"{settings.NuGetPackage},{settings.ChocolateyPackage}\""
            //	: $"\"{settings.NuGetPackage}\"";

            Information($"Publishing release {tagName} to GitHub");

			//GitReleaseManagerAddAssets(token, owner, repository, tagName, assets);
			//GitReleaseManagerClose(token, owner, repository, tagName);
		}
		else
		{
			Information("Skipping CreateProductionRelease because this is not a production release");
		}
	});
