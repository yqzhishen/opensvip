namespace Plugin.SynthV
{
    public abstract class IParamExpression
    {
        public abstract int ValueAtTicks(int ticks);
        
        // operators between two params
        public static IParamExpression operator +(IParamExpression expr1, IParamExpression expr2)
        {
            return new CompoundParam(expr1, ParamOperators.Add, expr2);
        }
        
        public static IParamExpression operator -(IParamExpression expr1, IParamExpression expr2)
        {
            return new CompoundParam(expr1, ParamOperators.Sub, expr2);
        }
        
        public static IParamExpression operator *(IParamExpression expr1, IParamExpression expr2)
        {
            return new CompoundParam(expr1, ParamOperators.Mul, expr2);
        }
        
        public static IParamExpression operator /(IParamExpression expr1, IParamExpression expr2)
        {
            return new CompoundParam(expr1, ParamOperators.Div, expr2);
        }
        
        // operators between one param and one constant
        public static IParamExpression operator +(IParamExpression expression, int value)
        {
            return new TranslationalParam(expression, value);
        }

        public static IParamExpression operator -(IParamExpression expression, int value)
        {
            return expression + -value;
        }

        public static IParamExpression operator *(IParamExpression expression, double value)
        {
            return new ScaledParam(expression, value);
        }

        public static IParamExpression operator /(IParamExpression expression, double value)
        {
            return expression * (1.0 / value);
        }
    }
}
