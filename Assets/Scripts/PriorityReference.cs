using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PriorityReference {
    private readonly List<RequestClass> noOrder = new List<RequestClass>();
    protected abstract Dictionary<string, Dictionary<RequestClass, int>> propertyMap { get; }
    protected abstract Dictionary<string, Dictionary<int, List<RequestClass>>> orderMap { get; }

    public int getPriority(string property, RequestClass request) {
		return propertyMap[property].ContainsKey(request) ? propertyMap[property][request] : (int)RequestClass.NoRequest;
	}

	public List<RequestClass> getOrder(string property, int priority) {
		return orderMap[property].ContainsKey(priority) ? orderMap[property][priority] : noOrder;
	}
}
