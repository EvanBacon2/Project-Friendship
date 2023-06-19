using System;
using System.Collections.Generic;

/*
 * A value that can only be updated and set through requests
 *
 * Requsts made to this value will be managed by a RequestManager
 */
public class ManagedRequestableValue<T> : IManagedRequestPort<T> {
    protected T _value;
    protected ManagedRequestableWrapper<T> wrapper;

    public T value {
        get { return _value; }
    }

    public int priority {
        get { return wrapper.priority; }
    }

    public PriorityAlias priorityAlias {
        get { return wrapper.priorityAlias; }
    }

    public ManagedRequestableValue(T value, IPriorityReference reference, IPriorityManager priorityManager, 
            IManageAnyRequest<T> requestManager) {
        _value = value;
        wrapper = new(() => { return _value; }, (T val) => { _value = val; }, reference, priorityManager, requestManager);
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

    public void setReference(IPriorityReference reference) {
        wrapper.setReference(reference);
    }

    public void executeRequests() {
        wrapper.executeRequests();
    }

    public void notifySenders() {
        wrapper.notifySenders();
    }

    public T pendingValue() {
        return wrapper.pendingValue();
    }

    public void addSendersTo(Dictionary<RequestSender, HashSet<Guid>> senders) {
        wrapper.addSendersTo(senders);
    }
}
