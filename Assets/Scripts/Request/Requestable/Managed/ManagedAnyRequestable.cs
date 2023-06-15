using System;

public class ManagedAnyRequestable<T> : ManagedRequestableBase<T>, IManagedRequestPort<T> {
    protected new IAnyRequestManager<T> requestManager;

    public ManagedAnyRequestable(Func<T> get, Action<T> set, IRequestReference reference, IPriorityManager priority, 
            IAnyRequestManager<T> requestManager) : base(get, set, reference, priority, requestManager) {
        this.requestManager = requestManager;
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
