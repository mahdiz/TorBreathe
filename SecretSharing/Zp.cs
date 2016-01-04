﻿using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace SecretSharing
{
    /// <summary>
    /// Represents a finite field.
    /// </summary>
    public class Zp
	{
		private long num;
		public readonly long Prime;

		public long Value
		{
			get
			{
				return num;
			}

			set
			{
				num = Modulo(value);
			}
		}

		public Zp AdditiveInverse
		{
			get
			{
				return new Zp(Prime, -1 * this.num);
			}
		}

		/// <summary>
		/// Multiplicative inverse of the number modulo Prime.
		/// </summary>
		public Zp MultipInverse
		{
			get
			{
				return new Zp(Prime, NumTheoryUtils.MultiplicativeInverse(this.num, Prime));
			}
		}

		public Zp(long prime, long num)
		{
			this.Prime = prime;
			this.num = Modulo(num, prime);
		}

		public Zp(long prime)
		{
			this.Prime = prime;
		}

		public Zp(Zp toCopy)
		{
			this.Prime = toCopy.Prime;
			this.num = Modulo(toCopy.num, this.Prime);
		}

		public static Zp operator +(Zp zp1, Zp zp2)
		{
			Debug.Assert(zp1.Prime == zp2.Prime, "Finite fields do not match!");
			return new Zp(zp1.Prime, zp1.num + zp2.num);
		}

		public static Zp operator -(Zp zp1, Zp zp2)
		{
			Debug.Assert(zp1.Prime == zp2.Prime, "Finite fields do not match!");
			return new Zp(zp1.Prime, zp1.num - zp2.num);
		}

		public static Zp operator *(Zp zp1, Zp zp2)
		{
			Debug.Assert(zp1.Prime == zp2.Prime, "Finite fields do not match!");
			return new Zp(zp1.Prime, zp1.num * zp2.num);
		}

		public static Zp operator /(Zp zp1, Zp zp2)
		{
			Debug.Assert(zp1.Prime == zp2.Prime, "Finite fields do not match!");
			return zp1 * zp2.MultipInverse;
		}

		public static Zp operator +(Zp zp1, long i)
		{
			return new Zp(zp1.Prime, zp1.num + i);
		}

		public static Zp operator ++(Zp zp1)
		{
			zp1.num = Modulo(zp1.num + 1, zp1.Prime);
			return zp1;
		}

		public static bool operator <(Zp zp1, long i)
		{
			return zp1.num < i;
		}

		public static bool operator >(Zp zp1, long i)
		{
			return zp1.num > i;
		}

		public static bool operator ==(Zp zp1, Zp zp2)
		{
			if (Object.ReferenceEquals(zp1, zp2))
				return true;

			if (((object)zp1 == null) || ((object)zp2 == null))
				return false;

			return zp1.num == zp2.num;
		}

		public static bool operator !=(Zp zp1, Zp zp2)
		{
			return !(zp1 == zp2);
		}

		public Zp Add(Zp operand)
		{
			num = Modulo(num + operand.Value);
			return this;
		}

		public Zp ConstAdd(Zp operand)
		{
			var temp = new Zp(this);
			temp.num = Modulo(num + operand.Value);
			return temp;
		}

		public Zp AddListContent(IList<Zp> zpList)
		{
			foreach (Zp zp in zpList)
				Add(zp);

			return this;
		}

		public Zp Sub(Zp operand)
		{
			num = Modulo(num - operand.Value);
			return this;
		}

		public Zp ConstSub(Zp operand)
		{
			var temp = new Zp(this);
			temp.num = Modulo(num - operand.Value);
			return temp;
		}

		public Zp Div(Zp operand)
		{
			if (operand.num == 0)
				throw new ArgumentException("Cannot divide by zero!");

			return this * operand.MultipInverse;
		}

		public Zp ConstDvide(Zp operand)
		{
			if (operand.num == 0)
				throw new System.ArgumentException("Cannot divide by zero!");

			return this * operand.MultipInverse;
		}

		public Zp Mul(Zp operand)
		{
			num = Modulo(num * operand.Value);
			return this;
		}

		public Zp ConstMul(Zp operand)
		{
			var temp = new Zp(this);
			temp.num = Modulo(num * operand.Value);
			return temp;
		}

		public Zp Calculate(Zp operand, Operation operation)
		{
			switch (operation)
			{
				case Operation.Add:
					return Add(operand);
				case Operation.Mul:
					return Mul(operand);
				case Operation.Sub:
					return Sub(operand);
				case Operation.Div:
					return Div(operand);
				default:
					throw new Exception("Unknown operation: " + operation);
			}
		}

		public long Modulo(long i)
		{
			return Modulo(i, Prime);
		}

		public static long Modulo(long n, long prime)
		{
			n = n % prime;
			return n < 0 ? n + prime : n;
		}

		public static Zp EvalutePolynomialAtPoint(IList<Zp> polynomial, Zp point)
		{
			long evaluation = 0;
			for (int i = 0; i < polynomial.Count; i++)
			{
				evaluation += polynomial[i].Value * NumTheoryUtils.ModPow(point.Value, i, point.Prime);
			}
			return new Zp(point.Prime, evaluation);
		}

		public override string ToString()
		{
			return Convert.ToString(num);
		}

		public override bool Equals(object obj)
		{
			if (!(obj is Zp))
				return false;
			Zp compare = (Zp)obj;
			return Value == compare.Value && Prime == compare.Prime;
		}

		public override int GetHashCode()
		{
			return Value.GetHashCode();
		}

		public int Size
		{
			get { return sizeof(long); }
		}
	}
}