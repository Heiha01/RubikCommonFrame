using System;
using System.Collections.Generic;
using System.Text;
using Unity.VisualScripting.ReorderableList;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using Rubik_Tools;
using UnityEngine.UI;
using System.IO;
using System.Reflection;

namespace UIFrame.Editor
{
    public class GenerateScriptWindow : EditorWindow
    {
        private const string IconPath = "Assets/_Res/InApp/Sprites/AvatarBoy/0b1.png";
        private static GenerateScriptWindow codeWindow = null;
        private static GenerateScriptWindow CodeWindow
        {
            get
            {
                if (codeWindow == null)
                {
                    codeWindow = EditorWindow.GetWindow(typeof(GenerateScriptWindow)) as GenerateScriptWindow;
                }
                return codeWindow;
            }
        }

            [MenuItem("Tools/GenerateUIScript")]
        public static void OpenWindow()
        {
            Texture2D icon = (Texture2D)EditorGUIUtility.Load(IconPath);

            CodeWindow.titleContent = new GUIContent("QuickScript", icon);
            CodeWindow.Show();
        }

        private GameObject _root;//选择根游戏物体

        private List<UIBehaviour> _uiComponentList = new List<UIBehaviour>();//UI组件列表

        private List<Component> _uiObjectList = new List<Component>();

        private float _viewWidth;//视图宽度

        private float _viewHeight;//视图高度

        private Vector2 _scrollComponentPos;
        private Vector2 _scrollObjectPos;
        private Vector2 _scrollTextPos;

        private int _selectedBarNum;

        private bool _isPanel = true;

        private SerializedObject _serializedObject;

        #region 生成代码所需的变量

        private StringBuilder _codeStateText = new StringBuilder();
        private StringBuilder _codeEventText = new StringBuilder();
        private StringBuilder _codeAssignText = new StringBuilder();
        private StringBuilder _codeText = new StringBuilder();

        private Dictionary<string, object> _variableNameDic = new Dictionary<string, object>();
        private Dictionary<string, int> __variableIndexDic = new Dictionary<string, int>();//重复的变量

        private int _variableIndex = 0;//变量编号

        private Dictionary<string,bool> _selectEventComponent = new Dictionary<string,bool>();

        private string _className;

        private Type _scriptType;




        #endregion

        #region 获取不同的代码

        private string regionStartFmt => CodeConfig.regionStartFmt;
        private string regionEnd => CodeConfig.regionEnd;

        private string statementRegion=>CodeConfig.statementRegion;

        private string assignRegion=>CodeConfig.assignRegion;

        private string eventRegion => CodeConfig.eventRegion;

        private string methodStartFmt => CodeConfig.methodStartFmt;

        private string methodEndFmt => CodeConfig.methodEnd;

        private string assignCodeFmt=> CodeConfig.assignCodeFmt;

        private string assignGameObjectCodeFmt => CodeConfig.assignGameObjectCodeFmt;
        private string assignRootCodeFmt => CodeConfig.assignRootCodeFmt;
        private string onClickSerilCode => CodeConfig.onClickSerilCode;
        private string onValueChangeSerilCode => CodeConfig.onValueChangeSerilCode;
        private string btnCallbackSerilCode => CodeConfig.btnCallbackSerilCode;
        private string eventCallbackSerilCode => CodeConfig.eventCallbackSerilCode;
        #endregion

        #region 绘制页面
        private void OnEnable()
        {
            _serializedObject = new SerializedObject(this);
        }

        private void OnGUI()
        {
            _serializedObject.Update();

            _viewWidth = EditorGUIUtility.currentViewWidth;
            _viewHeight = CodeWindow.position.height;

            using (new EditorGUILayout.HorizontalScope())//在自定义编辑器脚本中创建水平布局组
            {
                using (EditorGUILayout.VerticalScope leftScope
                    = new EditorGUILayout.VerticalScope(GUILayout.Width(_viewWidth * 0.5f)))
                {
                    GUI.backgroundColor = Color.white;
                    Rect rect = leftScope.rect;
                    rect.height = _viewHeight;
                    GUI.Box(rect, "");

                    DrawSelectUI();
                    DrawFindComponent();
                    DrawComponentList();
                    DrawCustomObjectList();
                }

                using (new EditorGUILayout.VerticalScope(GUILayout.Width(_viewWidth*0.5f)))
                {
                    DrawCodeGenerateTitle();
                    DrawCodeGenerateToolBar();
                }
            }
        }

