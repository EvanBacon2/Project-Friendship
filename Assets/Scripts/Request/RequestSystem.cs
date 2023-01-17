using System;
using System.Collections.Generic;

public interface RequestSender {
	public void onRequestsExecuted(HashSet<Guid> executedRequests);
}

/*
* A class which recieves input and conditionaly makes requests based on the input
*/
public abstract class RequestSystem<T> : RequestSender where T : EventArgs { 
	public abstract void OnStateReceived(object sender, T args);
	public virtual void onRequestsExecuted(HashSet<Guid> executedRequests) {}
}


