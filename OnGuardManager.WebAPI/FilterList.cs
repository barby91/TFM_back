using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Linq;
using onGuardManager.Models;
using System.Collections;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Metadata;

namespace onGuardManager.WebAPI
{
	public static class FilterList<TEntity>
	{
		public static IQueryable<TEntity> FilterQuery(IQueryable<TEntity> query, string properties, 
													string subMethod, List<ExpressionType> subMethodOperations, string submehtodValues,
													string subProp, List<ExpressionType> subPropOperations, string subPropValues)
		{
			IQueryable<TEntity> result;

			//primero creamos el parámetro principal
			ParameterExpression param = Expression.Parameter(typeof(TEntity), "a");

			//a continuación se crea el cuerpo de la expresión
			Expression body = (Expression)param;

			List<LambdaExpression> lambdas = new List<LambdaExpression>();
			MethodCallExpression queryExpression;
			List<string> subMethodSplit = subMethod.Split(",").ToList();
			List<string> subPropSplit = subProp.Split(",").ToList();
			List<string> submehtodValuesSplit = submehtodValues.Split(",").ToList();
			List<string> subPropValuesSplit = subPropValues.Split(",").ToList();
			List<string> propertiesSplit = properties.Split('&', '|').ToList();
			List<string> unionLambda = new List<string>();
			int i = 0;

			//El string properties contiene una lista de propiedades separadas por comas, la recorremos
			foreach (string property in propertiesSplit)
			{
				properties = properties.Replace(property, "");
				string unionChar = properties.FirstOrDefault().ToString();
				if (unionChar != null && unionChar != "\0")
				{
					unionLambda.Add(unionChar);
					properties = properties.Replace(unionChar, "");
				}
				PropertyInfo? prop = null;
				if (!string.IsNullOrEmpty(subMethodSplit[i]))
				{
					//se crea una subquery
					MemberExpression collection = Expression.Property(param, propertiesSplit[i]);
					var itemType = collection.Type.GetInterfaces()
											.Single(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>))
											.GetGenericArguments()[0];
					ParameterExpression paramUser = Expression.Parameter(itemType, "u");
					var userProperty = Expression.PropertyOrField(paramUser, subPropSplit[i]);
					LambdaExpression subLambdaExpresion = Expression.Lambda(Expression.MakeBinary(subPropOperations[i], userProperty, Expression.Constant(
														string.IsNullOrEmpty(subPropValuesSplit[i]) || subPropValuesSplit[i].Equals("null", StringComparison.OrdinalIgnoreCase) ? null :
														Convert.ChangeType(subPropValuesSplit[i], userProperty.Type))),
														paramUser);

					//Esto solo vale para el where no para el count
					body = Expression.Call(typeof(Enumerable), subMethodSplit[i], new[] { itemType }, collection, subLambdaExpresion);

					if (subMethodSplit[i].Equals("Count"))
					{
						object value = ChangeType("int", submehtodValuesSplit[i]);
						lambdas.Add(Expression.Lambda(Expression.MakeBinary(subMethodOperations[i],
																							body,
																							Expression.Constant(value)),
																		 param));
					}
					else
					{
						lambdas.Add(Expression.Lambda(body, param));
					}
				}
				else
				{
					string propName = propertiesSplit[i];
					if (!string.IsNullOrEmpty(subPropSplit[i]))
					{
						propName += "." + subPropSplit[i];
					}
					foreach (string propertyName in propName.Split("."))
					{
						prop = typeof(TEntity).GetProperty(propertyName);
						if (prop != null)
						{
							body = Expression.PropertyOrField(body, propertyName);
						}
					}

					if (prop != null)
					{
						object value = ChangeType(prop.PropertyType.Name, subPropValuesSplit[i]);
						ConstantExpression constant = Expression.Constant(Convert.ChangeType(value, prop.PropertyType));
						body = Expression.Equal(body, constant);
					}

					lambdas.Add(Expression.Lambda(body, param));
				}
				i++;
			}

