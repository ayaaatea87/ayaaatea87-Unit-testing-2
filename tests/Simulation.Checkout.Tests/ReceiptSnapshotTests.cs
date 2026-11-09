using System;
using FluentAssertions;
using Moq;
using Simulation.Checkout.Application;
using Simulation.Checkout.Application.Abstractions;
using Simulation.Checkout.Domain;
using VerifyXunit;

namespace Simulation.Checkout.Tests;
// Task-4 :
public class ReceiptSnapshotTests
{
    [Fact]
    public async Task CheckoutAsync_WhenUsingExampleC_ShouldMatchReceiptSnapshot()
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

        var order = new Order
        {
            CustomerName = "Aya",
            CustomerEmail = "aya@example.com",
            Items =
            [
                new OrderItem
                {
                    ProductId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                    ProductName = "Laptop",
                    UnitPrice = 500m,
                    Quantity = 3
                }
            ]
        };

        var tst = new CheckoutService(
            orderRepositoryMock.Object,
            inventoryServiceMock.Object,
            notificationServiceMock.Object);

        // Act
        var receipt = await tst.CheckoutAsync(order);

        // Assert
        receipt.Subtotal.Should().Be(1500m);
        receipt.Discount.Should().Be(100m);
        receipt.Total.Should().Be(1400m);

        // Ignore OrderId , CreatedAt changes
        var settings = new VerifySettings();
        settings.IgnoreMember("OrderId");
        settings.IgnoreMember("CreatedAt");
        await Verifier.Verify(receipt);
    }
}

