using System;
using System.Collections.Generic;

public abstract class RequestPoolBase<T> : IRequestManagerBase<T> {
    protected bool setFlag;
    protected T setValue;
    protected Dictionary<RequestClass, List<Func<T, T>>> mutations;

    public RequestPoolBase() {
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
    public virtual T executeRequests(T baseValue, IEnumerable<RequestClass> order) {
        T newValue = setFlag ? setValue : baseValue;
    
        //Ordered mutations
        foreach (RequestClass entry in order) {
            if (mutations.ContainsKey(entry)) {
                foreach(Func<T, T> mutation in mutations[entry]) {
                    newValue = mutation(newValue);
                }
                mutations.Remove(entry);
            }
        }
        //Unordered mutations
        foreach (List<Func<T, T>> mutations in mutations.Values) {
            foreach(Func<T, T> mutation in mutations) {
                newValue = mutation(newValue);
            }
        }

        reset();

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
