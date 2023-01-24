using System;
using System.Collections.Generic;

public class UniqueRequestPoolBase<T> : RequestPoolBase<T>, IUniqueRequestManagerBase {
    protected Dictionary<RequestSender, HashSet<Guid>> senders;

    public UniqueRequestPoolBase() {
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
                jointSenders[sender] = senders[sender];
            else {
                foreach(Guid id in senders[sender]) {
                    jointSenders[sender].Add(id);
                }
            }   
        }
    }

    public override void reset() {
        base.reset();
        senders.Clear();
    }
}
