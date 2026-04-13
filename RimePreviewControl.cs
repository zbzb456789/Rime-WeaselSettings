using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Windows.Forms;


namespace WeaselSettings
{
    public class RimePreviewControl : UserControl
    {
        private RimeStyle? _style;
        // [新增] 用于存储解析后的自定义标签
        private string[] _customLabels = null;

        // 模拟预览数据
        private readonly string _preeditText = "aaaa";
        private readonly string _readingText = "^ 读音：gōng";
        private readonly (string Text, string Root)[] _cands = new[] {
            ("工", "[ 工工工工・aaaa・(43) ]"),
            ("恭恭敬敬", "[ 廿廿艹艹・aaaa ]"),
            ("藏匿", "[ 艹厂匚艹・aaaa ]"),
            ("花花草草", "[ 艹艹艹艹・aaaa ]"),
            ("㠭", "[ 工工工工・aaaa・(22068) ]")
        };

        public RimePreviewControl()
        {
            // 基础设置
            this.DoubleBuffered = true;
            this.AutoScroll = true; // 开启自动滚动
            this.SetStyle(ControlStyles.ResizeRedraw | ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint, true);
        }

        public void BindStyle(RimeStyle style)
        {
            _style = style;
            // 样式改变时触发重绘
            _style.StyleChanged += (s, e) => {
                this.Invalidate();
            };
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            if (_style == null) return;
            Graphics g = e.Graphics;

            // --- 核心修改：应用滚动偏移 ---
            // 当滚动条拉动时，AutoScrollPosition.X/Y 是负值，TranslateTransform 会移动整个画布
            g.TranslateTransform(this.AutoScrollPosition.X, this.AutoScrollPosition.Y);

            // 高质量绘图设置
            g.SmoothingMode = SmoothingMode.HighQuality;
            g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;

            // 根据 Rime 配置选择绘制引擎
            if (_style.VerticalText)
            {
                DrawVerticalLayout(g);
            }
            else
            {
                DrawHorizontalLayout(g);
            }
        }

