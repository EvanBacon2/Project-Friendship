using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerShipController : MonoBehaviour {
    PlayerShipModel shipModel;
    
    private Dictionary<string, object> priorityRequests;
    private Dictionary<RequestSystem, HashSet<string>> prioritySenders;
    private Dictionary<string, int> requestPriorities;
    private Dictionary<string, object> setRequests;//stores the highest priority set request for each property
    private Dictionary<string, Dictionary<RequestClass, Func<object, object>>> mutateRequests;//stores the highest priority mutate requests for each property
    private Dictionary<string, int> setPriorities;//The priority of the highest priority set request for each property
    private Dictionary<string, int> mutatePriorities;//The priority of the highest priority mutate requests for each property
    private Dictionary<string, int> propertyPriorities;
    private Dictionary<string, Func<object>> updateChains;//stores a callback that, when invoked, executes all requests for the given property
    private Dictionary<RequestSystem, HashSet<string>> setSenders;//The properties for which the request has set the highest set priority
    private Dictionary<RequestSystem, HashSet<string>> mutateSenders;//The properties for which the request has set the highest mutate priority
    
    public void Start() {
        shipModel = GetComponent<PlayerShipModel>();

        priorityRequests = new Dictionary<string, object>();
        prioritySenders = new Dictionary<RequestSystem, HashSet<string>>();
        requestPriorities = new Dictionary<string, int>();

        setRequests = new Dictionary<string, object>();
        mutateRequests = new Dictionary<string, Dictionary<RequestClass, Func<object, object>>>();
        setPriorities = new Dictionary<string, int>();
        mutatePriorities = new Dictionary<string, int>();
        updateChains = new Dictionary<string, Func<object>>();
        setSenders = new Dictionary<RequestSystem, HashSet<string>>();
        mutateSenders = new Dictionary<RequestSystem, HashSet<string>>();
    }

    /*public void executeRequests() {
        foreach (KeyValuePair<string, object> entry in priorityRequests) {
            if (entry.Key == PlayerShipProperties.Force) 
                shipModel.addForce(((Vector3, ForceMode))entry.Value);
            else 
                shipModel.GetType().GetProperty(entry.Key).SetValue(shipModel, entry.Value);
        }

        //notify senders of properties they modified
        foreach (KeyValuePair<Request, HashSet<string>> entry in prioritySenders) {
            entry.Key.onRequestExecuted(entry.Value);
        }

        priorityRequests.Clear();
        prioritySenders.Clear();
        requestPriorities.Clear();
    }*/

    public void makeRequest<T>(RequestSystem sender, RequestClass type, string property, T request) {
        int priority = PlayerShipRequestPriority.getPriority(property, type);
        if (!requestPriorities.ContainsKey(property) || priority > requestPriorities[property]) {
            priorityRequests[property] = request;
            requestPriorities[property] = priority;

            removeSender(prioritySenders, property);
            addSender(prioritySenders, sender, property);
        }
    }

    //A request to raise the priority of the given property to that of the RequestType, without 
    //requesting any change to the value of the property.
    /*public void blockRequest(Request sender, RequestType type, string property) {
        int priority = PlayerShipRequestPriorities.getPriority(property, type);
        if (!requestPriorities.ContainsKey(property) || priority > requestPriorities[property]) {
            priorityRequests.Remove(property);
            requestPriorities[property] = priority;

            removeSender(prioritySenders, property);
            addSender(prioritySenders, sender, property);
        }
    }*/

    public void executeRequests() {
        //execute updateChain for each property
        foreach (KeyValuePair<string, dynamic> entry in updateChains) {
            if (entry.Key == PlayerShipProperties.Force) 
                shipModel.addForce(((Vector3, ForceMode))entry.Value());
            else 
                shipModel.GetType().GetProperty(entry.Key).SetValue(shipModel, entry.Value());
        }

        //notify senders of properties they modified
        foreach (KeyValuePair<RequestSystem, HashSet<string>> entry in setSenders) {
            entry.Key.onRequestExecuted(entry.Value);
        }

        foreach (KeyValuePair<RequestSystem, HashSet<string>> entry in mutateSenders) {
            if (!setSenders.ContainsKey(entry.Key))//don't notify again
                entry.Key.onRequestExecuted(entry.Value);
        }

        setRequests.Clear();
        mutateRequests.Clear();
        setPriorities.Clear();
        mutatePriorities.Clear();
        updateChains.Clear();
        setSenders.Clear();
        mutateSenders.Clear();
    }

    //A request to set a property to a specific value, only the highest priority set request will be executed,
    //and only if its priority is greater or equal to the highest mutate priority.
    public void setRequest<T>(RequestSystem sender, RequestClass type, string property, T request) {
        int priority = PlayerShipRequestPriority.getPriority(property, type);

        /*if (!setPriorities.ContainsKey(property)) 
            setPriorities[property] = -1; 

        if (!mutatePriorities.ContainsKey(property)) 
            mutatePriorities[property] = -1; */

        if (!propertyPriorities.ContainsKey(property))
            propertyPriorities[property] = -1;

        if (priority >= propertyPriorities[property]) {
            if (priority > propertyPriorities[property]) 
                updatePriority(property, priority);

            setRequests[property] = request;
            addSender(setSenders, sender, property);

            if (!updateChains.ContainsKey(property))
                updateChains[property] = prepareUpdateChain<T>(property, priority);
        }

        /*if (priority > setPriorities[property] && priority >= mutatePriorities[property]) {
            if (priority > mutatePriorities[property]) {
                mutateRequests.Remove(property);
                removeSender(mutateSenders, property);
            }

            setRequests[property] = request;
            setPriorities[property] = priority;

            removeSender(setSenders, property);
            addSender(setSenders, sender, property);

            if (!updateChains.ContainsKey(property))
                updateChains[property] = prepareUpdateChain<T>(property, priority);
        }*/
    }

    //A request to mutate the value of a property via the function mutate,  Only the highest priority requests 
    //are executed, and only if their priority is greater than or equal to the highest set priority.  
    //If multiple mutate requests share the highest priority, then they are all executed.
    public void mutateRequest<T>(RequestSystem sender, RequestClass type, string property, Func<T,T> mutate) {
        int priority = PlayerShipRequestPriority.getPriority(property, type);

        /*if (!setPriorities.ContainsKey(property)) 
            setPriorities[property] = -1; 

        if (!mutatePriorities.ContainsKey(property)) 
            mutatePriorities[property] = -1; */

        if (!propertyPriorities.ContainsKey(property))
            propertyPriorities[property] = -1;

        if (priority >= propertyPriorities[property]) {
            if (priority > propertyPriorities[property]) 
                updatePriority(property, priority);

            mutateRequests[property][type] = mutate;
            addSender(mutateSenders, sender, property);

            if (!updateChains.ContainsKey(property))
                updateChains[property] = prepareUpdateChain<T>(property, priority);
        }

        /*if (priority >= setPriorities[property]) {
            if (priority > setPriorities[property]) {
                setRequests.Remove(property);
                removeSender(setSenders, property);
            }

            if (priority > mutatePriorities[property]) {
                mutatePriorities[property] = priority;
                mutateRequests[property] = new Dictionary<RequestType, Func<object, object>>();
                removeSender(mutateSenders, property);
            }
            
            if (priority >= mutatePriorities[property]) {
                mutateRequests[property][type] = mutate;
                addSender(mutateSenders, sender, property);

                if (!updateChains.ContainsKey(property))
                    updateChains[property] = prepareUpdateChain<T>(property, priority);
            }
        }*/
    }

    private void updatePriority(string property, int priority) {
        setRequests.Remove(property);
        mutateRequests[property] = new Dictionary<RequestClass, Func<object, object>>();

        removeSender(setSenders, property);
        removeSender(mutateSenders, property);

        propertyPriorities[property] = priority;
    }

    //Prepares a callback which, when invoked, will execute all requests for a given property.  The first operation
    //performed will always be the priority set request, if one exists.  Then mutation requests are executed.
    //First, if an order of execution is specified for some or all mutation requests then they will be executed in said order.
    //Lastly, any remaining mutation requests with an unspecified execution order are executed in random order.  
    private Func<T> prepareUpdateChain<T>(string property, int priority) {
        return () => { 
            T baseValue;

            if (setRequests.ContainsKey(property))
                baseValue = (T)setRequests[property];
            else
                baseValue = (T)shipModel.GetType().GetProperty(property).GetValue(shipModel);

            foreach (RequestClass entry in PlayerShipRequestPriority.getOrder(property, priority)) {
                if (mutateRequests[property].ContainsKey(entry)) {
                    baseValue = mutateRequests[property][entry](baseValue);
                    mutateRequests[property].Remove(entry);
                }
            }

            foreach (KeyValuePair<RequestClass, dynamic> entry in mutateRequests[property]) {
                baseValue = entry.Value(baseValue);
            }

            return baseValue;
        };
    }

    private Func<T> prepareUpdateChain<T>(string property, int priority, T baseValue, Dictionary<RequestClass, Func<T, T>> mutations) {
        return () => { 
            foreach (RequestClass entry in PlayerShipRequestPriority.getOrder(property, priority)) {
                if (mutations.ContainsKey(entry)) {
                    baseValue = mutations[entry](baseValue);
                    mutations.Remove(entry);
                }
            }

            foreach (KeyValuePair<RequestClass, Func<T, T>> entry in mutations) {
                baseValue = entry.Value(baseValue);
            }

            return baseValue;
        };
    }

    public void blockRequest(RequestSystem sender, RequestClass type, string property) {
        int priority = PlayerShipRequestPriority.getPriority(property, type);

        /*if (!setPriorities.ContainsKey(property)) 
            setPriorities[property] = -1; 

        if (!mutatePriorities.ContainsKey(property)) 
            mutatePriorities[property] = -1; */

        if (!propertyPriorities.ContainsKey(property)) 
            propertyPriorities[property] = -1; 

        /*if (priority >= setPriorities[property] && priority >= mutatePriorities[property]) {
            setRequests.Remove(property);
            mutateRequests[property].Clear();

            setPriorities[property] = priority;
            mutatePriorities[property] = priority;

            removeSender(setSenders, property);
            removeSender(mutateSenders, property);

            addSender(setSenders, sender, property);
        }*/

        if (priority >= propertyPriorities[property])
            updatePriority(property, priority);
    }

    private void addSender(Dictionary<RequestSystem, HashSet<string>> senders, RequestSystem sender, string property) {
        if (!senders.ContainsKey(sender))
            senders[sender] = new HashSet<string>();

        senders[sender].Add(property);
    }

    private void removeSender(Dictionary<RequestSystem, HashSet<string>> senders, string property) {
        foreach (RequestSystem entry in new List<RequestSystem>(senders.Keys)) {
            if (senders[entry].Contains(property)) {
                senders[entry].Remove(property);
                if (senders[entry].Count == 0)
                    senders.Remove(entry);
            }
        }
    }
}
