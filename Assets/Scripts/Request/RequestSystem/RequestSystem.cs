using System;
using System.Collections.Generic;

public interface RequestSender {
	public void onRequestsExecuted(HashSet<Guid> executedRequests);
}

/*
* A class which recieves input and conditionaly makes requests based on the input
*/
public interface RequestSystem<T> : RequestSender where T : EventArgs { 
	public void OnStateReceived(object sender, T args);
}


