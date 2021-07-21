using BipedLocomotion;
using JetBrains.Annotations;
using Unity.Kinematica;
using Unity.Mathematics;
using Unity.SnapshotDebugger;
using UnityEngine;

public sealed class LocomotionAnimation : SnapshotProvider, IAnimation
{
	[Header("Predication Settings")] [SerializeField]
	private float responsiveness = 0.45f;

	[SerializeField] private float velocityFactor = 1.0f;

	[SerializeField] private float rotationFactor = 1.0f;

	[SerializeField] private float desiredSlowSpeed = 2.2f;

	[SerializeField] private float desiredFastSpeed = 4.45f;

	[SerializeField] private KeyCode sprintKey = KeyCode.LeftShift;

	private readonly float _brakeSpeed = 0.4f;

	private float _desiredSpeed;
	private Unity.Kinematica.Identifier<TrajectoryHeuristicTask> _heuristic;

	private float _horizontal;
	private bool _isBraking;
	private Kinematica _kinematica;
	private Unity.Kinematica.Identifier<SelectorTask> _locomotion;
	private float3 _moveDirection = Missing.forward;

	private int _previousFrameCount;
	private float _vertical;

	public override void OnEnable()
	{
		base.OnEnable();

		_kinematica = gameObject.GetComponent<Kinematica>();

		_previousFrameCount = -1;
		_desiredSpeed = desiredSlowSpeed;
	}

	[NotNull]
	public IAnimation OnUpdate(float deltaTime)
	{
		ref var synthesizer = ref _kinematica.Synthesizer.Ref;

		if (_previousFrameCount != Time.frameCount - 1 || !synthesizer.IsIdentifierValid(_locomotion))
		{
			//First animation sequence we play will be an animation that contains both Locomotion tag and Idle tag
			synthesizer
				.PlayFirstSequence(synthesizer
					.Query
					.Where(Locomotion.Default)
					.And(Idle.Default));

			//We create a selector task that connect to the current task (root node). It will select animations.
			var selector = synthesizer.Root.Selector();
			{
				//We create a Sequence task that will play animation in sequence which will connect to a Condition task which
				//will play after a condition which will be connected to the selector task.
				var sequence = selector
					.Condition()
					.Sequence();

				//We create an animation action to play an animation which will be connect to sequence
				//the animation action will play an animation that contains both Locomotion tag and idle tag
				//remember that the condition task must be true to play the sequence node and then this task.
				sequence
					.Action()
					.MatchPose(synthesizer.Query
						.Where(Locomotion.Default)
						.And(Idle.Default), 0.1f);

				//We create an animation action to wait for the timer which will wait indefinitely, since no duration is passed
				sequence
					.Action()
					.Timer();
			}

			{
				//We create an action task that will be connected to the selector task
				var action = selector.Action();

				//We append to the action that it will play an animation depending on Pose and Trajectory
				//That contains the tag Locomotion, but not the idle tag.
				//The trajectory will be predicated by a TrajectoryPredication Task.
				action
					.MatchPoseAndTrajectory(synthesizer
						.Query
						.Where(Locomotion.Default)
						.Except(Idle.Default), action
						.TrajectoryPrediction()
						.GetAs<TrajectoryPredictionTask>()
						.trajectory);

				//
				action.GetChildByType<MatchFragmentTask>().trajectoryWeight = responsiveness;

				//We store the TrajectoryHeuristic to manipulate the threshold when the player is close to stopping and playing the stopping clip
				//Giving a higher threshold will make kinematica conservative even though kinematica finds a better clip.
				//Once the stopping clip is finished we make the threshold to zero to make kinematica not conservative and will make sure we will reach a valid transition point to idle.
				_heuristic = action.GetChildByType<TrajectoryHeuristicTask>();
			}

			//_selector contains the SelectorTask.
			_locomotion = selector.GetAs<SelectorTask>();
		}

		_previousFrameCount = Time.frameCount;

		synthesizer.Tick(_locomotion);

		ref var idle = ref synthesizer.GetChildByType<ConditionTask>(_locomotion).Ref;
		ref var prediction = ref synthesizer.GetChildByType<TrajectoryPredictionTask>(_locomotion).Ref;

		prediction.velocityFactor = velocityFactor;
		prediction.rotationFactor = rotationFactor;

		synthesizer.GetChildByType<TrajectoryHeuristicTask>(_locomotion).Ref.threshold = 100;

		var analogInput = Utility.GetAnalogInput(_horizontal, _vertical);
		idle.value = math.length(analogInput) <= 0.2f;

		if (idle)
		{
			prediction.linearSpeed = 0;
			if (!_isBraking && math.length(synthesizer.CurrentVelocity) < _brakeSpeed) _isBraking = true;
		}
		else
		{
			_moveDirection = Utility.GetDesiredForwardDirection(analogInput, _moveDirection);
			prediction.linearSpeed = math.length(analogInput) * _desiredSpeed;

			prediction.movementDirection = _moveDirection;
			prediction.forwardDirection = _moveDirection;
			_isBraking = false;
		}

		var threshold = 0.03f;
		var samplingTimeInfo = AnimationUtility.GetSamplingTimeInfo<Locomotion>(_kinematica);

		if (samplingTimeInfo.x)
		{
			if (_isBraking) threshold = 0.25f;
		}
		else if (samplingTimeInfo.y)
		{
			threshold = 0.0f;
		}

		synthesizer.GetChildByType<TrajectoryHeuristicTask>(_heuristic).Ref.threshold = threshold;

		IAnimation targetAnimation = null;

		foreach (var target in gameObject.GetComponents<IAnimation>())
			if (target.OnChange(ref synthesizer))
				targetAnimation = target;

		if (targetAnimation != null) return targetAnimation;

		return this;
		//Return other Animation before hand if
	}

	public bool OnChange(ref MotionSynthesizer synthesizer)
	{
		return false;
	}

	public override void OnEarlyUpdate(bool rewind)
	{
		if (!rewind)
		{
			_horizontal = Input.GetAxis("Horizontal");
			_vertical = Input.GetAxis("Vertical");

			_desiredSpeed = Input.GetKey(sprintKey) ? desiredFastSpeed : desiredSlowSpeed;
		}
	}

	public override void WriteToStream(Buffer buffer)
	{
		buffer.Write(_previousFrameCount);

		buffer.Write(responsiveness);
		buffer.Write(velocityFactor);
		buffer.Write(rotationFactor);
		buffer.Write(desiredSlowSpeed);
		buffer.Write(desiredFastSpeed);

		buffer.Write(_horizontal);
		buffer.Write(_vertical);
	}

	public override void ReadFromStream(Buffer buffer)
	{
		_previousFrameCount = buffer.Read32();

		responsiveness = buffer.ReadSingle();
		velocityFactor = buffer.ReadSingle();
		rotationFactor = buffer.ReadSingle();
		desiredSlowSpeed = buffer.ReadSingle();
		desiredFastSpeed = buffer.ReadSingle();

		_horizontal = buffer.ReadSingle();
		_vertical = buffer.ReadSingle();
	}
}