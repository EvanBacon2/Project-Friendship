using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class ThrowOffset : MonoBehaviour {
    public GameObject player;
    public static GameObject tracked;

    private CinemachineFramingTransposer cam;
    private Vector2 playerViewport;
    private Vector2 trackedViewport;

	private void Start() {
        cam = GetComponent<CinemachineVirtualCamera>().GetCinemachineComponent<CinemachineFramingTransposer>();
        cam.m_ScreenX = .5f;
        cam.m_ScreenY = .5f;
    }

    void Update() {
        playerViewport = Camera.main.WorldToViewportPoint(player.transform.position);

        if (tracked != null) {
            Vector2 trackedVelocity = tracked.GetComponent<Rigidbody>().velocity.normalized * 1;
            Vector2 offset = Camera.main.WorldToViewportPoint(new Vector2(player.transform.position.x - trackedVelocity.x, player.transform.position.y + trackedVelocity.y));
            Debug.Log(trackedVelocity);

            cam.m_ScreenX = offset.x;
            cam.m_ScreenY = offset.y;

            tracked = null;
        }

        if (Mathf.Clamp01(playerViewport.x) != playerViewport.x || Mathf.Clamp01(playerViewport.y) != playerViewport.y)
            tracked = null;

        if (tracked == null) {
            //cam.m_ScreenX = .5f;
            //cam.m_ScreenY = .5f;
        }
    }
}

//record viewport pos of tracked object
//if values outside the range [0,1] than find the difference and add it to cam's screen x/y
//if screen x/y exceeds [0,1] range than stop following tracked and reset offset to .5, .5
