using System;

/*
 * Wraps a variable in a read only view of its value, and associated priority
 *
 * This class is abstract as it provides no public logic for setting the variable being wrapped.
 * Said logic should be implemented in a sub class
 */
public abstract class PriorityWrapper<T> : IPriorityValue<T> {
    protected Func<T> _get;
    protected Action<T> _set;

    protected IPriorityReference reference;
    protected IPriorityManager priorityManager;

    public T value {
        get { return _get(); } 
        protected set { _set(value); } 
    }

    public int priority {
        get { return priorityManager.priority; }
    }

    public PriorityAlias priorityAlias {
        get { return priorityManager.priorityClass; }
    }

    public PriorityWrapper(Func<T> get, Action<T> set, IPriorityReference reference, IPriorityManager priority) {
        this._get = get;
        this._set = set;
        this.reference = reference;
        this.priorityManager = priority;
    }

    public void setReference(IPriorityReference reference) {
        this.reference = reference;
    }

    protected virtual void reset() {
        priorityManager.reset();
    }
}
