using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using WeaselSettings;

namespace 词库管理工具
{
    public partial class WubiDict : Form
    {
        private string _originalHeader = "";
        private string _dictFilePath;

        // 唯一真实数据源
        private readonly List<DictItem> _allItems = new();

        // 当前视图（VirtualMode 的 backing list）
        private List<DictItem> _currentView = new();

        // 主词库索引
        private readonly Dictionary<string, HashSet<string>> _mainDictIndex = new();

        public WubiDict()
        {
            InitializeComponent();

            // === DataGridView VirtualMode 设置 ===
            dgvList.VirtualMode = true;
            dgvList.AutoGenerateColumns = false;
            dgvList.AllowUserToAddRows = false;
            dgvList.ReadOnly = true;
            dgvList.RowHeadersVisible = false;
            dgvList.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;


            string userDir = YamlHelper.GetUserDataDir();
            _dictFilePath = Path.Combine(userDir, "wubi.extended.dict.yaml");

            LoadMainDictIndex();
            LoadData();
        }

        #region 搜索

        private void TxtFilter_TextChanged(object sender, EventArgs e)
        {
            timer1.Stop();
            timer1.Start();
        }

        private void SearchTimer_Tick(object sender, EventArgs e)
        {
            timer1.Stop();
            PerformSearch(txtFilter.Text.Trim());
        }

        private void PerformSearch(string keyword)
        {
            if (string.IsNullOrEmpty(keyword))
            {
                _currentView = _allItems;
            }
            else
            {
                _currentView = _allItems
                    .Where(x =>
                        (x.Code?.Contains(keyword, StringComparison.OrdinalIgnoreCase) == true) ||
                        (x.Word?.Contains(keyword) == true))
                    .ToList();
            }

            dgvList.RowCount = _currentView.Count;
            lblPageInfo.Text = $"共 {_currentView.Count} 条数据";
        }

        #endregion

        #region DataGridView VirtualMode

        private void dgvList_CellValueNeeded(object sender, DataGridViewCellValueEventArgs e)
        {
            if (e.RowIndex < 0 || e.RowIndex >= _currentView.Count)
                return;

            var item = _currentView[e.RowIndex];

            switch (e.ColumnIndex)
            {
                case 0: // 编码
                    e.Value = item.Code;
                    break;
                case 1: // 词条
                    e.Value = item.Word;
                    break;
                case 2: // 权重（如果你有第三列）
                    e.Value = item.Weight > 0 ? item.Weight.ToString() : "";
                    break;
            }
        }


        #endregion

        #region 加载 / 保存

        private void LoadData()
        {
            if (!File.Exists(_dictFilePath))
            {
                MessageBox.Show($"未找到词库文件：\r\n{_dictFilePath}");
                Close();
                return;
            }

            var result = RimeDictParser.LoadDict(_dictFilePath);
            _originalHeader = result.Header;

            _allItems.Clear();
            _allItems.AddRange(result.Items);

            _currentView = _allItems;
            dgvList.RowCount = _currentView.Count;

            lblPageInfo.Text = $"共 {_currentView.Count} 条数据";
        }

        private void BtnOk_Click(object sender, EventArgs e)
        {
            try
            {
                RimeDictParser.SaveDict(_dictFilePath, _originalHeader, _allItems);
                MessageBox.Show("保存成功，点击下方保存后才能生效。");
                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("保存失败：" + ex.Message);
            }
        }
        private void btnCancel_Click(object sender, EventArgs e)
        {
            Close();
        }

        #endregion

        #region 增删改

        private void btnAdd_Click(object sender, EventArgs e)
        {
            using var addForm = new AddWordForm(_allItems, _mainDictIndex);
            if (addForm.ShowDialog() == DialogResult.OK)
            {
                _allItems.Add(addForm.NewItem);
                PerformSearch(txtFilter.Text.Trim());
            }
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            var rows = dgvList.SelectedRows
                .Cast<DataGridViewRow>()
                .Select(r => r.Index)
                .OrderByDescending(i => i)
                .ToArray();

            foreach (var index in rows)
            {
                var item = _currentView[index];
                _allItems.Remove(item);
            }

            PerformSearch(txtFilter.Text.Trim());
        }

        private void BtnClear_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("确定清空所有自定义词条？", "警告",
                MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                _allItems.Clear();
                PerformSearch("");
            }
        }

        #endregion

        #region 导入 / 导出

        private void BtnImport_Click(object sender, EventArgs e)
        {
            using var ofd = new OpenFileDialog { Filter = "文本文件 (UTF8无BOM.txt)|*.txt" };
            if (ofd.ShowDialog() != DialogResult.OK) return;

            int imported = 0, skipped = 0;

            using var reader = new StreamReader(ofd.FileName, new UTF8Encoding(false));
            string? line;

            while ((line = reader.ReadLine()) != null)
            {
                if (!TryParseDictLine(line, out var item))
                    continue;

                if (_mainDictIndex.TryGetValue(item.Word, out var codes) &&
                    codes.Contains(item.Code))
                {
                    skipped++;
                    continue;
                }

                if (_allItems.Any(x => x.Word == item.Word && x.Code == item.Code))
                {
                    skipped++;
                    continue;
                }

                _allItems.Add(item);
                imported++;
            }

            PerformSearch(txtFilter.Text.Trim());

            MessageBox.Show($"导入完成\r\n成功：{imported}\r\n跳过：{skipped}");
        }

