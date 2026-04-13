using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using WeaselSettings; // 确保你有这个命名空间引用

namespace 短语管理工具
{
    public partial class Phrase : Form
    {
        // 【核心修改】AOT安全的显示列表与全量列表，取代 BindingList
        private List<DictItem> _currentList = new List<DictItem>();
        private List<DictItem> _allItems = new List<DictItem>();

        // 文件路径
        private string _dictFilePath;

        // 主词库：Word -> 所有 Code
        private Dictionary<string, HashSet<string>> _mainDictIndex = new Dictionary<string, HashSet<string>>();

        public Phrase()
        {
            InitializeComponent();

            // 初始化路径 (假设在用户文件夹下)
            string userDir = YamlHelper.GetUserDataDir();
            _dictFilePath = Path.Combine(userDir, "phrase.txt");

            // ==========================================
            // 【核心修改】开启虚拟模式，兼容 AOT 编译
            // ==========================================
            dgvList.VirtualMode = true;
            dgvList.AutoGenerateColumns = false;

            LoadData();
        }

        // 提供虚拟模式数据的事件 (AOT 不会崩溃的核心所在)
        private void DgvList_CellValueNeeded(object sender, DataGridViewCellValueEventArgs e)
        {
            if (_currentList == null || e.RowIndex < 0 || e.RowIndex >= _currentList.Count)
                return;

            var item = _currentList[e.RowIndex];

            // 使用列的 Name 属性进行硬编码匹配，完全脱离反射
            if (dgvList.Columns[e.ColumnIndex].Name == "colCode")
            {
                e.Value = item.Code;
            }
            else if (dgvList.Columns[e.ColumnIndex].Name == "colWord")
            {
                e.Value = item.Word;
            }
        }

        // 文本框内容改变时，只重置定时器，不立即搜索
        private void TxtFilter_TextChanged(object sender, EventArgs e)
        {
            timer1.Stop();
            timer1.Start();
        }

        // 定时器触发真正的搜索
        private void SearchTimer_Tick(object sender, EventArgs e)
        {
            timer1.Stop();
            PerformSearch(txtFilter.Text.Trim());
        }

        private void PerformSearch(string keyword)
        {
            if (string.IsNullOrEmpty(keyword))
            {
                // 关键词为空，显示全部
                _currentList = _allItems.ToList();
            }
            else
            {
                // 忽略大小写
                string lowerKey = keyword.ToLower();

                // 内存过滤
                _currentList = _allItems
                    .Where(item => (item.Code != null && item.Code.Contains(lowerKey, StringComparison.OrdinalIgnoreCase)) ||
                                   (item.Word != null && item.Word.Contains(lowerKey)))
                    .ToList();
            }

            // 【核心修改】直接告诉 DataGridView 数据量变了，强制重绘
            dgvList.RowCount = _currentList.Count;
            dgvList.Invalidate();

            UpdatePageInfo();
        }

        // 1. 加载数据
        private void LoadData()
        {
            try
            {
                // 如果文件不存在，直接建个空文件，避免弹错退出
                if (!File.Exists(_dictFilePath))
                {
                    File.WriteAllText(_dictFilePath, "", new UTF8Encoding(false));
                }

                _allItems = RimeDictParser.LoadDict(_dictFilePath);
                _currentList = _allItems.ToList();

                // 更新行数
                dgvList.RowCount = _currentList.Count;

                UpdatePageInfo();
            }
            catch (Exception ex)
            {
                MessageBox.Show("读取短语文件失败：" + ex.Message);
            }
        }

        private void BtnOk_Click(object sender, EventArgs e)
        {
            try
            {
                // 保存全量数据，防止搜索状态下只保存了过滤后的数据
                RimeDictParser.SaveDict(_dictFilePath, _allItems);

                MessageBox.Show("保存成功！需要重新部署才能生效。", "提示");
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("保存失败：" + ex.Message);
            }
        }

