using System;
using System.Collections.Generic;
using UnityEngine;

public interface RequestSender {
	public void onRequestsExecuted(HashSet<Guid> executedRequests);
}

/*
* A class which recieves a state and conditionaly makes requests based on the state
*/
public abstract class RequestSystem<T> : RequestSender where T : EventArgs { 
	public abstract void OnStateReceived(object sender, T args);
	public virtual void onRequestsExecuted(HashSet<Guid> executedRequests) {}
}


