using System.Collections.Generic;
using System.Diagnostics;

namespace Helios.Common.Concepts.Storages
{
	// usefull to sort linkedlist from best to worst
	public static class SortedLinkedListEngine
	{
		public static LinkedListNode<T> Insert<T, TInsert>(this LinkedList<T> list, T item, IStrictComparer<TInsert> comparer) where T : TInsert
		{
			var current = list.First;

			// we look for the first order that has less value
			while (current != null && comparer.IsBetter(item, current.Value))
				current = current.Next;
			// happends in case it is empty or order should be last of the list
			return current == null ? list.AddLast(item) : list.AddBefore(current, item);
		}

		public static LinkedListNode<T> Update<T, TUpdate>(this LinkedList<T> list, LinkedListNode<T> node, IStrictComparer<TUpdate> comparer) where T : TUpdate
		{
#if DEBUG
			PrivateUpdate(list, node, comparer);

			LinkedListNode<T> item = list.First;
			while (item != null)
			{
				var previous = item.Previous;
				if (previous != null) Debug.Assert(!comparer.IsBetter(previous.Value, item.Value));
				item = item.Next;
			}

			return node;
		}
		
		public static LinkedListNode<T> PrivateUpdate<T, TUpdate>(this LinkedList<T> list, LinkedListNode<T> node, IStrictComparer<TUpdate> comparer) where T : TUpdate
		{
#endif
			Debug.Assert(node.List == list);
			var item = node.Value;
			LinkedListNode<T> next;
			LinkedListNode<T> previous;
			var isMoving = false;

			// if lower then go down
			previous = node;
			next = node.Next;
			while (next != null && comparer.IsBetter(item, next.Value))
			{
				isMoving = true;
				previous = next;
				if (next.Next == null) break;
				next = next.Next;
			}

			if (isMoving)
			{
				list.Remove(node);
				list.AddAfter(previous, node);
				return node;
			}

			// if higher then go up
			next = node.Previous;
			while (next != null && !comparer.IsBetter(item, next.Value))
			{
				isMoving = true;
				previous = next;
				if (next.Previous == null) break;
				next = next.Previous;
			}

			if (isMoving)
			{
				list.Remove(node);
				list.AddBefore(previous, node);
				return node;
			}

			return node;
		}

		public static LinkedListNode<T> Match<T,TMatch>(this LinkedList<T> list, TMatch item, IMatcher<TMatch> comparer) where T : TMatch
		{
			var current = list.First;

			// we look if the first order that has less value
			if (current != null && comparer.IsMatching(item, current.Value)) return current;
			return null;
		}
	}

	public interface IStrictComparer<in T>
	{
		// returns true is a is better
		bool IsBetter(T a, T b);
	}

	public interface IMatcher<in T>
	{
		// returns true is a is better
		bool IsMatching(T price, T maker);
	}
}