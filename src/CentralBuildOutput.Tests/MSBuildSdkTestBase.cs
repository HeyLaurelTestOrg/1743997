namespace Treasure.Build.CentralBuildOutput.Tests;

using Microsoft.Build.Utilities.ProjectCreation;
using Xunit.Abstractions;

public abstract class MSBuildSdkTestBase : MSBuildTestBase, IDisposable
{
    protected static string ThisAssemblyDirectory => Path.GetDirectoryName(typeof(MSBuildSdkTestBase).Assembly.Location)!;

    private bool disposedValue;

    private protected TestProjectOutput ProjectOutput { get; } = TestProjectOutput.CreateInTemp();

    protected ITestOutputHelper TestOutput { get; }

    protected MSBuildSdkTestBase(ITestOutputHelper testOutput)
    {
        this.TestOutput = testOutput;
        WriteNuGetConfig();
        WriteDirectoryBuildTargets();
    }

    public void Dispose()
    {
        this.Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!this.disposedValue)
        {
            if (disposing)
            {
                this.ProjectOutput.Dispose();
            }
            this.disposedValue = true;
        }
    }

    private void WriteDirectoryBuildTargets()
        => ProjectCreator.Create().Save(Path.Combine(this.ProjectOutput, "Directory.Build.targets"));

    private void WriteNuGetConfig()
            => File.WriteAllText(
            Path.Combine(this.ProjectOutput, "NuGet.config"),
            @"<?xml version=""1.0"" encoding=""utf-8""?>
<configuration>
  <packageSources>
    <clear />
    <add key=""NuGet.org"" value=""https://api.nuget.org/v3/index.json"" />
  </packageSources>
</configuration>");
}
