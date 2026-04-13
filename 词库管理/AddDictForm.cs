using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using WeaselSettings;

namespace 词库管理工具
{
    public partial class AddWordForm : Form
    {
        public DictItem? NewItem { get; private set; }

        private readonly List<DictItem> _extItems;
        private readonly Dictionary<string, HashSet<string>> _mainDict;
        // --- 新增：用于缓存单字五笔全码的字典和状态 ---
        private readonly Dictionary<char, string> _wubiCharCodes = new Dictionary<char, string>();
        private bool _isDictValid = false;
        // 1. 新增一个私有变量记录原始词条，用于在编辑时排除掉对自身的重复检查
        private string _editingOriginalWord = "";

        // 2. 修改/新增构造函数
        public AddWordForm(List<DictItem> extItems, Dictionary<string, HashSet<string>> mainDict, DictItem? targetItem = null)
        {
            InitializeComponent();
            _extItems = extItems;
            _mainDict = mainDict;
            LoadWubiDict();

            if (targetItem != null)
            {
                // 进入编辑模式
                this.Text = "编辑词条";
                _editingOriginalWord = targetItem.Word;
                txtWord.Text = targetItem.Word;
                txtCode.Text = targetItem.Code;
                // 注意：先赋值再绑定事件，或者在赋值后手动触发一次逻辑
            }
        }

