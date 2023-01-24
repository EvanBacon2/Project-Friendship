using System.Collections.Generic;

public class RequestReferenceMap : IRequestReference {
	private List<RequestClass> noOrder = new List<RequestClass>();
	private Dictionary<RequestClass, int> _priority;
	private Dictionary<int, List<RequestClass>> _order;

	public RequestReferenceMap(Dictionary<RequestClass, int> priority, Dictionary<int, List<RequestClass>> order) {
		this._priority = priority;
		this._order = order;
	} 

	public int priority(RequestClass request) {
		return _priority.ContainsKey(request) ? _priority[request] : (int)RequestClass.NoRequest;
	}
	
	public IEnumerable<RequestClass> order(int priority) {
		return _order.ContainsKey(priority) ? _order[priority] : noOrder;
	}
}
