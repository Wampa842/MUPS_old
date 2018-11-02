using System;
using UnityEngine;

namespace MUPS.UI
{
	public enum GizmoCoordinateSystem { Off, Local, Global }
	public class GizmoBehaviour : MonoBehaviour
	{
		public Transform attachment;
		public Transform handles;
		public MUPS.Scene.SceneControllerBehaviour sceneController;

		private Transform _selectedModel;
		public Transform SelectedModel
		{
			get
			{
				return _selectedModel;
			}
			set
			{
				_selectedModel = value;
				transform.SetPositionAndRotation(_selectedModel.position, _selectedModel.rotation);
			}
		}

		private bool _visible = true;
		public bool Visible
		{
			get
			{
				return _visible;
			}
			set
			{
				_visible = value;
				for (int i = 0; i < handles.childCount; ++i)
				{
					handles.GetChild(i).GetComponent<Renderer>().enabled = value;
				}
			}
		}

		private GizmoCoordinateSystem _gizmoType;
		public GizmoCoordinateSystem GizmoType
		{
			get
			{
				return _gizmoType;
			}
			set
			{
				if(sceneController.Selected < 0)
				{
					_gizmoType = GizmoCoordinateSystem.Off;
					Visible = false;
					return;
				}
				_gizmoType = value;
				switch (_gizmoType)
				{
					case GizmoCoordinateSystem.Off:
						Visible = false;
						break;
					case GizmoCoordinateSystem.Local:
						Visible = true;
						handles.localRotation = Quaternion.Euler(0, 0, 0);
						break;
					case GizmoCoordinateSystem.Global:
						Visible = true;
						handles.rotation = Quaternion.Euler(0, 0, 0);
						break;
				}
			}
		}
		public string GizmoButtonName
		{
			get
			{
				switch (_gizmoType)
				{
					case GizmoCoordinateSystem.Off:
						return "Control: off";
					case GizmoCoordinateSystem.Local:
						return "Control: local";
					case GizmoCoordinateSystem.Global:
						return "Control: global";
					default:
						return "Control: invalid";
				}
			}
		}

		void Update()
		{
			if (Input.GetKey(KeyCode.Space))
				Debug.LogFormat("Angle {0} right {1}", transform.localEulerAngles.y, transform.right);
		}
	}
}