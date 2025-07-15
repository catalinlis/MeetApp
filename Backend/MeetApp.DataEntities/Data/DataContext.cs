using MeetApp.DataEntities.Entities;
using MeetApp.DataEntities.Entities.ManyToMany;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace MeetApp.DataEntities.Data;

public class DataContext(DbContextOptions options) : 
            IdentityDbContext<AppUser, IdentityRole<int>, int>(options) {

    public DbSet<Interest> Interests { get; set;} = null!;
    public DbSet<FriendRequest> FriendRequests { get; set; } = null!;
    public DbSet<Friendship> Friendships { get; set; } = null!;
    public DbSet<Chat> Chats { get; set; } = null!;
    public DbSet<Photo> Photos { get; set; } = null!;
    public DbSet<PhotoLikes> PhotoLikes { get; set; } = null!;
    public DbSet<PhotoComment> PhotosComments { get; set; } = null!;
    public DbSet<PostInterest> PostInterests { get; set; } = null!;
    public DbSet<Post> Posts { get; set; } = null!;
    public DbSet<PostLikes> PostLikes { get; set; } = null!;
    public DbSet<PostComment> PostsComments { get; set; } = null!;
    public DbSet<Notification> Notifications { get; set; } = null!;

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

        builder.Entity<AppUser>().HasMany(x => x.Posts)
                        .WithOne(x => x.CreatedBy)
                        .HasForeignKey(x => x.CreatedById)
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired(false);

        builder.Entity<AppUser>().HasMany(x => x.Photos)
                        .WithOne(x => x.AddedBy)
                        .HasForeignKey(x => x.AddedById)
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired(false);


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

        builder.Entity<Post>()
                .HasMany(x => x.Comments)
                .WithOne(x => x.Post)
                .HasForeignKey(x => x.PostId)
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired(false);

        builder.Entity<Photo>()
                .HasMany(x => x.Comments)
                .WithOne(x => x.Photo)
                .HasForeignKey(x => x.PhotoId)
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired(false);

        builder.Entity<PostInterest>()
                .HasOne(x => x.Post)
                .WithMany(pi => pi.PostInterests)
                .HasForeignKey(x => x.PostId);

        builder.Entity<PostInterest>()
                .HasOne(x => x.Interest)
                .WithMany(pi => pi.PostsInterest)
                .HasForeignKey(x => x.InterestId);

        builder.Entity<PostInterest>()
                .HasKey(pi => new { pi.InterestId, pi.PostId });

        builder.Entity<PhotoLikes>()
                .HasOne(x => x.User)
                .WithMany(pl => pl.PhotosLiked)
                .HasForeignKey(x => x.UserId);

        builder.Entity<PhotoLikes>()
                .HasOne(x => x.Photo)
                .WithMany(pl => pl.LikedByUsers)
                .HasForeignKey(x => x.PhotoId)
                .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<PhotoLikes>()
                .HasKey(pl => new { pl.PhotoId, pl.UserId });

        builder.Entity<PostLikes>()
                .HasOne(x => x.User)
                .WithMany(pl => pl.PostLiked)
                .HasForeignKey(x => x.UserId);

        builder.Entity<PostLikes>()
                .HasOne(x => x.Post)
                .WithMany(pl => pl.LikedByUsers)
                .HasForeignKey(x => x.PostId)
                .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<PostLikes>()
                .HasKey(pl => new { pl.PostId, pl.UserId });

        builder.Entity<Notification>()
                .HasOne(x => x.SenderUser)
                .WithMany(x => x.SentNotifications)
                .HasForeignKey(x => x.SenderUserId)
                .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Notification>()
                .HasOne(x => x.TargetUser)
                .WithMany(x => x.ReceivedNotifications)
                .OnDelete(DeleteBehavior.Restrict);
    }

}