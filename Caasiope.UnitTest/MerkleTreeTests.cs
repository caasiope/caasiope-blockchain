using System;
using System.Collections.Generic;
using System.Linq;
using Caasiope.NBitcoin;
using Caasiope.Protocol.MerkleTrees;
using Caasiope.Protocol.Types;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Caasiope.UnitTest
{
    [TestClass]
    public class MerkleTreeTests
    {
        private static readonly List<Address> addresses1;
        private static readonly List<Address> addresses2;

        private static readonly List<Account> accounts1 = new List<Account>();
        private static readonly List<Account> accounts2 = new List<Account>();

        static MerkleTreeTests()
        {
            addresses1 = new List<Address>
            {
                new Address("qy6c04r0qka3lfarf2cs50gsm6yt0maajtd6td5g"),
                new Address("qy9z4e3vrmvpfpyk7fu2k530khnf43anx4rlzpma"),
                new Address("qy0fdvyuxegzscq94p3su82lamtr3as0d3uclxld"),
                new Address("qyaqhx22qrfr9ndm7f6k0dqayw0wqkr6qglzzg36"),
                new Address("qyrqnl087wcj5rqekdtyyxl5kurzpsr8aesasv7j"),
                new Address("qyfdzamnqslxrsz5yuwhxe7fjxymcfv3nwhj6amq"),
                new Address("qydukj5ul7qweewkx5uh8sjqr5tdyprryts35e80"),
                new Address("qy932c3fzptwgwahfqhl95r9e2kkkh0l2wdhkt2k"),
            };

            addresses2 = new List<Address>
            {
                new Address("qyqpwhaue0mqkruaeepgz50tywnqxwzauuhg794v"),
                new Address("qym2xjl5jf2g67ghrrxuha3264a7r9lf6unvcfyx"),
                new Address("qyzmf7ymmsnsqu2jmr757cta4gdxk5dmtl96m5uw"),
                new Address("qyuee3k4e5fxlv4xm7gfy8j4uz3slzyxkz0426qp"),
                new Address("qymnx6cy976z6zmt786tx7dfm2axlzwhf3tq28c8"),
                new Address("qyfnjfsmc5kquvc9zdd7zdut9fyhchyu3f5n2k9k"),
            };

            foreach (var address in addresses1)
                accounts1.Add(new MutableAccount(address, 0));

            foreach (var address in addresses2)
                accounts2.Add(new MutableAccount(address, 0));
        }

        [TestMethod]
        public void TestTreeAddition()
        {
            var tree = CreateTree();
            var count = addresses1.Count;
            
            // add objects
            FillTree(tree, accounts1);

            Assert.AreEqual(count, tree.Count);
            Assert.AreEqual(count, tree.GetEnumerable().Count());

            // try add same objects
            foreach (var account in accounts1)
            {
                Assert.IsFalse(tree.Add(account.Address.ToRawBytes(), account));
            }

            Assert.AreEqual(count, tree.Count);
            Assert.AreEqual(count, tree.GetEnumerable().Count());

            // try get objects
            foreach (var account in accounts1)
            {
                Assert.IsTrue(tree.TryGetValue(account.Address.ToRawBytes(), out var item));
                Assert.AreSame(account, item);
            }

            var count2 = count + accounts2.Count;
            FillTree(tree, accounts2);

            Assert.AreEqual(count2, tree.Count);
            Assert.AreEqual(count2, tree.GetEnumerable().Count());
        }

        private Trie<Account> CreateTree()
        {
            return new Trie<Account>(Address.RAW_SIZE);
        }

        private void FillTree(Trie<Account> tree, List<Account> accounts)
        {
            foreach (var account in accounts)
            {
                Assert.IsTrue(tree.Add(account.Address.ToRawBytes(), account));
            }
        }

        [TestMethod]
        public void TestTreeUpdate()
        {
            // create first tree
            var tree1 = CreateTree();
            FillTree(tree1, accounts1);

            // compute first hash
            var hash1 = tree1.ComputeHash();

            // update account
            var tree2 = tree1.Clone();
            var address = addresses1[0].ToRawBytes();
            Assert.IsTrue(tree1.TryGetValue(address, out var account));
            tree2.Update(address, new MutableAccount(account.Address, account.CurrentLedger + 1));

            // compute second hash
            var hash2 = tree2.ComputeHash();

            // check first hash is same
            Assert.AreEqual(hash1, tree1.GetHash());
            // check second hash is different
            Assert.AreNotEqual(hash1, hash2);
        }

        [TestMethod]
        public void TestTreeCreateOrUpdate()
        {
            // create first tree
            var tree = CreateTree();

            foreach (var account in accounts1)
            {
                tree.CreateOrUpdate(account.Address.ToRawBytes(), old =>
                {
                    Assert.IsNull(old);
                    return account;
                });
            }

            Assert.AreEqual(accounts1.Count, tree.Count);
            Assert.AreEqual(accounts1.Count, tree.GetEnumerable().Count());

            foreach (var account in accounts1)
            {
                tree.CreateOrUpdate(account.Address.ToRawBytes(), old =>
                {
                    Assert.IsNotNull(old);
                    return account;
                });
            }

            Assert.AreEqual(accounts1.Count, tree.Count);
            Assert.AreEqual(accounts1.Count, tree.GetEnumerable().Count());
        }

        [TestMethod]
        public void TestTreeHash()
        {
            // create a first tree
            var tree1 = CreateTree();
            FillTree(tree1, accounts1);
            var hash1 = tree1.ComputeHash();
            tree1 = tree1.Clone();

            // create a second tree with same objects in different order
            var tree2 = CreateTree();
            var reversed = new List<Account>(accounts1);
            reversed.Reverse();
            FillTree(tree2, reversed);
            var hash2 = tree2.ComputeHash();

            Assert.AreEqual(hash2, hash1);

            // add more objects
            FillTree(tree1, accounts2);
            hash1 = tree1.ComputeHash();
            Assert.AreNotEqual(hash2, hash1);

            // create a third tree with different objects
            var tree3 = CreateTree();
            FillTree(tree3, accounts2);
            var hash3 = tree3.ComputeHash();
            tree3 = tree3.Clone();
            Assert.AreNotEqual(hash3, hash1);
            Assert.AreNotEqual(hash3, hash2);

            // add complementary objects
            FillTree(tree3, accounts1);
            hash3 = tree3.ComputeHash();
            Assert.AreEqual(hash3, hash1);
        }

        [TestMethod]
        public void TestImmutability()
        {
            // create a tree
            var tree = CreateTree();
            FillTree(tree, accounts1);
            // make it immutable
            tree.ComputeHash();

            // try add
            try
            {
                FillTree(tree, accounts2);
                Assert.Fail("Did not throw expected exception !");
            }
            catch (TrieFinalizedException e) {}

            // get value
            var address = addresses1[0].ToRawBytes();
            Assert.IsTrue(tree.TryGetValue(address, out var account));

            // try update
            try
            {
                tree.Update(address, new MutableAccount(account.Address, account.CurrentLedger + 1));
                Assert.Fail("Did not throw expected exception !");
            }
            catch (TrieFinalizedException e) { }
        }

        [TestMethod]
        public void TestMutability()
        {
            // create a first tree
            var tree1 = CreateTree();
            FillTree(tree1, accounts1);
            // make it immutable
            tree1.ComputeHash();

            // create a second tree from the first tree
            var tree2 = tree1.Clone();
            Assert.AreEqual(tree2.Count, accounts1.Count);
            // add elements
            FillTree(tree2, accounts2);

            // check the first tree is still the same
            Assert.AreEqual(accounts1.Count, tree1.GetEnumerable().Count());
            foreach (var account in accounts2)
                Assert.IsFalse(tree1.TryGetValue(account.Address.ToRawBytes(), out var item));

            // check the second tree is different
            Assert.AreEqual(tree2.Count, accounts1.Count + accounts2.Count);



            // check the second and first tree share some common parts
        }

        [TestMethod]
        public void TestCompact()
        {
            var tree = new Trie<int>(4);

            // root - 1 -> (compact) 2345678 -> 0x12345678
            AddHexa(tree, 0x12345678);

            // diverge on last leaf
            // root - 1 -> (compact) 234567 - 8 -> 0x12345678
            //                              - 9 -> 0x12345679
            AddHexa(tree, 0x12345679);

            // diverge on root node
            // root - 0 -> (compact) 2345678 -> 0x02345678
            //      - 1 -> (compact) 234567 - 8 -> 0x12345678
            //                              - 9 -> 0x12345679
            AddHexa(tree, 0x02345678);

            // diverge on random node
            // root - 0 -> (compact) 2345678 -> 0x02345678
            //      - 1 -> (compact) 23 - 4 -> (compact) 567 - 8 -> 0x12345678
            //                                               - 9 -> 0x12345679
            //                          - 5 -> (compact) 5678 -> 0x12355678
            AddHexa(tree, 0x12355678);

            // diverge on random node
            // root - 0 -> (compact) 23 - 4 -> (compact) 5678 -> 0x02345678
            //                          - 5 -> (compact) 5678 -> 0x02355678
            //      - 1 -> (compact) 23 - 4 -> (compact) 567 - 8 -> 0x12345678
            //                                               - 9 -> 0x12345679
            //                          - 5 -> (compact) 5678 -> 0x12355678
            AddHexa(tree, 0x02355678);

            // root - 0 -> (compact) 23 - 4 -> (compact) 5678 -> 0x02345678
            //                          - 5 -> (compact) 5678 -> 0x02355678
            //                          - 6 -> (compact) 5678 -> 0x02365678
            //      - 1 -> (compact) 23 - 4 -> (compact) 567 - 8 -> 0x12345678
            //                                               - 9 -> 0x12345679
            //                          - 5 -> (compact) 5678 -> 0x12355678
            AddHexa(tree, 0x02365678);

            // root - 0 -> (compact) 23 - 4 -> (compact) 5678 -> 0x02345678
            //                          - 5 -> (compact) 5678 -> 0x02355678
            //                          - 6 -> 5 - 6 (compact) 78 -> 0x02365678
            //                                   - 7 (compact) 78 -> 0x02365778
            //      - 1 -> (compact) 23 - 4 -> (compact) 567 - 8 -> 0x12345678
            //                                               - 9 -> 0x12345679
            //                          - 5 -> (compact) 5678 -> 0x12355678
            AddHexa(tree, 0x02365778);

            // root - 0 -> (compact) 23 - 4 -> (compact) 5678 -> 0x02345678
            //                          - 5 -> (compact) 5678 -> 0x02355678
            //                          - 6 -> 5 - 6 (compact) 78 -> 0x02365678
            //                                   - 7 (compact) 78 -> 0x02365778
            //      - 1 - 1 -> (compact) 345678 -> 0x11345678
            //          - 2 -> 3 - 4 -> (compact) 567 - 8 -> 0x12345678
            //                                        - 9 -> 0x12345679
            //                   - 5 -> (compact) 5678 -> 0x12355678
            AddHexa(tree, 0x11345678);
        }

        private void AddHexa(Trie<int> tree, int value)
        {
            var expected = tree.Count + 1;
            Assert.IsTrue(tree.Add(BitConverter.GetBytes(value).Reverse().ToArray(), value));
            Assert.AreEqual(expected, tree.Count);
            Assert.AreEqual(expected, tree.GetEnumerable().Count());
        }
    }
}