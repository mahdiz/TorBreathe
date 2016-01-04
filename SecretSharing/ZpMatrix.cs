using System;
using System.Collections.Generic;

namespace SecretSharing
{
    public class ZpMatrix
	{
		public long Prime { get; private set; }

		public int RowCount { get; private set; }

		public int ColCount { get; private set; }

		public long[][] Data
		{
			get
			{
				return data;
			}
		}

		private long[][] data;

		public Zp this[int i, int j]
		{
			get
			{
				return new Zp(Prime, data[i][j]);
			}
			set
			{
				data[i][j] = value.Value;
			}
		}

		// TODO: Mahdi: Incorrect code! Works only if the matrix is triangular.
		// Should choose a fast algorithm to find determinant.
		private bool Nonsingular
		{
			get
			{
				for (int i = 0; i < RowCount; i++)
				{
					if (data[i][i] == 0)
						return false;
				}
				return true;
			}
		}

		/// <summary>
		/// Creates M-by-N matrix of zero initialized elements.
		/// </summary>
		public ZpMatrix(int rowNum, int colNum, long prime)
		{
			RowCount = rowNum;
			ColCount = colNum;
			Prime = prime;
			data = initMatrix<long>(rowNum, colNum);
		}

		/// <summary>
		/// Creates matrix based on 2d array of integers.
		/// </summary>
		public ZpMatrix(long[][] data, long prime)
		{
			RowCount = data.Length;
			ColCount = data[0].Length;
			Prime = prime;
			this.data = initMatrix<long>(RowCount, ColCount);

			for (int i = 0; i < RowCount; i++)
			{
				for (int j = 0; j < ColCount; j++)
					this.data[i][j] = Modulo(data[i][j]);
			}
		}

		/// <summary>
		/// Creates matrix based on 2d array of integers.
		/// </summary>
		public ZpMatrix(long[,] data, long prime)
		{
			RowCount = data.GetLength(0);
			ColCount = data.GetLength(1);
			Prime = prime;
			this.data = initMatrix<long>(RowCount, ColCount);

			for (int i = 0; i < RowCount; i++)
			{
				for (int j = 0; j < ColCount; j++)
					this.data[i][j] = Modulo(data[i,j]);
			}
		}

		/// <summary>
		/// Creates a  vector matrix from Zp array.
		/// </summary>
		public ZpMatrix(Zp[] vector, VectorType vec_type)
		{
			Prime = vector[0].Prime;
			if (vec_type.Equals(VectorType.Row))
			{
				RowCount = 1;
				ColCount = vector.Length;
				data = initMatrix<long>(RowCount, ColCount);
				for (int j = 0; j < ColCount; j++)
					data[0][j] = vector[j].Value;
			}
			else	// VectorType.COLOMN_VECTOR
			{
				RowCount = vector.Length;
				ColCount = 1;
				data = initMatrix<long>(RowCount, ColCount);
				for (int i = 0; i < RowCount; i++)
					data[i][0] = vector[i].Value;
			}
		}

		private ZpMatrix(ZpMatrix A)
			: this(A.data, A.Prime)
		{
		}

		private T[][] initMatrix<T>(int r, int c)
		{
			var array = new T[r][];
			for (int i = 0; i < r; i++)
				array[i] = new T[c];
			return array;
		}

		public Zp[] ZpVector
		{
			get
			{
				Zp[] vector = null;
				if (RowCount == 1)
				{
					vector = new Zp[ColCount];
					for (int j = 0; j < ColCount; j++)
						vector[j] = new Zp(Prime, data[0][j]);
				}
				else if (ColCount == 1)
				{
					vector = new Zp[RowCount];
					for (int i = 0; i < RowCount; i++)
						vector[i] = new Zp(Prime, data[i][0]);
				}
				return vector;
			}
		}

		public IList<Zp> GetMatrixRow(int rowNumber)
		{
			if (RowCount <= rowNumber)
				throw new ArgumentException("Illegal  matrix  row number.");

			var wantedRow = new List<Zp>();
			for (int j = 0; j < ColCount; j++)
				wantedRow.Add(new Zp(Prime, data[rowNumber][j]));

			return wantedRow;
		}

