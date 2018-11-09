using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Caasiope.NBitcoin.Crypto;

namespace Caasiope.NBitcoin
{
    public class MerkleNode<T> where T : Hash256
    {
        public static MerkleNode<T> GetRoot(IEnumerable<T> leafs)
        {
            var row = leafs.Select(l => new MerkleNode<T>(l)).ToList();
            if (row.Count == 0)
                return new MerkleNode<T>();
            while (row.Count != 1)
            {
                var parentRow = new List<MerkleNode<T>>();
                for (int i = 0; i < row.Count; i += 2)
                {
                    var left = row[i];
                    var right = i + 1 < row.Count ? row[i + 1] : null;
                    var parent = new MerkleNode<T>(left, right);
                    parentRow.Add(parent);
                }
                row = parentRow;
            }
            return row[0];
        }
        public static MerkleNode<T> GetRoot(int leafCount)
        {
            if (leafCount > 1024 * 1024)
                throw new ArgumentOutOfRangeException("leafCount", "To prevent DDOS attacks, NBitcoin does not support more than 1024*1024 transactions for the creation of a MerkleNode, if this case is legitimate, contact us.");
            return GetRoot(Enumerable.Range(0, leafCount).Select(i => null as T));
        }
        
        private MerkleNode()
        {
            _Hash = Hash256.Zero;
            IsLeaf = true;
        }

        public MerkleNode(T hash)
        {
            _Hash = hash;
            IsLeaf = true;
        }

        public MerkleNode(MerkleNode<T> left, MerkleNode<T> right)
        {
            Left = left;
            Right = right;
            if (left != null)
                left.Parent = this;
            if (right != null)
                right.Parent = this;
            UpdateHash();
        }

        public Hash256 Hash
        {
            get
            {
                return _Hash;
            }
            set
            {
                _Hash = value;
            }
        }

        public void UpdateHash()
        {
            var right = Right ?? Left;
            if (Left != null && Left.Hash != null && right.Hash != null)
                _Hash = Hashes.Hash256(Left.Hash.ToBytes().Concat(right.Hash.ToBytes()).ToArray());
        }

        public bool IsLeaf
        {
            get;
            private set;
        }
        Hash256 _Hash;
        public MerkleNode<T> Parent
        {
            get;
            private set;
        }
        public MerkleNode<T> Left
        {
            get;
            private set;
        }
        public MerkleNode<T> Right
        {
            get;
            private set;
        }

        public IEnumerable<MerkleNode<T>> EnumerateDescendants()
        {
            IEnumerable<MerkleNode<T>> result = new MerkleNode<T>[] { this };
            if (Right != null)
                result = Right.EnumerateDescendants().Concat(result);
            if (Left != null)
                result = Left.EnumerateDescendants().Concat(result);
            return result;
        }

        public MerkleNode<T> GetLeaf(int i)
        {
            return GetLeafs().Skip(i).FirstOrDefault();
        }
        public IEnumerable<MerkleNode<T>> GetLeafs()
        {
            return EnumerateDescendants().Where(l => l.IsLeaf);
        }


        internal bool IsMarked
        {
            get;
            set;
        }

        public IEnumerable<MerkleNode<T>> Ancestors()
        {
            var n = Parent;
            while (n != null)
            {
                yield return n;
                n = n.Parent;
            }
        }

        public override string ToString()
        {
            return Hash == null ? "???" : Hash.ToString();
        }

        public string ToString(bool hierachy)
        {
            if (!hierachy)
                return ToString();
            StringBuilder builder = new StringBuilder();
            ToString(builder, 0);
            return builder.ToString();
        }

        private void ToString(StringBuilder builder, int indent)
        {
            var tabs = new String(Enumerable.Range(0, indent).Select(_ => '\t').ToArray());
            builder.Append(tabs);
            builder.AppendLine(ToString());
            if (Left != null)
                Left.ToString(builder, indent + 1);
            if (Right != null)
                Right.ToString(builder, indent + 1);
        }
    }
}