using System;

public class ManagedAnyRequestable<T> : ManagedRequestableBase<T>, IManagedAnyRequest<T> {
    protected new IAnyRequestManager<T> requestManager;

    public ManagedAnyRequestable(Func<T> get, Action<T> set, IRequestReference reference, IPriorityManager priority, 
            IAnyRequestManager<T> requestManager) : base(get, set, reference, priority, requestManager) {
        this.requestManager = requestManager;
    }

    public bool set(RequestClass rClass, T value) {
        return priorityManager.setPriority(reference.priority(rClass), () => {
            requestManager.manageSet(value);
        }); 
    }

    public Guid set(RequestSender sender, RequestClass rClass, T value) {
        bool success = priorityManager.setPriority(reference.priority(rClass));
        return success ? requestManager.manageSet(sender, value) : Guid.Empty;
    }

    public bool mutate(RequestClass rClass, Func<T, T> mutation) {
        return priorityManager.setPriority(reference.priority(rClass), () => {
            requestManager.manageMutation(rClass, mutation);
        });
    }

    public Guid mutate(RequestSender sender, RequestClass rClass, Func<T, T> mutation) {
        bool success = priorityManager.setPriority(reference.priority(rClass));
        return success ? requestManager.manageMutation(sender, rClass, mutation) : Guid.Empty;
    }

    public virtual bool block(RequestClass rClass) {
        return priorityManager.setPriority(reference.priority(rClass));
    }

    public virtual Guid block(RequestSender sender, RequestClass priority) {
        bool success = priorityManager.setPriority(reference.priority(priority));
        return success ? Guid.NewGuid() : Guid.Empty;
    }
}
