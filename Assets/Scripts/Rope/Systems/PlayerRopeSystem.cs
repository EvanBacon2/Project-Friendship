using UnityEngine;

public class PlayerRopeSystem : RequestSystem<PlayerRopeState> {
    public override void OnStateReceived(object sender, PlayerRopeState state) {
        if (state.mode && state.rope.extended) {
            if (state.rope.mode == RopeMode.FLEXIBLE) 
                state.rope.stiff();
            else 
                state.rope.flexible();
        }
    }
}
