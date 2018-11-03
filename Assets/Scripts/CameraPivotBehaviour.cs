using UnityEngine;
using UnityEngine.EventSystems;
using Crosstales.FB;
using System.IO;
using MUPS;

namespace MUPS.Scene
{
	public class CameraPivotBehaviour : MonoBehaviour
	{
		// Singleton instance
		public static CameraPivotBehaviour Instance { get; private set; }

		// Inputs
		private float _scroll;  // Zoom
		private bool _rmb;      // Rotate
		private bool _mmb;      // Pan
		private bool _shift;    // Faster movement
		private bool _ctrl;     // Slower movement
		private bool _alt;      // Change movement type
		private float _deltaMult { get { return (_shift ? 3.0f : 1.0f) / (_ctrl ? 3.0f : 1.0f); } } // Movement speed multiplier
		private bool _mouseControlsActive;  // Whether the cursor is currently on a UI element

		// GameObjects
		public Transform cameraSlider;
		public Camera geometryCamera;
		public Camera overlayCamera;
		public Camera renderCamera;
		private PointerEventData _pointerEvent;

		// Screenshot
		private bool _takeScreenshot = false;
		private string _screenshotPath;

		// Behaviour
		public CameraData CameraData
		{
			get
			{
				return new CameraData(transform.position, transform.rotation, cameraSlider.localRotation.z, cameraSlider.localPosition.z, geometryCamera.fieldOfView);
			}
			set
			{
				transform.position = value.Position;
				transform.rotation = value.Rotation;
				cameraSlider.localPosition = new Vector3(0, 0, value.Distance);
				cameraSlider.localRotation = Quaternion.Euler(0, 0, value.Roll);
				geometryCamera.fieldOfView = value.FieldOfView;
				overlayCamera.fieldOfView = value.FieldOfView;
				renderCamera.fieldOfView = value.FieldOfView;
			}
		}

		public void ResetCamera()
		{
			transform.position = new Vector3(0, 1, 0);
			transform.rotation = new Quaternion();
			cameraSlider.localPosition = new Vector3(0, 0, -5);
			cameraSlider.localRotation = new Quaternion();
			geometryCamera.fieldOfView = 60.0f;
			overlayCamera.fieldOfView = 60.0f;
			renderCamera.fieldOfView = 60.0f;
		}

		public void RenderImage()
		{
			ExtensionFilter[] exts = new ExtensionFilter[] { new ExtensionFilter("PNG image", "png"), new ExtensionFilter("JPG image", "jpg", "jpeg"), new ExtensionFilter("TrueVision TGA", "tga"), new ExtensionFilter("OpenEXR", "exr") };
			_screenshotPath = FileBrowser.SaveFile("Render image", SettingsBehaviour.Instance.Settings.ImageExportDirectory, "", exts);
			if (!string.IsNullOrEmpty(_screenshotPath))
			{
				SettingsBehaviour.Instance.Settings.ImageExportDirectory = _screenshotPath;
				_takeScreenshot = true;
			}
		}

		// Messages
		private void Awake()
		{
			if(Instance == null)
			{
				Instance = this;
			}
			else if(Instance != this)
			{
				Destroy(Instance);
				Instance = this;
			}
		}

		private void Update()
		{
			// Get relevant controls
			_scroll = Input.GetAxis("Mouse ScrollWheel");
			_rmb = Input.GetMouseButton(1);
			_mmb = Input.GetMouseButton(2);
			_shift = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
			_ctrl = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
			_alt = Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt) || Input.GetKey(KeyCode.AltGr);
			_mouseControlsActive = !EventSystem.current.IsPointerOverGameObject();

			// Pan camera
			if (_mmb && _mouseControlsActive)
			{
				transform.Translate(new Vector3(Input.GetAxis("Horizontal") * _deltaMult, Input.GetAxis("Vertical") * _deltaMult, 0) * -0.01f, Space.Self);
			}
			// If not panning, orbit camera
			else if (_rmb && _mouseControlsActive)
			{
				if (_alt)
				{
					// Rotate camera around local Z axis
					cameraSlider.Rotate(new Vector3(0, 0, Input.GetAxis("Horizontal") * _deltaMult * 0.2f), Space.Self);
				}
				else
				{
					// Orbit around pivot
					transform.Rotate(new Vector3(0, Input.GetAxis("Horizontal") * _deltaMult * 0.2f, 0), Space.World);
					transform.Rotate(new Vector3(-Input.GetAxis("Vertical") * _deltaMult * 0.2f, 0, 0), Space.Self);
				}
			}

			// Zoom camera (independently from pan/rotate)
			if (_scroll != 0 && _mouseControlsActive)
			{
				if (_alt)
				{
					// Zoom with FOV
					geometryCamera.fieldOfView -= 5.0f * Mathf.Sign(_scroll) * _deltaMult;
					overlayCamera.fieldOfView = renderCamera.fieldOfView = geometryCamera.fieldOfView;
				}
				else
				{
					// Zoom with position
					cameraSlider.Translate(new Vector3(0, 0, 0.25f * Mathf.Sign(_scroll) * _deltaMult), Space.Self);
				}
			}
		}

		//private void OnGUI()
		//{
		//	GUI.Label(new Rect(10, 10, 200, 200), string.Format("LMB: {0}\nRMB: {1}\nMMB: {2}\n3: {3}\n4: {4}\nOn bottom panel: {5}\nCam distance: {6}", Input.GetMouseButton(0), _rmb, _mmb, Input.GetMouseButton(3), Input.GetMouseButton(4), _mouseControlsActive, cameraSlider.localPosition.z));

		//}

		public void LateUpdate()
		{
			if (_takeScreenshot)
			{
				renderCamera.enabled = true;

				RenderTexture target = new RenderTexture(new RenderTextureDescriptor(SettingsBehaviour.Instance.Settings.RenderWidth, SettingsBehaviour.Instance.Settings.RenderHeight, RenderTextureFormat.ARGB32));
				renderCamera.targetTexture = target;
				Texture2D img = new Texture2D(target.width, target.height, TextureFormat.RGBA32, false);
				renderCamera.Render();
				RenderTexture.active = target;
				img.ReadPixels(new Rect(0, 0, target.width, target.height), 0, 0);
				renderCamera.targetTexture = null;
				RenderTexture.active = null;

				string ext = Path.GetExtension(_screenshotPath).ToLower();
				switch (ext)
				{
					case ".png":
						File.WriteAllBytes(_screenshotPath, img.EncodeToPNG());
						break;
					case ".jpg":
						File.WriteAllBytes(_screenshotPath, img.EncodeToJPG());
						break;
					case ".jpeg":
						File.WriteAllBytes(_screenshotPath, img.EncodeToJPG());
						break;
					case ".exr":
						File.WriteAllBytes(_screenshotPath, img.EncodeToEXR());
						break;
					default: throw new System.InvalidOperationException(string.Format("Output format {0} is currently not supported.", ext));
				}

				Destroy(target);
				Destroy(img);

				renderCamera.enabled = false;
				_takeScreenshot = false;
			}
		}
	}
}