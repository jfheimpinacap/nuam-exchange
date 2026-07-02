namespace NuamExchange.XFactorTestRunner;

internal record RunnerOptions(
    Uri ApiBaseUrl,
    int RecordId,
    string OutputDirectory);
