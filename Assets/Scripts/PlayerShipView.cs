using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerShipView : MonoBehaviour {
    PlayerShipModel shipModel;
    PlayerShipController shipController;

    void Start() {
        shipModel = GetComponent<PlayerShipModel>();
        shipController = GetComponent<PlayerShipController>();
    }

    void Update() {
        shipModel.horizontalInput = Input.GetAxisRaw("Horizontal");
        shipModel.verticalInput = Input.GetAxisRaw("Vertical");
        shipModel.brakeInput = Input.GetKey(KeyCode.LeftShift);
        shipModel.boostInput = Input.GetKeyDown(KeyCode.Space);

        shipController.notifyListeners();
    }
}
