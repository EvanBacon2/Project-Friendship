using System;
using System.Collections.Generic;
using UnityEngine;

public class ManagedRequestableBase<T> : RequestableBase<T>, IManagedRequestBase<T> {
    protected IAnyRequestManager<T> requestManager;

    public ManagedRequestableBase(Func<T> get, Action<T> set, IRequestReference reference, IPriorityManager priority, 
            IAnyRequestManager<T> requestManager) : base(get, set, reference, priority) {
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
}
