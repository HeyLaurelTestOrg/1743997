namespace Treasure.Build.CentralBuildOutput.Tests;

using Microsoft.Build.Utilities.ProjectCreation;
using Shouldly;
using Treasure.Build.CentralBuildOutput.Tests.MSBuild;
using Xunit;
using Xunit.Abstractions;

public class CentralBuildOutputTests : MSBuildSdkTestBase
{
    public CentralBuildOutputTests(ITestOutputHelper testOutput)
        : base(testOutput)
    {
    }

    /// <summary>
    /// Validates a project in a project folder:
    ///     Directory.Build.props
    ///     Directory.Build.targets
    ///     nuget.config
    ///     src/MyClassLibrary/MyClassLibrary.csproj
    /// </summary>
    [Fact]
    public void DefaultConfiguration()
    {
        // Arrange
        this.SetupDirectoryBuildProps();

        // Act
        ProjectCreator project = this.CreateSaveAndBuildProject(() => ProjectCreator.Templates
            .SdkCsproj(path: "src/MyClassLibrary/MyClassLibrary.csproj"));

        // Assert
        Properties properties = Properties.Load(project);

        CentralBuildOutputProperties cboProps = properties.CentralBuildOutput;
        cboProps.AppxPackageDir.MakeRelative(this.ProjectOutput).ShouldBe("__output/Debug/AnyCPU/src/MyClassLibrary/AppPackages/");
        cboProps.BaseIntDir.MakeRelative(this.ProjectOutput).ShouldBe("__intermediate/");
        cboProps.BaseNuGetDir.MakeRelative(this.ProjectOutput).ShouldBe("__packages/NuGet/");
        cboProps.BaseOutDir.MakeRelative(this.ProjectOutput).ShouldBe("__output/");
        cboProps.BasePackagesDir.MakeRelative(this.ProjectOutput).ShouldBe("__packages/");
        cboProps.BaseProjectIntermediateOutputPath.MakeRelative(this.ProjectOutput).ShouldBe("__intermediate/src/MyClassLibrary/");
        cboProps.BaseProjectPublishOutputPath.MakeRelative(this.ProjectOutput).ShouldBe("__publish/Debug/AnyCPU/src/MyClassLibrary/");
        cboProps.BaseProjectTestResultsOutputPath.MakeRelative(this.ProjectOutput).ShouldBe("__test-results/src/MyClassLibrary/");
        cboProps.BasePublishDir.MakeRelative(this.ProjectOutput).ShouldBe("__publish/");
        cboProps.BaseTestResultsDir.MakeRelative(this.ProjectOutput).ShouldBe("__test-results/");
        cboProps.CentralBuildOutputFolderPrefix.MakeRelative(this.ProjectOutput).ShouldBe("__");
        cboProps.CentralBuildOutputPath.MakeRelative(this.ProjectOutput).ShouldBeEmpty();
        cboProps.DefaultArtifactsSource.MakeRelative(this.ProjectOutput).ShouldBe("__packages/NuGet/Debug/");
        cboProps.EnableCentralBuildOutput.MakeRelative(this.ProjectOutput).ShouldBeEmpty();
        cboProps.PackageOutputPath.MakeRelative(this.ProjectOutput).ShouldBe("__packages/NuGet/Debug/");
        cboProps.ProjectIntermediateOutputPath.MakeRelative(this.ProjectOutput).ShouldBe("__intermediate/src/MyClassLibrary/");
        cboProps.ProjectOutputPath.MakeRelative(this.ProjectOutput).ShouldBe("__output/Debug/AnyCPU/src/MyClassLibrary/");
        cboProps.RelativeProjectPath.MakeRelative(this.ProjectOutput).ShouldBe("src/MyClassLibrary/");

        CoverletProperties coverletProps = properties.Coverlet;
        coverletProps.CoverletOutput.MakeRelative(this.ProjectOutput).ShouldBe("__test-results/src/MyClassLibrary/");

        CommonMSBuildProperties msbuildProps = properties.MSBuildCommon;
        msbuildProps.BaseIntermediateOutputPath.MakeRelative(this.ProjectOutput).ShouldBe("__intermediate/src/MyClassLibrary/");
        msbuildProps.BaseOutputPath.MakeRelative(this.ProjectOutput).ShouldBe("__output/Debug/AnyCPU/src/MyClassLibrary/");
        msbuildProps.OutputPath.MakeRelative(this.ProjectOutput).ShouldBe("__output/Debug/AnyCPU/src/MyClassLibrary/netstandard2.0\\");

        CommonMSBuildMacros msbuildMacros = properties.MSBuildMacros;
        msbuildMacros.PublishDir.MakeRelative(this.ProjectOutput).ShouldBe("__publish/Debug/AnyCPU/src/MyClassLibrary/");

        MSBuildOtherProperties msBuildOtherProps = properties.MSBuildOther;
        msBuildOtherProps.MSBuildProjectExtensionPath.MakeRelative(this.ProjectOutput).ShouldBe("__intermediate/src/MyClassLibrary/");

        VSTestProperties vsTestProps = properties.VSTest;
        vsTestProps.VSTestResultsDirectory.MakeRelative(this.ProjectOutput).ShouldBe("__test-results/src/MyClassLibrary/");

        File.Exists("__output/Debug/AnyCPU/src/MyClassLibrary/netstandard2.0/MyClassLibrary.dll").ShouldBeTrue();
        Directory.Exists("__intermediate/src/MyClassLibrary/Debug/netstandard2.0").ShouldBeTrue();
    }

