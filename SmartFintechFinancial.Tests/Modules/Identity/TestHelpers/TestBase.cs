using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using SmartFintechFinancial.Modules.Identity.Domain.Entities;
using SmartFintechFinancial.Modules.Identity.Infrastructure.Persistence;
using SmartFintechFinancial.Shared.Infrastructure.Persistence;
using Xunit;

namespace SmartFintechFinancial.Tests.Modules.Identity.Infrastructure.Persistence;

public abstract class TestBase : IDisposable
{
    protected readonly IdentityDbContext Context;
    protected readonly Mock<ILogger<IdentityDbContext>> LoggerMock;
    protected readonly DateTime TestStartTime;

    protected TestBase()
    {
        TestStartTime = DateTime.UtcNow;
        LoggerMock = new Mock<ILogger<IdentityDbContext>>();

        var options = new DbContextOptionsBuilder<IdentityDbContext>()
            .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
            .Options;

        Context = new IdentityDbContext(options, LoggerMock.Object);
    }

    public void Dispose()
    {
        Context.Database.EnsureDeleted();
        Context.Dispose();
    }

    protected async Task<T> ExecuteWithAuditCheck<T>(Func<Task<T>> action,
        Action<T>? additionalValidation = null)
    {
        var result = await action();

        // Verify audit fields were set
        var entries = Context.ChangeTracker.Entries<IAuditableEntity>();
        foreach (var entry in entries)
        {
            var entity = entry.Entity;
            Assert.True(entity.CreatedAt >= TestStartTime);
            Assert.NotNull(entity.CreatedBy);

            if (entry.State == EntityState.Modified)
            {
                Assert.True(entity.UpdatedAt >= TestStartTime);
                Assert.NotNull(entity.UpdatedBy);
            }
        }

        additionalValidation?.Invoke(result);
        return result;
    }
}