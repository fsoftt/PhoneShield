using System.Reflection;
using NetArchTest.Rules;
using ArchTestResult = NetArchTest.Rules.TestResult;

namespace Tranqui.Architecture.Tests;

public sealed class LayerDependencyTests
{
    private const string ApplicationNamespace = "Tranqui.Application";
    private const string InfrastructureNamespace = "Tranqui.Infrastructure";
    private const string ApiNamespace = "Tranqui.Api";

    private static readonly Assembly domainAssembly = typeof(Domain.AssemblyReference).Assembly;
    private static readonly Assembly applicationAssembly = typeof(Application.AssemblyReference).Assembly;
    private static readonly Assembly contractsAssembly = typeof(Contracts.AssemblyReference).Assembly;
    private static readonly Assembly appCoreAssembly = typeof(App.Core.DependencyInjection).Assembly;
    private static readonly Assembly backOfficeAssembly = typeof(BackOffice.Strings).Assembly;

    private static readonly string[] frameworkNamespaces =
    [
        "Microsoft.EntityFrameworkCore",
        "Microsoft.AspNetCore",
        "StackExchange.Redis",
        "FirebaseAdmin",
        "Npgsql",
    ];

    [Fact]
    public void Domain_DoesNotDependOnOtherLayers()
    {
        var result = Types.InAssembly(domainAssembly)
            .ShouldNot()
            .HaveDependencyOnAny(ApplicationNamespace, InfrastructureNamespace, ApiNamespace)
            .GetResult();

        result.IsSuccessful.Should().BeTrue(FailingTypes(result));
    }

    [Fact]
    public void Application_DoesNotDependOnInfrastructureOrApi()
    {
        var result = Types.InAssembly(applicationAssembly)
            .ShouldNot()
            .HaveDependencyOnAny(InfrastructureNamespace, ApiNamespace)
            .GetResult();

        result.IsSuccessful.Should().BeTrue(FailingTypes(result));
    }

    [Fact]
    public void DomainAndApplication_DoNotDependOnFrameworks()
    {
        var result = Types.InAssemblies([domainAssembly, applicationAssembly])
            .ShouldNot()
            .HaveDependencyOnAny(frameworkNamespaces)
            .GetResult();

        result.IsSuccessful.Should().BeTrue(FailingTypes(result));
    }

    [Fact]
    public void Contracts_DoNotDependOnAnyOtherLayer()
    {
        var result = Types.InAssembly(contractsAssembly)
            .ShouldNot()
            .HaveDependencyOnAny(ApplicationNamespace, InfrastructureNamespace, ApiNamespace)
            .GetResult();

        result.IsSuccessful.Should().BeTrue(FailingTypes(result));
    }

    [Fact]
    public void AppCore_NeverDependsOnServerLayers()
    {
        var result = Types.InAssembly(appCoreAssembly)
            .ShouldNot()
            .HaveDependencyOnAny(ApplicationNamespace, InfrastructureNamespace, ApiNamespace)
            .GetResult();

        result.IsSuccessful.Should().BeTrue(FailingTypes(result));
    }

    /// <summary>The back office is a client of the API like the app: it only shares the contracts.</summary>
    [Fact]
    public void BackOffice_OnlyTalksToTheApi()
    {
        var result = Types.InAssembly(backOfficeAssembly)
            .ShouldNot()
            .HaveDependencyOnAny("Tranqui.Domain", ApplicationNamespace, InfrastructureNamespace, ApiNamespace)
            .GetResult();

        result.IsSuccessful.Should().BeTrue(FailingTypes(result));
    }

    private static string FailingTypes(ArchTestResult result) =>
        string.Join(", ", result.FailingTypeNames ?? []);
}
