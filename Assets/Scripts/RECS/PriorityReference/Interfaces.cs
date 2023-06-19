using System.Collections.Generic;

/*
 * Defines a relative hierarchy of priority for a set of PriorityAliases, and orders of execution among PriorityAliases 
 * with the same priority.
 */
public interface IPriorityReference {
	/*
	 * Returns the priority for the given PriorityAliases, or -1 if this reference does not define a priority
	 * for the PriorityAliases.
	 */
	public int priority(PriorityAlias request);
	/*
	 * Returns the order that PriorityAliases with the same given priority should be executed in. Returns an empty
	 * enumerator if no such order is defined.  
	 */
	public IEnumerable<PriorityAlias> order(int priority);
}