using System;

public class RequestableWrapper<T> : PriorityWrapper<T>, IRequestPort<T>, IUniqueRequest<T>, IRequest<T> {
    public RequestableWrapper(Func<T> get, Action<T> set, IRequestReference reference, IPriorityManager priority) :
            base(get, set, reference, priority) {
    }

    public virtual bool set(PriorityAlias rClass, T value) {
        return priorityManager.setPriority(rClass, reference.priority(rClass), () => {
            this.value = value;
        }); 
    }

    public virtual Guid set(RequestSender sender, PriorityAlias rClass, T value) {
        return set(rClass, value) ? Guid.NewGuid() : Guid.Empty; 
    }

    public virtual bool mutate(PriorityAlias rClass, Func<T, T> mutation) {
        return priorityManager.setPriority(rClass, reference.priority(rClass), () => {
            this.value = mutation(this.value);
        });
    }

    public virtual Guid mutate(RequestSender sender, PriorityAlias rClass, Func<T, T> mutation) {
        return mutate(rClass, mutation) ? Guid.NewGuid() : Guid.Empty;
    }

    public virtual bool block(PriorityAlias rClass) {
        return priorityManager.setPriority(rClass, reference.priority(rClass));
    }

    public virtual Guid block(RequestSender sender, PriorityAlias rClass) {
        return block(rClass) ? Guid.NewGuid() : Guid.Empty;
    }
}



