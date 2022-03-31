using System;

namespace Plugin.SynthV
{
    public enum ParamOperators
    {
        Add, Sub, Mul, Div
    }
    
    public class CompoundParam : IParamExpression
    {
        private readonly IParamExpression Expr1;

        private readonly IParamExpression Expr2;

        private readonly ParamOperators Operator;

        public CompoundParam(IParamExpression expr1, ParamOperators op, IParamExpression expr2)
        {
            Expr1 = expr1;
            Operator = op;
            Expr2 = expr2;
        }

        public override int ValueAtTicks(int ticks)
        {
            switch (Operator)
            {
                case ParamOperators.Add:
                    return Expr1.ValueAtTicks(ticks) + Expr2.ValueAtTicks(ticks);
                case ParamOperators.Sub:
                    return Expr1.ValueAtTicks(ticks) - Expr2.ValueAtTicks(ticks);
                case ParamOperators.Mul:
                    return Expr1.ValueAtTicks(ticks) * Expr2.ValueAtTicks(ticks);
                case ParamOperators.Div:
                    return Expr1.ValueAtTicks(ticks) / Expr2.ValueAtTicks(ticks);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

    }
}