        /// <summary>
        /// 绘制 选择要分析的UI
        /// </summary>
        private void DrawSelectUI()
        {
            EditorGUILayout.Space();
            using (EditorGUILayout.HorizontalScope hScope = new EditorGUILayout.HorizontalScope())
            {
                GUI.backgroundColor = Color.white;
                Rect rect = hScope.rect;
                rect.height = EditorGUIUtility.singleLineHeight;
                GUI.Box(rect, "");

                EditorGUILayout.LabelField("选择需要处理的UI:", GUILayout.Width(_viewWidth * 0.125f));
                GameObject lastRoot = _root;
                _root = EditorGUILayout.ObjectField(_root, typeof(GameObject), true) as GameObject;

                if ((lastRoot == null && _root != null) || (lastRoot != null && lastRoot != _root))
                {
                    _uiComponentList.Clear();
                    _uiObjectList.Clear();
                    FindAllComponents();
                }
            }
        }

        /// <summary>
        /// 绘制 查找UI控件
        /// </summary>
        private void DrawFindComponent()
        {
            EditorGUILayout.Space();
            using (EditorGUILayout.HorizontalScope hScope = new EditorGUILayout.HorizontalScope())
            {
                GUI.backgroundColor = Color.white;
                Rect rect = hScope.rect;
                rect.height = EditorGUIUtility.singleLineHeight;
                GUI.Box(rect, "");

                if (GUILayout.Button("查找UI控件", GUILayout.Width(_viewWidth*0.25f)))
                {
                    FindAllComponents();
                }

                if (GUILayout.Button("清除控件"))
                {
                    _uiComponentList.Clear();
                }

                if (GUILayout.Button("清除其他"))
                {
                    _uiObjectList.Clear();
                }
            }
        }

        /// <summary>
        /// 绘制 控件列表
        /// </summary>
        private void DrawComponentList()
        {
            EditorGUILayout.Space();

            ReorderableListGUI.Title("UI控件");
            _scrollComponentPos = EditorGUILayout.BeginScrollView(_scrollComponentPos);
            ReorderableListGUI.ListField<UIBehaviour>(_uiComponentList, DrawComponent);
            EditorGUILayout.EndScrollView();
        }

        /// <summary>
        /// 绘制 其他ui gameobject,比如某些节点要控制下层的隐藏显示
        /// </summary>
        private void DrawCustomObjectList()
        {
            EditorGUILayout.Space();

            ReorderableListGUI.Title("其他UI对象");
            _scrollObjectPos = EditorGUILayout.BeginScrollView(_scrollObjectPos);
            ReorderableListGUI.ListField<Component>(_uiObjectList, DrawCustomObject);
            EditorGUILayout.EndScrollView();
        }

        private void FindAllComponents()
        {
            if (_root == null)
            {
                Debug.LogWarning("请先选择一个UI物体!");
                return;
            }

            TraversalUI(_root.transform, (tran) =>
            {
                UIBehaviour[] widgets = tran.GetComponents<UIBehaviour>();
                for (int i = 0; i < widgets.Length; i++)
                {
                    var widget = widgets[i];
                    if (widget != null && !_uiComponentList.Contains(widget))
                    {
                        _uiComponentList.Add(widget);
                    }
                }
            });
        }


        /// <summary>
        /// 遍历UI
        /// </summary>
        /// unity内置的.AddListener只能注册UnityAction来添加非持久监听器,如果希望在inspector面板访问委托，只能使用UnityAction，否则无法序列化
        /// <param name="parent">父节点</param>
        /// <param name="callback">回调</param>
        public void TraversalUI(Transform parent, UnityAction<Transform> callback)
        {
            if (callback != null)
                callback(parent);

            if (parent.childCount >= 0)
            {
                for (int i = 0; i < parent.childCount; i++)
                {
                    Transform child = parent.GetChild(i);

                    TraversalUI(child, callback);
                }
            }
        }

