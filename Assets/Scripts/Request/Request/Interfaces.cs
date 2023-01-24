using System;

public interface IRequestBase<T> {
    public T value { get; }
}

/*
 * Defines a value that can only be changed through requests
 *
 * A return value of true means the request has priority, a value of false
 * means it does not.
 */
public interface IRequest<T> : IRequestBase<T> {
    public bool set(RequestClass priority, T value);
    public bool mutate(RequestClass priority, Func<T, T> mutation);
    public bool block(RequestClass priority);
}
/*
 * Defines a value that can only be changed through requests
 *
 * Requests made to this value will be associated with a Guid
 *
 * A return value of Guid.Empty means the request does not have priority,
 * any other Guid means it does.
 */
public interface IUniqueRequest<T> : IRequestBase<T> {
    public Guid set(RequestSender sender, RequestClass priority, T value);
    public Guid mutate(RequestSender sender, RequestClass priority, Func<T, T> mutation);
    public Guid block(RequestSender sender, RequestClass priority);
}

public interface IAnyRequest<T> : IRequest<T>, IUniqueRequest<T> {}
