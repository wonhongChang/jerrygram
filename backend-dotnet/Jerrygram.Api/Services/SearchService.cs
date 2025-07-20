using Jerrygram.Api.Data;
using Jerrygram.Api.Models;
using Jerrygram.Api.Search;
using Microsoft.EntityFrameworkCore;

namespace Jerrygram.Api.Services
{
    public class SearchService
    {
        private readonly AppDbContext _context;
        private readonly ElasticService _elastic;

        public SearchService(AppDbContext context, ElasticService elastic)
        {
            _context = context;
            _elastic = elastic;
        }

        public async Task<object> SearchAsync(string query, string? userIdStr)
        {
            Guid? userId = string.IsNullOrEmpty(userIdStr) ? null : Guid.Parse(userIdStr);
            List<Guid> followingIds = [];

            if (userId != null)
            {
                followingIds = await _context.UserFollows
                    .Where(f => f.FollowerId == userId)
                    .Select(f => f.FollowingId)
                    .ToListAsync();
            }

            if (query.StartsWith('#'))
            {
                var tag = query.TrimStart('#').ToLowerInvariant();

                var postIds = await _context.PostTags
                    .Where(pt => pt.Tag.Name == tag)
                    .Select(pt => pt.PostId)
                    .Distinct()
                    .ToListAsync();

                var posts = await _context.Posts
                    .Include(p => p.User)
                    .Where(p => postIds.Contains(p.Id))
                    .ToListAsync();

                var filtered = posts.Where(p =>
                    p.Visibility == PostVisibility.Public ||
                    (p.Visibility == PostVisibility.FollowersOnly && userId != null && followingIds.Contains(p.UserId)));

                return new
                {
                    query,
                    posts = filtered.Select(p => new
                    {
                        p.Id,
                        p.Caption,
                        p.User.Username,
                        p.CreatedAt
                    })
                };
            }
            else
            {
                var posts = await _elastic.SearchPostsAsync(query);
                var filtered = posts.Where(p =>
                    p.Visibility == PostVisibility.Public.ToString() ||
                    (p.Visibility == PostVisibility.FollowersOnly.ToString() && userId != null && followingIds.Contains(p.UserId))
                );

                return new
                {
                    query,
                    posts = filtered.Select(p => new
                    {
                        p.Id,
                        p.Caption,
                        p.Username,
                        p.CreatedAt
                    })
                };
            }
        }

        public async Task<object> AutocompleteAsync(string query)
        {
            query = query.Trim();

            if (query.StartsWith('#'))
            {
                var keyword = query.TrimStart('#').ToLowerInvariant();
                var tags = await _elastic.SearchTagsAsync(keyword, 3);
                var users = await _elastic.SearchUsersAsync(keyword, 10);

                if (tags.Any())
                {
                    return new
                    {
                        mode = "tag",
                        tags = tags.Select(t => t.Name),
                        users = users.Select(u => u.Username)
                    };
                }
                else
                {
                    return new
                    {
                        mode = "tag",
                        tags = Array.Empty<string>(),
                        users = Array.Empty<string>(),
                        fallback = $"Search for \"{query}\""
                    };
                }
            }

            var tagResults = await _elastic.SearchTagsAsync(query.ToLowerInvariant(), 3);
            var userResults = await _elastic.SearchUsersAsync(query, 10);

            return new
            {
                mode = "default",
                tags = tagResults.Select(t => t.Name),
                users = userResults.Select(u => u.Username)
            };
        }
    }
}
