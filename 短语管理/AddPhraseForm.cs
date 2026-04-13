using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace 短语管理工具
{
    public partial class AddWordForm : Form
    {
        public DictItem? NewItem { get; private set; }
        private readonly List<DictItem> _extItems;
        // 1. 增加一个私有字段记录正在编辑的项目
        private readonly DictItem? _editingItem;
        // 2. 修改构造函数，增加可选参数 targetItem
        public AddWordForm(List<DictItem> extItems, DictItem? targetItem = null)
        {
            InitializeComponent();
            _extItems = extItems;
            _editingItem = targetItem;

            if (_editingItem != null)
            {
                // 编辑模式初始化
                this.Text = "编辑短语";
                txtCode.Text = _editingItem.Code;
                // 将存储的 \n 换行符还原为文本框可见的换行
                txtWord.Text = _editingItem.Word.Replace("\\n", Environment.NewLine);

                lblDupItems.Text = "正在编辑现有短语";
                txtWord.Enabled = true;
            }
            else
            {
                lblDupItems.Text = "请先输入快捷编码";
            }
        }

        // 3. 修改编码改变时的逻辑：编辑模式下排除掉自己
        private void txtCode_TextChanged(object sender, EventArgs e)
        {
            string code = txtCode.Text.Trim().ToLower();

            if (string.IsNullOrEmpty(code))
            {
                lblDupItems.Text = "编码不能为空";
                lblDupItems.ForeColor = Color.Gray;
                txtWord.Enabled = false;
                CheckCanSave();
                return;
            }

            // 【关键修改】如果正在编辑且编码没变，不算重复
            bool isOriginalCode = _editingItem != null &&
                                 _editingItem.Code.Equals(code, StringComparison.OrdinalIgnoreCase);

            var duplicateItem = _extItems.FirstOrDefault(i =>
                !isOriginalCode && i.Code.Equals(code, StringComparison.OrdinalIgnoreCase));

            if (duplicateItem != null)
            {
                lblDupItems.Text = $"⚠ 编码已占用: {duplicateItem.Word}";
                lblDupItems.ForeColor = Color.Red;
                txtWord.Enabled = false;
            }
            else
            {
                lblDupItems.Text = isOriginalCode ? "✅ 原始编码（未修改）" : "✅ 编码可用，请输入短语内容";
                lblDupItems.ForeColor = Color.Green;
                txtWord.Enabled = true;
            }

            CheckCanSave();
        }

        // 2. 短语内容改变时只负责校验是否为空
        private void TxtWord_TextChanged(object sender, EventArgs e)
        {
            CheckCanSave();
        }

        private void CheckCanSave()
        {
            // 只有当编码可用（txtWord已启用）且短语不为空时，才允许确定
            btnOk.Enabled = txtWord.Enabled && !string.IsNullOrWhiteSpace(txtWord.Text);
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            // 处理多行输入，转换为 phrase.txt 识别的 \n 字面量
            string finalWord = txtWord.Text.Trim().Replace("\r\n", "\\n").Replace("\n", "\\n");

            NewItem = new DictItem
            {
                Word = finalWord,
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