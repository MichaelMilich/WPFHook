using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace WPFHook.ViewModels.BackgroundLogic
{
    public class Rule
    {
        private string parameter;
        private string operation;
        private string constant;
        private int tagId;
        public static string everythingElseRuleString = "EveryTHingElse";
        public string Parameter
        {
            get { return parameter; }
            set { parameter = value; }
        }
        public string Operation
        {
            get { return operation; }
            set { operation = value; }
        }
        public string Constant
        {
            get { return constant; }
            set { constant = value; }
        }
        public int TagId
        {
            get { return tagId; }
            set { tagId = value; }
        }
        public Rule()
        {

        }
        public Rule(string parameter, string operation, string constant,int tagId)
        {
            this.parameter = parameter;
            this.operation = operation;
            this.constant = constant;
            this.tagId = tagId;
        }

        public static Func<T, bool> CompileRule<T>(Rule r)
        {
            if (r.Operation.Equals(Rule.everythingElseRuleString)) // if the function is everything else is the last tag than ,make a function that makes everything last tag
            {
               var exp= Expression.Lambda<Func<T, bool>>(Expression.Constant(true), Expression.Parameter(typeof(T), "_"));
                return exp.Compile();
            }
            else
            {
                var paramUser = Expression.Parameter(typeof(T));
                Expression expr = BuildExpr<T>(r, paramUser);
                // build a lambda function User->bool and compile it
                return Expression.Lambda<Func<T, bool>>(expr, paramUser).Compile();
            }
        }
        private static readonly MethodInfo StringContainExpressionMethodInfo = typeof(string).GetMethod("Contains", new Type[] {
        typeof(string), typeof(StringComparison)});

        static Expression BuildExpr<T>(Rule r, ParameterExpression param)
        {
            var left = MemberExpression.Property(param, r.Parameter);
            var tProp = typeof(T).GetProperty(r.Parameter).PropertyType;
            ExpressionType tBinary;
            // is the operator a known .NET operator?
            if (ExpressionType.TryParse(r.Operation, out tBinary))
            {
                var right = Expression.Constant(Convert.ChangeType(r.Constant, tProp));
                // use a binary operation, e.g. 'Equal' -> 'u.Age == 15'
                return Expression.MakeBinary(tBinary, left, right);
            }
            else
            {
                // var method = tProp.GetMethod(r.Operation);
                MethodInfo method = null;
                var methods = tProp.GetMethods();
                foreach(var m in methods)
                {
                    if (m.Name.Equals(r.Operation))
                    {
                        if (m.Name.Equals("Contains"))
                        {
                            if (m.Equals(StringContainExpressionMethodInfo))
                            {
                                var tParam2 = m.GetParameters()[0].ParameterType;
                                var right2 = Expression.Constant(Convert.ChangeType(r.Constant, tParam2));
                                var ignoreCase = Expression.Constant(StringComparison.CurrentCultureIgnoreCase);
                                return Expression.Call(left, m, new Expression[] { right2, ignoreCase });
                            }
                        }
                        else if (m.GetParameters().Length == 1)
                        {
                            method = m;
                            break;
                        }
                    }

                }
                var tParam = method.GetParameters()[0].ParameterType;
                var right = Expression.Constant(Convert.ChangeType(r.Constant, tParam));
                // use a method call, e.g. 'Contains' -> 'u.Tags.Contains(some_tag)'
                return Expression.Call(left, method, right);
            }
        }
    }

}
