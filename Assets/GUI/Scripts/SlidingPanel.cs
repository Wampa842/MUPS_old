// Place the containing panel in the position where it's visible, snap it to the edge of the parent container, then set up the properties.
// If the size of the panel changes, make sure you call the UpdateOffsets method before the user clicks a button.

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MUPS.UI
{
	[Serializable]
	public class SlidingPanel : MonoBehaviour
	{
		public enum HideDirection { Down, Up, Left, Right, CustomVector }
		public HideDirection Direction = HideDirection.CustomVector;
		public Vector2 customVector = new Vector2();
		public float transitionDuration = 0.3f;
		public bool visible = true;

		[Space]
		public Button toggleButton;
		public string textWhenHidden = "Show";
		public string textWhenVisible = "Hide";

		private RectTransform _rt;
		private Dictionary<HideDirection, Vector2> _offsets;
		private Text _buttonText;

		private Vector2 _step;
		private Vector2 _distance;
		private Vector2 _stepAbs;
		private Vector2 _distanceAbs;
		private bool _moving = false;

		private static Vector2 Vec2Abs(Vector2 src)
		{
			return new Vector2(Mathf.Abs(src.x), Mathf.Abs(src.y));
		}

		public void UpdateOffsets()
		{
			_offsets = new Dictionary<HideDirection, Vector2>();
			_offsets.Add(HideDirection.Down, new Vector2(0, -(_rt.rect.height)));
			_offsets.Add(HideDirection.Up, new Vector2(0, _rt.rect.height));
			_offsets.Add(HideDirection.Left, new Vector2(-(_rt.rect.width), 0));
			_offsets.Add(HideDirection.Right, new Vector2(_rt.rect.width, 0));
			_offsets.Add(HideDirection.CustomVector, customVector);
		}

		public void Toggle()
		{
			if (visible)
				Hide();
			else
				Show();
		}

		public void Hide()
		{
			if (visible)
			{
				Vector2 offset = _offsets[Direction];
				if (transitionDuration > 0)
				{
					_step = offset / transitionDuration;
					_distance = offset;
					_stepAbs = Vec2Abs(_step);
					_distanceAbs = Vec2Abs(offset);
					_moving = true;
				}
				else
					_rt.Translate(offset.x, offset.y, 0);
				visible = false;
				_buttonText.text = textWhenHidden;
			}
		}

		public void Show()
		{
			if (!visible)
			{
				Vector2 offset = -_offsets[Direction];
				if (transitionDuration > 0)
				{
					_step = offset / transitionDuration;
					_distance = offset;
					_stepAbs = Vec2Abs(_step);
					_distanceAbs = Vec2Abs(offset);
					_moving = true;
				}
				else
					_rt.Translate(offset.x, offset.y, 0);
				visible = transform;
				_buttonText.text = textWhenVisible;
			}
		}

		public void HideImmediate()
		{
			if (visible)
			{
				Vector2 offset = _offsets[Direction];
				_rt.Translate(offset.x, offset.y, 0);
				visible = false;
				_buttonText.text = textWhenHidden;
			}
		}

		public void ShowImmediate()
		{
			if (!visible)
			{
				Vector2 offset = -_offsets[Direction];
				_rt.Translate(offset.x, offset.y, 0);
				visible = true;
				_buttonText.text = textWhenVisible;
			}
		}

		public void SetVisible(bool setTo)
		{
			if (setTo)
				ShowImmediate();
			else
				HideImmediate();
		}

		private void Awake()
		{
			_buttonText = toggleButton.GetComponentInChildren<Text>();

			_rt = (RectTransform)transform;
			UpdateOffsets();
			if (!visible)
			{
				_rt.Translate(_offsets[Direction]);
				_buttonText.text = textWhenHidden;
			}

			if (toggleButton != null)
				toggleButton.onClick.AddListener(Toggle);
		}

		private void Start()
		{
		}

		private void Update()
		{
			if (_moving)
			{
				_rt.Translate(_step.x * Time.deltaTime, _step.y * Time.deltaTime, 0);
				_distance -= _step * Time.deltaTime;
				_distanceAbs -= _stepAbs * Time.deltaTime;
				if (_distanceAbs.x <= 0 && _distanceAbs.y <= 0)
				{
					_rt.Translate(_distance.x, _distance.y, 0);
					_moving = false;
				}
			}
		}
	}
}