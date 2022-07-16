using System.Collections.Generic;
using UnityEngine;

public class PlayerShipController {
    PlayerShipModel shipModel;

    private Dictionary<string, object> priorityRequests;
    private HashSet<string> blockedRequests;
    private Dictionary<string, int> requestPriorities;

    public PlayerShipController(PlayerShipModel shipModel) {
        this.shipModel = shipModel;

        priorityRequests = new Dictionary<string, object>();
        blockedRequests = new HashSet<string>();
        requestPriorities = new Dictionary<string, int>();
    }

    public void executeRequests() {
        foreach (KeyValuePair<string, object> entry in priorityRequests) {
            if (entry.Key == PlayerShipProperties.Force) 
                shipModel.addForce(((Vector3, ForceMode))entry.Value);
            else 
                shipModel.GetType().GetProperty(entry.Key).SetValue(shipModel, entry.Value);
        }

        priorityRequests.Clear();
        blockedRequests.Clear();
        requestPriorities.Clear();
    }

    public void makeRequest<T>(string property, int priorityID, T request) {
        int priority = PlayerShipRequestPriorities.getPriority(property, priorityID);
        if (!priorityRequests.ContainsKey(property) || priority > requestPriorities[property]) {
            priorityRequests[property] = request;
            requestPriorities[property] = priority;
            blockedRequests.Remove(property);
        }
    }

    public void blockRequest(string property, int priorityID) {
        int priority = PlayerShipRequestPriorities.getPriority(property, priorityID);
        if (!priorityRequests.ContainsKey(property) || priority > requestPriorities[property]) {
            requestPriorities[property] = priority;
            priorityRequests.Remove(property);
            blockedRequests.Add(property);
        }
    }
}

/**
 * Todo
 *  - Find way to cut down on ShipController code.(Generics?)
 *  - add ability for requests to know if they were chosen.
 *  - finish implementing BoostRequest.
 */
