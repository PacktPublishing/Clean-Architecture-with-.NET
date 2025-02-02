namespace Infrastructure.IntegrationTests.Persistence;

[CollectionDefinition(nameof(SharedTestCollection))]
public class SharedTestCollection : ICollectionFixture<TestInitializer>;