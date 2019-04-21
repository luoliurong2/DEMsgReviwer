using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace DEKafkaMessageViewer.Common
{
	public class ObservableCollectionEx<T> : ObservableCollection<T>
	{
		public ObservableCollectionEx()
		{
		}

		public ObservableCollectionEx(IEnumerable<T> collection)
			: base(collection)
		{
		}

		public ObservableCollectionEx(List<T> collection)
			: base(collection)
		{
		}

		public new void Clear()
		{
			while (Count > 0)
			{
				RemoveAt(0);
			}
		}
	}

	public class SynchronizedObservableCollection<T> : ObservableCollectionEx<T>
	{
		private object syncLockObj = new object();

		public new void Add(T item)
		{
			lock (syncLockObj)
			{
				base.Add(item);
			}
		}

		public new bool Contains(T item)
		{
			lock (syncLockObj)
			{
				return base.Contains(item);
			}
		}

		public new void Remove(T item)
		{
			lock (syncLockObj)
			{
				base.Remove(item);
			}
		}

		public new void RemoveItem(int index)
		{
			lock (syncLockObj)
			{
				base.RemoveItem(index);
			}
		}

		public new void RemoveAt(int index)
		{
			lock (syncLockObj)
			{
				base.RemoveAt(index);
			}
		}

		public new void Clear()
		{
			lock (syncLockObj)
			{
				base.Clear();
			}
		}

		public new void ClearItems()
		{
			lock (syncLockObj)
			{
				base.ClearItems();
			}
		}

		public new int IndexOf(T item)
		{
			lock (syncLockObj)
			{
				return base.IndexOf(item);
			}
		}

		public new void Insert(int index, T item)
		{
			lock (syncLockObj)
			{
				base.Insert(index, item);
			}
		}

		public new void InsertItem(int index, T item)
		{
			lock (syncLockObj)
			{
				base.InsertItem(index, item);
			}
		}

		public new void Move(int oldIndex, int newIndex)
		{
			lock (syncLockObj)
			{
				base.Move(oldIndex, newIndex);
			}
		}

		public new void MoveItem(int oldIndex, int newIndex)
		{
			lock (syncLockObj)
			{
				base.MoveItem(oldIndex, newIndex);
			}
		}

		public new IEnumerator<T> GetEnumerator()
		{
			lock (this)
			{
				SynchronizedCollection<T> collection = new SynchronizedCollection<T>(syncLockObj, this);
				return collection.GetEnumerator();
			}
		}
	}
}
