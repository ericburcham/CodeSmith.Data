﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading;

namespace CodeSmith.Data.Linq.Dynamic
{
    public static class DynamicQueryable
    {
        public static bool Any(this IQueryable source)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            return (bool)source.Provider.Execute(
                Expression.Call(
                    typeof(Queryable),
                    "Any",
                    new[] { source.ElementType },
                    source.Expression));
        }

        public static IQueryable AppendPageSort(this IQueryable query, int? start, int? limit, string sort, string dir)
        {
            if (!string.IsNullOrEmpty(sort))
            {
                if (!string.IsNullOrEmpty(dir) &&
                    dir.Equals("desc", StringComparison.OrdinalIgnoreCase))
                {
                    query = query.OrderBy(sort + " desc");
                }
                else
                {
                    query = query.OrderBy(sort);
                }
            }

            if (start.HasValue && start > 0)
            {
                query = query.Skip(start.Value);
            }

            if (limit.HasValue && limit > 0)
            {
                query = query.Take(limit.Value);
            }

            return query;
        }

        public static IQueryable<T> AppendPageSort<T>(
            this IQueryable<T> query,
            int? start,
            int? limit,
            string sort,
            string dir)
        {
            return ((IQueryable)query).AppendPageSort(start, limit, sort, dir) as IQueryable<T>;
        }

        public static IQueryable<T> AppendSort<T>(this IQueryable<T> query, string sort, string dir)
        {
            if (!string.IsNullOrEmpty(sort))
            {
                if (!string.IsNullOrEmpty(dir) &&
                    dir.Equals("desc", StringComparison.OrdinalIgnoreCase))
                {
                    query = query.OrderBy(sort + " desc");
                }
                else
                {
                    query = query.OrderBy(sort);
                }
            }

            return query;
        }

        public static IQueryable<T> AppendWhereClause<T>(this IQueryable<T> query, IList<string> clauses)
        {
            if (clauses == null || clauses.Count == 0)
            {
                return query;
            }

            foreach (var clause in clauses)
            {
                if (!string.IsNullOrEmpty(clause))
                {
                    query = query.Where(clause);
                }
            }

            return query;
        }

