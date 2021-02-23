using SmidtFramework.LoggingSystem;
using System;
using UnityEngine;

namespace VisualizingTheDatabike.Services
{
	/// <summary>
	/// Class for converting measured units into in-scene units.
	/// </summary>
	public class UnitConverter
	{
		private double? startPosPhi = null;
		private double? startPosLam = null;
		private double? startPosH = null;

		#region public_functions
		/// <summary>
		/// Converts gps coordinates into ENU local tangent plane coordinates.
		/// All equations were taken from the paper 'Converting GPS coordinates [phi, lambda, h] to navigation coordinates (ENU)' by Samuel Picton Drake from 2002.
		/// Provides all converted locations relative to the first converted one (first converted location = (0,0,0))
		/// </summary>
		/// <param name="gps">GPS coordinates, x = lat, y = ele, z = lon</param>
		/// <returns>In-scene coordinates of the gps location with the first one converted being 0,0,0; x = east, y = up, z = north</returns>
		public Vector3 ConvertGPSToENUScene(Vector3 gps)
		{
			const double degToRad = Math.PI / 180.0;

			//if this is the first location to be converted it will serve as the 0 reference
			if (startPosPhi == null)
			{
				startPosPhi = gps.x * degToRad;
				startPosLam = gps.z * degToRad;
				startPosH = gps.y;
				return Vector3.zero;
			}

			//constants
			const double a = 6378137;
			const double b = 6356752.3142;
			double e2 = 1.0 - Math.Pow(b / a, 2.0);

			double phi = startPosPhi.Value;
			double lam = startPosLam.Value;
			double h = startPosH.Value;

			double dphi = gps.x * degToRad - phi;
			double dlam = gps.z * degToRad - lam;
			double dh = gps.y - h;

			double tmp1 = Math.Sqrt(1 - e2 * Math.Pow(Math.Sin(phi), 2.0));
			double cp = Math.Cos(phi);
			double sp = Math.Sin(phi);

			//extracted repeated Math.Pow calls
			double dphi2 = Math.Pow(dphi, 2.0);
			double dlam2 = Math.Pow(dlam, 2.0);
			double temp13 = Math.Pow(tmp1, 3.0);
			double cp2 = Math.Pow(cp, 2.0);

			double de = ((a / tmp1 + h) * cp * dlam) - ((a * (1 - e2) / temp13 + h) * sp * dphi * dlam) + (cp * dlam * dh);
			double dn = ((a * (1 - e2) / temp13 + h) * dphi) + (1.5 * cp * sp * a * e2 * dphi2) + (Math.Pow(sp, 2.0) * dh * dphi) + (0.5 * sp * cp * (a / tmp1 + h) * dlam2);
			double du = dh - (0.5 * (a - (1.5 * a * e2 * cp2) + (0.5 * a * e2) + h) * dphi2) - (0.5 * cp2 * (a / tmp1 - h) * dlam2);

			return new Vector3(Convert.ToSingle(de), Convert.ToSingle(du), Convert.ToSingle(dn));
		}

		/// <summary>
		/// Converts the measured (absolute) rotation of (x pos = front, y pos = right, z pos = up) to in-scene (absolute) rotation (x pos = right, y pos = up, z pos = front)
		/// </summary>
		/// <param name="rot">Measured absolute rotation.</param>
		/// <returns>The converted in-scene rotation.</returns>
		public Vector3 ConvertAbsRotationToScene(Vector3 rot)
		{
			return new Vector3(rot.y, rot.z + 180, rot.x);
		}

		/// <summary>
		/// Converts a measured additive rotation of (x pos = front, y pos = right, z pos = up) to in-scene additive rotation.
		/// </summary>
		/// <param name="rot">The measured additive rotation.</param>
		/// <returns>The converted in-scene rotation.</returns>
		public Vector3 ConvertAddRotationToScene(Vector3 rot)
		{
			return new Vector3(rot.y, rot.z, rot.x);
		}

