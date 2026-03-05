using BenchmarkDotNet.Attributes;
using Cslint.Core.Config;
using Cslint.Core.Engine;
using Cslint.Core.Rules;

namespace Cslint.Benchmarks;

[MemoryDiagnoser]
public class LintBenchmarks
{
    private string _tracerSettingsSource = null!;
    private string _duckTypeSource = null!;
    private RuleRegistry _registry = null!;
    private LintConfiguration _config = null!;

    [GlobalSetup]
    public void Setup()
    {
        string fixturesDir = Path.Combine(AppContext.BaseDirectory, "Fixtures");
        _tracerSettingsSource = File.ReadAllText(Path.Combine(fixturesDir, "TracerSettings.cs"));
        _duckTypeSource = File.ReadAllText(Path.Combine(fixturesDir, "DuckType.cs"));
        _registry = RuleRegistry.CreateDefault();
        _config = LintConfiguration.Empty;
    }

    [Benchmark]
    public int LintTracerSettings()
    {
        var linter = new FileLinter(_registry, new NullConfigProvider(_config));
        IReadOnlyList<LintDiagnostic> diagnostics = linter.LintSource("TracerSettings.cs", _tracerSettingsSource, _config);
        return diagnostics.Count;
    }

    [Benchmark]
    public int LintDuckType()
    {
        var linter = new FileLinter(_registry, new NullConfigProvider(_config));
        IReadOnlyList<LintDiagnostic> diagnostics = linter.LintSource("DuckType.cs", _duckTypeSource, _config);
        return diagnostics.Count;
    }

    private sealed class NullConfigProvider(LintConfiguration config) : IConfigProvider
    {
        public LintConfiguration GetConfiguration(string filePath) => config;
    }
}
