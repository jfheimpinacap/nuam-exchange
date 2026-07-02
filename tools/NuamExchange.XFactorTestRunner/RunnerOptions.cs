namespace NuamExchange.XFactorTestRunner;

internal sealed record RunnerOptions(
    Uri ApiBaseUrl,
    int RecordId,
    string OutputDirectory);
