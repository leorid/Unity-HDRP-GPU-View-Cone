using System;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

public class MatrixSetter : MonoBehaviour
{
	[SerializeField] DecalProjector _decalProjector;
	[SerializeField] Camera _cam;

	private void Update()
	{
		SetMatrix();
	}

	public void SetMatrix()
	{
		if (!_decalProjector)
		{
			Debug.LogError("Field not assigned: _decalProjector", gameObject);
			return;
		}
		if (!_cam)
		{
			Debug.LogError("Field not assigned: _cam", gameObject);
			return;
		}

		// get view matrix
		Matrix4x4 V = _cam.worldToCameraMatrix;
		// get projection matrix
		Matrix4x4 P = _cam.projectionMatrix;
		// multiply to get view projection matrix
		// order is important
		Matrix4x4 VP = P * V;

		// calculate because the matrix is different on different devices
		VP = GL.GetGPUProjectionMatrix(VP, false);

		// set it on the projector
		_decalProjector.material.SetMatrix("_EnemyCamWorldToViewportMatrix", VP);
	}
}
