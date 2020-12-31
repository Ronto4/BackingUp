using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Compression.Huffman.AdaptiveHuffman
{
    // Sources for whole file: https://en.wikipedia.org/wiki/Adaptive_Huffman_coding, https://www2.cs.duke.edu/csed/curious/compression/adaptivehuff.html
    class Node<T> : IComparer<Node<T>> where T : struct, IEquatable<T>
    {
        // Attributes
        public Node<T>? Parent { get; private set; }
        public Node<T>? LeftChild { get; private set; }
        public Node<T>? RightChild { get; private set; }
        public bool IsLeaf => LeftChild is null && RightChild is null;
        public bool IsRoot => Parent is null && IsLeaf;
        public bool IsNotYetTransferred => Value is null;
        public T? Value { get; }
        public int Number { get; private set; }
        private int _weight;
        public int Weight => IsLeaf ? _weight : (LeftChild?.Weight ?? 0) + (RightChild?.Weight ?? 0);
        // Constructors
        private Node(T? value, int weight, int number, Node<T>? leftChild, Node<T>? rightChild, Node<T>? parent)
        {
            Value = value;
            _weight = weight;
            Number = number;
            LeftChild = leftChild;
            RightChild = rightChild;
            Parent = parent;
        }
        public Node() : this(default, 0, 0, null, null, null) { }
        public Node(T? value)
        {
            Value = value;
            _weight = 1;
        }
        // Methods
        public static Node<T> CreateNewRoot() => new Node<T>() { Number = int.MaxValue };
        public (Node<T>?, int) SetLeftChild(Node<T> node)
        {
            Node<T>? oldChild = LeftChild;
            int nodesOldNumber = node.Number;
            LeftChild = node;
            LeftChild.Parent = this;
            LeftChild.Number = Number - 1;
            return (oldChild, nodesOldNumber);
        }
        public Node<T>? SetLeftChild(T? value = null)
        {
            Node<T>? oldChild = LeftChild;
            LeftChild = new Node<T>(value)
            {
                Parent = this,
                Number = Number - 1
            };
            return oldChild;
        }
        public (Node<T>?, int) SetRightChild(Node<T> node)
        {
            Node<T>? oldChild = RightChild;
            int nodesOldNumber = node.Number;
            RightChild = node;
            RightChild.Parent = this;
            RightChild.Number = Number - 2;
            return (oldChild, nodesOldNumber);
        }
        public Node<T>? SetRightChild(T? value = null)
        {
            Node<T>? oldChild = RightChild;
            RightChild = new Node<T>(value)
            {
                Parent = this,
                Number = Number - 2
            };
            return oldChild;
        }
        public void AddOneEntry()
        {
            _weight++;
        }

        public int Compare([AllowNull] Node<T> x, [AllowNull] Node<T> y) => x is Node<T> && y is Node<T> ? x.Number - y.Number : throw new ArgumentNullException(x is null ? (y is null ? $"{nameof(x)} and {nameof(y)}" : nameof(x)) : nameof(y), "Comparer argument was null.");
    }
}
