using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace SecretSharing
{
	/// <summary>
	/// Implements some number-theoretic utility functions.
	/// </summary>
	public static class NumTheoryUtils
	{
		/// <summary>
		/// Performs the Extended Euclidean algorithm and returns gcd(a,b).
		/// The function also returns integers x and y such that ax + by = gcd(a,b).
		/// If gcd(a,b) = 1, then x is a multiplicative inverse of "a mod b" and
		/// y is a multiplicative inverse of "b mod a".
		/// </summary>
		/// <returns>
		/// An array of three integers:
		/// The first element is gcd(a,b).
		/// x is an inverse of a mod b.
		/// y is an inverse of b mod a.
		/// </returns>
		public static int ExtendedEuclidean(int a, int b, out int x, out int y)
		{
			long xl = 0;
			long yl = 0;
			var r = (int)ExtendedEuclidean((long)a, (long)b, out xl, out yl);
			x = (int)xl;
			y = (int)yl;
			return r;
		}

		/// <summary>
		/// Performs the Extended Euclidean algorithm and returns gcd(a,b).
		/// The function also returns integers x and y such that ax + by = gcd(a,b).
		/// If gcd(a,b) = 1, then x is a multiplicative inverse of "a mod b" and
		/// y is a multiplicative inverse of "b mod a".
		/// </summary>
		/// <returns>
		/// An array of three integers:
		/// The first element is gcd(a,b).
		/// x is an inverse of a mod b.
		/// y is an inverse of b mod a.
		/// </returns>
		public static long ExtendedEuclidean(long a, long b, out long x, out long y)
		{
			if (b == 0)
			{
				x = 1;
				y = 0;
				return a;
			}
			else
			{
				var g = ExtendedEuclidean(b, a % b, out x, out y);
				var tmp = y;
				y = x - a / b * y;
				x = tmp;
				return g;
			}
		}

		/// <summary>
		/// Returns the multiplicative inverse of a modulo m.
		/// </summary>
		public static int MultiplicativeInverse(int a, int m)
		{
			return (int)MultiplicativeInverse((long)a, (long)m);
		}

		/// <summary>
		/// Returns the multiplicative inverse of a modulo m.
		/// </summary>
		public static long MultiplicativeInverse(long a, long m)
		{
			long x = 0, y = 0;
			ExtendedEuclidean(a, m, out x, out y);
			return x < 0 ? x + m : x;
		}

		/// <summary>
		/// Performs a O(sqrt(|n|)) primality test, where |n| is the bit-length of the input.
		/// </summary>
		public static bool IsPrime(int n)
		{
			int temp;
			for (int i = 2; i <= (int)Math.Sqrt(n); i++)
			{
				temp = n / i;
				if (n == (temp * i))
					return false;
			}
			return true;
		}

		public static int GetFieldMinimumPrimitive(long prime)
		{
			long w_i;
			bool cond;
			var fieldElements = new bool[prime];

			for (int p = 2; p < prime; p++)
			{
				w_i = 1;
				cond = true;
				for (int i = 1; i < prime; i++)
				{
					fieldElements[i] = false;
				}

				for (int i = 1; i < prime; i++)
				{
					var v = (p * w_i) % prime;
					w_i = v < 0 ? v + prime : v;
					if (fieldElements[w_i])
					{
						cond = false;
						break;
					}
					fieldElements[w_i] = true;
				}
				if (cond)
					return p;
			}
			throw new ArgumentException("Cannot find field primitive for a field from a non-prime number.");
		}

		/// <summary>
		/// Performs modular exponentiation.
		/// </summary>
		public static long ModPow(long powerBase, int exp, long prime)
		{
			long p = 1;
			for (int i = 0; i < exp; i++)
			{
				var v = (powerBase * p) % prime;
				p = v < 0 ? v + prime : v;
			}
			return p;
		}

		/// <summary>
		/// Returns the actual number of bits of a byte array by ignoring trailing zeros in the binary representation.
		/// </summary>
		public static int GetBitLength(byte[] a)
		{
			bool done = false;
			int k = 0;
			for (int i = a.Length - 1; i >= 0; i--)
			{
				var b = a[i];
				for (int j = 0; j < 8; j++)
				{
					if ((b & 1) == 1)
					{
						done = true;
						break;
					}

					b = (byte)(b >> 1);
					k++;
				}
				if (done)
					break;
			}
			return (a.Length * 8) - k;
		}
	}
}
