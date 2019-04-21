using System;
using System.ComponentModel;
using System.Linq.Expressions;

namespace DEKafkaMessageViewer.Extensions
{
	public static class NotificationHelper
	{
		public static void Notify<T>(this PropertyChangedEventHandler handler, object owner, Expression<Func<T>> memberExpression)
		{
			if (owner == null)
			{
				throw new ArgumentNullException("Owner");
			}
			if (handler != null)
			{
				//send the property change event
				var propertyName = GetPropertyName(memberExpression);
				if (!string.IsNullOrEmpty(propertyName))
				{
					handler(owner, new PropertyChangedEventArgs(propertyName));
				}
			}
		}

		public const string MemberExpressionParameterName = "memberExpression";

		public static string GetPropertyName<T>(Expression<Func<T>> memberExpression)
		{
			string propertyName = string.Empty;
			//Get the name of the property
			if (memberExpression == null)
			{
				throw new ArgumentNullException(MemberExpressionParameterName);
			}
			var experssionBody = memberExpression.Body as MemberExpression;
			if (experssionBody == null)
			{
				throw new ArgumentException("Lambda must return type");
			}
			propertyName = experssionBody.Member.Name;
			return propertyName;
		}
	}
}
