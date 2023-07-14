using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rope))]
public class RopeExtender : MonoBehaviour, RopeBehaviour {
    private Rope rope;

	public bool extended { get { return rope.activeSegments != 0; } }

	protected bool _autoExtend = false;
	public bool autoExtend {
		get { return _autoExtend; }
		set {
            if (_autoExtend != value) {
                _autoExtend = value;

                if (_autoExtend) 
                    autoExtendStartEvent.Invoke(this, new());
                else
                    autoExtendEndEvent.Invoke(this, new());
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
                    autoRetractStartEvent.Invoke(this, new());
                else
                    autoRetractEndEvent.Invoke(this, new());
            }
        }
	}

	protected double _winchOffset;
	public double winchOffset {
		get { return _winchOffset; }
		set {
			if (rope.uniformSegments)
				adjustWinch(value, rope.segments[0].length);
			else
				adjustWinch(value);
		}
	}

    public double winchScrollBuffer { get; set; }//Number of frames that wind input will be applied

    public double autoExtendRate;//segments/fixed time step the rope is auto extended by
    public double autoRetractRate;//segments/fixed time step the rope is auto retracted by
    public double winchUnit;//length the rope is extended/retracted by per wind input
    public int winchFrames;//number of frames it takes to wind/unwind the rope by one winchUnit
    public double baseExtention { get { return winchOffset / _baseLength; } }//value between [1,0) indicating how extended the base segment is
    private double _baseLength { get { return rope.baseSegment > -1 ? rope.segments[rope.baseSegment].length : Double.PositiveInfinity; } }

    private Vector2d inactivePosition = Vector2d.zero;//position of all inactive segments
    private Vector2d inactiveOrientation = Vector2d.zero;//orientation of all inactive segments

    private List<Action> autoExtendStart = new List<Action>();
    private List<Action> autoExtendEnd = new List<Action>();
    private List<Action> autoRetractStart = new List<Action>();
    private List<Action> autoRetractEnd = new List<Action>();

    public EventHandler autoExtendStartEvent;
    public EventHandler autoExtendEndEvent;
    public EventHandler autoRetractStartEvent;
    public EventHandler autoRetractEndEvent;

    private void Awake() {
        rope = GetComponent<Rope>();
    }

    public void OnUpdateLate() {
        //apply scroll wind
        if (winchScrollBuffer != 0) {
            winchOffset += winchUnit / winchFrames * System.Math.Sign(winchScrollBuffer);
            winchScrollBuffer -= winchScrollBuffer > 0 ? 1 : -1;
        }
        
        //apply auto extention
        if (autoExtend) {
            winchOffset += autoExtendRate;
            if (rope.activeSegments == rope.segments.Length)//stop extending 
                autoExtend = false;
        }
        
        //apply auto retraction
        if (autoRetract) {
            winchOffset -= autoRetractRate;
            if (!extended)//stop retracting
                autoRetract = false;
        }
    }

    public void ApplyConstraints() {
        for (int i = rope.activeSegments; i < rope.segments.Length; i++) {
            inactiveConstraint(rope.segments[i]);
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

	/*
     * Winds and unwinds rope based on adjustment
     *
     * Assumes all segments are of the given length
     */
    protected void adjustWinch(double adjustment, double length) {
        _winchOffset = adjustment;
        if (_winchOffset >= length || _winchOffset < 0) {
            int segmentChange = (int)(_winchOffset / length);
            while (_winchOffset < 0)  {
                segmentChange -= 1;
                _winchOffset = rope.baseSegment > -1 ? length + _winchOffset : 0;
            }
            
            rope.baseSegment = System.Math.Clamp(rope.baseSegment + segmentChange, -1, rope.segments.Length - 1);
            rope.activeSegments = rope.baseSegment + 1;

            _winchOffset = _winchOffset % length;
        }      
    }

	/*
     * Winds and unwinds rope based on adjustment
     */
    protected void adjustWinch(double adjustment) {
        _winchOffset += adjustment;
        int sign = _winchOffset < 0 ? -1 : 1;

        double baseLength = rope.baseSegment > -1 ? rope.segments[rope.baseSegment].length : 0;

        while (System.Math.Abs(_winchOffset) >= baseLength) {
            _winchOffset -= baseLength * sign;
            rope.baseSegment = System.Math.Clamp(rope.baseSegment + sign, -1, rope.segments.Length - 1);	
            baseLength = rope.baseSegment != -1 ? rope.segments[rope.baseSegment].length : double.PositiveInfinity;
        }
        
        if (_winchOffset < 0) {
            _winchOffset = rope.baseSegment != -1 ? baseLength + _winchOffset : 0;
            rope.baseSegment = System.Math.Clamp(rope.baseSegment - 1, -1, rope.segments.Length - 1);
        }

        rope.activeSegments = rope.baseSegment + 1;
    }
}
