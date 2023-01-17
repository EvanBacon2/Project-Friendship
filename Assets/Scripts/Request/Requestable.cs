using System;
using System.Collections.Generic;

public interface IRequest<T> {
    //public T value { get; }
    public bool set(RequestClass priority, T value);
    public bool mutate(RequestClass priority, Func<T, T> mutation);
    public bool block(RequestClass priority);
}

public interface IUniqueRequest<T> {
    //public T value { get; }

    public Guid set(RequestSender sender, RequestClass priority, T value);
    public Guid mutate(RequestSender sender, RequestClass priority, Func<T, T> mutation);
    public Guid block(RequestSender sender, RequestClass priority);
}

public class RequestPool<T> {
    private bool setFlag;
    private T setValue;
    private Dictionary<RequestClass, Func<T, T>> mutations;

    public RequestPool() {
        this.setFlag = false;
        this.mutations = new();
    }

    public virtual T execute(T baseValue, IEnumerable<RequestClass> order) {
        T newValue = setFlag ? setValue : baseValue;
    
        //Ordered mutations
        foreach (RequestClass entry in order) {
            if (mutations.ContainsKey(entry)) {
                newValue = mutations[entry](newValue);
                mutations.Remove(entry);
            }
        }
        //Unordered mutations
        foreach (Func<T, T> mutate in mutations.Values) {
            newValue = mutate(newValue);
        }

        Clear();

        return newValue;
    }

    public void addSet(T value) {
        setFlag = true;
        this.setValue = value;
    }

    public void addMutation(RequestClass reqClass, Func<T, T> mutation) {
        mutations[reqClass] = mutation;
    }

    public virtual void Clear() {
        setFlag = false;
        mutations.Clear();
    }
}

public class NotifyRequestPool<T> : RequestPool<T> {
    private Dictionary<RequestSender, HashSet<Guid>> senders;

    public NotifyRequestPool() {
        this.senders = new();
    }

    public override T execute(T baseValue, IEnumerable<RequestClass> order) {
        foreach(KeyValuePair<RequestSender, HashSet<Guid>> entry in senders) {
            entry.Key.onRequestsExecuted(entry.Value);
        }

        return base.execute(baseValue, order);
    }

    public Guid addSet(RequestSender sender, T value) {
        addSet(value);
        return addSender(sender);
    }

    public Guid addMutation(RequestSender sender, RequestClass reqClass, Func<T, T> mutation) {
        addMutation(reqClass, mutation);
        return addSender(sender);
    }

    public Guid addSender(RequestSender sender) {
        if (!senders.ContainsKey(sender))
            senders[sender] = new();
        
        Guid id = Guid.NewGuid();
        senders[sender].Add(id);
        return id;
    }

    public override void Clear() {
        base.Clear();
        senders.Clear();
    }
}

public class Requestable<T> : IRequest<T>, IUniqueRequest<T> {
    private Func<T> _get;
    private Action<T> _set;

    private RequestablePropertyReference reference;

    private int priority;
    private Dictionary<RequestSender, HashSet<Guid>> senders;

    private NotifyRequestPool<T> pool;

    public T value { 
        get { return _get(); } 
        private set { _set(value); } 
    }

    public Requestable(Func<T> get, Action<T> set, RequestablePropertyReference reference) {
        this.reference = reference;

        this.priority = -1;
        this.senders = new();
        this.pool = new();
    }

    public bool set(RequestClass priority, T value) {
        int refPriority = reference.getPriority(priority);
        return (setPriority(refPriority, () => {
            pool.addSet(value);
        })); 
    }

    public Guid set(RequestSender sender, RequestClass priority, T value) {
        int refPriority = reference.getPriority(priority);
        return setPriority(refPriority) ? pool.addSet(sender, value) : Guid.Empty;
    }

    public bool mutate(RequestClass priority, Func<T, T> mutation) {
        int refPriority = reference.getPriority(priority);
        return (setPriority(refPriority, () => {
            pool.addMutation(priority, mutation);
        }));
    }

    public Guid mutate(RequestSender sender, RequestClass priority, Func<T, T> mutation) {
        int refPriority = reference.getPriority(priority);
        return setPriority(refPriority) ? pool.addMutation(sender, priority, mutation) : Guid.Empty;
    }

    public bool block(RequestClass priority) {
        return setPriority(reference.getPriority(priority));
    }

    public Guid block(RequestSender sender, RequestClass priority) {
        return block(priority) ? pool.addSender(sender) : Guid.Empty;
    }

    public void executeRequests() {
        value = pool.execute(value, reference.getOrder(priority));
        pool.Clear();
    }

    public void setReference(RequestablePropertyReference reference) {
        this.reference = reference;
        pool.Clear();
    }

    /*
     * Attempts to set the priority.  Returns true if successful, false otherwise.
     */
    private bool setPriority(int priority, Action onSuccess) {
        if (priority > this.priority) {
            pool.Clear();

            this.priority = priority;
            onSuccess();

            return true;
        }
            
        return priority == this.priority;
    }

    private bool setPriority(int priority) {
        return setPriority(priority, () => {});
    }

