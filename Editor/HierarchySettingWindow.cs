using System;
using System.Collections;
using UnityEngine;
using UnityEditor;

namespace Editor
{
    public class HierarchySettingWindow : EditorWindow
    {

        public static int port
        {
            get => EditorPrefs.GetInt("HierarchySettingWindow_Port", 44571); 
            private set => EditorPrefs.SetInt("HierarchySettingWindow_Port", value);
        }
        public static float heartbeatIntervalSeconds
        {
            get => EditorPrefs.GetFloat("HierarchySettingWindow_HeartbeatIntervalSeconds", 1.0f); 
            private set => EditorPrefs.SetFloat("HierarchySettingWindow_HeartbeatIntervalSeconds", value);
        }

        
        // 临时变量负责GUI中的显示修改，降低反复get/set开销
        private int m_Port;
        private float m_HeartbeatIntervalSeconds;
        
        
        [MenuItem("Tools/IDE Hierarchy Viewer Setting/Settings")]
        public static void ShowWindow()
        {
            GetWindow<HierarchySettingWindow>("IDE Hierarchy Viewer Setting");
        }

        private void OnEnable()
        {
            m_Port = port;
            m_HeartbeatIntervalSeconds = heartbeatIntervalSeconds;
        }


        private void OnGUI()
        {
            GUILayout.Label("TCP Port Setting", EditorStyles.boldLabel);
            m_Port = EditorGUILayout.IntField("Port:", m_Port); //需要检查port是否占用

            m_HeartbeatIntervalSeconds = EditorGUILayout.FloatField("Heartbeat Interval (second):", m_HeartbeatIntervalSeconds);
            GUILayout.Space(10);


            if (GUILayout.Button("Save Settings"))
            {
                port = m_Port;
                heartbeatIntervalSeconds = m_HeartbeatIntervalSeconds;
                Debug.Log($"{port}, {heartbeatIntervalSeconds} saved to EditorPrefs.");
            }
        }


        /*
        private int PortChecker(int port)
        {
            while (IsPortInUse(port))
            {
                port++;
            }
            return port;
        }

        private bool IsPortInUse(int port)
        {
            try
            {
                //如果被占用，则start时会抛出异常
                TcpListener listener = new TcpListener(IPAddress.Loopback, port);
                listener.Start();
                listener.Stop();
                return false; //端口未被占用
            }
            catch
            {
                return true; //端口被占用
            }
        }*/
    }
}