using System;
using AutoFixture;
using Simulation.Checkout.Domain;

namespace Simulation.Checkout.Tests.TestData;

public static class OrderTestData
{
    public static Order CreateValidOrder(string customerName, string customerEmail)
    {
        var fixture = new Fixture();

        var item = fixture.Build<OrderItem>()
            .With(x => x.Quantity, 2)
            .With(x => x.UnitPrice, 100m)
            .Create();

        return fixture.Build<Order>()
            .With(x => x.CustomerName, customerName)
            .With(x => x.CustomerEmail, customerEmail)
            .With(x => x.Items, new List<OrderItem> { item })
            .Create();
    }

}
