using JetBrains.Annotations;
using Unity.Kinematica;
using Unity.Mathematics;
using UnityEngine;

public class UnitAnimation : Kinematica
{
	[SerializeField] private LocomotionAnimation initialAnimation;

	private IAnimation _currentAnimation;

	public new virtual void Update()
	{
		if (!Synthesizer.IsValid || !initialAnimation)
			return;

		_currentAnimation = _currentAnimation != null
			? _currentAnimation.OnUpdate(_deltaTime)
			: initialAnimation.OnUpdate(_deltaTime);

		base.Update();
	}
}


internal static class AnimationUtility
{
    /// <summary>
    ///     returns SamplingInformation of current pose
    /// </summary>
    /// <returns>(Is T, hasReachedEndOfSegment)</returns>
    public static bool2 GetSamplingTimeInfo<T>([NotNull] Kinematica kinematica)
		where T : struct
	{
		ref var synthesizer = ref kinematica.Synthesizer.Ref;
		ref var binary = ref synthesizer.Binary;

		var sampleTime = synthesizer.Time;

		ref var segment = ref binary.GetSegment(sampleTime.timeIndex.segmentIndex);
		ref var tag = ref binary.GetTag(segment.tagIndex);
		ref var trait = ref binary.GetTrait(tag.traitIndex);

		var condition = new bool2(false);
		if (trait.typeIndex == binary.GetTypeIndex<T>())
			condition.x = true;
		else if (sampleTime.timeIndex.frameIndex >= segment.destination.numFrames - 1) condition.y = true;

		return condition;
	}

	public static bool IsSegmentCompleted([NotNull] Kinematica kinematica)
	{
		ref var synthesizer = ref kinematica.Synthesizer.Ref;
		ref var binary = ref synthesizer.Binary;

		var sampleTime = synthesizer.Time;

		ref var segment = ref binary.GetSegment(sampleTime.timeIndex.segmentIndex);
		ref var tag = ref binary.GetTag(segment.tagIndex);
		ref var trait = ref binary.GetTrait(tag.traitIndex);

		return sampleTime.timeIndex.frameIndex >= segment.destination.numFrames - 1;
	}
}