using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Flatwhite
{
    internal static class ExpressionHelper
    {
        public static MethodInfo ToMethodInfo<T>(Expression<Func<T, object>> method) where T : class
        {
            MethodInfo mInfo;
            if (method.Body is MethodCallExpression)
            {
                mInfo = ((MethodCallExpression)method.Body).Method;
            }
            else
            {
                var expressionBody = method.Body as UnaryExpression;

                if (expressionBody == null)
                {
                    throw new ArgumentException("Expect a method call");
                }

                var temp = expressionBody.Operand as MethodCallExpression;
                if (temp == null)
                {
                    throw new ArgumentException("Expect a method call expression");
                }

                mInfo = temp.Method;
            }
            return mInfo;
        }
    }
}