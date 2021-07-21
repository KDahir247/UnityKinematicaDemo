using System;
using Unity.Kinematica.Editor;

namespace BipedLocomotion
{
	[Serializable]
	[Tag("Attack", "#f54242")]
	public struct AttackTag : Payload<Attack>
	{
		public Attack Build(PayloadBuilder builder)
		{
			return Attack.Default;
		}
	}
}