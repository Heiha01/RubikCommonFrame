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

        private GameObject _root;//ѡ�����Ϸ����

        private List<UIBehaviour> _uiComponentList = new List<UIBehaviour>();//UI����б�

        private List<Component> _uiObjectList = new List<Component>();

        private float _viewWidth;//��ͼ���

        private float _viewHeight;//��ͼ�߶�

        private Vector2 _scrollComponentPos;
        private Vector2 _scrollObjectPos;
        private Vector2 _scrollTextPos;

        private int _selectedBarNum;

        private bool _isPanel = true;

        private SerializedObject _serializedObject;

        #region ���ɴ�������ı���

        private StringBuilder _codeStateText = new StringBuilder();
        private StringBuilder _codeEventText = new StringBuilder();
        private StringBuilder _codeAssignText = new StringBuilder();
        private StringBuilder _codeText = new StringBuilder();

        private Dictionary<string, object> _variableNameDic = new Dictionary<string, object>();
        private Dictionary<string, int> __variableIndexDic = new Dictionary<string, int>();//�ظ��ı���

        private int _variableIndex = 0;//�������

        private Dictionary<string,bool> _selectEventComponent = new Dictionary<string,bool>();

        private string _className;

        private Type _scriptType;




        #endregion

        #region ��ȡ��ͬ�Ĵ���

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

        #region ����ҳ��
        private void OnEnable()
        {
            _serializedObject = new SerializedObject(this);
        }

        private void OnGUI()
        {
            _serializedObject.Update();

            _viewWidth = EditorGUIUtility.currentViewWidth;
            _viewHeight = CodeWindow.position.height;

            using (new EditorGUILayout.HorizontalScope())//���Զ���༭���ű��д���ˮƽ������
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
        /// ���� ѡ��Ҫ������UI
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

                EditorGUILayout.LabelField("ѡ����Ҫ�����UI:", GUILayout.Width(_viewWidth * 0.125f));
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
        /// ���� ����UI�ؼ�
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

                if (GUILayout.Button("����UI�ؼ�", GUILayout.Width(_viewWidth*0.25f)))
                {
                    FindAllComponents();
                }

                if (GUILayout.Button("����ؼ�"))
                {
                    _uiComponentList.Clear();
                }

                if (GUILayout.Button("�������"))
                {
                    _uiObjectList.Clear();
                }
            }
        }

        /// <summary>
        /// ���� �ؼ��б�
        /// </summary>
        private void DrawComponentList()
        {
            EditorGUILayout.Space();

            ReorderableListGUI.Title("UI�ؼ�");
            _scrollComponentPos = EditorGUILayout.BeginScrollView(_scrollComponentPos);
            ReorderableListGUI.ListField<UIBehaviour>(_uiComponentList, DrawComponent);
            EditorGUILayout.EndScrollView();
        }

        /// <summary>
        /// ���� ����ui gameobject,����ĳЩ�ڵ�Ҫ�����²��������ʾ
        /// </summary>
        private void DrawCustomObjectList()
        {
            EditorGUILayout.Space();

            ReorderableListGUI.Title("����UI����");
            _scrollObjectPos = EditorGUILayout.BeginScrollView(_scrollObjectPos);
            ReorderableListGUI.ListField<Component>(_uiObjectList, DrawCustomObject);
            EditorGUILayout.EndScrollView();
        }

        private void FindAllComponents()
        {
            if (_root == null)
            {
                Debug.LogWarning("����ѡ��һ��UI����!");
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
        /// ����UI
        /// </summary>
        /// unity���õ�.AddListenerֻ��ע��UnityAction����ӷǳ־ü�����,���ϣ����inspector������ί�У�ֻ��ʹ��UnityAction�������޷����л�
        /// <param name="parent">���ڵ�</param>
        /// <param name="callback">�ص�</param>
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

                EditorGUILayout.LabelField("��������:");
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
            _isPanel = GUILayout.Toggle(_isPanel, "�̳�PanelBase");
            EditorGUILayout.Space();


            if (GUILayout.Button("��������", GUILayout.Width(_viewWidth / 6f)))
            {
                BuildStatementCode();
            }

            EditorGUILayout.Space();
            using (EditorGUILayout.VerticalScope vScope = new EditorGUILayout.VerticalScope())
            {
                GUI.backgroundColor = Color.white;
                GUI.Box(vScope.rect, "");

                EditorGUILayout.LabelField("ѡ����Ҫע���¼��ص��Ŀؼ�:");
                DrawEventWidget();

                EditorGUILayout.Space();
                if (GUILayout.Button("ע���¼�", GUILayout.Width(_viewWidth / 6f)))
                {
                    BuildEventCode();
                }
            }

            EditorGUILayout.Space();
            using (EditorGUILayout.HorizontalScope hScope = new EditorGUILayout.HorizontalScope())
            {

                if (GUILayout.Button("���ƴ���"))
                {
                    TextEditor p = new TextEditor();
                    _codeText = new StringBuilder(_codeStateText.ToString());
                    _codeText.Append(_codeAssignText);
                    _codeText.Append(_codeEventText);
                    p.text = _codeText.ToString();
                    p.OnFocus();
                    p.Copy();

                    EditorUtility.DisplayDialog("��ʾ", "���븴�Ƴɹ�", "OK");
                }

                if (GUILayout.Button("���ɽű�"))
                {
                    CreateCsUIScript();
                }
            }

            EditorGUILayout.Space();
            using (EditorGUILayout.HorizontalScope hScope = new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("��������", GUILayout.Width(_viewWidth*0.25f), GUILayout.Height(70)))
                {
                    if (_root == null)
                    {
                        EditorUtility.DisplayDialog("��ʾ", "����ѡ��һ��UI����!", "OK");
                        return;
                    }
                    BuildStatementCode();
                    BuildEventCode();
                    CreateCsUIScript();
                }

                if (GUILayout.Button("���ؽű����", GUILayout.Width(_viewWidth*0.25f), GUILayout.Height(70)))
                {
                    AddScriptComponent();
                }

            }

            DrawPreviewText();
        }

        #endregion

        #region ���ɴ���

        private string BuildStatementCode()
        {
            __variableIndexDic.Clear();
            _variableNameDic.Clear();

            _codeStateText = null;
            _codeStateText = new StringBuilder();

            _codeStateText.Append(CodeConfig.statementRegion);


            //�ؼ��б�
            for (int i = 0; i < _uiComponentList.Count; i++)
            {
                if (_uiComponentList[i] == null) continue;
                AddVariableCode(_uiComponentList[i], _uiComponentList[i].name);
            }

            //���������б�Ŀǰ����GameObject
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
        ///  ���ɸ�ֵ����
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="name"></param>
        private void AddVariableCode(object obj, string name)
        {
            string typeName = obj.GetType().Name;
            string variableName = string.Format("_{0}{1}", typeName.ToLower(), name.CapitalizeFirstLetter());
            variableName = variableName.Replace(" ", ""); //�����пո�����
            //��������
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
                //EditorUtility.DisplayDialog("��ʾ", variableName + "�����ظ�", "OK");
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
                //ɸѡ��ǰUI���¼��ؼ�
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

                //����toggle,ע�ⲻ�ܱ���dic��ͬʱ��ֵ
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
        /// ����ע��ؼ��¼��Ĵ���
        /// </summary>
        /// <returns></returns>
        private string BuildEventCode()
        {
            _codeEventText = null;
            _codeEventText = new StringBuilder();

            StringBuilder callbackText = new StringBuilder();

            _codeEventText.Append(eventRegion);
            _codeEventText.AppendFormat(methodStartFmt, "AddEvent");

            bool hasEventWidget = false; //��ʶ�Ƿ��пؼ�ע�����¼�
            for (int i = 0; i < _uiComponentList.Count; i++)
            {
                if (_uiComponentList[i] == null) continue;

                //�޳������¼��������¼���δ��ѡtoggle�Ŀؼ�
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
        /// ����C# UI�ű�
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
            Debug.Log("�ű����ɳɹ�,����·��Ϊ:" + componentsPath);
            // EditorPrefs.SetString("create_script_folder", componentsPath);
        }

        /// <summary>
        /// �ڸ������Ϲ������ɵĽű�(����̳�monobehavior)
        /// </summary>
        private void AddScriptComponent()
        {
            if (EditorApplication.isCompiling)
            {
                EditorUtility.DisplayDialog("����", "��ȴ��༭����ɱ�����ִ�д˲���", "OK");
                return;
            }

            if (_root == null)
            {
                EditorUtility.DisplayDialog("����", "���Ȱ�˳������UI�ű���ִ�д˲���", "OK");
                return;
            }

            if (string.IsNullOrEmpty(_className))
            {
                _className = _root.name;
            }

            //ͨ��Assembly-CSharp���򼯹��ؽű�
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
                EditorUtility.DisplayDialog("����", "����ʧ�ܣ����ȼ��ű��Ƿ���ȷ����", "OK");
                return;
            }

            var target = _root.GetComponent(_scriptType);
            if (target == null)
            {
                target = _root.AddComponent(_scriptType);
            }
        }

        /// <summary>
        /// ��ǰ�������ɵĴ���Ԥ��
        /// </summary>
        private void DrawPreviewText()
        {
            EditorGUILayout.Space();

            using (var ver = new EditorGUILayout.VerticalScope())
            {
                GUI.backgroundColor = Color.white;
                GUI.Box(ver.rect, "");

                EditorGUILayout.HelpBox("����Ԥ��:", MessageType.None);
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
                    //�ж��Ƿ���ڸ��ϣ����ϲ���Ҫ·��
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
