using System;
using System.Collections.Generic;
using Request;

/*
 * An object that take Requests.
 * 
 * Contains a set of methods that take Requests of various types, returning their respective Guid. 
 */
public interface RequestPort<T> {
    public Guid takeRequest(SetRequest<T> reqeust);
    public Guid takeRequest(MutateRequest<T> request);
    public Guid takeRequest(BlockRequest request);
}

/*
 * Defines a property whose value can only be modified through requests
 */
public class RequestableProperty<T> : RequestPort<T> {
    private T _value;
    private RequestablePropertyReference reference;//Used to get priorities for RequestClass's and execution orders for priorities

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

    public RequestableProperty(T value, RequestablePropertyReference reference) {
        this._value = value;
        this.reference = reference;

        this.priority = -1;
        this.setValue = value;
        this.mutations = new();
    }

    /*
     * Executes all priority requests, and resets priority.
     */
    public void executeRequests() {
        executeUpdateChain();

        //notify senders
        foreach(KeyValuePair<RequestSystem, HashSet<Guid>> entry in senders) {
            entry.Key.onRequestsExecuted(entry.Value);
        }

        resetState();
    }

    /*
     * Takes a SetRequest.  If it has priority its value will be stored.
     */
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
     */
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
     */
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
     */
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
    }

    /*
     * Attempts to set the priority.  Returns true if successful, false otherwise.
     */
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
     */
    private void addSender(RequestSystem sender, Guid requestID) {
        if (!senders.ContainsKey(sender))
            senders[sender] = new();

        senders[sender].Add(requestID);
    }

    /*
     * Resets all fields that track priority, requests, and senders.
     */
    private void resetState() {
        priority = -1;
        senders.Clear();
        setValue = _value;
        mutations.Clear();
    }
}