        private UIBehaviour DrawComponent(Rect position, UIBehaviour item)
        {
            item = (UIBehaviour)EditorGUI.ObjectField(position, item, typeof(UIBehaviour), true);
            return item;
        }

        private Component DrawCustomObject(Rect position, Component item)
        {
            item = (Component)EditorGUI.ObjectField(position, item, typeof(Component), true);
            return item;
        }

        private void DrawCodeGenerateTitle()
        {
            EditorGUILayout.Space();
            using (var hScope =
                   new EditorGUILayout.HorizontalScope(GUILayout.Height(EditorGUIUtility.singleLineHeight)))
            {
                GUI.backgroundColor = Color.white;
                Rect rect = hScope.rect;
                GUI.Box(rect, "");

                EditorGUILayout.LabelField("代码生成:");
            }
        }

        private void DrawCodeGenerateToolBar()
        {
            EditorGUILayout.Space();

            _selectedBarNum = GUILayout.Toolbar(_selectedBarNum, new string[] { "C#" });

            switch (_selectedBarNum)
            {
                case 0:
                    DrawCsPage();
                    break;
                default:
                    break;
            }
        }

        private void DrawCsPage()
        {
            EditorGUILayout.Space();
            _isPanel = GUILayout.Toggle(_isPanel, "继承PanelBase");
            EditorGUILayout.Space();


            if (GUILayout.Button("变量声明", GUILayout.Width(_viewWidth / 6f)))
            {
                BuildStatementCode();
            }

            EditorGUILayout.Space();
            using (EditorGUILayout.VerticalScope vScope = new EditorGUILayout.VerticalScope())
            {
                GUI.backgroundColor = Color.white;
                GUI.Box(vScope.rect, "");

                EditorGUILayout.LabelField("选择需要注册事件回调的控件:");
                DrawEventWidget();

                EditorGUILayout.Space();
                if (GUILayout.Button("注册事件", GUILayout.Width(_viewWidth / 6f)))
                {
                    BuildEventCode();
                }
            }

            EditorGUILayout.Space();
            using (EditorGUILayout.HorizontalScope hScope = new EditorGUILayout.HorizontalScope())
            {

                if (GUILayout.Button("复制代码"))
                {
                    TextEditor p = new TextEditor();
                    _codeText = new StringBuilder(_codeStateText.ToString());
                    _codeText.Append(_codeAssignText);
                    _codeText.Append(_codeEventText);
                    p.text = _codeText.ToString();
                    p.OnFocus();
                    p.Copy();

                    EditorUtility.DisplayDialog("提示", "代码复制成功", "OK");
                }

                if (GUILayout.Button("生成脚本"))
                {
                    CreateCsUIScript();
                }
            }

            EditorGUILayout.Space();
            using (EditorGUILayout.HorizontalScope hScope = new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("快速生成", GUILayout.Width(_viewWidth*0.25f), GUILayout.Height(70)))
                {
                    if (_root == null)
                    {
                        EditorUtility.DisplayDialog("提示", "请先选择一个UI物体!", "OK");
                        return;
                    }
                    BuildStatementCode();
                    BuildEventCode();
                    CreateCsUIScript();
                }

                if (GUILayout.Button("挂载脚本组件", GUILayout.Width(_viewWidth*0.25f), GUILayout.Height(70)))
                {
                    AddScriptComponent();
                }

            }

            DrawPreviewText();
        }

        #endregion

        #region 生成代码

        private string BuildStatementCode()
        {
            __variableIndexDic.Clear();
            _variableNameDic.Clear();

            _codeStateText = null;
            _codeStateText = new StringBuilder();

            _codeStateText.Append(CodeConfig.statementRegion);


            //控件列表
            for (int i = 0; i < _uiComponentList.Count; i++)
            {
                if (_uiComponentList[i] == null) continue;
                AddVariableCode(_uiComponentList[i], _uiComponentList[i].name);
            }

            //其他对象列表，目前都是GameObject
            for (int i = 0; i < _uiObjectList.Count; i++)
            {
                if (_uiObjectList[i] == null) continue;
                //++variableNum;
                AddVariableCode(_uiObjectList[i], _uiObjectList[i].name);
            }

            _codeStateText.Append(CodeConfig.regionEnd);
            //Debug.Log(codeStateText);
            return _codeStateText.ToString();
        }


