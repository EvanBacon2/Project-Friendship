public class PlayerRopeSimulator : RopeSimulator {
    private Anchor anchor;
    private PlayerRopeHook hook;

    /*public void OnStart(object sender, PlayerRopeState state) {
        hook.addHookedCallback(() => {
            rope.autoExtend = false;
        });
    }*/

    /*protected override void snapshotState(PlayerRopeState state) {
        this.state = state;
        rope = state.rope;

        //extender.OnStateReceived(this, state.extenderState);
        //anchor.OnStateReceived(this, state.anchorState);
        //hook.OnStateReceived(this, state.hookState);
    }*/

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
