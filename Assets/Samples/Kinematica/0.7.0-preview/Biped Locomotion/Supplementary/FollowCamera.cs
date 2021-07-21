using Unity.Kinematica;
using Unity.Mathematics;
using UnityEngine;

namespace BipedLocomotion
{
	public class FollowCamera : MonoBehaviour
	{
		//
		// Target transform to be tracked
		//

		public Transform targetTransform;

		[Range(0.01f, 1.0f)] public float smoothFactor = 0.5f;

		public float degreesPerSecond = 180.0f;

		public float maximumYawAngle = 45.0f;

		public float minimumHeight = 0.2f;

		public float heightOffset = 1.0f;

		//
		// Offset to be maintained between camera and target
		//

		private float3 offset;

		private float3 TargetPosition => Convert(targetTransform.position) + new float3(0.0f, heightOffset, 0.0f);

		private void Start()
		{
			offset = Convert(transform.position) - TargetPosition;
		}

		private void LateUpdate()
		{
			var radiansPerSecond = math.radians(degreesPerSecond);

			var horizontal = InputUtility.GetCameraHorizontalInput();
			var vertical = InputUtility.GetCameraVerticalInput();

			if (math.abs(horizontal) >= 0.2f) RotateOffset(Time.deltaTime * horizontal * radiansPerSecond, Vector3.up);

			if (math.abs(vertical) >= 0.2f)
			{
				var angleAt = math.abs(math.asin(transform.forward.y));
				var maximumAngle = math.radians(maximumYawAngle);
				var angleDeltaDesired = Time.deltaTime * vertical * radiansPerSecond;
				var angleDeltaClamped =
					CalculateAngleDelta(angleDeltaDesired,
						maximumAngle - angleAt);

				RotateOffset(angleDeltaClamped, transform.right);
			}

			Vector3 cameraPosition = TargetPosition + offset;

			if (cameraPosition.y <= minimumHeight) cameraPosition.y = minimumHeight;

			transform.position = Vector3.Slerp(transform.position, cameraPosition, smoothFactor);

			transform.LookAt(TargetPosition);
		}

		private float CalculateAngleDelta(float angleDeltaDesired, float angleRemaining)
		{
			if (math.dot(transform.forward, Missing.up) >= 0.0f)
				return -math.min(-angleDeltaDesired, angleRemaining);
			return math.min(angleDeltaDesired, angleRemaining);
		}

		private void RotateOffset(float angleInRadians, float3 axis)
		{
			offset = math.mul(quaternion.AxisAngle(axis, angleInRadians), offset);
		}

		private static float3 Convert(Vector3 p)
		{
			return p;
		}
	}
}