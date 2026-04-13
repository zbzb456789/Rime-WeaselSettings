using System;
using System.Collections.Generic;
using System.Text;

namespace 词库管理工具
{
    // 用于在内存中存储单行词条
    public class DictItem
    {
        public string Code { get; set; }  // 编码
        public string Word { get; set; }  // 词条
        public int Weight { get; set; } = 0; // 权重 (可选)

        // 是否为新添加的行（用于UI逻辑，可选）
        public bool IsNew { get; set; } = false;
    }
}
