using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Request {
	public abstract RequestType type { get; }

	public virtual void onRequestExecuted(List<string> executedProperties) {}
}
