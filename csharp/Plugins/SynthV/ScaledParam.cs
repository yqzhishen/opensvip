using System;

namespace SynthV.Param
{
    public class ScaledParam : ParamExpression
    {
        private readonly ParamExpression Expression;

        private readonly double Ratio;

        public ScaledParam(ParamExpression expression, double ratio)
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
