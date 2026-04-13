using WeaselSettings;

namespace 模糊音设置
{
    public partial class FuzzyPinyinForm : Form
    {
        private readonly YamlHelper _yamlHelper = new YamlHelper();
        private readonly string _configFileName = "pinyin.custom.yaml";
        private readonly string _fullPath;

        // 映射表：CheckBox 对应具体的 Rime 规则字符串
        private Dictionary<CheckBox, string[]> _ruleMap = new Dictionary<CheckBox, string[]>();

        public FuzzyPinyinForm()
        {
            InitializeComponent();
            _fullPath = Path.Combine(YamlHelper.GetUserDataDir(), _configFileName);
            InitRuleMap();
        }

        private static readonly string[] value1 = ["derive/^z([^h])/zh$1/", "derive/^zh/z/"];
        private static readonly string[] value2 = ["derive/^c([^h])/ch$1/", "derive/^ch/c/"];
        private static readonly string[] value3 = ["derive/^s([^h])/sh$1/", "derive/^sh/s/"];
        private static readonly string[] value4 = ["derive/^f/h/", "derive/^h/f/"];
        private static readonly string[] value5 = ["derive/^n/l/", "derive/^l/n/"];
        private static readonly string[] value6 = ["derive/^r/l/", "derive/^l/r/"];
        private static readonly string[] value7 = ["derive/^([^iuv]*)an$/$1ang/", "derive/^([^iuv]*)ang$/$1an/"];
        private static readonly string[] value8 = ["derive/en$/eng/", "derive/eng$/en/"];
        private static readonly string[] value9 = ["derive/in$/ing/", "derive/ing$/in/"];
        private static readonly string[] value10 = ["derive/ian$/iang/", "derive/iang$/ian/"];
        private static readonly string[] value11 = ["derive/uan$/uang/", "derive/uang$/uan/"];

        private void InitRuleMap()
        {
            _ruleMap = new Dictionary<CheckBox, string[]>
            {
                { chkZ_ZH, value1 },
                { chkC_CH, value2 },
                { chkS_SH, value3 },
                { chkF_H, value4 },
                { chkL_N, value5 },
                { chkR_L, value6 },
                { chkAn_Ang, value7 },
                { chkEn_Eng, value8 },
                { chkIn_Ing, value9 },
                { chkIan_Iang, value10 },
                { chkUan_Uang, value11 }
            };
        }

        private void FuzzyPinyinForm_Load(object sender, EventArgs e)
        {
            if (!File.Exists(_fullPath)) return;

            string currentAlgebra = _yamlHelper.GetNodeValue(_fullPath, "patch/speller/algebra", true);

            foreach (var kvp in _ruleMap)
            {
                // 只要当前配置包含规则组的第一条，就勾选
                kvp.Key.Checked = currentAlgebra.Contains(kvp.Value[0]);
            }
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            try
            {
                // 收集所有勾选的规则
                List<string> activeRules = new List<string>();
                foreach (var kvp in _ruleMap)
                {
                    if (kvp.Key.Checked)
                    {
                        activeRules.AddRange(kvp.Value);
                    }
                }

                string yamlListValue = "[" + string.Join(", ", activeRules) + "]";

                // 调用 ModifyNode 进行物理写入
                _yamlHelper.ModifyNode(_fullPath, "patch/speller/algebra", yamlListValue);
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"保存失败: {ex.Message}");
            }
        }

        private void btnReset_Click(object sender, EventArgs e)
        {
            foreach (var chk in _ruleMap.Keys) chk.Checked = false;
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