        // ============================================================
        // 1. 标准布局 (Horizontal/Vertical List)
        // ============================================================
        private void DrawHorizontalLayout(Graphics g)
        {
            if (_style == null) return;

            using Font fMain = new Font(_style.FontFace, _style.FontPoint);
            using Font fLabel = new Font(_style.LabelFontFace, _style.LabelFontPoint);
            using Font fComment = new Font(_style.CommentFontFace, _style.CommentFontPoint);

            // ===============================
            // A. 尺寸测量
            // ===============================
            SizeF sPreedit = g.MeasureString(_preeditText, fMain);
            // [修改点 1]：测量 _readingText 时使用 fMain，使其跟随主字体大小
            SizeF sReading = g.MeasureString(_readingText, fMain);

            float preeditBoxW = sPreedit.Width + 8;
            float preeditBoxH = sPreedit.Height + 4;
            float preeditRowW = preeditBoxW + sReading.Width + 10;

            var measures = new (SizeF L, SizeF T, SizeF C, float W, float H)[_cands.Length];
            float candTotalW = 0, candTotalH = 0, maxItemH = 0;

            for (int i = 0; i < _cands.Length; i++)
            {
                string labelText = GetLabelText(i);
                SizeF sl = g.MeasureString(labelText, fLabel);
                SizeF st = g.MeasureString(_cands[i].Text, fMain);
                SizeF sc = g.MeasureString(_cands[i].Root, fComment);

                float w = sl.Width + st.Width + sc.Width + _style.HilitePaddingX * 2 + 10;
                float h = Math.Max(st.Height, Math.Max(sl.Height, sc.Height)) + _style.HilitePaddingY * 2;
                measures[i] = (sl, st, sc, w, h);

                if (_style.Horizontal)
                {
                    candTotalW += w + _style.CandidateSpacing - 10;
                    maxItemH = Math.Max(maxItemH, h);
                }
                else
                {
                    candTotalW = Math.Max(candTotalW, w);
                    candTotalH += h + _style.CandidateSpacing - 10;
                }
            }

            // ===============================
            // B. 窗口尺寸
            // ===============================
            float contentW = Math.Max(preeditRowW, candTotalW);
            float contentH = preeditBoxH + (_style.Spacing - 12) +
                             (_style.Horizontal ? maxItemH : candTotalH);

            int winW = Math.Clamp((int)(contentW + (_style.MarginX - 5) * 2),
                _style.MinWidth, _style.MaxWidth);
            int winH = Math.Clamp((int)(contentH + (_style.MarginY - 5) * 2),
                _style.MinHeight, _style.MaxHeight);

            UpdateScrollRange(winW, winH);

            RectangleF rectWin = new RectangleF(20, 20, winW, winH);

            // ===============================
            // C. 窗口基础（shadow_color / back_color / border_color）
            // ===============================
            DrawWindowBase(g, rectWin);

            float curX = rectWin.X + (_style.MarginX - 5);
            float curY = rectWin.Y + (_style.MarginY - 5);

            // ===============================
            // D. 编码区（hilited_*）
            // ===============================

            // hilited_shadow_color
            DrawItemShadow(
                g,
                new RectangleF(curX, curY, preeditBoxW, preeditBoxH),
                _style.hilited_shadow_color,
                _style.ShadowRadius
            );

            // hilited_back_color
            g.FillPath(
                new SolidBrush(_style.hilited_back_color),
                GetRoundedRect(new RectangleF(curX, curY, preeditBoxW, preeditBoxH), 8)
            );

            // hilited_text_color
            g.DrawString(_preeditText, fMain,
                new SolidBrush(_style.hilited_text_color),
                curX + 4, curY + 2);

            // [修改点 2]：绘制 _readingText 时使用 fMain
            g.DrawString(_readingText, fMain,
                new SolidBrush(_style.text_color),
                curX + preeditBoxW + 5, curY + 2);

            curY += preeditBoxH + (_style.Spacing - 12);

            // ===============================
            // E. 候选区（完整 19 色逻辑）
            // ===============================
            for (int i = 0; i < _cands.Length; i++)
            {
                var m = measures[i];
                bool isHi = (i == 0);

                RectangleF itemRect = new RectangleF(
                    curX,
                    curY,
                    _style.Horizontal ? m.W : (rectWin.Width - (_style.MarginX - 5) * 2),
                    m.H
                );

                if (isHi)
                {
                    // hilited_candidate_shadow_color
                    DrawItemShadow(g, itemRect, _style.hilited_candidate_shadow_color);

                    // hilited_candidate_back_color
                    g.FillPath(
                        new SolidBrush(_style.hilited_candidate_back_color),
                        GetRoundedRect(itemRect, 8)
                    );

                    // hilited_candidate_border_color
                    if (_style.hilited_candidate_border_color.A > 0)
                    {
                        using var p = new Pen(_style.hilited_candidate_border_color, 1);
                        g.DrawPath(p, GetRoundedRect(itemRect, 8));
                    }

                    // hilited_mark_color
                    if (_style.hilited_mark_color.A > 0)
                    {
                        g.FillRectangle(
                            new SolidBrush(_style.hilited_mark_color),
                            itemRect.X + 2,
                            itemRect.Y + 4,
                            3,
                            itemRect.Height - 8
                        );
                    }
                }
                else
                {
                    // candidate_shadow_color
                    DrawItemShadow(g, itemRect, _style.candidate_shadow_color);

                    // candidate_back_color
                    if (_style.candidate_back_color.A > 0)
                    {
                        g.FillPath(
                            new SolidBrush(_style.candidate_back_color),
                            GetRoundedRect(itemRect, 8)
                        );
                    }
                    // [新增] candidate_border_color: 绘制非选中候选词的边框
                    if (_style.candidate_border_color.A > 0)
                    {
                        using var p = new Pen(_style.candidate_border_color, 1);
                        g.DrawPath(p, GetRoundedRect(itemRect, 8));
                    }
                }

                float tx = itemRect.X + _style.HilitePaddingX;
                float ty = itemRect.Y + _style.HilitePaddingY;

                // [修改 2] 使用 GetLabelText(i) 进行绘制
                string labelText = GetLabelText(i);

                // label_color / hilited_label_color
                g.DrawString(labelText, fLabel,
                    new SolidBrush(isHi ? _style.hilited_label_color : _style.label_color),
                    tx, ty);

                // candidate_text_color / hilited_candidate_text_color
                g.DrawString(_cands[i].Text, fMain,
                    new SolidBrush(isHi ? _style.hilited_candidate_text_color : _style.candidate_text_color),
                    tx + m.L.Width, ty);

                // comment_text_color / hilited_comment_text_color
                g.DrawString(_cands[i].Root, fComment,
                    new SolidBrush(isHi ? _style.hilited_comment_text_color : _style.comment_text_color),
                    tx + m.L.Width + m.T.Width + 5, ty + 2);

                if (_style.Horizontal)
                    curX += m.W + _style.CandidateSpacing - 10;
                else
                    curY += m.H + _style.CandidateSpacing - 10;
            }
        }


