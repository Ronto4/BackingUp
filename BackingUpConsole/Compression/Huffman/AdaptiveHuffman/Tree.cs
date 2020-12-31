using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Compression.Huffman.AdaptiveHuffman
{
    class Tree<T> where T : struct, IEquatable<T>
    {
        // Attributes
        public Node<T> Root { get; private set; }
        private SortedSet<Node<T>> Nodes { get; } = new SortedSet<Node<T>>();
        private List<Node<T>> Leaves { get; } = new List<Node<T>>();
        // Constrcutors
        public Tree()
        {
            Root = Node<T>.CreateNewRoot();
            Leaves.Add(Root);
            Nodes.Add(Root);
        }
        // Methods
        public void AddElement(T value)
        {
            Node<T> node = Leaves.FirstOrDefault(Leaf => Leaf.Value is T && Leaf.Value.Equals(value)) ?? Leaves.First(Leaf => Leaf.IsNotYetTransferred);
            if (node.IsNotYetTransferred)
            {
                Leaves.Remove(node);
                Node<T> leftChild = new Node<T>();
                node.SetLeftChild(leftChild);
                Node<T> rightChild = new Node<T>(value);
                node.SetRightChild(rightChild);
                Leaves.Add(leftChild);
                Leaves.Add(rightChild);
                Nodes.Add(leftChild);
                Nodes.Add(rightChild);
                return;
            }
            Node<T> maxWithWeight = Nodes.Where(Node => Node.Weight == node.Weight).Max();
            if (maxWithWeight == node)
            {
                node.AddOneEntry();
                return;
            }
            //Node<T>? leafToIncrement = null;
            //Node<T> nextNode = Leaves.FirstOrDefault(Leave => Leave.Value is T && Leave.Value.Equals(value)) ?? Leaves.First(Leave => Leave.IsNotYetTransferred);
            //if (nextNode.IsNotYetTransferred)
            //{
            //    nextNode.LeftChild = new Node<T>();
            //    nextNode.RightChild = new Node<T>(value);
            //    leafToIncrement = nextNode.RightChild;
            //}
            //else
            //{

            //}
        }
    }
}
