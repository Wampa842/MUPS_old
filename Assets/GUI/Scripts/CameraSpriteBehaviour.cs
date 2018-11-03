using UnityEngine;
using UnityEngine.UI;

namespace MUPS
{
	class CameraSpriteBehaviour : MonoBehaviour
	{
		public Transform cameraPivot = null;
		public Transform cameraSlider = null;

		private RectTransform _rt;
		private Image _sprite;

		void Awake()
		{
			_rt = (RectTransform)transform;
			_sprite = GetComponent<Image>();
		}

		void Update()
		{
			_rt.position = Camera.main.WorldToScreenPoint(cameraPivot.position);
			_sprite.color = new Color(0.28f, 0.28f, 1, (cameraSlider.localPosition.z > 0 ? 0.3f : 1.0f));
		}
	}
}