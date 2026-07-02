namespace NuamExchange.XFactorTestRunner;

internal static class ArtifactPathResolver
{
    private const string DefaultDirectoryName = "NuamExchangeTestRuns";

    public static string Resolve(string? outputDirectory, string repositoryRoot)
    {
        string requestedPath = string.IsNullOrWhiteSpace(outputDirectory)
            ? Path.Combine(GetDocumentsDirectory(), DefaultDirectoryName)
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

    private static string GetDocumentsDirectory()
    {
        string documents = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

        if (string.IsNullOrWhiteSpace(documents))
        {
            string profile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            documents = string.IsNullOrWhiteSpace(profile)
                ? Environment.CurrentDirectory
                : Path.Combine(profile, "Documents");
        }

        return documents;
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
