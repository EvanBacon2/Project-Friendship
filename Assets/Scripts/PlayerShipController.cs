using System.Collections.Generic;
using UnityEngine;

public class PlayerShipController : MonoBehaviour {
    PlayerShipModel shipModel;
    
    private Dictionary<string, object> priorityRequests;
    private Dictionary<Request, HashSet<string>> prioritySenders;
    private Dictionary<string, int> requestPriorities;

    public void Start() {
        shipModel = GetComponent<PlayerShipModel>();

        priorityRequests = new Dictionary<string, object>();
        prioritySenders = new Dictionary<Request, HashSet<string>>();
        requestPriorities = new Dictionary<string, int>();
    }

    public void executeRequests() {
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
    }

    public void makeRequest<T>(Request sender, RequestType type, string property, T request) {
        int priority = PlayerShipRequestPriorities.getPriority(property, type);
        if (!requestPriorities.ContainsKey(property) || priority > requestPriorities[property]) {
            priorityRequests[property] = request;
            requestPriorities[property] = priority;

            removeSenderProperty(property);
            addSenderProperty(sender, property);
        }
    }

    public void blockRequest(Request sender, RequestType type, string property) {
        int priority = PlayerShipRequestPriorities.getPriority(property, type);
        if (!requestPriorities.ContainsKey(property) || priority > requestPriorities[property]) {
            priorityRequests.Remove(property);
            requestPriorities[property] = priority;

            removeSenderProperty(property);
            addSenderProperty(sender, property);
        }
    }

    private void addSenderProperty(Request sender, string property) {
        if (!prioritySenders.ContainsKey(sender))
            prioritySenders[sender] = new HashSet<string>();

        prioritySenders[sender].Add(property);
    }

    private void removeSenderProperty(string property) {
        foreach (Request entry in new List<Request>(prioritySenders.Keys)) {
            if (prioritySenders[entry].Contains(property)) {
                prioritySenders[entry].Remove(property);
                if (prioritySenders[entry].Count == 0)
                    prioritySenders.Remove(entry);
            }
        }
    }
}

/**
 * Todo
 *  - finish implementing BoostRequest.
 */
