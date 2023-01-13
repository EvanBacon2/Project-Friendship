using System;
using System.Collections.Generic;
using Request;

/*
* A RequestController takes requests and keeps track of the priority for each property.  
* It can then execute these requests, updating the appropriate properties.
*
* This base class only contains code for storing and updating the priorities of properties, and the senders 
* of priority requests.  
* 
* Code for storing and updating the requests themselves must be implemented in a sub class where the explicit 
* property types are known.
*/
public abstract class RequestController {
    private PriorityReference reference;//Used to get priorities and orders of execution
    protected PlayerShipModel model;//Contains the properties which will be updated by the requests

    private Dictionary<string, int> priorities;//Stores a property's priority
    private Dictionary<RequestSystem, Dictionary<string, List<string>>> senders;//Stores the senders of each priority request, seperated by RequestSystem, then by property
    private List<Action> updateChains;//Stores updateChains. 

    public RequestController(PriorityReference reference, PlayerShipModel/*make RequestableModel*/ model) {
        this.reference = reference;
        this.model = model;
        this.priorities = new();
        this.senders = new();
        this.updateChains = new();
    }

    /*
    * Methods for updating and acessing priority requests.
    * 
    * These methods, along with the actual data structures to store the requests, must be implemented in a sub class.
    */

    protected abstract void storePriorityRequest<T>(SetRequest<T> req);
    protected abstract void storePriorityRequest<T>(MutateRequest<T> req);
    protected abstract void clearSetRequest(string property);
    protected abstract void clearMutationRequests(string property);
    protected abstract void clearAllRequests();

    /*
    * Returns the value of the priority SetRequest for property.  If no such request exists, the value of property
    * in model should be returned
    */
    protected abstract T getSetValue<T>(string property);
    /*
    * Returns the list of mutations for property.
    */
    protected abstract Dictionary<RequestClass, Func<T, T>> getMutations<T>(string property);

    /*
    * Executes all priority requests, and resets priorities for all properties
    */
    public void executeRequests() {
        //execute all updateChains
        foreach(Action updateChain in updateChains) {
            updateChain();
        }
        
        //notify senders
        foreach(KeyValuePair<RequestSystem, Dictionary<string, List<string>>> entry in senders) {
            //entry.Key.onRequestExecuted(entry.Value);
        }

        //reset storage
        priorities.Clear();
        senders.Clear();
        updateChains.Clear();
        clearAllRequests();
    }

    /*
    * Takes a SetRequest.  If it's priority is greater than or equal to any Request recieved for 
    * the given property, then it is stored until it is either executed, another, higher priority,
    * Request for the same property is recieved, or a BlockRequest with a greater than or equal priority
    * is recieved
    */
    public void takeRequest<T>(SetRequest<T> req) {
        int priority = reference.getPriority(req.property, req.requestClass);

        if (!priorities.ContainsKey(req.property))
            prepareUpdateChain<T>(req.property, priority);

        if (setPriority(req.property, priority)) {
            storePriorityRequest<T>(req);
            addSender(req.system, req.property, req.id);
        }
    }

    /*
    * Takes a MutateRequest.  If it's priority is greater than or equal to any Request recieved for 
    * the given property, then it is stored until it is either executed, another, higher priority,
    * Request for the same property is recieved, or a BlockRequest with a greater than or equal priority
    * is recieved.
    */
    public void takeRequest<T>(MutateRequest<T> req) {
        int priority = reference.getPriority(req.property, req.requestClass);

        if (!priorities.ContainsKey(req.property))
            prepareUpdateChain<T>(req.property, priority);

        if (setPriority(req.property, priority)) {
            storePriorityRequest<T>(req);
            addSender(req.system, req.property, req.id);
        }
    }

    /*
    * Takes a BlockRequest.  If it's priority is greater than or equal to any Request recieved for 
    * the given property, then it is stored until it is either executed or another, higher priority,
    * Request for the same property is recieved.
    */
    public void takeRequest(BlockRequest req) {
        int priority = reference.getPriority(req.property, req.requestClass);

        setPriority(req.property, priority);
    }

    /*
    * Prepares a callback which, when invoked, will execute all requests for a given property.  
    * 
    * First, the property's value is set by the priority set request, if it exists.  
    * Then, the ordered mutation requests are executed in their specified order.
    * Lastly, any remaining mutation requests with an unspecified execution order are executed in random order.
    */
    private void prepareUpdateChain<T>(string property, int priority) {
        updateChains.Add(() => { 
            T baseValue = getSetValue<T>(property);
            Dictionary<RequestClass, Func<T, T>> mutations = getMutations<T>(property);
        
            foreach (RequestClass entry in reference.getOrder(property, priority)) {
                if (mutations.ContainsKey(entry)) {
                    baseValue = mutations[entry](baseValue);
                    mutations.Remove(entry);
                }
            }

            foreach (KeyValuePair<RequestClass, Func<T, T>> entry in mutations) {
                baseValue = entry.Value(baseValue);
            }

            model.GetType().GetProperty(property).SetValue(model, baseValue);
        });
    }

    //Attempts to set the priority of property.  Returns true if successful, false otherwise.
    private bool setPriority(string property, int priority) {
        if (!priorities.ContainsKey(property)) 
            priorities[property] = -1;

        if (priority > priorities[property]) 
            updatePriority(property, priority);

        return priority >= priorities[property];
    }

    /*
    * Updates the priority of property. Should only be called to raise the priority of a property.
    */
    private void updatePriority(string property, int priority) {
        clearSetRequest(property);
        clearMutationRequests(property);

        removeSenders(property);
        priorities[property] = priority;
    }

    /*
    * Marks sender as having a priority request, identified by requestID, for property.
    */
    private void addSender(RequestSystem sender, string property, string requestID) {
        if (!senders.ContainsKey(sender)) 
            senders[sender] = new();
        
        if (!senders[sender].ContainsKey(property))
            senders[sender][property] = new();

        senders[sender][property].Add(requestID);
    }

    /*
    * Marks all senders as no longer having any priority requests for property.
    */
    private void removeSenders(string property) {
        foreach(KeyValuePair<RequestSystem, Dictionary<string, List<string>>> senderPair in senders) {
            senders[senderPair.Key].Remove(property);
            
            if (senderPair.Value.Count == 0)
                senders.Remove(senderPair.Key);
        }
    }
}
