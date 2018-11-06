using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PmxSharp
{
	public static class PmxConstants
	{
		/// <summary>
		/// The 4-byte PMX header signature. ASCII representation is "PMX ".
		/// </summary>
		public static byte[] PmxSignature { get { return _pmxSignature; } }
		private static byte[] _pmxSignature = new byte[] { 0x50, 0x4d, 0x58, 0x20 };

		/// <summary>
		/// The size of the PMX signature in bytes.
		/// </summary>
		public static int SignatureLength { get { return 4; } }

		/// <summary>
		/// Checks whether the PMX file's 4-byte signature is valid.
		/// </summary>
		/// <returns>A boolean indicating whether the signature is valid.</returns>
		public static bool ValidatePmxSignature(this byte[] sig)
		{
			if (sig.Length < 4)
				return false;

			// 50-4D-58-20
			if (sig[0] != PmxSignature[0])
				return false;
			if (sig[1] != PmxSignature[1])
				return false;
			if (sig[2] != PmxSignature[2])
				return false;
			if (sig[3] != PmxSignature[3])
				return false;
			return true;
		}

		/// <summary>
		/// Checks whether the PMD file's 3-byte signature is valid.
		/// </summary>
		/// <returns>A boolean indicating whether the signature is valid.</returns>
		public static bool ValidatePmdSignature(this byte[] sig)
		{
			if (sig.Length < 3)
				return false;
			else throw new NotImplementedException("Legacy PMD is currently not supported.");
		}
	}
}
