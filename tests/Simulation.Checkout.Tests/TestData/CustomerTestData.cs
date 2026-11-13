using System;
using Bogus;

namespace Simulation.Checkout.Tests.TestData;

public sealed record CustomerData(string Name, string Email);

public static class CustomerTestData
{
    public static CustomerData Create()
    {
        var faker = new Faker<CustomerData>("en")
            .UseSeed(1)
            .CustomInstantiator(f => new CustomerData(
                f.Name.FullName(),
                f.Internet.Email()));

        return faker.Generate();
    }
}

