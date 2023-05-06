using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tyrant.Core;
using Tyrant.GameCore;

namespace Tyrant.ComponentsEditorForUnity
{
    public class DesignerDataToolData
    {
        #region init
        private const string _xmlFilePath = "ProjectSettings";
        private const string _xmlFileName = "Tyrant Designer Data Tool.xml";
        private static string _xmlFileAbsPath = "";
        public static string GetXmlFileAbsPath()
        {
            if (string.IsNullOrEmpty(_xmlFileAbsPath))
            {
                _xmlFileAbsPath = Utility.GetAbsolutePath(Environment.CurrentDirectory.AppendDirectorySeparatorIfNotEndWith(), _xmlFilePath.AppendDirectorySeparatorIfNotEndWith()) + _xmlFileName;
            }
            return _xmlFileAbsPath;
        }

        private static DesignerDataToolData _instance;
        public static DesignerDataToolData Instance
        {
            get
            {
                if (_instance == null)
                {
                    var absFilePath = GetXmlFileAbsPath();
                    if (File.Exists(absFilePath))
                    {
                        try
                        {
                            _instance = XmlConfig<DesignerDataToolData>.Deserialize(absFilePath);
                        }
                        catch (Exception ex)
                        {
                            Debug.OutputException(ex);
                        }
                    }
                    if (_instance == null)
                    {
                        _instance = new DesignerDataToolData();
                    }
                }

                return _instance;
            }
        }

        public void Save()
        {
            if (_instance == null)
            {
                Debug.OutputError("DesignerDataToolData  Save() _instance == null");
                return;
            }
            //CheckOutputDirectory();

            var filePath = GetXmlFileAbsPath();
            XmlConfig<DesignerDataToolData>.Serialize(filePath, _instance);
            _instance = null;
        }

        //private void CheckOutputDirectory()
        //{//去掉重复地址path
        //    for (int i = 0; i < OutputDirectoryLst.Count; i++)
        //    {
        //        var iItem = OutputDirectoryLst[i];
        //        if (!string.IsNullOrEmpty(iItem))
        //        {
        //            for (int j = 0; j < OutputDirectoryLst.Count; j++)
        //            {
        //                if (OutputDirectoryLst[j] == iItem && j != i)
        //                {
        //                    OutputDirectoryLst[j] = "";
        //                }
        //            }
        //        }
        //    }
        //}
        #endregion

        /// <summary>
        /// excel生成的gzg文件所在文件夹
        /// </summary>
        public string ExcelFilesRootPath = null;
        /// <summary>
        /// 输出文件夹目录 多个
        /// </summary>
        public string OutputDirectory = null;
        /// <summary>
        /// 运行时生成
        /// </summary>
        public bool BuildOnRuntimeInitialize = false;
    }
}