        /// <summary>
        ///  生成赋值代码
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="name"></param>
        private void AddVariableCode(object obj, string name)
        {
            string typeName = obj.GetType().Name;
            string variableName = string.Format("_{0}{1}", typeName.ToLower(), name.CapitalizeFirstLetter());
            variableName = variableName.Replace(" ", ""); //命名有空格的情况
            //重名处理
            var firstLetterVariableName = variableName[1..].CapitalizeFirstLetter();

            if (_variableNameDic.ContainsKey(firstLetterVariableName))
            {
                int variableNum = 1;

                if (__variableIndexDic.ContainsKey(firstLetterVariableName))
                {
                    variableNum = __variableIndexDic[firstLetterVariableName] + 1;
                    __variableIndexDic[firstLetterVariableName] = variableNum;
                }
                else
                {
                    __variableIndexDic.Add(firstLetterVariableName, variableNum);
                }
                //EditorUtility.DisplayDialog("提示", variableName + "命名重复", "OK");
                firstLetterVariableName += variableNum;
                variableName += variableNum;
            }

            _variableNameDic.Add(firstLetterVariableName, obj);

            _codeStateText.AppendFormat(CodeConfig.serilStateCodeFmt, typeName, variableName,
                firstLetterVariableName, BuildAssignmentCode(obj));
        }

        private void DrawEventWidget()
        {
            using (EditorGUILayout.HorizontalScope hScope = new EditorGUILayout.HorizontalScope())
            {
                //筛选当前UI的事件控件
                foreach (var elem in Enum.GetValues(typeof(CodeConfig.EventComponentType)))
                {
                    for (int i = 0; i < _uiComponentList.Count; i++)
                    {
                        if (_uiComponentList[i] == null) continue;

                        Type type = _uiComponentList[i].GetType();
                        if (type == null)
                        {
                            Debug.LogError("BuildUICode type error !");
                            continue;
                        }

                        if (type.Name == elem.ToString() && !_selectEventComponent.ContainsKey(type.Name))
                        {
                            _selectEventComponent.Add(type.Name, true);
                        }
                    }
                }

                //绘制toggle,注意不能遍历dic的同时赋值
                List<string> list = new List<string>(_selectEventComponent.Keys);
                foreach (string wedagetName in list)
                {
                    _selectEventComponent[wedagetName] = EditorGUILayout.ToggleLeft(wedagetName,
                        _selectEventComponent[wedagetName],
                        GUILayout.Width(_viewWidth / 8f));
                }
            }
        }

        /// <summary>
        /// 构建注册控件事件的代码
        /// </summary>
        /// <returns></returns>
        private string BuildEventCode()
        {
            _codeEventText = null;
            _codeEventText = new StringBuilder();

            StringBuilder callbackText = new StringBuilder();

            _codeEventText.Append(eventRegion);
            _codeEventText.AppendFormat(methodStartFmt, "AddEvent");

            bool hasEventWidget = false; //标识是否有控件注册了事件
            for (int i = 0; i < _uiComponentList.Count; i++)
            {
                if (_uiComponentList[i] == null) continue;

                //剔除不是事件或者是事件但未勾选toggle的控件
                string typeName = _uiComponentList[i].GetType().Name;
                if (!_selectEventComponent.ContainsKey(typeName) || !_selectEventComponent[typeName])
                {
                    continue;
                }

                foreach (var vName in _variableNameDic.Keys)
                {
                    if (_uiComponentList[i].Equals(_variableNameDic[vName]))
                    {
                        string variableName = vName;
                        if (!string.IsNullOrEmpty(variableName))
                        {
                            string methodName = variableName.Substring(variableName.IndexOf('_') + 1);
                            if (_uiComponentList[i] is Button)
                            {
                                string onClickStr = string.Format(onClickSerilCode, variableName, methodName);
                                if (hasEventWidget)
                                {
                                    string str = _codeEventText.ToString();
                                    _codeEventText.Insert(str.LastIndexOf(';') + 1, "\n" + onClickStr);
                                }
                                else
                                {
                                    _codeEventText.Append(onClickStr);
                                }

                                callbackText.AppendFormat(btnCallbackSerilCode, methodName, _root.name);
                                hasEventWidget = true;
                            }
                            else
                            {
                                string addEventStr = string.Format(onValueChangeSerilCode, variableName, methodName);
                                if (hasEventWidget)
                                {
                                    _codeEventText.Insert(_codeEventText.ToString().LastIndexOf(';') + 1, addEventStr);
                                }
                                else
                                {
                                    _codeEventText.Append(addEventStr);
                                }

                                string paramType = "";
                                foreach (string widgetType in CodeConfig.EventParamDic.Keys)
                                {
                                    if (typeName == widgetType)
                                    {
                                        paramType = CodeConfig.EventParamDic[widgetType];
                                        break;
                                    }
                                }

                                if (!string.IsNullOrEmpty(paramType))
                                {
                                    callbackText.AppendFormat(eventCallbackSerilCode, methodName, paramType, _root.name);
                                }

                                hasEventWidget = true;
                            }
                        }

                        break;
                    }
                }
            }

            string codeStr = _codeEventText.ToString();
            if (hasEventWidget)
            {
                _codeEventText.Insert(codeStr.LastIndexOf(';') + 1, methodEndFmt);
            }
            else
            {
                _codeEventText.Append(methodEndFmt);
            }

            _codeEventText.Append(callbackText);
            _codeEventText.Append(regionEnd);
            return _codeEventText.ToString();
        }

