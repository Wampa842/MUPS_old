using MUPS;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace MUPS.UI
{
	public class GUIControllerBehaviour : MonoBehaviour
	{
		public static GUIControllerBehaviour Instance { get; private set; }

		public RectTransform bottomPanel;
		public Text toggleButtonText;
		public InputField renderWidthInput;
		public InputField renderHeightInput;
		public Slider lightAzimuthSlider;
		public Text lightAzimuthInput;
		public Slider lightElevationSlider;
		public Text lightElevationInput;
		public Light mainLight;
		public Transform mainLightPivot;
		public GameObject floorGrid;
		private float _mainLightH = 1;
		private float _mainLightS = 0;
		private float _mainLightV = 1;
		private float _mainLightI = 1;

		// Update render properties
		public void RenderInputUpdate()
		{
			int w, h;
			if (int.TryParse(renderWidthInput.text, out w) && int.TryParse(renderHeightInput.text, out h))
			{
				SettingsBehaviour.Instance.Settings.RenderWidth = w;
				SettingsBehaviour.Instance.Settings.RenderHeight = h;
			}
		}

		// Set light rotation
		public void SetLightDirectionSlider()
		{
			float az = lightAzimuthSlider.value;
			float el = lightElevationSlider.value;

			SetLightDirection(az, el);
		}

		public void SetLightDirectionInput()
		{
			float az, el;

			if(float.TryParse(lightAzimuthInput.text, out az) && float.TryParse(lightElevationInput.text, out el))
			{
				SetLightDirection(az, el);
			}
		}

		public void SetLightDirection(float azimuth, float elevation)
		{
			lightAzimuthInput.text = azimuth.ToString();
			lightElevationInput.text = elevation.ToString();

			lightAzimuthSlider.value = azimuth;
			lightElevationSlider.value = elevation;

			mainLightPivot.rotation = Quaternion.Euler(0, azimuth, 0);
			mainLight.transform.localRotation = Quaternion.Euler(elevation, 0, 0);
		}

		public void GetLightDirection(out float azimuth, out float elevation)
		{
			azimuth = mainLightPivot.rotation.y;
			elevation = mainLight.transform.localRotation.x;
		}

		// Set light color
		public void SetLightColor(int channel)
		{
			const float scale = 0.01f;
			switch (channel)
			{
				case 0:
					_mainLightH = Mathf.Clamp01(_mainLightV + Input.GetAxis("Horizontal") * scale);
					break;
				case 1:
					_mainLightS = Mathf.Clamp01(_mainLightV + Input.GetAxis("Horizontal") * scale);
					_mainLightV = Mathf.Clamp01(_mainLightV + Input.GetAxis("Vertical") * scale);
					break;
				case 2:
					_mainLightI = Mathf.Clamp(_mainLightV + Input.GetAxis("Vertical") * scale, 0, float.MaxValue);
					break;
			}
			mainLight.color = Color.HSVToRGB(_mainLightH, _mainLightS, _mainLightV);
			mainLight.intensity = _mainLightI;
			Debug.LogFormat("{0} {1} {2} {3}", _mainLightH, _mainLightS, _mainLightV, _mainLightI);
		}

		// Toggle floor grid
		public void ToggleGrid()
		{
			floorGrid.SetActive(!floorGrid.activeSelf);
		}

		private void Start()
		{
			int w = SettingsBehaviour.Instance.Settings.RenderWidth;
			int h = SettingsBehaviour.Instance.Settings.RenderHeight;
			renderWidthInput.text = w.ToString();
			renderHeightInput.text = h.ToString();
			SetLightDirection(0, 60);
			
		}

		private void Awake()
		{
			if (Instance == null)
			{
				Instance = this;
			}
			else if (Instance != this)
			{
				Destroy(Instance);
				Instance = this;
			}
		}
	}
}