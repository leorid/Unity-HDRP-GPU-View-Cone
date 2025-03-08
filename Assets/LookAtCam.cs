using UnityEngine;

public class LookAtCam : MonoBehaviour
{
	Transform _camT;

	private void Start()
	{
		_camT = Camera.main.transform;
	}

	void Update()
	{
		transform.rotation = _camT.rotation;
	}
}
