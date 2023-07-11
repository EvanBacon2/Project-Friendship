public class PlayerRopeSimulator : RopeSimulator {
    private Anchor anchor;
    private PlayerRopeHook hook;

    private ExtendRopeNoIn extender;
    private PlayerRopeNoIn playerRope;

    public PlayerRopeSimulator(Rope rope, Anchor anchor, PlayerRopeHook hook, ExtendRopeNoIn extender, 
        PlayerRopeNoIn playerRope) : base(rope) {
        this.anchor = anchor;
        this.hook = hook;

        this.extender = extender;
        this.playerRope = playerRope;
    }

    public void advance() {
        OnUpdate();
        mainLoop();
        OnUpdateLate();
    }

    protected override void OnUpdate() {
        anchor.OnUpdate();
        rope.OnUpdate();
        //extender.OnUpdate();
        playerRope.OnUpdate();
        hook.OnUpdate();
    }

    protected override void OnSubUpdate() {
        anchor.OnSubUpdate();
        rope.OnSubUpdate();
        //extender.OnSubUpdate();
        playerRope.OnSubUpdate();
        hook.OnSubUpdate();
    }

    protected override void ApplyConstraints() {
        rope.ApplyConstraints();
        extender.ApplyConstraints();
        //playerRope.ApplyConstraints();
        anchor.ApplyConstraints();
        hook.ApplyConstraints();
    }

    protected override void OnUpdateLate() {
        rope.OnUpdateLate();
        extender.OnUpdateLate();
        //playerRope.OnUpdateLate();
        anchor.OnUpdateLate();
        hook.OnUpdateLate();
    }

    protected override void applyDrag() {
        anchor.applyDrag();
        base.applyDrag();
    }
}
