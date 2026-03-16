using BenchmarkDotNet.Attributes;
using Cslint.Core.Config;
using Cslint.Core.Engine;
using Cslint.Core.Rules;

namespace Cslint.Benchmarks;

[MemoryDiagnoser]
public class LintBenchmarks
{
    private string _tracerSettingsSource = null!;
    private string _tracerSettingsPath = null!;
    private string _duckTypeSource = null!;
    private string _duckTypePath = null!;
    private string _repoRoot = null!;
    private FileLinter _linter = null!;
    private FileLinter _semanticLinter = null!;
    private DirectoryLinter _directoryLinter = null!;
    private DirectoryLinter _semanticDirectoryLinter = null!;
    private LintConfiguration _fixtureConfig = null!;

    [GlobalSetup]
    public void Setup()
    {
        // Walk up from bin output to find the repo root (contains .editorconfig)
        string dir = AppContext.BaseDirectory;

        while (dir is not null && !File.Exists(Path.Combine(dir, ".editorconfig")))
        {
            dir = Path.GetDirectoryName(dir)!;
        }

        _repoRoot = dir ?? throw new InvalidOperationException("Could not find repo root with .editorconfig");

        // Use fixture files at their real repo paths so EditorConfigProvider resolves .editorconfig
        _tracerSettingsPath = Path.Combine(_repoRoot, "benchmarks", "CsLint.Benchmarks", "Fixtures", "TracerSettings.cs");
        _duckTypePath = Path.Combine(_repoRoot, "benchmarks", "CsLint.Benchmarks", "Fixtures", "DuckType.cs");
        _tracerSettingsSource = File.ReadAllText(_tracerSettingsPath);
        _duckTypeSource = File.ReadAllText(_duckTypePath);

        var configProvider = new EditorConfigProvider();
        RuleRegistry registry = RuleRegistry.CreateDefault();
        _fixtureConfig = configProvider.GetConfiguration(_tracerSettingsPath);
        _linter = new FileLinter(registry, configProvider);
        _semanticLinter = new FileLinter(registry, configProvider) { EnableSemantic = true };
        _directoryLinter = new DirectoryLinter(_linter);
        _semanticDirectoryLinter = new DirectoryLinter(_semanticLinter);
    }

    [Benchmark]
    public int LintTracerSettings()
    {
        IReadOnlyList<LintDiagnostic> diagnostics = _linter.LintSource(_tracerSettingsPath, _tracerSettingsSource, _fixtureConfig);
        return diagnostics.Count;
    }

    [Benchmark]
    public int LintDuckType()
    {
        IReadOnlyList<LintDiagnostic> diagnostics = _linter.LintSource(_duckTypePath, _duckTypeSource, _fixtureConfig);
        return diagnostics.Count;
    }

    [Benchmark]
    public int LintTracerSettings_Semantic()
    {
        IReadOnlyList<LintDiagnostic> diagnostics = _semanticLinter.LintSource(_tracerSettingsPath, _tracerSettingsSource, _fixtureConfig);
        return diagnostics.Count;
    }

    [Benchmark]
    public int LintDuckType_Semantic()
    {
        IReadOnlyList<LintDiagnostic> diagnostics = _semanticLinter.LintSource(_duckTypePath, _duckTypeSource, _fixtureConfig);
        return diagnostics.Count;
    }

    [Benchmark]
    public int LintSelf()
    {
        IReadOnlyList<LintDiagnostic> diagnostics = _directoryLinter.LintDirectoryAsync(_repoRoot).GetAwaiter().GetResult();
        return diagnostics.Count;
    }

    [Benchmark]
    public int LintSelf_Semantic()
    {
        IReadOnlyList<LintDiagnostic> diagnostics = _semanticDirectoryLinter.LintDirectoryAsync(_repoRoot).GetAwaiter().GetResult();
        return diagnostics.Count;
    }
}