        // 4. 删除词条
        private void BtnDelete_Click(object sender, EventArgs e)
        {
            if (dgvList.SelectedRows.Count == 0) return;

            // 【核心修改】必须倒序获取选中行的索引，按索引从 List 中删除
            var selectedIndices = dgvList.SelectedRows
                                         .Cast<DataGridViewRow>()
                                         .Select(r => r.Index)
                                         .OrderByDescending(i => i)
                                         .ToList();

            foreach (var index in selectedIndices)
            {
                var itemToDelete = _currentList[index];
                _allItems.Remove(itemToDelete);
                _currentList.RemoveAt(index);
            }

            // 刷新 UI
            dgvList.RowCount = _currentList.Count;
            dgvList.Invalidate();

            UpdatePageInfo();
        }

        // 5. 导入数据 (追加)
        private void BtnImport_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog
            {
                Filter = "文本文件|*.txt;*"
            };

            if (ofd.ShowDialog() != DialogResult.OK)
                return;

            try
            {
                Cursor = Cursors.WaitCursor;

                int imported = 0;
                int skipped = 0;

                using var fs = new FileStream(ofd.FileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                using var reader = new StreamReader(fs, Encoding.UTF8);

                string? line;
                while ((line = reader.ReadLine()) != null)
                {
                    if (string.IsNullOrWhiteSpace(line) || line.StartsWith('#'))
                        continue;

                    if (!TryParsePhraseLine(line, out var item))
                        continue;

                    // 主词库已有 → 跳过
                    if (_mainDictIndex.TryGetValue(item.Word, out var codes) && codes.Contains(item.Code))
                    {
                        skipped++;
                        continue;
                    }

                    // 用户词库已有 → 跳过
                    if (_allItems.Any(x => x.Code == item.Code && x.Word == item.Word))
                    {
                        skipped++;
                        continue;
                    }

                    _allItems.Add(item);
                    imported++;
                }

                PerformSearch(txtFilter.Text.Trim());

                MessageBox.Show(
                    $"导入完成\r\n成功：{imported}\r\n跳过重码：{skipped}",
                    "导入结果"
                );
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        // 专门用于解析 Phrase 的行 (Code \t Word)
        private static bool TryParsePhraseLine(string line, out DictItem item)
        {
            item = null!;

            var parts = line.Split(new[] { '\t' }, 2, StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length < 2)
                return false;

            item = new DictItem
            {
                Code = parts[0].Trim(),
                Word = parts[1].Trim()
            };
            return true;
        }

        // 6. 清空数据
        private void BtnClear_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("确定要清空所有自定义短语吗？", "警告", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                _allItems.Clear();
                _currentList.Clear();
                dgvList.RowCount = 0;
                dgvList.Invalidate();
                UpdatePageInfo();
            }
        }

        private void UpdatePageInfo()
        {
            lblPageInfo.Text = $"共 {_currentList.Count} 条数据";
        }

        public static class RimeDictParser
        {
            // 读取词库
            public static List<DictItem> LoadDict(string filePath)
            {
                var items = new List<DictItem>();
                if (!File.Exists(filePath)) return items;

                using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                using (var reader = new StreamReader(fs, Encoding.UTF8))
                {
                    string? line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        if (string.IsNullOrWhiteSpace(line) || line.StartsWith('#')) continue;

                        var parts = line.Split(new[] { '\t' }, 2, StringSplitOptions.RemoveEmptyEntries);

                        if (parts.Length >= 2)
                        {
                            var item = new DictItem
                            {
                                Code = parts[0].Trim(),
                                Word = parts[1].Trim()
                            };
                            items.Add(item);
                        }
                    }
                }
                return items;
            }

            // 保存词库
            public static void SaveDict(string filePath, List<DictItem> items)
            {
                var sb = new StringBuilder();

                foreach (var item in items)
                {
                    sb.AppendLine($"{item.Code}\t{item.Word}");
                }

                File.WriteAllText(filePath, sb.ToString(), new UTF8Encoding(false));
            }
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            // 注意这里只传入 _allItems，对应最新的 AddWordForm 构造函数
            using var addForm = new AddWordForm(_allItems);

            if (addForm.ShowDialog() == DialogResult.OK)
            {
                var newItem = addForm.NewItem;
                if (newItem != null)
                {
                    _allItems.Add(newItem);
                    PerformSearch(txtFilter.Text.Trim());
                }
            }
        }

