using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Reflection;

namespace FManagerApp.Forms
{
    public partial class ShowAssemblyForm : Form
    {
        public Assembly assembly
        { get; set; }

        public ShowAssemblyForm()
        {
            InitializeComponent();
        }

        private void ShowAssemblyForm_Load(object sender, EventArgs e)
        {
            Type[] types = assembly.GetTypes();
            foreach (Type type in types)
            {
                if (!type.IsClass && !type.IsInterface) continue;
                TreeNode treeNode = new TreeNode(type.Name);
                AssemblyTreeView.Nodes.Add(treeNode);
                AddInterfaces(treeNode,type);
                AddFields(treeNode, type);
                AddConstructors(treeNode, type);
                AddMethods(treeNode,type);
            }
        }

        private void AddInterfaces(TreeNode root, Type type)
        {
            TreeNode headerNode = new TreeNode("Реализуемые интерфейсы");
            root.Nodes.Add(headerNode);
            foreach (Type interfaseInfo in type.GetInterfaces())
            {
                headerNode.Nodes.Add(new TreeNode(interfaseInfo.Name));
            }
        }

        private void AddFields(TreeNode root, Type type)
        {
            TreeNode headerNode = new TreeNode("Поля");
            root.Nodes.Add(headerNode);
            foreach (FieldInfo field in type.GetFields(BindingFlags.DeclaredOnly | BindingFlags.NonPublic | BindingFlags.Static))
            {
                headerNode.Nodes.Add(new TreeNode("private static "+field.Name+" : " + field.FieldType.Name));
            }

            foreach (FieldInfo field in type.GetFields(BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Static))
            {
                headerNode.Nodes.Add(new TreeNode("public static " + field.Name + " : " + field.FieldType.Name));
            }

            foreach (FieldInfo field in type.GetFields(BindingFlags.DeclaredOnly | BindingFlags.NonPublic | BindingFlags.Instance))
            {
                headerNode.Nodes.Add(new TreeNode("private " + field.Name + " : " + field.FieldType.Name));
            }

            foreach (FieldInfo field in type.GetFields(BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance))
            {
                headerNode.Nodes.Add(new TreeNode("public " + field.Name + " : " + field.FieldType.Name));
            }
        }

        private void AddMethods(TreeNode root, Type type)
        {
            TreeNode headerNode = new TreeNode("Методы");
            root.Nodes.Add(headerNode);
            foreach (MethodInfo method in type.GetMethods(BindingFlags.DeclaredOnly | BindingFlags.NonPublic | BindingFlags.Static))
            {
                headerNode.Nodes.Add(new TreeNode("private static " + method.Name + GetParametersString(method) + " : " + method.ReturnType.Name));
            }

            foreach (MethodInfo method in type.GetMethods(BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Static))
            {
                headerNode.Nodes.Add(new TreeNode("public static " + method.Name + GetParametersString(method) + " : " + method.ReturnType.Name));
            }

            foreach (MethodInfo method in type.GetMethods(BindingFlags.DeclaredOnly | BindingFlags.NonPublic | BindingFlags.Instance))
            {
                headerNode.Nodes.Add(new TreeNode("private " + method.Name + GetParametersString(method) + " : " + method.ReturnType.Name));
            }

            foreach (MethodInfo method in type.GetMethods(BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance))
            {
                headerNode.Nodes.Add(new TreeNode("public " + method.Name + GetParametersString(method) + " : " + method.ReturnType.Name));
            }
        }

        private string GetParametersString(MethodBase method)
        {
            string args = "(";
            ParameterInfo[] parameterInfoArr = method.GetParameters();
            for (int i = 0; i < parameterInfoArr.Length; i++)
            {
                args = args + parameterInfoArr[i].ParameterType.Name + " " + parameterInfoArr[i].Name;
                if (i != parameterInfoArr.Length-1) args+=", ";
            }
            args += ")";
            return args;
        }

        private void AddConstructors(TreeNode root, Type type)
        {
            TreeNode headerNode = new TreeNode("Конструкторы");
            root.Nodes.Add(headerNode);
            foreach (ConstructorInfo constructor in type.GetConstructors(BindingFlags.DeclaredOnly | BindingFlags.NonPublic | BindingFlags.Instance))
            {
                headerNode.Nodes.Add(new TreeNode("private " + constructor.Name + GetParametersString(constructor)));
            }
            foreach (ConstructorInfo constructor in type.GetConstructors(BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance))
            {
                headerNode.Nodes.Add(new TreeNode("public " + constructor.Name + GetParametersString(constructor)));
            }
        }

    }
}
