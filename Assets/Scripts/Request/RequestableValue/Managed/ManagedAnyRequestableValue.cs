using System;
using System.Collections.Generic;

public class ManagedAnyRequestableValue<T> : ManagedRequestableValueBase<T>, IAnyRequest<T> {
    protected ManagedAnyRequestable<T> wrapper;

    public ManagedAnyRequestableValue(T value, IRequestReference reference, IPriorityManager priorityManager, 
            IAnyRequestManager<T> requestManager) : base(value) {
        wrapper = new(getVal, setVal, reference, priorityManager, requestManager);
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

    public override void executeRequests() {
        wrapper.executeRequests();
    }

    public void addSendersTo(Dictionary<RequestSender, HashSet<Guid>> senders) {
        wrapper.addSendersTo(senders);
    }
}
