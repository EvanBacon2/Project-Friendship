using System;
using System.Collections.Generic;

/*
 * Wraps a variable in a view that allows its value to only be changed through requests
 * 
 * Requsts made to this variable will be managed by a RequestManager
 */
public class ManagedRequestableWrapper<T> : PriorityWrapper<T>, IManagedPriorityValue<T>, IManagedRequestPort<T> {
    protected IManageAnyRequest<T> requestManager;

    public ManagedRequestableWrapper(Func<T> get, Action<T> set, IPriorityReference reference, IPriorityManager priority, 
            IManageAnyRequest<T> requestManager) : base(get, set, reference, priority) {
        this.requestManager = requestManager;
    }

    public void executeRequests() {
        value = requestManager.executeRequests(value, reference.order(priorityManager.priority));
        reset();
    }

    public T pendingValue() {
        return requestManager.pendingValue(value, reference.order(priorityManager.priority));
    }

    public void notifySenders() {
        requestManager.notifySenders();
    }

    public void addSendersTo(Dictionary<RequestSender, HashSet<Guid>> senders) {
        requestManager.addSendersTo(senders);
    }

    protected override void reset() {
        base.reset();
        requestManager.reset();
    }

    public bool set(PriorityAlias rClass, T value) {
        return priorityManager.setPriority(rClass, reference.priority(rClass), () => {
            requestManager.manageSet(value);
        }); 
    }

    public Guid set(RequestSender sender, PriorityAlias rClass, T value) {
        bool success = priorityManager.setPriority(rClass, reference.priority(rClass));
        return success ? requestManager.manageSet(sender, value) : Guid.Empty;
    }

    public bool mutate(PriorityAlias rClass, Func<T, T> mutation) {
        return priorityManager.setPriority(rClass, reference.priority(rClass), () => {
            requestManager.manageMutation(rClass, mutation);
        });
    }

    public Guid mutate(RequestSender sender, PriorityAlias rClass, Func<T, T> mutation) {
        bool success = priorityManager.setPriority(rClass, reference.priority(rClass));
        return success ? requestManager.manageMutation(sender, rClass, mutation) : Guid.Empty;
    }

    public virtual bool block(PriorityAlias rClass) {
        return priorityManager.setPriority(rClass, reference.priority(rClass));
    }

    public virtual Guid block(RequestSender sender, PriorityAlias rClass) {
        bool success = priorityManager.setPriority(rClass, reference.priority(rClass));
        return success ? Guid.NewGuid() : Guid.Empty;
    }
}