        /// <summary>
        /// 生成C# UI脚本
        /// </summary>
        private void CreateCsUIScript()
        {
            // string path = EditorPrefs.GetString("create_script_folder", "Assets/_Scripts/Runtime/UIModule");
            string componentsPath =
                EditorUtility.SaveFilePanel("Create Script", "Assets/_Scripts/Runtime/UIModule", _root.name + "Components" + ".cs", "cs");
            string panelPath = componentsPath.Replace("Components", "");

            if (string.IsNullOrEmpty(componentsPath)) return;

            int index = componentsPath.LastIndexOf('/');
            string componentsName = componentsPath.Substring(index + 1, componentsPath.LastIndexOf('.') - index - 1);

            StringBuilder scriptBuilder = new StringBuilder();
            scriptBuilder.Append(CodeConfig.codeAnnotation);
            scriptBuilder.Append(CodeConfig.usingNamespace);
            if (_isPanel)
            {
                scriptBuilder.AppendFormat(CodeConfig.componentsPanelStart, componentsName);
            }
            else
            {
                scriptBuilder.AppendFormat(CodeConfig.componentsMonoStart, componentsName);
            }

            scriptBuilder.Append(_codeStateText);
            scriptBuilder.Append(_codeAssignText);
            scriptBuilder.Append(_codeEventText);
            if (_isPanel)
            {
                scriptBuilder.AppendFormat(CodeConfig.PanelOverride, _root.name);
            }
            else
            {
                scriptBuilder.AppendFormat(CodeConfig.MonoOverride);
            }
            scriptBuilder.Append(CodeConfig.classEnd);

            File.WriteAllText(componentsPath, scriptBuilder.ToString(), new UTF8Encoding(false));

            _className = panelPath.Substring(index + 1, panelPath.LastIndexOf('.') - index - 1);

            if (!File.Exists(panelPath))
            {
                var panelScriptBuilder = new StringBuilder();
                panelScriptBuilder.Append(CodeConfig.usingNamespace);
                panelScriptBuilder.AppendFormat(CodeConfig.classStart, _className, componentsName);
                panelScriptBuilder.Append(CodeConfig.classEnd);
                File.WriteAllText(panelPath, panelScriptBuilder.ToString(), new UTF8Encoding(false));
            }

            AssetDatabase.Refresh();
            Debug.Log("脚本生成成功,生成路径为:" + componentsPath);
            // EditorPrefs.SetString("create_script_folder", componentsPath);
        }

