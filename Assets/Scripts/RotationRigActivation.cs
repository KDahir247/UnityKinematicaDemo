using UnityEngine;
using UnityEngine.Animations.Rigging;
using UnityEngine.Assertions;

//Requires any Rig Constraint that Inherit from IAnimationJobData.
[RequireComponent(typeof(IAnimationJobData))]
public sealed class RotationRigActivation : MonoBehaviour
{
	private MultiRotationConstraint _constraint;

	// Start is called before the first frame update
	private void Start()
	{
		var rotationConstraint = gameObject.GetComponent<MultiRotationConstraint>();

		_constraint = rotationConstraint;

		Assert.IsNotNull(_constraint);
	}

	// Update is called once per frame
	private void Update()
	{
		if (Input.GetMouseButtonDown(1))
			_constraint.weight = 1.0f;
		else if (Input.GetMouseButtonUp(1)) _constraint.weight = 0.0f;
	}
}