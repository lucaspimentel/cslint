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
    private FileLinter _linter = null!;
    private LintConfiguration _config = null!;

    [GlobalSetup]
    public void Setup()
    {
        string fixturesDir = Path.Combine(AppContext.BaseDirectory, "Fixtures");
        _tracerSettingsSource = File.ReadAllText(Path.Combine(fixturesDir, "TracerSettings.cs"));
        _duckTypeSource = File.ReadAllText(Path.Combine(fixturesDir, "DuckType.cs"));
        RuleRegistry registry = RuleRegistry.CreateDefault();
        _config = LintConfiguration.Empty;
        _linter = new FileLinter(registry, new NullConfigProvider(_config));
    }

    [Benchmark]
    public int LintTracerSettings()
    {
        IReadOnlyList<LintDiagnostic> diagnostics = _linter.LintSource("TracerSettings.cs", _tracerSettingsSource, _config);
        return diagnostics.Count;
    }

    [Benchmark]
    public int LintDuckType()
    {
        IReadOnlyList<LintDiagnostic> diagnostics = _linter.LintSource("DuckType.cs", _duckTypeSource, _config);
        return diagnostics.Count;
    }

    private sealed class NullConfigProvider(LintConfiguration config) : IConfigProvider
    {
        public LintConfiguration GetConfiguration(string filePath) => config;
    }
}
