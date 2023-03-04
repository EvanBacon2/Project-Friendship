using System;

public class AnyRequestableValue<T> : RequestableValueBase<T>, IAnyRequest<T>, IUniqueRequest<T>, IRequest<T> {
    protected AnyRequestable<T> wrapper;

    public int priority {
        get { return wrapper.priority; }
    }

    public RequestClass priorityClass {
        get { return wrapper.priorityClass; }
    }

    public AnyRequestableValue(T value, IRequestReference reference, IPriorityManager priorityManager) : 
            base(value) {
        wrapper = new(getVal, setVal, reference, priorityManager);
    }

    public bool set(RequestClass priority, T value) {
        return wrapper.set(priority, value);
    }

    public Guid set(RequestSender sender, RequestClass rClass, T value) {
        return wrapper.set(sender, rClass, value);
    }

    public bool mutate(RequestClass priority, Func<T, T> mutation) {
        return wrapper.mutate(priority, mutation);
    }

    public Guid mutate(RequestSender sender, RequestClass rClass, Func<T, T> mutation) {
        return wrapper.mutate(sender, rClass, mutation);
    }

    public bool block(RequestClass priority) {
        return wrapper.block(priority);
    }

    public Guid block(RequestSender sender, RequestClass rClass) {
        return wrapper.block(sender, rClass);
    }

    public override void setReference(IRequestReference reference) {
        wrapper.setReference(reference);
    }
}