        /// <summary>
        /// 在根物体上挂载生成的脚本(必须继承monobehavior)
        /// </summary>
        private void AddScriptComponent()
        {
            if (EditorApplication.isCompiling)
            {
                EditorUtility.DisplayDialog("警告", "请等待编辑器完成编译再执行此操作", "OK");
                return;
            }

            if (_root == null)
            {
                EditorUtility.DisplayDialog("警告", "请先按顺序生成UI脚本再执行此操作", "OK");
                return;
            }

            if (string.IsNullOrEmpty(_className))
            {
                _className = _root.name;
            }

            //通过Assembly-CSharp程序集挂载脚本
            Assembly[] AssbyCustmList = System.AppDomain.CurrentDomain.GetAssemblies();
            Assembly asCSharp = null;
            for (int i = 0; i < AssbyCustmList.Length; i++)
            {
                string assbyName = AssbyCustmList[i].GetName().Name;
                if (assbyName == "Assembly-CSharp")
                {
                    asCSharp = AssbyCustmList[i];
                    break;
                }
            }

            _scriptType = asCSharp.GetType(_className);
            if (_scriptType == null)
            {
                EditorUtility.DisplayDialog("警告", "挂载失败，请先检查脚本是否正确生成", "OK");
                return;
            }

            var target = _root.GetComponent(_scriptType);
            if (target == null)
            {
                target = _root.AddComponent(_scriptType);
            }
        }

        /// <summary>
        /// 当前操作生成的代码预览
        /// </summary>
        private void DrawPreviewText()
        {
            EditorGUILayout.Space();

            using (var ver = new EditorGUILayout.VerticalScope())
            {
                GUI.backgroundColor = Color.white;
                GUI.Box(ver.rect, "");

                EditorGUILayout.HelpBox("代码预览:", MessageType.None);
                using (var scr = new EditorGUILayout.ScrollViewScope(_scrollTextPos))
                {
                    _scrollTextPos = scr.scrollPosition;

                    if (_codeStateText != null && !string.IsNullOrEmpty(_codeStateText.ToString()) && _selectedBarNum == 0)
                    {
                        //GUILayout.TextArea(codeStateText.ToString());
                        GUILayout.Label(_codeStateText.ToString());
                    }

                    if (_codeAssignText != null && !string.IsNullOrEmpty(_codeAssignText.ToString()))
                    {
                        GUILayout.Label(_codeAssignText.ToString());
                    }

                    if (_codeEventText != null && !string.IsNullOrEmpty(_codeEventText.ToString()))
                    {
                        //GUILayout.TextArea(codeEventText.ToString());
                        GUILayout.Label(_codeEventText.ToString());
                    }
                }
            }
        }

        private string BuildAssignmentCode(object obj)
        {
            var allPath = GetChildrenPaths(_root);

            string path = "";
            bool isRootComponent = false;
            foreach (var tran in allPath.Keys)
            {
                if (tran == null) continue;

                UIBehaviour behav = obj as UIBehaviour;

                if (behav != null)
                {
                    //判断是否挂在根上，根上不需要路径
                    isRootComponent = behav.gameObject == _root;
                    if (isRootComponent) break;

                    if (behav.gameObject == tran.gameObject)
                    {
                        path = allPath[tran];
                        break;
                    }
                }
                else
                {
                    Component comp = obj as Component;

                    if (comp != null)
                    {
                        if (tran.name == comp.name)
                        {
                            path = allPath[tran];
                            break;
                        }
                    }
                }
            }

            if (obj is GameObject)
            {
                return string.Format(assignGameObjectCodeFmt, path);
            }

            return isRootComponent
                ? string.Format(assignRootCodeFmt, obj.GetType().Name)
                : string.Format(assignCodeFmt, path, obj.GetType().Name);
        }

        private Dictionary<Transform, string> GetChildrenPaths(GameObject rootGo)
        {
            Dictionary<Transform, string> pathDic = new Dictionary<Transform, string>();
            string path = string.Empty;
            Transform[] tfArray = rootGo.GetComponentsInChildren<Transform>(true);
            for (int i = 0; i < tfArray.Length; i++)
            {
                Transform node = tfArray[i];

                string str = node.name;
                while (node.parent != null && node.gameObject != rootGo && node.parent.gameObject != rootGo)
                {
                    str = string.Format("{0}/{1}", node.parent.name, str);
                    node = node.parent;
                }

                path += string.Format("{0}\n", str);

                if (!pathDic.ContainsKey(tfArray[i]))
                {
                    pathDic.Add(tfArray[i], str);
                }
            }
            //Debug.Log(path);

            return pathDic;
        }

        #endregion


    }
}
