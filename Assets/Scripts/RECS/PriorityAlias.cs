/*
 *  PriorityAliases create a layer of abstraction which allow for requests to have different priorities
 *  depending on the specific variable they are requesting on without the sender of the request needing to
 *  specify different priorities.  They can simply send the request with the same alias, and the reciever
 *  of the request can the actual relative priority for that specific variable.
 */
public enum PriorityAlias {
	NoRequest = -1,
	Move,
	Brake,
	Boost,
	BoostReset,
	LookAtMouse,
	Rope,
}
