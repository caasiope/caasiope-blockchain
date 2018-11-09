using System.Collections.Generic;
using System.Diagnostics;

namespace Helios.Common.Concepts.Storages
{
	public abstract class Storage<TStorage, T> where TStorage : Storage<TStorage, T>
	{
		public interface IStorageItem : IStorageItem<TStorage, T> { }

		protected interface IReference
		{
			LinkedList<T> List { get; }
		}

		public class Reference : IReference
		{
			private readonly LinkedList<T> orders = new LinkedList<T>();
			LinkedList<T> IReference.List { get { return orders; } }

			public IEnumerable<T> GetEnumerable()
			{
				return ((IReference)this).List;
			}

			public void Clear() { orders.Clear(); }
		}

		public void Remove(IStorageItem item)
		{
			var node = item.Node;
			if (node == null) return;
			Debug.Assert(node.List != null);
			node.List.Remove(node);
			item.Node = null;
		}

		public bool TryRemove(IStorageItem item)
		{
			var node = item.Node;
			if (node == null) return false;
			Debug.Assert(node.List != null);
			node.List.Remove(node);
			item.Node = null;
			return true;
		}

		public abstract void Clear();
	}

	public interface IStorageItem<TStorage, TLive> where TStorage : Storage<TStorage, TLive>
	{
		LinkedListNode<TLive> Node { get; set; }
	}
}