        // --- 新增：高性能词库加载模块 ---
        private void LoadWubiDict()
        {
            try
            {
                string userDir = YamlHelper.GetUserDataDir();
                string dictPath = Path.Combine(userDir, "wb_spelling.dict.yaml");

                if (!File.Exists(dictPath))
                {
                    _isDictValid = false;
                    return;
                }

                bool isDataSection = false;
                // 使用 StreamReader 逐行读取，极大降低大词库的内存分配开销
                using (var sr = new StreamReader(dictPath, Encoding.UTF8))
                {
                    string? line;
                    while ((line = sr.ReadLine()) != null)
                    {
                        if (!isDataSection)
                        {
                            if (line.Trim() == "...") isDataSection = true;
                            continue;
                        }

                        if (string.IsNullOrWhiteSpace(line) || line.StartsWith('#'))
                            continue;

                        int tabIndex = line.IndexOf('\t');
                        if (tabIndex > 0)
                        {
                            string word = line.Substring(0, tabIndex).Trim();
                            // 仅提取单字用来做词组推导，过滤词组
                            if (word.Length == 1)
                            {
                                char c = word[0];
                                // 遇到多重编码时，保留权重最高的第一条
                                if (!_wubiCharCodes.ContainsKey(c))
                                {
                                    string data = line.Substring(tabIndex + 1).Trim();
                                    // 解析匹配格式：[GB2312,字体拆分,rqyy,拼音,(1)]
                                    if (data.StartsWith('[') && data.EndsWith(']'))
                                    {
                                        // 直接寻找逗号索引，避免 Split() 产生大量字符串数组
                                        int firstComma = data.IndexOf(',');
                                        if (firstComma != -1)
                                        {
                                            int secondComma = data.IndexOf(',', firstComma + 1);
                                            if (secondComma != -1)
                                            {
                                                int thirdComma = data.IndexOf(',', secondComma + 1);
                                                if (thirdComma != -1)
                                                {
                                                    string code = data.Substring(secondComma + 1, thirdComma - secondComma - 1).Trim();
                                                    if (!string.IsNullOrEmpty(code))
                                                    {
                                                        _wubiCharCodes[c] = code;
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                _isDictValid = _wubiCharCodes.Count > 0;
            }
            catch
            {
                _isDictValid = false;
            }
        }

        // --- 新增：五笔词组编码推导模块 ---
        private string GenerateWubiCode(string word)
        {
            if (!_isDictValid || string.IsNullOrEmpty(word)) return "";

            // 单字直接查表返回
            if (word.Length == 1)
            {
                return _wubiCharCodes.TryGetValue(word[0], out var code) ? code : "";
            }

            // 如果词组中有任意一个字在字库里找不到，则直接返回空，交由人工手动输入
            foreach (char c in word)
            {
                if (!_wubiCharCodes.ContainsKey(c)) return "";
            }

            try
            {
                if (word.Length == 2)
                {
                    return GetCodePrefix(_wubiCharCodes[word[0]], 2) +
                           GetCodePrefix(_wubiCharCodes[word[1]], 2);
                }
                else if (word.Length == 3)
                {
                    return GetCodePrefix(_wubiCharCodes[word[0]], 1) +
                           GetCodePrefix(_wubiCharCodes[word[1]], 1) +
                           GetCodePrefix(_wubiCharCodes[word[2]], 2);
                }
                else
                {
                    return GetCodePrefix(_wubiCharCodes[word[0]], 1) +
                           GetCodePrefix(_wubiCharCodes[word[1]], 1) +
                           GetCodePrefix(_wubiCharCodes[word[2]], 1) +
                           GetCodePrefix(_wubiCharCodes[word[word.Length - 1]], 1);
                }
            }
            catch
            {
                return "";
            }
        }

        private string GetCodePrefix(string code, int length)
        {
            if (string.IsNullOrEmpty(code)) return "";
            return code.Length >= length ? code.Substring(0, length) : code;
        }


        private void TxtWord_TextChanged(object sender, EventArgs e)
        {
            string word = txtWord.Text.Trim();

            if (string.IsNullOrEmpty(word))
            {
                lblDupItems.Text = "请输入新词条";
                txtCode.Enabled = false;
                txtCode.Text = "";
                return;
            }

            // 编辑模式逻辑：如果输入的词和原始词一样，视为“合法”
            bool isSelf = word == _editingOriginalWord;
            var extCodes = _extItems
                .Where(i => i.Word == word && !isSelf)
                .Select(i => i.Code)
                .Distinct()
                .ToList();

            _mainDict.TryGetValue(word, out var mainCodes);

            if (extCodes.Count > 0 || (mainCodes != null && mainCodes.Count > 0))
            {
                var allCodes = new HashSet<string>(extCodes);
                if (mainCodes != null)
                    foreach (var c in mainCodes) allCodes.Add(c);

                lblDupItems.Text =
                    $"⚠ 词条已存在\r\n编码: {string.Join(", ", allCodes)}";

                lblDupItems.ForeColor = Color.Red;
                txtCode.Enabled = false;
                txtCode.Text = "";
            }
            else
            {
                lblDupItems.Text = "新词条，请输入编码";
                lblDupItems.ForeColor = Color.Green;
                txtCode.Enabled = true;

                // --- 结合自动推导逻辑 ---
                if (_isDictValid)
                {
                    // 如果推导为空（缺字或非法格式），Text 也会被置空，强制你手动输入
                    txtCode.Text = GenerateWubiCode(word);
                }
            }

            CheckCanSave();
        }


        // 监听编码框，决定是否允许点击确定
        private void txtCode_TextChanged(object sender, EventArgs e)
        {
            CheckCanSave();
            string original = txtCode.Text;

            // 过滤：转小写 + 只保留 a~z
            string filtered = new string(original
                .ToLower()
                .Where(c => c >= 'a' && c <= 'z')
                .Take(5)                // 限制最多5位
                .ToArray());

            if (original != filtered)
            {
                int cursor = txtCode.SelectionStart - (original.Length - filtered.Length);

                txtCode.Text = filtered;
                txtCode.SelectionStart = Math.Max(cursor, 0);
            }
        }

        private void txtCode_KeyPress(object sender, KeyPressEventArgs e)
        {
            // 允许控制键（退格、Delete等）
            if (char.IsControl(e.KeyChar))
                return;

            char c = char.ToLower(e.KeyChar);

            // 只允许 a~z
            if (c < 'a' || c > 'z')
            {
                e.Handled = true;
                return;
            }

            // 强制小写
            e.KeyChar = c;

            // 第二层长度保险（理论上 MaxLength 已经限制）
            if (txtCode.TextLength >= 15)
                e.Handled = true;
        }

        private void CheckCanSave()
        {
            // 逻辑：词条不为空 且 编码不为空 且 编码框处于启用状态
            btnOk.Enabled = !string.IsNullOrWhiteSpace(txtWord.Text) &&
                             !string.IsNullOrWhiteSpace(txtCode.Text) &&
                             txtCode.Enabled;
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            NewItem = new DictItem
            {
                Word = txtWord.Text.Trim(),
                Code = txtCode.Text.Trim().ToLower(),
                IsNew = true
            };
            this.DialogResult = DialogResult.OK;
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
