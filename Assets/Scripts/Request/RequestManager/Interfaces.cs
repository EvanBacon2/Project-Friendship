using System;
using System.Collections.Generic;

public interface IRequestManagerBase<T> {
    public T executeRequests(T baseValue, IEnumerable<PriorityAlias> order);
    public T pendingValue(T baseValue, IEnumerable<PriorityAlias> order);
    public void reset();
}

public interface IUniqueRequestManagerBase {
    public void notifySenders();
    public void addSendersTo(Dictionary<RequestSender, HashSet<Guid>> senders);
}

public interface IRequestManager<T> : IRequestManagerBase<T> {
    public void manageSet(T value);
    public void manageMutation(PriorityAlias requestClass, Func<T, T> mutation);
}

public interface IUniqueRequestManager<T> : IUniqueRequestManagerBase {
    public Guid manageSet(RequestSender sender, T value);
    public Guid manageMutation(RequestSender sender, PriorityAlias requestClass, Func<T, T> mutation);
    public Guid manageSender(RequestSender sender);
}

public interface IAnyRequestManager<T> : IRequestManager<T>, IUniqueRequestManager<T> {}
