namespace Plugin.SynthV
{
    public class TranslationalParam : IParamExpression
    {
        private readonly IParamExpression Expression;

        private readonly int Offset;

        public TranslationalParam(IParamExpression expression, int offset)
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
