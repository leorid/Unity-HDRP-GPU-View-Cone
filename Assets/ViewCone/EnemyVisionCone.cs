using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

namespace JL
{
	// execute always to see the effect in the scene view
	[ExecuteAlways]
	public class EnemyVisionCone : MonoBehaviour
	{
		[Header("View Cone Settings")]
		[SerializeField] Color _color = Color.yellow;
		[SerializeField, Range(0, 1000)] float _viewDistance = 5;
		[SerializeField, Range(0, 180)] float _angle = 45;

		[Header("Editor Settings")]
		[SerializeField] bool _showInEditor;

		[Header("References")]
		[SerializeField] CustomPassVolume _customPass;
		[SerializeField] Camera _enemyCam;
		[SerializeField] DecalProjector _decalProjector;
		[SerializeField] MatrixSetter _matrixSetter;
		[SerializeField] Material _templateMaterial;
		[SerializeField] Renderer _debugQuad;
		[SerializeField] Material _debugQuadDefaultMat;

		[Header("ReadOnly")]
		[SerializeField] RenderTexture _depthTexture;
		[SerializeField] Material _decalMat;
		[SerializeField] int _instanceID;

		Vector3 _lastPosition;
		Quaternion _lastRotation;

		float _startTimeOffset = 1;
		float _startTime;

		void OnEnable()
		{
			if (!CheckFields())
			{
				return;
			}

			if (_instanceID != gameObject.GetInstanceID())
			{
				_depthTexture = null;
				_decalMat = null;
			}

			_instanceID = gameObject.GetInstanceID();

			_startTime = Time.time;

			VisionConeCustomPass visionConeCustomPass = null;
			foreach (CustomPass cp in _customPass.customPasses)
			{
				if (cp is VisionConeCustomPass)
				{
					visionConeCustomPass = cp as VisionConeCustomPass;
					break;
				}
			}

			if (visionConeCustomPass == null)
			{
				Debug.LogError("Custom pass not found, aborting.", gameObject);
				return;
			}

			// create a depth texture for this object
			if (!_depthTexture)
			{
				_depthTexture = new RenderTexture(_enemyCam.pixelWidth * 2, _enemyCam.pixelHeight * 2, 24, RenderTextureFormat.Depth);
				_depthTexture.name = gameObject.name + " RT";
				_depthTexture.Create();
			}

			if (_debugQuad)
			{
				MaterialPropertyBlock mpb = new MaterialPropertyBlock();
				mpb.SetTexture("_BaseColorMap", _depthTexture);

				_debugQuad.SetPropertyBlock(mpb);
			}

			// create a new material to set the texture for this object
			_decalMat = new Material(_templateMaterial);
			_decalMat.name = gameObject.name + " Mat";

			SetMaterialValues();
			_decalMat.SetTexture("_DepthTex", _depthTexture);
			_decalMat.enableInstancing = true;

			_decalProjector.material = _decalMat;

			visionConeCustomPass.depthTexture = _depthTexture;
		}

		private void OnDisable()
		{
			if (_depthTexture && _depthTexture.IsCreated())
			{
				_depthTexture.Release();
#if UNITY_EDITOR
				DestroyImmediate(_depthTexture);
#else
				Destroy(_depthTexture);
#endif
			}
			if (_decalMat)
			{
#if UNITY_EDITOR
				DestroyImmediate(_decalMat);
#else
				Destroy(_decalMat);
#endif
			}
			_instanceID = 0;
		}

		bool CheckFields()
		{
			if (!_customPass)
			{
				Debug.LogError("Field not assigned: _customPass", gameObject);
				return false;
			}
			if (!_enemyCam)
			{
				Debug.LogError("Field not assigned: _enemyCam", gameObject);
				return false;
			}
			if (!_decalProjector)
			{
				Debug.LogError("Field not assigned: _decalProjector", gameObject);
				return false;
			}
			if (!_matrixSetter)
			{
				Debug.LogError("Field not assigned: _matrixSetter", gameObject);
				return false;
			}
			if (!_templateMaterial)
			{
				Debug.LogError("Field not assigned: _templateMaterial", gameObject);
				return false;
			}
			return true;
		}

		void SetMaterialValues()
		{
			if (_decalMat)
			{
				_decalMat.SetColor("_BaseColor", _color);
				_decalMat.SetFloat("_Range", _viewDistance);
				_decalMat.SetFloat("_Angle", _angle);
				_decalMat.SetFloat("_Margin", 0.001f);
			}
		}

		void Update()
		{
			if (!_matrixSetter || !_customPass) return;

			bool execute;
			if (Application.isPlaying)
			{
				execute = _lastPosition != transform.position ||
				_lastRotation != transform.rotation ||
				Time.time < _startTime + _startTimeOffset;
			}
			else
			{
				execute = _showInEditor;
			}

			if (execute)
			{
				_matrixSetter.SetMatrix();
				_customPass.gameObject.SetActive(true);
				_lastPosition = transform.position;
				_lastRotation = transform.rotation;
			}
			else
			{
				_customPass.gameObject.SetActive(false);
			}
		}

		private void OnValidate()
		{
			SetMaterialValues();
		}
	}
}
