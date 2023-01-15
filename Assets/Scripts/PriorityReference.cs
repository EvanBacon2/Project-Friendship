using System.Collections.Generic;

/* 
 *A context which defines the priority of a RequestClass, and the execution order
 */
public interface RequestReference {
    /*
    protected abstract Dictionary<string, Dictionary<RequestClass, int>> priorityMap { get; }
    protected abstract Dictionary<string, Dictionary<int, List<RequestClass>>> orderMap { get; }

    public int getPriority(string property, RequestClass request) {
		return priorityMap[property].ContainsKey(request) ? priorityMap[property][request] : (int)RequestClass.NoRequest;
	}

	public List<RequestClass> getOrder(string property, int priority) {
		return orderMap[property].ContainsKey(priority) ? orderMap[property][priority] : noOrder;
	}*/
}
