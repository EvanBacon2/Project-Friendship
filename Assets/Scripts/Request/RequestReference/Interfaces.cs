using System.Collections.Generic;

/*
 * Defines a relative hierarchy of priority for a set of RequestClasses and orders of execution among RequestClasses 
 * with the same priority.
 */
public interface IRequestReference {
	/*
	 * Returns the priority for the given RequestClass, or -1 if this reference does not define a priority
	 * for the RequestClass.
	 */
	public int priority(PriorityAlias request);
	/*
	 * Returns the order that RequestClasses with the same given priority should be executed in. Returns an empty
	 * enumerator if no such order is defined.  
	 */
	public IEnumerable<PriorityAlias> order(int priority);
}