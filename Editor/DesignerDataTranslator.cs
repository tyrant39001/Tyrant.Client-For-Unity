using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
//using Tyrant.AddIn.Core;
using Tyrant.Core;
using Tyrant.DesignerDataTranslator.Properties;

namespace Tyrant.GameCore
{
    public class DesignerDataTranslator
    {
        #region 参数数据
        internal class CommandArgs
        {
            /// <summary>
            /// 源文件根目录
            /// </summary>
            public string ResourcePath = "";

            /// <summary>
            /// 输出根目录
            /// </summary>
            public string OutputRootPath = null;

            private string _extentionName = "tydc";

            /// <summary>
            /// 生成文件拓展名(默认后缀:tydc)
            /// </summary>
            public string ExtentionName
            {
                get
                {
                    if (string.IsNullOrEmpty(_extentionName))
                        _extentionName = "tydc";
                    return _extentionName;
                }

                set
                {
                    _extentionName = value;
                }
            }
        }

        #endregion

        //private static CommandArgs _commandArgs = null;//参数数据_可扩展

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sourcePath">Excel文件和其对应的配置文件(.gzg)所在的根目录</param>
        /// <param name="extentionName">生成文件后缀名称</param>
        /// <param name="outputPath">生成的数据文件的根目录</param>
        public static Task<ProgressTask> Excute(string sourcePath, string extentionName, string outputPath)
        {
            var taskCompletionSource = new TaskCompletionSource<ProgressTask>();
            using (var threadDispatcher = ThreadDispatcher.Create(null, true, async () =>
            {
                //源目录
                sourcePath = Utility.GetAbsolutePath(Environment.CurrentDirectory.AppendDirectorySeparatorIfNotEndWith(), sourcePath.AppendDirectorySeparatorIfNotEndWith());
                if (!Directory.Exists(sourcePath))
                {
                    Debug.OutputError(string.Format(Resources.Arg1DirectoryNotFound, sourcePath));
                    taskCompletionSource.SetResult(ProgressTask.CompletedTask);
                    return;
                }

                outputPath = Utility.GetAbsolutePath(Environment.CurrentDirectory.AppendDirectorySeparatorIfNotEndWith(), outputPath.AppendDirectorySeparatorIfNotEndWith());

                var mutexDesc = $"outputPath:{outputPath.ToLower()}";
                var m = new Mutex(true, $"{typeof(DesignerDataTranslator).FullName} Excute Mutex {Utility.APHash(mutexDesc)}", out bool mutexWasCreated);
                if (!mutexWasCreated)
                {
                    Debug.Output(Resources.WaitOtherProcessesFinish);
                    m.WaitOne();
                }

                try
                {
                    DateTime begin = DateTime.Now;
                    Debug.Output(Resources.Begin);

                    if (string.IsNullOrEmpty(extentionName))
                        extentionName = "tydc";
                    var commandArgs = new CommandArgs() { ResourcePath = sourcePath, ExtentionName = extentionName, OutputRootPath = outputPath };

                    if (!Directory.Exists(outputPath))
                    {
                        try
                        {
                            Directory.CreateDirectory(outputPath); // 当输出目录不存在时，尝试创建
                        }
                        catch (Exception e)
                        {
                            Debug.OutputError(string.Format(Resources.ArgOuputDirectoryNotFoundAndCanNotCreate, e.Message));
                            taskCompletionSource.SetResult(ProgressTask.CompletedTask);
                            return;
                        }
                    }

                    DirectoryInfo theSourceFolder = new DirectoryInfo(sourcePath);
                    DirectoryInfo theOutputFolder = new DirectoryInfo(outputPath);
                    //var _allCreateFilePaths = new HashSet<string>();
                    //HandleRedundantFileDirectory(true, commandArgs, theSourceFolder, theOutputFolder, _allCreateFilePaths);
                    FileInfo[] fileInfo = null;
                    try
                    {
                        fileInfo = theSourceFolder.GetFiles("*.xlsx", SearchOption.AllDirectories);// 遍历所有Excel文件
                    }
                    catch (Exception)
                    {
                        taskCompletionSource.SetResult(ProgressTask.CompletedTask);
                        return;
                    }

                    List<Task> taskFileList = new List<Task>();
                    ProgressTask progressTask = null;
                    try
                    {
                        fileInfo = fileInfo.Where(f => !f.Attributes.HasFlag(FileAttributes.Hidden)).ToArray(); // 跳过隐藏的文件
                        progressTask = new ProgressTask((uint)fileInfo.Length + 1);
                        taskCompletionSource.SetResult(progressTask);
                        Debug.Output(string.Format(Resources.ExcelFilesCount, fileInfo.Length));
                        foreach (var exl in fileInfo)
                        {
                            taskFileList.Add(Task.Run(() =>
                            {
                                HandleOneFile(exl, commandArgs/*, _allCreateFilePaths*/);
                                progressTask.IncreaseProgress();
                            }));
                        }
                    }
                    catch (Exception e)
                    {
                        taskCompletionSource.SetResult(ProgressTask.CompletedTask);
                        Debug.OutputException(e);
                        return;
                    }
                    await Task.WhenAll(taskFileList);

                    //HandleRedundantFileDirectory(false, commandArgs, theSourceFolder, theOutputFolder, _allCreateFilePaths);
                    Debug.Output(string.Format(Resources.End, DateTime.Now - begin));
                    progressTask.IncreaseProgress();
                    //taskCompletionSource.SetResult(null);
                }
                finally
                {
                    m.ReleaseMutex();
                }
            }, null)) { }
            return taskCompletionSource.Task;
        }