        // 独立解析主词库（wubi.dict.yaml）防冲突
        private void LoadMainDictIndex()
        {
            string userDir = YamlHelper.GetUserDataDir();
            string mainDictPath = Path.Combine(userDir, "wubi.dict.yaml");

            if (!File.Exists(mainDictPath))
                return;

            using var fs = new FileStream(mainDictPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            using var reader = new StreamReader(fs, Encoding.UTF8);

            bool isBody = false;
            string? line;

            while ((line = reader.ReadLine()) != null)
            {
                if (!isBody)
                {
                    if (line.Trim() == "...")
                        isBody = true;
                    continue;
                }

                if (string.IsNullOrWhiteSpace(line) || line.StartsWith('#'))
                    continue;

                // 主词库一般为: Word <Tab> Code <Tab> Weight
                // 或者: Code <Tab> Word
                var parts = line.Split(new[] { '\t', ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length < 2) continue;

                // 这里按 词(0) 码(1) 解析，如果是 码(0) 词(1) 的主词库请自行调整 parts 顺序
                string word = parts[0];
                string code = parts[1];

                // 如果 parts[0] 是英文，说明是 码在前 词在后
                if (parts[0].Length > 0 && parts[0][0] <= 0x7F)
                {
                    code = parts[0];
                    word = parts[1];
                }

                if (!_mainDictIndex.TryGetValue(word, out var codes))
                {
                    codes = new HashSet<string>();
                    _mainDictIndex[word] = codes;
                }

                codes.Add(code);
            }
        }

        private void btnExport_Click(object sender, EventArgs e)
        {
            if (_currentList == null || _currentList.Count == 0)
            {
                MessageBox.Show("当前没有可导出的数据。");
                return;
            }

            using var sfd = new SaveFileDialog
            {
                Filter = "文本文件 (UTF8无BOM.txt)|*.txt",
                FileName = "phrase_export.txt"
            };

            if (sfd.ShowDialog() != DialogResult.OK)
                return;

            try
            {
                Cursor = Cursors.WaitCursor;

                using var fs = new FileStream(sfd.FileName, FileMode.Create, FileAccess.Write, FileShare.Read);
                using var writer = new StreamWriter(fs, new UTF8Encoding(false), 1 << 16);

                foreach (var item in _currentList)
                {
                    writer.WriteLine($"{item.Code}\t{item.Word}");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("导出失败：" + ex.Message);
            }
            finally
            {
                Cursor = Cursors.Default;
            }

            MessageBox.Show($"成功导出 {_currentList.Count} 条数据。");
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void dgvList_CellMouseDoubleClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            // 确保点击的是有效行（RowIndex >= 0 排除表头）
            if (e.RowIndex < 0 || e.RowIndex >= _currentList.Count)
                return;

            // 获取当前视图中的对应数据对象
            var targetItem = _currentList[e.RowIndex];

            // 调用构造函数，传入全量列表（用于查重）和当前项目（用于编辑）
            using var editForm = new AddWordForm(_allItems, targetItem);

            if (editForm.ShowDialog() == DialogResult.OK && editForm.NewItem != null)
            {
                // 将修改后的值更新到原始对象中
                targetItem.Code = editForm.NewItem.Code;
                targetItem.Word = editForm.NewItem.Word;

                // 刷新 DataGridView 的当前行
                dgvList.InvalidateRow(e.RowIndex);

                // 如果你担心编辑后不符合当前的搜索过滤条件，也可以重新执行一遍搜索
                // PerformSearch(txtFilter.Text.Trim());
            }
        }
    }
}