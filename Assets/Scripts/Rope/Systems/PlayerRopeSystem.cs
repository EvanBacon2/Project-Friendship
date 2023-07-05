using System.Collections.Generic;
using System.Collections;
using UnityEngine;

public class PlayerRopeSystem : RequestSystem<PlayerRopeState> {
    private bool tight;

    private TightenRope tighty = new TightenRope();

    public override void OnStateReceived(object sender, PlayerRopeState state) {
        if (state.mode && state.rope.extended) {
            if (state.rope.mode == RopeMode.FLEXIBLE) {
                this.tight = true;
                tighty.execute(state);
            } else 
                state.rope.flexible();
        }

        if (tight)
            tight = tighty.execute(state);
    }    
}
