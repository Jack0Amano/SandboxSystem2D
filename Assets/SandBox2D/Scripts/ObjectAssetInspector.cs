using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace SandBox2D
{
#if UNITY_EDITOR
    [CustomEditor(typeof(BlockAsset))]
    public class ObjectAssetInspector : Editor
    {
        private int gridSize = 3;

        public override void OnInspectorGUI()
        {
            BlockAsset sandbox = (BlockAsset)target;

            DrawDefaultInspector();

            // Draw block shape editor
            //GUILayout.Space(10);
            //GUILayout.Label("Grid Editor", EditorStyles.boldLabel);
            
            //for (int y = 0; y < gridSize; y++)
            //{
            //    GUILayout.BeginHorizontal();
            //    for (int x = 0; x < gridSize; x++)
            //    {
            //        if (GUILayout.Button("", GUILayout.Width(20), GUILayout.Height(20)))
            //        {
            //            Debug.Log("Clicked");
            //        }
            //    }
            //    GUILayout.EndHorizontal();
            //}

        }
    }
#endif
}
