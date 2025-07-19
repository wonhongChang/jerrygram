using Jerrygram.Api.Search.IndexModels;
using Nest;

namespace Jerrygram.Api.Search
{
    public class ElasticService
    {
        private readonly IElasticClient _client;

        public ElasticService(IElasticClient client)
        {
            _client = client;
        }

        public async Task IndexPostAsync(PostIndex post)
        {
            await _client.IndexDocumentAsync(post);
        }

        public async Task DeletePostAsync(Guid postId)
        {
            await _client.DeleteAsync<PostIndex>(postId);
        }

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

        public async Task IndexUserAsync(UserIndex user)
        {
            await _client.IndexDocumentAsync(user);
        }

        public async Task<List<UserIndex>> SearchUsersAsync(string query)
        {
            var response = await _client.SearchAsync<UserIndex>(s => s
                .Query(q => q
                    .Wildcard(w => w
                        .Field("username.keyword")
                        .Value($"{query.ToLower()}*")
                    )
                )
            );

            return [.. response.Documents];
        }
    }
}
