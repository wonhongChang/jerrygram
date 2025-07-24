using Application.Common;
using Application.Interfaces;
using Nest;

namespace Infrastructure.Services
{
    public class ElasticService : IElasticService
    {
        private readonly IElasticClient _client;

        public ElasticService(IElasticClient client)
        {
            _client = client;
        }

        public async Task IndexUserAsync(UserIndex userIndex)
        {
            await _client.IndexDocumentAsync(userIndex);
        }

        public async Task IndexPostAsync(PostIndex postIndex)
        {
            await _client.IndexDocumentAsync(postIndex);
        }

        public async Task IndexTagAsync(TagIndex tagIndex)
        {
            var response = await _client.IndexAsync(tagIndex, i => i
                .Index("tags")
                .Id(tagIndex.Id)
            );

            if (!response.IsValid)
                throw new Exception($"Failed to index tag: {response.DebugInformation}");
        }

        public async Task UpdateUserAsync(UserIndex userIndex)
        {
            await _client.UpdateAsync<UserIndex>(userIndex.Id, u => u.Doc(userIndex));
        }

        public async Task UpdatePostAsync(PostIndex postIndex)
        {
            await _client.UpdateAsync<PostIndex>(postIndex.Id, u => u.Doc(postIndex));
        }

        public async Task DeletePostAsync(string postId)
        {
            if (Guid.TryParse(postId, out var guidPostId))
            {
                await _client.DeleteAsync<PostIndex>(guidPostId);
            }
        }

        // Additional methods for search functionality

        public async Task<List<PostIndex>> SearchPostsAsync(string query)
        {
            var response = await _client.SearchAsync<PostIndex>(s => s
                .Query(q => q
                    .Bool(b => b
                        .Should(
                            bs => bs.Wildcard(w => w
                                .Field("caption.keyword")
                                .Value($"*{query.ToLower()}*")
                            ),
                            bs => bs.Wildcard(w => w
                                .Field("username.keyword")
                                .Value($"*{query.ToLower()}*")
                            )
                        )
                    )
                )
            );

            return [.. response.Documents];
        }

        public async Task<List<UserIndex>> SearchUsersAsync(string query, int size = 10)
        {
            var response = await _client.SearchAsync<UserIndex>(s => s
                .Size(size)
                .Query(q => q
                    .Wildcard(w => w
                        .Field("username.keyword")
                        .Value($"{query.ToLower()}*")
                    )
                )
            );

            return [.. response.Documents];
        }

        public async Task<List<TagIndex>> SearchTagsAsync(string query, int size = 3)
        {
            var response = await _client.SearchAsync<TagIndex>(s => s
                .Index("tags")
                .Size(size)
                .Query(q => q
                    .MatchPhrasePrefix(m => m
                        .Field(f => f.Name)
                        .Query(query.ToLower())
                    )
                )
            );

            return [.. response.Documents];
        }
    }
}