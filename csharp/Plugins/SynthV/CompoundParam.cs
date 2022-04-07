using System;

namespace SynthV.Param
{
    public enum ParamOperators
    {
        Add, Sub, Mul, Div
    }
    
    public class CompoundParam : ParamExpression
    {
        private readonly ParamExpression Expr1;

        private readonly ParamExpression Expr2;

        private readonly ParamOperators Operator;

        public CompoundParam(ParamExpression expr1, ParamOperators op, ParamExpression expr2)
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