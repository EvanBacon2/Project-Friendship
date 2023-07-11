using System.Collections.Generic;
using System.Collections;
using UnityEngine;

public class PlayerRopeSystem : RequestSystem<PlayerRopeState> {
    public override void OnStateReceived(object sender, PlayerRopeState state) {//playerRope
        if (state.mode && state.rope.extended) {
            if (state.rope.mode == RopeMode.FLEXIBLE) {
                state.rope.tighten = true;
            } else {
                state.rope.flexible();
            }
        }
    }    
}