    /// <summary>
    /// Validates a project in a project folder:
    ///     Directory.Build.props
    ///     Directory.Build.targets
    ///     nuget.config
    ///     src/MyClassLibrary/MyClassLibrary.csproj
    /// </summary>
    [Fact]
    public void DefaultConfigurationWithMultiTargetting()
    {
        // Arrange
        this.SetupDirectoryBuildProps();

        // Act
        ProjectCreator project = this.CreateSaveAndBuildProject(() => ProjectCreator.Templates
            .SdkCsproj(
                path: "src/MyClassLibrary/MyClassLibrary.csproj",
                targetFrameworks: new[] { "netstandard1.6", "netstandard2.0", "netstandard2.1" }));

        // Assert
        Properties properties = Properties.Load(project);

        CommonMSBuildProperties msbuildProps = properties.MSBuildCommon;
        msbuildProps.OutputPath.MakeRelative(this.ProjectOutput).ShouldBe("__output/Debug/AnyCPU/src/MyClassLibrary/");

        File.Exists("__output/Debug/AnyCPU/src/MyClassLibrary/netstandard1.6/MyClassLibrary.dll").ShouldBeTrue();
        File.Exists("__output/Debug/AnyCPU/src/MyClassLibrary/netstandard2.0/MyClassLibrary.dll").ShouldBeTrue();
        File.Exists("__output/Debug/AnyCPU/src/MyClassLibrary/netstandard2.1/MyClassLibrary.dll").ShouldBeTrue();
        Directory.Exists("__intermediate/src/MyClassLibrary/Debug/netstandard1.6").ShouldBeTrue();
        Directory.Exists("__intermediate/src/MyClassLibrary/Debug/netstandard2.0").ShouldBeTrue();
        Directory.Exists("__intermediate/src/MyClassLibrary/Debug/netstandard2.1").ShouldBeTrue();
    }

    /// <summary>
    /// Validates a project in a project folder, but with CentralBuildOutputFolderPrefix set to "_prefix_":
    ///     Directory.Build.props
    ///     Directory.Build.targets
    ///     nuget.config
    ///     src/MyClassLibrary/MyClassLibrary.csproj
    /// </summary>
    [Fact]
    public void PrefixOverride()
    {
        // Arrange
        this.SetupDirectoryBuildProps(
            projectFunction: p => p.Property("CentralBuildOutputFolderPrefix", "_prefix_"));

        // Act
        ProjectCreator project = this.CreateSaveAndBuildProject(() => ProjectCreator.Templates
            .SdkCsproj(path: "src/MyClassLibrary/MyClassLibrary.csproj"));

        // Assert
        Properties properties = Properties.Load(project);

        CentralBuildOutputProperties cboProps = properties.CentralBuildOutput;
        cboProps.BaseIntDir.MakeRelative(this.ProjectOutput).ShouldBe("_prefix_intermediate/");
        cboProps.BaseOutDir.MakeRelative(this.ProjectOutput).ShouldBe("_prefix_output/");
        cboProps.BasePackagesDir.MakeRelative(this.ProjectOutput).ShouldBe("_prefix_packages/");
        cboProps.BasePublishDir.MakeRelative(this.ProjectOutput).ShouldBe("_prefix_publish/");
        cboProps.BaseTestResultsDir.MakeRelative(this.ProjectOutput).ShouldBe("_prefix_test-results/");
        cboProps.CentralBuildOutputFolderPrefix.MakeRelative(this.ProjectOutput).ShouldBe("_prefix_");

        File.Exists("_prefix_output/Debug/AnyCPU/src/MyClassLibrary/netstandard2.0/MyClassLibrary.dll").ShouldBeTrue();
        Directory.Exists("_prefix_intermediate/src/MyClassLibrary/Debug/netstandard2.0").ShouldBeTrue();
    }

