namespace NuamExchange.XFactorTestRunner;

internal static class ArtifactPathResolver
{
    private const string DefaultDirectoryName = "NuamExchangeTestRuns";

    public static string Resolve(string? outputDirectory, string repositoryRoot)
    {
        string requestedPath = string.IsNullOrWhiteSpace(outputDirectory)
            ? Path.Combine(GetDesktopOrSafeDirectory(repositoryRoot), DefaultDirectoryName)
            : outputDirectory;

        string fullPath = Path.GetFullPath(Environment.ExpandEnvironmentVariables(requestedPath));
        string fullRepositoryRoot = Path.GetFullPath(repositoryRoot);

        if (IsInsideRepository(fullPath, fullRepositoryRoot))
        {
            throw new ArgumentException(
                "El directorio de evidencias debe estar fuera del repositorio actual y de sus subcarpetas.");
        }

        return fullPath;
    }

    private static string GetDesktopOrSafeDirectory(string repositoryRoot)
    {
        string desktop = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);

        if (!string.IsNullOrWhiteSpace(desktop) &&
            !IsInsideRepository(Path.GetFullPath(desktop), Path.GetFullPath(repositoryRoot)))
        {
            return desktop;
        }

        string profile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        string fallback = string.IsNullOrWhiteSpace(profile)
            ? Path.Combine(Path.GetTempPath(), "NuamExchangeExternalArtifacts")
            : Path.Combine(profile, ".nuam-exchange-test-runs");

        if (IsInsideRepository(Path.GetFullPath(fallback), Path.GetFullPath(repositoryRoot)))
        {
            fallback = Path.Combine(Path.GetTempPath(), "NuamExchangeExternalArtifacts");
        }

        return fallback;
    }

    private static bool IsInsideRepository(string candidatePath, string repositoryRoot)
    {
        string normalizedCandidate = EnsureTrailingSeparator(candidatePath);
        string normalizedRepository = EnsureTrailingSeparator(repositoryRoot);

        return normalizedCandidate.StartsWith(normalizedRepository, StringComparison.OrdinalIgnoreCase);
    }

    private static string EnsureTrailingSeparator(string path) =>
        path.EndsWith(Path.DirectorySeparatorChar) || path.EndsWith(Path.AltDirectorySeparatorChar)
            ? path
            : path + Path.DirectorySeparatorChar;
}
