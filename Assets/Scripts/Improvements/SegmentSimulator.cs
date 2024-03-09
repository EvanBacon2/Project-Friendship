using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SegmentSimulator : MonoBehaviour {
	private ResponsibilityLink<SegmentGroupCollection, SegmentGroupCollection> _constraintChain;
	
	public int substeps;
	public double h;

	public EventHandler Step;
	public EventHandler SubStep;
	public EventHandler LateStep;

	public EventHandler<SimulationState> Simulate;
	public EventHandler<SimulationState> AdjustVelocities;
	public EventHandler<SimulationState> SolveVelocities;
	public EventHandler<SimulationState> Drag;
	public EventHandler<InterGroupDragState> InterGroupDrag;

	

	private void _publishSimulate() {
		Simulate.Invoke(this, new() {
			handle = (SegmentGroup group) => { _simulate(group); }
		});
	}

	private void _publishAdjustVelocities() {
		AdjustVelocities.Invoke(this, new() {
			handle = (SegmentGroup group) => { _adjustVelocities(group); }
		});
	}

	private void _publishSolveVelocities() {
		SolveVelocities.Invoke(this, new() {
			handle = (SegmentGroup group) => { _solveVelocities(group); }
		});
	}

	private void _publishDrag() {
		Drag.Invoke(this, new() {
			handle = (SegmentGroup group) => { _applyDrag(group); }
		});
	}

	private void _publishInterGroupDrag() {
		InterGroupDrag.Invoke(this, new() {
			handle = (SegmentGroup group1, SegmentGroup group2, int index1, int index2) => {
				_applyInterGroupDrag(group1, group2, index1, index2);
			}
		});
	}

	void Awake() {
		this._constraintChain = new SegmentConstraintChain();
	}

	public void advance(SegmentGroupCollection inState, SegmentGroupCollection outState) {
		Step.Invoke(this, new());
		for (int i = 0; i < substeps; i++) {
			SubStep.Invoke(this, new());
			_publishSimulate();
			_constraintChain.execute(inState, outState);
			_publishAdjustVelocities();
			_publishSolveVelocities();
			_publishDrag();
			_publishInterGroupDrag();
		}
		LateStep.Invoke(this, new());
	}

	private void _simulate(SegmentGroup group) {
		for (int i = 0; i < group.Count; i++) {
			if (!group.Active(i))
					continue;

			group.PreviousPosition(i).x = group.Position(i).x;
			group.PreviousPosition(i).y = group.Position(i).y;
			group.setPosition(i, group.Position(i).x + h * group.Velocity(i).x, 
								 group.Position(i).y + h * group.Velocity(i).y);

			group.PreviousOrientation(i).x = group.Orientation(i).x;
			group.PreviousOrientation(i).y = group.Orientation(i).y;
			group.Rotate(i, group.AngularVelocity(i) * h);
		}
	}

	private void _adjustVelocities(SegmentGroup group) {
		for (int i = 0; i < group.Count; i++) {
			if (!group.Active(i))
					continue;

			group.Velocity(i).x = (group.Position(i).x - group.PreviousPosition(i).x) / h;
			group.Velocity(i).y = (group.Position(i).y - group.PreviousPosition(i).y) / h;
			group.SetAngularVelocity(i, Vector2d.SignedAngle(group.PreviousOrientation(i), group.Orientation(i)) / h);
		}
	}

	private void _solveVelocities(SegmentGroup group) {
		_clampMags(group);
		_applyDrag(group);
	}

	private void _clampMags(SegmentGroup group) {
		if (!group.SpeedClamped)
			return;

		for (int i = 0; i < group.Count; i++) {
			if (!group.Active(i))
				continue;

			double mag = group.Velocity(i).magnitude;
			double clampedMag = System.Math.Min(mag, group.MaxSpeedBase * System.Math.Pow(group.ActiveSegments - i, group.MaxSpeedScale));

			if (mag > 0) {
				group.Velocity(i).x = group.Velocity(i).x / mag * clampedMag;
				group.Velocity(i).y = group.Velocity(i).y / mag * clampedMag;
			}
		}
	}

	protected Vector2d linearDiff = Vector2d.zero;
	protected double angulerDiff = 0;
	private void _applyDrag(SegmentGroup group) {
		for (int i = 0; i < group.Count; i++) {
			if (!group.Active(i))
					continue;

			int s1 = i;
			int s2 = i - 1;

			double linearRatio = group.InverseMass(s1) / (group.InverseMass(s1) + group.InverseMass(s2));
			double angulerRatio = group.InverseInertia(s1) / (group.InverseInertia(s1) + group.InverseInertia(s2));

			linearDiff.x = (group.Velocity(s2).x - group.Velocity(s1).x) * group.SubLinearDrag;
			linearDiff.y = (group.Velocity(s2).y - group.Velocity(s1).y) * group.SubLinearDrag;
			angulerDiff = (group.AngularVelocity(s2) - group.AngularVelocity(s1)) * group.SubAngularDrag;
				
			group.Velocity(s1).x += linearDiff.x * linearRatio;
			group.Velocity(s1).y += linearDiff.y * linearRatio;
			group.SetAngularVelocity(s1, group.AngularVelocity(s1) + angulerDiff * angulerRatio);

			group.Velocity(s2).x -= linearDiff.x * (1 - linearRatio);
			group.Velocity(s2).y -= linearDiff.y * (1 - linearRatio);
			group.SetAngularVelocity(s2, group.AngularVelocity(s2) - angulerDiff * (1 - angulerRatio));
		}
	}

	private void _applyInterGroupDrag(SegmentGroup group1, SegmentGroup group2, int index1, int index2) {
		if (!group1.Active(index1) || !group2.Active(index2))
			return;

		double linearRatio = group1.InverseMass(index1) / (group1.InverseMass(index1) + group2.InverseMass(index2));
		double angulerRatio = group1.InverseInertia(index1) / (group1.InverseInertia(index1) + group2.InverseInertia(index2));

		linearDiff.x = (group2.Velocity(index2).x - group1.Velocity(index1).x) * group1.SubLinearDrag;
		linearDiff.y = (group2.Velocity(index2).y - group1.Velocity(index1).y) * group1.SubLinearDrag;
		angulerDiff = (group2.AngularVelocity(index2) - group1.AngularVelocity(index1)) * group1.SubAngularDrag;
			
		group1.Velocity(index1).x += linearDiff.x * linearRatio;
		group1.Velocity(index1).y += linearDiff.y * linearRatio;
		group1.SetAngularVelocity(index1, group1.AngularVelocity(index1) + angulerDiff * angulerRatio);

		group2.Velocity(index2).x -= linearDiff.x * (1 - linearRatio);
		group2.Velocity(index2).y -= linearDiff.y * (1 - linearRatio);
		group2.SetAngularVelocity(index2, group2.AngularVelocity(index2) - angulerDiff * (1 - angulerRatio));
	}
}

public class SimulationState : EventArgs {
	public Action<SegmentGroup> handle { get; set; }
}

public class InterGroupDragState : EventArgs {
	public Action<SegmentGroup, SegmentGroup, int, int> handle { get; set; }
}