		/// <summary>
		/// Computes the global absolute rotation based on the last, the current additive, and optionally the current absolute rotation. Always uses the current absolute rotation over the additive one if it's available.
		/// </summary>
		/// <param name="prevRot">Previous global absolute rotation.</param>
		/// <param name="addRot">Current additive rotation.</param>
		/// <param name="absRot">Current modulo absolute rotation.</param>
		/// <returns>The current global absolute rotation.</returns>
		public Vector3 ComputeGlobalAbsRot(Vector3 prevRot, Vector3 addRot, Vector3? absRot, float errorLimit = 10f)
		{
			if (prevRot == null || addRot == null)
				throw new ArgumentException("prevRot and addRot cannot be null.");

			Vector3 adjRot = ConvertAddRotationToScene(addRot) + prevRot;

			if (absRot != null)
			{
				Vector3 sceneAbsRot = ConvertAbsRotationToScene(absRot.Value);
				Vector3 recGlobalAbsRot = ReconstructGlobalAbsRotFromModuloAbsRot(sceneAbsRot, prevRot);

				if ((recGlobalAbsRot - adjRot).magnitude >= errorLimit)
					LSLogger.Log("UnitConverter", "Large mismatch in additive and absolute rotation - " + adjRot + "/" + recGlobalAbsRot + ".");

				adjRot = recGlobalAbsRot;
			}

			return adjRot;
		}

		/// <summary>
		/// Reconstructs the global absolute rotation from an absolute rotation modulo 360 using the last global absolute rotation under the assumption that the difference between the new and old ones in any axis is less than 180.
		/// </summary>
		/// <param name="newModAbsRot">The new absolute rotation modulo 360.</param>
		/// <param name="prevGlobAbsRot">The previous global absolute rotation.</param>
		/// <returns></returns>
		public Vector3 ReconstructGlobalAbsRotFromModuloAbsRot(Vector3 newModAbsRot, Vector3 prevGlobAbsRot)
		{
			float ConvertSpatialRotation(float modAbsRot, float prevRot)
			{
				int div = (int)(prevRot / 360);
				float rem = prevRot % 360f;
				if (rem < 0) rem += 360f;

				//invert anlge if last rotation was negative
				float rel = (prevRot >= 0) ? modAbsRot : (modAbsRot - 360f);

				if (Mathf.Abs(rem - modAbsRot) <= 180f) //difference < 180 - both within same multiple of 360
					return div * 360f + rel;
				else if (rem > modAbsRot) //difference > 180 and old is larger than new rot - next multiple of 360
					return (div + 1) * 360f + rel;
				else //diff > 180 and old is smaller than new - previous multiple of 360
					return (div - 1) * 360f + rel;
			}

			return new Vector3(ConvertSpatialRotation(newModAbsRot.x, prevGlobAbsRot.x), ConvertSpatialRotation(newModAbsRot.y, prevGlobAbsRot.y), ConvertSpatialRotation(newModAbsRot.z, prevGlobAbsRot.z));
		}

		/// <summary>
		/// Reconstructs the global absolute pedal rotation from the last global absolute rotation, the current absolute rotation modulo 360, and the current rotation direction.
		/// </summary>
		/// <param name="lastRot">The last global absolute rotation.</param>
		/// <param name="modRot">The current absolute rotation modulo 360.</param>
		/// <param name="rotDir">The current rotation direction, 1 = clockwise, -1 = counter-clockwise.</param>
		/// <returns>The current global absolute rotation.</returns>
		public float ReconstructGlobalAbsPedalRot(float lastRot, float modRot, int rotDir)
		{
			int div = (int)(lastRot / 360);
			float rem = lastRot % 360f;
			if (rem < 0) rem += 360f;

			float rel = (lastRot >= 0) ? modRot : (modRot - 360f);

			float res;
			if (rotDir == 1) //clockwise
			{
				if (rem <= modRot) res = div * 360f + rel;
				else res = (div + 1) * 360f + rel;
			}
			else //counter-clockwise
			{
				if (rem < modRot) res = (div - 1) * 360f + rel;
				else res = div * 360f + rel;
			}

			return res;
		}
		#endregion
	}

}

