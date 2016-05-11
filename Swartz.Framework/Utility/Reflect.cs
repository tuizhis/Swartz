using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using Swartz.Validation;

namespace Swartz.Utility
{
    /// <summary>
    ///     Allows static reflection on members (methods, fields, properties).
    ///     This code has been adapted from the following blog post:
    ///     http://wekempf.spaces.live.com/blog/cns!D18C3EC06EA971CF!694.entry
    /// </summary>
    public static class Reflect
    {
        public static MemberInfo GetMember(Expression<Action> expression)
        {
            Argument.ThrowIfNull(expression, "expression");

            return GetMemberInfo(expression);
        }

        public static MemberInfo GetMember<T>(Expression<Func<T>> expression)
        {
            Argument.ThrowIfNull(expression, "expression");

            return GetMemberInfo(expression);
        }

        public static MethodInfo GetMethod(Expression<Action> expression)
        {
            var method = GetMember(expression) as MethodInfo;
            Argument.ThrowIfNull(method, "expression", "Expression is not a method call");

            return method;
        }

        public static PropertyInfo GetProperty<T>(Expression<Func<T>> expression)
        {
            var property = GetMember(expression) as PropertyInfo;
            Argument.ThrowIfNull(property, "expression", "Expression is not a property");

            return property;
        }

        public static FieldInfo GetField<T>(Expression<Func<T>> expression)
        {
            var field = GetMember(expression) as FieldInfo;
            Argument.ThrowIfNull(field, "expression", "Expression is not a field access");

            return field;
        }

        public static string NameOf<T>(T value, Expression<Action<T>> expression)
        {
            return GetNameOf(expression);
        }

        public static string NameOf<T, TResult>(T value, Expression<Func<T, TResult>> expression)
        {
            return GetNameOf(expression);
        }

        internal static MemberInfo GetMemberInfo(LambdaExpression lambda)
        {
            Argument.ThrowIfNull(lambda, "lambda");

            if (lambda.Body.NodeType == ExpressionType.Call)
            {
                return ((MethodCallExpression) lambda.Body).Method;
            }

            var memberExpression = GetMemberExpression(lambda.Body);
            Argument.ThrowIfNull(memberExpression, "lambda", "Expression is not a member access");

            return memberExpression.Member;
        }

        internal static MemberExpression GetMemberExpression(Expression expression)
        {
            MemberExpression memberExpression = null;
            if (expression.NodeType == ExpressionType.Convert)
            {
                memberExpression = ((UnaryExpression) expression).Operand as MemberExpression;
            }
            else if (expression.NodeType == ExpressionType.MemberAccess)
            {
                memberExpression = expression as MemberExpression;
            }
            return memberExpression;
        }

        internal static void AddNames(Expression expression, NameBuilder nb)
        {
            if (expression == null)
                return;

            switch (expression.NodeType)
            {
                case ExpressionType.MemberAccess:
                    var memberExpression = (MemberExpression) expression;
                    AddNames(memberExpression.Expression, nb);
                    if (nb.DotNeeded)
                        nb.Append(".");
                    nb.Append(memberExpression.Member.Name);
                    break;

                //case ExpressionType.Convert:
                //    var unaryExpression = (UnaryExpression)expression;
                //    AddNames(unaryExpression.Operand, nb);
                //    break;

                case ExpressionType.Call:
                    var callExpression = (MethodCallExpression) expression;
                    var method = callExpression.Method;
                    var isIndexer = method.Name == "get_Item" && method.IsSpecialName;
                    if (!isIndexer)
                    {
                        goto default;
                    }

                    AddNames(callExpression.Object, nb);
                    nb.Append("[" + string.Join("", GetArguments(callExpression.Arguments)) + "]");
                    break;

                case ExpressionType.Parameter:
                    break;

                default:
                    throw new InvalidOperationException(
                        $"Unsupported expression type \"{Enum.GetName(typeof(ExpressionType), expression.NodeType)}\" in named expression");
            }
        }

        private static IEnumerable<string> GetArguments(IEnumerable<Expression> expressions)
        {
            foreach (var expression in expressions)
            {
                var value = GetExpressionConstantValue(expression);
                var result = value?.ToString();
                yield return result;
            }
        }

        private static object GetExpressionConstantValue(Expression expression)
        {
            switch (expression.NodeType)
            {
                case ExpressionType.Constant:
                    var constantExpression = (ConstantExpression) expression;
                    return constantExpression.Value;

                case ExpressionType.MemberAccess:
                    var memberExpression = (MemberExpression) expression;
                    var value = GetExpressionConstantValue(memberExpression.Expression);
                    if (value == null)
                        throw new InvalidOperationException("Member access to \"null\" instance is not supported");

                    var fieldInfo = memberExpression.Member as FieldInfo;
                    if (fieldInfo != null)
                    {
                        return fieldInfo.GetValue(value);
                    }

                    var propertyInfo = memberExpression.Member as PropertyInfo;
                    if (propertyInfo != null)
                    {
                        return propertyInfo.GetValue(value, null);
                    }
                    throw new InvalidOperationException(
                        $"Member access expression \"{memberExpression.GetType().FullName}\" not supported");

                default:
                    throw new InvalidOperationException(
                        $"Unsupported expression type\"{Enum.GetName(typeof(ExpressionType), expression.NodeType)}\" in method or indexer argument");
            }
        }

        internal static string GetNameOf(LambdaExpression expression)
        {
            var nb = new NameBuilder(expression);
            AddNames(expression.Body, nb);
            return nb.ToString();
        }

        internal class NameBuilder
        {
            private readonly LambdaExpression _expression;
            private readonly StringBuilder _stringBuilder = new StringBuilder();

            public NameBuilder(LambdaExpression expression)
            {
                _expression = expression;
            }

            public bool DotNeeded => _stringBuilder.Length > 0;

            public override string ToString()
            {
                return _stringBuilder.ToString();
            }

            public void Append(string s)
            {
                _stringBuilder.Append(s);
            }
        }
    }
}