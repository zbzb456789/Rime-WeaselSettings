using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace WeaselSettings
{
    public class AlphaColorPanel : Panel
    {
        private Color _displayColor = Color.Transparent;

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Color DisplayColor
        {
            get => _displayColor;
            set
            {
                if (_displayColor == value) return;
                _displayColor = value;
                Invalidate(); // 触发重绘
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            DrawChecker(e.Graphics, ClientRectangle);
            using var b = new SolidBrush(_displayColor);
            e.Graphics.FillRectangle(b, ClientRectangle);
            e.Graphics.DrawRectangle(Pens.Gray, 0, 0, Width - 1, Height - 1);
        }
        public static void DrawChecker(Graphics g, Rectangle rect)
        {
            const int size = 8;
            using var b1 = new SolidBrush(Color.LightGray);
            using var b2 = new SolidBrush(Color.White);

            for (int y = 0; y < rect.Height; y += size)
                for (int x = 0; x < rect.Width; x += size)
                {
                    bool odd = ((x / size) + (y / size)) % 2 == 1;
                    g.FillRectangle(odd ? b1 : b2,
                        rect.X + x, rect.Y + y, size, size);
                }
        }
    }

}
