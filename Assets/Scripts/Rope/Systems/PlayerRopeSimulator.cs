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
        anchor.OnUpdate();
        tightener.OnUpdate();
        playerRope.OnUpdate();
        hook.OnUpdate();
        extender.OnUpdateLate();
    }

    protected override void OnSubUpdate() {
        anchor.OnSubUpdate();
        tightener.OnSubUpdate();
        playerRope.OnSubUpdate();
        hook.OnSubUpdate();
    }

    protected override void ApplyConstraints() {
        rope.ApplyConstraints();
        extender.ApplyConstraints();
        anchor.ApplyConstraints();
        hook.ApplyConstraints();
    }

    protected void OnUpdateLate() {
        
        anchor.OnUpdateLate();
        hook.OnUpdateLate();
    }

    protected override void applyDrag() {
        base.applyDrag();
        anchor.applyDrag();
    }
}
