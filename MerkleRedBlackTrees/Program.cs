using System.Security.Cryptography;
using System.Text;

namespace MerkleRedBlackTrees;

public enum NodeColor
{
    Red,
    Black
}

public class MerkleRedBlackNode<T> where T : IComparable<T>
{
    public T Data { get; set; }
    public NodeColor Color { get; set; }
    public MerkleRedBlackNode<T>? Left { get; set; }
    public MerkleRedBlackNode<T>? Right { get; set; }
    public MerkleRedBlackNode<T>? Parent { get; set; }
    public string Hash { get; private set; }

    public MerkleRedBlackNode(T data)
    {
        Data = data;
        Color = NodeColor.Red;
        Left = null;
        Right = null;
        Parent = null;
        Hash = CalculateHash();
    }

    public string CalculateHash()
    {
        using var sha256 = SHA256.Create();
        var dataString = Data.ToString();
        var leftHash = Left?.Hash ?? string.Empty;
        var rightHash = Right?.Hash ?? string.Empty;

        var combined = $"{dataString}{leftHash}{rightHash}";
        return BitConverter.ToString(sha256.ComputeHash(Encoding.UTF8.GetBytes(combined))).Replace("-", "");
    }

    public void UpdateHash()
    {
        Hash = CalculateHash();
    }
	
    public override bool Equals(object obj)
    {
        return Equals(obj as MerkleRedBlackNode<T>);
    }

    public bool Equals(MerkleRedBlackNode<T> other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;

        return Data.CompareTo(other.Data) == 0 && Hash == other.Hash;
    }

    public override int GetHashCode()
    {
        return Hash != null ? Hash.GetHashCode() : 0;
    }

    public static bool operator ==(MerkleRedBlackNode<T> left, MerkleRedBlackNode<T> right)
    {
        if (ReferenceEquals(left, right)) return true;
        if (left is null || right is null) return false;

        return left.Equals(right);
    }

    public static bool operator !=(MerkleRedBlackNode<T> left, MerkleRedBlackNode<T> right)
    {
        return !(left == right);
    }
}

public class MerkleRedBlackTree<T> where T : IComparable<T>
{
    private MerkleRedBlackNode<T> root;
    private readonly MerkleRedBlackNode<T> nilNode;

    public MerkleRedBlackTree()
    {
        nilNode = new MerkleRedBlackNode<T>(default(T)) { Color = NodeColor.Black };
        root = nilNode;
    }

    public void Insert(T data)
    {
        var newNode = new MerkleRedBlackNode<T>(data)
        {
            Left = nilNode,
            Right = nilNode,
            Parent = null
        };

        MerkleRedBlackNode<T> y = null;
        MerkleRedBlackNode<T> x = root;

        while (x != nilNode)
        {
            y = x;
            if (newNode.Data.CompareTo(x.Data) < 0)
                x = x.Left;
            else
                x = x.Right;
        }

        newNode.Parent = y;
        if (y == null)
            root = newNode;
        else if (newNode.Data.CompareTo(y.Data) < 0)
            y.Left = newNode;
        else
            y.Right = newNode;

        newNode.Color = NodeColor.Red;
        InsertFixUp(newNode);
        UpdateHashesUpToRoot(newNode);
    }

    private void InsertFixUp(MerkleRedBlackNode<T> z)
    {
        while (z.Parent != null && z.Parent.Color == NodeColor.Red)
        {
            if (z.Parent == z.Parent.Parent.Left)
            {
                var y = z.Parent.Parent.Right;
                if (y.Color == NodeColor.Red)
                {
                    z.Parent.Color = NodeColor.Black;
                    y.Color = NodeColor.Black;
                    z.Parent.Parent.Color = NodeColor.Red;
                    z = z.Parent.Parent;
                }
                else
                {
                    if (z == z.Parent.Right)
                    {
                        z = z.Parent;
                        LeftRotate(z);
                    }
                    z.Parent.Color = NodeColor.Black;
                    z.Parent.Parent.Color = NodeColor.Red;
                    RightRotate(z.Parent.Parent);
                }
            }
            else
            {
                var y = z.Parent.Parent.Left;
                if (y.Color == NodeColor.Red)
                {
                    z.Parent.Color = NodeColor.Black;
                    y.Color = NodeColor.Black;
                    z.Parent.Parent.Color = NodeColor.Red;
                    z = z.Parent.Parent;
                }
                else
                {
                    if (z == z.Parent.Left)
                    {
                        z = z.Parent;
                        RightRotate(z);
                    }
                    z.Parent.Color = NodeColor.Black;
                    z.Parent.Parent.Color = NodeColor.Red;
                    LeftRotate(z.Parent.Parent);
                }
            }
        }
        root.Color = NodeColor.Black;
    }

