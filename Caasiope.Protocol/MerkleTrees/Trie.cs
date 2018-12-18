using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
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
                return new CompactNode(Depth, Children[0], compact.Path);
            }

            public bool IsLeaf()
            {
                return Children == null;
            }

            public bool IsCompact()
            {
                return Children.Length == 1;
            }

            public void SetChild(byte index, Node child)
            {
                Debug.Assert(Hash == null);
                Children[index] = child;
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

            public CompactNode(byte depth, Node child, byte[] path) : base(depth, new[]{child})
            {
                Debug.Assert(child.Depth > depth);
                Path = path;
            }

            public Node Child => Children[0];
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

        private Node GetCompactOrNode(byte depth, Node child, byte[] path)
        {
            // used for leaf
            if (child.Depth == depth)
            {
                return child;
            }
            return new CompactNode(depth, child, path);
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
            Node child;

            if (IsCompact(parent))
            {
                var compact = (CompactNode) parent;
                child = compact.Child;
                var begin = parent.Depth;
                var end = child.Depth;

                // not on our path
                if (FindDivergence(compact.Path, key, begin, end) != 0)
                {
                    item = default(T);
                    return false;
                }
            }
            else
            {
                var index = parent.GetIndex(key);
                child = parent.Children[index];

                // dead end
                if (child == null)
                {
                    item = default(T);
                    return false;
                }
            }

            if (child.IsLeaf())
            {
                item = ((Leaf)child).Item;
                return true;
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

        private Node SetChild(Node parent, byte index, Node child)
        {
            // the node is immutable
            if (IsFinalized(parent))
                parent = parent.Clone();

            // we dont check, those operations are not costly
            parent.SetChild(index, child);
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
                        GetEnumerable(child, list);
                    }
                }
            }
        }

        // the callback will give the old item and get the new item
        public void CreateOrUpdate(byte[] key, Func<T,T> get)
        {
            CheckFinalized();
            CheckKey(key);
            CreateOrUpdate(root, key, get);
        }

        private Node CreateOrUpdate(Node parent, byte[] key, Func<T, T> get)
        {
            byte index;
            Node child;
            if (parent.IsCompact())
            {
                var compact = (CompactNode)parent;
                child = compact.Child;
                index = 0;
                var depth = FindDivergence(key, compact.Path, parent.Depth, child.Depth);

                // we are on same path
                if (depth == 0)
                {
                    // we already have the value
                    if (child.IsLeaf())
                    {
                        child = CreateOrUpdateLeaf(child, get);
                    }
                    else
                    {
                        child = CreateOrUpdate(child, key, get);
                    }
                }
                // we have to branch
                else
                {
                    // create the branching
                    var node = new Node(depth);

                    // add the previous one 
                    node.SetChild(node.GetIndex(compact.Path), GetCompactOrNode((byte)(depth + 1), compact.Child, compact.Path));
                    // add the new one
                    CreateOrUpdate(node, key, get);

                    // handle the case where we need to replace the compact node to a simple node
                    if (parent.Depth == depth)
                    {
                        return node;
                    }
                    else
                    {
                        // TODO shall we reuse the compact node ?
                        return new CompactNode(compact.Depth, node, compact.Path);
                    }
                }
            }
            else
            {
                var next = (byte) (parent.Depth + 1);
                index = parent.GetIndex(key);
                child = parent.Children[index];

                // no child
                if (child == null)
                {
                    var leaf = CreateOrUpdateLeaf(null, get);
                    if (IsLeafDepth(next))
                        child = leaf;
                    else
                        child = new CompactNode(next, leaf, key);
                }
                else
                {
                    // recursively call on children
                    child = CreateOrUpdate(child, key, get);
                }
            }

            return SetChild(parent, index, child);
        }

        private Node CreateOrUpdateLeaf(Node node, Func<T, T> get)
        {
            T old;
            if (node == null)
            {
                old = default(T);
                Count++;
            }
            else
            {
                old = ((Leaf)node).Item;
            }

            var item = get(old);
            // add the item
            return new Leaf(max_depth, item);
        }

        public bool Verify()
        {
            return Verify(root);
        }

        private bool Verify(Node node)
        {
            if (node.IsLeaf())
            {
                var leaf = (Leaf) node;
                return leaf.Item != null;
            }

            if (node.IsCompact())
            {
                var compact = (CompactNode) node;
                if (compact.Child == null)
                    return false;
                return Verify(compact.Child);
            }

            var children = node.Children.Where(child => child != null).ToList();
            if (children.Count < 2 && node.Depth != 0)
                return false;
            foreach (var child in children)
            {
                if (child.Depth != node.Depth + 1)
                    return false;
                if (!Verify(child))
                    return false;
            }

            return true;
        }
    }

    public class TrieFinalizedException : Exception
    {
        public TrieFinalizedException(string msg) : base(msg) { }
    }
}
