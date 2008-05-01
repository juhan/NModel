//-------------------------------------
// Copyright (c) Microsoft Corporation
//-------------------------------------
using System;
using System.Collections.Generic;
using System.Text;
using NModel.Internals;

namespace NModel
{
    /// <summary>
    /// Static class providing functions from mathematical combinatorics.
    /// </summary>
    public static class Combinatorics
    {
        /// <summary>
        /// The factorial of <paramref name="i"/>, <paramref name="i"/>!. This is equal to the number
        /// of permutations without repetition of a sequence of <paramref name="i"/> elements.
        /// </summary>
        /// <param name="i">The number of elements</param>
        /// <returns>The number of nonrepeating permutations</returns>
        /// <exception cref="ArgumentException">Thrown if <paramref name="i"/> is less than zero or greater than 12. </exception>
        public static int Factorial(int i)
        {
            if (!(0 <= i && i <= 12)) throw new ArgumentException(MessageStrings.FactorialArgumentOutOfRange);
            int j = 1;
            for (int k = i; k > 0; k -= 1) j *= k;
            return j;
        }

        /// <summary>
        /// Chooses an arbitrary ordering of the sequence [0, 1, ... <paramref name="n"/> - 1] using <paramref name="i"/> to choose.
        /// </summary>
        /// <param name="n">The number of elements to permute</param>
        /// <param name="i">The index of the permutation. If this is a nonnegative number less than 
        /// Factorial(<paramref name="n"/>) and <paramref name="n"/> &lt;= 12, then the permutations are enumerated. In other words,
        /// each of the possible permutations will correspond to exactly one integer in [0..Factorial(n)). Outside
        /// of that range, the permutation will be chosen using the given integer as a random seed.</param>
        /// <returns>An array of indices between 0 and <paramref name="n"/> in a chosen order.</returns>
        public static int[] ChoosePermutation(int n, int i)
        {
            // for case of small numbers, use enumerated choice
            if (0 <= n && n <= 12 && 0 <= i && i < Combinatorics.Factorial(n))
                return ChoosePermutation_1(n, i);

            if (n < 0) throw new ArgumentException("NModel.Combinatorics.ChoosePermutation: Parameter may not be negative", "n");
            
            // otherwise use i as a random seed
            int[] result = new int[n];
            Set<int> indicesToChoose = Set<int>.EmptySet;
            for (int k = 0; k < n; k += 1)
                indicesToChoose = indicesToChoose.Add(k);

            int choice = TypedHash<int>.ComputeHash(n, i);

            for (int l = 0; l < n; l += 1)
            {
                //^ assume indicesToChoose.Count > 0;
                int currentChoice = indicesToChoose.Choose(Math.Abs(choice) % indicesToChoose.Count);
                result[l] = currentChoice;
                indicesToChoose = indicesToChoose.Remove(currentChoice);
                choice = TypedHash<int>.ComputeHash(choice, l);
            }
            return result;
        }

        /// <summary>        /// Chooses an arbitrary ordering of the sequence [0, 1, ... <paramref name="n"/> - 1] using <paramref name="i"/> to choose.
        /// </summary>
        /// <param name="n">The number of elements to permute</param>
        /// <param name="i">The index of the permutation. This should be a nonnegative number less than 
        /// Factorial(<paramref name="n"/>).</param>
        /// <returns>An array of indices between 0 and <paramref name="n"/> in an arbitrary order.</returns>
        /// <exception cref="ArgumentException">Thrown if <paramref name="n"/>is out of range. The current 
        /// implementation allows 
        /// permutation of up to 16 elements.</exception>
        /// <exception cref="ArgumentException">Thorwn if <paramref name="i"/>is not a nonnegative integer 
        /// less than Factorial(<paramref name="n"/>).</exception>
        static int[] ChoosePermutation_1(int n, int i)
        //^ requires 0 <= n && n <= 12 && 0 <= i && i < Combinatorics.Factorial(n);
        {
            // if (!(0 <= n && n <= 12)) throw new ArgumentException("ChoosePermutation: out of range. Must be between 0 and 16.");
            // if (!(0 <= i && i < Combinatorics.Factorial(n))) throw new ArgumentException("ChoosePermutation: out of range. Must be in [0, Combinatorics.Factorial(16)).");

            int[] result = new int[n];
            Set<int> indicesToChoose = Set<int>.EmptySet;
            for (int k = 0; k < n; k += 1)
                indicesToChoose = indicesToChoose.Add(k);

            int choice = i;

            for (int l = n - 1; l >= 0; l -= 1)
            {
                int f = Combinatorics.Factorial(l);
                int currentChoiceIndex = choice / f;
                int currentChoice = indicesToChoose.Choose(currentChoiceIndex);
                result[l] = currentChoice;
                indicesToChoose = indicesToChoose.Remove(currentChoice);
                choice = choice % f;
            }
            return result;
        }

    }
}
