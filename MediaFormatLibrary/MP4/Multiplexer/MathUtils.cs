using System;
using System.Collections.Generic;
using System.Text;

namespace Utilities
{
    public class MathUtils
    {
        public static long DetermineGreatestCommonDivisor(long a, long b)
        {
            while (b != 0)
            {
                long temp = b;
                b = a % b;
                a = temp;
            }
            return a;
        }

        public static long DetermineLeastCommonMultiple(long a, long b)
        {
            return (a / DetermineGreatestCommonDivisor(a, b)) * b;
        }
    }
}
