using System;

namespace Plugin.SynthV
{
    public class ScaledParam : IParamExpression
    {
        private readonly IParamExpression Expression;

        private readonly double Ratio;

        public ScaledParam(IParamExpression expression, double ratio)
        {
            Expression = expression;
            Ratio = ratio;
        }
        
        public override int ValueAtTicks(int ticks)
        {
            return (int) Math.Round(Ratio * Expression.ValueAtTicks(ticks));
        }
    }
}