        // ============================================================
        // 2. 直书布局 (VerticalText + AutoReverse)
        // ============================================================
        private void DrawVerticalLayout(Graphics g)
        {
            if (_style == null) return;

            g.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;

            using StringFormat fmt = new StringFormat(StringFormatFlags.DirectionVertical);
            using Font fMain = new Font(_style.FontFace, _style.FontPoint);
            using Font fLabel = new Font(_style.LabelFontFace, _style.LabelFontPoint);
            using Font fComment = new Font(_style.CommentFontFace, _style.CommentFontPoint);

            // =====================================================
            // A. 测量编码区
            // =====================================================
            SizeF sPre = g.MeasureString(_preeditText, fMain, 1000, fmt);
            SizeF sRead = g.MeasureString(_readingText, fComment, 1000, fmt);

            float preW = Math.Max(sPre.Width, sRead.Width) + _style.HilitePaddingX * 2;
            float preH = sPre.Height + sRead.Height + 10;

            // =====================================================
            // B. 测量候选
            // =====================================================
            var ms = new (SizeF L, SizeF T, SizeF C, float W, float H)[_cands.Length];
            float candTotalW = 0;
            float maxCandH = 0;
            for (int i = 0; i < _cands.Length; i++)
            {
                // [修改 1]
                string labelText = GetLabelText(i);
                SizeF sl = g.MeasureString(labelText, fLabel, 1000, fmt);
                SizeF st = g.MeasureString(_cands[i].Text, fMain, 1000, fmt);
                SizeF sc = g.MeasureString(_cands[i].Root, fComment, 1000, fmt);

                float w = Math.Max(st.Width, sc.Width) + _style.HilitePaddingX * 2;
                float h = sl.Height + st.Height + sc.Height + _style.HilitePaddingY * 2;

                ms[i] = (sl, st, sc, w, h);
                candTotalW += w + _style.CandidateSpacing - 10;
                // [新增] 追踪最高的一个候选词项
                if (h > maxCandH) maxCandH = h;
            }

            // =====================================================
            // C. 窗口尺寸
            // =====================================================
            int winW = Math.Clamp(
                (int)(preW + (_style.Spacing - 12) + candTotalW + (_style.MarginX - 5) * 2),
                _style.MinWidth,
                _style.MaxWidth
            );

            int winH = Math.Clamp(
                (int)(Math.Max(maxCandH, 200) + (_style.MarginY - 5) * 2),
                _style.MinHeight,
                _style.MaxHeight
            );

            UpdateScrollRange(winW, winH);

            RectangleF rectWin = new RectangleF(20, 20, winW, winH);

            // =====================================================
            // D. 窗口基础（shadow_color / back_color / border_color）
            // =====================================================
            DrawWindowBase(g, rectWin);

            bool ltr = _style.VerticalTextLeftToRight;
            bool isRev = _style.VerticalAutoReverse;

            float curX = ltr
                ? rectWin.X + (_style.MarginX - 5)
                : rectWin.Right - (_style.MarginX - 5) - preW;

            float contentTop = rectWin.Y + (_style.MarginY - 5);
            float contentBottom = rectWin.Bottom - (_style.MarginY - 5);

            // =====================================================
            // E. 编码区（hilited_*）
            // =====================================================
            RectangleF preRect = new RectangleF(curX, contentTop, preW, preH);

            // hilited_shadow_color
            DrawItemShadow(g, preRect, _style.hilited_shadow_color, _style.ShadowRadius);

            // hilited_back_color
            g.FillPath(
                new SolidBrush(_style.hilited_back_color),
                GetRoundedRect(preRect, 8)
            );

            // hilited_text_color
            g.DrawString(
                _preeditText,
                fMain,
                new SolidBrush(_style.hilited_text_color),
                curX + _style.HilitePaddingX,
                contentTop,
                fmt
            );

            // [修改点]：_readingText 使用 TextColor
            g.DrawString(
                _readingText,
                fComment,
                new SolidBrush(_style.text_color), // 修改此处
                curX + _style.HilitePaddingX,
                contentTop + sPre.Height + 5,
                fmt
            );

            curX += ltr ? (preW + (_style.Spacing - 12)) : -(_style.Spacing - 12);

            // =====================================================
            // F. 候选区（19 色完整逻辑）
            // =====================================================
            for (int i = 0; i < _cands.Length; i++)
            {
                var m = ms[i];
                bool isHi = (i == 0);

                if (!ltr) curX -= m.W;

                float itemY = isRev ? (contentBottom - m.H) : contentTop;
                RectangleF r = new RectangleF(curX, itemY, m.W, m.H);

                if (isHi)
                {
                    // hilited_candidate_shadow_color
                    DrawItemShadow(g, r, _style.hilited_candidate_shadow_color);

                    // hilited_candidate_back_color
                    g.FillPath(
                        new SolidBrush(_style.hilited_candidate_back_color),
                        GetRoundedRect(r, 6)
                    );

                    // hilited_candidate_border_color
                    if (_style.hilited_candidate_border_color.A > 0)
                    {
                        using var p = new Pen(_style.hilited_candidate_border_color, 1);
                        g.DrawPath(p, GetRoundedRect(r, 6));
                    }

                    // hilited_mark_color（竖排：顶部或底部条）
                    if (_style.hilited_mark_color.A > 0)
                    {
                        if (isRev)
                        {
                            g.FillRectangle(
                                new SolidBrush(_style.hilited_mark_color),
                                r.X + 4,
                                r.Bottom - 5,
                                r.Width - 8,
                                3
                            );
                        }
                        else
                        {
                            g.FillRectangle(
                                new SolidBrush(_style.hilited_mark_color),
                                r.X + 4,
                                r.Y + 2,
                                r.Width - 8,
                                3
                            );
                        }
                    }
                }
                else
                {
                    // candidate_shadow_color
                    DrawItemShadow(g, r, _style.candidate_shadow_color);

                    // candidate_back_color
                    if (_style.candidate_back_color.A > 0)
                    {
                        g.FillPath(
                            new SolidBrush(_style.candidate_back_color),
                            GetRoundedRect(r, 6)
                        );
                    }
                    // [新增] candidate_border_color: 绘制非选中候选词的边框
                    if (_style.candidate_border_color.A > 0)
                    {
                        using var p = new Pen(_style.candidate_border_color, 1);
                        g.DrawPath(p, GetRoundedRect(r, 6));
                    }
                }

                float textY = itemY + _style.HilitePaddingY;

                Color cLabel = isHi ? _style.hilited_label_color : _style.label_color;
                Color cText = isHi ? _style.hilited_candidate_text_color : _style.candidate_text_color;
                Color cCmt = isHi ? _style.hilited_comment_text_color : _style.comment_text_color;

                string labelText = GetLabelText(i);
                if (isRev)
                {
                    // 反序：注释 → 正文 → 序号
                    g.DrawString(_cands[i].Root, fComment, new SolidBrush(cCmt),
                        curX + (m.W - m.C.Width) / 2, textY, fmt);

                    g.DrawString(_cands[i].Text, fMain, new SolidBrush(cText),
                        curX + (m.W - m.T.Width) / 2, textY + m.C.Height, fmt);

                    g.DrawString(labelText, fLabel, new SolidBrush(cLabel), // <--- 这里修改
                        curX + (m.W - m.L.Width) / 2,
                        textY + m.C.Height + m.T.Height, fmt);
                }
                else
                {
                    // 正序：序号 → 正文 → 注释
                    g.DrawString(labelText, fLabel, new SolidBrush(cLabel), // <--- 这里修改
                        curX + (m.W - m.L.Width) / 2, textY, fmt);

                    g.DrawString(_cands[i].Text, fMain, new SolidBrush(cText),
                        curX + (m.W - m.T.Width) / 2, textY + m.L.Height, fmt);

                    g.DrawString(_cands[i].Root, fComment, new SolidBrush(cCmt),
                        curX + (m.W - m.C.Width) / 2,
                        textY + m.L.Height + m.T.Height, fmt);
                }

                if (ltr)
                    curX += m.W + _style.CandidateSpacing - 10;
                else
                    curX -= _style.CandidateSpacing - 10;
            }
        }


