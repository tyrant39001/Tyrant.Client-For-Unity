//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Reflection;
//using System.Text;
//using System.Threading.Tasks;
//using UnityEditor;
//using UnityEngine;
//using UnityEngine.UIElements;

//[InitializeOnLoad]
//public static class ToolbarCallbackTest
//{
//	static ToolbarCallbackTest()
//	{
//        ToolbarExtender.LeftToolbarGUI += ToolbarCallback_LeftToolbarGUI;
//        ToolbarExtender.RightToolbarGUI += ToolbarCallback_RightToolbarGUI;
//    }

//    private static void ToolbarCallback_LeftToolbarGUI()
//    {
//        GUILayout.FlexibleSpace();
//        if (GUILayout.Button(new GUIContent("1", "Start Scene 1")))
//        {
//            //SceneHelper.StartScene("Assets/ToolbarExtender/Example/Scenes/Scene1.unity");
//        }

//        if (GUILayout.Button(new GUIContent("2", "Start Scene 2")))
//        {
//            //SceneHelper.StartScene("Assets/ToolbarExtender/Example/Scenes/Scene2.unity");
//        }
//    }

//    private static void ToolbarCallback_RightToolbarGUI()
//    {
        
//    }
//}