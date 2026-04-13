using System;
using System.Collections.Generic;
using System.Text;

namespace WeaselSettings
{
    public class ColorSchemeItem
    {
        public string Id { get; set; }   // 内部 ID, 如 "custom"
        public string Name { get; set; } // 显示名, 如 "自定义皮肤"

        public override string ToString() => Name;
    }
}
