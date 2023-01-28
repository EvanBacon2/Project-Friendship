using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * Defines a rope that can be extended and retracted.
 */
public abstract class ExtendableRope : RopeSimulator {
    protected double sUnit;//Length of the segments in this rope

    private bool extended = false;
	private bool autoExtend = false;
	private bool autoRetract = false;

    public double winchForce;
	protected double winchOffset;
	protected double winchScrollBuffer;
	protected double winchBrakeBuffer;

    private void Update() {
        //auto extend rope
        if (Input.GetMouseButtonDown(1) && !extended) 
            setAutoExtend(true);

        //auto retract rope
        if (Input.GetMouseButtonDown(1) && extended) 
            setAutoRetract(true);

        //wind/unwind rope 
        if (extended && Input.mouseScrollDelta.y != 0 && winchScrollBuffer == 0 && (activeSegments < rope.Length || Input.mouseScrollDelta.y < 0))
            winchScrollBuffer = 4 * System.Math.Sign(Input.mouseScrollDelta.y);
    }

    private void FixedUpdate() {
        //apply auto extention
        if (autoExtend) {
            adjustWinch(winchForce);
            if (activeSegments == rope.Length)//stop extending
                setAutoExtend(false);
        }

        //apply auto retraction
        if (autoRetract) {
            adjustWinch(-sUnit);
            if (!extended)//stop retracting
                setAutoRetract(false);
        }
    }

    /*
     * Winds and unwinds rope based on adjustment
     */
    protected void adjustWinch(double adjustment) {
        winchOffset += adjustment;
        if (winchOffset >= sUnit || winchOffset < 0) {
            int segmentChange = (int)(winchOffset / sUnit);
            if (winchOffset < 0) segmentChange -= 1;

            activeSegments = System.Math.Clamp(activeSegments + segmentChange, 0, rope.Length);
            baseSegment = System.Math.Clamp(baseSegment + segmentChange, 0, rope.Length - 1);
            if (winchOffset < 0) 
                winchOffset = activeSegments > 0 ? sUnit + winchOffset : 0;	

            winchOffset = winchOffset % sUnit;
        }

        extended = activeSegments != 0;
    }

    protected void setAutoExtend(bool state) {
        if (autoExtend != state) {
            autoExtend = state;
            if (autoExtend)
                onAutoExtendStart();
            else
                onAutoExtendEnd();
        }
    }

    protected void setAutoRetract(bool state) {
        if (autoExtend != state) {
            autoExtend = state;
            if (autoExtend)
                onAutoExtendStart();
            else
                onAutoExtendEnd();
        }
    }

    protected virtual void onAutoExtendStart() {
        
    }

    protected virtual void onAutoExtendEnd() {

    }

    protected virtual void onAutoRetract() {

    }

    protected virtual void onAutoRetractEnd() {

    }
}
