namespace Application.Commands
{
    public interface ICommand<TResult>
    {
    }

    public interface ICommand : ICommand<bool>
    {
    }

    public interface ICommandHandler<in TCommand, TResult>
        where TCommand : ICommand<TResult>
    {
        Task<TResult> HandleAsync(TCommand command);
    }

    public interface ICommandHandler<in TCommand> : ICommandHandler<TCommand, bool>
        where TCommand : ICommand
    {
    }
}