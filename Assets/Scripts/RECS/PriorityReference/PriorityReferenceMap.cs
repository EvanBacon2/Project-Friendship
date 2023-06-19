using System.Collections.Generic;

/*
 *  A PriorityReference which uses key value maps to represent its priority heiarchy and order.
 */
public class PriorityReferenceMap : IPriorityReference {
	private List<PriorityAlias> noOrder = new List<PriorityAlias>();
	private Dictionary<PriorityAlias, int> _priority;
	private Dictionary<int, List<PriorityAlias>> _order;

	public PriorityReferenceMap(Dictionary<PriorityAlias, int> priority, Dictionary<int, List<PriorityAlias>> order) {
		this._priority = priority;
		this._order = order;
	} 

	public int priority(PriorityAlias request) {
		return _priority.ContainsKey(request) ? _priority[request] : (int)PriorityAlias.NoRequest;
	}
	
	public IEnumerable<PriorityAlias> order(int priority) {
		return _order.ContainsKey(priority) ? _order[priority] : noOrder;
	}
}
