using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SegmentRope;

class HookSegment : MonoBehaviour {
	public Segment s;

	private Rigidbody hookBody;
	private Vector3 hookPos = new Vector3(0,0,0);

	private GameObject hooked;//object attatched to hook
	private Vector3 hookedPos = new Vector3(0, 0, 0);
	private Vector3 hookOffset = new Vector3(0, 0, 0);

	public bool active;
	public bool isHooked;
	public bool justHooked;

	private void Start() {
		hookBody = GetComponent<Rigidbody>();
		transform.position = new Vector3((float)3, (float)4, 0);
	}

	private void FixedUpdate() {
		hookPos.x = (float)s.p2.x;
		hookPos.y = (float)s.p2.y;
		hookBody.position = hookPos;

		float angle = Mathf.Atan2((float)s.orientation.y, (float)s.orientation.x) * Mathf.Rad2Deg - 90;
		transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);

		if (hooked != null) {
			hookedPos.x = (float)s.p2.x;
			hookedPos.y = (float)s.p2.y;

			hooked.transform.position = hookedPos + hookOffset;
		}

		justHooked = false;
	}

	private void OnTriggerEnter(Collider other) {
		if (active && other.tag != "Player" && other.tag != "Wall" && other.GetComponent<Throwable>().hookable) {
			hooked = other.gameObject;
			hookOffset.x = hooked.transform.position.x - (float)s.p2.x;
			hookOffset.y = hooked.transform.position.y - (float)s.p2.y;
			s.mass = 1;
			isHooked = true;
			justHooked = true;
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
}