        // ============================================================
        // 公共辅助与绘制方法
        // ============================================================
        private void UpdateScrollRange(int winW, int winH)
        {
            // 更新虚拟画布大小，留出一定的 Margin (例如各 100)
            Size required = new Size(winW + 35, winH);
            if (this.AutoScrollMinSize != required)
            {
                this.AutoScrollMinSize = required;
            }
        }

        private void DrawWindowBase(Graphics g, RectangleF rect)
        {
            // --- 阴影与模糊算法 ---
            if (_style.shadow_color.A > 0 && _style.ShadowRadius > 0)
            {
                for (int i = _style.ShadowRadius; i > 0; i--)
                {
                    RectangleF sRect = rect;
                    sRect.Offset(_style.ShadowOffsetX, _style.ShadowOffsetY);
                    // 模糊核心：Inflate(i * 系数) 实现羽化边缘
                    sRect.Inflate(i * 2.0f, i * 2.0f);

                    // 透明度随层数衰减
                    int alpha = (int)(_style.shadow_color.A * (0.0 / i));
                    alpha = Math.Clamp(alpha, 8, 255);

                    using var b = new SolidBrush(Color.FromArgb(alpha, _style.shadow_color));
                    g.FillPath(b, GetRoundedRect(sRect, _style.CornerRadius + i));
                }
            }

            // --- 背景 ---
            g.FillPath(new SolidBrush(_style.back_color), GetRoundedRect(rect, _style.CornerRadius));

            // --- 边框 ---
            if (_style.BorderWidth > 0)
            {
                using var p = new Pen(_style.border_color, _style.BorderWidth) { Alignment = PenAlignment.Inset };
                g.DrawPath(p, GetRoundedRect(rect, _style.CornerRadius));
            }
        }
        private void DrawItemShadow(Graphics g, RectangleF rect, Color color, float radius = 6)
        {
            if (color.A == 0 || radius <= 0) return;

            for (int i = (int)radius; i > 0; i--)
            {
                RectangleF r = rect;
                r.Inflate(i * 1.8f, i * 1.8f);

                int alpha = (int)(color.A * (1f / (i + 18)));
                alpha = Math.Clamp(alpha, 4, color.A);

                using var b = new SolidBrush(Color.FromArgb(alpha, color));
                g.FillPath(b, GetRoundedRect(r, _style!.CornerRadius + i));
            }
        }

