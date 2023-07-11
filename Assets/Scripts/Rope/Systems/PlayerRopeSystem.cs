using System.Collections.Generic;
using System.Collections;
using UnityEngine;

public class PlayerRopeSystem : RequestSystem<PlayerRopeState> {
    public override void OnStateReceived(object sender, PlayerRopeState state) {//playerRope
        if (state.mode && state.extender.extended) {
            if (state.playerRope.mode == RopeMode.FLEXIBLE) {
                state.playerRope.tighten = true;
            } else {
                state.playerRope.flexible();
            }
        }
    }    
}
