using System.Collections.Generic;
using UnityEngine;

public class PlayerShipController {
    PlayerShipModel shipModel;

    private Dictionary<string, object> priorityRequests;
    private Dictionary<Request, List<string>> prioritySenders;
    private Dictionary<string, int> requestPriorities;

    public PlayerShipController(PlayerShipModel shipModel) {
        this.shipModel = shipModel;

        priorityRequests = new Dictionary<string, object>();
        prioritySenders = new Dictionary<Request, List<string>>();
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
        foreach (KeyValuePair<Request, List<string>> entry in prioritySenders) {
            entry.Key.onRequestExecuted(entry.Value);
        }

        priorityRequests.Clear();
        prioritySenders.Clear();
        requestPriorities.Clear();
    }

    public void makeRequest<T>(Request sender, string property, T request) {
        int priority = PlayerShipRequestPriorities.getPriority(property, sender.type);
        if (!priorityRequests.ContainsKey(property) || priority > requestPriorities[property]) {
            priorityRequests[property] = request;
            requestPriorities[property] = priority;

            if (!prioritySenders.ContainsKey(sender))
                prioritySenders[sender] = new List<string>();

            prioritySenders[sender].Add(property);
        }
    }

    public void blockRequest(Request sender, string property) {
        int priority = PlayerShipRequestPriorities.getPriority(property, sender.type);
        if (!priorityRequests.ContainsKey(property) || priority > requestPriorities[property]) {
            priorityRequests.Remove(property);
            requestPriorities[property] = priority;

            //remove entry from prioritySenders
            foreach (KeyValuePair<Request, List<string>> entry in prioritySenders) {
                if (entry.Value.Contains(property)) {
                    entry.Value.Remove(property);
                    if (entry.Value.Count == 0)
                        prioritySenders.Remove(entry.Key);
                }
            }

            if (!prioritySenders.ContainsKey(sender))
                prioritySenders[sender] = new List<string>();

            prioritySenders[sender].Add(property);
        }
    }
}

/**
 * Todo
 *  - add ability for requests to know what properties they affected.
 *  - finish implementing BoostRequest.
 */
