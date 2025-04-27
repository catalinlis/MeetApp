using API.Entities;
using API.Entities.ManyToMany;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace API.Data;

public class DataContext(DbContextOptions options) : 
            IdentityDbContext<AppUser, IdentityRole<int>, int>(options) {

    public DbSet<Interest> Interests { get; set;} = null!;
    public DbSet<FriendRequest> FriendRequests { get; set; } = null!;
    public DbSet<Friendship> Friendships { get; set; } = null!;
    public DbSet<Chat> Chats { get; set; } = null!;
 
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<AppUser>().HasMany(x => x.Interests)
                        .WithMany(x => x.Users)
                        .UsingEntity<UserInterest>(
                            x => x.HasOne( i => i.Interest)
                                    .WithMany( intermediary => intermediary.UsersInterest )
                                    .HasForeignKey( k => k.InterestId),
                            x => x.HasOne( u => u.User )
                                    .WithMany( intermediary => intermediary.UserInterests )
                                    .HasForeignKey( k => k.UserId ));

        builder.Entity<FriendRequest>()
                        .HasKey(fr => new { fr.SentUserId, fr.ReceiverUserId });

        builder.Entity<FriendRequest>().HasOne(x => x.SentUser)
                        .WithMany(sf => sf.SentFriendRequests)
                        .HasForeignKey(x => x.SentUserId)
                        .IsRequired();

        builder.Entity<FriendRequest>().HasOne(x => x.ReceivedUser)
                        .WithMany(rf => rf.ReceivedFriendRequest)
                        .HasForeignKey(x => x.ReceiverUserId)
                        .IsRequired();

        builder.Entity<Friendship>()
            .HasKey(f => new { f.UserId, f.FriendId });

        builder.Entity<Friendship>().HasOne(x => x.User)
                        .WithMany(x => x.Friends)
                        .HasForeignKey(x => x.UserId)
                        .IsRequired();

        builder.Entity<Friendship>().HasOne(x => x.Friend)
                        .WithMany()
                        .HasForeignKey(x => x.FriendId)
                        .IsRequired();

        builder.Entity<Chat>()
            .HasKey(c => new { c.ChatFirstUserId, c.ChatSecondUserId });

        builder.Entity<Chat>().HasOne(x => x.ChatFristUser)
                        .WithMany(x => x.Chats)
                        .HasForeignKey(x => x.ChatFirstUserId)
                        .IsRequired();

        builder.Entity<Chat>().HasOne(x => x.ChatSecondUser)
                        .WithMany()
                        .HasForeignKey(x => x.ChatSecondUserId)
                        .IsRequired();

    }

}