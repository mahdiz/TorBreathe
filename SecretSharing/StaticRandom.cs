using System;
using System.Diagnostics;

namespace SecretSharing
{
	/// <summary>
	/// Only for simulation use. Do not use for real runs.
	/// </summary>
	public static class StaticRandom
	{
		private static Random randGen;

		public static void Init(int seed)
		{
			Debug.WriteLine("StaticRandom is already initialized. Reinitializing...");
			randGen = new Random(seed);
		}

		/// <summary>
		/// Returns a random number within a specified range.
		/// </summary>
		/// <param name="minValue">The inclusive lower bound of the random number returned.</param>
		/// <param name="maxValue">
		/// The exclusive upper bound of the random number returned. maxValue must be
		/// greater than or equal to minValue.
		/// </param>
		/// <returns>
		/// A 32-bit signed integer greater than or equal to minValue and less than maxValue;
		/// that is, the range of return values includes minValue but not maxValue. If
		/// minValue equals maxValue, minValue is returned.
		/// </returns>
		public static int Next(int minValue, int maxValue)
		{
			Debug.Assert(randGen != null, "StaticRandom is not initialized yet.");
			return randGen.Next(minValue, maxValue);
		}

		/// <summary>
		/// Returns a random number within a specified range.
		/// </summary>
		/// <param name="min">The inclusive lower bound.</param>
		/// <param name="max">The exclusive upper bound.</param>
		/// <param name="rand"></param>
		/// <returns>A 64-bit signed integer.</returns>
		public static long Next(long min, long max)
		{
			Debug.Assert(randGen != null, "StaticRandom is not initialized yet.");
			var buf = new byte[8];
			randGen.NextBytes(buf);
			var longRand = BitConverter.ToInt64(buf, 0);
			return Math.Abs(longRand % (max - min)) + min;
		}

		/// <summary>
		/// Returns a random number between 0.0 and 1.0.
		/// </summary>
		public static double NextDouble()
		{
			Debug.Assert(randGen != null, "StaticRandom is not initialized yet.");
			return randGen.NextDouble();
		}
	}
}