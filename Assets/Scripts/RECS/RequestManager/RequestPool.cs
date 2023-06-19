using System;
using System.Collections.Generic;
using UnityEngine;

/*
 * A RequestManager that stores priority requests in a pool that can executed all at once at a later time
 */
public abstract class RequestPool<T> : IRequestManager<T> {
    protected bool setFlag;
    protected T setValue;
    protected Dictionary<PriorityAlias, List<Func<T, T>>> mutations;

    public RequestPool() {
        this.setFlag = false;
        this.mutations = new();
    }

    /*
     * Executes all stored requests on baseValue.
     *
     * Requests are executed in the following order.
     *
     * The starting value is set according to the stored set request, if one does not exist, baseValue is used instead.
     * Ordered mutation requests are executed according to the given order.
     * Any remaining mutation requests are executed in random order.
     * 
     * The pool will be cleared upon execution of all requests.
     */
    public virtual T executeRequests(T baseValue, IEnumerable<PriorityAlias> order) {
        T newValue = pendingValue(baseValue, order);
        reset();
        return newValue;
    }

    /*
     * Applies all priority requests to the baseValue and returns the result
     */
    public virtual T pendingValue(T baseValue, IEnumerable<PriorityAlias> order) {
        T newValue = setFlag ? setValue : baseValue;

        HashSet<PriorityAlias> orderedClasses = new();

        //Ordered mutations
        foreach (PriorityAlias entry in order) {
            if (mutations.ContainsKey(entry)) {
                foreach(Func<T, T> mutation in mutations[entry]) {
                    newValue = mutation(newValue);
                }
                orderedClasses.Add(entry);
            }
        }

        //Unordered mutations
        foreach (PriorityAlias entry in mutations.Keys) {
            if (!orderedClasses.Contains(entry)) {
                foreach(Func<T, T> mutation in mutations[entry]) {
                    newValue = mutation(newValue);
                }
            }
        }

        return newValue;
    }

    /*
     * Remove all requests from the pool.
     */
    public virtual void reset() {
        setFlag = false;
        mutations.Clear();
    }
}
