using Jerrygram.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Jerrygram.Api.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        // Register entity sets for EF Core
        public DbSet<User> Users { get; set; }
        public DbSet<Post> Posts { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<UserFollow> UserFollows { get; set; }
        public DbSet<PostLike> PostLikes { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // --- User Entity Config ---
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Username)
                .IsUnique();

            var adminId = Guid.Parse("11111111-1111-1111-1111-111111111111");
            var postId = Guid.Parse("22222222-2222-2222-2222-222222222222");
            var commentId = Guid.Parse("33333333-3333-3333-3333-333333333333");

            modelBuilder.Entity<User>().HasData(new User
            {
                Id = adminId,
                Username = "admin",
                Email = "admin@jerrygram.com",
                PasswordHash = "hashed-password",  // NOTE: replace with actual hash
                CreatedAt = DateTime.UtcNow
            });

            modelBuilder.Entity<Post>().HasData(new Post
            {
                Id = postId,
                UserId = adminId,
                ImageUrl = "https://placehold.co/600x400/EEE/31343C?font=poppins&text=Jerrygram",
                Caption = "Welcome to Jerrygram!",
                CreatedAt = DateTime.UtcNow
            });

            modelBuilder.Entity<Comment>().HasData(new Comment
            {
                Id = commentId,
                UserId = adminId,
                PostId = postId,
                Content = "First comment on the first post!",
                CreatedAt = DateTime.UtcNow
            });

            // --- UserFollow Unique Constraint (no duplicate follows) ---
            modelBuilder.Entity<UserFollow>()
                .HasIndex(f => new { f.FollowerId, f.FollowingId })
                .IsUnique();

            modelBuilder.Entity<UserFollow>()
                .HasOne(f => f.Follower)
                .WithMany(u => u.Followings)
                .HasForeignKey(f => f.FollowerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<UserFollow>()
                .HasOne(f => f.Following)
                .WithMany(u => u.Followers)
                .HasForeignKey(f => f.FollowingId)
                .OnDelete(DeleteBehavior.Restrict);

            // --- PostLike Unique Constraint (no double likes) ---
            modelBuilder.Entity<PostLike>()
                .HasIndex(l => new { l.UserId, l.PostId })
                .IsUnique();

            modelBuilder.Entity<PostLike>()
                .HasOne(like => like.User)
                .WithMany(user => user.Likes)
                .HasForeignKey(like => like.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<PostLike>()
                .HasOne(like => like.Post)
                .WithMany(post => post.Likes)
                .HasForeignKey(like => like.PostId)
                .OnDelete(DeleteBehavior.Cascade);

            // --- Comment Relations ---
            modelBuilder.Entity<Comment>()
                .HasOne(comment => comment.User)
                .WithMany(user => user.Comments)
                .HasForeignKey(comment => comment.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Comment>()
                .HasOne(comment => comment.Post)
                .WithMany(post => post.Comments)
                .HasForeignKey(comment => comment.PostId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
