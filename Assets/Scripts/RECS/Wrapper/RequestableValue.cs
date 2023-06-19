using System;

/*
 * A value that can only be updated and set through requests
 */
public class RequestableValue<T> : IRequestPort<T>, IUniqueRequest<T>, IRequest<T> {
    protected T _value;
    protected RequestableWrapper<T> wrapper;

    public T value {
        get { return _value; }
    }

    public int priority {
        get { return wrapper.priority; }
    }

    public PriorityAlias priorityAlias {
        get { return wrapper.priorityAlias; }
    }

    public RequestableValue(T value, IPriorityReference reference, IPriorityManager priorityManager) {
        _value = value;
        wrapper = new(() => { return _value; }, (T val) => { _value = val; }, reference, priorityManager);
    }

    public bool set(PriorityAlias priority, T value) {
        return wrapper.set(priority, value);
    }

    public Guid set(RequestSender sender, PriorityAlias rClass, T value) {
        return wrapper.set(sender, rClass, value);
    }

    public bool mutate(PriorityAlias priority, Func<T, T> mutation) {
        return wrapper.mutate(priority, mutation);
    }

    public Guid mutate(RequestSender sender, PriorityAlias rClass, Func<T, T> mutation) {
        return wrapper.mutate(sender, rClass, mutation);
    }

    public bool block(PriorityAlias priority) {
        return wrapper.block(priority);
    }

    public Guid block(RequestSender sender, PriorityAlias rClass) {
        return wrapper.block(sender, rClass);
    }

    public  void setReference(IPriorityReference reference) {
        wrapper.setReference(reference);
    }
}