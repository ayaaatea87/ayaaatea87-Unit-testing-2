using System;
using FluentAssertions;
using Moq;
using Simulation.Checkout.Application;
using Simulation.Checkout.Application.Abstractions;
using Simulation.Checkout.Domain;
using Simulation.Checkout.Domain.Exceptions;

namespace Simulation.Checkout.Tests;

public class CheckoutServiceTests
{
    [Fact]
    public async Task CheckoutAsync_WhenOrderIsEmpty_ShouldThrowEmptyOrderException()
    {
        // Arrange
        var orderRepositoryMock = new Mock<IOrderRepository>();
        var inventoryServiceMock = new Mock<IInventoryService>();
        var notificationServiceMock = new Mock<INotificationService>();

        var tst = new CheckoutService(
            orderRepositoryMock.Object,
            inventoryServiceMock.Object,
            notificationServiceMock.Object);

        var order = new Order
        {
            CustomerName = "Aya",
            CustomerEmail = "aya@example.com",
            Items = Array.Empty<OrderItem>()
        };

        // Act
        Func<Task> act = () => tst.CheckoutAsync(order);

        // Assert
        await act.Should().ThrowAsync<EmptyOrderException>();
    }
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task CheckoutAsync_WhenQuantityIsInvalid_ShouldThrowInvalidQuantityException(int quantity)
    {
        // Arrange
        var orderRepositoryMock = new Mock<IOrderRepository>();
        var inventoryServiceMock = new Mock<IInventoryService>();
        var notificationServiceMock = new Mock<INotificationService>();

        var tst = new CheckoutService(
            orderRepositoryMock.Object,
            inventoryServiceMock.Object,
            notificationServiceMock.Object);

        var productName = "Laptop";

        var order = new Order
        {
            CustomerName = "Aya",
            CustomerEmail = "aya@example.com",
            Items =
            [
                new OrderItem
            {
                ProductId = Guid.NewGuid(),
                ProductName = productName,
                UnitPrice = 500m,
                Quantity = quantity
            }
            ]
        };

        // Act
        Func<Task> act = () => tst.CheckoutAsync(order);

        // Assert
        var exception = await act.Should()
            .ThrowAsync<InvalidQuantityException>();

        exception.Which.Message.Should().Contain(productName);
    }
    [Fact]
    public async Task CheckoutAsync_WhenPriceIsNegative_ShouldThrowInvalidPriceException()
    {
        // Arrange
        var orderRepositoryMock = new Mock<IOrderRepository>();
        var inventoryServiceMock = new Mock<IInventoryService>();
        var notificationServiceMock = new Mock<INotificationService>();

        var tst = new CheckoutService(
            orderRepositoryMock.Object,
            inventoryServiceMock.Object,
            notificationServiceMock.Object);

        var productName = "Laptop";

        var order = new Order
        {
            CustomerName = "Aya",
            CustomerEmail = "aya@example.com",
            Items =
            [
                new OrderItem
            {
                ProductId = Guid.NewGuid(),
                ProductName = productName,
                UnitPrice = -0.01m,
                Quantity = 1
            }
            ]
        };

        // Act
        Func<Task> act = () => tst.CheckoutAsync(order);

        // Assert
        var exception = await act.Should()
            .ThrowAsync<InvalidPriceException>();

        exception.Which.Message.Should().Contain(productName);
    }
    [Fact]
    public async Task CheckoutAsync_WhenPriceIsZero_ShouldSucceed()
    {
        // Arrange
        var orderRepositoryMock = new Mock<IOrderRepository>();
        var inventoryServiceMock = new Mock<IInventoryService>();
        var notificationServiceMock = new Mock<INotificationService>();

        inventoryServiceMock
            .Setup(x => x.IsAvailableAsync(
                It.IsAny<Guid>(),
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var tst = new CheckoutService(
            orderRepositoryMock.Object,
            inventoryServiceMock.Object,
            notificationServiceMock.Object);

        var order = new Order
        {
            CustomerName = "Aya",
            CustomerEmail = "aya@example.com",
            Items =
            [
                new OrderItem
            {
                ProductId = Guid.NewGuid(),
                ProductName = "Free Product",
                UnitPrice = 0m,
                Quantity = 1
            }
            ]
        };

        // Act
        var receipt = await tst.CheckoutAsync(order);

        // Assert
        receipt.Should().NotBeNull();
        receipt.Subtotal.Should().Be(0m);
        receipt.Discount.Should().Be(0m);
        receipt.Total.Should().Be(0m);
    }
    [Theory]
    [InlineData(999.99, 0)]
    [InlineData(1000, 100)]
    [InlineData(1500, 100)]
    public async Task CheckoutAsync_WhenSubtotalChanges_ShouldApplyCorrectDiscount(
        decimal subtotal,
        decimal expectedDiscount)
    {
        // Arrange
        var orderRepositoryMock = new Mock<IOrderRepository>();
        var inventoryServiceMock = new Mock<IInventoryService>();
        var notificationServiceMock = new Mock<INotificationService>();

        inventoryServiceMock
            .Setup(x => x.IsAvailableAsync(
                It.IsAny<Guid>(),
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var tst = new CheckoutService(
            orderRepositoryMock.Object,
            inventoryServiceMock.Object,
            notificationServiceMock.Object);

        var order = new Order
        {
            CustomerName = "Aya",
            CustomerEmail = "aya@example.com",
            Items =
            [
                new OrderItem
            {
                ProductId = Guid.NewGuid(),
                ProductName = "Laptop",
                UnitPrice = subtotal,
                Quantity = 1
            }
            ]
        };

        // Act
        var receipt = await tst.CheckoutAsync(order);

        // Assert
        receipt.Subtotal.Should().Be(subtotal);
        receipt.Discount.Should().Be(expectedDiscount);
    }
    [Theory]
    [InlineData(150, 2, 400, 1, 700, 0, 700)]
    [InlineData(500, 3, 0, 0, 1500, 100, 1400)]
    public async Task CheckoutAsync_WhenUsingExampleOrders_ShouldCalculateCorrectReceipt(
    decimal firstPrice,
    int firstQuantity,
    int secondPrice,
    int secondQuantity,
    int expectedSubtotal,
    int expectedDiscount,
    int expectedTotal)
    {
        // Arrange
        var orderRepositoryMock = new Mock<IOrderRepository>();
        var inventoryServiceMock = new Mock<IInventoryService>();
        var notificationServiceMock = new Mock<INotificationService>();

        inventoryServiceMock
            .Setup(x => x.IsAvailableAsync(
                It.IsAny<Guid>(),
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var tst = new CheckoutService(
            orderRepositoryMock.Object,
            inventoryServiceMock.Object,
            notificationServiceMock.Object);

        var items = new List<OrderItem>
    {
        new OrderItem
        {
            ProductId = Guid.NewGuid(),
            ProductName = "Product 1",
            UnitPrice = firstPrice,
            Quantity = firstQuantity
        }
    };

        if (secondPrice > 0)
        {
            items.Add(new OrderItem
            {
                ProductId = Guid.NewGuid(),
                ProductName = "Product 2",
                UnitPrice = secondPrice,
                Quantity = secondQuantity
            });
        }

        var order = new Order
        {
            CustomerName = "Aya",
            CustomerEmail = "aya@example.com",
            Items = items
        };

        // Act
        var receipt = await tst.CheckoutAsync(order);

        // Assert
        receipt.Subtotal.Should().Be(expectedSubtotal);
        receipt.Discount.Should().Be(expectedDiscount);
        receipt.Total.Should().Be(expectedTotal);
    }
}
