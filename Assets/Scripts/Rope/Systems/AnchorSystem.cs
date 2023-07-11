/*
 * Defines a system that controls an Anchor that a rope can attach to
 */
public class AnchorSystem : RequestSystem<AnchorState> {
    public override void OnStateReceived(object sender, AnchorState state) {
        //state.anchor.setAngleLimit(state.angleLimit);
    }
}
