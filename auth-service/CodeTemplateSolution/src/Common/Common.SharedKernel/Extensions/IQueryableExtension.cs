using System.Linq.Expressions;
using System.Reflection;
using Common.Domain.Filters;
using Microsoft.EntityFrameworkCore;

namespace Common.SharedKernel.Extensions
{
    public static class IQueryableExtension
    {
        public static IQueryable<T> Filter<T>(this IQueryable<T> query, FilterModel model) where T : class
        {
            var filterItems = model.Filters;
            var properties = typeof(T).GetProperties();

            foreach (var property in properties)
            {
                filterItems.TryGetValue(property.Name, out List<FilterItem> values);
                if (values == null || values.TrueForAll(a => !a.Checked)) continue;

                var checkedItems = values.Where(a => a.Checked).Select(a => a.Name).ToList();
                if (checkedItems.Contains(Constants.EmptyStringFilterValue))
                {
                    checkedItems.Remove(Constants.EmptyStringFilterValue);
                    checkedItems.Add(string.Empty);
                }
                var containsMethod = checkedItems.GetType().GetMethods().First(a => string.Equals(a.Name, nameof(List<string>.Contains), StringComparison.Ordinal));

                var parameterExpression = Expression.Parameter(typeof(T), "t");
                var memberExpression = Expression.Property(parameterExpression, property);
                if (memberExpression.Type.IsEnum || memberExpression.Type == typeof(int))
                {
                    var enumCheckedItems = values.Where(a => a.Checked).Select(a => a.EnumValue).ToList();
                    var enumMemberExpression = Expression.Convert(memberExpression, typeof(int));
                    containsMethod = enumCheckedItems.GetType().GetMethods().First(a => string.Equals(a.Name, nameof(List<int>.Contains), StringComparison.Ordinal));
                    if (memberExpression.Type.IsDefined(typeof(FlagsAttribute), false))
                    {
                        // Handle flags enum
                        Expression combinedOrExpression = null;
                        foreach (var enumValue in enumCheckedItems)
                        {
                            var enumConstant = Expression.Constant(enumValue);
                            var andExpression = Expression.And(enumMemberExpression, enumConstant);
                            var equalsExpression = Expression.Equal(andExpression, enumConstant);

                            if (combinedOrExpression == null)
                            {
                                combinedOrExpression = equalsExpression;
                            }
                            else
                            {
                                combinedOrExpression = Expression.OrElse(combinedOrExpression, equalsExpression);
                            }
                        }

                        var enumWhereExpression = Expression.Lambda<Func<T, bool>>(combinedOrExpression!, parameterExpression);
                        query = query.Where(enumWhereExpression);
                    }
                    else
                    {
                        var enumContainsCallExpression = Expression.Call(Expression.Constant(enumCheckedItems), containsMethod!, enumMemberExpression);
                        var enumWhereExpression = Expression.Lambda<Func<T, bool>>(enumContainsCallExpression, parameterExpression);
                        query = query.Where(enumWhereExpression);
                    }
                }
                else
                {
                    var containsCallExpression = Expression.Call(
                        Expression.Constant(checkedItems),
                        containsMethod!,
                        memberExpression
                    );
                    var whereExpression = Expression.Lambda<Func<T, bool>>(containsCallExpression, parameterExpression);
                    query = query.Where(whereExpression);
                }
            }
            query = query.Search(model);
            query = ((IOrderedQueryable<T>)query).Sort(model);

            return query;
        }

        public static IQueryable<T> Search<T>(this IQueryable<T> query, FilterModel model) where T : class
        {
            var searchContent = model.SearchContent;
            if (!string.IsNullOrEmpty(searchContent))
            {
                var normalizedSearchContent = searchContent.RemoveDiacritics().Trim();
                var searchKeys = model.SearchKeys.Select(a => a.ToLower()).ToList();
                var properties = typeof(T).GetProperties();
                var searchKeyProperties = properties.Where(a => searchKeys.Contains(a.Name.ToLower())).ToList();
                if (searchKeyProperties.Any())
                {
                    var containsMethod = typeof(string).GetMethods().First(a => a.Name == nameof(string.Contains) && a.GetParameters().Length == 1);

                    var parameterExpression = BuildParameterExpression<T>();

                    Expression whereExpression = Expression.Constant(false);
                    foreach (var property in searchKeyProperties)
                    {
                        var memberExpression = BuildProperty(property, parameterExpression);
                        Expression memberExpressionToLower = memberExpression;
                        if (memberExpression.Type == typeof(string))
                        {
                            var stringToLowerMethod = typeof(string).GetMethods().First(a => a.Name == nameof(string.ToLower) && a.GetParameters().Length == 0);
                            memberExpressionToLower = Expression.Call(memberExpression, stringToLowerMethod!);
                        }
                        var containsCallExpression = Expression.Call(memberExpressionToLower, containsMethod!, Expression.Constant(normalizedSearchContent, typeof(string)));
                        whereExpression = Expression.Or(whereExpression, containsCallExpression);
                    }
                    var expression = Expression.Lambda<Func<T, bool>>(whereExpression, parameterExpression);
                    query = query.Where(expression);
                }
            }
            return query;
        }

