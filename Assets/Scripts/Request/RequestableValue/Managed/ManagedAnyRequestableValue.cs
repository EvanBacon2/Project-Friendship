using System;
using System.Collections.Generic;

public class ManagedAnyRequestableValue<T> : ManagedRequestableValueBase<T>, IManagedRequestPort<T> {
    protected ManagedAnyRequestable<T> wrapper;

    public int priority {
        get { return wrapper.priority; }
    }

    public PriorityAlias priorityClass {
        get { return wrapper.priorityClass; }
    }

    public ManagedAnyRequestableValue(T value, IRequestReference reference, IPriorityManager priorityManager, 
            IAnyRequestManager<T> requestManager) : base(value) {
        wrapper = new(getVal, setVal, reference, priorityManager, requestManager);
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

    public override void setReference(IRequestReference reference) {
        wrapper.setReference(reference);
    }

    public override void executeRequests() {
        wrapper.executeRequests();
    }

    public override void notifySenders() {
        wrapper.notifySenders();
    }

    public override T pendingValue() {
        return wrapper.pendingValue();
    }

    public override void addSendersTo(Dictionary<RequestSender, HashSet<Guid>> senders) {
        wrapper.addSendersTo(senders);
    }
}
