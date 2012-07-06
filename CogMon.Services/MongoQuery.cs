using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using MongoDB.Driver.Builders;
using MongoDB.Driver;
using MongoDB.Bson;

namespace CogMon.Services
{
    /// <summary>
    /// Mongo query AST node
    /// </summary>
    public abstract class QueryNode
    {
        public AndNode And(QueryNode rhs)
        {
            return new AndNode(this, rhs);
        }

        public OrNode Or(QueryNode rhs)
        {
            return new OrNode(this, rhs);
        }

        public QueryNode Add(QueryNode rhs)
        {
            return new PlusNode(this, rhs);
        }

        public NotNode Not()
        {
            return new NotNode(this);
        }

        public static bool operator true(QueryNode expr)
        {
            return false; // never true to disable short-circuit evaluation of a || b
        }

        public static bool operator false(QueryNode expr)
        {
            return false; // never false to disable short-circuit evaluation of a && b
        }

        public static QueryNode operator &(QueryNode lhs, QueryNode rhs)
        {
            return lhs.And(rhs);
        }

        public static QueryNode operator |(QueryNode lhs, QueryNode rhs)
        {
            return lhs.Or(rhs);
        }

        public static QueryNode operator +(QueryNode lhs, QueryNode rhs)
        {
            return lhs.Add(rhs);
        }


        public static QueryNode operator !(QueryNode n)
        {
            return n.Not();
        }

        public static QueryNode Raw(string query)
        {
            return new RawQuery(query);
        }

        public override string ToString()
        {
            throw new NotImplementedException();
        }

        public abstract T Accept<T>(IQueryNodeVisitor<T> visitor);
    }

    public class RawQuery : QueryNode
    {
        private string _q;
        public RawQuery(string q)
        {
            _q = q;
        }

        public string QueryText
        {
            get { return _q; }
        }

        public override string ToString()
        {
            return _q;
        }

        public override T Accept<T>(IQueryNodeVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }
    }



    public class AndNode : QueryNode
    {
        public QueryNode Left { get; set; }
        public QueryNode Right { get; set; }
        public AndNode(QueryNode left, QueryNode right)
        {
            Left = left;
            Right = right;
        }

        public override string ToString()
        {
            return string.Format("({0}) AND ({1})", Left.ToString(), Right.ToString());
        }


        public override T Accept<T>(IQueryNodeVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }

    }

    public class PlusNode : QueryNode
    {
        public QueryNode Left { get; set; }
        public QueryNode Right { get; set; }
        public PlusNode(QueryNode left, QueryNode right)
        {
            Left = left;
            Right = right;
        }

        public override string ToString()
        {
            return string.Format("{0} {1}", Left.ToString(), Right.ToString());
        }

        public override T Accept<T>(IQueryNodeVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }
    }

    public class NotNode : QueryNode
    {
        public QueryNode Inner { get; set; }

        public NotNode(QueryNode q)
        {
            Inner = q;
        }

        public override string ToString()
        {
            return string.Format("NOT ({0})", Inner.ToString());
        }

        public override T Accept<T>(IQueryNodeVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }
    }

    public class OrNode : QueryNode
    {
        public QueryNode Left { get; set; }
        public QueryNode Right { get; set; }

        public OrNode(QueryNode left, QueryNode right)
        {
            Left = left;
            Right = right;
        }

        public override string ToString()
        {
            return string.Format("({0}) OR ({1})", Left.ToString(), Right.ToString());
        }

        public override T Accept<T>(IQueryNodeVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }
    }

    public class FieldPredicate : QueryNode
    {
        public Field Fld { get; set; }
        public enum OperatorType
        {
            EQ,
            LT,
            GT,
            NE,
            IN,
            RANGE,
            LK,
            NULL,
            NOTNULL,
            GTE,
            LTE
        }
        public OperatorType Operator { get; set; }
        public object Value { get; set; }
        public object Value2 { get; set; }

        private static string FormatFieldValue(object v)
        {
            if (v == null) return null;
            var nt = Nullable.GetUnderlyingType(v.GetType());
            if (nt != null)
            {
                PropertyInfo pi = v.GetType().GetProperty("HasValue");
                bool b = (bool)pi.GetValue(v, null);
                if (!b) return null;
                pi = v.GetType().GetProperty("Value");
                v = pi.GetValue(v, null);
            }
            if (v is DateTime)
            {
                return QueryBuilder.FormatQueryDate((DateTime)v);
            }
            else if (v is string)
            {
                return QueryBuilder.EscapeQueryText((string)v);
            }
            else
            {
                return Convert.ToString(v);
            }
        }

