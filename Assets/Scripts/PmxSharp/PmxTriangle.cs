using System.Collections.Generic;

namespace PmxSharp
{
	/// <summary>
	/// Represents a triangle's three points.
	/// </summary>
	public class PmxTriangle
	{
		/// <summary>
		/// The array of vertices in the triangle.
		/// </summary>
		public int[] Vertices { get; set; }

		/// <summary>
		/// Create an empty triangle.
		/// </summary>
		public PmxTriangle()
		{
			Vertices = new int[3];
		}

		/// <summary>
		/// Create a triangle between the specified vertices.
		/// </summary>
		/// <param name="v1">The first vertex.</param>
		/// <param name="v2">The second vertex.</param>
		/// <param name="v3">The third vertex.</param>
		/// <param name="ccw">Flips the triangle orientation. Use false (clockwise) for DirectX, and true (counter-clockwise) for OpenGL applications.</param>
		public PmxTriangle(int v1, int v2, int v3, bool ccw = false)
		{
			if (ccw)
				Vertices = new int[3] { v3, v2, v1 };
			else
				Vertices = new int[3] { v1, v2, v3 };
		}

		/// <summary>
		/// The triangle's first vertex.
		/// </summary>
		public int Vertex1
		{
			get
			{
				return Vertices[0];
			}
			set
			{
				if (Vertices == null)
				{
					Vertices = new int[3];
				}

				Vertices[0] = value;
			}
		}
		/// <summary>
		/// The triangle's second vertex.
		/// </summary>
		public int Vertex2
		{
			get
			{
				return Vertices[1];
			}
			set
			{
				if (Vertices == null)
				{
					Vertices = new int[3];
				}

				Vertices[1] = value;
			}
		}
		/// <summary>
		/// The triangle's third vertex.
		/// </summary>
		public int Vertex3
		{
			get
			{
				return Vertices[2];
			}
			set
			{
				if (Vertices == null)
				{
					Vertices = new int[3];
				}

				Vertices[2] = value;
			}
		}

		/// <summary>
		/// Switch between clockwise and counterclockwise vertex order.
		/// </summary>
		public void Turn()
		{
			int temp = Vertex2;
			Vertex2 = Vertex3;
			Vertex3 = temp;
		}

		/// <summary>
		/// Return a list of vertex indices where every three items represent a single triangle.
		/// </summary>
		/// <param name="coll"></param>
		/// <returns></returns>
		public static List<int> GetVertices(IEnumerable<PmxTriangle> coll)
		{
			List<int> output = new List<int>();
			foreach (PmxTriangle tri in coll)
			{
				output.Add(tri.Vertex1);
				output.Add(tri.Vertex2);
				output.Add(tri.Vertex3);
			}
			return output;
		}
	}
}
