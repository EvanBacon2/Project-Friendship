using System;
using System.Collections.Generic;
using UnityEngine;

public class ExtendableRope : Rope {
	public bool extended { get { return activeSegments != 0; } }

	protected bool _autoExtend = false;
	public bool autoExtend {
		get { return _autoExtend; }
		set {
            if (_autoExtend != value) {
                _autoExtend = value;

                if (_autoExtend)
                    on(autoExtendStart);
                else
                    on(autoExtendEnd);
            }
        }
	}

	protected bool _autoRetract = false;
	public bool autoRetract {
		get { return _autoRetract; }
		set { 
            if (_autoRetract != value) {
                _autoRetract = value;
            
                if (_autoRetract)
                    on(autoRetractStart);
                else
                    on(autoRetractEnd);
            }
        }
	}

	protected double _winchOffset;
	public double winchOffset {
		get { return _winchOffset; }
		set {
			if (uniformSegments)
				adjustWinch(value, segments[0].length);
			else
				adjustWinch(value);
		}
	}

    public double winchScrollBuffer { get; set; }//Number of frames that wind input will be applied

    public double autoExtendRate;//segments/fixed time step the rope is auto extended by
    public double autoRetractRate;//segments/fixed time step the rope is auto retracted by
    public double winchUnit;//length the rope is extended/retracted by per wind input
    public int winchFrames;//number of frames it takes to wind/unwind the rope by one winchUnit

    private Vector2d inactivePosition = Vector2d.zero;
    private Vector2d inactiveOrientation = Vector2d.zero;

    private List<Action> autoExtendStart = new List<Action>();
    private List<Action> autoExtendEnd = new List<Action>();
    private List<Action> autoRetractStart = new List<Action>();
    private List<Action> autoRetractEnd = new List<Action>();

    public override void OnUpdateLate() {
        base.OnUpdateLate();

        //apply scroll wind
        if (winchScrollBuffer != 0) {
            winchOffset += winchUnit / winchFrames * System.Math.Sign(winchScrollBuffer);
            winchScrollBuffer -= winchScrollBuffer > 0 ? 1 : -1;
        }
        
        //apply auto extention
        if (autoExtend) {
            winchOffset += autoExtendRate;
            if (activeSegments == segments.Length)//stop extending 
                autoExtend = false;
        }
        
        //apply auto retraction
        if (autoRetract) {
            winchOffset -= autoRetractRate;
            if (!extended)//stop retracting
                autoRetract = false;
        }

        //anchor.setOffset(0, winchOffset);//move to system
    }

    public override void ApplyConstraints() {
        base.ApplyConstraints();
        for (int i = activeSegments; i < segments.Length; i++) {
            inactiveConstraint(segments[i]);
        }
    }

    protected void inactiveConstraint(Segment segment) {
        segment.setP1(inactivePosition.x, inactivePosition.y);
		segment.setOrientation(inactiveOrientation.x, inactiveOrientation.y);
		segment.previousPosition.x = segment.position.x;
		segment.previousPosition.y = segment.position.y;
	    segment.velocity.x = 0;
		segment.velocity.y = 0;
		segment.angulerVelocity = 0;
    }

    public void setInactivePosition(double x, double y) {
        inactivePosition.x = x;
        inactivePosition.y = y;
    }

    public void setInactiveOrientation(double x, double y) {
        inactiveOrientation.x = x;
        inactiveOrientation.y = y;
    }

    public void addAutoExtendStartCallback(Action callback) {
        autoExtendStart.Add(callback);
    }

    public void addAutoExtendEndCallback(Action callback) {
        autoExtendEnd.Add(callback);
    }

    public void addAutoRetractStartCallback(Action callback) {
        autoRetractStart.Add(callback);
    }

    public void addAutoRetractEndCallback(Action callback) {
        autoRetractEnd.Add(callback);
    }

    private void on(List<Action> callbacks) {
        callbacks.ForEach((callback) => callback());
    } 

	/*
     * Winds and unwinds rope based on adjustment
     *
     * Assumes all segments are of the given length
     */
    protected void adjustWinch(double adjustment, double length) {
        _winchOffset = adjustment;
        if (_winchOffset >= length || _winchOffset < 0) {
            int segmentChange = (int)(_winchOffset / length);
            if (_winchOffset < 0)  {
                segmentChange -= 1;
                _winchOffset = baseSegment > -1 ? length + _winchOffset : 0;
            }
            
            baseSegment = System.Math.Clamp(baseSegment + segmentChange, -1, segments.Length - 1);
            activeSegments = baseSegment + 1;

            _winchOffset = _winchOffset % length;
        }      
    }

	 /*
     * Winds and unwinds rope based on adjustment
     */
    protected void adjustWinch(double adjustment) {
        _winchOffset += adjustment;
        int sign = _winchOffset < 0 ? -1 : 1;

        double baseLength = baseSegment > -1 ? segments[baseSegment].length : 0;

        while (System.Math.Abs(_winchOffset) >= baseLength) {
            _winchOffset -= baseLength * sign;
            baseSegment = System.Math.Clamp(baseSegment + sign, -1, segments.Length - 1);	
            baseLength = baseSegment != -1 ? segments[baseSegment].length : double.PositiveInfinity;
        }
        
        if (_winchOffset < 0) {
            _winchOffset = baseSegment != -1 ? baseLength + _winchOffset : 0;
            baseSegment = System.Math.Clamp(baseSegment - 1, -1, segments.Length - 1);
        }

        activeSegments = baseSegment + 1;
    }
}
