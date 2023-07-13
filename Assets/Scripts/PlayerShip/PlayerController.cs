using System;
using UnityEngine;

/*
 * Creates state objects from player input. Sends state to RequestSystems.
 */
public class PlayerController : MonoBehaviour {
    private RECSShipbody rigidBody;

    
    private Rope rope;
    private RopeExtender extender;
    private PlayerRopeNoIn playerRope;
    private RopeTightener tightener;
    

    //private PlayerRope rope;
    private Anchor anchor;
    public PlayerRopeHook hook;

    private PlayerRopeSimulator simulator;

    private BoostManager boostManager;

    public event EventHandler<ShipState> ShipStateReceived;
    public event EventHandler<PlayerRopeState> PlayerRopeStateReceived;
    public event EventHandler<ExtendableState> ExtendableStateReceived;
    public event EventHandler<AnchorState> AnchorStateReceived;

    void Awake() {
        rope = GetComponent<Rope>();
        extender = GetComponent<RopeExtender>();
        playerRope = GetComponent<PlayerRopeNoIn>();
        tightener = GetComponent<RopeTightener>();
    }

    void Start() {
        rigidBody = GetComponent<RECSShipbody>();
        anchor = GetComponent<Anchor>();

        simulator = new PlayerRopeSimulator(rope, anchor, hook, extender, tightener, playerRope);

        boostManager = GetComponent<BoostManager>();

        ShipStateReceived += new BoostSystem().OnStateReceived;
        ShipStateReceived += new BrakeRequest().OnStateReceived;
        ShipStateReceived += new RotateToCursorSystem().OnStateReceived;
        ShipStateReceived += new MoveSystem().OnStateReceived;

        PlayerRopeStateReceived += new PlayerRopeSystem().OnStateReceived;
        ExtendableStateReceived += new ExtendableSystem().OnStateReceived;
        AnchorStateReceived += new AnchorSystem().OnStateReceived;
    }

    void FixedUpdate() {
        publishShipState();
        publishExtendableState();
        publishAnchorState();
        publishPlayerRopeState();

        simulator.advance();
        rigidBody.executeRequests();
    }

    private void publishShipState() {
        ShipStateReceived?.Invoke(this, new() { 
            time = Time.time,
            rigidbody = rigidBody,
            manager = boostManager,
            lookDirection = PlayerInputProvider.lookInput,
            horizontalMove = PlayerInputProvider.horizontalInput, 
            verticalMove = PlayerInputProvider.verticalInput,
            brake = PlayerInputProvider.brakeInput,
            boost = PlayerInputProvider.boostInput,
            isAccelerating = PlayerInputProvider.horizontalInput != 0 || PlayerInputProvider.verticalInput != 0,
        });
    }

    private void publishPlayerRopeState() {
        PlayerRopeStateReceived?.Invoke(this, new() {
            playerRope = this.playerRope,
            extender = this.extender,
            tightener = this.tightener,
            mode = PlayerInputProvider.ropeModeInput,
        });
    }

    private void publishExtendableState() {
        ExtendableStateReceived?.Invoke(this, new() {
            rope = this.rope,
            extender = this.extender,
            auto = PlayerInputProvider.ropeAutoInput,
            wind = PlayerInputProvider.ropeWindInput,
        });
    }

    private void publishAnchorState() {
        AnchorStateReceived?.Invoke(this, new() {
            anchor = this.anchor,
            angleLimit = rope.angleLimitRadians,
        });
    }
}

public class ShipState : EventArgs {
    public float time { get; set; }
    public RECSShipbody rigidbody { get; set; }
    public BoostManager manager { get; set; }
    public Vector3 lookDirection { get; set; }
    public float horizontalMove { get; set; }
    public float verticalMove { get; set; }
    public bool brake { get; set; }
    public bool boost { get; set; }
    public bool isAccelerating { get; set; }
}

public class PlayerRopeState : EventArgs {
    public PlayerRopeNoIn playerRope { get; set; }
    public RopeExtender extender { get; set; }
    public RopeTightener tightener { get; set; }
    public bool mode { get; set; }
}

public class AnchorState : EventArgs {
    public Anchor anchor { get; set; }
    public double angleLimit { get; set; }
}

public class ExtendableState : EventArgs {
    public Rope rope { get; set; }
    public RopeExtender extender { get; set; }
    public bool auto { get; set; }
    public float wind { get; set; }
}