        private GraphicsPath GetRoundedRect(RectangleF rect, float radius)
        {
            GraphicsPath path = new GraphicsPath();
            float d = Math.Min(radius * 2, Math.Min(rect.Width, rect.Height));
            if (d <= 0) { path.AddRectangle(rect); return path; }
            path.AddArc(rect.X, rect.Y, d, d, 180, 90);
            path.AddArc(rect.Right - d, rect.Y, d, d, 270, 90);
            path.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0, 90);
            path.AddArc(rect.X, rect.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }

        // [新增] 公开方法：供外部调用设置标签
        public void SetSelectLabels(string rawValue)
        {
            if (string.IsNullOrWhiteSpace(rawValue) || rawValue.Trim() == "null")
            {
                _customLabels = null;
            }
            else
            {
                try
                {
                    // 去掉首尾的 [ 和 ]，然后按逗号分割
                    string clean = rawValue.Trim();
                    if (clean.StartsWith('[') && clean.EndsWith(']'))
                    {
                        clean = clean.Substring(1, clean.Length - 2);
                    }

                    _customLabels = clean.Split([','], StringSplitOptions.RemoveEmptyEntries)
                                         .Select(s => s.Trim()) // 去除每个元素两边的空格
                                         .ToArray();
                }
                catch
                {
                    _customLabels = null;
                }
            }
            // 触发重绘
            this.Invalidate();
        }

        // [新增] 内部辅助方法：获取第 i 个候选的标签文本
        private string GetLabelText(int index)
        {
            // 如果有自定义标签，且下标没越界，直接返回（通常不带点）
            if (_customLabels != null && index < _customLabels.Length)
            {
                // Rime 的逻辑：如果有自定义 label，直接显示字符，后面通常跟一个空格(由 padding 控制)
                // 这里如果不想要默认的点，就直接返回
                return _customLabels[index];
            }

            // 默认回退逻辑：显示 "1." "2."
            return $"{index + 1}.";
        }
    }
}