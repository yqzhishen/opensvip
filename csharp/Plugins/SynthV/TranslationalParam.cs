namespace SynthV.Param
{
    public class TranslationalParam : ParamExpression
    {
        private readonly ParamExpression Expression;

        private readonly int Offset;

        public TranslationalParam(ParamExpression expression, int offset)
        {
            Expression = expression;
            Offset = offset;
        }


        public override int ValueAtTicks(int ticks)
        {
            return Expression.ValueAtTicks(ticks) + Offset;
        }
    }
}
