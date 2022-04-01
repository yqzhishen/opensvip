namespace Plugin.SynthV
{
    public abstract class ParamExpression
    {
        public abstract int ValueAtTicks(int ticks);
        
        // operators between two params
        public static ParamExpression operator +(ParamExpression expr1, ParamExpression expr2)
        {
            return new CompoundParam(expr1, ParamOperators.Add, expr2);
        }
        
        public static ParamExpression operator -(ParamExpression expr1, ParamExpression expr2)
        {
            return new CompoundParam(expr1, ParamOperators.Sub, expr2);
        }
        
        public static ParamExpression operator *(ParamExpression expr1, ParamExpression expr2)
        {
            return new CompoundParam(expr1, ParamOperators.Mul, expr2);
        }
        
        public static ParamExpression operator /(ParamExpression expr1, ParamExpression expr2)
        {
            return new CompoundParam(expr1, ParamOperators.Div, expr2);
        }
        
        // operators between one param and one constant
        public static ParamExpression operator +(ParamExpression expression, int value)
        {
            return new TranslationalParam(expression, value);
        }

        public static ParamExpression operator -(ParamExpression expression, int value)
        {
            return expression + -value;
        }

        public static ParamExpression operator *(ParamExpression expression, double value)
        {
            return new ScaledParam(expression, value);
        }

        public static ParamExpression operator /(ParamExpression expression, double value)
        {
            return expression * (1.0 / value);
        }
    }
}
