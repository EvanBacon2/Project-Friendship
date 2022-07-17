using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Request : MonoBehaviour {
	public abstract RequestType type { get; }
	public abstract void OnPlayerInputRecorded(object sender, PlayerInputArgs args);
	public virtual void onRequestExecuted(HashSet<string> executedProperties) {}
}
