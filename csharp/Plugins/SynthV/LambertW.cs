using System;

namespace SynthV.Param
{
    /// <summary>
    /// 使用牛顿法估算 LambertW 函数。<br/>
    /// 作者：Martin Chloride<br/>
    /// 来源：http://martin1994.sinaapp.com/archives/886
    /// </summary>
    public static class LambertW
    {
        public static double Evaluate(double x, int branch = 0)
        {
            if (x < -1 / Math.E)
            {
                throw new ArgumentOutOfRangeException
                    (nameof(x), "x should be larger than -1/e.");
            }
            if (branch != 0 && branch != -1)
            {
                throw new ArgumentOutOfRangeException
                    (nameof(branch), "branch should be 0 or -1.");
            }
            if (x > 0 && branch == -1)
            {
                throw new ArgumentOutOfRangeException(nameof(x), "Only branch 0 has positive input.");
            }
            
            // Start with an estimation
            var result = Estimate(x, branch);
 
            // Iterate using Newton's method
            var loopsRemaining = 100;
            while (TestResult(result, x) && loopsRemaining > 0)
            {
                var x0 = result;
                var y0 = result * Math.Exp(result);
                var k = Math.Exp(x0) * (1 + x0);
                var x1 = (x - y0) / k + x0;
                result = x1;
                loopsRemaining--;
            }
            if (loopsRemaining == 0)
            {
                throw new Exception("Cannot find solution.");
            }
            return result;
        }
        
        private static double Estimate(double x, int branch = 0)
        {
            switch (branch)
            {
                case -1 when x > -0.2706706:
                    return -2;
                case -1:
                    return -1 - Math.Acos(-4.11727 * x - 0.514659);
                case 0:
                    return -1 + Math.Log(1 / Math.E + Math.E + x);
                default:
                    throw new ArgumentException();
            }
        }
        
        private static bool TestResult(double x, double y)
        {
            var fx = x * Math.Exp(x);
            var delta = fx - y;
            var deltaBits = Math.Log(Math.Abs(y / delta), 2);
            return deltaBits < 42;
        }
    }
}