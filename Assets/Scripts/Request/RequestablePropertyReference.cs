using System.Collections.Generic;

/*
 * Defines priorities for RequestClasses, and orders of execution for priorities.
 *
 * When a property recieves a request, it will look at its reference to determine the priority of the request.
 * When a property executes its requests, it will look at its reference to determine what order to execute its requests
 * in.
 */
public class RequestablePropertyReference {
	private List<RequestClass> noOrder = new List<RequestClass>();
	private Dictionary<RequestClass, int> priority;
	private Dictionary<int, List<RequestClass>> order;

	public RequestablePropertyReference(Dictionary<RequestClass, int> priority, Dictionary<int, List<RequestClass>> order) {
		this.priority = priority;
		this.order = order;
	} 

	/*
	 * Return the priority for the given RequestClass, or return -1 if this reference does not define a priority
	 * for the RequestClass.
	 */
	public int getPriority(RequestClass request) {
		return priority.ContainsKey(request) ? priority[request] : (int)RequestClass.NoRequest;
	}

	/*
	 * Returns the order that RequestClasses with the same given priority should be executed in. Returns an empty
	 * enumerator if no such order is defined.  
	 */
	public IEnumerable<RequestClass> getOrder(int priority) {
		return order.ContainsKey(priority) ? order[priority] : noOrder;
	}
}
