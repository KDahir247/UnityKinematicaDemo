using Unity.Kinematica;
using Unity.Mathematics;
using UnityEngine;

namespace BipedLocomotion
{
	[RequireComponent(typeof(Kinematica))]
	public class Biped : MonoBehaviour
	{
		[Header("Prediction settings")]
		[Tooltip("Desired speed in meters per second for slow movement.")]
		[Range(0.0f, 10.0f)]
		public float desiredSpeedSlow = 3.9f;

		[Tooltip("Desired speed in meters per second for fast movement.")] [Range(0.0f, 10.0f)]
		public float desiredSpeedFast = 5.5f;

		[Tooltip("How fast or slow the target velocity is supposed to be reached.")] [Range(0.0f, 1.0f)]
		public float velocityPercentage = 1.0f;

		[Tooltip("How fast or slow the desired forward direction is supposed to be reached.")] [Range(0.0f, 1.0f)]
		public float forwardPercentage = 1.0f;

		private Identifier<SelectorTask> locomotion;

		private float3 movementDirection = Missing.forward;

		private float desiredLinearSpeed => InputUtility.IsPressingActionButton() ? desiredSpeedFast : desiredSpeedSlow;

		private void Update()
		{
			var kinematica = GetComponent<Kinematica>();

			ref var synthesizer = ref kinematica.Synthesizer.Ref;

			MemoryIdentifier root = locomotion;
			if (Input.GetKey(KeyCode.P))
			{
				var action = synthesizer.Root.Action();

				root = action.GetAs<ActionTask>().self;
			}
			else if (Input.GetKey(KeyCode.L))
			{
				root = locomotion;
			}

			synthesizer.Tick(root);

			/*ref var prediction = ref synthesizer.GetChildByType<TrajectoryPredictionTask>(locomotion).Ref;
			ref var idle = ref synthesizer.GetChildByType<ConditionTask>(locomotion).Ref;

			var horizontal = InputUtility.GetMoveHorizontalInput();
			var vertical = InputUtility.GetMoveVerticalInput();

			float3 analogInput = Utility.GetAnalogInput(horizontal, vertical);

			prediction.velocityFactor = velocityPercentage;
			prediction.rotationFactor = forwardPercentage;

			idle.value = math.length(analogInput) <= 0.1f;

			if (idle)
			{
			    prediction.linearSpeed = 0.0f;
			}
			else
			{
			    movementDirection =
			        Utility.GetDesiredForwardDirection(
			            analogInput, movementDirection);

			    prediction.linearSpeed =
			        math.length(analogInput) *
			        desiredLinearSpeed;

			    prediction.movementDirection = movementDirection;
			    prediction.forwardDirection = movementDirection;
			}*/
		}

		private void OnEnable()
		{
			var kinematica = GetComponent<Kinematica>();

			ref var synthesizer = ref kinematica.Synthesizer.Ref;

			synthesizer.PlayFirstSequence(
				synthesizer.Query.Where(
					Locomotion.Default).And(Idle.Default));

			var selector = synthesizer.Root.Selector();

			{
				var sequence = selector.Condition().Sequence();

				sequence.Action().MatchPose(
					synthesizer.Query.Where(
						Locomotion.Default).And(Idle.Default), 0.01f);

				sequence.Action().Timer();
			}

			{
				var action = selector.Action();

				action.MatchPoseAndTrajectory(
					synthesizer.Query.Where(
						Locomotion.Default).Except(Idle.Default),
					action.TrajectoryPrediction().GetAs<TrajectoryPredictionTask>().trajectory);
			}

			locomotion = selector.GetAs<SelectorTask>();
		}

		private void OnGUI()
		{
			InputUtility.DisplayMissingInputs(InputUtility.ActionButtonInput | InputUtility.MoveInput |
			                                  InputUtility.CameraInput);
		}
	}
}