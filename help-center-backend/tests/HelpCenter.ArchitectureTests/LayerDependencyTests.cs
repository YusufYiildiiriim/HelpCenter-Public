using System.Reflection;
using FluentAssertions;
using NetArchTest.Rules;
using Xunit;

namespace HelpCenter.ArchitectureTests;

public class LayerDependencyTests
{
    private static readonly Assembly DomainAsm = typeof(HelpCenter.Domain.Entities.Role).Assembly;
    private static readonly Assembly ApplicationAsm = typeof(HelpCenter.Application.Interfaces.IUserContext).Assembly;
    private static readonly Assembly PersistenceAsm = typeof(HelpCenter.Persistence.Context.EfContext).Assembly;
    private static readonly Assembly WebApiAsm = typeof(HelpCenter.WebApi.Controllers.Admin.AdminCompaniesController).Assembly;

    [Fact]
    public void Domain_should_not_reference_any_project()
    {
        var forbidden = new[] { "HelpCenter.Application", "HelpCenter.Persistence", "HelpCenter.WebApi", "HelpCenter.Infrastructure",
                                 "Microsoft.EntityFrameworkCore", "Microsoft.AspNetCore" };
        var result = Types.InAssembly(DomainAsm).ShouldNot().HaveDependencyOnAny(forbidden).GetResult();
        result.IsSuccessful.Should().BeTrue(
            "Domain saf tutulmalı. İhlal edenler: " + string.Join(", ", result.FailingTypeNames ?? Array.Empty<string>()));
    }

    [Fact]
    public void Application_should_not_reference_Persistence_or_WebApi()
    {
        // All of Microsoft.AspNetCore is forbidden: the Application layer must stay
        // independent of the web framework. File upload uses FileUpload instead of IFormFile,
        // IUserContext instead of HttpContext. Caching and Configuration are consumed via abstractions.
        var forbidden = new[]
        {
            "HelpCenter.Persistence",
            "HelpCenter.WebApi",
            "Microsoft.AspNetCore",
            "Microsoft.Extensions.Caching",
            "Microsoft.Extensions.Configuration",
            "Microsoft.EntityFrameworkCore"
        };
        var result = Types.InAssembly(ApplicationAsm).ShouldNot().HaveDependencyOnAny(forbidden).GetResult();
        result.IsSuccessful.Should().BeTrue(
            "Application dışa bağlanmamalı. İhlal edenler: " + string.Join(", ", result.FailingTypeNames ?? Array.Empty<string>()));
    }

    [Fact]
    public void Controllers_should_not_use_DbContext_or_Repositories_directly()
    {
        var result = Types.InAssembly(WebApiAsm)
            .That().ResideInNamespace("HelpCenter.WebApi.Controllers")
            .ShouldNot().HaveDependencyOnAny("HelpCenter.Persistence", "Microsoft.EntityFrameworkCore")
            .GetResult();
        result.IsSuccessful.Should().BeTrue(
            "Controllers sadece IMediator kullanmalı. İhlal edenler: " + string.Join(", ", result.FailingTypeNames ?? Array.Empty<string>()));
    }

    [Fact]
    public void Application_should_not_contain_concrete_infrastructure_services()
    {
        var offenders = Types.InAssembly(ApplicationAsm)
            .That().ResideInNamespace("HelpCenter.Application.Services")
            .GetTypes()
            .Select(t => t.FullName)
            .ToList();

        offenders.Should().BeEmpty(
            "Application.Services altında somut servis kalmamalı; bunlar Persistence/Infrastructure'a taşınır.");
    }

    [Fact]
    public void Handlers_should_not_send_other_requests_through_mediator()
    {
        var result = Types.InAssembly(ApplicationAsm)
            .That().ImplementInterface(typeof(MediatR.IRequestHandler<,>))
            .ShouldNot().HaveDependencyOnAny("MediatR.IMediator", "MediatR.ISender")
            .GetResult();
        result.IsSuccessful.Should().BeTrue(
            "Handler'lar IMediator/ISender enjekte etmemeli. İhlal edenler: "
            + string.Join(", ", result.FailingTypeNames ?? Array.Empty<string>()));
    }

