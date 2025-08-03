namespace Descrio
{
    public class ExpressionEvaluator
    {
        public bool IsTruthy(object value)
        {
            if (value == null)
                return false;
            if (value is bool b)
                return b;
            if (value is int i)
                return i != 0;
            if (value is float f)
                return f != 0.0f;
            if (value is string s)
                return !string.IsNullOrEmpty(s);
            return true;
        }
    }
}