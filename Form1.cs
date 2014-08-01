using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using System.DirectoryServices;
using RTXSAPILib;
using RTXServerApi;
using System.IO;
using System.Collections;
using System.Runtime.InteropServices;
using System.Xml;

namespace GetLDAPUsers
{
    public partial class Form1 : Form
    {
        ArrayList arrayList = new ArrayList();
        ArrayList nodeList = new ArrayList();
        Hashtable hashTable = new Hashtable();

        RTXSAPILib.RTXSAPIRootObj RootObj; //声明一个根对象
        RTXSAPILib.RTXSAPIUserAuthObj UserAuthObj; //声明一个用户认证对象
        private TreeView thetreeview;
        private string xmlfilepath;
        XmlTextWriter textWriter;
        XmlNode Xmlroot;
        XmlDocument textdoc;
        string labe1Text1 = "";
        public Form1()
        {
            InitializeComponent();
            RootObj = new RTXSAPILib.RTXSAPIRootObj(); //创建根对象
            UserAuthObj = RootObj.UserAuthObj;//通过根对象创建用户认证对象

            RootObj.ServerIP = "127.0.0.1"; //设置服务器IP
            RootObj.ServerPort = 8006; //设置服务器端口

            UserAuthObj.AppGUID = "{0EA21A17-BF30-4603-8F42-149CF4A27D87}";

            this.createTree();
            // this.getUsers("","");
        }

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {

        }
        private void treeView1_AfterCheck(object sender, TreeViewEventArgs e)
        {

            CheckControl(e);
            label1.Text = labe1Text1;
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void createTree()
        {
            RTXServerApi.RTXObjectClass RTXObj = new RTXObjectClass();  //创建一个业务逻辑对象
            RTXServerApi.RTXCollectionClass RTXParams = new RTXCollectionClass();// 创建一个集合对象

            RTXObj.Name = "USERSYNC";  //业务逻辑对象名称为用户数据同步

            RTXParams.Add("MODIFYMODE", 1);
            RTXParams.Add("XMLENCODE", @"<?xml version=" + "\"" + "1.0" + "\"" + " encoding=" + "\"" + "gb2312" + "\"" + " ?>");
            try
            {
                object rtxData = RTXObj.Call2(enumCommand_.PRO_SYNC_FROM_RTX, RTXParams);
                string userInfos = rtxData.ToString();

                string dir = "c:\\";
                FileStream fs = new FileStream(dir + "users.xml", FileMode.Create);
                StreamWriter sw = new StreamWriter(fs, Encoding.Default);
                sw.Write(userInfos);
                sw.Close();
                fs.Close();
                XMLToTree("c:/users.xml", treeView1);
                //MessageBox.Show("导出成功");
            }
            catch (COMException ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        public void CheckControl(TreeViewEventArgs e)
        {
            if (e.Action != TreeViewAction.Unknown)
            {
                if (e.Node != null && !Convert.IsDBNull(e.Node))
                {
                    CheckParentNode(e.Node);
                    if (e.Node.Nodes.Count > 0)
                    {
                        CheckAllChildNodes(e.Node, e.Node.Checked);
                    }
                    else
                    {
                        arrayList.Add(e.Node.Text);
                    }
                }
            }
        }

        //改变所有子节点的状态
        private void CheckAllChildNodes(TreeNode pn, bool IsChecked)
        {
            foreach (TreeNode tn in pn.Nodes)
            {
                tn.Checked = IsChecked;

                if (tn.Nodes.Count > 0)
                {
                    CheckAllChildNodes(tn, IsChecked);
                }
                else
                {

                    arrayList.Add(tn.Text);
                }

            }
        }

        //改变父节点的选中状态，此处为所有子节点不选中时才取消父节点选中，可以根据需要修改
        private void CheckParentNode(TreeNode curNode)
        {
            bool bChecked = false;

            if (curNode.Parent != null)
            {
                foreach (TreeNode node in curNode.Parent.Nodes)
                {
                    if (node.Checked)
                    {
                        bChecked = true;
                        break;
                    }
                }
                if (bChecked)
                {
                    curNode.Parent.Checked = true;
                    CheckParentNode(curNode.Parent);
                }
                else
                {
                    curNode.Parent.Checked = false;
                    CheckParentNode(curNode.Parent);
                }
            }
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {

            radioButton2.Checked = !radioButton1.Checked;
        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {

            radioButton1.Checked = !radioButton2.Checked;
        }


        private void button1_Click(object sender, EventArgs e)
        {
            bool authType = false;
            string authTypeStr = "本地认证";

            if (radioButton1.Checked)
            {
                authType = true;
                authTypeStr = "第三方认证";
            }
            else if (radioButton2.Checked)
            {
                authType = false;
            }
            try
            {
                string label1Text = "";
                foreach (string name in arrayList)
                {
                    UserAuthObj.SetUserAuthType(name, authType);

                    label1Text += name + ",";

                }

                label1.Text = label1Text + "的认证方式已更改为" + authTypeStr;
                MessageBox.Show("设置成功");

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }


        #region 遍历XML并实现向tree的转化
        /// <summary>   
        /// 遍历treeview并实现向XML的转化
        /// </summary>   
        /// <param name="XMLFilePath">XML输出路径</param>   
        /// <param name="TheTreeView">树控件对象</param>   
        /// <returns>0表示函数顺利执行</returns>   

        public int XMLToTree(string XMLFilePath, TreeView TheTreeView)
        {
            //-------重新初始化转换环境变量
            thetreeview = TheTreeView;
            xmlfilepath = XMLFilePath;

            textdoc = new XmlDocument();
            //-------重新对XMLDocument对象赋值
            textdoc.Load(xmlfilepath);

            XmlNode root = textdoc.SelectSingleNode("enterprise");

            foreach (XmlNode subXmlnod in root.ChildNodes)
            {
                TreeNode trerotnod = new TreeNode();
                trerotnod.Text = subXmlnod.Name;

                //""
                Console.WriteLine(trerotnod.Text);
                thetreeview.Nodes.Add(trerotnod);
                TransXML(subXmlnod.ChildNodes, trerotnod);

            }

            return 0;
        }

        private int TransXML(XmlNodeList Xmlnodes, TreeNode partrenod)
        {
            //------遍历XML中的所有节点，仿照treeview节点遍历函数
            foreach (XmlNode xmlnod in Xmlnodes)
            {
                TreeNode subtrnod = new TreeNode();
                string str = xmlnod.Name;
                subtrnod.Text = xmlnod.Name;//user/department
                if (subtrnod.Text == "user")
                {
                    XmlElement xe = (XmlElement)xmlnod;
                    str = xe.OuterXml;
                    str = str.Substring((str.IndexOf("uid=") + 5), (str.IndexOf("name=") - str.IndexOf("uid=") - 7));
                    Console.WriteLine(str);
                    if (str.Length > 0)
                    {
                        subtrnod.Text = str;
                        // partrenod.Nodes.Add(str);
                    }

                }
                else
                {
                    if (subtrnod.Text == "department")
                    {
                        XmlElement xe = (XmlElement)xmlnod;
                        str = xe.OuterXml;
                        str = str.Substring((str.IndexOf("name=") + 6), (str.IndexOf("describe=") - str.IndexOf("name=") - 8));
                        if (str.Length > 0)
                        {
                            subtrnod.Text = str;
                            //partrenod.Nodes.Add(str);
                        }
                    }
                }
                partrenod.Nodes.Add(subtrnod);

                if (xmlnod.ChildNodes.Count > 0)
                {
                    TransXML(xmlnod.ChildNodes, subtrnod);
                }
            }

            return 0;

        }

        #endregion


    }
}
