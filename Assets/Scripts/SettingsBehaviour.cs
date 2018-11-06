using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using UnityEngine;

namespace MUPS
{
	[Serializable]
	public class SettingsData
	{
		public string ModelImportDirectory { get; set; }
		public string SkyboxImportDirectory { get; set; }
		public string SceneImportDirectory { get; set; }
		public string ImageExportDirectory { get; set; }
		public int RenderHeight { get; set; }
		public int RenderWidth { get; set; }

		public SettingsData()
		{
			ModelImportDirectory = string.Empty;
			SkyboxImportDirectory = string.Empty;
			SceneImportDirectory = string.Empty;
			ImageExportDirectory = string.Empty;
			RenderWidth = Screen.width;
			RenderHeight = Screen.height;
		}

		public static SettingsData Load(string path)
		{
			XmlReader reader = null;
			XmlSerializer serializer;
			Stream stream = null;
			try
			{
				serializer = new XmlSerializer(typeof(SettingsData));
				stream = new FileStream(path, FileMode.OpenOrCreate, FileAccess.ReadWrite);
				reader = XmlReader.Create(stream);
				return (serializer.Deserialize(reader) as SettingsData) ?? new SettingsData();
			}
			catch (Exception ex)
			{
				Debug.LogError(ex.ToString());
			}
			finally
			{
				if (stream != null)
					stream.Close();
				if (reader != null)
					reader.Close();
			}
			return new SettingsData();
		}

		public static void Save(string path, SettingsData data)
		{
			XmlSerializer serializer = null;
			XmlWriter writer = null;
			Stream stream = null;
			try
			{
				serializer = new XmlSerializer(typeof(SettingsData));
				stream = new FileStream(path, FileMode.Create, FileAccess.ReadWrite);
				writer = XmlWriter.Create(stream);
				serializer.Serialize(writer, data);
			}
			catch (Exception ex)
			{
				Debug.LogError(ex.ToString());
			}
			finally
			{
				if (stream != null)
					stream.Close();
				if (writer != null)
					writer.Close();
			}
		}
	}

	[Serializable]
	public class CameraData
	{
		public Vector3 Position { get; }
		public Quaternion Rotation { get; }
		public float Roll { get; }
		public float Distance { get; }
		public float FieldOfView { get; }

		public CameraData(Vector3 position, Quaternion rotation, float roll, float distance, float fieldOfView)
		{
			Position = position;
			Rotation = rotation;
			Roll = roll;
			Distance = distance;
			FieldOfView = fieldOfView;
		}
	}

	[Serializable]
	public class LightData
	{
		public Vector3 Euler { get; }
		public Color Color { get; }
		public float Intensity { get; }

		public LightData(float azimuth, float elevation, Color color, float intensity)
		{
			Euler = MUPS.Math.EulerFromAzEl(azimuth, elevation);
			Color = color;
			Intensity = intensity;
		}
	}

	[Serializable]
	public class SceneData
	{
		public CameraData Camera { get; set; }
		public LightData MainLight { get; set; }
		public Vector2 RenderSize { get; set; }
		List<string> Models { get; set; }
		string SkyTexturePath { get; set; }
	}

	public class SettingsBehaviour : MonoBehaviour
	{
		public static string SavePath
		{
			get
			{
				return Path.Combine(Application.persistentDataPath, "settings.xml");
			}
		}

		public static SettingsBehaviour Instance { get; private set; }

		public string modelImportPath;
		public string sceneOpenPath;
		public string imageExportPath;
		public Vector2 renderSize;

		private SettingsData _settings;
		public SettingsData Settings
		{
			get
			{
				if (_settings == null)
				{
					_settings = SettingsData.Load(SavePath);
				}
				return _settings ?? new SettingsData();
			}
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

		private void OnApplicationQuit()
		{
			SettingsData.Save(SavePath, Settings);
		}
	}
}