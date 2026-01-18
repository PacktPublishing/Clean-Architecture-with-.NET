using Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.EntityFramework;

internal class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(user => user.Id);
        builder.Property(user => user.Id).ValueGeneratedNever();
        builder.Property(user => user.Username).IsRequired().HasMaxLength(50);
        builder.Property(user => user.Email).IsRequired().HasMaxLength(320);
        builder.Property(user => user.FullName).IsRequired().HasMaxLength(100);
        builder.Property(user => user.Roles).IsRequired().HasMaxLength(4000);

        builder.HasIndex(user => user.Username).IsUnique();
    }
}