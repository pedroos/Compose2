using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Text.RegularExpressions;

namespace Compose2WinForms
{
    using Abstraction;
    using System.Reflection;

    public partial class StateForm : Form
    {
        Node root;
        IEnumerable<TreeNode> lastTreePath;
        IEnumerable<int> lastInitialObj;
        readonly string plusText;

        public StateForm()
        {
            InitializeComponent();

            plusText = typeof(Funcs).GetMember("Plus").Single().GetCustomAttribute<TextAttribute>().Text;

            posArgsTextBox.TextChanged += posArgsTextBox_TextChanged;

            trv.NodeMouseClick += trv_NodeMouseClick;
            trv.NodeMouseDoubleClick += trv_NodeMouseDoubleClick;
            trv.MouseDoubleClick += trv_MouseDoubleClick;
            trv.KeyUp += trv_KeyUp;
            trv2.KeyUp += trv_KeyUp;
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            initObjTextBox.Text = new int[] { 1, 2, 3, 4 }.ToString3();

            posArgsTextBox.Text = new Rule<int, int>((1, 1), (3, -1)).ToString();

            posFuncComboBox.Items.Add(plusText);
            posFuncComboBox.SelectedIndex = 0;
        }

        private void trv_AfterSelect(object sender, TreeViewEventArgs e)
        {

        }

        void trv_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
        }

        void trv_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
        }

        void trv_MouseDoubleClick(object sender, MouseEventArgs e)
        {
        }

        void posArgsTextBox_TextChanged(object sender, EventArgs e)
        {
            var rule = Rule<int, int>.ParseInt(posArgsTextBox.Text);

            var change = new SinglePositionChange<int>(Funcs.Plus, plusText, rule);

            root =
                new Node(change,
                    new Node(change,
                        new Node(change,
                            new Node(change,
                                new Node(change)
                            ),
                            new Node(change)
                        )
                    ),
                    new Node(change)
                );

            ShowTree(root);
        }

        void ShowTree(Node node)
        {
            trv.Nodes.Clear();
            trv2.Nodes.Clear();

            static TreeNode MakeTree1Node(Node node) =>
                new TreeNode(string.Format("no value"), AddChildren(node, MakeTree1Node)) { Tag = node };

            static TreeNode MakeTree2Node(Node node) =>
                new TreeNode(string.Format("{0}", node.Value), AddChildren(node, MakeTree2Node)) { Tag = node };

            static TreeNode[] AddChildren(Node n, Func<Node,TreeNode> makeTreeNode) =>
                (n.Children != null) ?
                    n.Children.Select(ch => makeTreeNode((Node)ch)).ToArray() :
                    Array.Empty<TreeNode>();

            var tree1Node = MakeTree1Node(node);

            trv.Nodes.Add(tree1Node);
            trv.ExpandAll();

            var tree2Node = MakeTree2Node(node);
            trv2.Nodes.Add(tree2Node);
            trv2.ExpandAll();
        }

        void trv_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                if (sender == trv)
                {
                    Calculate();
                    trv.SelectedNode = null;
                }
            }
            else if (e.KeyCode == Keys.F2)
            {
                ((TreeView)sender).SelectedNode.BeginEdit();
            }
        }

        private void calcButton_Click(object sender, EventArgs e)
        {
        }

        void Calculate()
        {
            if (trv.SelectedNode == null) return;

            var node = (Node)trv.SelectedNode.Tag;

            var initialObj = initObjTextBox.Text.ParseIntArray();
            var posArgs = Rule<int, int>.ParseInt(posArgsTextBox.Text);
            if (posArgs == default)
                return;

            if (lastInitialObj == null || !initialObj.SequenceEqual(lastInitialObj))
            {
                trv.TopNode.DepthFirst(tn => tn.Text = string.Format("no value"));
                lastInitialObj = initialObj;
            }

            var path = node.WalkUp().Reverse().Cast<Node>();
            var objsChanged = path.ChangeHistory(initialObj);
            // From node to root, so reverse history
            var objsChangedEn = objsChanged.Reverse().GetEnumerator();

            if (lastTreePath != null)
                foreach (var trvNode in lastTreePath)
                    trvNode.BackColor = Color.Empty;

            var trvPath = lastTreePath = trv.SelectedNode.WalkUp();

            foreach (var trvNode in trvPath)
            {
                trvNode.BackColor = trvNode == trv.SelectedNode ? Color.Orange : Color.Yellow;
                objsChangedEn.MoveNext();
                trvNode.Text = string.Format("{0}", objsChangedEn.Current.ToString2());
            }
        }

        private void label4_Click(object sender, EventArgs e)
        {

        }
    }

    public static class Extensions
    {
        public static string ToString2(this IEnumerable<int> obj) 
        {
            return string.Format("[{0}]", string.Join(", ", obj.Select(e => e.ToString())));
        }
        public static string ToString3(this IEnumerable<int> obj)
        {
            return string.Format("{0}", string.Join(", ", obj.Select(e => e.ToString())));
        }

        static readonly Regex parseIntArrayRegex = new Regex("^( *[0-9]*,?)*$");

        public static IEnumerable<int> ParseIntArray(this string str)
        {
            if (!parseIntArrayRegex.IsMatch(str)) return Array.Empty<int>();
            return str.Split(",").Select(s => s.Trim()).Where(s => s.Length > 0).Select(s => int.Parse(s)).ToArray();
        }

        public static IEnumerable<TreeNode> WalkUp(this TreeNode node)
        {
            var curr = node;
            while (true)
            {
                yield return curr;
                if (curr.Parent == default)
                    yield break;
                curr = curr.Parent;
            }
        }

        public static IEnumerable<TreeNode> DepthFirst(this TreeNode node)
        {
            yield return node;
            foreach (TreeNode child in node.Nodes)
                foreach (var n in child.DepthFirst())
                    yield return n;
        }

        public static void DepthFirst(this TreeNode node, Action<TreeNode> action)
        {
            action(node);
            foreach (TreeNode child in node.Nodes)
                foreach (var n in child.DepthFirst())
                    action(n);
        }
    }
}
