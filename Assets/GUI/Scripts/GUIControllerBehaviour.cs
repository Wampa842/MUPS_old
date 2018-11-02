using System;
using UnityEngine;
using UnityEngine.UI;
using MUPS;

namespace MUPS.UI
{
	public class GUIControllerBehaviour : MonoBehaviour
	{
		public RectTransform bottomPanel;
		public Text toggleButtonText;
		public Text renderWidthInput;
		public Text renderHeightInput;
		public Slider lightAzimuth;
		public Slider lightElevation;
		public Transform mainLight;
		private bool _showGui = true;

		public void ToggleGui()
		{
			_showGui = !_showGui;
			float height = bottomPanel.rect.height;
			if (_showGui)
			{
				bottomPanel.Translate(new Vector3(0, height, 0));
				toggleButtonText.text = "▼ Hide";
			}
			else
			{
				bottomPanel.Translate(new Vector3(0, -height, 0));
				toggleButtonText.text = "▲ Show";
			}
		}

		public void RenderInputUpdate()
		{
			
		}

		public void SetLightDirection()
		{
			mainLight.rotation = Quaternion.Euler(0, lightAzimuth.value, 0);
			mainLight.Rotate(new Vector3(lightElevation.value, 0, 0), Space.Self);
		}

		public void Awake()
		{

		}
	}
}