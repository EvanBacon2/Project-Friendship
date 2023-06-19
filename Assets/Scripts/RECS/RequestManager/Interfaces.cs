using System;
using System.Collections.Generic;

public interface IRequestManager<T> {
    public T executeRequests(T baseValue, IEnumerable<PriorityAlias> order);
    public T pendingValue(T baseValue, IEnumerable<PriorityAlias> order);
    public void reset();
}

public interface IUniqueRequestManager {
    public void notifySenders();
    public void addSendersTo(Dictionary<RequestSender, HashSet<Guid>> senders);
}

public interface IManageRequest<T> : IRequestManager<T> {
    public void manageSet(T value);
    public void manageMutation(PriorityAlias requestClass, Func<T, T> mutation);
}

public interface IManageUniqueRequest<T> : IUniqueRequestManager {
    public Guid manageSet(RequestSender sender, T value);
    public Guid manageMutation(RequestSender sender, PriorityAlias requestClass, Func<T, T> mutation);
    public Guid manageSender(RequestSender sender);
}

public interface IManageAnyRequest<T> : IManageRequest<T>, IManageUniqueRequest<T> {}
