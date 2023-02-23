using System;
using UnityEngine;
using SegmentRope;

class HookSegment : MonoBehaviour {
	public Segment s;

	//private Rigidbody hookBody;
	//private Vector3 hookPos = new Vector3(0, 0, 0);

	private GameObject hooked;//object attatched to hook
	//private Vector3 hookedPos = new Vector3(0, 0, 0);
	private Vector3 hookOffset = new Vector3(0, 0, 0);

	public bool active;
	public bool isHooked;
	//public bool justHooked;
	public Action hookedCallback = () => {};

	private void Start() {
		//hookBody = GetComponent<Rigidbody>();
		//transform.position = new Vector3(3, 4, 0);
	}

	void FixedUpdate() {
		//hookPos.x = (float)s.p2.x;
		//hookPos.y = (float)s.p2.y;
		//hookBody.position = hookPos;
		//transform.position = hookPos;
		transform.position.Set((float)s.p2.x, (float)s.p2.y, transform.position.z);

		float angle = Mathf.Atan2((float)s.orientation.y, (float)s.orientation.x) * Mathf.Rad2Deg - 90;
		transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);

		if (hooked != null) {
			//hookedPos.x = (float)s.p2.x;
			//hookedPos.y = (float)s.p2.y;
			hooked.transform.position.Set((float)s.p2.x + hookOffset.x, (float)s.p2.y + hookOffset.y, hookOffset.z);
		}

		//justHooked = false;
	}

	void OnTriggerEnter(Collider other) {
		if (active && other.tag != "Player" && other.tag != "Wall" && other.GetComponent<Throwable>().hookable) {
			hooked = other.gameObject;
			hookOffset.x = hooked.transform.position.x - (float)s.p2.x;
			hookOffset.y = hooked.transform.position.y - (float)s.p2.y;
			//s.inverseMass = 1;
			isHooked = true;
			//justHooked = true;

			hookedCallback();
		}
	}

	public void unHook() {
		if (hooked != null) {
			hooked.GetComponent<Rigidbody>().velocity = new Vector3((float)s.velocity.x, (float)s.velocity.y, 0);
			hooked.GetComponent<Throwable>().unHook();
			ThrowOffset.tracked = hooked;
		}
		hooked = null;
		isHooked = false;
	}

	public void setHookedCallback(Action callback) {
		hookedCallback = callback;
	}
}
