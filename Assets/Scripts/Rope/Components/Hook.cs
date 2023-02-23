using System;
using System.Collections.Generic;
using UnityEngine;

public class Hook : MonoBehaviour {
	protected GameObject hooked;

	protected Vector3 hookOffset;
	protected Vector3 hookVelocity = Vector3.zero;
	protected float hookMass;

	public bool active;
	public bool isHooked { get; protected set; }

	private List<Action> hookedCallbacks = new List<Action>();

	void Start() {
		start();
	}

	void OnTriggerEnter(Collider other) {
		hook(other);
		Debug.Log("hook");
	}

	public void hook(Collider other) {
		if (active && other.tag != "Player" && other.tag != "Wall" && other.GetComponent<Throwable>().hookable) {
			hooked = other.gameObject;
			hookOffset.x = hooked.transform.position.x - transform.position.x;
			hookOffset.y = hooked.transform.position.y - transform.position.y;
			hookMass = other.attachedRigidbody.mass;
			isHooked = true;

			foreach (Action callback in hookedCallbacks) {
				callback();
			}
		}
	}

	public void unHook() {
		if (hooked != null) {
			hooked.GetComponent<Rigidbody>().velocity = new Vector3(hookVelocity.x, hookVelocity.y, 0);
			hooked.GetComponent<Throwable>().unHook();
			ThrowOffset.tracked = hooked;
		}
		hooked = null;
		isHooked = false;
		hookMass = 1;
	}

	public void addHookedCallback(Action callback) {
		hookedCallbacks.Add(callback);
	}

	protected virtual void start() {}
}
