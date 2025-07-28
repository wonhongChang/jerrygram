package com.jerrygram.application.interfaces;

/**
 * Generic interface for query handlers in CQRS pattern
 * @param <TQuery> The query type
 * @param <TResult> The result type
 */
public interface IQueryHandler<TQuery, TResult> {
    TResult handle(TQuery query);
}