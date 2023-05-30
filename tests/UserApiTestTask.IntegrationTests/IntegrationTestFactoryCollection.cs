using UserApiTestTask.Infrastructure.Persistence;
using Xunit;

namespace UserApiTestTask.IntegrationTests;

/// <summary>
/// Определение общей коллекции <see cref="IntegrationTestFactory{TProgram, TDbContext}"/>
/// </summary>
[CollectionDefinition("FactoryCollection")]
public class IntegrationTestFactoryCollection : ICollectionFixture<IntegrationTestFactory<Program, ApplicationDbContext>>
{
}
