using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace MUPS
{
	[Serializable]
	public struct MyQuaternion
	{
		public float x, y, z, w;
		public MyQuaternion(Quaternion src)
		{
			x = src.x;
			y = src.y;
			z = src.z;
			w = src.w;
		}
		public Quaternion GetUnityQuaternion()
		{
			return new Quaternion(x, y, z, w);
		}
		public MyVector3 GetEuler()
		{
			return new MyVector3(GetUnityQuaternion().eulerAngles);
		}
	}

	[Serializable]
	public struct MyVector3
	{
		public float x, y, z;
		public MyVector3(Vector3 src)
		{
			x = src.x;
			y = src.y;
			z = src.z;
		}
		public Vector3 GetUnityVector3()
		{
			return new Vector3(x, y, z);
		}
	}

	[Serializable]
	public struct MyVector2
	{
		public float x, y;
		public MyVector2(Vector2 src)
		{
			x = src.x;
			y = src.y;
		}
		public Vector2 GetUnityVector2()
		{
			return new Vector2(x, y);
		}
	}


	public static class Math
	{
		/// <summary>
		/// Euler angles from azimuth and elevation (degrees).
		/// </summary>
		public static Vector3 EulerFromAzEl(float azimuth, float elevation)
		{
			float az = azimuth * Mathf.Deg2Rad;
			float el = elevation * Mathf.Deg2Rad;
			return new Vector3(Mathf.Sin(az) * Mathf.Cos(el), Mathf.Cos(az) * Mathf.Cos(el), Mathf.Sin(el));
		}

		/// <summary>
		/// Quaternion from azimuth and elevation (degrees).
		/// </summary>
		public static Quaternion QuaternionFromAzEl(float azimuth, float elevation)
		{
			return Quaternion.Euler(EulerFromAzEl(azimuth, elevation));
		}
	}
}
