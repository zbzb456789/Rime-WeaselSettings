using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Text;
using System.Linq;
using System.Windows.Forms;

namespace WeaselSettings
{
    // 数据模型 (AOT 安全，无反射)
    public class FontOption
    {
        public string? FontName { get; set; }
        public bool IsChecked { get; set; }
    }

    // 继承自 TextBox，使其在界面上看起来像一个输入框
    public class MultiFontComboBox : TextBox
    {
        private ToolStripDropDown? _dropDown;
        private ListBox? _listBox;
        private List<FontOption>? _fontList = new List<FontOption>();

        // 自定义事件：当用户勾选/取消勾选字体时触发，方便主窗体保存数据
        public event EventHandler? FontsChanged;

        public MultiFontComboBox()
        {
            // 让文本框只读，但背景保持白色，看起来像下拉框
            this.ReadOnly = true;
            this.BackColor = SystemColors.Window;
            this.Cursor = Cursors.Default;

            InitDropDown();
        }

        private void InitDropDown()
        {
            _listBox = new ListBox();
            _listBox.DrawMode = DrawMode.OwnerDrawFixed;

            _listBox.ItemHeight = 24;

            // 固定大小
            _listBox.Width = 320;
            _listBox.Height = 300;

            // 自动滚动
            _listBox.IntegralHeight = false;
            _listBox.ScrollAlwaysVisible = true;

            _listBox.SelectionMode = SelectionMode.One;
            _listBox.BorderStyle = BorderStyle.FixedSingle;

            _listBox.DrawItem += ListBox_DrawItem;
            _listBox.MouseClick += ListBox_MouseClick;

            ToolStripControlHost host = new ToolStripControlHost(_listBox);
            host.Margin = Padding.Empty;
            host.Padding = Padding.Empty;
            host.AutoSize = false;
            host.Size = _listBox.Size;

            _dropDown = new ToolStripDropDown();
            _dropDown.Padding = Padding.Empty;
            _dropDown.Items.Add(host);
        }


        /// <summary>
        /// 外部调用：加载系统字体，并根据传入的字符串（如"微软雅黑, Segoe UI"）打勾
        /// </summary>
        public void InitializeFonts(string initialFonts)
        {
            _listBox.Items.Clear();
            _fontList.Clear();

            var selectedList = string.IsNullOrWhiteSpace(initialFonts)
                ? new List<string>()
                : initialFonts.Split([','], StringSplitOptions.RemoveEmptyEntries)
                              .Select(f => f.Trim())
                              .ToList();

            using (InstalledFontCollection fonts = new InstalledFontCollection())
            {
                foreach (FontFamily family in fonts.Families.OrderBy(f => f.Name))
                {
                    _fontList.Add(new FontOption
                    {
                        FontName = family.Name,
                        IsChecked = selectedList.Contains(family.Name)
                    });
                }
            }

            ApplySort();

            this.Text = initialFonts;
        }
        private void ApplySort()
        {
            _fontList = _fontList
                .OrderByDescending(f => f.IsChecked) // 勾选优先
                //.ThenBy(f => f.FontName)             // 按名字排序
                .ToList();

            _listBox.BeginUpdate();
            _listBox.Items.Clear();

            foreach (var f in _fontList)
                _listBox.Items.Add(f);

            _listBox.EndUpdate();
        }


        // 重写点击事件：点击文本框时弹出列表
        protected override void OnClick(EventArgs e)
        {
            base.OnClick(e);
            if (_dropDown != null && !_dropDown.Visible)
            {
                _dropDown.Show(this, 0, this.Height);
            }
        }

        // 自绘：画出 CheckBox 和 字体预览
        private void ListBox_DrawItem(object? sender, DrawItemEventArgs e)
        {
            if (e.Index < 0) return;

            e.DrawBackground();

            FontOption item = (FontOption)_listBox.Items[e.Index];

            int checkboxSize = 16;
            int checkboxX = e.Bounds.X + 6;
            int checkboxY = e.Bounds.Y + (e.Bounds.Height - checkboxSize) / 2;

            // 画 checkbox
            ButtonState state = item.IsChecked ? ButtonState.Checked : ButtonState.Normal;
            ControlPaint.DrawCheckBox(e.Graphics, checkboxX, checkboxY, checkboxSize, checkboxSize, state);

            // 文本区域
            Rectangle textRect = new Rectangle(
                e.Bounds.X + 28,
                e.Bounds.Y,
                e.Bounds.Width - 28,
                e.Bounds.Height
            );

            Font previewFont;

            try
            {
                previewFont = new Font(item.FontName, 10f);
            }
            catch
            {
                previewFont = _listBox.Font;
            }

            using (previewFont)
            using (StringFormat sf = new StringFormat())
            {
                sf.LineAlignment = StringAlignment.Center;
                sf.Alignment = StringAlignment.Near;

                e.Graphics.DrawString(
                    item.FontName,
                    previewFont,
                    Brushes.Black,
                    textRect,
                    sf
                );
            }

            e.DrawFocusRectangle();
        }


        // 点击事件：改变勾选状态，不关闭下拉框
        private void ListBox_MouseClick(object? sender, MouseEventArgs e)
        {
            int index = _listBox.IndexFromPoint(e.Location);
            if (index >= 0)
            {
                FontOption item = (FontOption)_listBox.Items[index];

                item.IsChecked = !item.IsChecked;

                ApplySort();   // 重新排序

                var checkedFonts = _fontList
                    .Where(opt => opt.IsChecked)
                    .Select(opt => opt.FontName);

                this.Text = string.Join(", ", checkedFonts);

                FontsChanged?.Invoke(this, EventArgs.Empty);
            }
        }
    }
}