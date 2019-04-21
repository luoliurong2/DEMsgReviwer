using DEKafkaMessageViewer.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Windows.Threading;

namespace DEKafkaMessageViewer.ViewModels
{
	public abstract class NotificationObject : INotifyPropertyChanged
	{
		public static Dispatcher UIDispatcher;
		public static void UIInvoke(Action act)
		{
			UIDispatcher.Invoke(act);
		}
		public event PropertyChangedEventHandler PropertyChanged;

		public void Notify<T>(Expression<Func<T>> memberExpression)
		{
			PropertyChanged.Notify<T>(this, memberExpression);
		}

		private void RaisePropertyChanged(string propName)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
		}

		protected void ChangeAndNotify<T>(ref T prop, T value, Expression<Func<T>> getProp)
		{
			string propertyName = GetPropertyName(getProp);

			if (EqualityComparer<T>.Default.Equals(prop, value))
			{
				return;
			}

			prop = value;
			RaisePropertyChanged(propertyName);

			return;
		}

		private string GetPropertyName<T>(Expression<Func<T>> memberExpression)
		{
			string propertyName = string.Empty;
			var experssionBody = memberExpression.Body as MemberExpression;
			propertyName = experssionBody.Member.Name;
			return propertyName;
		}
	}
}