        private void btnExport_Click(object sender, EventArgs e)
        {
            if (_currentView.Count == 0)
            {
                MessageBox.Show("没有可导出的数据");
                return;
            }

            using var sfd = new SaveFileDialog
            {
                Filter = "文本文件 (*.txt)|*.txt",
                FileName = "wubi_export.txt"
            };

            if (sfd.ShowDialog() != DialogResult.OK)
                return;

            using var writer = new StreamWriter(sfd.FileName, false, new UTF8Encoding(false));
            foreach (var item in _currentView)
            {
                if (item.Weight > 0)
                    writer.WriteLine($"{item.Code}\t{item.Word}\t{item.Weight}");
                else
                    writer.WriteLine($"{item.Code}\t{item.Word}");
            }

            MessageBox.Show($"成功导出 {_currentView.Count} 条数据");
        }

        #endregion

        #region 主词库索引

        private void LoadMainDictIndex()
        {
            string path = Path.Combine(YamlHelper.GetUserDataDir(), "wubi.dict.yaml");
            if (!File.Exists(path)) return;

            using var reader = new StreamReader(path, new UTF8Encoding(false));
            string? line;
            bool body = false;

            while ((line = reader.ReadLine()) != null)
            {
                if (!body)
                {
                    if (line.Trim() == "...") body = true;
                    continue;
                }

                if (!TryParseDictLine(line, out var item))
                    continue;

                if (!_mainDictIndex.TryGetValue(item.Word, out var set))
                    _mainDictIndex[item.Word] = set = new();

                set.Add(item.Code);
            }
        }

        #endregion

        #region 工具方法（AOT-safe）

        private static bool TryParseDictLine(string line, out DictItem item)
        {
            item = null;

            if (string.IsNullOrWhiteSpace(line))
                return false;

            if (line.StartsWith('#'))
                return false;

            var parts = line.Split(['\t'], StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length < 2)
                return false;

            string word = null;
            string code = null;
            string fullCode = null;
            int weight = 0;

            foreach (var part in parts)
            {
                // 1️⃣ 权重
                if (int.TryParse(part, out var w))
                {
                    weight = w;
                    continue;
                }

                // 2️⃣ 编码（ASCII字母）
                if (IsCode(part))
                {
                    if (code == null)
                    {
                        code = part;
                    }
                    else
                    {
                        // 第二个编码字段 -> 认为是全码
                        fullCode ??= part;
                    }
                    continue;
                }

                // 3️⃣ 其余全部当作词条（支持符号）
                word ??= part;
            }

            if (string.IsNullOrEmpty(word) || string.IsNullOrEmpty(code))
                return false;

            item = new DictItem
            {
                Word = word,
                Code = code,
                Weight = weight
                // 如果你有 FullCode 字段可以加：
                // FullCode = fullCode
            };

            return true;
        }


        private static bool IsWord(string text)
        {
            foreach (char c in text)
            {
                if (c > 0x7F)
                    return true;
            }
            return false;
        }

        private static bool IsCode(string text)
        {
            foreach (char c in text)
            {
                if (c < 'a' || c > 'z')
                    return false;
            }
            return true;
        }



        #endregion

        #region RimeDictParser（AOT-safe）

        private static class RimeDictParser
        {
            public static (string Header, List<DictItem> Items) LoadDict(string filePath)
            {
                var items = new List<DictItem>();
                var header = new StringBuilder();

                using var reader = new StreamReader(filePath, new UTF8Encoding(false));
                string? line;
                bool body = false;

                while ((line = reader.ReadLine()) != null)
                {
                    if (!body)
                    {
                        header.AppendLine(line);
                        if (line.Trim() == "...")
                            body = true;
                        continue;
                    }

                    if (TryParseDictLine(line, out var item))
                        items.Add(item);
                }

                return (header.ToString(), items);
            }

            public static void SaveDict(string filePath, string header, List<DictItem> items)
            {
                var sb = new StringBuilder();
                sb.Append(header);

                if (!header.TrimEnd().EndsWith("..."))
                    sb.AppendLine("...");

                foreach (var item in items)
                {
                    if (item.Weight > 0)
                        sb.AppendLine($"{item.Word}\t{item.Code}\t{item.Weight}");
                    else
                        sb.AppendLine($"{item.Word}\t{item.Code}");
                }


                File.WriteAllText(filePath, sb.ToString(), new UTF8Encoding(false));
            }
        }

        #endregion

        private void dgvList_CellMouseDoubleClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            // 确保点击的是有效行（排除表头）
            if (e.RowIndex < 0 || e.RowIndex >= _currentView.Count) return;

            var selectedItem = _currentView[e.RowIndex];

            // 调用构造函数，传入当前选中的词条
            using var editForm = new AddWordForm(_allItems, _mainDictIndex, selectedItem);

            if (editForm.ShowDialog() == DialogResult.OK && editForm.NewItem != null)
            {
                // 更新原始数据
                selectedItem.Word = editForm.NewItem.Word;
                selectedItem.Code = editForm.NewItem.Code;
                selectedItem.Weight = editForm.NewItem.Weight;

                // 刷新显示
                dgvList.InvalidateRow(e.RowIndex);
                // 或者简单粗暴地执行搜索以刷新视图
                // PerformSearch(txtFilter.Text.Trim());
            }
        }
    }
}
