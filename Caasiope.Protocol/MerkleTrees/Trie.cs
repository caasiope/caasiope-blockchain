using System;
using System.Collections.Generic;
using System.Diagnostics;
using Caasiope.NBitcoin;
using Caasiope.NBitcoin.Crypto;

namespace Caasiope.Protocol.MerkleTrees
{
    // Inspirations :
    // https://github.com/aeternity/epoch/wiki/Merkle-tree-implementation-alternatives
    // https://github.com/ethereum/wiki/wiki/Patricia-Tree

    // Expected behaviour :
    // - tree hash not dependent on insertion order
    // - account hash can change but position in the tree stays the same
    // - when the tree is finalized, you cant update anymore
    // - each node can either be mutable or finalized
    // - when we update a value, if the node is finalized, we need to replace it by a mutable node
 
    // Specifications :
    // (compact) radix trie structure
    // key is a byte array with fixed length
    // each nodes have 16 children

    // TODO
    // use compact trie
    // use loop instead of recursion
    // optimize get enumerable or use foreach callback

    public class Trie<T>
    {
        const int NB_CHILDREN = 16;

        public class Node
        {
            public readonly byte Depth;
            public readonly Node[] Children;
            public Hash256 Hash;
            
            public Node(byte depth) : this(depth, new Node[NB_CHILDREN]) { }

            protected Node(byte depth, Node[] children)
            {
                Depth = depth;
                Children = children;
            }

            // inline
            // OPTIMIZE is it better to return an int ?
            public byte GetIndex(byte[] key)
            {
                var @byte = key[Depth/2];
                return (byte) (Depth % 2 == 0 ? @byte >> 4 : @byte & 0b1111);
            }

            public Node Clone()
            {
                var clone = new Node(Depth);
                for (int i = 0; i < NB_CHILDREN; i++)
                    clone.Children[i] = Children[i];
                return clone;
            }
        }

        public class Leaf : Node
        {
            public readonly T Item;

            public Leaf(byte depth, T item) : base(depth)
            {
                Item = item;
            }
        }

        private readonly Node root;
        public int Count { get; private set; }

        private readonly byte max_depth;

        // used for cloning
        private Trie(int length, Node root, int count)
        {
            if (length < 1)
                throw new ArgumentException("The minimum lenght of the key is 1");
            if (length > byte.MaxValue / 2)
                throw new ArgumentException($"The maximum lenght of the key is {byte.MaxValue / 2}");
            Count = count;
            max_depth = (byte)(length * 2);
            this.root = root;
        }

        // used to create an empty trie
        public Trie(int length) : this(length, new Node(0), 0) { }

        // return false if it has already been added
        public bool Add(byte[] key, T item)
        {
            CheckFinalized();
            CheckKey(key);
            return Add(root, key, item) != null;
        }

        // TODO use a loop and an array to store the parents instead of recursive
        private Node Add(Node parent, byte[] key, T item)
        {
            var next = (byte) (parent.Depth + 1); // pass this as argument ? i would say no, it should not be in the stack
            var index = parent.GetIndex(key);
            var child = parent.Children[index];

            // check if we have child
            if (child != null)
            {
                // we already have this item
                if (IsLeaf(next))
                    // failed
                    return null;
                child = Add(child, key, item);

                // failed
                if (child == null)
                    return null;
            }
            else
            {
                if (IsLeaf(next))
                {
                    // create child
                    child = new Leaf(next, item);
                    Count++;
                }
                else
                {
                    child = new Node(next);
                    Add(child, key, item); // AddNoReplace
                }
            }

            // the node is immutable
            if (IsFinalized(parent))
                parent = parent.Clone();

            // we dont check, those operations are not costly
            parent.Children[index] = child;
            return parent;
        }

        private void AddNoReplace(Node child, byte[] key, T item)
        {
            throw new NotImplementedException();
        }

        public bool IsFinalized()
        {
            return IsFinalized(root);
        }

        private bool IsFinalized(Node node)
        {
            return node.Hash != null;
        }

        private bool IsLeaf(byte next)
        {
            return max_depth <= next;
        }

        // inline
        private void CheckKey(byte[] key)
        {
            if (key.Length*2 != max_depth)
            {
                throw new ArgumentException($"the key must have fixed size : {max_depth}");
            }
        }

        public bool TryGetValue(byte[] key, out T item)
        {
            CheckKey(key);
            return TryGetValue(root, key, out item);
        }

        // TODO use a loop and an array to store the parents instead of recursive
        private bool TryGetValue(Node parent, byte[] key, out T item)
        {
            var index = parent.GetIndex(key);
            var child = parent.Children[index];

            // check if we can continue
            if (child == null)
            {
                item = default(T);
                return false;
            }

            // check if next node is leaf
            if (IsLeaf(child.Depth))
            {
                item = ((Leaf)child).Item;
                return true;
            }

            // recursively call on children
            return TryGetValue(child, key, out item);
        }