    [Fact]
    public void Handlers_should_not_throw_business_exceptions_directly()
    {
        var businessExceptions = typeof(HelpCenter.Application.Exceptions.BaseException).Assembly
            .GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract
                && t.Namespace == "HelpCenter.Application.Exceptions"
                && typeof(HelpCenter.Application.Exceptions.BaseException).IsAssignableFrom(t)
                && t != typeof(HelpCenter.Application.Exceptions.InternalServerErrorException))
            .Select(t => t.FullName!)
            .ToArray();

        var result = Types.InAssembly(ApplicationAsm)
            .That().HaveNameEndingWith("Handler")
            .ShouldNot().HaveDependencyOnAny(businessExceptions)
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            "İş kuralları handler'da değil *Rules sınıflarında olmalı. İhlal edenler: "
            + string.Join(", ", result.FailingTypeNames ?? Array.Empty<string>()));
    }

    [Fact]
    public void Handlers_should_end_with_Handler()
    {
        var result = Types.InAssembly(ApplicationAsm)
            .That().ImplementInterface(typeof(MediatR.IRequestHandler<,>))
            .Should().HaveNameEndingWith("Handler")
            .GetResult();
        result.IsSuccessful.Should().BeTrue(
            "Handler'lar 'Handler' ile bitmeli: " + string.Join(", ", result.FailingTypeNames ?? Array.Empty<string>()));
    }

    [Fact]
    public void Validators_should_end_with_Validator_and_inherit_AbstractValidator()
    {
        var result = Types.InAssembly(ApplicationAsm)
            .That().ResideInNamespaceStartingWith("HelpCenter.Application.Features")
            .And().HaveNameEndingWith("Validator")
            .Should().Inherit(typeof(FluentValidation.AbstractValidator<>))
            .GetResult();
        result.IsSuccessful.Should().BeTrue(
            "Tüm validator'lar AbstractValidator'dan türemeli: " + string.Join(", ", result.FailingTypeNames ?? Array.Empty<string>()));
    }

    [Fact]
    public void Rules_classes_should_be_concrete_or_static()
    {
        // Rules classes must be either concrete (injected via DI) or static (containing only
        // stateless validation methods); real abstract classes designed to be inherited
        // (abstract but not sealed) are not allowed here.
        var ruleTypes = Types.InAssembly(ApplicationAsm)
            .That().ResideInNamespaceStartingWith("HelpCenter.Application.Features")
            .And().HaveNameMatching("Rules$")
            .GetTypes();

        var invalid = ruleTypes.Where(t => t.IsAbstract && !t.IsSealed).Select(t => t.FullName).ToList();

        invalid.Should().BeEmpty(
            "Rules sınıfları somut veya static olmalıdır: " + string.Join(", ", invalid));
    }

    [Fact]
    public void Controllers_should_inherit_from_ControllerBase()
    {
        var result = Types.InAssembly(WebApiAsm)
            .That().ResideInNamespace("HelpCenter.WebApi.Controllers")
            .And().HaveNameEndingWith("Controller")
            .Should().Inherit(typeof(Microsoft.AspNetCore.Mvc.ControllerBase))
            .GetResult();
        result.IsSuccessful.Should().BeTrue(
            "Tüm controller'lar ControllerBase'den türemeli: " + string.Join(", ", result.FailingTypeNames ?? Array.Empty<string>()));
    }

    [Fact]
    public void Admin_mutation_endpoints_should_have_a_rate_limit_policy()
    {
        var mutationMethods = Types.InAssembly(WebApiAsm)
            .That().ResideInNamespace("HelpCenter.WebApi.Controllers.Admin")
            .And().HaveNameEndingWith("Controller")
            .GetTypes()
            .SelectMany(type => type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly))
            .Where(method => method.GetCustomAttributes<Microsoft.AspNetCore.Mvc.Routing.HttpMethodAttribute>()
                .Any(attribute => attribute.HttpMethods.Any(httpMethod =>
                    httpMethod is "POST" or "PUT" or "PATCH" or "DELETE")))
            .ToArray();

        mutationMethods.Should().NotBeEmpty();
        mutationMethods.Should().OnlyContain(method =>
            method.GetCustomAttributes<Microsoft.AspNetCore.RateLimiting.EnableRateLimitingAttribute>().Any(),
            "every admin mutation must opt into a named rate-limit policy");
    }
}
