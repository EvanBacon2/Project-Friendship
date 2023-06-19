using System;

/*
 * A reqeust pool that, upon execution, notifies all RequestSenders that their requests were executed.
 */
public class PoolManager<T> : UniqueRequestPool<T>, IManageAnyRequest<T> {
    public void manageSet(T value) {
        setFlag = true;
        this.setValue = value;
    }

    public Guid manageSet(RequestSender sender, T value) {
        manageSet(value);
        return manageSender(sender);
    }

    public void manageMutation(PriorityAlias rClass, Func<T, T> mutation) {
        if (!mutations.ContainsKey(rClass))
            mutations[rClass] = new();

        mutations[rClass].Add(mutation);
    }

    public Guid manageMutation(RequestSender sender, PriorityAlias reqClass, Func<T, T> mutation) {
        manageMutation(reqClass, mutation);
        return manageSender(sender);
    }

    public Guid manageSender(RequestSender sender) {
        if (!senders.ContainsKey(sender))
            senders[sender] = new();
        
        Guid id = Guid.NewGuid();
        senders[sender].Add(id);
        return id;
    }
}