			int l = 0;
			if (unionLambda.Count == 0)
			{
				queryExpression = Expression.Call(typeof(Queryable), "Where", new[] { typeof(TEntity) }, query.Expression, lambdas[0]);
				result = query.Provider.CreateQuery<TEntity>(queryExpression);
			}
			else
			{
				BinaryExpression finalExpression = null;
				foreach (string union in unionLambda)
				{
					switch (union)
					{
						case "&":
							if (l == 0)
							{
								finalExpression = Expression.AndAlso(lambdas[l].Body, lambdas[l + 1].Body);
							}
							else
							{
								finalExpression = Expression.AndAlso(Expression.Lambda(finalExpression).Body, lambdas[l + 1].Body);
							}
							break;
						default:
							if (l == 0)
							{
								finalExpression = Expression.OrElse(lambdas[l].Body, lambdas[l + 1].Body);
							}
							else
							{
								finalExpression = Expression.OrElse(Expression.Lambda(finalExpression).Body, lambdas[l + 1]);
							}
							break;
					}
					l++;
				}

				Expression<Func<TEntity, bool>> exp = Expression.Lambda<Func<TEntity, bool>>(finalExpression, param);
				result = query.Where(exp.Compile()).AsQueryable();

				//if (method.Equals("Count"))
				//{
				//	body = Expression.Call(typeof(Queryable), method, new[] { typeof(TEntity) }, query.Expression, lambda);
				//	object value = ChangeType("int", values);
				//	BinaryExpression binaryExpression;
				//	switch (operation)
				//	{
				//		case ExpressionType.Equal:
				//			binaryExpression = Expression.Equal(body, Expression.Constant(value));
				//			break;
				//		case ExpressionType.LessThan:
				//			binaryExpression = Expression.LessThan(body, Expression.Constant(value));
				//			break;
				//		case ExpressionType.GreaterThan:
				//			binaryExpression = Expression.GreaterThan(body, Expression.Constant(value));
				//			break;
				//		case ExpressionType.LessThanOrEqual:
				//			binaryExpression = Expression.LessThanOrEqual(body, Expression.Constant(value));
				//			break;
				//		default:
				//			binaryExpression = Expression.GreaterThanOrEqual(body, Expression.Constant(value));
				//			break;
				//	}

				//	Expression<Func<TEntity, bool>> exp = Expression.Lambda<Func<TEntity, bool>>(binaryExpression, param);
				//	result = query.Where(exp.Compile()).AsQueryable();
				//}
				//else
				//{

				//queryExpression = Expression.Call(typeof(Queryable), "Where", new[] { typeof(TEntity) }, query.Expression, Expression.Lambda(finalExpression));
			}

			//result = query.Provider.CreateQuery<TEntity>(queryExpression);
			
			return result;
		}


		static MethodCallExpression MakeCountPredicate<TSource>(string collectionName, string itemName, ExpressionType itemComparison, string itemValue, ExpressionType countComparison, int countValue)
		{
			var source = Expression.Parameter(typeof(TSource), "a");
			var collection = Expression.Property(source, collectionName);
			var itemType = collection.Type.GetInterfaces()
				.Single(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>))
				.GetGenericArguments()[0];
			var item = Expression.Parameter(itemType, "u");
			var itemProperty = Expression.Property(item, itemName);
			var itemPredicate = Expression.Lambda(
				Expression.MakeBinary(itemComparison, itemProperty, Expression.Constant(
					string.IsNullOrEmpty(itemValue) || itemValue.Equals("null", StringComparison.OrdinalIgnoreCase) ? null :
					Convert.ChangeType(itemValue, itemProperty.Type))),
				item);
			var itemCount = Expression.Call(
				typeof(Enumerable), "Count", new[] { itemType },
				collection, itemPredicate);
			/*var predicate = Expression.Lambda<Func<TSource, bool>>(
				Expression.MakeBinary(countComparison, itemCount, Expression.Constant(countValue)),
				source);*/
			return itemCount;
		}

		private static object ChangeType(string typeName, string value)
		{
			object newValue;
			switch (typeName)
			{
				case "DateOnly":
					newValue = DateOnly.Parse(value);
					break;
				case "int":
					newValue = int.Parse(value);
					break;
				case "decimal":
					newValue = decimal.Parse(value);
					break;
				case "float":
					newValue = float.Parse(value);
					break;
				default:
					newValue = value;
					break;
			}

			return newValue;
		}
	}
}
