using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace BsTableQueryable
{
    internal static class QueryableExtensions
    {
        internal static IQueryable<T> GlobalSearch<T>(this IQueryable<T> data, string searchText = null)
        {
            if (string.IsNullOrEmpty(searchText) || string.IsNullOrWhiteSpace(searchText)) return data;

            var props = typeof(T).GetProperties();
            Expression<Func<T, bool>> predicate = null;
            foreach (var prop in props)
            {
                var expr = BuildStringContainsPredicate<T>(prop.Name, searchText, true);
                predicate = predicate == null
                    ? PredicateBuilder.Create(expr)
                    : predicate.Or(expr);
            }
            return data.Where(predicate);
        }

        internal static IQueryable<T> SpecificSearch<T>(this IQueryable<T> data, IList<BsTableModel.FilterByField> filters)
        {
            if (filters == null)
                return data;

            var predicate = filters.ConstructAndExpressionTree<T>();
            return predicate == null ? data : data.Where(predicate);
        }

        internal static IQueryable<T> Sort<T>(this IQueryable<T> data, string column = null, BsTableModel.SortOrder dir = BsTableModel.SortOrder.Asc)
        {
            column = typeof(T).GetOrFixOrderColumn(column);
            //return data.OrderBy(column, dir == BsTableModel.SortOrder.Asc ? ListSortDirection.Ascending : ListSortDirection.Descending, false, false);
            return data.OrderByDynamic(column, dir == BsTableModel.SortOrder.Desc);
        }

        private static string GetOrFixOrderColumn(this Type type, string column = null)
        {
            // make sure the column is exist
            column = type.GetProperties().First(x => x.Name.ToLower().Contains(column.ToLower())).Name;
            if (!string.IsNullOrEmpty(column) && !string.IsNullOrWhiteSpace(column)) return column;

            // find first column with "id"
            column = type.GetProperties().First(x => x.Name.ToLower().Contains("id")).Name;
            if (!string.IsNullOrEmpty(column) && !string.IsNullOrWhiteSpace(column)) return column;

            // or just return first column
            return type.GetMembers().FirstOrDefault().Name;
        }

        private static readonly MethodInfo startsWithMethod = typeof(string).GetMethod("StartsWith", new Type[] { typeof(string) });
        private static readonly MethodInfo endsWithMethod = typeof(string).GetMethod("EndsWith", new Type[] { typeof(string) });

        /// <summary>
        /// <see cref="object.ToString()"/> method info.
        /// Used for building search predicates when the searchable property has non-string type.
        /// </summary>
        private static readonly MethodInfo Object_ToString = typeof(object).GetMethod(nameof(object.ToString));

        /// <summary>
        /// <see cref="string.ToLower()"/> method info.
        /// Used for conversion of string values to lower case.
        /// </summary>
        private static readonly MethodInfo String_ToLower = typeof(string).GetMethod(nameof(String.ToLower), new Type[] { });

        /// <summary>
        /// <see cref="string.Contains(string)"/> method info.
        /// Used for building default search predicates.
        /// </summary>
        private static readonly MethodInfo String_Contains = typeof(string).GetMethod(nameof(String.Contains), new[] { typeof(string) });

        /// <summary>
        /// Builds the property expression from the full property name.
        /// </summary>
        /// <param name="param">Parameter expression, like <code>e =></code></param>
        /// <param name="propertyName">Name of the property</param>
        /// <returns>MemberExpression instance</returns>
        private static MemberExpression BuildPropertyExpression(ParameterExpression param, string propertyName)
        {
            string[] parts = propertyName.Split('.');
            Expression body = param;
            foreach (var member in parts)
            {
                body = Expression.Property(body, member);
            }
            return (MemberExpression)body;
        }

        /// <summary>
        /// Creates predicate expression like
        /// <code>(T t) => t.SomeProperty.Contains("Constant")</code>
        /// where "SomeProperty" name is defined by <paramref name="stringConstant"/> parameter, and "Constant" is the <paramref name="stringConstant"/>.
        /// If property has non-string type, it is converted to string with <see cref="object.ToString()"/> method.
        /// </summary>
        /// <typeparam name="T">Data type</typeparam>
        /// <param name="propertyName">Property name</param>
        /// <param name="stringConstant">String constant to construct the <see cref="string.Contains(string)"/> expression.</param>
        /// <param name="caseInsensitive">Case insensitive search</param>
        /// <returns>Predicate instance</returns>
        private static Expression<Func<T, bool>> BuildStringContainsPredicate<T>(string propertyName, string stringConstant, bool caseInsensitive)
        {
            var type = typeof(T);
            var parameterExp = Expression.Parameter(type, "e");
            var propertyExp = BuildPropertyExpression(parameterExp, propertyName);

            Expression exp = propertyExp;

            // if the property value type is not string, it needs to be casted at first
            if (propertyExp.Type != typeof(string))
            {
                exp = Expression.Call(propertyExp, Object_ToString);
            }

            var someValue = Expression.Constant(caseInsensitive ? stringConstant.ToLower() : stringConstant, typeof(string));
            var containsMethodExp = Expression.Call(exp, String_Contains, someValue);
            var notNullExp = Expression.NotEqual(exp, Expression.Constant(null, typeof(object)));

            // call ToLower if case insensitive search
            if (caseInsensitive)
            {
                var toLowerExp = Expression.Call(exp, String_ToLower);
                containsMethodExp = Expression.Call(toLowerExp, String_Contains, someValue);
            }

            return Expression.Lambda<Func<T, bool>>(Expression.AndAlso(notNullExp, containsMethodExp), parameterExp);
        }

        /// <summary>
        /// Orders the <see cref="IQueryable{T}"/> by property with specified name.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="query">Data type</param>
        /// <param name="propertyName">Property name</param>
        /// <param name="direction">Sorting direction</param>
        /// <param name="caseInsensitive">If true, case insensitive ordering will be performed (with forced <see cref="String.ToLower()"/> conversion).</param>
        /// <param name="alreadyOrdered">Flag indicating the <see cref="IQueryable{T}"/> is already ordered.</param>
        /// <returns>Ordered <see cref="IQueryable{T}"/>.</returns>
        private static IQueryable<T> OrderBy<T>(this IQueryable<T> query, string propertyName, ListSortDirection direction, bool caseInsensitive, bool alreadyOrdered)
        {
            string methodName = null;

            if (direction == ListSortDirection.Ascending && !alreadyOrdered)
                methodName = nameof(Queryable.OrderBy);
            else if (direction == ListSortDirection.Descending && !alreadyOrdered)
                methodName = nameof(Queryable.OrderByDescending);
            if (direction == ListSortDirection.Ascending && alreadyOrdered)
                methodName = nameof(Queryable.ThenBy);
            else if (direction == ListSortDirection.Descending && alreadyOrdered)
                methodName = nameof(Queryable.ThenByDescending);

            var type = typeof(T);
            var parameterExp = Expression.Parameter(type, "e");
            var propertyExp = BuildPropertyExpression(parameterExp, propertyName);

            Expression exp = propertyExp;

            // call ToLower if case insensitive search
            if (caseInsensitive && propertyExp.Type == typeof(string))
            {
                exp = Expression.Call(exp, String_ToLower);
            }

            var orderByExp = Expression.Lambda(exp, parameterExp);
            var typeArguments = new Type[] { type, propertyExp.Type };

            var resultExpr = Expression.Call(typeof(System.Linq.Queryable), methodName, typeArguments, query.Expression, Expression.Quote(orderByExp));

            return query.Provider.CreateQuery<T>(resultExpr);
        }

        private static IQueryable<T> OrderByDynamic<T>(this IQueryable<T> query, string sortColumn, bool descending)
        {
            // Dynamically creates a call like this: query.OrderBy(p => p.SortColumn)
            var parameter = Expression.Parameter(typeof(T), "p");

            string command = "OrderBy";

            if (descending)
            {
                command = "OrderByDescending";
            }

            var property = typeof(T).GetProperty(sortColumn);
            // this is the part p.SortColumn
            var propertyAccess = Expression.MakeMemberAccess(parameter, property);

            // this is the part p => p.SortColumn
            var orderByExpression = Expression.Lambda(propertyAccess, parameter);

            // finally, call the "OrderBy" / "OrderByDescending" method with the order by lamba expression
            Expression resultExpression = Expression.Call(typeof(Queryable), command, new Type[] { typeof(T), property.PropertyType },
               query.Expression, Expression.Quote(orderByExpression));
            return query.Provider.CreateQuery<T>(resultExpression);
        }

        private static MethodCallExpression StringToLowerExpression(this MemberExpression member)
        {
            return Expression.Call(member, String_ToLower);
        }

        public static Expression GetExpression<T>(ParameterExpression param, BsTableModel.FilterByField filter)
        {
            var member = Expression.Property(param, filter.Name);
            var constant = ConstantCreatorBasedOnType(member, filter.Value, filter.CaseSensitive);
            var isString = member.Type == typeof(string);

            return filter.Comparator switch
            {
                BsTableModel.Comparator.GreaterThan => Expression.GreaterThan(member, constant),
                BsTableModel.Comparator.GreaterThanOrEqual => Expression.GreaterThanOrEqual(member, constant),
                BsTableModel.Comparator.LessThan => Expression.LessThan(member, constant),
                BsTableModel.Comparator.LessThanOrEqual => Expression.LessThanOrEqual(member, constant),
                BsTableModel.Comparator.Equal =>
                    (isString && !filter.CaseSensitive)
                    ? Expression.Equal(member.StringToLowerExpression(), constant)
                    : Expression.Equal(member, constant),
                BsTableModel.Comparator.NotEqual =>
                    (isString && !filter.CaseSensitive)
                    ? Expression.NotEqual(member.StringToLowerExpression(), constant)
                    : Expression.NotEqual(member, constant),
                BsTableModel.Comparator.Contains =>
                    (isString && !filter.CaseSensitive)
                    ? Expression.Call(member.StringToLowerExpression(), String_Contains, constant)
                    : Expression.Call(member, String_Contains, constant),
                BsTableModel.Comparator.StartsWith =>
                    (isString && !filter.CaseSensitive)
                    ? Expression.Call(member.StringToLowerExpression(), startsWithMethod, constant)
                    : Expression.Call(member, startsWithMethod, constant),
                BsTableModel.Comparator.EndsWith =>
                    (isString && !filter.CaseSensitive)
                    ? Expression.Call(member.StringToLowerExpression(), endsWithMethod, constant)
                    : Expression.Call(member, endsWithMethod, constant),
                _ => null,
            };
        }

        private static ConstantExpression ConstantCreatorBasedOnType(MemberExpression member, string value, bool caseSensitive = true)
        {
            if (member.Type == typeof(Guid))
            {
                return Expression.Constant(Guid.Parse(value));
            }
            else
            {
                return Expression.Constant(!caseSensitive ? value.ToLower() : value);
            }
        }

        private static Expression<Func<T, bool>> ConstructAndExpressionTree<T>(this IList<BsTableModel.FilterByField> filters)
        {
            if (filters.Count == 0)
                return null;

            var param = Expression.Parameter(typeof(T), "t");
            Expression exp;
            if (filters.Count == 1)
            {
                exp = GetExpression<T>(param, filters[0]);
            }
            else
            {
                exp = GetExpression<T>(param, filters[0]);
                for (var i = 1; i < filters.Count; i++)
                {
                    exp = Expression.And(exp, GetExpression<T>(param, filters[i]));
                }
            }

            return Expression.Lambda<Func<T, bool>>(exp, param);
        }

        public static IQueryable<T> Paging<T>(this IQueryable<T> source, int pageNumber = 1, int pageSize = 10)
        {
            return source
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize);
        }
    }
}