    /// <summary>
    /// Validates a project in the root folder:
    ///     Directory.Build.props
    ///     Directory.Build.targets
    ///     nuget.config
    ///     MyClassLibrary.csproj
    /// </summary>
    [Fact]
    public void ProjectInRoot()
    {
        // Arrange
        this.SetupDirectoryBuildProps();

        // Act
        ProjectCreator project = this.CreateSaveAndBuildProject(() => ProjectCreator.Templates
            .SdkCsproj(path: Path.Combine(this.ProjectOutput, "MyClassLibrary.csproj")));

        // Assert
        Properties properties = Properties.Load(project);

        CentralBuildOutputProperties cboProps = properties.CentralBuildOutput;
        cboProps.AppxPackageDir.MakeRelative(this.ProjectOutput).ShouldBe("__output/Debug/AnyCPU/AppPackages/");
        cboProps.BaseIntDir.MakeRelative(this.ProjectOutput).ShouldBe("__intermediate/");
        cboProps.BaseNuGetDir.MakeRelative(this.ProjectOutput).ShouldBe("__packages/NuGet/");
        cboProps.BaseOutDir.MakeRelative(this.ProjectOutput).ShouldBe("__output/");
        cboProps.BasePackagesDir.MakeRelative(this.ProjectOutput).ShouldBe("__packages/");
        cboProps.BaseProjectIntermediateOutputPath.MakeRelative(this.ProjectOutput).ShouldBe("__intermediate/");
        cboProps.BaseProjectPublishOutputPath.MakeRelative(this.ProjectOutput).ShouldBe("__publish/Debug/AnyCPU/");
        cboProps.BaseProjectTestResultsOutputPath.MakeRelative(this.ProjectOutput).ShouldBe("__test-results/");
        cboProps.BasePublishDir.MakeRelative(this.ProjectOutput).ShouldBe("__publish/");
        cboProps.BaseTestResultsDir.MakeRelative(this.ProjectOutput).ShouldBe("__test-results/");
        cboProps.CentralBuildOutputFolderPrefix.MakeRelative(this.ProjectOutput).ShouldBe("__");
        cboProps.CentralBuildOutputPath.MakeRelative(this.ProjectOutput).ShouldBeEmpty();
        cboProps.DefaultArtifactsSource.MakeRelative(this.ProjectOutput).ShouldBe("__packages/NuGet/Debug/");
        cboProps.EnableCentralBuildOutput.MakeRelative(this.ProjectOutput).ShouldBeEmpty();
        cboProps.PackageOutputPath.MakeRelative(this.ProjectOutput).ShouldBe("__packages/NuGet/Debug/");
        cboProps.ProjectIntermediateOutputPath.MakeRelative(this.ProjectOutput).ShouldBe("__intermediate/");
        cboProps.ProjectOutputPath.MakeRelative(this.ProjectOutput).ShouldBe("__output/Debug/AnyCPU/");
        cboProps.RelativeProjectPath.MakeRelative(this.ProjectOutput).ShouldBeEmpty();

        CoverletProperties coverletProps = properties.Coverlet;
        coverletProps.CoverletOutput.MakeRelative(this.ProjectOutput).ShouldBe("__test-results/");

        CommonMSBuildProperties msbuildProps = properties.MSBuildCommon;
        msbuildProps.BaseIntermediateOutputPath.MakeRelative(this.ProjectOutput).ShouldBe("__intermediate/");
        msbuildProps.BaseOutputPath.MakeRelative(this.ProjectOutput).ShouldBe("__output/Debug/AnyCPU/");
        msbuildProps.OutputPath.MakeRelative(this.ProjectOutput).ShouldBe("__output/Debug/AnyCPU/netstandard2.0\\");

        CommonMSBuildMacros msbuildMacros = properties.MSBuildMacros;
        msbuildMacros.PublishDir.MakeRelative(this.ProjectOutput).ShouldBe("__publish/Debug/AnyCPU/");

        MSBuildOtherProperties msBuildOtherProps = properties.MSBuildOther;
        msBuildOtherProps.MSBuildProjectExtensionPath.MakeRelative(this.ProjectOutput).ShouldBe("__intermediate/");

        VSTestProperties vsTestProps = properties.VSTest;
        vsTestProps.VSTestResultsDirectory.MakeRelative(this.ProjectOutput).ShouldBe("__test-results/");
    }

