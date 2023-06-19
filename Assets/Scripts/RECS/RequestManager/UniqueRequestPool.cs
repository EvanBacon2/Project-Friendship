using System;
using System.Collections.Generic;

public class UniqueRequestPool<T> : RequestPool<T>, IUniqueRequestManager {
    protected Dictionary<RequestSender, HashSet<Guid>> senders;

    public UniqueRequestPool() {
        this.senders = new();
    }

    public void notifySenders() {
        foreach(KeyValuePair<RequestSender, HashSet<Guid>> entry in senders) {
            entry.Key.onRequestsExecuted(entry.Value);
        }
    }

    public void addSendersTo(Dictionary<RequestSender, HashSet<Guid>> jointSenders) {
        foreach(RequestSender sender in senders.Keys) {
            if (!jointSenders.ContainsKey(sender))
                jointSenders[sender] = new HashSet<Guid>();

            foreach(Guid id in senders[sender]) {
                jointSenders[sender].Add(id);
            }
        }
    }

    public override void reset() {
        base.reset();
        senders.Clear();
    }
}
