using System.Collections.Generic;
using UnityEngine;

//A Request System is a class which recieves input and conditionaly makes requests based on the input
public abstract class RequestSystem : MonoBehaviour {
	public abstract void OnPlayerInputRecorded(object sender, PlayerInputArgs args);
	public virtual void onRequestExecuted(HashSet<string> executedProperties) {}
}
