using Unity.Kinematica;
using UnityEngine;

namespace HelloWorld
{
	[RequireComponent(typeof(Kinematica))]
	public class HelloWorld : MonoBehaviour
	{
		private bool idle;

		private void Update()
		{
			if (Input.anyKeyDown)
			{
				idle ^= true;

				var kinematica = GetComponent<Kinematica>();

				ref var synthesizer = ref kinematica.Synthesizer.Ref;

				if (idle)
					synthesizer.Root.Action().PlayFirstSequence(
						synthesizer.Query.Where(
							Locomotion.Default).And(Idle.Default));
				else
					synthesizer.Root.Action().PlayFirstSequence(
						synthesizer.Query.Where(
							Locomotion.Default).Except(Idle.Default));
			}
		}
	}
}