		public ZpMatrix Transpose
		{
			get
			{
				var A = new ZpMatrix(ColCount, RowCount, Prime);
				for (int i = 0; i < RowCount; i++)
				{
					for (int j = 0; j < ColCount; j++)
						A.data[j][i] = data[i][j];
				}
				return A;
			}
		}

		public ZpMatrix Plus(ZpMatrix B)
		{
			var A = this;
			if ((B.RowCount != A.RowCount) || (B.ColCount != A.ColCount))
				throw new ArgumentException("Illegal  matrix  dimensions.");

			if (A.Prime != B.Prime)
				throw new ArgumentException("Trying to add Matrix  from different fields.");

			var C = new ZpMatrix(RowCount, ColCount, A.Prime);
			for (int i = 0; i < RowCount; i++)
			{
				for (int j = 0; j < ColCount; j++)
					C.data[i][j] = Modulo(A.data[i][j] + B.data[i][j]);
			}
			return C;
		}

		public ZpMatrix Times(ZpMatrix B)
		{
			var A = this;
			if (A.ColCount != B.RowCount)
				throw new ArgumentException("Illegal matrix dimensions.");

			if (A.Prime != B.Prime)
				throw new ArgumentException("Matrix  from different fields.");

			// create initialized matrix (zero value to all elements)
			var C = new ZpMatrix(A.RowCount, B.ColCount, A.Prime);
			for (int i = 0; i < C.RowCount; i++)
			{
				for (int j = 0; j < C.ColCount; j++)
					for (int k = 0; k < A.ColCount; k++)
						C.data[i][j] = Modulo(C.data[i][j] + A.data[i][k] * B.data[k][j]);
			}
			return C;
		}

		public bool Eq(ZpMatrix B)
		{
			var A = this;
			if ((B.RowCount != A.RowCount) || (B.ColCount != A.ColCount) || (A.Prime != B.Prime))
				return false;

			for (int i = 0; i < RowCount; i++)
			{
				for (int j = 0; j < ColCount; j++)
				{
					if (A.data[i][j] != B.data[i][j])
						return false;
				}
			}
			return true;
		}

		public static ZpMatrix operator +(ZpMatrix m1, ZpMatrix m2)
		{
			return (new ZpMatrix(m1)).Plus(m2);
		}

		public static ZpMatrix operator *(ZpMatrix m1, ZpMatrix m2)
		{
			return (new ZpMatrix(m1)).Times(m2);
		}

		/* r  -   Array of row indices,  j0 -   Initial column index,  j1 -   Final column index
			return     A(r(:),j0:j1)  */
		private ZpMatrix GetSubMatrix(int[] r, int j0, int j1)
		{
			var X = new ZpMatrix(r.Length, j1 - j0 + 1, Prime);
			var B = X.data;

			for (int i = 0; i < r.Length; i++)
			{
				for (int j = j0; j <= j1; j++)
					B[i][j - j0] = data[r[i]][j];
			}
			return X;
		}

		/* swap rows i and j in the matrix*/

		private void SwapRows(int i, int j)
		{
			var temp = data[i];
			data[i] = data[j];
			data[j] = temp;
		}

		/* calculate i mod prime */

		private long Modulo(long i)
		{
			return Zp.Modulo(i, Prime);
		}

		/// <summary>
		/// Creates and return a random rowNum-by-colNum  matrix with values between '0' and 'prime-1'.
		/// </summary>
		public static ZpMatrix GetRandomMatrix(int rowNum, int colNum, long prime)
		{
			var A = new ZpMatrix(rowNum, colNum, prime);
			for (int i = 0; i < rowNum; i++)
			{
				for (int j = 0; j < colNum; j++)
					A.data[i][j] = Zp.Modulo((long)(StaticRandom.NextDouble() * (prime)), prime);
			}
			return A;
		}

		/// <summary>
		/// Create and return the N-by-N identity matrix.
		/// </summary>
		public static ZpMatrix GetIdentityMatrix(int matrixSize, int prime)
		{
			var I = new ZpMatrix(matrixSize, matrixSize, prime);
			for (int i = 0; i < matrixSize; i++)
				I.data[i][i] = 1;

			return I;
		}

		/* Create and return N-by-N  matrix that its first "trucToSize" elements in
		  its diagonal is "1" and the rest of the matrix is "0"*/

