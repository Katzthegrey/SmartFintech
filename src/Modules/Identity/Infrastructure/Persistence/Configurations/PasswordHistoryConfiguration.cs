using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartFintechFinancial.Modules.Identity.Domain.Entities;


namespace SmartFintechFinancial.Modules.Identity.Infrastructure.Persistence.Configurations;

public class PasswordHistoryConfiguration : IEntityTypeConfiguration<PasswordHistory>
{
    public void Configure(EntityTypeBuilder<PasswordHistory> builder)
    {
        builder.ToTable("password_histories");

        builder.HasKey(ph => ph.Id);

        builder.Property(ph => ph.Id)
            .HasColumnName("id")
            .HasDefaultValueSql("gen_random_uuid()");

        builder.Property(ph => ph.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        builder.Property(ph => ph.PasswordHash)
            .HasColumnName("password_hash")
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(ph => ph.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.HasIndex(ph => ph.UserId)
            .HasDatabaseName("ix_password_histories_user_id");

        builder.HasIndex(ph => ph.CreatedAt)
            .HasDatabaseName("ix_password_histories_created_at");
    }
}