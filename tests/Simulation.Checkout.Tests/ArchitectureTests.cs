using System;
using FluentAssertions;
using NetArchTest.Rules;
using Simulation.Checkout.Application;
using Simulation.Checkout.Application.Abstractions;
using Simulation.Checkout.Domain;
using Simulation.Checkout.Infrastructure;

namespace Simulation.Checkout.Tests;

public class ArchitectureTests
{
    [Fact]
    public void Domain_ShouldNotDependOnApplication()
    {
        // Arrange
        var result = Types.InAssembly(typeof(Order).Assembly)
            .ShouldNot()
            .HaveDependencyOn("Simulation.Checkout.Application")
            .GetResult();

        // Act & Assert
        result.IsSuccessful.Should().BeTrue();
    }

    [Fact]
    public void Domain_ShouldNotDependOnInfrastructure()
    {
        // Arrange
        var result = Types.InAssembly(typeof(Order).Assembly)
            .ShouldNot()
            .HaveDependencyOn("Simulation.Checkout.Infrastructure")
            .GetResult();

        // Act & Assert
        result.IsSuccessful.Should().BeTrue();
    }

    [Fact]
    public void Application_ShouldNotDependOnInfrastructure()
    {
        // Arrange
        var result = Types.InAssembly(typeof(CheckoutService).Assembly)
            .ShouldNot()
            .HaveDependencyOn("Simulation.Checkout.Infrastructure")
            .GetResult();

        // Act & Assert
        result.IsSuccessful.Should().BeTrue();
    }

    [Fact]
    public void Interfaces_ShouldStartWithI()
    {
        // Arrange
        var result = Types.InAssembly(typeof(IOrderRepository).Assembly)
            .That()
            .AreInterfaces()
            .Should()
            .HaveNameStartingWith("I")
            .GetResult();

        // Act & Assert
        result.IsSuccessful.Should().BeTrue();
    }

    [Fact]
    public void OrderRepositoryImplementations_ShouldEndWithRepository()
    {
        // Arrange
        var result = Types.InAssembly(typeof(InMemoryOrderRepository).Assembly)
            .That()
            .ImplementInterface(typeof(IOrderRepository))
            .Should()
            .HaveNameEndingWith("Repository")
            .GetResult();

        // Act & Assert
        result.IsSuccessful.Should().BeTrue();
    }
}

