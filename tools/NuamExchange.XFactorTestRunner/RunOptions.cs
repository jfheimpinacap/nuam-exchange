namespace NuamExchange.XFactorTestRunner;

internal sealed record RunOptions(
    Uri ApiBaseUrl,
    int RecordId,
    string OutputDirectory,
    string ExpectedMarket,
    string ExpectedInstrumentCode,
    int ExpectedTaxPeriod,
    bool ConfirmWrite) : RunnerOptions(ApiBaseUrl, RecordId, OutputDirectory);