        public static int Count(this IQueryable source)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            return (int)source.Provider.Execute(
                Expression.Call(
                    typeof(Queryable),
                    "Count",
                    new[] { source.ElementType },
                    source.Expression));
        }

        public static IQueryable GroupBy(
            this IQueryable source,
            string keySelector,
            string elementSelector,
            params object[] values)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            if (keySelector == null)
            {
                throw new ArgumentNullException("keySelector");
            }

            if (elementSelector == null)
            {
                throw new ArgumentNullException("elementSelector");
            }

            var keyLambda = DynamicExpression.ParseLambda(source.ElementType, null, keySelector, values);
            var elementLambda = DynamicExpression.ParseLambda(source.ElementType, null, elementSelector, values);

            return source.Provider.CreateQuery(
                Expression.Call(
                    typeof(Queryable),
                    "GroupBy",
                    new[] { source.ElementType, keyLambda.Body.Type, elementLambda.Body.Type },
                    source.Expression,
                    Expression.Quote(keyLambda),
                    Expression.Quote(elementLambda)));
        }

        public static IQueryable<T> OrderBy<T>(this IQueryable<T> source, string ordering, params object[] values)
        {
            return (IQueryable<T>)OrderBy((IQueryable)source, ordering, values);
        }

        public static IQueryable OrderBy(this IQueryable source, string ordering, params object[] values)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            if (ordering == null)
            {
                throw new ArgumentNullException("ordering");
            }

            ParameterExpression[] parameters =
            {
                Expression.Parameter(source.ElementType, "")
            };

            var parser = new ExpressionParser(parameters, ordering, values);
            var orderings = parser.ParseOrdering();
            var queryExpr = source.Expression;
            var methodAsc = "OrderBy";
            var methodDesc = "OrderByDescending";
            foreach (var o in orderings)
            {
                queryExpr = Expression.Call(
                    typeof(Queryable),
                    o.Ascending
                        ? methodAsc
                        : methodDesc,
                    new[] { source.ElementType, o.Selector.Type },
                    queryExpr,
                    Expression.Quote(Expression.Lambda(o.Selector, parameters)));

                methodAsc = "ThenBy";
                methodDesc = "ThenByDescending";
            }

            return source.Provider.CreateQuery(queryExpr);
        }

        public static IQueryable Select(this IQueryable source, string selector, params object[] values)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            if (selector == null)
            {
                throw new ArgumentNullException("selector");
            }

            var lambda = DynamicExpression.ParseLambda(source.ElementType, null, selector, values);

            return source.Provider.CreateQuery(
                Expression.Call(
                    typeof(Queryable),
                    "Select",
                    new[] { source.ElementType, lambda.Body.Type },
                    source.Expression,
                    Expression.Quote(lambda)));
        }

        public static IQueryable Skip(this IQueryable source, int count)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            return source.Provider.CreateQuery(
                Expression.Call(
                    typeof(Queryable),
                    "Skip",
                    new[] { source.ElementType },
                    source.Expression,
                    Expression.Constant(count)));
        }

        public static IQueryable Take(this IQueryable source, int count)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            return source.Provider.CreateQuery(
                Expression.Call(
                    typeof(Queryable),
                    "Take",
                    new[] { source.ElementType },
                    source.Expression,
                    Expression.Constant(count)));
        }

        public static IQueryable<T> Where<T>(this IQueryable<T> source, string predicate, params object[] values)
        {
            return (IQueryable<T>)Where((IQueryable)source, predicate, values);
        }

        public static IQueryable Where(this IQueryable source, string predicate, params object[] values)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            if (predicate == null)
            {
                throw new ArgumentNullException("predicate");
            }

            var lambda = DynamicExpression.ParseLambda(source.ElementType, typeof(bool), predicate, values);

            return source.Provider.CreateQuery(
                Expression.Call(
                    typeof(Queryable),
                    "Where",
                    new[] { source.ElementType },
                    source.Expression,
                    Expression.Quote(lambda)));
        }
    }

    public abstract class DynamicClass
    {
        public override string ToString()
        {
            var props = GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public);
            var sb = new StringBuilder();
            sb.Append("{");
            for (var i = 0; i < props.Length; i++)
            {
                if (i > 0)
                {
                    sb.Append(", ");
                }

                sb.Append(props[i].Name);
                sb.Append("=");
                sb.Append(props[i].GetValue(this, null));
            }

            sb.Append("}");

            return sb.ToString();
        }
    }

    public class DynamicProperty
    {
        public DynamicProperty(string name, Type type)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }

            if (type == null)
            {
                throw new ArgumentNullException("type");
            }

            Name = name;
            Type = type;
        }

        public string Name { get; }

        public Type Type { get; }
    }

    public static class DynamicExpression
    {
        public static Expression<Func<T, S>> BuildExpression<T, S>(string identifier, IEnumerable values)
        {
            var expression = BuildExpressionString<T>(identifier, values);
            var array = values.Cast<object>().ToArray();

            return ParseLambda<T, S>(expression, array);
        }

        public static Type CreateClass(params DynamicProperty[] properties)
        {
            return ClassFactory.Instance.GetDynamicClass(properties);
        }

        public static Type CreateClass(IEnumerable<DynamicProperty> properties)
        {
            return ClassFactory.Instance.GetDynamicClass(properties);
        }

        public static Expression Parse(Type resultType, string expression, params object[] values)
        {
            var parser = new ExpressionParser(null, expression, values);

            return parser.Parse(resultType);
        }

        public static LambdaExpression ParseLambda(
            Type itType,
            Type resultType,
            string expression,
            params object[] values)
        {
            return ParseLambda(new[] { Expression.Parameter(itType, "") }, resultType, expression, values);
        }

        public static LambdaExpression ParseLambda(
            ParameterExpression[] parameters,
            Type resultType,
            string expression,
            params object[] values)
        {
            var parser = new ExpressionParser(parameters, expression, values);

            return Expression.Lambda(parser.Parse(resultType), parameters);
        }

        public static Expression<Func<T, S>> ParseLambda<T, S>(string expression, params object[] values)
        {
            return (Expression<Func<T, S>>)ParseLambda(typeof(T), typeof(S), expression, values);
        }

        private static string BuildExpressionString<T>(string identifier, IEnumerable values)
        {
            var expression = new StringBuilder();
            var count = 0;

            foreach (var value in values)
            {
                if (count > 0)
                {
                    expression.Append(" || ");
                }

                if (value == null)
                {
                    expression.AppendFormat("{0} == null", identifier);
                }
                else
                {
                    expression.AppendFormat("{0} == @{1}", identifier, count);
                }

                count++;
            }

            return expression.ToString();
        }

        private static bool IsPropertyNullable<T>(string identifier)
        {
            var property = typeof(T).GetProperty(identifier);
            if (property == null || !property.PropertyType.IsGenericType)
            {
                return false;
            }

            var genericType = property.PropertyType.GetGenericTypeDefinition();

            return genericType != null && genericType == typeof(Nullable<>);
        }
    }

    internal class DynamicOrdering
    {
        public bool Ascending;

        public Expression Selector;
    }

    internal class Signature : IEquatable<Signature>
    {
        public int hashCode;

        public DynamicProperty[] properties;

        public Signature(IEnumerable<DynamicProperty> properties)
        {
            this.properties = properties.ToArray();
            hashCode = 0;
            foreach (var p in properties)
            {
                hashCode ^= p.Name.GetHashCode() ^ p.Type.GetHashCode();
            }
        }

        public bool Equals(Signature other)
        {
            if (properties.Length != other.properties.Length)
            {
                return false;
            }

            for (var i = 0; i < properties.Length; i++)
            {
                if (properties[i].Name != other.properties[i].Name ||
                    properties[i].Type != other.properties[i].Type)
                {
                    return false;
                }
            }

            return true;
        }

        public override bool Equals(object obj)
        {
            return obj is Signature
                ? Equals((Signature)obj)
                : false;
        }

        public override int GetHashCode()
        {
            return hashCode;
        }
    }

    internal class ClassFactory
    {
        public static readonly ClassFactory Instance = new ClassFactory();

        private readonly Dictionary<Signature, Type> classes;

        private readonly ModuleBuilder module;

        private readonly ReaderWriterLock rwLock;

        private int classCount;

        static ClassFactory()
        {
        } // Trigger lazy initialization of static fields

        private ClassFactory()
        {
            var name = new AssemblyName("DynamicClasses");
            var assembly = AppDomain.CurrentDomain.DefineDynamicAssembly(name, AssemblyBuilderAccess.Run);
#if ENABLE_LINQ_PARTIAL_TRUST
            new ReflectionPermission(PermissionState.Unrestricted).Assert();
#endif
            module = assembly.DefineDynamicModule("Module");
            classes = new Dictionary<Signature, Type>();
            rwLock = new ReaderWriterLock();
        }

        public Type GetDynamicClass(IEnumerable<DynamicProperty> properties)
        {
            rwLock.AcquireReaderLock(Timeout.Infinite);
            try
            {
                var signature = new Signature(properties);
                Type type;
                if (!classes.TryGetValue(signature, out type))
                {
                    type = CreateDynamicClass(signature.properties);
                    classes.Add(signature, type);
                }

                return type;
            }
            finally
            {
                rwLock.ReleaseReaderLock();
            }
        }

        private Type CreateDynamicClass(DynamicProperty[] properties)
        {
            var cookie = rwLock.UpgradeToWriterLock(Timeout.Infinite);
            try
            {
                var typeName = "DynamicClass" + (classCount + 1);
#if ENABLE_LINQ_PARTIAL_TRUST
                new ReflectionPermission(PermissionState.Unrestricted).Assert();
#endif
                var tb = module.DefineType(
                    typeName,
                    TypeAttributes.Class |
                    TypeAttributes.Public,
                    typeof(DynamicClass));

                var fields = GenerateProperties(tb, properties);
                GenerateEquals(tb, fields);
                GenerateGetHashCode(tb, fields);
                var result = tb.CreateType();
                classCount++;

                return result;
            }
            finally
            {
                rwLock.DowngradeFromWriterLock(ref cookie);
            }
        }

        private void GenerateEquals(TypeBuilder tb, FieldInfo[] fields)
        {
            var mb = tb.DefineMethod(
                "Equals",
                MethodAttributes.Public | MethodAttributes.ReuseSlot |
                MethodAttributes.Virtual | MethodAttributes.HideBySig,
                typeof(bool),
                new[] { typeof(object) });

            var gen = mb.GetILGenerator();
            var other = gen.DeclareLocal(tb);
            var next = gen.DefineLabel();
            gen.Emit(OpCodes.Ldarg_1);
            gen.Emit(OpCodes.Isinst, tb);
            gen.Emit(OpCodes.Stloc, other);
            gen.Emit(OpCodes.Ldloc, other);
            gen.Emit(OpCodes.Brtrue_S, next);
            gen.Emit(OpCodes.Ldc_I4_0);
            gen.Emit(OpCodes.Ret);
            gen.MarkLabel(next);
            foreach (var field in fields)
            {
                var ft = field.FieldType;
                var ct = typeof(EqualityComparer<>).MakeGenericType(ft);
                next = gen.DefineLabel();
                gen.EmitCall(OpCodes.Call, ct.GetMethod("get_Default"), null);
                gen.Emit(OpCodes.Ldarg_0);
                gen.Emit(OpCodes.Ldfld, field);
                gen.Emit(OpCodes.Ldloc, other);
                gen.Emit(OpCodes.Ldfld, field);
                gen.EmitCall(OpCodes.Callvirt, ct.GetMethod("Equals", new[] { ft, ft }), null);
                gen.Emit(OpCodes.Brtrue_S, next);
                gen.Emit(OpCodes.Ldc_I4_0);
                gen.Emit(OpCodes.Ret);
                gen.MarkLabel(next);
            }

            gen.Emit(OpCodes.Ldc_I4_1);
            gen.Emit(OpCodes.Ret);
        }

        private void GenerateGetHashCode(TypeBuilder tb, FieldInfo[] fields)
        {
            var mb = tb.DefineMethod(
                "GetHashCode",
                MethodAttributes.Public | MethodAttributes.ReuseSlot |
                MethodAttributes.Virtual | MethodAttributes.HideBySig,
                typeof(int),
                Type.EmptyTypes);

            var gen = mb.GetILGenerator();
            gen.Emit(OpCodes.Ldc_I4_0);
            foreach (var field in fields)
            {
                var ft = field.FieldType;
                var ct = typeof(EqualityComparer<>).MakeGenericType(ft);
                gen.EmitCall(OpCodes.Call, ct.GetMethod("get_Default"), null);
                gen.Emit(OpCodes.Ldarg_0);
                gen.Emit(OpCodes.Ldfld, field);
                gen.EmitCall(OpCodes.Callvirt, ct.GetMethod("GetHashCode", new[] { ft }), null);
                gen.Emit(OpCodes.Xor);
            }

            gen.Emit(OpCodes.Ret);
        }

        private FieldInfo[] GenerateProperties(TypeBuilder tb, DynamicProperty[] properties)
        {
            FieldInfo[] fields = new FieldBuilder[properties.Length];
            for (var i = 0; i < properties.Length; i++)
            {
                var dp = properties[i];
                var fb = tb.DefineField("_" + dp.Name, dp.Type, FieldAttributes.Private);
                var pb = tb.DefineProperty(dp.Name, PropertyAttributes.HasDefault, dp.Type, null);
                var mbGet = tb.DefineMethod(
                    "get_" + dp.Name,
                    MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig,
                    dp.Type,
                    Type.EmptyTypes);

                var genGet = mbGet.GetILGenerator();
                genGet.Emit(OpCodes.Ldarg_0);
                genGet.Emit(OpCodes.Ldfld, fb);
                genGet.Emit(OpCodes.Ret);
                var mbSet = tb.DefineMethod(
                    "set_" + dp.Name,
                    MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig,
                    null,
                    new[] { dp.Type });

                var genSet = mbSet.GetILGenerator();
                genSet.Emit(OpCodes.Ldarg_0);
                genSet.Emit(OpCodes.Ldarg_1);
                genSet.Emit(OpCodes.Stfld, fb);
                genSet.Emit(OpCodes.Ret);
                pb.SetGetMethod(mbGet);
                pb.SetSetMethod(mbSet);
                fields[i] = fb;
            }

            return fields;
        }
    }

    public sealed class ParseException : Exception
    {
        public ParseException(string message, int position)
            : base(message)
        {
            Position = position;
        }

        public int Position { get; }

        public override string ToString()
        {
            return string.Format(Res.ParseExceptionFormat, Message, Position);
        }
    }

    internal class ExpressionParser
    {
        private static readonly Type[] predefinedTypes =
        {
            typeof(object),
            typeof(bool),
            typeof(char),
            typeof(string),
            typeof(sbyte),
            typeof(byte),
            typeof(short),
            typeof(ushort),
            typeof(int),
            typeof(uint),
            typeof(long),
            typeof(ulong),
            typeof(float),
            typeof(double),
            typeof(decimal),
            typeof(DateTime),
            typeof(TimeSpan),
            typeof(Guid),
            typeof(Math),
            typeof(Convert)
        };

        private static readonly Expression trueLiteral = Expression.Constant(true);

        private static readonly Expression falseLiteral = Expression.Constant(false);

        private static readonly Expression nullLiteral = Expression.Constant(null);

        private static readonly string keywordIt = "it";

        private static readonly string keywordIif = "iif";

        private static readonly string keywordNew = "new";

        private static Dictionary<string, object> keywords;

        private readonly Dictionary<Expression, string> literals;

        private readonly Dictionary<string, object> symbols;

        private readonly string text;

        private readonly int textLen;

        private char ch;

        private IDictionary<string, object> externals;

        private ParameterExpression it;

        private int textPos;

        private Token token;

        public ExpressionParser(ParameterExpression[] parameters, string expression, object[] values)
        {
            if (expression == null)
            {
                throw new ArgumentNullException("expression");
            }

            if (keywords == null)
            {
                keywords = CreateKeywords();
            }

            symbols = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
            literals = new Dictionary<Expression, string>();
            if (parameters != null)
            {
                ProcessParameters(parameters);
            }

            if (values != null)
            {
                ProcessValues(values);
            }

            text = expression;
            textLen = text.Length;
            SetTextPos(0);
            NextToken();
        }

        private enum TokenId
        {
            Unknown,

            End,

            Identifier,

            StringLiteral,

            IntegerLiteral,

            RealLiteral,

            Exclamation,

            Percent,

            Amphersand,

            OpenParen,

            CloseParen,

            Asterisk,

            Plus,

            Comma,

            Minus,

            Dot,

            Slash,

            Colon,

            LessThan,

            Equal,

            GreaterThan,

            Question,

            OpenBracket,

            CloseBracket,

            Bar,

            ExclamationEqual,

            DoubleAmphersand,

            LessThanEqual,

            LessGreater,

            DoubleEqual,

            GreaterThanEqual,

            DoubleBar
        }

        private interface IAddSignatures : IArithmeticSignatures
        {
            void F(DateTime x, TimeSpan y);

            void F(TimeSpan x, TimeSpan y);

            void F(DateTime? x, TimeSpan? y);

            void F(TimeSpan? x, TimeSpan? y);
        }

        private interface IArithmeticSignatures
        {
            void F(int x, int y);

            void F(uint x, uint y);

            void F(long x, long y);

            void F(ulong x, ulong y);

            void F(float x, float y);

            void F(double x, double y);

            void F(decimal x, decimal y);

            void F(int? x, int? y);

            void F(uint? x, uint? y);

            void F(long? x, long? y);

            void F(ulong? x, ulong? y);

            void F(float? x, float? y);

            void F(double? x, double? y);

            void F(decimal? x, decimal? y);
        }

        private interface IEnumerableSignatures
        {
            void All(bool predicate);

            void Any();

            void Any(bool predicate);

            void Average(int selector);

            void Average(int? selector);

            void Average(long selector);

            void Average(long? selector);

            void Average(float selector);

            void Average(float? selector);

            void Average(double selector);

            void Average(double? selector);

            void Average(decimal selector);

            void Average(decimal? selector);

            void Count();

            void Count(bool predicate);

            void Max(object selector);

            void Min(object selector);

            void Sum(int selector);

            void Sum(int? selector);

            void Sum(long selector);

            void Sum(long? selector);

            void Sum(float selector);

            void Sum(float? selector);

            void Sum(double selector);

            void Sum(double? selector);

            void Sum(decimal selector);

            void Sum(decimal? selector);

            void Where(bool predicate);
        }

        private interface IEqualitySignatures : IRelationalSignatures
        {
            void F(bool x, bool y);

            void F(bool? x, bool? y);

            void F(Guid x, Guid y);

            void F(Guid? x, Guid? y);
        }

        private interface ILogicalSignatures
        {
            void F(bool x, bool y);

            void F(bool? x, bool? y);
        }

        private interface INegationSignatures
        {
            void F(int x);

            void F(long x);

            void F(float x);

            void F(double x);

            void F(decimal x);

            void F(int? x);

            void F(long? x);

            void F(float? x);

            void F(double? x);

            void F(decimal? x);
        }

        private interface INotSignatures
        {
            void F(bool x);

            void F(bool? x);
        }

        private interface IRelationalSignatures : IArithmeticSignatures
        {
            void F(string x, string y);

            void F(char x, char y);

            void F(DateTime x, DateTime y);

            void F(TimeSpan x, TimeSpan y);

            void F(char? x, char? y);

            void F(DateTime? x, DateTime? y);

            void F(TimeSpan? x, TimeSpan? y);
        }

        private interface ISubtractSignatures : IAddSignatures
        {
            void F(DateTime x, DateTime y);

            void F(DateTime? x, DateTime? y);
        }

        public Expression Parse(Type resultType)
        {
            var exprPos = token.pos;
            var expr = ParseExpression();
            if (resultType != null)
            {
                if ((expr = PromoteExpression(expr, resultType, true)) == null)
                {
                    throw ParseError(exprPos, Res.ExpressionTypeMismatch, GetTypeName(resultType));
                }
            }

            ValidateToken(TokenId.End, Res.SyntaxError);

            return expr;
        }

#pragma warning disable 0219
        public IEnumerable<DynamicOrdering> ParseOrdering()
        {
            var orderings = new List<DynamicOrdering>();
            while (true)
            {
                var expr = ParseExpression();
                var ascending = true;
                if (TokenIdentifierIs("asc") || TokenIdentifierIs("ascending"))
                {
                    NextToken();
                }
                else if (TokenIdentifierIs("desc") || TokenIdentifierIs("descending"))
                {
                    NextToken();
                    ascending = false;
                }

                orderings.Add(new DynamicOrdering { Selector = expr, Ascending = ascending });
                if (token.id != TokenId.Comma)
                {
                    break;
                }

                NextToken();
            }

            ValidateToken(TokenId.End, Res.SyntaxError);

            return orderings;
        }
#pragma warning restore 0219

        private static void AddInterface(List<Type> types, Type type)
        {
            if (!types.Contains(type))
            {
                types.Add(type);
                foreach (var t in type.GetInterfaces())
                {
                    AddInterface(types, t);
                }
            }
        }

        // Return 1 if s -> t1 is a better conversion than s -> t2
        // Return -1 if s -> t2 is a better conversion than s -> t1
        // Return 0 if neither conversion is better
        private static int CompareConversions(Type s, Type t1, Type t2)
        {
            if (t1 == t2)
            {
                return 0;
            }

            if (s == t1)
            {
                return 1;
            }

            if (s == t2)
            {
                return -1;
            }

            var t1t2 = IsCompatibleWith(t1, t2);
            var t2t1 = IsCompatibleWith(t2, t1);
            if (t1t2 && !t2t1)
            {
                return 1;
            }

            if (t2t1 && !t1t2)
            {
                return -1;
            }

            if (IsSignedIntegralType(t1) && IsUnsignedIntegralType(t2))
            {
                return 1;
            }

            if (IsSignedIntegralType(t2) && IsUnsignedIntegralType(t1))
            {
                return -1;
            }

            return 0;
        }

        private static Dictionary<string, object> CreateKeywords()
        {
            var d = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
            d.Add("true", trueLiteral);
            d.Add("false", falseLiteral);
            d.Add("null", nullLiteral);
            d.Add(keywordIt, keywordIt);
            d.Add(keywordIif, keywordIif);
            d.Add(keywordNew, keywordNew);
            foreach (var type in predefinedTypes)
            {
                d.Add(type.Name, type);
            }

            return d;
        }

        private static Type FindGenericType(Type generic, Type type)
        {
            while (type != null && type != typeof(object))
            {
                if (type.IsGenericType && type.GetGenericTypeDefinition() == generic)
                {
                    return type;
                }

                if (generic.IsInterface)
                {
                    foreach (var intfType in type.GetInterfaces())
                    {
                        var found = FindGenericType(generic, intfType);
                        if (found != null)
                        {
                            return found;
                        }
                    }
                }

                type = type.BaseType;
            }

            return null;
        }

        private static Type GetNonNullableType(Type type)
        {
            return IsNullableType(type)
                ? type.GetGenericArguments()[0]
                : type;
        }

        private static int GetNumericTypeKind(Type type)
        {
            type = GetNonNullableType(type);
            if (type.IsEnum)
            {
                return 0;
            }

            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Char:
                case TypeCode.Single:
                case TypeCode.Double:
                case TypeCode.Decimal:
                    return 1;
                case TypeCode.SByte:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                    return 2;
                case TypeCode.Byte:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                    return 3;
                default:
                    return 0;
            }
        }

        private static string GetTypeName(Type type)
        {
            var baseType = GetNonNullableType(type);
            var s = baseType.Name;
            if (type != baseType)
            {
                s += '?';
            }

            return s;
        }

        private static bool IsBetterThan(Expression[] args, MethodData m1, MethodData m2)
        {
            var better = false;
            for (var i = 0; i < args.Length; i++)
            {
                var c = CompareConversions(
                    args[i].Type,
                    m1.Parameters[i].ParameterType,
                    m2.Parameters[i].ParameterType);

                if (c < 0)
                {
                    return false;
                }

                if (c > 0)
                {
                    better = true;
                }
            }

            return better;
        }

        private static bool IsCompatibleWith(Type source, Type target)
        {
            if (source == target)
            {
                return true;
            }

            if (!target.IsValueType)
            {
                return target.IsAssignableFrom(source);
            }

            var st = GetNonNullableType(source);
            var tt = GetNonNullableType(target);
            if (st != source && tt == target)
            {
                return false;
            }

            var sc = st.IsEnum
                ? TypeCode.Object
                : Type.GetTypeCode(st);

            var tc = tt.IsEnum
                ? TypeCode.Object
                : Type.GetTypeCode(tt);

            switch (sc)
            {
                case TypeCode.SByte:
                    switch (tc)
                    {
                        case TypeCode.SByte:
                        case TypeCode.Int16:
                        case TypeCode.Int32:
                        case TypeCode.Int64:
                        case TypeCode.Single:
                        case TypeCode.Double:
                        case TypeCode.Decimal:
                            return true;
                    }

                    break;
                case TypeCode.Byte:
                    switch (tc)
                    {
                        case TypeCode.Byte:
                        case TypeCode.Int16:
                        case TypeCode.UInt16:
                        case TypeCode.Int32:
                        case TypeCode.UInt32:
                        case TypeCode.Int64:
                        case TypeCode.UInt64:
                        case TypeCode.Single:
                        case TypeCode.Double:
                        case TypeCode.Decimal:
                            return true;
                    }

                    break;
                case TypeCode.Int16:
                    switch (tc)
                    {
                        case TypeCode.Int16:
                        case TypeCode.Int32:
                        case TypeCode.Int64:
                        case TypeCode.Single:
                        case TypeCode.Double:
                        case TypeCode.Decimal:
                            return true;
                    }

                    break;
                case TypeCode.UInt16:
                    switch (tc)
                    {
                        case TypeCode.UInt16:
                        case TypeCode.Int32:
                        case TypeCode.UInt32:
                        case TypeCode.Int64:
                        case TypeCode.UInt64:
                        case TypeCode.Single:
                        case TypeCode.Double:
                        case TypeCode.Decimal:
                            return true;
                    }

                    break;
                case TypeCode.Int32:
                    switch (tc)
                    {
                        case TypeCode.Int32:
                        case TypeCode.Int64:
                        case TypeCode.Single:
                        case TypeCode.Double:
                        case TypeCode.Decimal:
                            return true;
                    }

                    break;
                case TypeCode.UInt32:
                    switch (tc)
                    {
                        case TypeCode.UInt32:
                        case TypeCode.Int64:
                        case TypeCode.UInt64:
                        case TypeCode.Single:
                        case TypeCode.Double:
                        case TypeCode.Decimal:
                            return true;
                    }

                    break;
                case TypeCode.Int64:
                    switch (tc)
                    {
                        case TypeCode.Int64:
                        case TypeCode.Single:
                        case TypeCode.Double:
                        case TypeCode.Decimal:
                            return true;
                    }

                    break;
                case TypeCode.UInt64:
                    switch (tc)
                    {
                        case TypeCode.UInt64:
                        case TypeCode.Single:
                        case TypeCode.Double:
                        case TypeCode.Decimal:
                            return true;
                    }

                    break;
                case TypeCode.Single:
                    switch (tc)
                    {
                        case TypeCode.Single:
                        case TypeCode.Double:
                            return true;
                    }

                    break;
                default:
                    if (st == tt)
                    {
                        return true;
                    }

                    break;
            }

            return false;
        }

        private static bool IsEnumType(Type type)
        {
            return GetNonNullableType(type).IsEnum;
        }

        private static bool IsNullableType(Type type)
        {
            return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
        }

        private static bool IsNumericType(Type type)
        {
            return GetNumericTypeKind(type) != 0;
        }

        private static bool IsPredefinedType(Type type)
        {
            foreach (var t in predefinedTypes)
            {
                if (t == type)
                {
                    return true;
                }
            }

            return false;
        }

        private static bool IsSignedIntegralType(Type type)
        {
            return GetNumericTypeKind(type) == 2;
        }

        private static bool IsUnsignedIntegralType(Type type)
        {
            return GetNumericTypeKind(type) == 3;
        }

        private static object ParseEnum(string name, Type type)
        {
            if (type.IsEnum)
            {
                var memberInfos = type.FindMembers(
                    MemberTypes.Field,
                    BindingFlags.Public | BindingFlags.DeclaredOnly | BindingFlags.Static,
                    Type.FilterNameIgnoreCase,
                    name);

                if (memberInfos.Length != 0)
                {
                    return ((FieldInfo)memberInfos[0]).GetValue(null);
                }
            }

            return null;
        }

        private static object ParseNumber(string text, Type type)
        {
            switch (Type.GetTypeCode(GetNonNullableType(type)))
            {
                case TypeCode.SByte:
                    sbyte sb;
                    if (sbyte.TryParse(text, out sb))
                    {
                        return sb;
                    }

                    break;
                case TypeCode.Byte:
                    byte b;
                    if (byte.TryParse(text, out b))
                    {
                        return b;
                    }

                    break;
                case TypeCode.Int16:
                    short s;
                    if (short.TryParse(text, out s))
                    {
                        return s;
                    }

                    break;
                case TypeCode.UInt16:
                    ushort us;
                    if (ushort.TryParse(text, out us))
                    {
                        return us;
                    }

                    break;
                case TypeCode.Int32:
                    int i;
                    if (int.TryParse(text, out i))
                    {
                        return i;
                    }

                    break;
                case TypeCode.UInt32:
                    uint ui;
                    if (uint.TryParse(text, out ui))
                    {
                        return ui;
                    }

                    break;
                case TypeCode.Int64:
                    long l;
                    if (long.TryParse(text, out l))
                    {
                        return l;
                    }

                    break;
                case TypeCode.UInt64:
                    ulong ul;
                    if (ulong.TryParse(text, out ul))
                    {
                        return ul;
                    }

                    break;
                case TypeCode.Single:
                    float f;
                    if (float.TryParse(text, out f))
                    {
                        return f;
                    }

                    break;
                case TypeCode.Double:
                    double d;
                    if (double.TryParse(text, out d))
                    {
                        return d;
                    }

                    break;
                case TypeCode.Decimal:
                    decimal e;
                    if (decimal.TryParse(text, out e))
                    {
                        return e;
                    }

                    break;
            }

            return null;
        }

        private static IEnumerable<Type> SelfAndBaseClasses(Type type)
        {
            while (type != null)
            {
                yield return type;

                type = type.BaseType;
            }
        }

        private static IEnumerable<Type> SelfAndBaseTypes(Type type)
        {
            if (type.IsInterface)
            {
                var types = new List<Type>();
                AddInterface(types, type);

                return types;
            }

            return SelfAndBaseClasses(type);
        }

        private void AddSymbol(string name, object value)
        {
            if (symbols.ContainsKey(name))
            {
                throw ParseError(Res.DuplicateIdentifier, name);
            }

            symbols.Add(name, value);
        }

        private void CheckAndPromoteOperand(Type signatures, string opName, ref Expression expr, int errorPos)
        {
            Expression[] args = { expr };
            MethodBase method;
            if (FindMethod(signatures, "F", false, args, out method) != 1)
            {
                throw ParseError(
                    errorPos,
                    Res.IncompatibleOperand,
                    opName,
                    GetTypeName(args[0].Type));
            }

            expr = args[0];
        }

        private void CheckAndPromoteOperands(
            Type signatures,
            string opName,
            ref Expression left,
            ref Expression right,
            int errorPos)
        {
            Expression[] args = { left, right };
            MethodBase method;
            if (FindMethod(signatures, "F", false, args, out method) != 1)
            {
                throw IncompatibleOperandsError(opName, left, right, errorPos);
            }

            left = args[0];
            right = args[1];
        }

        private Expression CreateLiteral(object value, string text)
        {
            var expr = Expression.Constant(value);
            literals.Add(expr, text);

            return expr;
        }

        private int FindBestMethod(IEnumerable<MethodBase> methods, Expression[] args, out MethodBase method)
        {
            var applicable = methods.Select(m => new MethodData { MethodBase = m, Parameters = m.GetParameters() })
                                    .Where(m => IsApplicable(m, args)).ToArray();

            if (applicable.Length > 1)
            {
                applicable = applicable.Where(m => applicable.All(n => m == n || IsBetterThan(args, m, n))).ToArray();
            }

            if (applicable.Length == 1)
            {
                var md = applicable[0];
                for (var i = 0; i < args.Length; i++)
                {
                    args[i] = md.Args[i];
                }

                method = md.MethodBase;
            }
            else
            {
                method = null;
            }

            return applicable.Length;
        }

        private int FindIndexer(Type type, Expression[] args, out MethodBase method)
        {
            foreach (var t in SelfAndBaseTypes(type))
            {
                var members = t.GetDefaultMembers();
                if (members.Length != 0)
                {
                    var methods = members.OfType<PropertyInfo>().Select(p => (MethodBase)p.GetGetMethod())
                                         .Where(m => m != null);

                    var count = FindBestMethod(methods, args, out method);
                    if (count != 0)
                    {
                        return count;
                    }
                }
            }

            method = null;

            return 0;
        }

        private int FindMethod(
            Type type,
            string methodName,
            bool staticAccess,
            Expression[] args,
            out MethodBase method)
        {
            var flags = BindingFlags.Public | BindingFlags.DeclaredOnly |
                (staticAccess
                    ? BindingFlags.Static
                    : BindingFlags.Instance);

            foreach (var t in SelfAndBaseTypes(type))
            {
                var members = t.FindMembers(
                    MemberTypes.Method,
                    flags,
                    Type.FilterNameIgnoreCase,
                    methodName);

                var count = FindBestMethod(members.Cast<MethodBase>(), args, out method);
                if (count != 0)
                {
                    return count;
                }
            }

            method = null;

            return 0;
        }

        private MemberInfo FindPropertyOrField(Type type, string memberName, bool staticAccess)
        {
            var flags = BindingFlags.Public | BindingFlags.DeclaredOnly |
                (staticAccess
                    ? BindingFlags.Static
                    : BindingFlags.Instance);

            foreach (var t in SelfAndBaseTypes(type))
            {
                var members = t.FindMembers(
                    MemberTypes.Property | MemberTypes.Field,
                    flags,
                    Type.FilterNameIgnoreCase,
                    memberName);

                if (members.Length != 0)
                {
                    return members[0];
                }
            }

            return null;
        }

        private Expression GenerateAdd(Expression left, Expression right)
        {
            if (left.Type == typeof(string) && right.Type == typeof(string))
            {
                return GenerateStaticMethodCall("Concat", left, right);
            }

            return Expression.Add(left, right);
        }

        private Expression GenerateConditional(Expression test, Expression expr1, Expression expr2, int errorPos)
        {
            if (test.Type != typeof(bool))
            {
                throw ParseError(errorPos, Res.FirstExprMustBeBool);
            }

            if (expr1.Type != expr2.Type)
            {
                var expr1as2 = expr2 != nullLiteral
                    ? PromoteExpression(expr1, expr2.Type, true)
                    : null;

                var expr2as1 = expr1 != nullLiteral
                    ? PromoteExpression(expr2, expr1.Type, true)
                    : null;

                if (expr1as2 != null && expr2as1 == null)
                {
                    expr1 = expr1as2;
                }
                else if (expr2as1 != null && expr1as2 == null)
                {
                    expr2 = expr2as1;
                }
                else
                {
                    var type1 = expr1 != nullLiteral
                        ? expr1.Type.Name
                        : "null";

                    var type2 = expr2 != nullLiteral
                        ? expr2.Type.Name
                        : "null";

                    if (expr1as2 != null && expr2as1 != null)
                    {
                        throw ParseError(errorPos, Res.BothTypesConvertToOther, type1, type2);
                    }

                    throw ParseError(errorPos, Res.NeitherTypeConvertsToOther, type1, type2);
                }
            }

            return Expression.Condition(test, expr1, expr2);
        }

        private Expression GenerateConversion(Expression expr, Type type, int errorPos)
        {
            var exprType = expr.Type;
            if (exprType == type)
            {
                return expr;
            }

            if (exprType.IsValueType && type.IsValueType)
            {
                if ((IsNullableType(exprType) || IsNullableType(type)) &&
                    GetNonNullableType(exprType) == GetNonNullableType(type))
                {
                    return Expression.Convert(expr, type);
                }

                if ((IsNumericType(exprType) || IsEnumType(exprType)) &&
                    IsNumericType(type) || IsEnumType(type))
                {
                    return Expression.ConvertChecked(expr, type);
                }
            }

            if (exprType.IsAssignableFrom(type) || type.IsAssignableFrom(exprType) ||
                exprType.IsInterface || type.IsInterface)
            {
                return Expression.Convert(expr, type);
            }

            throw ParseError(
                errorPos,
                Res.CannotConvertValue,
                GetTypeName(exprType),
                GetTypeName(type));
        }

        private Expression GenerateEqual(Expression left, Expression right)
        {
            return Expression.Equal(left, right);
        }

        private Expression GenerateGreaterThan(Expression left, Expression right)
        {
            if (left.Type == typeof(string))
            {
                return Expression.GreaterThan(
                    GenerateStaticMethodCall("Compare", left, right),
                    Expression.Constant(0));
            }

            return Expression.GreaterThan(left, right);
        }

        private Expression GenerateGreaterThanEqual(Expression left, Expression right)
        {
            if (left.Type == typeof(string))
            {
                return Expression.GreaterThanOrEqual(
                    GenerateStaticMethodCall("Compare", left, right),
                    Expression.Constant(0));
            }

            return Expression.GreaterThanOrEqual(left, right);
        }

        private Expression GenerateLessThan(Expression left, Expression right)
        {
            if (left.Type == typeof(string))
            {
                return Expression.LessThan(
                    GenerateStaticMethodCall("Compare", left, right),
                    Expression.Constant(0));
            }

            return Expression.LessThan(left, right);
        }

        private Expression GenerateLessThanEqual(Expression left, Expression right)
        {
            if (left.Type == typeof(string))
            {
                return Expression.LessThanOrEqual(
                    GenerateStaticMethodCall("Compare", left, right),
                    Expression.Constant(0));
            }

            return Expression.LessThanOrEqual(left, right);
        }

        private Expression GenerateNotEqual(Expression left, Expression right)
        {
            return Expression.NotEqual(left, right);
        }

        private Expression GenerateStaticMethodCall(string methodName, Expression left, Expression right)
        {
            return Expression.Call(null, GetStaticMethod(methodName, left, right), new[] { left, right });
        }

        private Expression GenerateStringConcat(Expression left, Expression right)
        {
            return Expression.Call(
                null,
                typeof(string).GetMethod("Concat", new[] { typeof(object), typeof(object) }),
                new[] { left, right });
        }

        private Expression GenerateSubtract(Expression left, Expression right)
        {
            return Expression.Subtract(left, right);
        }

        private string GetIdentifier()
        {
            ValidateToken(TokenId.Identifier, Res.IdentifierExpected);
            var id = token.text;
            if (id.Length > 1 && id[0] == '@')
            {
                id = id.Substring(1);
            }

            return id;
        }

        private MethodInfo GetStaticMethod(string methodName, Expression left, Expression right)
        {
            return left.Type.GetMethod(methodName, new[] { left.Type, right.Type });
        }

        private Exception IncompatibleOperandsError(string opName, Expression left, Expression right, int pos)
        {
            return ParseError(
                pos,
                Res.IncompatibleOperands,
                opName,
                GetTypeName(left.Type),
                GetTypeName(right.Type));
        }

        private bool IsApplicable(MethodData method, Expression[] args)
        {
            if (method.Parameters.Length != args.Length)
            {
                return false;
            }

            var promotedArgs = new Expression[args.Length];
            for (var i = 0; i < args.Length; i++)
            {
                var pi = method.Parameters[i];
                if (pi.IsOut)
                {
                    return false;
                }

                var promoted = PromoteExpression(args[i], pi.ParameterType, false);
                if (promoted == null)
                {
                    return false;
                }

                promotedArgs[i] = promoted;
            }

            method.Args = promotedArgs;

            return true;
        }

        private void NextChar()
        {
            if (textPos < textLen)
            {
                textPos++;
            }

            ch = textPos < textLen
                ? text[textPos]
                : '\0';
        }

        private void NextToken()
        {
            while (char.IsWhiteSpace(ch))
            {
                NextChar();
            }

            TokenId t;
            var tokenPos = textPos;
            switch (ch)
            {
                case '!':
                    NextChar();
                    if (ch == '=')
                    {
                        NextChar();
                        t = TokenId.ExclamationEqual;
                    }
                    else
                    {
                        t = TokenId.Exclamation;
                    }

                    break;
                case '%':
                    NextChar();
                    t = TokenId.Percent;

                    break;
                case '&':
                    NextChar();
                    if (ch == '&')
                    {
                        NextChar();
                        t = TokenId.DoubleAmphersand;
                    }
                    else
                    {
                        t = TokenId.Amphersand;
                    }

                    break;
                case '(':
                    NextChar();
                    t = TokenId.OpenParen;

                    break;
                case ')':
                    NextChar();
                    t = TokenId.CloseParen;

                    break;
                case '*':
                    NextChar();
                    t = TokenId.Asterisk;

                    break;
                case '+':
                    NextChar();
                    t = TokenId.Plus;

                    break;
                case ',':
                    NextChar();
                    t = TokenId.Comma;

                    break;
                case '-':
                    NextChar();
                    t = TokenId.Minus;

                    break;
                case '.':
                    NextChar();
                    t = TokenId.Dot;

                    break;
                case '/':
                    NextChar();
                    t = TokenId.Slash;

                    break;
                case ':':
                    NextChar();
                    t = TokenId.Colon;

                    break;
                case '<':
                    NextChar();
                    if (ch == '=')
                    {
                        NextChar();
                        t = TokenId.LessThanEqual;
                    }
                    else if (ch == '>')
                    {
                        NextChar();
                        t = TokenId.LessGreater;
                    }
                    else
                    {
                        t = TokenId.LessThan;
                    }

                    break;
                case '=':
                    NextChar();
                    if (ch == '=')
                    {
                        NextChar();
                        t = TokenId.DoubleEqual;
                    }
                    else
                    {
                        t = TokenId.Equal;
                    }

                    break;
                case '>':
                    NextChar();
                    if (ch == '=')
                    {
                        NextChar();
                        t = TokenId.GreaterThanEqual;
                    }
                    else
                    {
                        t = TokenId.GreaterThan;
                    }

                    break;
                case '?':
                    NextChar();
                    t = TokenId.Question;

                    break;
                case '[':
                    NextChar();
                    t = TokenId.OpenBracket;

                    break;
                case ']':
                    NextChar();
                    t = TokenId.CloseBracket;

                    break;
                case '|':
                    NextChar();
                    if (ch == '|')
                    {
                        NextChar();
                        t = TokenId.DoubleBar;
                    }
                    else
                    {
                        t = TokenId.Bar;
                    }

                    break;
                case '"':
                case '\'':
                    var quote = ch;
                    do
                    {
                        NextChar();
                        while (textPos < textLen && ch != quote)
                        {
                            NextChar();
                        }

                        if (textPos == textLen)
                        {
                            throw ParseError(textPos, Res.UnterminatedStringLiteral);
                        }

                        NextChar();
                    }
                    while (ch == quote);

                    t = TokenId.StringLiteral;

                    break;
                default:
                    if (char.IsLetter(ch) || ch == '@' || ch == '_')
                    {
                        do
                        {
                            NextChar();
                        }
                        while (char.IsLetterOrDigit(ch) || ch == '_');

                        t = TokenId.Identifier;

                        break;
                    }

                    if (char.IsDigit(ch))
                    {
                        t = TokenId.IntegerLiteral;
                        do
                        {
                            NextChar();
                        }
                        while (char.IsDigit(ch));

                        if (ch == '.')
                        {
                            t = TokenId.RealLiteral;
                            NextChar();
                            ValidateDigit();
                            do
                            {
                                NextChar();
                            }
                            while (char.IsDigit(ch));
                        }

                        if (ch == 'E' || ch == 'e')
                        {
                            t = TokenId.RealLiteral;
                            NextChar();
                            if (ch == '+' || ch == '-')
                            {
                                NextChar();
                            }

                            ValidateDigit();
                            do
                            {
                                NextChar();
                            }
                            while (char.IsDigit(ch));
                        }

                        if (ch == 'F' || ch == 'f')
                        {
                            NextChar();
                        }

                        break;
                    }

                    if (textPos == textLen)
                    {
                        t = TokenId.End;

                        break;
                    }

                    throw ParseError(textPos, Res.InvalidCharacter, ch);
            }

            token.id = t;
            token.text = text.Substring(tokenPos, textPos - tokenPos);
            token.pos = tokenPos;
        }

        // +, -, & operators
        private Expression ParseAdditive()
        {
            var left = ParseMultiplicative();
            while (token.id == TokenId.Plus || token.id == TokenId.Minus ||
                token.id == TokenId.Amphersand)
            {
                var op = token;
                NextToken();
                var right = ParseMultiplicative();
                switch (op.id)
                {
                    case TokenId.Plus:
                        if (left.Type == typeof(string) || right.Type == typeof(string))
                        {
                            goto case TokenId.Amphersand;
                        }

                        CheckAndPromoteOperands(typeof(IAddSignatures), op.text, ref left, ref right, op.pos);
                        left = GenerateAdd(left, right);

                        break;
                    case TokenId.Minus:
                        CheckAndPromoteOperands(typeof(ISubtractSignatures), op.text, ref left, ref right, op.pos);
                        left = GenerateSubtract(left, right);

                        break;
                    case TokenId.Amphersand:
                        left = GenerateStringConcat(left, right);

                        break;
                }
            }

            return left;
        }

        private Expression ParseAggregate(Expression instance, Type elementType, string methodName, int errorPos)
        {
            var outerIt = it;
            var innerIt = Expression.Parameter(elementType, "");
            it = innerIt;
            var args = ParseArgumentList();
            it = outerIt;
            MethodBase signature;
            if (FindMethod(typeof(IEnumerableSignatures), methodName, false, args, out signature) != 1)
            {
                throw ParseError(errorPos, Res.NoApplicableAggregate, methodName);
            }

            Type[] typeArgs;
            if (signature.Name == "Min" || signature.Name == "Max")
            {
                typeArgs = new[] { elementType, args[0].Type };
            }
            else
            {
                typeArgs = new[] { elementType };
            }

            if (args.Length == 0)
            {
                args = new[] { instance };
            }
            else
            {
                args = new[] { instance, Expression.Lambda(args[0], innerIt) };
            }

            return Expression.Call(typeof(Enumerable), signature.Name, typeArgs, args);
        }

        private Expression[] ParseArgumentList()
        {
            ValidateToken(TokenId.OpenParen, Res.OpenParenExpected);
            NextToken();
            var args = token.id != TokenId.CloseParen
                ? ParseArguments()
                : new Expression[0];

            ValidateToken(TokenId.CloseParen, Res.CloseParenOrCommaExpected);
            NextToken();

            return args;
        }

        private Expression[] ParseArguments()
        {
            var argList = new List<Expression>();
            while (true)
            {
                argList.Add(ParseExpression());
                if (token.id != TokenId.Comma)
                {
                    break;
                }

                NextToken();
            }

            return argList.ToArray();
        }

        // =, ==, !=, <>, >, >=, <, <= operators
        private Expression ParseComparison()
        {
            var left = ParseAdditive();
            while (token.id == TokenId.Equal || token.id == TokenId.DoubleEqual ||
                token.id == TokenId.ExclamationEqual || token.id == TokenId.LessGreater ||
                token.id == TokenId.GreaterThan || token.id == TokenId.GreaterThanEqual ||
                token.id == TokenId.LessThan || token.id == TokenId.LessThanEqual)
            {
                var op = token;
                NextToken();
                var right = ParseAdditive();
                var isEquality = op.id == TokenId.Equal || op.id == TokenId.DoubleEqual ||
                    op.id == TokenId.ExclamationEqual || op.id == TokenId.LessGreater;

                if (isEquality && !left.Type.IsValueType && !right.Type.IsValueType)
                {
                    if (left.Type != right.Type)
                    {
                        if (left.Type.IsAssignableFrom(right.Type))
                        {
                            right = Expression.Convert(right, left.Type);
                        }
                        else if (right.Type.IsAssignableFrom(left.Type))
                        {
                            left = Expression.Convert(left, right.Type);
                        }
                        else
                        {
                            throw IncompatibleOperandsError(op.text, left, right, op.pos);
                        }
                    }
                }
                else if (IsEnumType(left.Type) || IsEnumType(right.Type))
                {
                    if (left.Type != right.Type)
                    {
                        Expression e;
                        if ((e = PromoteExpression(right, left.Type, true)) != null)
                        {
                            right = e;
                        }
                        else if ((e = PromoteExpression(left, right.Type, true)) != null)
                        {
                            left = e;
                        }
                        else
                        {
                            throw IncompatibleOperandsError(op.text, left, right, op.pos);
                        }
                    }
                }
                else
                {
                    CheckAndPromoteOperands(
                        isEquality
                            ? typeof(IEqualitySignatures)
                            : typeof(IRelationalSignatures),
                        op.text,
                        ref left,
                        ref right,
                        op.pos);
                }

                switch (op.id)
                {
                    case TokenId.Equal:
                    case TokenId.DoubleEqual:
                        left = GenerateEqual(left, right);

                        break;
                    case TokenId.ExclamationEqual:
                    case TokenId.LessGreater:
                        left = GenerateNotEqual(left, right);

                        break;
                    case TokenId.GreaterThan:
                        left = GenerateGreaterThan(left, right);

                        break;
                    case TokenId.GreaterThanEqual:
                        left = GenerateGreaterThanEqual(left, right);

                        break;
                    case TokenId.LessThan:
                        left = GenerateLessThan(left, right);

                        break;
                    case TokenId.LessThanEqual:
                        left = GenerateLessThanEqual(left, right);

                        break;
                }
            }

            return left;
        }

        private Expression ParseElementAccess(Expression expr)
        {
            var errorPos = token.pos;
            ValidateToken(TokenId.OpenBracket, Res.OpenParenExpected);
            NextToken();
            var args = ParseArguments();
            ValidateToken(TokenId.CloseBracket, Res.CloseBracketOrCommaExpected);
            NextToken();
            if (expr.Type.IsArray)
            {
                if (expr.Type.GetArrayRank() != 1 || args.Length != 1)
                {
                    throw ParseError(errorPos, Res.CannotIndexMultiDimArray);
                }

                var index = PromoteExpression(args[0], typeof(int), true);
                if (index == null)
                {
                    throw ParseError(errorPos, Res.InvalidIndex);
                }

                return Expression.ArrayIndex(expr, index);
            }

            MethodBase mb;
            switch (FindIndexer(expr.Type, args, out mb))
            {
                case 0:
                    throw ParseError(
                        errorPos,
                        Res.NoApplicableIndexer,
                        GetTypeName(expr.Type));
                case 1:
                    return Expression.Call(expr, (MethodInfo)mb, args);
                default:
                    throw ParseError(
                        errorPos,
                        Res.AmbiguousIndexerInvocation,
                        GetTypeName(expr.Type));
            }
        }

        private Exception ParseError(string format, params object[] args)
        {
            return ParseError(token.pos, format, args);
        }

        private Exception ParseError(int pos, string format, params object[] args)
        {
            return new ParseException(string.Format(CultureInfo.CurrentCulture, format, args), pos);
        }

        // ?: operator
        private Expression ParseExpression()
        {
            var errorPos = token.pos;
            var expr = ParseLogicalOr();
            if (token.id == TokenId.Question)
            {
                NextToken();
                var expr1 = ParseExpression();
                ValidateToken(TokenId.Colon, Res.ColonExpected);
                NextToken();
                var expr2 = ParseExpression();
                expr = GenerateConditional(expr, expr1, expr2, errorPos);
            }

            return expr;
        }

        private Expression ParseIdentifier()
        {
            ValidateToken(TokenId.Identifier);
            object value;
            if (keywords.TryGetValue(token.text, out value))
            {
                if (value is Type)
                {
                    return ParseTypeAccess((Type)value);
                }

                if (value == keywordIt)
                {
                    return ParseIt();
                }

                if (value == keywordIif)
                {
                    return ParseIif();
                }

                if (value == keywordNew)
                {
                    return ParseNew();
                }

                NextToken();

                return (Expression)value;
            }

            if (symbols.TryGetValue(token.text, out value) ||
                externals != null && externals.TryGetValue(token.text, out value))
            {
                var expr = value as Expression;
                if (expr == null)
                {
                    expr = Expression.Constant(value);
                }
                else
                {
                    var lambda = expr as LambdaExpression;
                    if (lambda != null)
                    {
                        return ParseLambdaInvocation(lambda);
                    }
                }

                NextToken();

                return expr;
            }

            if (it != null)
            {
                return ParseMemberAccess(null, it);
            }

            throw ParseError(Res.UnknownIdentifier, token.text);
        }

        private Expression ParseIif()
        {
            var errorPos = token.pos;
            NextToken();
            var args = ParseArgumentList();
            if (args.Length != 3)
            {
                throw ParseError(errorPos, Res.IifRequiresThreeArgs);
            }

            return GenerateConditional(args[0], args[1], args[2], errorPos);
        }

        private Expression ParseIntegerLiteral()
        {
            ValidateToken(TokenId.IntegerLiteral);
            var text = token.text;
            if (text[0] != '-')
            {
                ulong value;
                if (!ulong.TryParse(text, out value))
                {
                    throw ParseError(Res.InvalidIntegerLiteral, text);
                }

                NextToken();
                if (value <= int.MaxValue)
                {
                    return CreateLiteral((int)value, text);
                }

                if (value <= uint.MaxValue)
                {
                    return CreateLiteral((uint)value, text);
                }

                if (value <= long.MaxValue)
                {
                    return CreateLiteral((long)value, text);
                }

                return CreateLiteral(value, text);
            }
            else
            {
                long value;
                if (!long.TryParse(text, out value))
                {
                    throw ParseError(Res.InvalidIntegerLiteral, text);
                }

                NextToken();
                if (value >= int.MinValue && value <= int.MaxValue)
                {
                    return CreateLiteral((int)value, text);
                }

                return CreateLiteral(value, text);
            }
        }

        private Expression ParseIt()
        {
            if (it == null)
            {
                throw ParseError(Res.NoItInScope);
            }

            NextToken();

            return it;
        }

        private Expression ParseLambdaInvocation(LambdaExpression lambda)
        {
            var errorPos = token.pos;
            NextToken();
            var args = ParseArgumentList();
            MethodBase method;
            if (FindMethod(lambda.Type, "Invoke", false, args, out method) != 1)
            {
                throw ParseError(errorPos, Res.ArgsIncompatibleWithLambda);
            }

            return Expression.Invoke(lambda, args);
        }

        // &&, and operator
        private Expression ParseLogicalAnd()
        {
            var left = ParseComparison();
            while (token.id == TokenId.DoubleAmphersand || TokenIdentifierIs("and"))
            {
                var op = token;
                NextToken();
                var right = ParseComparison();
                CheckAndPromoteOperands(typeof(ILogicalSignatures), op.text, ref left, ref right, op.pos);
                left = Expression.AndAlso(left, right);
            }

            return left;
        }

        // ||, or operator
        private Expression ParseLogicalOr()
        {
            var left = ParseLogicalAnd();
            while (token.id == TokenId.DoubleBar || TokenIdentifierIs("or"))
            {
                var op = token;
                NextToken();
                var right = ParseLogicalAnd();
                CheckAndPromoteOperands(typeof(ILogicalSignatures), op.text, ref left, ref right, op.pos);
                left = Expression.OrElse(left, right);
            }

            return left;
        }

        private Expression ParseMemberAccess(Type type, Expression instance)
        {
            if (instance != null)
            {
                type = instance.Type;
            }

            var errorPos = token.pos;
            var id = GetIdentifier();
            NextToken();
            if (token.id == TokenId.OpenParen)
            {
                if (instance != null && type != typeof(string))
                {
                    var enumerableType = FindGenericType(typeof(IEnumerable<>), type);
                    if (enumerableType != null)
                    {
                        var elementType = enumerableType.GetGenericArguments()[0];

                        return ParseAggregate(instance, elementType, id, errorPos);
                    }
                }

                var args = ParseArgumentList();
                MethodBase mb;
                switch (FindMethod(type, id, instance == null, args, out mb))
                {
                    case 0:
                        throw ParseError(
                            errorPos,
                            Res.NoApplicableMethod,
                            id,
                            GetTypeName(type));
                    case 1:
                        var method = (MethodInfo)mb;
                        if (!IsPredefinedType(method.DeclaringType))
                        {
                            throw ParseError(errorPos, Res.MethodsAreInaccessible, GetTypeName(method.DeclaringType));
                        }

                        if (method.ReturnType == typeof(void))
                        {
                            throw ParseError(
                                errorPos,
                                Res.MethodIsVoid,
                                id,
                                GetTypeName(method.DeclaringType));
                        }

                        return Expression.Call(instance, method, args);
                    default:
                        throw ParseError(
                            errorPos,
                            Res.AmbiguousMethodInvocation,
                            id,
                            GetTypeName(type));
                }
            }

            var member = FindPropertyOrField(type, id, instance == null);
            if (member == null)
            {
                throw ParseError(
                    errorPos,
                    Res.UnknownPropertyOrField,
                    id,
                    GetTypeName(type));
            }

            return member is PropertyInfo
                ? Expression.Property(instance, (PropertyInfo)member)
                : Expression.Field(instance, (FieldInfo)member);
        }

        // *, /, %, mod operators
        private Expression ParseMultiplicative()
        {
            var left = ParseUnary();
            while (token.id == TokenId.Asterisk || token.id == TokenId.Slash ||
                token.id == TokenId.Percent || TokenIdentifierIs("mod"))
            {
                var op = token;
                NextToken();
                var right = ParseUnary();
                CheckAndPromoteOperands(typeof(IArithmeticSignatures), op.text, ref left, ref right, op.pos);
                switch (op.id)
                {
                    case TokenId.Asterisk:
                        left = Expression.Multiply(left, right);

                        break;
                    case TokenId.Slash:
                        left = Expression.Divide(left, right);

                        break;
                    case TokenId.Percent:
                    case TokenId.Identifier:
                        left = Expression.Modulo(left, right);

                        break;
                }
            }

            return left;
        }

        private Expression ParseNew()
        {
            NextToken();
            ValidateToken(TokenId.OpenParen, Res.OpenParenExpected);
            NextToken();
            var properties = new List<DynamicProperty>();
            var expressions = new List<Expression>();
            while (true)
            {
                var exprPos = token.pos;
                var expr = ParseExpression();
                string propName;
                if (TokenIdentifierIs("as"))
                {
                    NextToken();
                    propName = GetIdentifier();
                    NextToken();
                }
                else
                {
                    var me = expr as MemberExpression;
                    if (me == null)
                    {
                        throw ParseError(exprPos, Res.MissingAsClause);
                    }

                    propName = me.Member.Name;
                }

                expressions.Add(expr);
                properties.Add(new DynamicProperty(propName, expr.Type));
                if (token.id != TokenId.Comma)
                {
                    break;
                }

                NextToken();
            }

            ValidateToken(TokenId.CloseParen, Res.CloseParenOrCommaExpected);
            NextToken();
            var type = DynamicExpression.CreateClass(properties);
            var bindings = new MemberBinding[properties.Count];
            for (var i = 0; i < bindings.Length; i++)
            {
                bindings[i] = Expression.Bind(type.GetProperty(properties[i].Name), expressions[i]);
            }

            return Expression.MemberInit(Expression.New(type), bindings);
        }

        private Expression ParseParenExpression()
        {
            ValidateToken(TokenId.OpenParen, Res.OpenParenExpected);
            NextToken();
            var e = ParseExpression();
            ValidateToken(TokenId.CloseParen, Res.CloseParenOrOperatorExpected);
            NextToken();

            return e;
        }

        private Expression ParsePrimary()
        {
            var expr = ParsePrimaryStart();
            while (true)
            {
                if (token.id == TokenId.Dot)
                {
                    NextToken();
                    expr = ParseMemberAccess(null, expr);
                }
                else if (token.id == TokenId.OpenBracket)
                {
                    expr = ParseElementAccess(expr);
                }
                else
                {
                    break;
                }
            }

            return expr;
        }

        private Expression ParsePrimaryStart()
        {
            switch (token.id)
            {
                case TokenId.Identifier:
                    return ParseIdentifier();
                case TokenId.StringLiteral:
                    return ParseStringLiteral();
                case TokenId.IntegerLiteral:
                    return ParseIntegerLiteral();
                case TokenId.RealLiteral:
                    return ParseRealLiteral();
                case TokenId.OpenParen:
                    return ParseParenExpression();
                default:
                    throw ParseError(Res.ExpressionExpected);
            }
        }

        private Expression ParseRealLiteral()
        {
            ValidateToken(TokenId.RealLiteral);
            var text = token.text;
            object value = null;
            var last = text[text.Length - 1];
            if (last == 'F' || last == 'f')
            {
                float f;
                if (float.TryParse(text.Substring(0, text.Length - 1), out f))
                {
                    value = f;
                }
            }
            else
            {
                double d;
                if (double.TryParse(text, out d))
                {
                    value = d;
                }
            }

            if (value == null)
            {
                throw ParseError(Res.InvalidRealLiteral, text);
            }

            NextToken();

            return CreateLiteral(value, text);
        }

        private Expression ParseStringLiteral()
        {
            ValidateToken(TokenId.StringLiteral);
            var quote = token.text[0];
            var s = token.text.Substring(1, token.text.Length - 2);
            var start = 0;
            while (true)
            {
                var i = s.IndexOf(quote, start);
                if (i < 0)
                {
                    break;
                }

                s = s.Remove(i, 1);
                start = i + 1;
            }

            if (quote == '\'')
            {
                if (s.Length != 1)
                {
                    throw ParseError(Res.InvalidCharacterLiteral);
                }

                NextToken();

                return CreateLiteral(s[0], s);
            }

            NextToken();

            return CreateLiteral(s, s);
        }

        private Expression ParseTypeAccess(Type type)
        {
            var errorPos = token.pos;
            NextToken();
            if (token.id == TokenId.Question)
            {
                if (!type.IsValueType || IsNullableType(type))
                {
                    throw ParseError(errorPos, Res.TypeHasNoNullableForm, GetTypeName(type));
                }

                type = typeof(Nullable<>).MakeGenericType(type);
                NextToken();
            }

            if (token.id == TokenId.OpenParen)
            {
                var args = ParseArgumentList();
                MethodBase method;
                switch (FindBestMethod(type.GetConstructors(), args, out method))
                {
                    case 0:
                        if (args.Length == 1)
                        {
                            return GenerateConversion(args[0], type, errorPos);
                        }

                        throw ParseError(errorPos, Res.NoMatchingConstructor, GetTypeName(type));
                    case 1:
                        return Expression.New((ConstructorInfo)method, args);
                    default:
                        throw ParseError(errorPos, Res.AmbiguousConstructorInvocation, GetTypeName(type));
                }
            }

            ValidateToken(TokenId.Dot, Res.DotOrOpenParenExpected);
            NextToken();

            return ParseMemberAccess(type, null);
        }

        // -, !, not unary operators
        private Expression ParseUnary()
        {
            if (token.id == TokenId.Minus || token.id == TokenId.Exclamation ||
                TokenIdentifierIs("not"))
            {
                var op = token;
                NextToken();
                if (op.id == TokenId.Minus && (token.id == TokenId.IntegerLiteral ||
                    token.id == TokenId.RealLiteral))
                {
                    token.text = "-" + token.text;
                    token.pos = op.pos;

                    return ParsePrimary();
                }

                var expr = ParseUnary();
                if (op.id == TokenId.Minus)
                {
                    CheckAndPromoteOperand(typeof(INegationSignatures), op.text, ref expr, op.pos);
                    expr = Expression.Negate(expr);
                }
                else
                {
                    CheckAndPromoteOperand(typeof(INotSignatures), op.text, ref expr, op.pos);
                    expr = Expression.Not(expr);
                }

                return expr;
            }

            return ParsePrimary();
        }

        private void ProcessParameters(ParameterExpression[] parameters)
        {
            foreach (var pe in parameters)
            {
                if (!string.IsNullOrEmpty(pe.Name))
                {
                    AddSymbol(pe.Name, pe);
                }
            }

            if (parameters.Length == 1 && string.IsNullOrEmpty(parameters[0].Name))
            {
                it = parameters[0];
            }
        }

        private void ProcessValues(object[] values)
        {
            for (var i = 0; i < values.Length; i++)
            {
                var value = values[i];
                if (i == values.Length - 1 && value is IDictionary<string, object>)
                {
                    externals = (IDictionary<string, object>)value;
                }
                else
                {
                    AddSymbol("@" + i.ToString(CultureInfo.InvariantCulture), value);
                }
            }
        }

        private Expression PromoteExpression(Expression expr, Type type, bool exact)
        {
            if (expr.Type == type)
            {
                return expr;
            }

            if (expr is ConstantExpression)
            {
                var ce = (ConstantExpression)expr;
                if (ce == nullLiteral)
                {
                    if (!type.IsValueType || IsNullableType(type))
                    {
                        return Expression.Constant(null, type);
                    }
                }
                else
                {
                    string text;
                    if (literals.TryGetValue(ce, out text))
                    {
                        var target = GetNonNullableType(type);
                        object value = null;
                        switch (Type.GetTypeCode(ce.Type))
                        {
                            case TypeCode.Int32:
                            case TypeCode.UInt32:
                            case TypeCode.Int64:
                            case TypeCode.UInt64:
                                value = ParseNumber(text, target);

                                break;
                            case TypeCode.Double:
                                if (target == typeof(decimal))
                                {
                                    value = ParseNumber(text, target);
                                }

                                break;
                            case TypeCode.String:
                                value = ParseEnum(text, target);

                                break;
                        }

                        if (value != null)
                        {
                            return Expression.Constant(value, type);
                        }
                    }
                }
            }

            if (IsCompatibleWith(expr.Type, type))
            {
                if (type.IsValueType || exact)
                {
                    return Expression.Convert(expr, type);
                }

                return expr;
            }

            return null;
        }

        private void SetTextPos(int pos)
        {
            textPos = pos;
            ch = textPos < textLen
                ? text[textPos]
                : '\0';
        }

        private bool TokenIdentifierIs(string id)
        {
            return token.id == TokenId.Identifier && string.Equals(id, token.text, StringComparison.OrdinalIgnoreCase);
        }

        private void ValidateDigit()
        {
            if (!char.IsDigit(ch))
            {
                throw ParseError(textPos, Res.DigitExpected);
            }
        }

        private void ValidateToken(TokenId t, string errorMessage)
        {
            if (token.id != t)
            {
                throw ParseError(errorMessage);
            }
        }

        private void ValidateToken(TokenId t)
        {
            if (token.id != t)
            {
                throw ParseError(Res.SyntaxError);
            }
        }

        private struct Token
        {
            public TokenId id;

            public string text;

            public int pos;
        }

        private class MethodData
        {
            public Expression[] Args;

            public MethodBase MethodBase;

            public ParameterInfo[] Parameters;
        }
    }

    internal static class Res
    {
        public const string DuplicateIdentifier = "The identifier '{0}' was defined more than once";

        public const string ExpressionTypeMismatch = "Expression of type '{0}' expected";

        public const string ExpressionExpected = "Expression expected";

        public const string InvalidCharacterLiteral = "Character literal must contain exactly one character";

        public const string InvalidIntegerLiteral = "Invalid integer literal '{0}'";

        public const string InvalidRealLiteral = "Invalid real literal '{0}'";

        public const string UnknownIdentifier = "Unknown identifier '{0}'";

        public const string NoItInScope = "No 'it' is in scope";

        public const string IifRequiresThreeArgs = "The 'iif' function requires three arguments";

        public const string FirstExprMustBeBool = "The first expression must be of type 'Boolean'";

        public const string BothTypesConvertToOther = "Both of the types '{0}' and '{1}' convert to the other";

        public const string NeitherTypeConvertsToOther = "Neither of the types '{0}' and '{1}' converts to the other";

        public const string MissingAsClause = "Expression is missing an 'as' clause";

        public const string ArgsIncompatibleWithLambda = "Argument list incompatible with lambda expression";

        public const string TypeHasNoNullableForm = "Type '{0}' has no nullable form";

        public const string NoMatchingConstructor = "No matching constructor in type '{0}'";

        public const string AmbiguousConstructorInvocation = "Ambiguous invocation of '{0}' constructor";

        public const string CannotConvertValue = "A value of type '{0}' cannot be converted to type '{1}'";

        public const string NoApplicableMethod = "No applicable method '{0}' exists in type '{1}'";

        public const string MethodsAreInaccessible = "Methods on type '{0}' are not accessible";

        public const string MethodIsVoid = "Method '{0}' in type '{1}' does not return a value";

        public const string AmbiguousMethodInvocation = "Ambiguous invocation of method '{0}' in type '{1}'";

        public const string UnknownPropertyOrField = "No property or field '{0}' exists in type '{1}'";

        public const string NoApplicableAggregate = "No applicable aggregate method '{0}' exists";

        public const string CannotIndexMultiDimArray = "Indexing of multi-dimensional arrays is not supported";

        public const string InvalidIndex = "Array index must be an integer expression";

        public const string NoApplicableIndexer = "No applicable indexer exists in type '{0}'";

        public const string AmbiguousIndexerInvocation = "Ambiguous invocation of indexer in type '{0}'";

        public const string IncompatibleOperand = "Operator '{0}' incompatible with operand type '{1}'";

        public const string IncompatibleOperands = "Operator '{0}' incompatible with operand types '{1}' and '{2}'";

        public const string UnterminatedStringLiteral = "Unterminated string literal";

        public const string InvalidCharacter = "Syntax error '{0}'";

        public const string DigitExpected = "Digit expected";

        public const string SyntaxError = "Syntax error";

        public const string TokenExpected = "{0} expected";

        public const string ParseExceptionFormat = "{0} (at index {1})";

        public const string ColonExpected = "':' expected";

        public const string OpenParenExpected = "'(' expected";

        public const string CloseParenOrOperatorExpected = "')' or operator expected";

        public const string CloseParenOrCommaExpected = "')' or ',' expected";

        public const string DotOrOpenParenExpected = "'.' or '(' expected";

        public const string OpenBracketExpected = "'[' expected";

        public const string CloseBracketOrCommaExpected = "']' or ',' expected";

        public const string IdentifierExpected = "Identifier expected";
    }
}
