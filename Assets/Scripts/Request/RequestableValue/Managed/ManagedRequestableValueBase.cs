using System;
using System.Collections.Generic;

public abstract class ManagedRequestableValueBase<T> : RequestableValueBase<T> {
    public ManagedRequestableValueBase(T value) : base(value) {}

    public abstract void executeRequests();
    public abstract T pendingValue();
    public abstract void notifySenders();
    public abstract void addSendersTo(Dictionary<RequestSender, HashSet<Guid>> senders);
}
