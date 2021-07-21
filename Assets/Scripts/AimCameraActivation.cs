using Cinemachine;
using UnityEngine;
using UnityEngine.Assertions;

[RequireComponent(typeof(CinemachineVirtualCamera))]
public class AimCameraActivation : MonoBehaviour
{
	[SerializeField] private int priority = 10;

	[SerializeField] private GameObject crossHairGameObject;

	[SerializeField] private float viewRange;
	
	private CinemachinePOV _cinemachinePOV;

	private CinemachineVirtualCamera _cinemachineVirtualCamera;

	private Transform _followTarget;
	private int maxPriority;

	private void Awake()
	{
		Cursor.lockState = CursorLockMode.Locked;
		Assert.IsNotNull(crossHairGameObject);

		_cinemachineVirtualCamera = gameObject.GetComponent<CinemachineVirtualCamera>();
		maxPriority = _cinemachineVirtualCamera.Priority + priority;

		if (crossHairGameObject.activeInHierarchy)
			crossHairGameObject.SetActive(false);

		Assert.IsNotNull(_cinemachineVirtualCamera.Follow);
		_followTarget = _cinemachineVirtualCamera.Follow;

		var povCinemachine = _cinemachineVirtualCamera.GetCinemachineComponent<CinemachinePOV>();
		Assert.IsNotNull(povCinemachine);
		_cinemachinePOV = povCinemachine;
	}

	// Update is called once per frame
	private void Update()
	{
		if (Input.GetMouseButtonDown(1))
		{
			var eulerAngles = _followTarget.eulerAngles;
			_cinemachinePOV.m_HorizontalAxis.Value = eulerAngles.y;

			_cinemachinePOV.m_HorizontalAxis.m_MaxValue = eulerAngles.y + viewRange;
			_cinemachinePOV.m_HorizontalAxis.m_MinValue = eulerAngles.y - viewRange;
			_cinemachinePOV.m_HorizontalAxis.m_Wrap = false;

			_cinemachineVirtualCamera.Priority += priority;
			crossHairGameObject.SetActive(true);
		}
		else if (Input.GetMouseButtonUp(1))
		{
			_cinemachineVirtualCamera.Priority -= priority;
			crossHairGameObject.SetActive(false);

			_cinemachinePOV.m_HorizontalAxis.m_MaxValue = 180.0f;
			_cinemachinePOV.m_HorizontalAxis.m_MinValue = -180.0f;
			_cinemachinePOV.m_HorizontalAxis.m_Wrap = true;

			//we want to make the to make the horizontal value for the pov cinemamachine to the forward rotation in this case it the y axis
		}

		if (_cinemachineVirtualCamera.Priority > maxPriority ||
		    _cinemachineVirtualCamera.Priority < maxPriority - priority)
		{
			_cinemachineVirtualCamera.Priority = maxPriority - priority;
			crossHairGameObject.SetActive(false);
		}
	}
}