        /// <summary>
        /// 每个excel文件对应 内部多个表的字典
        /// </summary>
        private static ConcurrentDictionary<FileInfo, ConcurrentDictionary<string, string>> dicOutputFileNameDict = new ConcurrentDictionary<FileInfo, ConcurrentDictionary<string, string>>();

        private static void HandleOneFile(FileInfo fileInfo, CommandArgs commandArgs/*, HashSet<string> _allCreateFilePaths*/)
        {
            //var outputRootPath = _commandArgs.OutputRootPaths;
            //var extentionName = commandArgs.ExtentionName;
            //SpreadsheetDocument spreadDocument = null;
            //try
            //{
            //    // 完整路径+文件名
            //    string FileFullName = fileInfo.FullName;
            //    string fileName = fileInfo.Name.Replace(fileInfo.Extension, "");
            //    // 获取Excel文件更新时间
            //    try
            //    {
            //        fileInfo.Refresh();
            //    }
            //    catch { }
            //    DateTime ExcelLastWrieteTime = fileInfo.LastWriteTime;
            //    // 读取配置
            //    string cfg = FileFullName.Remove(FileFullName.LastIndexOf(".") + 1);
            //    cfg += "gzg";
            //    if (!File.Exists(cfg))
            //    {
            //        Debug.OutputError(string.Format(Resources.ExcelConfigFileNotFound, FileFullName));
            //        return;
            //    }
            //    ExcelConfig excCfg = ExcelConfig.LoadConfig(cfg);
            //    if (excCfg == null)
            //    {
            //        Debug.OutputError(string.Format(Resources.ExcelConfigFormatInvalid, FileFullName));
            //        return;
            //    }
            //    // 读取Excel

            //    //List<Task> taskList = new List<Task>();
            //    // 检查每个sheet是否需要更新Data文件
            //    try
            //    {
            //        using (var fileStream = new FileStream(FileFullName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            //        {
            //            try
            //            {
            //                spreadDocument = SpreadsheetDocument.Open(fileStream, false);
            //            }
            //            catch (Exception e)
            //            {
            //                Debug.OutputError(string.Format(Resources.ReadExcelFileFailed, FileFullName, e.Message));
            //                return;
            //            }

            //            WorkbookPart workbookPart = spreadDocument.WorkbookPart;
            //            if (!dicOutputFileNameDict.TryGetValue(fileInfo, out ConcurrentDictionary<string, string> dicOutputFileName))
            //            {
            //                dicOutputFileName = new ConcurrentDictionary<string, string>();
            //                dicOutputFileNameDict.TryAdd(fileInfo, dicOutputFileName);
            //            }

            //            foreach (Sheet sheet in workbookPart.Workbook.Sheets)
            //            {// 检查sheet名称是否有重复
            //                var name = $"{fileName}_{sheet.Name}";
            //                if (!dicOutputFileName.TryAdd(name, FileFullName))
            //                {
            //                    Debug.OutputError(string.Format(Resources.SheetNameRepeat, name));
            //                }
            //            }

            //            foreach (Sheet sheet in workbookPart.Workbook.Sheets)
            //            {
            //                //taskList.Add(Task.Factory.StartNew(() =>
            //                //{
            //                try
            //                {
            //                    DateTime DataLastWrieteTime;
            //                    // 获取data文件内保存的对应excel文件最后更新时间
            //                    var outputPath = commandArgs.OutputRootPath + fileInfo.FullName.Replace(commandArgs.ResourcePath, "").Replace(fileInfo.Name, "");
            //                    if (!Directory.Exists(outputPath))
            //                        Directory.CreateDirectory(outputPath);
            //                    string outFileName = Path.Combine(outputPath, $"{fileName}_{sheet.Name}" + "." + extentionName);
            //                    //_allCreateFilePaths.Add(outFileName);
            //                    if (ReadDataFileLastTime(outFileName, out DataLastWrieteTime))
            //                    {
            //                        if (ExcelLastWrieteTime == DataLastWrieteTime)// excel文件是最新的不需要更新
            //                        {
            //                            //if (DesignerDataLoader.OutputDetais)
            //                                Debug.Output(string.Format(Resources.FileNotOutOfData, outFileName));
            //                            continue;
            //                        }
            //                    }
            //                    //if (DesignerDataLoader.OutputDetais)
            //                        Debug.Output(string.Format(Resources.SavingFile, outFileName));
            //                    // 保存data
            //                    if (!SaveData(fileInfo, outFileName, cfg, sheet, workbookPart, excCfg, ExcelLastWrieteTime.Ticks))
            //                    {
            //                        try
            //                        {
            //                            if (File.Exists(outFileName))
            //                                File.Delete(outFileName);
            //                        }
            //                        catch { }
            //                    }
            //                }
            //                catch (Exception e)
            //                {
            //                    Debug.OutputException(e);
            //                }
            //                //}));
            //            }
            //        }
            //    }
            //    catch (Exception e)
            //    {
            //        Debug.OutputException(e);
            //    }
            //    //Task.WaitAll(taskList.ToArray());
            //}
            //catch (Exception e)
            //{
            //    Debug.OutputException(e);
            //}
            //finally
            //{
            //    try
            //    {
            //        if (spreadDocument != null)
            //            spreadDocument.Close();
            //    }
            //    catch { }
            //    //if (DesignerDataLoader.OutputDetais)
            //        Debug.Output(string.Format(Resources.HandleOneExcelFileFinish, fileInfo.FullName));
            //}
        }

