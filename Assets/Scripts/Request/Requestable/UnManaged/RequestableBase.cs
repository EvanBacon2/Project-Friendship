using System;

public class RequestableBase<T> : IRequestBase<T> {
    protected Func<T> _get;
    protected Action<T> _set;

    protected IRequestReference reference;
    protected IPriorityManager priorityManager;

    public T value {
        get { return _get(); } 
        protected set { _set(value); } 
    }

    public int priority {
        get { return priorityManager.priority; }
    }

    public RequestClass priorityClass {
        get { return priorityManager.priorityClass; }
    }

    public RequestableBase(Func<T> get, Action<T> set, IRequestReference reference, IPriorityManager priority) {
        this._get = get;
        this._set = set;
        this.reference = reference;
        this.priorityManager = priority;
    }

    public void setReference(IRequestReference reference) {
        this.reference = reference;
    }

    protected virtual void reset() {
        priorityManager.reset();
    }
}
