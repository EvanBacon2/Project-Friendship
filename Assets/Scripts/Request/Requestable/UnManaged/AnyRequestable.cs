using System;

public class AnyRequestable<T> : RequestableBase<T>, IAnyRequest<T>, IUniqueRequest<T>, IRequest<T> {
    public AnyRequestable(Func<T> get, Action<T> set, IRequestReference reference, IPriorityManager priority) :
            base(get, set, reference, priority) {
    }

    public virtual bool set(RequestClass rClass, T value) {
        return priorityManager.setPriority(reference.priority(rClass), () => {
            this.value = value;
        }); 
    }

    public virtual Guid set(RequestSender sender, RequestClass rClass, T value) {
        return set(rClass, value) ? Guid.NewGuid() : Guid.Empty; 
    }

    public virtual bool mutate(RequestClass rClass, Func<T, T> mutation) {
        return priorityManager.setPriority(reference.priority(rClass), () => {
            this.value = mutation(this.value);
        });
    }

    public virtual Guid mutate(RequestSender sender, RequestClass rClass, Func<T, T> mutation) {
        return mutate(rClass, mutation) ? Guid.NewGuid() : Guid.Empty;
    }

    public virtual bool block(RequestClass rClass) {
        return priorityManager.setPriority(reference.priority(rClass));
    }

    public virtual Guid block(RequestSender sender, RequestClass rClass) {
        return block(rClass) ? Guid.NewGuid() : Guid.Empty;
    }
}



