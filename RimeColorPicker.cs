using System;
using System.Collections.Generic;
using System.Text;
using static WeaselSettings.Form1;
using System.ComponentModel;

namespace WeaselSettings
{
    public class RimeColorPicker : UserControl
    {
        private readonly AlphaColorPanel _preview;
        private readonly TrackBar _alpha;
        private readonly Button _pick;
        public event EventHandler? CurrentColorChanged;


        private Color _currentColor = Color.Black;

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Color CurrentColor
        {
            get => _currentColor;
            set
            {
                if (_currentColor == value)
                    return;

                _currentColor = value;

                ApplyColor();                 // 更新 AlphaColorPanel
                OnCurrentColorChanged();      // 通知外部
            }
        }
        protected virtual void OnCurrentColorChanged()
        {
            CurrentColorChanged?.Invoke(this, EventArgs.Empty);
        }

        public RimeColorPicker()
        {
            _preview = new AlphaColorPanel { Width = 32, Dock = DockStyle.Left };
            _pick = new Button { Text = "…", Width = 28, Dock = DockStyle.Left };
            _alpha = new TrackBar
            {
                Minimum = 0,
                Maximum = 255,
                Value = 255,
                TickStyle = TickStyle.None,
                Width = 80,
                Dock = DockStyle.Fill // 改为 Fill，占据剩余所有空间
            };

            Controls.AddRange(new Control[] { _alpha, _pick, _preview });

            _pick.Click += OnPickColor;
            _alpha.ValueChanged += OnAlphaChanged;

            ApplyColor();
        }

        private void OnPickColor(object? sender, EventArgs e)
        {
            using var cd = new ColorDialog();
            cd.Color = Color.FromArgb(255, _currentColor);

            if (cd.ShowDialog() == DialogResult.OK)
            {
                CurrentColor = Color.FromArgb(_alpha.Value, cd.Color);
            }
        }

        private void OnAlphaChanged(object? sender, EventArgs e)
        {
            // 只有当滑块值确实与当前颜色的 Alpha 不同时才更新
            if (_currentColor.A != _alpha.Value)
            {
                CurrentColor = Color.FromArgb(_alpha.Value, _currentColor);
            }
        }

        private void ApplyColor()
        {
            // 1. 实时同步到 AlphaColorPanel 预览框
            _preview.DisplayColor = _currentColor;

            // 2. 反馈到滑块位置 (A 是 0-255 的值)
            if (_alpha.Value != _currentColor.A)
            {
                _alpha.Value = _currentColor.A;
            }
        }

        public string CurrentRimeColor => ToRimeColor(_currentColor);

        /// <summary>
        /// 将 System.Drawing.Color 转换为 Rime 要求的十六进制字符串
        /// 注意：Rime 颜色顺序是 BGR (蓝-绿-红)
        /// </summary>
        public static string ToRimeColor(Color color)
        {
            //// 如果不透明 (Alpha = 255)，通常 Rime 习惯简写为 0xBBGGRR
            //if (color.A == 255)
            //{
            //    return $"0x{color.B:X2}{color.G:X2}{color.R:X2}";
            //}

            // 如果有透明度，格式为 0xAABBGGRR
            return $"0x{color.A:X2}{color.B:X2}{color.G:X2}{color.R:X2}";
        }
    }

}
