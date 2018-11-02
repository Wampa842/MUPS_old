using UnityEngine;
using UnityEngine.UI;

namespace MUPS
{
	class CameraSpriteBehaviour : MonoBehaviour
	{
		RectTransform _rt;
		Image _sprite;
		Transform _follow;
		Transform _parent;

		void Awake()
		{
			_follow = GameObject.Find("MainCamPivot").transform;
			_parent = _follow.Find("MainCam");
			_rt = GetComponent<RectTransform>();
			_sprite = GetComponent<Image>();
		}

		void Update()
		{
			_rt.position = Camera.main.WorldToScreenPoint(_follow.position);
			_sprite.color = new Color(0.28f, 0.28f, 1, (_parent.localPosition.z > 0 ? 0.3f : 1.0f));
		}
	}
}