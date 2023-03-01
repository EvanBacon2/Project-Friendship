public class PlayerRopeSimulator : RopeSimulator {
    private Anchor anchor;
    private PlayerRopeHook hook;

    public PlayerRopeSimulator(Rope rope, Anchor anchor, PlayerRopeHook hook) : base(rope) {
        this.anchor = anchor;
        this.hook = hook;
    }

    public void advance() {
        OnUpdate();
        mainLoop();
        OnUpdateLate();
    }

    protected override void OnUpdate() {
        anchor.OnUpdate();
        rope.OnUpdate();
        hook.OnUpdate();
    }

    protected override void OnSubUpdate() {
        anchor.OnSubUpdate();
        rope.OnSubUpdate();
        hook.OnSubUpdate();
    }

    protected override void ApplyConstraints() {
        rope.ApplyConstraints();
        anchor.ApplyConstraints();
        hook.ApplyConstraints();
    }

    protected override void OnUpdateLate() {
        rope.OnUpdateLate();
        anchor.OnUpdateLate();
        hook.OnUpdateLate();
    }

    protected override void applyDrag() {
        anchor.applyDrag();
        base.applyDrag();
    }
}
