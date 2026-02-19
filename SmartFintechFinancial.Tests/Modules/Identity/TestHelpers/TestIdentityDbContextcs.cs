using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using SmartFintechFinancial.Modules.Identity.Infrastructure.Persistence;

namespace SmartFintechFinancial.Tests.Modules.Identity.TestHelpers;

public class TestIdentityDbContext : IdentityDbContext
{
    private readonly string _testDbName;

    public TestIdentityDbContext(string? testDbName = null)
        : base(
            new DbContextOptionsBuilder<IdentityDbContext>()
                .UseInMemoryDatabase(testDbName ?? $"TestDb_{Guid.NewGuid()}")
                .Options,
            new Mock<ILogger<IdentityDbContext>>().Object)
    {
        _testDbName = testDbName ?? Guid.NewGuid().ToString();
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // Don't call base - we want in-memory, not PostgreSQL
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseInMemoryDatabase(_testDbName);
        }
    }
}