        public override string ToString()
        {
            if (Fld.Name == null)
            {
                return Convert.ToString(Value); //default field query or a raw query...
            }
            switch (Operator)
            {
                case OperatorType.EQ:
                    return string.Format("{0}:{1}", Fld.Name, FormatFieldValue(Value));
                case OperatorType.LK:
                    var str = Convert.ToString(Value);
                    if (str.EndsWith("*")) str = str.Substring(0, str.Length - 1);
                    return string.Format("{0}:{1}*", Fld.Name, QueryBuilder.EscapeQueryText(str) + "*");
                case OperatorType.NE:
                    return string.Format("-{0}:{1}", Fld.Name, FormatFieldValue(Value));
                case OperatorType.NULL:
                    return string.Format("-{0}", Fld.Name);
                case OperatorType.NOTNULL:
                    return string.Format("{0}:[* TO *]", Fld.Name);
                case OperatorType.RANGE:
                    string s1 = FormatFieldValue(Value);
                    string s2 = FormatFieldValue(Value2);
                    return string.Format("{0}:[{1} TO {2}]", Fld.Name, s1 == null ? "*" : s1, s2 == null ? "*" : s2);
                case OperatorType.LT:
                    return string.Format("{0}:[* TO {1}]", Fld.Name, FormatFieldValue(Value));
                case OperatorType.GT:
                    return string.Format("{0}:[{1} TO *]", Fld.Name, FormatFieldValue(Value));
                case OperatorType.IN:
                    StringBuilder sb = new StringBuilder();
                    List<object> l = Value as List<object>;
                    if (l == null) throw new Exception("Invalid argument in field " + Fld.Name);
                    foreach (object v in l)
                    {
                        if (sb.Length > 0) sb.Append(" OR ");
                        sb.Append(FormatFieldValue(v));
                    }
                    return string.Format("{0}:({1})", Fld.Name, sb);
                default:
                    throw new NotImplementedException(string.Format("Field: {0}, Operator: {1}, Value: {2}", Fld.Name, Operator, Value));
            }

        }

        public override T Accept<T>(IQueryNodeVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }
    }

    public class Field
    {
        public string Name { get; set; }

        public FieldPredicate EQ(object v)
        {
            if (v == null) return this.IsNull();
            return new FieldPredicate { Fld = this, Operator = FieldPredicate.OperatorType.EQ, Value = v };
        }

        public static FieldPredicate operator ==(Field lhs, object rhs)
        {
            return lhs.EQ(rhs);
        }

        public FieldPredicate NE(object v)
        {
            if (v == null) return this.IsNotNull();
            return new FieldPredicate { Fld = this, Operator = FieldPredicate.OperatorType.NE, Value = v };
        }

        public static FieldPredicate operator !=(Field lhs, object rhs)
        {
            return lhs.NE(rhs);
        }

        public FieldPredicate LT(object v)
        {
            return new FieldPredicate { Fld = this, Operator = FieldPredicate.OperatorType.LT, Value = v };
        }

        public static FieldPredicate operator <(Field lhs, object rhs)
        {
            return lhs.LT(rhs);
        }

        public FieldPredicate LTE(object v)
        {
            return new FieldPredicate { Fld = this, Operator = FieldPredicate.OperatorType.LTE, Value = v };
        }

        public static FieldPredicate operator <=(Field lhs, object rhs)
        {
            return lhs.LTE(rhs);
        }

        public FieldPredicate GT(object v)
        {
            return new FieldPredicate { Fld = this, Operator = FieldPredicate.OperatorType.GT, Value = v };
        }


        public static FieldPredicate operator >(Field lhs, object rhs)
        {
            return lhs.GT(rhs);
        }

        public FieldPredicate GTE(object v)
        {
            return new FieldPredicate { Fld = this, Operator = FieldPredicate.OperatorType.GTE, Value = v };
        }


        public static FieldPredicate operator >=(Field lhs, object rhs)
        {
            return lhs.GTE(rhs);
        }

        public FieldPredicate In<T>(IEnumerable<T> values)
        {
            List<object> l = new List<object>();
            foreach (var v in values)
            {
                l.Add(v);
            }
            return new FieldPredicate { Fld = this, Operator = FieldPredicate.OperatorType.IN, Value = l };
        }

        

        public FieldPredicate Between<T>(T min, T max)
        {
            return new FieldPredicate { Fld = this, Operator = FieldPredicate.OperatorType.RANGE, Value = min, Value2 = max };
        }

        public FieldPredicate Like(string t)
        {
            return new FieldPredicate { Fld = this, Operator = FieldPredicate.OperatorType.LK, Value = t };
        }

        public FieldPredicate IsNull()
        {
            return new FieldPredicate { Fld = this, Operator = FieldPredicate.OperatorType.NULL, Value = null };
        }

        public FieldPredicate IsNotNull()
        {
            return new FieldPredicate { Fld = this, Operator = FieldPredicate.OperatorType.NOTNULL, Value = null };
        }


