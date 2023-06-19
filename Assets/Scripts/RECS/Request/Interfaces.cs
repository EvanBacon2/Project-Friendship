using System;

/*
 * A Request is an operation to perform on a variable, combined with a priority
 *
 * Requests can be used to change the value of a PriorityValue.
 *
 * The three types of requests are:
 *
 * Set - Set the target variable to a given value
 * Mutate - Pass the target variable in to a given function, then set its value to the return of the function 
 * Block - Block other requests based off of the priority of this request 
 */

/*
 * Defines a set of methods for submiting requests
 *
 * A return value of true means the request has priority, a value of false
 * means it does not.
 */
public interface IRequest<T> {
    public bool set(PriorityAlias priority, T value);
    public bool mutate(PriorityAlias priority, Func<T, T> mutation);
    public bool block(PriorityAlias priority);
}

/*
 * Defines a set of methods for submiting requests
 *
 * Requests made to this value will be associated with a Guid
 *
 * A return value of Guid.Empty means the request does not have priority,
 * any other Guid means it does.
 */
public interface IUniqueRequest<T> {
    public Guid set(RequestSender sender, PriorityAlias priority, T value);
    public Guid mutate(RequestSender sender, PriorityAlias priority, Func<T, T> mutation);
    public Guid block(RequestSender sender, PriorityAlias priority);
}

/*
 * Defines a set of methods that gives read access to a priority value, as well as
 * methods that can be used to request changes to its value.
 */
public interface IRequestPort<T> : IRequest<T>, IUniqueRequest<T>, IPriorityValue<T> {}

/*
 * Defines a set of methods that gives read access to a managed priority value, as well as
 * methods that can be used to request changes to its value.
 */
public interface IManagedRequestPort<T>: IRequest<T>, IUniqueRequest<T>, IManagedPriorityValue<T> {}
