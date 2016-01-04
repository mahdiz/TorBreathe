using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecretSharing
{
    /// <summary>
    /// Implemenets the threshold secret sharing scheme based on Adi Shamir's method.
    /// </summary>
    public static class ShamirSharing
    {
        /// <summary>
        /// Calculates the shares of a secret with polynomial of degree t and numPlayers players.
        /// </summary>
        public static IList<Zp> Share(Zp secret, int numPlayers, int polyDeg)
        {
            IList<Zp> coeffs;
            return Share(secret, numPlayers, polyDeg, false, out coeffs);
        }

        /// <summary>
        /// Calculates the shares of a secret with polynomial of degree t and numPlayers players.
        /// The method also returns the array of coefficients of the polynomial.
        /// </summary>
        public static IList<Zp> Share(Zp secret, int numPlayers, int polyDeg, out IList<Zp> coeffs)
        {
            return Share(secret, numPlayers, polyDeg, false, out coeffs);
        }

        /// <summary>
        /// Evaluates the shares of secret with polynomial of degree 'polynomDeg' and 'numPlayers' players.
        /// </summary>
        private static IList<Zp> Share(Zp secret, int numPlayers, int polynomDeg, bool usePrimitiveShare, out IList<Zp> coeffs)
        {
            Debug.Assert(numPlayers > polynomDeg, "Polynomial degree cannot be greater than or equal to the number of players!");

            // Create a random polynomial - f(x)
            // Note: Polynomial of degree d has d+1 coefficients
            var randomMatrix = ZpMatrix.GetRandomMatrix(1, polynomDeg + 1, secret.Prime);

            // The free variable in the Random Polynomial (i.e.	f(x)) is the secret
            randomMatrix.SetMatrixCell(0, 0, secret);

            // Polynomial coefficients
            coeffs = randomMatrix.GetMatrixRow(0);

            // Create vanderMonde matrix
            ZpMatrix vanderMonde;
            if (usePrimitiveShare)
                vanderMonde = ZpMatrix.GetPrimitiveVandermondeMatrix(polynomDeg + 1, numPlayers, secret.Prime);
            else
                vanderMonde = ZpMatrix.GetVandermondeMatrix(polynomDeg + 1, numPlayers, secret.Prime);

            // Compute f(i) for the i-th  player
            var sharesArr = randomMatrix.Times(vanderMonde).ZpVector;

            Debug.Assert(sharesArr.Length == numPlayers);
            return sharesArr;
        }

        public static Zp Reconstruct(IList<Zp> sharedSecrets, int polyDeg, int prime)
        {
            if (sharedSecrets.Count < polyDeg)
                throw new System.ArgumentException("Polynomial degree cannot be bigger or equal to the number of  shares");

            // find Lagrange basis polynomials free coefficients
            var L = new Zp[polyDeg + 1];
            for (int i = 0; i < polyDeg + 1; i++)
                L[i] = new Zp(prime, 1);

            int ix = 0;
            for (var i = new Zp(prime, 1); i < polyDeg + 2; i++, ix++)
            {
                for (var j = new Zp(prime, 1); j < polyDeg + 2; j++)
                {
                    if (j != i)
                    {
                        var additiveInverse = j.AdditiveInverse;
                        L[ix] = L[ix] * (additiveInverse / (i + additiveInverse));      // note: division done in finite-field
                    }
                }
            }

            // find the secret by multiplying each share to the corresponding Lagrange's free coefficient
            var secret = new Zp(prime, 0);
            for (int i = 0; i < polyDeg + 1; i++)
                secret = secret + (L[i] * sharedSecrets[i]);

            return secret;
        }
    }
}