    /// <summary>
    /// Validates a project in a project folder, but with CentralBuildOutputRelativeToPath set to remove the src folder:
    ///     Directory.Build.props
    ///     Directory.Build.targets
    ///     nuget.config
    ///     src/MyClassLibrary/MyClassLibrary.csproj
    /// </summary>
    [Fact]
    public void RelativePathOverride()
    {
        // Arrange
        this.SetupDirectoryBuildProps(
            projectFunction: p => p.Property("CentralBuildOutputRelativeToPath", Path.Combine(this.ProjectOutput, "src")));

        // Act
        ProjectCreator project = this.CreateSaveAndBuildProject(() => ProjectCreator.Templates
            .SdkCsproj(path: "src/MyClassLibrary/MyClassLibrary.csproj"));

        // Assert
        Properties properties = Properties.Load(project);

        CentralBuildOutputProperties cboProps = properties.CentralBuildOutput;
        cboProps.AppxPackageDir.MakeRelative(this.ProjectOutput).ShouldBe("__output/Debug/AnyCPU/MyClassLibrary/AppPackages/");
        cboProps.BaseProjectIntermediateOutputPath.MakeRelative(this.ProjectOutput).ShouldBe("__intermediate/MyClassLibrary/");
        cboProps.BaseProjectPublishOutputPath.MakeRelative(this.ProjectOutput).ShouldBe("__publish/Debug/AnyCPU/MyClassLibrary/");
        cboProps.BaseProjectTestResultsOutputPath.MakeRelative(this.ProjectOutput).ShouldBe("__test-results/MyClassLibrary/");
        cboProps.ProjectIntermediateOutputPath.MakeRelative(this.ProjectOutput).ShouldBe("__intermediate/MyClassLibrary/");
        cboProps.ProjectOutputPath.MakeRelative(this.ProjectOutput).ShouldBe("__output/Debug/AnyCPU/MyClassLibrary/");
        cboProps.RelativeProjectPath.MakeRelative(this.ProjectOutput).ShouldBe("MyClassLibrary/");

        CoverletProperties coverletProps = properties.Coverlet;
        coverletProps.CoverletOutput.MakeRelative(this.ProjectOutput).ShouldBe("__test-results/MyClassLibrary/");

        CommonMSBuildProperties msbuildProps = properties.MSBuildCommon;
        msbuildProps.BaseIntermediateOutputPath.MakeRelative(this.ProjectOutput).ShouldBe("__intermediate/MyClassLibrary/");
        msbuildProps.BaseOutputPath.MakeRelative(this.ProjectOutput).ShouldBe("__output/Debug/AnyCPU/MyClassLibrary/");
        msbuildProps.OutputPath.MakeRelative(this.ProjectOutput).ShouldBe("__output/Debug/AnyCPU/MyClassLibrary/netstandard2.0\\");

        CommonMSBuildMacros msbuildMacros = properties.MSBuildMacros;
        msbuildMacros.PublishDir.MakeRelative(this.ProjectOutput).ShouldBe("__publish/Debug/AnyCPU/MyClassLibrary/");

        MSBuildOtherProperties msBuildOtherProps = properties.MSBuildOther;
        msBuildOtherProps.MSBuildProjectExtensionPath.MakeRelative(this.ProjectOutput).ShouldBe("__intermediate/MyClassLibrary/");

        VSTestProperties vsTestProps = properties.VSTest;
        vsTestProps.VSTestResultsDirectory.MakeRelative(this.ProjectOutput).ShouldBe("__test-results/MyClassLibrary/");

        File.Exists("__output/Debug/AnyCPU/MyClassLibrary/netstandard2.0/MyClassLibrary.dll").ShouldBeTrue();
        Directory.Exists("__intermediate/MyClassLibrary/Debug/netstandard2.0").ShouldBeTrue();
    }

    private ProjectCreator CreateSaveAndBuildProject(Func<ProjectCreator> projectFunction)
    {
        ProjectCreator projectCreator = projectFunction()
            .Save()
            .TryBuild(restore: true, out bool buildResult, out BuildOutput buildOutput);

        // Fail on a failed build, any warnings, or any errors (presumably also failed build).
        if (!buildResult || buildOutput.Warnings.Any() || buildOutput.Errors.Any())
        {
            foreach (string warning in buildOutput.Warnings)
            {
                this.TestOutput.WriteLine("Warning: " + warning);
            }

            foreach (string error in buildOutput.Errors)
            {
                this.TestOutput.WriteLine("Error: " + error);
            }

            buildOutput.Dispose();

            buildResult.ShouldBeTrue();
            buildOutput.Warnings.ShouldBeEmpty();
            buildOutput.Errors.ShouldBeEmpty();
        }

        buildOutput.Dispose();

        return projectCreator;
    }

    private ProjectCreator SetupDirectoryBuildProps(
        string centralBuidOutputPath = "$(MSBuildThisFileDirectory)",
        Action<ProjectCreator>? projectFunction = null)
        => ProjectCreator.Templates.DirectoryBuildProps(
            this.ProjectOutput,
            ThisAssemblyDirectory,
            centralBuidOutputPath,
            projectFunction);
}