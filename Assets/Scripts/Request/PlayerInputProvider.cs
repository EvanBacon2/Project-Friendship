using UnityEngine;

public class PlayerInputProvider : MonoBehaviour {
    private static Vector3 _lookInput;
    private static float _horizontalInput;
    private static float _verticalInput;
    private static bool _brakeInput;
    private static bool _boostInput;
    private static bool _ropeModeInput;
    private static bool _ropeAutoInput;
    private static float _ropeWindInput;

    public static Vector3 lookInput { get { return _lookInput; } }
    public static float horizontalInput { get { return _horizontalInput; } }
    public static float verticalInput { get { return _verticalInput; } }
    public static bool brakeInput { get { return _brakeInput; } }
    public static bool boostInput { get { return _boostInput; } }
    public static bool ropeModeInput { get { return _ropeModeInput; } }
    public static bool ropeAutoInput { get { return _ropeAutoInput; } }
    public static float ropeWindInput { get { return _ropeWindInput; } }

    void Update() {
        _lookInput = Input.mousePosition;
        _horizontalInput = Input.GetAxisRaw("Horizontal");
        _verticalInput = Input.GetAxisRaw("Vertical");
        _brakeInput = Input.GetKey(KeyCode.LeftShift);
        _boostInput = !_boostInput ? Input.GetKeyDown(KeyCode.Space) : _boostInput;
        _ropeModeInput = !_ropeModeInput ? Input.GetKeyDown(KeyCode.F) : _ropeModeInput;
        _ropeAutoInput = !_ropeAutoInput ? Input.GetMouseButtonDown(1) : _ropeAutoInput;
        _ropeWindInput = _ropeWindInput == 0 ? Input.mouseScrollDelta.y : _ropeWindInput;
    }

    void FixedUpdate() {
        _boostInput = false;
        _ropeModeInput = false;
        _ropeAutoInput = false;
        _ropeWindInput = 0;
    }
}
