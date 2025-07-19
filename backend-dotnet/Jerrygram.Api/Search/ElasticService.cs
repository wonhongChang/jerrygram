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
                    .MultiMatch(m => m
                        .Fields(f => f
                            .Field(p => p.Caption)
                            .Field(p => p.Username)
                        )
                        .Query(query)
                    )
                )
            );

            return [.. response.Documents];
        }
    }
}
