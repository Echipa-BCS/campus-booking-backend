namespace CampusBooking.Tests.Infrastructure;

[CollectionDefinition(Name)]
public sealed class IntegrationTestCollection : ICollectionFixture<CampusBookingApiFactory>
{
    public const string Name = "CampusBookingIntegration";
}
