using BipedLocomotion;
using Unity.Kinematica;
using Unity.SnapshotDebugger;
using UnityEngine;

public class CombatAnimation : SnapshotProvider, IAnimation
{
	private Kinematica _kinematica;
	private MemoryIdentifier _root;

	public override void OnEnable()
	{
		base.OnEnable();
		_kinematica = GetComponent<Kinematica>();
		_root = MemoryIdentifier.Invalid;
	}

	public IAnimation OnUpdate(float deltaTime)
	{
		if (_root.IsValid)
		{
			ref var synthesizer = ref _kinematica.Synthesizer.Ref;
			/*ref var condition = ref synthesizer.GetChildByType<ConditionTask>(_root).Ref; //TODO need better animation 
			condition.value = Input.GetMouseButtonDown(0);*/

			//Retrieving Task goes here

			if (!Input.GetMouseButtonUp(1)) //condition
			{
				synthesizer.Tick(_root);
				return this;
			}

			_root = MemoryIdentifier.Invalid;
		}

		return null;
	}

	public bool OnChange(ref MotionSynthesizer synthesizer)
	{
		//Condition goes here
		if (!Input.GetMouseButton(1)) return false;

		{
			var action = synthesizer.Root.Selector();
			var sequence = action.Condition().Sequence();

			//sequence.Action().MatchPose(synthesizer.Query.Where(Attack.Default).Except(Idle.Default), 0.01f); // TODO need better animation

			synthesizer.BringToFront(action.GetAs<ActionTask>().self);

			_root = action.GetAs<ActionTask>().self;

			synthesizer.PlayFirstSequence(synthesizer
				.Query
				.Where(Attack.Default)
				.And(Idle.Default));
		}
		//Add Some Tasks Here
		return true;
	}

	public override void WriteToStream(Buffer buffer)
	{
	}

	public override void ReadFromStream(Buffer buffer)
	{
	}
}