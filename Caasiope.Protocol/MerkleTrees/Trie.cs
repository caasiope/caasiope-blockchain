using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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
                return GetIndex(key, Depth);
            }

            public static byte GetIndex(byte[] key, byte depth)
            {
                var @byte = key[depth / 2];
                return (byte)(depth % 2 == 0 ? @byte >> 4 : @byte & 0b1111);
            }

            // TODO only we should only have specualized methods
            public Node Clone()
            {
                return Children.Length == 1 ? CloneCompact() : CloneNode();
            }

            private Node CloneNode()
            {
                var clone = new Node(Depth);
                for (int i = 0; i < NB_CHILDREN; i++)
                    clone.Children[i] = Children[i];
                return clone;
            }

            private Node CloneCompact()
            {
                var compact = (CompactNode) this;
                return new CompactNode(Children[0].CloneNode(), compact.Path);
            }

            public bool IsLeaf()
            {
                return Children == null;
            }

            public bool IsCompact()
            {
                return Children.Length == 1;
            }
        }

        public class Leaf : Node
        {
            public readonly T Item;

            public Leaf(byte depth, T item) : base(depth, null)
            {
                Item = item;
            }
        }

        public class CompactNode : Node
        {
            public readonly byte[] Path;

            public CompactNode(Node child, byte[] path) : base(child.Depth, new[]{child})
            {
                Path = path;
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
                child = CreateChild(child, key, item, next);
                // failed
                if (child == null)
                    return null;
            }
            else
            {
                var leaf  = new Leaf(max_depth, item);
                if (IsLeafDepth(next))
                    child = leaf;
                else
                    child = new CompactNode(leaf, key);
                Count++;
            }

            // the node is immutable
            if (IsFinalized(parent))
                parent = parent.Clone();

            // we dont check, those operations are not costly
            parent.Children[index] = child;
            return parent;
        }

        private Node CreateChild(Node child, byte[] key, T item, byte next)
        {
            // we already have this item
            if (IsLeafDepth(next))
                // failed
                return null;

            if (IsCompact(next, child.Depth))
            {
                var compactor = (CompactNode)child;
                var depth = FindDivergence(key, compactor.Path, next, child.Depth);
                // we are on same path
                Node node;
                if (depth > 0)
                {
                    // is it really a compactor ?
                    node = new Node(depth);

                    if (depth == next)
                    {
                        child = node;
                    }
                    else
                    {
                        child = new CompactNode(node, key);
                        // attach old compact to the new node
                    }
                    node.Children[child.GetIndex(compactor.Path)] = GetCompactorOrNode(compactor, depth);
                }
                else
                {
                    node = compactor.Children[0];
                    if (node.IsLeaf())
                    {
                        return null;
                    }
                }
                // attach children to node
                if(Add(node, key, item) == null)
                    return null;
                return child;
            }
            return Add(child, key, item);
        }

        // TODO create generic ?
        private Node GetCompactorOrNode(CompactNode compact, byte depth)
        {
            if (compact.Depth == depth + 1)
                return compact.Children[0];
            return compact;
        }

        // return 0 if no divergence ?
        private byte FindDivergence(byte[] path1, byte[] path2, byte begin, byte end)
        {
            for (byte depth = begin ; depth < end; depth++)
            {
                if (Node.GetIndex(path1, depth) != Node.GetIndex(path2, depth))
                    return depth;
            }

            return 0;
        }

        private bool IsCompact(byte next, byte depth)
        {
            return next != depth;
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

        private bool IsLeafDepth(byte next)
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
            if (child.IsLeaf())
            {
                item = ((Leaf)child).Item;
                return true;
            }

            // if compact
            if (IsCompact(child))
            {
                // look if shortcut matches
                var begin = parent.Depth;
                var end = child.Depth;
                var compactor = (CompactNode) child;
                child = child.Children[0];
                // not on the path
                if (FindDivergence(compactor.Path, key, begin, end) != 0)
                {
                    item = default(T);
                    return false;
                }

                if (child.IsLeaf())
                {
                    item = ((Leaf) child).Item;
                    return true;
                }

                return TryGetValue(child, key, out item);
            }

            // recursively call on children
            return TryGetValue(child, key, out item);
        }

        private bool IsCompact(Node node)
        {
            return node.Children.Length == 1;
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
            if(Count == 0)
                return Hash256.Zero;
            ComputeHash(root, hasher);
            return root.Hash;
        }

        // TODO use a loop and an array to store the parents instead of recursive
        private void ComputeHash(Node node, IHasher<T> hasher)
        {
            // TODO check if outdated
            // use old hash

            // leaf is return the hash of the item provided by the extractor
            if(node.IsLeaf())
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
                throw new TrieFinalizedException("The trie has already been finalized !");
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
            if (IsLeafDepth(next))
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
            if (IsLeafDepth(node.Depth))
                list.Add(((Leaf)node).Item);
            else
            {
                foreach (var child in node.Children)
                {
                    if (child != null)
                    {
                        var next = IsCompact((byte) (node.Depth + 1), child.Depth) ? child.Children[0] : child;
                        GetEnumerable(next, list);
                    }
                }
            }
        }

        // the callback will give the old item and get the new item
        public void CreateOrUpdate(byte[] key, Func<T,T> get)
        {
            CreateOrUpdate(root, key, get);
        }

        private void CreateOrUpdate(Node parent, byte[] key, Func<T, T> get)
        {
            var next = (byte)(parent.Depth + 1); // pass this as argument ? i would say no, it should not be in the stack
            var index = parent.GetIndex(key);
            var child = parent.Children[index];

            // check if next node is leaf
            if (IsLeafDepth(next))
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
                CreateOrUpdate(child, key, get);
            }

            // the node is immutable
            if (IsFinalized(parent))
                parent = parent.Clone();

            // we dont check, those operations are not costly
            parent.Children[index] = child;
        }
    }

    public class TrieFinalizedException : Exception
    {
        public TrieFinalizedException(string msg) : base(msg) { }
    }
}
