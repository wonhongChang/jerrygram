package com.jerrygram.application.interfaces;

/**
 * Generic interface for command handlers in CQRS pattern
 * @param <TCommand> The command type
 * @param <TResult> The result type
 */
public interface ICommandHandler<TCommand, TResult> {
    TResult handle(TCommand command);
}