        public static Field Named(string name)
        {
            return new Field(name);
        }

        public static Field Default
        {
            get
            {
                return Field.Named("_default");
            }
        }

        public Field(string name)
        {
            Name = name;
        }
    }




    internal class DynamicField : System.Dynamic.DynamicObject
    {
        public override bool TryGetMember(System.Dynamic.GetMemberBinder binder, out object result)
        {
            result = Field.Named(binder.Name);
            return true;
        }
    }

    public interface IQueryNodeVisitor<T>
    {
        T Visit(AndNode n);
        T Visit(OrNode n);
        T Visit(FieldPredicate n);
        T Visit(NotNode n);
        T Visit(PlusNode n);
        T Visit(RawQuery n);
    }

    
    public class MongoQueryBuilder : IQueryNodeVisitor<IMongoQuery>
    {
        public IMongoQuery Visit(AndNode n)
        {
            return Query.And(n.Left.Accept(this), n.Right.Accept(this));
        }

        public IMongoQuery Visit(OrNode n)
        {
            return Query.Or(n.Left.Accept(this), n.Right.Accept(this));
        }

        public IMongoQuery Visit(FieldPredicate n)
        {
            switch (n.Operator)
            {
                case FieldPredicate.OperatorType.EQ:
                    return Query.EQ(n.Fld.Name, BsonValue.Create(n.Value));
                case FieldPredicate.OperatorType.GT:
                    return Query.GT(n.Fld.Name, BsonValue.Create(n.Value));
                case FieldPredicate.OperatorType.GTE:
                    return Query.GTE(n.Fld.Name, BsonValue.Create(n.Value));
                case FieldPredicate.OperatorType.IN:
                    return Query.In(n.Fld.Name, BsonArray.Create(n.Value));
                case FieldPredicate.OperatorType.LK:
                    return Query.Matches(n.Fld.Name, BsonRegularExpression.Create(n.Value));
                case FieldPredicate.OperatorType.LT:
                    return Query.LT(n.Fld.Name, BsonValue.Create(n.Value));
                case FieldPredicate.OperatorType.LTE:
                    return Query.LTE(n.Fld.Name, BsonValue.Create(n.Value));
                case FieldPredicate.OperatorType.NE:
                    return Query.NE(n.Fld.Name, BsonValue.Create(n.Value));
                case FieldPredicate.OperatorType.NOTNULL:
                    return Query.Exists(n.Fld.Name, true);
                case FieldPredicate.OperatorType.NULL:
                    return Query.Exists(n.Fld.Name, false);
                case FieldPredicate.OperatorType.RANGE:
                    return Query.And(Query.LTE(n.Fld.Name, BsonValue.Create(n.Value2)), Query.GT(n.Fld.Name, BsonValue.Create(n.Value)));
                default:
                    throw new NotImplementedException();
            }
        }

        public IMongoQuery Visit(NotNode n)
        {
            return Query.Nor(n.Inner.Accept(this));
        }

        public IMongoQuery Visit(PlusNode n)
        {
            throw new NotImplementedException();
        }

        public IMongoQuery Visit(RawQuery n)
        {
            return Query.Where(BsonJavaScript.Create(n.QueryText));
        }

        public IMongoQuery VisitQ(QueryNode qn)
        {
            var mi = this.GetType().GetMethod("Visit", new Type[] { qn.GetType() });
            if (mi == null) throw new Exception("No Visit for " + qn.GetType().Name);
            return (IMongoQuery) mi.Invoke(this, new object[] { qn });
        }

        public static IMongoQuery BuildMongoQuery(QueryNode qn)
        {
            return new MongoQueryBuilder().VisitQ(qn);
        }

        public static IMongoQuery DynQuery(Func<dynamic, QueryNode> predicate)
        {
            var p = predicate(new DynamicField());
            return BuildMongoQuery(p);
        }
    }

    /// <summary>
    /// Builds MongoDB queries using LINQ-like syntax
    /// </summary>
    public class QueryBuilder
    {
        public static QueryNode DynQuery(Func<dynamic, QueryNode> predicate)
        {
            var dq = new DynamicField();
            return predicate(dq);
        }

        public static string EscapeQueryText(string txt)
        {
            return txt;
        }

        public static string FormatQueryDate(DateTime dt)
        {
            return dt.ToString("yyyy-MM-ddThh:mm:ssZ") + "/DAY";
        }

        public static QueryNode Raw(string query)
        {
            return new RawQuery(query);
        }

        public static string BuildParametrizedQuery(string query, params object[] parameters)
        {
            throw new NotImplementedException();
        }

        public static string BuildParametrizedQuery(string query, IDictionary<string, object> parameters)
        {
            throw new NotImplementedException();
        }

        static void Test()
        {

         

        }
    }
}