        public static IQueryable<T> Take<T>(this IQueryable<T> query, FilterModel model)
        {
            if (model.PageIndex < 0 || model.PageSize <= 0)
            {
                throw new ArgumentException($"Invalid page offset or page size: {model.PageIndex}, {model.PageSize}");
            }
            var offsetFinal = model.PageIndex <= 0 ? 1 : model.PageIndex;
            var sizeFinal = model.PageSize <= 0 ? 10 : model.PageSize;
            query = query.Skip((offsetFinal - 1) * sizeFinal).Take(sizeFinal);
            return query;
        }

        public static IQueryable<T> Sort<T>(this IOrderedQueryable<T> orderedQuery, FilterModel model) where T : class
        {
            var properties = typeof(T).GetProperties();
            var sortColumns = model.SortKey.ToDictionary(k => k.Key.ToLower(), k => k.Value);
            if (properties.Any(a => sortColumns.ContainsKey(a.Name.ToLower())))
            {
                var firstKey = sortColumns.First();
                ParameterExpression parameterExpression = BuildParameterExpression<T>();
                var propertyInfo = properties.First(a => a.Name.Equals(firstKey.Key, StringComparison.OrdinalIgnoreCase));
                var conditionExpression = BuildOrderExpression<T>(propertyInfo, parameterExpression);
                orderedQuery = firstKey.Value ? orderedQuery.OrderBy(conditionExpression) : orderedQuery.OrderByDescending(conditionExpression);
                foreach (var item in sortColumns.Skip(1))
                {
                    PropertyInfo nextProperty = properties.First(a => item.Key.EqualsIgnoreCase(a.Name));
                    var nextConditionExpression = BuildOrderExpression<T>(nextProperty, parameterExpression);
                    orderedQuery = item.Value ? orderedQuery.ThenBy(nextConditionExpression) : orderedQuery.ThenByDescending(nextConditionExpression);
                }

            }
            return orderedQuery;
        }


        public static async Task<FilterResult<List<T>>> GetPageResult<T>(this IQueryable<T> query, FilterModel model) where T : class
        {
            query = query.Filter(model);
            var totalCount = await query.CountAsync();
            var count = (int)Math.Ceiling(totalCount / (decimal)model.PageSize);
            query = query.Take(model);
            var result = await query.ToListAsync();

            return new FilterResult<List<T>>
            {
                Result = result,
                Filters = model,
                TotalCount = totalCount,
                TotalPageCount = count == 0 ? 1 : count
            };
        }


        private static Expression<Func<T, string>> BuildOrderExpression<T>(PropertyInfo propertyInfo, ParameterExpression parameterExpression) where T : class
        {
            Expression condition;
            Expression instance = BuildProperty(propertyInfo, parameterExpression);
            if (propertyInfo.PropertyType.FullName == typeof(DateTimeOffset).FullName)
            {
                condition = Expression.Call(instance, typeof(DateTimeOffset).GetMethod("ToString", new Type[] { }));
            }
            else if (propertyInfo.PropertyType.FullName == typeof(int).FullName)
            {
                condition = Expression.Call(instance, typeof(int).GetMethod("ToString", new Type[] { }));
            }
            else if (propertyInfo.PropertyType.IsEnum)
            {
                var expression = Expression.Convert(instance, typeof(int));
                condition = Expression.Call(expression, typeof(int).GetMethod("ToString", new Type[] { }));
            }
            else
            {
                condition = instance;
            }

            return Expression.Lambda<Func<T, string>>(condition, parameterExpression);
        }

        private static MemberExpression BuildProperty(PropertyInfo propertyInfo, ParameterExpression parameterExpression)
        {
            return Expression.Property(parameterExpression, propertyInfo);
        }

        private static ParameterExpression BuildParameterExpression<T>() where T : class
        {
            return Expression.Parameter(typeof(T), "a");
        }

    }
}