    private void LeftRotate(MerkleRedBlackNode<T> x)
    {
        var y = x.Right;
        x.Right = y.Left;
        if (y.Left != nilNode)
            y.Left.Parent = x;
        y.Parent = x.Parent;
        if (x.Parent == null)
            root = y;
        else if (x == x.Parent.Left)
            x.Parent.Left = y;
        else
            x.Parent.Right = y;
        y.Left = x;
        x.Parent = y;
    }

    private void RightRotate(MerkleRedBlackNode<T> y)
    {
        var x = y.Left;
        y.Left = x.Right;
        if (x.Right != nilNode)
            x.Right.Parent = y;
        x.Parent = y.Parent;
        if (y.Parent == null)
            root = x;
        else if (y == y.Parent.Left)
            y.Parent.Left = x;
        else
            y.Parent.Right = x;
        x.Right = y;
        y.Parent = x;
    }

    private void UpdateHashesUpToRoot(MerkleRedBlackNode<T> node)
    {
        while (node != null && node != nilNode)
        {
            node.UpdateHash();
            node = node.Parent;
        }
    }

    public void InOrderTraversal()
    {
        InOrderHelper(root);
    }

    private void InOrderHelper(MerkleRedBlackNode<T> node)
    {
        if (node != nilNode)
        {
            InOrderHelper(node.Left);
            Console.WriteLine($"{node.Data} ({node.Color}) - Hash: {node.Hash}");
            InOrderHelper(node.Right);
        }
    }

    public string GetRootHash()
    {
        return root?.Hash;
    }
	
    public override bool Equals(object obj)
    {
        return Equals(obj as MerkleRedBlackTree<T>);
    }

    public bool Equals(MerkleRedBlackTree<T> other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;

        return GetRootHash() == other.GetRootHash();
    }

    public override int GetHashCode()
    {
        var rootHash = GetRootHash();
        return rootHash != null ? rootHash.GetHashCode() : 0;
    }

    public static bool operator ==(MerkleRedBlackTree<T> left, MerkleRedBlackTree<T> right)
    {
        if (ReferenceEquals(left, right)) return true;
        if (left is null || right is null) return false;

        return left.Equals(right);
    }

    public static bool operator !=(MerkleRedBlackTree<T> left, MerkleRedBlackTree<T> right)
    {
        return !(left == right);
    }
}

// Usage example:
public class Program
{
    public static void Main()
    {
        var rbTree = new MerkleRedBlackTree<int>();
        rbTree.Insert(10);
        rbTree.Insert(20);
        rbTree.Insert(15);
        rbTree.Insert(30);
        rbTree.Insert(25);

        Console.WriteLine("In-order Traversal:");
        rbTree.InOrderTraversal();
        Console.WriteLine();
        Console.WriteLine($"Merkle Root Hash: {rbTree.GetRootHash()}");
        Console.WriteLine();
		
        var rbTree2 = new MerkleRedBlackTree<int>();
        rbTree2.Insert(10);
        rbTree2.Insert(20);
        rbTree2.Insert(15);
        rbTree2.Insert(30);
        rbTree2.Insert(25);

        Console.WriteLine("In-order Traversal:");
        rbTree2.InOrderTraversal();
        Console.WriteLine();
        Console.WriteLine($"Merkle Root Hash: {rbTree2.GetRootHash()}");
        Console.WriteLine();
		
        Console.WriteLine(rbTree == rbTree2 ? "Trees are equal" : "Trees are not equal");
    }
}