using UnityEditor;
using UnityEngine;

namespace UIFrame.Editor
{
    public class GenerateScriptWindow : EditorWindow
    {
        private const string IconPath = "Assets/_Res/InApp/Sprites/AvatarBoy/0b1.png";
        private static GenerateScriptWindow codeWindow = null;
        public static void OpenWindow()
        {
            if (codeWindow == null)
                codeWindow = EditorWindow.GetWindow(typeof(GenerateScriptWindow)) as GenerateScriptWindow;

            Texture2D icon = (Texture2D)EditorGUIUtility.Load(IconPath);

            codeWindow.titleContent = new GUIContent("QuickScript", icon);
            codeWindow.Show();
        }
    }
}