        /// <summary>
        /// 获取data文件内保存的对应excel文件最后更新时间
        /// </summary>
        /// <param name="FileName">data文件路径</param>
        /// <param name="dt">对应excel文件最后更新时间</param>
        /// <returns></returns>
        private static bool ReadDataFileLastTime(string FileName, out DateTime dt)
        {
            dt = DateTime.MinValue;
            try
            {
                if (!File.Exists(FileName))
                    return false;

                using (var fs = File.OpenRead(FileName))
                {
                    using (var br = new BinaryReader(fs))
                    {
                        dt = new DateTime(br.ReadInt64());
                    }
                }
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        ///// <summary>
        ///// 保存sheet数据生成data文件
        ///// </summary>
        ///// <param name="fileInfo">文件信息</param>
        ///// <param name="FileName">数据文件（.tydd文件）的完整路径及文件名(包括扩展名)</param>
        ///// <param name="configFileName">配置文件（.gzg文件）的完整路径及文件名(包括扩展名)</param>
        ///// <param name="sheet">Sheet对象</param>
        ///// <param name="workbookPart"></param>
        ///// <param name="ExcelCfg">配置对象</param>
        ///// <param name="ExcelLastWriteTime">Excel文件的最后更新时间</param>
        //private static bool SaveData(FileInfo fileInfo, string FileName, string configFileName, Sheet sheet, WorkbookPart workbookPart, ExcelConfig ExcelCfg, long ExcelLastWriteTime)
        //{
        //    if (fileInfo == null)
        //        return false;
        //    string fileName = fileInfo.Name.Replace(fileInfo.Extension, "");
        //    var worksheetPart = workbookPart.GetPartById(sheet.Id) as WorksheetPart;
        //    if (worksheetPart.Worksheet.Descendants<Row>().Count() <= 0)
        //        return false;

        //    // Sheet配置
        //    SheetConfig Sheetcfg = ExcelCfg.GetSheetConfig(sheet.Name);
        //    if (Sheetcfg == null)
        //    {
        //        Debug.OutputError(string.Format(Resources.ExcelConfigFileUnMatchExcel, configFileName, $"{fileName}_{sheet.Name}"));
        //        return false;
        //    }
        //    //if (!Sheetcfg.Export /*&& DesignerDataLoader.OutputDetais*/)
        //    //{
        //    //    Debug.Output(string.Format(Resources.SheetUnExport, $"{fileName}_{sheet.Name}"));
        //    //    return false;
        //    //}

        //    // 获取WorkbookPart中NumberingFormats样式集合
        //    var stylesList = GetNumberFormatsStyle(workbookPart);

        //    // 保存
        //    using (var fileStream = new FileStream(FileName, FileMode.Create, FileAccess.Write))
        //    {
        //        using (var writer = new PkgWriter(fileStream))
        //        {
        //            // 保存对应excel文件的最后更新时间
        //            writer.Write(ExcelLastWriteTime);
        //            // 写入列信息和其MD5
        //            try
        //            {
        //                var colomnInfoMD5Bytes = Sheetcfg.ColomnInfoBytesMD5.ConvertHexStringToByteArray();
        //                //writer.Write(colomnInfoMD5Bytes.Length);
        //                writer.Write(colomnInfoMD5Bytes);

        //                var colomnInfoBytes = Sheetcfg.ColomnInfoBytesStr.ConvertHexStringToByteArray();
        //                //writer.Write(colomnInfoBytes.Length);
        //                writer.Write(colomnInfoBytes);
        //            }
        //            catch (Exception e)
        //            {
        //                Debug.OutputError(e);
        //                return false;
        //            }

        //            // 每行数据
        //            foreach (var row in worksheetPart.Worksheet.Descendants<Row>())
        //            {
        //                if (row.RowIndex == 1)
        //                    continue;

        //                var cellUnitList = new List<CellUnit>();
        //                int i = 0, exportColumnIndex = 0;
        //                foreach (Cell cell in row)
        //                {
        //                    // 将CellReference换算成1起始列序号，以检查空格子
        //                    string cellReference = cell.CellReference;
        //                    int columnIndex = 0;
        //                    int factor = 1;
        //                    for (int pos = cellReference.Length - 1; pos >= 0; pos--) // R to L
        //                    {
        //                        if (char.IsLetter(cellReference[pos])) // for letters (columnName)
        //                        {
        //                            columnIndex += factor * ((cellReference[pos] - 'A') + 1);
        //                            factor *= 26;
        //                        }
        //                    }

        //                    while (i < columnIndex - 1) // 出现空格子
        //                    {
        //                        //SaveDate(i, Sheetcfg, null, workbookPart, stylesList, protoWriter);
        //                        if (!AddCellUnit(fileInfo, i, ref exportColumnIndex, Sheetcfg, null, workbookPart, stylesList, cellUnitList))
        //                            return false;
        //                        ++i;
        //                    }

        //                    //SaveDate(i, Sheetcfg, cell, workbookPart, stylesList, protoWriter);
        //                    if (!AddCellUnit(fileInfo, i, ref exportColumnIndex, Sheetcfg, cell, workbookPart, stylesList, cellUnitList))
        //                        return false;
        //                    ++i;
        //                }

        //                while (i < Sheetcfg.ColumnConfigData.Count) // 末尾出现空格子
        //                {
        //                    //SaveDate(i, Sheetcfg, null, workbookPart, stylesList, protoWriter);
        //                    if (!AddCellUnit(fileInfo, i, ref exportColumnIndex, Sheetcfg, null, workbookPart, stylesList, cellUnitList))
        //                        return false;
        //                    ++i;
        //                }

        //                writer.WriteUseZigZag32(cellUnitList.Count);
        //                foreach (var cellUnit in cellUnitList)
        //                    cellUnit.Write(writer);
        //            }
        //        }
        //    }

        //    return true;
        //}

        //static bool AddCellUnit(FileInfo fileInfo, int columnIndex, ref int exportColumnIndex, SheetConfig Sheetcfg, Cell cell, WorkbookPart workbookPart, List<string> stylesList, List<CellUnit> cellUnitList)
        //{
        //    if (fileInfo == null)
        //        goto Error;
        //    if (columnIndex >= Sheetcfg.ColumnConfigData.Count)
        //        return true;

        //    var columnConfig = Sheetcfg.ColumnConfigData[columnIndex];
        //    if (!columnConfig.Export)
        //        return true;

        //    var dataType = columnConfig.FieldType;
        //    string cellValue = cell == null ? "" : GetCellValue(cell, workbookPart, stylesList);
        //    CellUnit cellUnit = null;
        //    switch (dataType)
        //    {
        //        case DesignerDataType.Int:
        //            {
        //                int intValue = 0;
        //                if (!int.TryParse(cellValue, out intValue) && !string.IsNullOrEmpty(cellValue))
        //                    goto Error;
        //                cellUnit = new CellUnitIntValue(exportColumnIndex, intValue);
        //            }
        //            break;
        //        case DesignerDataType.Float:
        //            {
        //                float floatValue = 0.0f;
        //                if (!float.TryParse(cellValue, out floatValue) && !string.IsNullOrEmpty(cellValue))
        //                    goto Error;
        //                cellUnit = new CellUnitFloatValue(exportColumnIndex, floatValue);
        //            }
        //            break;
        //        case DesignerDataType.String:
        //            {
        //                cellUnit = new CellUnitStringValue(exportColumnIndex, cellValue);
        //            }
        //            break;
        //        case DesignerDataType.Bool:
        //            {
        //                decimal decimalValue = 0;
        //                cellUnit = new CellUnitBoolValue(exportColumnIndex, decimal.TryParse(cellValue, out decimalValue) && decimalValue != 0);
        //            }
        //            break;
        //    }

        //    if (cellUnit.IsNeedWrite())
        //        cellUnitList.Add(cellUnit);
        //    ++exportColumnIndex;

        //    return true;

        //Error:
        //    string excelFileName = "";
        //    if (fileInfo == null || !dicOutputFileNameDict.TryGetValue(fileInfo, out ConcurrentDictionary<string, string> dicOutputFileName))
        //    {
        //        Debug.OutputError(string.Format(Resources.ExcelConfigFileInValid, fileInfo?.Name ?? "[fileInfo == null]", Sheetcfg.Name));
        //        return false;
        //    }

        //    if (!dicOutputFileName.TryGetValue(Sheetcfg.Name, out excelFileName))
        //        excelFileName = "";
        //    Debug.OutputError(string.Format(Resources.ExcelConfigFileInValid, excelFileName, Sheetcfg.Name));
        //    return false;
        //}

        /// <summary>
        /// 根据Excel单元格和WorkbookPart对象获取单元格的值
        /// </summary>
        /// <param name="cell">Excel单元格对象</param>
        /// <param name="workBookPart">Excel WorkbookPart对象</param>
        /// <param name="stylesList"></param>
        /// <returns>单元格的值</returns>
        static string GetCellValue(Cell cell, WorkbookPart workBookPart, List<string> stylesList)
        {
            string cellValue = string.Empty;
            if (cell.ChildElements.Count == 0)//Cell节点下没有子节点
            {
                return cellValue;
            }
            //string cellRefId = cell.CellReference.InnerText;//获取引用相对位置
            string cellInnerText = cell.CellValue.InnerText;//获取Cell的InnerText
            cellValue = cellInnerText;//指定默认值(其实用来处理Excel中的数字)

            //获取WorkbookPart中共享String数据
            SharedStringTable sharedTable = workBookPart.SharedStringTablePart.SharedStringTable;

            try
            {
                EnumValue<CellValues> cellType = cell.DataType;//获取Cell数据类型
                if (cellType != null)//Excel对象数据
                {
                    switch (cellType.Value)
                    {
                        case CellValues.SharedString://字符串
                            //获取该Cell的所在的索引
                            int cellIndex = int.Parse(cellInnerText);
                            cellValue = sharedTable.ChildElements[cellIndex].InnerText;
                            break;
                        case CellValues.Boolean://布尔
                            cellValue = (cellInnerText == "1") ? "TRUE" : "FALSE";
                            break;
                        case CellValues.Date://日期
                            cellValue = Convert.ToDateTime(cellInnerText).ToString();
                            break;
                        case CellValues.Number://数字
                            cellValue = Convert.ToDecimal(cellInnerText).ToString();
                            break;
                        default:
                            cellValue = cellInnerText;
                            break;
                    }
                }
                else//格式化数据
                {
                    if (stylesList.Count > 0 && cell.StyleIndex != null)//对于数字,cell.StyleIndex==null
                    {
                        int styleIndex = Convert.ToInt32(cell.StyleIndex.Value) - 1;
                        if (styleIndex >= 0 && styleIndex < stylesList.Count)
                        {
                            string cellStyle = stylesList[styleIndex];//获取该索引的样式
                            if (cellStyle.Contains("yyyy") || cellStyle.Contains("h")
                                || cellStyle.Contains("dd") || cellStyle.Contains("ss"))
                            {
                                //如果为日期或时间进行格式处理,去掉“;@”
                                cellStyle = cellStyle.Replace(";@", "");
                                while (cellStyle.Contains("[") && cellStyle.Contains("]"))
                                {
                                    int otherStart = cellStyle.IndexOf('[');
                                    int otherEnd = cellStyle.IndexOf("]");

                                    cellStyle = cellStyle.Remove(otherStart, otherEnd - otherStart + 1);
                                }
                                double doubleDateTime = double.Parse(cellInnerText);
                                DateTime dateTime = DateTime.FromOADate(doubleDateTime);//将Double日期数字转为日期格式
                                                                                        //if (cellStyle.Contains("m"))
                                cellStyle = cellStyle.Replace("m", "M");
                                //if (cellStyle.Contains("AM/PM"))
                                cellStyle = cellStyle.Replace("AM/PM", "");
                                cellValue = dateTime.ToString(cellStyle);//不知道为什么Excel 2007中格式日期为yyyy/m/d
                            }
                            else//其他的货币、数值
                            {
                                var indexOfDot = cellStyle.LastIndexOf('.');
                                if (indexOfDot >= 1)
                                {
                                    cellStyle = cellStyle.Substring(indexOfDot - 1).Replace("\\", "");
                                    if (decimal.TryParse(cellInnerText, out var decimalNum) && decimal.TryParse(decimalNum.ToString(cellStyle), out var decimalNum2))
                                        cellValue = decimalNum2.ToString();
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Debug.OutputException(e);
            }
            return cellValue;
        }

        /// <summary>
        /// 根据WorkbookPart获取NumberingFormats样式集合
        /// </summary>
        /// <param name="workBookPart">WorkbookPart对象</param>
        /// <returns>NumberingFormats样式集合</returns>
        static List<string> GetNumberFormatsStyle(WorkbookPart workBookPart)
        {
            List<string> styleList = new List<string>();
            Stylesheet styleSheet = workBookPart.WorkbookStylesPart.Stylesheet;
            if (styleSheet.NumberingFormats != null)
            {
                OpenXmlElementList list = styleSheet.NumberingFormats.ChildElements;//获取NumberingFormats样式集合
                if (list != null)
                {
                    foreach (var element in list)//格式化节点
                    {
                        if (element.HasAttributes)
                        {
                            using (OpenXmlReader reader = OpenXmlReader.Create(element))
                            {
                                if (reader.Read())
                                {
                                    if (reader.Attributes.Count > 0)
                                    {
                                        string numFmtId = reader.Attributes[0].Value;//格式化ID
                                        string formatCode = reader.Attributes[1].Value;//格式化Code
                                        styleList.Add(formatCode);//将格式化Code写入List集合
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return styleList;
        }
    }

    abstract class CellUnit
    {
        private int ColumnIndex { get; set; } = 0;
        //internal DesignerDataType DataType { get; private set; } = DesignerDataType.String;

        internal CellUnit(int columnIndex)
        {
            ColumnIndex = columnIndex;
            //DataType = dataType;
        }

        internal abstract bool IsNeedWrite();

        internal virtual void Write(PkgWriter writer)
        {
            writer.WriteUseZigZag32(ColumnIndex);
        }
    }

    class CellUnitStringValue : CellUnit
    {
        private string Value { get; set; }

        internal CellUnitStringValue(int columnIndex, string value) : base(columnIndex)
        {
            Value = value;
        }

        internal override bool IsNeedWrite() => !string.IsNullOrEmpty(Value);

        internal override void Write(PkgWriter writer)
        {
            base.Write(writer);
            writer.Write(Value);
        }
    }

    class CellUnitIntValue : CellUnit
    {
        private int Value { get; set; }

        internal CellUnitIntValue(int columnIndex, int value) : base(columnIndex)
        {
            Value = value;
        }

        internal override bool IsNeedWrite() => Value != 0;

        internal override void Write(PkgWriter writer)
        {
            base.Write(writer);
            writer.WriteUseZigZag32(Value);
        }
    }

    class CellUnitFloatValue : CellUnit
    {
        private float Value { get; set; }

        internal CellUnitFloatValue(int columnIndex, float value) : base(columnIndex)
        {
            Value = value;
        }

        internal override bool IsNeedWrite() => Value != 0.0f;

        internal override void Write(PkgWriter writer)
        {
            base.Write(writer);
            writer.Write(Value);
        }
    }

    class CellUnitBoolValue : CellUnit
    {
        private bool Value { get; set; }

        internal CellUnitBoolValue(int columnIndex, bool value) : base(columnIndex)
        {
            Value = value;
        }

        internal override bool IsNeedWrite() => Value;

        internal override void Write(PkgWriter writer)
        {
            base.Write(writer);
            writer.Write(Value);
        }
    }
}