    /*private void notifySenders() {
        foreach(KeyValuePair<RequestSender, HashSet<Guid>> entry in senders) {
            entry.Key.onRequestsExecuted(entry.Value);
        }
    }

    /*
     * Store the requestID for a priority request and associate it with RequestSystem that sent it.
     *
    private Guid addSender(RequestSender sender) {
        if (!senders.ContainsKey(sender))
            senders[sender] = new();
        
        Guid id = Guid.NewGuid();
        senders[sender].Add(id);
        return id;
    }*/

    /*
     * Resets all fields that track priority, requests, and senders.
     */
    private void resetState() {
        priority = -1;
        //senders.Clear();
        pool.Clear();
    }
}

/*public class RequestableValue<T> : Requestable<T> {
    private T _value;

    private RequestManager<T> manager;

    public T value {
        get { return _value; }
    }

    public RequestableValue(T value, RequestablePropertyReference reference, Action<T> onExecuteCallback) {
        Func<T> get = () => { return _value; };
        Action<T> set = (T value) => {
            _value = value;
            onExecuteCallback(value);
        };

        this._value = value;
        this.manager = new RequestManager<T>(get, set, reference);
    }

    public Guid takeRequest(SetRequest<T> request) {
        return manager.takeRequest(request);
    }

    public Guid takeRequest(MutateRequest<T> request) {
        return manager.takeRequest(request);
    }

    public Guid takeRequest(BlockRequest request) {
        return manager.takeRequest(request);
    }
}*/

/*
 * An instance of T whose value can be modified through requests
 */
/*public class RequestableValue<T> : Requestable<T> {
    private T _value;
    private RequestablePropertyReference reference;//Used to get priorities for RequestClass's and execution orders for priorities
    private Action<T> onExecuteCallback;

    private int priority;
    private Dictionary<RequestSystem, HashSet<Guid>> senders;//Maps senders to requestIDs

    private T setValue;//The value specified by the priority SetRequest, set to _value if no such request exists
    private Dictionary<RequestClass, Func<T, T>> mutations;//Collection of all priority mutations.

    public T value {
        get { return _value; }
    }

    public void setReference(RequestablePropertyReference reference) {
        this.reference = reference;
        resetState();
    }

    public RequestableValue(T value, RequestablePropertyReference reference, Action<T> onExecuteCallback) {
        this._value = value;
        this.reference = reference;
        this.onExecuteCallback = onExecuteCallback;

        this.priority = -1;
        this.senders = new();
        this.setValue = value;
        this.mutations = new();
    }

    public RequestableValue(T value, RequestablePropertyReference reference) : this(value, reference, (T v) => {}) {}

    /*
     * Executes all priority requests, and resets priority.
     *
    public void executeRequests() {
        if (priority != -1) {
            executeUpdateChain();

            //notify senders
            foreach(KeyValuePair<RequestSystem, HashSet<Guid>> entry in senders) {
                entry.Key.onRequestsExecuted(entry.Value);
            }
        }
        resetState();
    }

    /*
     * Takes a SetRequest.  If it has priority its value will be stored.
     *
    public Guid takeRequest(SetRequest<T> request) {
        int reqPriority = reference.getPriority(request.requestClass);

        if (setPriority(reqPriority)) {
            setValue = request.value;
            addSender(request.system, request.id);
        }

        return request.id;
    }

    /*
     * Takes a MutateRequest.  If it has priority its mutation will be stored.
     *
    public Guid takeRequest(MutateRequest<T> request) {
        int reqPriority = reference.getPriority(request.requestClass);

        if (setPriority(reqPriority)) {
            mutations[request.requestClass] = request.mutation;
            addSender(request.system, request.id);
        }

        return request.id;
    }

    /*
     * Takes a BlockRequest.  If it has priority then this property's priority will be raised appropriately.
     *
    public Guid takeRequest(BlockRequest request) {
        int reqPriority = reference.getPriority(request.requestClass);

        setPriority(reqPriority);
    
        return request.id;
    }

    /*
     * Executes all requests.  
     * 
     * First, the property's value is set by the priority set request, if it exists.  
     * Then, the ordered mutation requests are executed in their specified order.
     * Lastly, any remaining mutation requests with an unspecified execution order are executed in random order.
     *
    private void executeUpdateChain() {
        T baseValue = setValue;
    
        foreach (RequestClass entry in reference.getOrder(priority)) {
            if (mutations.ContainsKey(entry)) {
                baseValue = mutations[entry](baseValue);
                mutations.Remove(entry);
            }
        }

        foreach (KeyValuePair<RequestClass, Func<T, T>> entry in mutations) {
            baseValue = entry.Value(baseValue);
        }

        _value = baseValue;

        onExecuteCallback(_value);
    }

    /*
     * Attempts to set the priority.  Returns true if successful, false otherwise.
     *
    private bool setPriority(int priority) {
        if (priority > this.priority) {
            setValue = _value;
            mutations.Clear();

            senders.Clear();
            this.priority = priority;

            return true;
        }
            
        return priority == this.priority;
    }

    /*
     * Store the requestID for a priority request and associate it with RequestSystem that sent it.
     *
    private void addSender(RequestSystem sender, Guid requestID) {
        if (!senders.ContainsKey(sender))
            senders[sender] = new();

        senders[sender].Add(requestID);
    }

    /*
     * Resets all fields that track priority, requests, and senders.
     *
    private void resetState() {
        priority = -1;
        senders.Clear();
        setValue = _value;
        mutations.Clear();
    }
}*/
