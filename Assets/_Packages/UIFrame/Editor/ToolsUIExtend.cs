    using TMPro;
    using UnityEditor;
    using UnityEditor.SceneManagement;
    using UnityEngine;

    public static class ToolsUIExtend
    {
        [MenuItem("GameObject/Tools/Create Canvas")]
        public static void CreateCanvas()
        {
            CreatePrefabs("UI_Canvas");
        }

        [MenuItem("GameObject/Tools/Create Panel")]
        public static void CreatePanel()
        {
            CreatePrefabs("UI_Panel");
        }
        
        [MenuItem("GameObject/Tools/Create Button")]
        public static void CreateButton()
        {
            CreatePrefabs("UI_Button");
        }
        
        [MenuItem("GameObject/Tools/Create TMPUI")]
        public static void CreateTMPUI()
        {
            CreatePrefabs("UI_Tex");
        }
        
        [MenuItem("GameObject/Tools/Create TMP")]
        public static void CreateTMP()
        {
            CreatePrefabs("Tex");
        }
        
        [MenuItem("GameObject/Tools/Copy TMPUI")]
        public static void CopyTMPUI(MenuCommand menuCommand)
        {
            GameObject selectedObject = menuCommand.context as GameObject;

            var newTex = CreatePrefabs("UI_Tex");

            var newRectTrans = newTex.GetComponent<RectTransform>();
            var oldRectTrans = selectedObject.GetComponent<RectTransform>();
            newTex.GetComponent<RectTransform>().GetCopyOf(selectedObject.GetComponent<RectTransform>());
            newTex.GetComponent<TextMeshProUGUI>().GetCopyOf(selectedObject.GetComponent<TextMeshProUGUI>());

            newRectTrans.anchoredPosition = oldRectTrans.anchoredPosition;
            newRectTrans.anchorMax = oldRectTrans.anchorMax;
            newRectTrans.anchorMin = oldRectTrans.anchorMin;
            newRectTrans.pivot = oldRectTrans.pivot;
            newRectTrans.localScale = oldRectTrans.localScale;
            newRectTrans.sizeDelta = oldRectTrans.sizeDelta;
        }
        


        private static GameObject CreatePrefabs(string path)
        {
            GameObject instance;
            var prefabPath = "Assets/_Packages/ATools/ToolsUI/Prefabs/" + path + ".prefab";
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            
            if (prefab == null) return null;

            if (Selection.transforms.Length > 0)
            {
                instance =
                    PrefabUtility.InstantiatePrefab(prefab, Selection.transforms[0]) as GameObject;
            }
            else
            {
                PrefabStage prefabStage = PrefabStageUtility.GetCurrentPrefabStage();
            
                if (prefabStage != null)
                {
                    instance = PrefabUtility.InstantiatePrefab(prefab, prefabStage.prefabContentsRoot.transform) as GameObject;
                }
                else
                {
                    instance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
                }
            }
            
            // 注册创建的预制体，以便能够撤消和重做操作
            Undo.RegisterCreatedObjectUndo(instance, "Create " + instance.name);

            Selection.activeObject = instance;

            return instance;
        }
    }
    
