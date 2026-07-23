using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.IO;
using System.Threading.Tasks;
using System.Xml.Serialization;
using UnityEngine.SceneManagement;


namespace Editor
{
    [XmlRoot("Hierarchy")]
    public class HierarchyData
    {
        [XmlAttribute("SceneName")] public string sceneName { get; set; }
        [XmlAttribute("ExportTime")] public DateTime exportTime { get; set; }
        [XmlElement("GameObject")] public List<HierarchyNode> roots = new();

        // 输出数据树
        public override string ToString()
        {
            string result = $"Scene: {sceneName}\n";

            foreach (var root in roots)
            {
                result += root.ToString();
            }

            return result;
        }
    }

    public class HierarchyNode
    {
        [XmlAttribute("Name")] public string name { get; set; }
        [XmlAttribute("ID")] public int instanceID { get; set; }
        [XmlAttribute("Active")] public bool isActive { get; set; }
        [XmlAttribute("Script")] public string scriptName { get; set; }

        [XmlElement("GameObject")] public List<HierarchyNode> children = new();

        public override string ToString()
        {
            return ToString(0);
        }

        // 递归打印层级结构
        private string ToString(int depth)
        {
            string indent = new string(' ', depth * 2);
            string result = $"{indent}└{name} (ID: {instanceID}, Active: {isActive}, Script: {scriptName})\n";

            if (children == null || children.Count == 0) return result;
            foreach (var child in children)
            {
                result += child.ToString(depth + 1);
            }

            return result;
        }
    }

    
    [InitializeOnLoad]
    public static class HierarchyExporter
    {
        static HierarchyExporter()
        {
            // 监听Hierarchy变化事件并获取层级数据
            EditorApplication.hierarchyChanged += GetHierarchyData;
        }

        private static async void GetHierarchyData()
        {
            List<GameObject> objectInScene = new List<GameObject>();

            foreach (var gameObject in SceneManager.GetActiveScene().GetRootGameObjects())
            {
                objectInScene.Add(gameObject);
            }

            // Hierarchy根目录构建
            var rootNode = new HierarchyData()
            {
                sceneName = SceneManager.GetActiveScene().name,
                exportTime = DateTime.Now,
                roots = new List<HierarchyNode>()
            };

            // 遍历场景中的所有根物体，构建层级树
            foreach (var rootObject in objectInScene)
            {
                rootNode.roots.Add(BuildHn(rootObject.transform));
            }

            
            // 将层级树打印到控制台
            //Debug.Log(rootNode.ToString());
            
            // 将层级树导出为XML-String
            string xmlData = XmlExporter.Export(rootNode);
            
            //Debug.Log(xmlData);
            
            // 发送XML数据到客户端
            await TcpServer.SendMessage(MessageType.XML, xmlData);
        }

        // 插入头节点构建树
        private static HierarchyNode BuildHn(Transform t)
        {
            if (t == null) return null; //防止空节点报错

            var node = new HierarchyNode()
            {
                name = t.name,
                instanceID = t.gameObject.GetInstanceID(),
                isActive = t.gameObject.activeSelf,
                scriptName = t.gameObject.GetComponent<MonoBehaviour>()?.GetType().Name ?? "",
                children = new List<HierarchyNode>(),
            };
            
            foreach (Transform child in t)
            {
                node.children.Add(BuildHn(child));
            }

            return node;
        }
    }
}

