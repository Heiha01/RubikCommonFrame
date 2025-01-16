using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System.Data;
using System.Text;
using Unity.VisualScripting;
using ExcelDataReader;

public class ExcelToJsonEditor : EditorWindow
{
    private const string TypeName = "##type";
    private const string SummaryName = "##summary";


    private string folderPath = "Assets/Res/Excle";
    private const string csFolderPath = "Assets/Res";
    private const string jsonFilePath = "Assets";
    private List<string> excelFiles = new List<string>();
    private List<Object> addNewExcelSpaceList = new List<Object>();
    private Object addNewExcelSpace;
    private Object OutputPath_json;
    private Object OutputPath_CS;
    private int addExcelNum =0;

    [MenuItem("ZYB/Excel To JSON Converter")]
    public static void ShowWindow()
    {
        GetWindow<ExcelToJsonEditor>("Excel To JSON Converter");
    }

    private void OnGUI()
    {
        GUILayout.Label("OutPut Path", EditorStyles.boldLabel);
        OutputPath_json = (Object)EditorGUILayout.ObjectField("Output path of json file", OutputPath_json, typeof(Object), true);
        OutputPath_CS = (Object)EditorGUILayout.ObjectField("Output path of CS file", OutputPath_CS, typeof(Object), true);

        EditorGUILayout.Space(10);
        GUILayout.Label("Excel to JSON Converter", EditorStyles.boldLabel);

        // 文件夹路径输入
        GUILayout.BeginHorizontal();
        folderPath = EditorGUILayout.TextField("Folder Path", folderPath);
        if (GUILayout.Button("Load Excel Files"))
        {
            LoadExcelFiles();
        }
        GUILayout.EndHorizontal();
        for (int i = 0; i < excelFiles.Count; i++)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(excelFiles[i]);
            if (GUILayout.Button("Conver", GUILayout.Width(70)))
            {
                SingleConvertToJson(excelFiles[i]);
            }
            if (GUILayout.Button("Updata", GUILayout.Width(70)))
            {
                SingleConvertToJson(excelFiles[i]);
            }
            if (GUILayout.Button("Remove",GUILayout.Width(70)))
            {
                RemoveFile(excelFiles[i]);
            }
            GUILayout.EndHorizontal();
        }
        // 显示已加载的Excel文件
        EditorGUILayout.Space(10);
        GUILayout.BeginHorizontal(); // 开始水平布局
        GUILayout.FlexibleSpace(); // 添加灵活空间，使内容居中
        GUILayout.Label("ADD NEW EXCEL SPACE", EditorStyles.boldLabel);
        GUILayout.FlexibleSpace(); // 添加灵活空间，使内容居中
        GUILayout.EndHorizontal(); // 结束水平布局
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("AddNewSpace"))
        {
            addExcelNum++;
        }
        if (GUILayout.Button("Remove"))
        {

            if (addExcelNum>0)
            {
                addExcelNum--;
            }
            else
            {
                addExcelNum=0;
            }
        }
        GUILayout.EndHorizontal();
        DealSingleExcel();
        // 转换按钮
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Convert all to JSON"))
        {
            ConvertToJSON();
        }
        if (GUILayout.Button("Updata all to JSON"))
        {
            ConvertToJSON();
        }
        GUILayout.EndHorizontal();

    }

    private void LoadExcelFiles()
    {
        excelFiles.Clear();
        string[] files = Directory.GetFiles(folderPath, "*.xlsx", SearchOption.AllDirectories);
        foreach (var file in files)
        {
            excelFiles.Add(file);
        }
    }

    private void AddFile(string filePath)
    {
        if (File.Exists(filePath) && Path.GetExtension(filePath).ToLower() == ".xlsx")
        {
            if (!excelFiles.Contains(filePath))
            {
                excelFiles.Add(filePath);
            }
            else
            {
                Debug.LogWarning("File already exists in the list: " + filePath);
            }
        }
        else
        {
            Debug.LogError("Invalid file path or file is not an Excel file: " + filePath);
        }
    }

    private void RemoveFile(string filePath)
    {
        if (excelFiles.Contains(filePath))
        {
            excelFiles.Remove(filePath);
        }
    }

    private void ConvertToJSON()
    {
        foreach (var item in addNewExcelSpaceList)
        {
            var tempPath = item.GetObjectPath();
            if (item != null && !excelFiles.Contains(tempPath))
            {
                excelFiles.Add(tempPath);
            }
        }
        foreach (var filePath in excelFiles)
        {
            SingleConvertToJson(filePath);
            CreateExcelDataScript(filePath, GetCsFolderPath());
        }
    }

    private void SingleConvertToJson(string tempPath)
    {
        string _json = ExcelToJson(tempPath);
        var fileName = tempPath.GetFileNameWithoutExtension();
        var folderPath = GetJsonFolderPath();
        File.WriteAllText($"{folderPath}/{fileName}{SuffixEnum.json.GetSuffixStr()}", _json);
    }

    private string GetJsonFolderPath()
    {
        var tempJson = GetPath(OutputPath_json);
        if (tempJson.IsUnityNull())
        {
            return jsonFilePath;
        }
        else
        {
            return tempJson;
        }
    }
    private string GetCsFolderPath() {
        var tempCs = GetPath(OutputPath_CS);
        if (tempCs.IsUnityNull())
        {
            return csFolderPath;
        }
        else
        {
            return tempCs;
        }
    }

    private string GetPath(Object ob)
    {
        if (ob.IsUnityNull())
        {
            Debug.LogError($"{ob.name} is null");
            return null;
        }
        return AssetDatabase.GetAssetPath(ob);
    }


    private string ExcelToJson(string filePath)
    {
        using (var stream = File.Open(filePath, FileMode.Open, FileAccess.Read))
        {
            using (var reader = ExcelReaderFactory.CreateReader(stream))
            {
                var result = reader.AsDataSet(new ExcelDataSetConfiguration()
                {
                    ConfigureDataTable = (_) => new ExcelDataTableConfiguration() { UseHeaderRow = true }
                });

                StringBuilder sb = new StringBuilder();
                foreach (DataTable table in result.Tables)
                {
                    sb.AppendLine($"[");
                    int tempAdd = 0;
                    foreach (DataRow row in table.Rows)
                    {
                        if (tempAdd<2)
                        {
                            tempAdd++;
                            continue;
                        }
                        sb.AppendLine("    {");
                        for (int i = 1; i < table.Columns.Count; i++)
                        {
                            string columnName = table.Columns[i].ColumnName;
                            string cellValue = row[i].ToString();
                            sb.AppendLine($"      \"{columnName}\": \"{cellValue}\"{(i < table.Columns.Count - 1 ? "," : "")}");
                        }
                        sb.AppendLine("    }" + (row != table.Rows[table.Rows.Count - 1] ? "," : ""));
                    }
                    sb.AppendLine("  ]" + (table != result.Tables[result.Tables.Count - 1] ? "," : ""));
                }
                return sb.ToString();
            }
        }
    }

    private void DealSingleExcel()
    {
        addNewExcelSpaceList.Clear();
        for (int i = 0; i < addExcelNum; i++)
        {
            GUILayout.BeginHorizontal();
            addNewExcelSpace = (Object)EditorGUILayout.ObjectField("AddExcel", addNewExcelSpace, typeof(Object), true);
            addNewExcelSpaceList.Add(addNewExcelSpace);
            if (GUILayout.Button("Conver", GUILayout.Width(70)))
            {
                ConvertSingle(i);
            }
            if (GUILayout.Button("Updata", GUILayout.Width(70)))
            {
                ConvertSingle(i);
            }
            GUILayout.EndHorizontal();
        }
    }

    private void ConvertSingle(int tempIndex)
    {
        if (addNewExcelSpaceList[tempIndex] == null)
        {
            Debug.Log("Cannot be empty");
            return;
        }
        var convertItem = addNewExcelSpaceList[tempIndex];
        var tempPath = convertItem.GetObjectPath();

        SingleConvertToJson(tempPath);
        CreateExcelDataScript(tempPath, GetCsFolderPath());
    }

    public DataSet GetExcelDataSet(string filePath)
    {
        using (var stream = File.Open(filePath, FileMode.Open, FileAccess.Read))
        {
            using (var reader = ExcelReaderFactory.CreateReader(stream))
            {
                var result = reader.AsDataSet(new ExcelDataSetConfiguration()
                {
                    ConfigureDataTable = (_) => new ExcelDataTableConfiguration() { UseHeaderRow = true }
                });
                return result;
            }
        }
    }

    public void CreateExcelDataScript(string fileExcelPath, string csFolder)
    {
        var table = GetExcelDataSet(fileExcelPath).Tables;
        var ClassNamePre = fileExcelPath.GetFileNameWithoutExtension();
        csFolder.GetFile();
        // 文件头
        var content =@$"";

        for (int sheetIndex = 0; sheetIndex < table.Count; sheetIndex++)
        {
            DataTable sheet = table[sheetIndex];
            if (sheet == null)
            {
                Debug.LogError("Read error!");
                continue;
            }

            // 没有数据
            if (sheet.Rows.Count <= 1)
            {
                Debug.LogError("Data is empty!");
                continue;
            }

            var configType = sheet.TableName;

            // params
            List<string> typeList = new List<string>();
            List<string> paramList = new List<string>();
            List<string> summariesList = new List<string>();

            // rows
            int typeRow = -1;
            int summaryRow = -1;
            var csFile = $"{csFolder}/{ClassNamePre}_{configType}{SuffixEnum.cs.GetSuffixStr()}";
            content +=
@$"
    public class {$"{ClassNamePre}_{configType}"} :GameExcleInfoBase {{";
            for (int i = 0; i < sheet.Rows.Count; i++)
            {
                var header = sheet.Rows[i][0].ToString().ToLower();
                if (string.IsNullOrEmpty(header))
                    continue;
                if (!header.Contains("##"))
                    continue;
                typeRow = header.Equals(TypeName) ? i : typeRow;
                summaryRow = header.Equals(SummaryName) ? i : summaryRow;
            }
            if (typeRow == -1 || summaryRow == -1)
            {
                Debug.LogError("Row null!");
                continue;
            }

            for (int j = 2; j < sheet.Columns.Count; j++)
            {
                var typeValue = sheet.Rows[typeRow][j].ToString();
                var paramValue = sheet.Columns[j].ToString();
                if (string.IsNullOrEmpty(typeValue) || string.IsNullOrEmpty(paramValue))
                    continue;
                paramList.Add(paramValue.Replace("-", "_"));
                typeList.Add(typeValue);
                summariesList.Add(sheet.Rows[summaryRow][j].ToString());
            }

            for (int i = 0; i < typeList.Count; i++)
            {
                content += @$"
        /// <summary>
        /// {summariesList[i]}
        /// </summary>
        public {typeList[i]} {paramList[i]};";
            }

            content +=
@$"

    public override void InitInfo()
    {{

    }}
}}
";
            // 写入文件
            File.WriteAllText(csFile, content);
        }

        AssetDatabase.Refresh();
    }

}