		public static ZpMatrix GetTruncationMatrix(int matrixSize, int truncToSize, int prime)
		{
			var I = new ZpMatrix(matrixSize, matrixSize, prime);
			for (int i = 0; i < truncToSize; i++)
				I.data[i][i] = 1;

			return I;
		}

		public static ZpMatrix GetVandermondeMatrix(int rowNum, int colNum, long prime)
		{
			var A = new ZpMatrix(rowNum, colNum, prime);

			for (int j = 0; j < colNum; j++)
				A.data[0][j] = 1;

			if (rowNum == 1)
				return A;

			for (int j = 0; j < colNum; j++)
				A.data[1][j] = j + 1;

			for (int j = 0; j < colNum; j++)
			{
				for (int i = 2; i < rowNum; i++)
					A.data[i][j] = Zp.Modulo(A.data[i - 1][j] * A.data[1][j], prime);
			}
			return A;
		}

		public static ZpMatrix GetVandermondeMatrix(int rowNum, IList<Zp> values, int prime)
		{
			int colNum = values.Count;
			var A = new ZpMatrix(rowNum, colNum, prime);

			for (int j = 0; j < colNum; j++)
				A.data[0][j] = 1;

			if (rowNum == 1)
				return A;

			for (int j = 0; j < colNum; j++)
			{
				for (int i = 1; i < rowNum; i++)
					A.data[i][j] = Zp.Modulo(A.data[i - 1][j] * values[j].Value, prime);
			}
			return A;
		}

		public static ZpMatrix GetSymmetricVanderMondeMatrix(int matrixSize, int prime)
		{
			return GetVandermondeMatrix(matrixSize, matrixSize, prime);
		}

		/// <summary>
		/// Returns a Vandermonde matrix in the field (each element is modulu prime).
		/// </summary>
		public static ZpMatrix GetShamirRecombineMatrix(int matrixSize, int prime)
		{
			var A = new ZpMatrix(matrixSize, matrixSize, prime);
			if (matrixSize == 1)
			{
				A.data[0][0] = 1;
				return A;
			}

			for (int i = 0; i < matrixSize; i++)
				A.data[i][0] = 1;

			for (int i = 0; i < matrixSize; i++)
				A.data[i][1] = i + 1;

			for (int i = 0; i < matrixSize; i++)
			{
				for (int j = 2; j < matrixSize; j++)
					A.data[i][j] = Zp.Modulo(A.data[i][j - 1] * A.data[i][1], prime);
			}
			return A;
		}

		public static ZpMatrix GetPrimitiveVandermondeMatrix(int rowNum, int colNum, long prime)
		{
			int primitive = NumTheoryUtils.GetFieldMinimumPrimitive(prime);
			if (primitive == 0)
				throw new ArgumentException("Cannot create a primitive Vandermonde matrix from a non-prime number. ");

			var A = new ZpMatrix(rowNum, colNum, prime);

			for (int j = 0; j < colNum; j++)
				A.data[0][j] = 1;

			if (rowNum == 1)
				return A;

			/*  This variable represents  primitive^j  for the j-th player*/
			long primitive_j = 1;
			for (int j = 0; j < colNum; j++)
			{
				A.data[1][j] = primitive_j;
				primitive_j = Zp.Modulo(primitive_j * primitive, prime);
			}

			for (int j = 0; j < colNum; j++)
			{
				for (int i = 2; i < rowNum; i++)
					A.data[i][j] = Zp.Modulo(A.data[i - 1][j] * A.data[1][j], prime);
			}

			return A;
		}

		public static ZpMatrix GetSymmetricPrimitiveVandermondeMatrix(int matrixSize, int prime)
		{
			return GetPrimitiveVandermondeMatrix(matrixSize, matrixSize, prime);
		}

        public void SetMatrixCell(int rowNumber, int colNumber, Zp value)
        {
            if ((RowCount <= rowNumber) || (ColCount <= colNumber))
                throw new ArgumentException("Illegal matrix cell.");

            data[rowNumber][colNumber] = value.Value;
        }

        /// <summary>
        /// Print matrix to standard output.
        /// </summary>
        public void Print()
		{
			for (int i = 0; i < RowCount; i++)
			{
				for (int j = 0; j < ColCount; j++)
				{
					Console.Write(data[i][j] + "   ");
				}
				Console.WriteLine();
			}
			Console.WriteLine();
		}
	}
}