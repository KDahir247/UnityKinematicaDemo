﻿using System;
using Unity.Kinematica;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.AI;

namespace Navigation
{
	[RequireComponent(typeof(Kinematica))]
	public class Navigation : MonoBehaviour
	{
		public Transform targetMarker;

		private Identifier<SelectorTask> locomotion;

		private void Update()
		{
			var kinematica = GetComponent<Kinematica>();

			ref var synthesizer = ref kinematica.Synthesizer.Ref;

			synthesizer.Tick(locomotion);

			ref var navigation = ref synthesizer.GetChildByType<NavigationTask>(locomotion).Ref;
			ref var idle = ref synthesizer.GetChildByType<ConditionTask>(locomotion).Ref;

			if (Input.GetMouseButtonDown(0))
			{
				RaycastHit hit;
				var ray = Camera.main.ScreenPointToRay(Input.mousePosition);

				if (Physics.Raycast(ray, out hit))
				{
					var targetPosition = hit.point;

					var desiredSpeed = 1.5f;
					var accelerationDistance = 0.1f;
					var decelerationDistance = 1.0f;

					var navMeshPath = new NavMeshPath();
					if (NavMesh.CalculatePath(transform.position, targetPosition, NavMesh.AllAreas, navMeshPath))
					{
						var navParams = new NavigationParams
						{
							desiredSpeed = desiredSpeed,
							maxSpeedAtRightAngle = 0.0f,
							maximumAcceleration =
								NavigationParams.ComputeAccelerationToReachSpeed(desiredSpeed, accelerationDistance),
							maximumDeceleration =
								NavigationParams.ComputeAccelerationToReachSpeed(desiredSpeed, decelerationDistance),
							intermediateControlPointRadius = 1.0f,
							finalControlPointRadius = 0.15f,
							pathCurvature = 5.0f
						};

						var points = Array.ConvertAll(navMeshPath.corners, pos => new float3(pos));
						navigation.FollowPath(points, navParams);

						targetMarker.gameObject.SetActive(true);
						targetMarker.position = targetPosition;
					}
				}
			}

			idle.value = !navigation.IsPathValid || navigation.GoalReached;
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
					action.Navigation().GetAs<NavigationTask>().trajectory);
			}

			locomotion = selector.GetAs<SelectorTask>();
		}

		private void OnGUI()
		{
			GUI.Label(new Rect(10, 10, 900, 20),
				"Click somewhere on the ground to move character toward that location.");
		}
	}
}