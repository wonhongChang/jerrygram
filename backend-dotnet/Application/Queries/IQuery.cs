namespace Application.Queries
{
    public interface IQuery<TResult>
    {
    }

    public interface IQueryHandler<in TQuery, TResult>
        where TQuery : IQuery<TResult>
    {
        Task<TResult> HandleAsync(TQuery query);
    }
}