using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace TripleTriad;

public sealed class DataContext(DbContextOptions<DataContext> options) : IdentityUserContext<User, string, UserClaim, UserLogin, UserToken>(options)
{
    //public DbSet<Lobby> Lobbies { get; set; } = default!;

    //public DbSet<OnlineUser> OnlineUsers { get; set; } = default!;

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<User>().ToTable("Users");
        builder.Entity<User>().Property(u => u.Id).HasConversion<Guid>();
        builder.Entity<UserClaim>().ToTable("UserClaims");
        builder.Entity<UserLogin>().ToTable("UserLogins");
        builder.Entity<UserToken>().ToTable("UserTokens");

        //builder.Entity<OnlineUser>().HasKey(u => u.UserId);
        //builder.Entity<OnlineLobby>().HasKey(u => u.LobbyId);
    }
}