        public Hash256 GetHash()
        {
            if (!IsFinalized())
                throw new Exception("The hash should be computed before calling GetHash() !");
            return root.Hash;
        }

        public Hash256 ComputeHash(IHasher<T> hasher)
        {
            CheckFinalized();
            if (Count == 0)
                return root.Hash = Hash256.Zero;

            ComputeHash(root, hasher);
            return root.Hash;
        }

        // TODO use a loop and an array to store the parents instead of recursive
        private void ComputeHash(Node node, IHasher<T> hasher)
        {
            // TODO check if outdated
            // use old hash

            // leaf is return the hash of the item provided by the extractor
            if (IsLeaf(node.Depth))
            {
                var leaf = (Leaf) node;
                leaf.Hash = hasher.GetHash(leaf.Item);
                return;
            }

            // to optimize, we first ask the children to prepare the hash
            foreach (var child in node.Children)
            {
                if (child != null)
                {
                    // recursive call
                    ComputeHash(child, hasher);
                }
            }

            // we dont want stuff to go to the stack when we have a recursive call
            {
                var count = 0; 
                foreach (var child in node.Children)
                    if (child != null)
                        count++;

                switch (count)
                {
                    case 1:
                        foreach (var child in node.Children)
                            if (child != null)
                            {
                                node.Hash = child.Hash;
                                return;
                            }
                        break;
                    case 0:
                        throw new NotImplementedException("trie has no children, this should not happen because we cant remove !");
                    default:
                        // concatenate the hash the of each child not null
                        var buffer = new byte[Hash256.SIZE * count];
                        count = 0;
                        foreach (var child in node.Children)
                        {
                            if (child != null)
                            {
                                Array.Copy(child.Hash.ToBytes(), 0, buffer, count * Hash256.SIZE, Hash256.SIZE);
                                count++;
                            }
                        }

                        node.Hash = Hashes.Hash256(buffer);
                        break;
                }
            }
        }

        public Trie<T> Clone()
        {
            return new Trie<T>(max_depth/2, root.Clone(), Count);
        }

        private void CheckFinalized()
        {
            if (IsFinalized())
                throw new Exception("The trie has already been finalized !");
        }

        public bool Update(byte[] key, T item)
        {
            CheckFinalized();
            CheckKey(key);
            return Update(root, key, item) != null;
        }

        // TODO use a loop and an array to store the parents instead of recursive
        private Node Update(Node parent, byte[] key, T item)
        {
            var next = (byte)(parent.Depth + 1); // pass this as argument ? i would say no, it should not be in the stack
            var index = parent.GetIndex(key);
            var child = parent.Children[index];

            // does not exists
            if (child == null)
                return null;

            // check if next node is leaf
            if (IsLeaf(next))
            {
                // add the item
                 child =  new Leaf(next, item);
            }
            else
            {
                // recursively call on children
                child = Update(child, key, item);

                // check if null
                if (child == null)
                    return null;
            }

            // the node is immutable
            if (IsFinalized(parent))
                parent = parent.Clone();

            // we dont check, those operations are not costly
            parent.Children[index] = child;
            return parent;
        }

        public IEnumerable<T> GetEnumerable()
        {
            var list = new List<T>(Count);
            GetEnumerable(root, list);
            return list;
        }

        private void GetEnumerable(Node node, List<T> list)
        {
            if (IsLeaf(node.Depth))
                list.Add(((Leaf)node).Item);
            else
            {
                foreach (var child in node.Children)
                {
                    if (child != null)
                        GetEnumerable(child, list);
                }
            }
        }

        // the callback will give the old item and get the new item
        public void CreateOrUpdate(byte[] key, Func<T,T> get)
        {
            var ret = CreateOrUpdate(root, key, get);
            Debug.Assert(ret == root);
        }

        // return the old parent or a new parent if modified
        private Node CreateOrUpdate(Node parent, byte[] key, Func<T, T> get)
        {
            var next = (byte)(parent.Depth + 1); // pass this as argument ? i would say no, it should not be in the stack
            var index = parent.GetIndex(key);
            var child = parent.Children[index];

            // check if next node is leaf
            if (IsLeaf(next))
            {
                T old;
                if (child == null)
                {
                    old = default(T);
                    Count++;
                }
                else
                {
                    old = ((Leaf)child).Item;
                }

                var item = get(old);
                // add the item
                child = new Leaf(next, item);
            }
            else
            {
                if (child == null)
                {
                    child = new Node(next);
                }
                // recursively call on children
                child = CreateOrUpdate(child, key, get);
            }

            // the node is immutable
            if (IsFinalized(parent))
                parent = parent.Clone();

            // we dont check, those operations are not costly
            parent.Children[index] = child;
            return parent;
        }
    }
}
