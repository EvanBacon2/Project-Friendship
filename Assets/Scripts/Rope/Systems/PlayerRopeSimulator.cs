public class PlayerRopeSimulator : RopeSimulator {
    private Anchor anchor;
    private PlayerRopeHook hook;

    private RopeExtender extender;
    private RopeTightener tightener;
    private PlayerRopeNoIn playerRope;

    public PlayerRopeSimulator(Rope rope, Anchor anchor, PlayerRopeHook hook, RopeExtender extender, 
        RopeTightener tightener, PlayerRopeNoIn playerRope) : base(rope) {
        this.anchor = anchor;
        this.hook = hook;

        this.extender = extender;
        this.tightener = tightener;
        this.playerRope = playerRope;
    }

    public void advance() {
        OnUpdate();
        mainLoop();
        OnUpdateLate();
    }

    protected void OnUpdate() {
        extender.OnUpdateLate();
        hook.OnUpdate();
        playerRope.OnUpdate();
        tightener.OnUpdate();
        anchor.OnUpdate();
    }

    protected override void OnSubUpdate() {
        hook.OnSubUpdate();
        playerRope.OnSubUpdate();
        tightener.OnSubUpdate();
        anchor.OnSubUpdate();
    }

    protected override void ApplyConstraints() {
        rope.ApplyConstraints();
        extender.ApplyConstraints();
        anchor.ApplyConstraints();
        hook.ApplyConstraints();
    }

    protected void OnUpdateLate() {
        hook.OnUpdateLate();
        anchor.OnUpdateLate();
    }

    protected override void applyDrag() {
        anchor.applyDrag();
        base.applyDrag();
    }
}
