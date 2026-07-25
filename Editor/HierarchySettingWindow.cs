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
        public static int heartbeatIntervalSeconds
        {
            get => EditorPrefs.GetInt("HierarchySettingWindow_HeartbeatIntervalSeconds", 1); 
            private set => EditorPrefs.SetInt("HierarchySettingWindow_HeartbeatIntervalSeconds", value);
        }

        
        // 临时变量负责GUI中的显示修改，降低反复get/set开销
        private int m_Port;
        private int m_HeartbeatIntervalSeconds;
        
        
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

            m_HeartbeatIntervalSeconds = EditorGUILayout.IntField("Heartbeat Interval (s):", m_HeartbeatIntervalSeconds);
            GUILayout.Space(10);


            if (GUILayout.Button("Save Settings"))
            {
                port = m_Port;
                heartbeatIntervalSeconds = m_HeartbeatIntervalSeconds;
                Debug.Log($"{port}, {heartbeatIntervalSeconds} saved to EditorPrefs.");
            }

            if (GUILayout.Button("Retry Connection"))
            {
                TcpServer.RetryStart(m_Port);
            }
        }
        
    }
}