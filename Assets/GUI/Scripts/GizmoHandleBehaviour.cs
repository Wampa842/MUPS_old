using System;
using UnityEngine;

namespace MUPS.UI
{
	[Serializable]
	public class GizmoHandleBehaviour : MonoBehaviour
	{
		public enum VectorType { Custom, Forward, Up, Right }
		public enum TransformType { Disabled, Translation, Rotation, Scale }

		// Appearance
		public Color defaultColor = Color.red;
		public Color hoverColor = Color.yellow;

		// Behaviour
		public TransformType transformType;
		public VectorType vectorType;
		public Vector3 customVector;
		public float scale;
		public bool useVerticalAxis = false;
		public GizmoCoordinateSystem coordinateSystem;

		// References
		public Transform attachment;

		private bool _local
		{
			get
			{
				return attachment.GetComponent<GizmoBehaviour>().GizmoType == GizmoCoordinateSystem.Local;
			}
		}
		private string _axisName { get { return useVerticalAxis ? "Vertical" : "Horizontal"; } }
		private Vector3 _vector
		{
			get
			{
				switch (vectorType)
				{
					case VectorType.Forward:
						return new Vector3(0, 0, 1);
					case VectorType.Right:
						return new Vector3(1, 0, 0);
					case VectorType.Up:
						return new Vector3(0, 1, 0);
					case VectorType.Custom:
						return customVector;
					default:
						return new Vector3();
				}
			}
		}
		private Space _space { get { return _local ? Space.Self : Space.World; } }
		private Material _material;
		private GizmoBehaviour _parent;

		private void Awake()
		{
			_material = GetComponent<MeshRenderer>().material;
			_material.SetColor("_Color", defaultColor);
			_parent = attachment.GetComponent<GizmoBehaviour>();
		}

		private void OnMouseEnter()
		{
			_material.SetColor("_Color", hoverColor);
		}

		private void OnMouseExit()
		{
			_material.SetColor("_Color", defaultColor);
		}

		private void OnMouseDrag()
		{
			float s = Input.GetAxis(_axisName) * scale;
			Transform target = _parent.SelectedModel;
			if (target != null)
			{
				switch (transformType)
				{
					case TransformType.Translation:
						target.Translate(_vector * s, _space);
						attachment.Translate(_vector * s, _space);
						break;
					case TransformType.Rotation:
						target.Rotate(_vector, s, _space);
						attachment.Rotate(_vector, s, _space);
						break;
					case TransformType.Scale:
						target.localScale += new Vector3(s, s, s);
						attachment.localScale += new Vector3(s, s, s);
						break;
					default:
						break;
				}
			}
			if (!_local)
			{
				transform.parent.rotation = Quaternion.Euler(0, 0, 0);
			}
		}
	}
}