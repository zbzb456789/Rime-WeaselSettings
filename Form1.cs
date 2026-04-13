using System.Diagnostics;
using System.Drawing.Text;
using System.IO.Compression;
using System.Text;
using 模糊音设置;
using 短语管理工具;
using 词库管理工具;

namespace WeaselSettings
{
    public partial class Form1 : Form
    {
        private readonly YamlHelper YamlHelper = new YamlHelper();
        private readonly string RimeUserDir = YamlHelper.GetUserDataDir();
        private RimeStyle _style = new RimeStyle();
        private bool _isInitializing = false; // 定义标志位
        private readonly string Zkey_Value = "[derive/^(...).$/$1z/, derive/^(..).(.*)$/$1z$2/, derive/^(.).(.*)$/$1z$2/]";
        public Form1()
        {
            InitializeComponent();

            InitSchemaPanel();
            LoadFontsToComboBox();
            LoadColorSchemesToComboBox();
            _ = YamlHelper.GetSchemaList(RimeUserDir);



            //// 初始化预览控件
            rimePreviewControl1.BindStyle(_style);

            // 初始化 UI 默认值 (同步 style 到设计器控件)
            //font_point_box.Value = _style.FontPoint;
            //Horizontal_box.Checked = _style.Horizontal;
            back_color_box.CurrentColor = _style.back_color;

        }

        public void InitSchemaPanel()
        {
            string defaultYamlPath = Path.Combine(RimeUserDir, "default.custom.yaml");

            // 1. 获取所有可用的方案 (目录下所有 .schema.yaml)
            var allSchemas = YamlHelper.GetSchemaList(RimeUserDir);

            // 2. 获取当前 default.yaml 中已启用的方案 ID
            var activeIds = YamlHelper.GetCurrentActiveSchemaIds(defaultYamlPath);

            flpSchemas.SuspendLayout();
            flpSchemas.Controls.Clear();

            foreach (var kvp in allSchemas)
            {
                string schemaId = kvp.Key;
                string schemaName = kvp.Value;

                CheckBox cb = new CheckBox
                {
                    Text = schemaName,
                    Tag = schemaId,
                    AutoSize = true,
                    Margin = new Padding(8),
                    // ★ 核心逻辑：如果在 activeIds 列表中，则勾选
                    Checked = activeIds.Contains(schemaId)
                };

                // 绑定事件：勾选或取消勾选时修改文件
                cb.CheckedChanged += SchemaCheckBox_CheckedChanged;

                flpSchemas.Controls.Add(cb);
            }

            flpSchemas.ResumeLayout();
        }

        // 统一的事件处理
        private void SchemaCheckBox_CheckedChanged(object? sender, EventArgs e)
        {
            if (sender is CheckBox cb && cb.Tag != null)
            {
                string schemaId = cb.Tag.ToString();
                bool isChecked = cb.Checked;

                // 获取路径，通常是用户文件夹下的 default.yaml
                string defaultYamlPath = Path.Combine(RimeUserDir, "default.custom.yaml");

                try
                {
                    YamlHelper.UpdateSchemaListAtPath(defaultYamlPath, schemaId, isChecked);

                    // 可选：状态栏提示
                    // toolStripStatusLabel1.Text = $"已{(isChecked ? "添加" : "移除")}方案: {cb.Text}";
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"更新配置文件失败: {ex.Message}");
                }
            }
        }

        private void LoadColorSchemesToComboBox()
        {
            color_scheme_box.Items.Clear();

            string yamlPath = Path.Combine(RimeUserDir, "weasel.yaml");
            var schemes = YamlHelper.GetColorSchemes(yamlPath);

            foreach (var kv in schemes)
            {
                color_scheme_box.Items.Add(new ColorSchemeItem
                {
                    Id = kv.Key,
                    Name = kv.Value
                });
                color_scheme_dark_box.Items.Add(new ColorSchemeItem
                {
                    Id = kv.Key,
                    Name = kv.Value
                });
            }

            string customPath = Path.Combine(RimeUserDir, "weasel.custom.yaml");
            string currentSchemeId = YamlHelper.GetNodeValue(customPath, "patch/style/color_scheme");
            string currentSchemeId1 = YamlHelper.GetNodeValue(customPath, "patch/style/color_scheme_dark");

            int targetIndex = -1;
            int targetIndex1 = -1;
            for (int i = 0; i < color_scheme_dark_box.Items.Count; i++)
            {
                if (color_scheme_dark_box.Items[i] is ColorSchemeItem item && item.Id == currentSchemeId1)
                {
                    targetIndex1 = i;
                    break;
                }
            }
            for (int i = 0; i < color_scheme_box.Items.Count; i++)
            {
                if (color_scheme_box.Items[i] is ColorSchemeItem item && item.Id == currentSchemeId)
                {
                    targetIndex = i;
                    break;
                }
            }

            color_scheme_dark_box.SelectedIndex = targetIndex1 >= 0 ? targetIndex1 : color_scheme_dark_box.Items.Count > 0 ? 0 : -1;
            color_scheme_box.SelectedIndex = targetIndex >= 0 ? targetIndex : color_scheme_box.Items.Count > 0 ? 0 : -1;

            _style.StyleChanged += (_, _) =>
            {
                text_color_box.CurrentColor = _style.text_color;
                back_color_box.CurrentColor = _style.back_color;
                border_color_box.CurrentColor = _style.border_color;
                shadow_color_box.CurrentColor = _style.shadow_color;
                hilited_back_color_box.CurrentColor = _style.hilited_back_color;
                hilited_text_color_box.CurrentColor = _style.hilited_text_color;
                hilited_shadow_color_box.CurrentColor = _style.hilited_shadow_color;
                comment_text_color_box.CurrentColor = _style.comment_text_color;
                hilited_mark_color_box.CurrentColor = _style.hilited_mark_color;
                hilited_candidate_back_color_box.CurrentColor = _style.hilited_candidate_back_color;
                hilited_candidate_shadow_color_box.CurrentColor = _style.hilited_candidate_shadow_color;
                hilited_label_color_box.CurrentColor = _style.hilited_label_color;
                hilited_candidate_text_color_box.CurrentColor = _style.hilited_candidate_text_color;
                hilited_candidate_border_color_box.CurrentColor = _style.hilited_candidate_border_color;
                hilited_comment_text_color_box.CurrentColor = _style.hilited_comment_text_color;
                candidate_back_color_box.CurrentColor = _style.candidate_back_color;
                candidate_text_color_box.CurrentColor = _style.candidate_text_color;
                candidate_shadow_color_box.CurrentColor = _style.candidate_shadow_color;
                label_color_box.CurrentColor = _style.label_color;
                candidate_border_color_box.CurrentColor = _style.candidate_border_color;
            };
        }


        private void color_scheme_box_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_isInitializing) return; // 如果正在加载，不跑下面的修改代码
            if (color_scheme_box.SelectedItem is not ColorSchemeItem item)
                return;

            string schemeId = item.Id;   // custom / aqua
            YamlHelper.ModifyNode(Path.Combine(RimeUserDir, "weasel.custom.yaml"), "patch/style/color_scheme", schemeId);
            YamlHelper.ApplyColorSchemeToStyle(Path.Combine(RimeUserDir, "weasel.yaml"), item.Id, _style);

        }
        private void color_scheme_dark_box_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_isInitializing) return; // 如果正在加载，不跑下面的修改代码
            if (color_scheme_dark_box.SelectedItem is not ColorSchemeItem item)
                return;

            string schemeId = item.Id;   // custom / aqua
            YamlHelper.ModifyNode(Path.Combine(RimeUserDir, "weasel.custom.yaml"), "patch/style/color_scheme_dark", schemeId);
            YamlHelper.ApplyColorSchemeToStyle(Path.Combine(RimeUserDir, "weasel.yaml"), item.Id, _style);
        }

        private void LoadFontsToComboBox()
        {
            // 从 YAML 获取当前的字体设置
            string customPath = Path.Combine(RimeUserDir, "weasel.custom.yaml");

            string currentFontFace = YamlHelper.GetNodeValue(customPath, "patch/style/font_face");
            string currentLabelFont = YamlHelper.GetNodeValue(customPath, "patch/style/label_font_face");
            string currentCommentFont = YamlHelper.GetNodeValue(customPath, "patch/style/comment_font_face");

            // 初始化控件 (内部会自动获取系统字体并勾选对应项)
            font_face_box.InitializeFonts(currentFontFace);
            label_font_face_box.InitializeFonts(currentLabelFont);
            comment_font_face_box.InitializeFonts(currentCommentFont);

            // 绑定事件，当用户在下拉框里打勾时，立刻保存
            font_face_box.FontsChanged += (s, e) => SaveFontConfig("font_face", font_face_box.Text);
            label_font_face_box.FontsChanged += (s, e) => SaveFontConfig("label_font_face", label_font_face_box.Text);
            comment_font_face_box.FontsChanged += (s, e) => SaveFontConfig("comment_font_face", comment_font_face_box.Text);
        }

        // 统一的保存逻辑
        private void SaveFontConfig(string key, string newFontString)
        {
            if (_isInitializing) return;

            string customPath = Path.Combine(RimeUserDir, "weasel.custom.yaml");

            // 更新你的内部模型 (如果有的话)
            if (key == "font_face") _style.FontFace = newFontString;
            else if (key == "label_font_face") _style.LabelFontFace = newFontString;
            else if (key == "comment_font_face") _style.CommentFontFace = newFontString;

            _style.TriggerChange(); // 触发界面预览更新

            // 写入 YAML
            YamlHelper.ModifyNode(customPath, $"patch/style/{key}", newFontString);
        }

        // 辅助方法：安全设置 ComboBox 的选中项
        private void SetComboSelection(ComboBox box, string fontName)
        {
            if (!string.IsNullOrEmpty(fontName) && box.Items.Contains(fontName))
            {
                box.SelectedItem = fontName;
            }
            else if (box.Items.Count > 0)
            {
                box.SelectedIndex = 0; // 如果找不到，默认选第一个
            }
        }
        private void Form1_Shown(object sender, EventArgs e)
        {
            LoadSettings();
        }

        private void LoadSettings()
        {
            _isInitializing = true; // 【核心】开始设置 UI 前，封锁事件触发
            lblStatus.Text = "正在读取配置...";

            try
            {
                // --- 1. 读取所有配置项 ---
                // 建议：如果值为空，给个默认值（比如 "false" 或 "0"），防止 Parse 失败
                string currentSchema1 = YamlHelper.GetNodeValue(Path.Combine(RimeUserDir, "user.yaml"), "var/previously_selected_schema");
                string currentSchema2 = YamlHelper.GetNodeValue(Path.Combine(RimeUserDir, "wubi.dict.yaml"), "version");

                // 开关类 (1/0)
                string ascii_mode1 = YamlHelper.GetNodeValue(Path.Combine(RimeUserDir, "wubi_pinyin.custom.yaml"), "patch/switches/@0/reset");
                string ascii_mode2 = YamlHelper.GetNodeValue(Path.Combine(RimeUserDir, "wubi.custom.yaml"), "patch/switches/@0/reset");
                string zh_trad1 = YamlHelper.GetNodeValue(Path.Combine(RimeUserDir, "wubi_pinyin.custom.yaml"), "patch/switches/@1/reset");
                string zh_trad2 = YamlHelper.GetNodeValue(Path.Combine(RimeUserDir, "wubi.custom.yaml"), "patch/switches/@1/reset");
                string full_shape1 = YamlHelper.GetNodeValue(Path.Combine(RimeUserDir, "wubi_pinyin.custom.yaml"), "patch/switches/@2/reset");
                string full_shape2 = YamlHelper.GetNodeValue(Path.Combine(RimeUserDir, "wubi.custom.yaml"), "patch/switches/@2/reset");
                string ascii_punct1 = YamlHelper.GetNodeValue(Path.Combine(RimeUserDir, "wubi_pinyin.custom.yaml"), "patch/switches/@3/reset");
                string ascii_punct2 = YamlHelper.GetNodeValue(Path.Combine(RimeUserDir, "wubi.custom.yaml"), "patch/switches/@3/reset");
                string single_char1 = YamlHelper.GetNodeValue(Path.Combine(RimeUserDir, "wubi_pinyin.custom.yaml"), "patch/switches/@4/reset");
                string single_char2 = YamlHelper.GetNodeValue(Path.Combine(RimeUserDir, "wubi.custom.yaml"), "patch/switches/@4/reset");
                string GB2312_1 = YamlHelper.GetNodeValue(Path.Combine(RimeUserDir, "wubi_pinyin.custom.yaml"), "patch/switches/@5/reset");
                string GB2312_2 = YamlHelper.GetNodeValue(Path.Combine(RimeUserDir, "wubi.custom.yaml"), "patch/switches/@5/reset");
                string show_spelling1 = YamlHelper.GetNodeValue(Path.Combine(RimeUserDir, "wubi_pinyin.custom.yaml"), "patch/switches/@6/reset");
                string show_spelling2 = YamlHelper.GetNodeValue(Path.Combine(RimeUserDir, "wubi.custom.yaml"), "patch/switches/@6/reset");
                string show_code1 = YamlHelper.GetNodeValue(Path.Combine(RimeUserDir, "wubi_pinyin.custom.yaml"), "patch/switches/@7/reset");
                string show_code2 = YamlHelper.GetNodeValue(Path.Combine(RimeUserDir, "wubi.custom.yaml"), "patch/switches/@7/reset");
                string show_pinyin1 = YamlHelper.GetNodeValue(Path.Combine(RimeUserDir, "wubi_pinyin.custom.yaml"), "patch/switches/@8/reset");
                string show_pinyin2 = YamlHelper.GetNodeValue(Path.Combine(RimeUserDir, "wubi.custom.yaml"), "patch/switches/@8/reset");
                string show_index1 = YamlHelper.GetNodeValue(Path.Combine(RimeUserDir, "wubi_pinyin.custom.yaml"), "patch/switches/@9/reset");
                string show_index2 = YamlHelper.GetNodeValue(Path.Combine(RimeUserDir, "wubi.custom.yaml"), "patch/switches/@9/reset");
                string show_es1 = YamlHelper.GetNodeValue(Path.Combine(RimeUserDir, "wubi_pinyin.custom.yaml"), "patch/switches/@10/reset");
                string show_es2 = YamlHelper.GetNodeValue(Path.Combine(RimeUserDir, "wubi.custom.yaml"), "patch/switches/@10/reset");
                string Zkey_wildcard = YamlHelper.GetNodeValue(Path.Combine(RimeUserDir, "wubi.custom.yaml"), "patch/speller/algebra");
                string Zkey_repetition = YamlHelper.GetNodeValue(Path.Combine(RimeUserDir, "wubi.custom.yaml"), "patch/history/input");
                string Zkey_pinyin = YamlHelper.GetNodeValue(Path.Combine(RimeUserDir, "wubi.custom.yaml"), "patch/rvlk1/prefix");
                string Return_esc = YamlHelper.GetNodeValue(Path.Combine(RimeUserDir, "wubi.custom.yaml"), "patch/key_binder/bindings/@next");
                string space_screen = YamlHelper.GetNodeValue(Path.Combine(RimeUserDir, "wubi.custom.yaml"), "patch/key_binder/bindings/@next1");
                string auto_select = YamlHelper.GetNodeValue(Path.Combine(RimeUserDir, "wubi.custom.yaml"), "patch/speller/auto_select");
                string max_code_length = YamlHelper.GetNodeValue(Path.Combine(RimeUserDir, "wubi.custom.yaml"), "patch/speller/max_code_length");
                string enable_encoder = YamlHelper.GetNodeValue(Path.Combine(RimeUserDir, "wubi.custom.yaml"), "patch/translator/enable_encoder");
                string user_dict1 = YamlHelper.GetNodeValue(Path.Combine(RimeUserDir, "wubi.custom.yaml"), "patch/translator/dictionary");
                string user_dict2 = YamlHelper.GetNodeValue(Path.Combine(RimeUserDir, "wubi.custom.yaml"), "patch/fixed/dictionary");
                string user_dict3 = YamlHelper.GetNodeValue(Path.Combine(RimeUserDir, "wubi.custom.yaml"), "patch/mkst/dictionary");
                string user_dict4 = YamlHelper.GetNodeValue(Path.Combine(RimeUserDir, "wubi.custom.yaml"), "patch/sentence_input/dictionary");
                string user_dict5 = YamlHelper.GetNodeValue(Path.Combine(RimeUserDir, "wubi.custom.yaml"), "patch/ci_reverse_lookup/dictionary");
                string enable_completion = YamlHelper.GetNodeValue(Path.Combine(RimeUserDir, "wubi.custom.yaml"), "patch/translator/enable_completion");



                // 布尔/数字类 (true/false, numbers)
                string currentSchema16 = YamlHelper.GetNodeValue(Path.Combine(RimeUserDir, "wubi.custom.yaml"), "patch/translator/enable_user_dict");
                string currentSchema17 = YamlHelper.GetNodeValue(Path.Combine(RimeUserDir, "default.custom.yaml"), "patch/menu/page_size");
                string currentSchema18 = YamlHelper.GetNodeValue(Path.Combine(RimeUserDir, "weasel.custom.yaml"), "patch/style/inline_preedit");
                string currentSchema19 = YamlHelper.GetNodeValue(Path.Combine(RimeUserDir, "weasel.custom.yaml"), "patch/style/font_point");
                string currentSchema20 = YamlHelper.GetNodeValue(Path.Combine(RimeUserDir, "weasel.custom.yaml"), "patch/style/label_font_point");
                string currentSchema21 = YamlHelper.GetNodeValue(Path.Combine(RimeUserDir, "weasel.custom.yaml"), "patch/style/comment_font_point");
                string currentSchema22 = YamlHelper.GetNodeValue(Path.Combine(RimeUserDir, "weasel.custom.yaml"), "patch/style/horizontal");
                string currentSchema23 = YamlHelper.GetNodeValue(Path.Combine(RimeUserDir, "weasel.custom.yaml"), "patch/style/vertical_text");
                string currentSchema24 = YamlHelper.GetNodeValue(Path.Combine(RimeUserDir, "weasel.custom.yaml"), "patch/style/vertical_text_left_to_right");
                string currentSchema25 = YamlHelper.GetNodeValue(Path.Combine(RimeUserDir, "default.custom.yaml"), "patch/menu/alternative_select_labels");
                string click_to_capture = YamlHelper.GetNodeValue(Path.Combine(RimeUserDir, "weasel.custom.yaml"), "patch/style/click_to_capture");
                rimePreviewControl1.SetSelectLabels(currentSchema25);
                // --- 2. 安全地设置 UI 状态 ---
                // 使用 TryParse 或逻辑比较，防止 YAML 为空时 Parse 报错
                Horizontal_box.Checked = (currentSchema22?.ToLower() == "true");
                vertical_text_box.Checked = (currentSchema23?.ToLower() == "true");
                vertical_text_left_to_right_Box.Checked = (currentSchema24?.ToLower() == "true");
                inline_preedit_box.Checked = (currentSchema18?.ToLower() == "true");
                enable_user_dict_box.Checked = (currentSchema16?.ToLower() == "true");
                auto_select_box.Checked = (auto_select?.ToLower() == "true");
                enable_encoder_box.Checked = (enable_encoder?.ToLower() == "true");
                click_to_capture_box.Checked = (click_to_capture?.ToLower() == "true");
                enable_completion_box.Checked = (enable_completion?.ToLower() == "true");

                if (byte.TryParse(currentSchema17, out byte ps)) page_size_box.Value = ps;
                if (byte.TryParse(currentSchema19, out byte fp)) font_point_box.Value = fp;
                if (byte.TryParse(currentSchema20, out byte lfp)) label_font_point_box.Value = lfp;
                if (byte.TryParse(currentSchema21, out byte cfp)) comment_font_point_box.Value = cfp;

                // 设置 RadioButtons (之前选择的方案)
                SetRadioButtonByTag(this.panel1, currentSchema1);
                SetRadioButtonByTag(this.panel2, currentSchema2);

                // 设置各种 reset 开关 (1=Checked)
                radioButton10.Checked = (ascii_mode1 == "1" || ascii_mode2 == "1");
                radioButton11.Checked = !radioButton10.Checked;

                radioButton12.Checked = (zh_trad1 == "1" || zh_trad2 == "1");
                radioButton13.Checked = !radioButton12.Checked;

                radioButton15.Checked = (full_shape1 == "1" || full_shape2 == "1");
                radioButton16.Checked = !radioButton15.Checked;

                radioButton14.Checked = (single_char1 == "1" || single_char2 == "1");
                radioButton9.Checked = !radioButton14.Checked;

                ascii_punct_box.Checked = (ascii_punct1 == "1" || ascii_punct2 == "1");
                show_spelling_box.Checked = (show_spelling1 == "1" || show_spelling2 == "1");
                new_hide_pinyin_box.Checked = (show_pinyin1 == "1" || show_pinyin2 == "1");
                show_code_box.Checked = (show_code1 == "1" || show_code2 == "1");
                show_index_box.Checked = (show_index1 == "1" || show_index2 == "1");
                GB2312_box.Checked = (GB2312_1 == "1" || GB2312_2 == "1");
                show_es_box.Checked = (show_es1 == "1" || show_es2 == "1");
                if (Zkey_wildcard == Zkey_Value)
                {
                    Zkey_wildcard_box.Checked = true;
                }
                else
                {
                    Zkey_wildcard_box.Checked = false;
                }
                if (Zkey_repetition == "z")
                {
                    Zkey_repetition_box.Checked = true;
                }
                else
                {
                    Zkey_repetition_box.Checked = false;
                }
                if (Zkey_pinyin == "[]")
                {
                    Zkey_pinyin_box.Checked = false;
                }
                else
                {
                    Zkey_pinyin_box.Checked = true;
                }
                if (Return_esc == "{}")
                {
                    Return_esc_box.Checked = false;
                    null_return_box.Checked = false;
                }
                else if (Return_esc == "{accept: Return, send: Escape, when: has_menu}")
                {
                    null_return_box.Checked = true;
                    Return_esc_box.Checked = true;
                    null_return_box.Enabled = true;
                }
                else if (Return_esc == "{accept: Return, send: Escape, when: composing}")
                {
                    Return_esc_box.Checked = true;
                }
                if (space_screen == "{}")
                {
                    space_screen_box.Checked = false;
                }
                else
                {
                    space_screen_box.Checked = true;
                }

                if (max_code_length == "4")
                {
                    max_code_length_box.Checked = true;
                }
                else
                {
                    max_code_length_box.Checked = false;
                }
                if (user_dict1 == "wubi" || user_dict2 == "wubi" || user_dict3 == "wubi" || user_dict4 == "wubi" || user_dict5 == "wubi")
                {
                    user_dict_box.Checked = false;
                }
                else
                {
                    user_dict_box.Checked = true;
                }



                lblStatus.Text = "配置读取完成";
            }
            catch (Exception ex)
            {
                lblStatus.Text = "读取失败";
                MessageBox.Show("初始化失败: " + ex.Message);
            }
            finally
            {
                _isInitializing = false; // 【核心】无论成功失败，最后必须允许事件触发
            }
        }

        // 辅助方法：减少重复的 RadioButton 遍历逻辑
        private void SetRadioButtonByTag(Control container, string tagValue)
        {
            if (string.IsNullOrEmpty(tagValue)) return;
            foreach (Control c in container.Controls)
            {
                if (c is RadioButton rb && rb.Tag?.ToString() == tagValue)
                {
                    rb.Checked = true;
                    break;
                }
            }
        }
        private async void Save_Settings_box_Click(object sender, EventArgs e)
        {
            Save_Settings_box.Enabled = false;
            lblStatus.Text = "正在保存配置...";
            lblStatus.ForeColor = Color.Blue;

            try
            {
                lblStatus.Text = "正在通知输入法引擎更新...";
                await Task.Run(() =>
                {
                    YamlHelper.RunDeploy();
                    System.Threading.Thread.Sleep(500); // 假装等一下，让用户感觉后台在工作
                });

                lblStatus.Text = "设置已生效！";
                lblStatus.ForeColor = Color.Green;
                MessageBox.Show("设置已保存！", "小狼毫设置", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                lblStatus.Text = "保存失败";
                lblStatus.ForeColor = Color.Red;
                MessageBox.Show("保存时发生错误: " + ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Save_Settings_box.Enabled = true;
                Close();
            }
        }

        private async void Redeploy_box_Click(object sender, EventArgs e)
        {
            await Task.Run(() =>
            {
                YamlHelper.RunDeploy();
                System.Threading.Thread.Sleep(500); // 假装等一下，让用户感觉后台在工作
            });
        }

        private void radioButton6_CheckedChanged(object sender, EventArgs e)
        {
            if (_isInitializing) return; // 如果正在加载，不跑下面的修改代码

            // 因为 CheckedChanged 会在“取消选中”和“被选中”时各触发一次
            // 我们只需要处理“被选中”的那一个
            if (sender is RadioButton rb && rb.Checked)
            {
                string userYamlPath = Path.Combine(RimeUserDir, "user.yaml");
                string schemaId = "";

                // 根据按钮的名字或 Tag 来判断要修改成哪个方案
                // 建议在设计器里把 RadioButton 的 Tag 属性分别设为 wubi, wubi_pinyin, pinyin
                switch (rb.Tag?.ToString())
                {
                    case "wubi":
                        schemaId = "wubi";
                        break;
                    case "wubi_pinyin":
                        schemaId = "wubi_pinyin";
                        break;
                    case "luna_pinyin_simp":
                        schemaId = "luna_pinyin_simp";
                        break;
                }

                if (!string.IsNullOrEmpty(schemaId))
                {
                    // 执行修改
                    YamlHelper.ModifyNode(userYamlPath, "var/previously_selected_schema", schemaId);

                    // 提示
                    //Console.WriteLine($"已将默认方案修改为: {schemaId}");
                }
            }
        }
        private void rbVersion_CheckedChanged(object sender, EventArgs e)
        {
            //if (_isInitializing) return; // 如果正在加载，不跑下面的修改代码
            // AOT 兼容：确保 sender 是 RadioButton 且处于选中状态
            if (sender is RadioButton rb && rb.Checked)
            {
                string? versionTag = rb.Tag?.ToString();
                if (versionTag == "custom")
                {
                    button1.Enabled = true;
                    button3.Enabled = true;
                    label32.Visible = true;
                    flpSchemas.Visible = true;
                }
                if (!string.IsNullOrEmpty(versionTag))
                {
                    // 调用上面写好的替换函数
                    ReplaceSchemaFiles(versionTag);
                }
            }
            else
            {
                button1.Enabled = false;
                button3.Enabled = false;
                label32.Visible = false;
                flpSchemas.Visible = false;
            }
        }
        private void ReplaceSchemaFiles(string versionDir)
        {
            Task.Run(async () =>
            {
                try
                {
                    // 1. 定义路径（假设程序运行目录下有 tables 文件夹）
                    string sourceBase = Path.Combine(RimeUserDir, "tables", versionDir);
                    // rimeUserDir 是你之前定义的 Rime 用户目录路径
                    string targetBase = RimeUserDir;

                    if (!Directory.Exists(sourceBase))
                    {
                        MessageBox.Show($"源目录不存在: {sourceBase}");
                        return;
                    }

                    // 2. 获取源目录下所有文件并覆盖复制
                    string[] files = Directory.GetFiles(sourceBase);
                    foreach (string file in files)
                    {
                        string fileName = Path.GetFileName(file);
                        string destFile = Path.Combine(targetBase, fileName);

                        // 执行复制，true 表示允许覆盖已有文件
                        File.Copy(file, destFile, true);
                    }

                    //Console.WriteLine($"已成功替换 {versionDir} 版方案文件");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"文件替换失败: {ex.Message}");
                }
            });
        }


        private void radioButton11_CheckedChanged(object sender, EventArgs e)
        {
            if (_isInitializing) return; // 如果正在加载，不跑下面的修改代码
            if (radioButton11.Checked)
            {
                YamlHelper.ModifyNode(Path.Combine(RimeUserDir, "wubi_pinyin.custom.yaml"), "patch/switches/@0/reset", "0");
                YamlHelper.ModifyNode(Path.Combine(RimeUserDir, "wubi.custom.yaml"), "patch/switches/@0/reset", "0");
                YamlHelper.ModifyNode(Path.Combine(RimeUserDir, "pinyin.custom.yaml"), "patch/switches/@0/reset", "0");
            }
            else
            {
                YamlHelper.ModifyNode(Path.Combine(RimeUserDir, "wubi_pinyin.custom.yaml"), "patch/switches/@0/reset", "1");
                YamlHelper.ModifyNode(Path.Combine(RimeUserDir, "wubi.custom.yaml"), "patch/switches/@0/reset", "1");
                YamlHelper.ModifyNode(Path.Combine(RimeUserDir, "pinyin.custom.yaml"), "patch/switches/@0/reset", "1");
            }
        }

        private void radioButton13_CheckedChanged(object sender, EventArgs e)
        {
            if (_isInitializing) return; // 如果正在加载，不跑下面的修改代码
            if (radioButton13.Checked)
            {
                YamlHelper.ModifyNode(Path.Combine(RimeUserDir, "wubi_pinyin.custom.yaml"), "patch/switches/@1/reset", "0");
                YamlHelper.ModifyNode(Path.Combine(RimeUserDir, "wubi.custom.yaml"), "patch/switches/@1/reset", "0");
                YamlHelper.ModifyNode(Path.Combine(RimeUserDir, "pinyin.custom.yaml"), "patch/switches/@1/reset", "0");
            }
            else
            {
                YamlHelper.ModifyNode(Path.Combine(RimeUserDir, "wubi_pinyin.custom.yaml"), "patch/switches/@1/reset", "1");
                YamlHelper.ModifyNode(Path.Combine(RimeUserDir, "wubi.custom.yaml"), "patch/switches/@1/reset", "1");
                YamlHelper.ModifyNode(Path.Combine(RimeUserDir, "pinyin.custom.yaml"), "patch/switches/@1/reset", "1");
            }
        }

        private void radioButton16_CheckedChanged(object sender, EventArgs e)
        {
            if (_isInitializing) return; // 如果正在加载，不跑下面的修改代码
            if (radioButton16.Checked)
            {
                YamlHelper.ModifyNode(Path.Combine(RimeUserDir, "wubi_pinyin.custom.yaml"), "patch/switches/@2/reset", "0");
                YamlHelper.ModifyNode(Path.Combine(RimeUserDir, "wubi.custom.yaml"), "patch/switches/@2/reset", "0");
                YamlHelper.ModifyNode(Path.Combine(RimeUserDir, "pinyin.custom.yaml"), "patch/switches/@2/reset", "0");
            }
            else
            {
                YamlHelper.ModifyNode(Path.Combine(RimeUserDir, "wubi_pinyin.custom.yaml"), "patch/switches/@2/reset", "1");
                YamlHelper.ModifyNode(Path.Combine(RimeUserDir, "wubi.custom.yaml"), "patch/switches/@2/reset", "1");
                YamlHelper.ModifyNode(Path.Combine(RimeUserDir, "pinyin.custom.yaml"), "patch/switches/@2/reset", "1");
            }
        }
        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (_isInitializing) return; // 如果正在加载，不跑下面的修改代码
            if (ascii_punct_box.Checked)
            {
                YamlHelper.ModifyNode(Path.Combine(RimeUserDir, "wubi_pinyin.custom.yaml"), "patch/switches/@3/reset", "1");
                YamlHelper.ModifyNode(Path.Combine(RimeUserDir, "wubi.custom.yaml"), "patch/switches/@3/reset", "1");
                YamlHelper.ModifyNode(Path.Combine(RimeUserDir, "pinyin.custom.yaml"), "patch/switches/@3/reset", "1");
            }
            else
            {
                YamlHelper.ModifyNode(Path.Combine(RimeUserDir, "wubi_pinyin.custom.yaml"), "patch/switches/@3/reset", "0");
                YamlHelper.ModifyNode(Path.Combine(RimeUserDir, "wubi.custom.yaml"), "patch/switches/@3/reset", "0");
                YamlHelper.ModifyNode(Path.Combine(RimeUserDir, "pinyin.custom.yaml"), "patch/switches/@3/reset", "0");
            }
        }


        private void radioButton14_CheckedChanged(object sender, EventArgs e)
        {
            if (_isInitializing) return; // 如果正在加载，不跑下面的修改代码
            if (radioButton14.Checked)
            {
                YamlHelper.ModifyNode(Path.Combine(RimeUserDir, "wubi_pinyin.custom.yaml"), "patch/switches/@4/reset", "1");
                YamlHelper.ModifyNode(Path.Combine(RimeUserDir, "wubi.custom.yaml"), "patch/switches/@4/reset", "1");
                YamlHelper.ModifyNode(Path.Combine(RimeUserDir, "pinyin.custom.yaml"), "patch/switches/@4/reset", "1");
            }
            else
            {
                YamlHelper.ModifyNode(Path.Combine(RimeUserDir, "wubi_pinyin.custom.yaml"), "patch/switches/@4/reset", "0");
                YamlHelper.ModifyNode(Path.Combine(RimeUserDir, "wubi.custom.yaml"), "patch/switches/@4/reset", "0");
                YamlHelper.ModifyNode(Path.Combine(RimeUserDir, "pinyin.custom.yaml"), "patch/switches/@4/reset", "0");
            }
        }

        private void GB2312_box_CheckedChanged(object sender, EventArgs e)
        {
            if (_isInitializing) return; // 如果正在加载，不跑下面的修改代码
            if (GB2312_box.Checked)
            {
                YamlHelper.ModifyNode(Path.Combine(RimeUserDir, "wubi_pinyin.custom.yaml"), "patch/switches/@5/reset", "1");
                YamlHelper.ModifyNode(Path.Combine(RimeUserDir, "wubi.custom.yaml"), "patch/switches/@5/reset", "1");
            }
            else
            {
                YamlHelper.ModifyNode(Path.Combine(RimeUserDir, "wubi_pinyin.custom.yaml"), "patch/switches/@5/reset", "0");
                YamlHelper.ModifyNode(Path.Combine(RimeUserDir, "wubi.custom.yaml"), "patch/switches/@5/reset", "0");
            }
        }

        private void show_spelling_box_CheckedChanged(object sender, EventArgs e)
        {
            if (_isInitializing) return; // 如果正在加载，不跑下面的修改代码
            if (show_spelling_box.Checked)
            {
                YamlHelper.ModifyNode(Path.Combine(RimeUserDir, "wubi_pinyin.custom.yaml"), "patch/switches/@6/reset", "1");
                YamlHelper.ModifyNode(Path.Combine(RimeUserDir, "wubi.custom.yaml"), "patch/switches/@6/reset", "1");
            }
            else
            {
                YamlHelper.ModifyNode(Path.Combine(RimeUserDir, "wubi_pinyin.custom.yaml"), "patch/switches/@6/reset", "0");
                YamlHelper.ModifyNode(Path.Combine(RimeUserDir, "wubi.custom.yaml"), "patch/switches/@6/reset", "0");
            }
        }
        private void show_code_box_CheckedChanged(object sender, EventArgs e)
        {
            if (_isInitializing) return; // 如果正在加载，不跑下面的修改代码
            if (show_code_box.Checked)
            {
                YamlHelper.ModifyNode(Path.Combine(RimeUserDir, "wubi_pinyin.custom.yaml"), "patch/switches/@7/reset", "1");
                YamlHelper.ModifyNode(Path.Combine(RimeUserDir, "wubi.custom.yaml"), "patch/switches/@7/reset", "1");
            }
            else
            {
                YamlHelper.ModifyNode(Path.Combine(RimeUserDir, "wubi_pinyin.custom.yaml"), "patch/switches/@7/reset", "0");
                YamlHelper.ModifyNode(Path.Combine(RimeUserDir, "wubi.custom.yaml"), "patch/switches/@7/reset", "0");
            }
        }



        private void new_hide_pinyin_box_CheckedChanged(object sender, EventArgs e)
        {
            if (_isInitializing) return; // 如果正在加载，不跑下面的修改代码
            if (new_hide_pinyin_box.Checked)
            {
                YamlHelper.ModifyNode(Path.Combine(RimeUserDir, "wubi_pinyin.custom.yaml"), "patch/switches/@8/reset", "1");
                YamlHelper.ModifyNode(Path.Combine(RimeUserDir, "wubi.custom.yaml"), "patch/switches/@8/reset", "1");
            }
            else
            {
                YamlHelper.ModifyNode(Path.Combine(RimeUserDir, "wubi_pinyin.custom.yaml"), "patch/switches/@8/reset", "0");
                YamlHelper.ModifyNode(Path.Combine(RimeUserDir, "wubi.custom.yaml"), "patch/switches/@8/reset", "0");
            }
        }
        private void show_index_box_CheckedChanged(object sender, EventArgs e)
        {
            if (_isInitializing) return; // 如果正在加载，不跑下面的修改代码
            if (show_index_box.Checked)
            {
                YamlHelper.ModifyNode(Path.Combine(RimeUserDir, "wubi_pinyin.custom.yaml"), "patch/switches/@9/reset", "1");
                YamlHelper.ModifyNode(Path.Combine(RimeUserDir, "wubi.custom.yaml"), "patch/switches/@9/reset", "1");
            }
            else
            {
                YamlHelper.ModifyNode(Path.Combine(RimeUserDir, "wubi_pinyin.custom.yaml"), "patch/switches/@9/reset", "0");
                YamlHelper.ModifyNode(Path.Combine(RimeUserDir, "wubi.custom.yaml"), "patch/switches/@9/reset", "0");
            }
        }

        private void show_es_box_CheckedChanged(object sender, EventArgs e)
        {
            if (_isInitializing) return; // 如果正在加载，不跑下面的修改代码
            if (show_es_box.Checked)
            {
                YamlHelper.ModifyNode(Path.Combine(RimeUserDir, "wubi_pinyin.custom.yaml"), "patch/switches/@10/reset", "1");
                YamlHelper.ModifyNode(Path.Combine(RimeUserDir, "wubi.custom.yaml"), "patch/switches/@10/reset", "1");
                YamlHelper.ModifyNode(Path.Combine(RimeUserDir, "pinyin.custom.yaml"), "patch/switches/@5/reset", "1");
            }
            else
            {
                YamlHelper.ModifyNode(Path.Combine(RimeUserDir, "wubi_pinyin.custom.yaml"), "patch/switches/@10/reset", "0");
                YamlHelper.ModifyNode(Path.Combine(RimeUserDir, "wubi.custom.yaml"), "patch/switches/@10/reset", "0");
                YamlHelper.ModifyNode(Path.Combine(RimeUserDir, "pinyin.custom.yaml"), "patch/switches/@5/reset", "0");
            }
        }

        private void Zkey_wildcard_box_CheckedChanged(object sender, EventArgs e)
        {
            if (_isInitializing) return; // 如果正在加载，不跑下面的修改代码

            if (Zkey_wildcard_box.Checked)
            {
                YamlHelper.ModifyNode(Path.Combine(RimeUserDir, "wubi.custom.yaml"), "patch/speller/algebra", Zkey_Value);
            }
            else
            {
                YamlHelper.ModifyNode(Path.Combine(RimeUserDir, "wubi.custom.yaml"), "patch/speller/algebra", "[]");
            }
        }

        private void Zkey_repetition_box_CheckedChanged(object sender, EventArgs e)
        {
            if (_isInitializing) return; // 如果正在加载，不跑下面的修改代码

            if (Zkey_repetition_box.Checked)
            {
                YamlHelper.ModifyNode(Path.Combine(RimeUserDir, "wubi.custom.yaml"), "patch/history/input", "z");
            }
            else
            {
                YamlHelper.ModifyNode(Path.Combine(RimeUserDir, "wubi.custom.yaml"), "patch/history/input", "[]");
            }
        }

        private void Zkey_pinyin_box_CheckedChanged(object sender, EventArgs e)
        {
            if (_isInitializing) return; // 如果正在加载，不跑下面的修改代码
            if (Zkey_pinyin_box.Checked)
            {
                YamlHelper.ModifyNode(Path.Combine(RimeUserDir, "wubi.custom.yaml"), "patch/rvlk1/prefix", "`");
            }
            else
            {
                YamlHelper.ModifyNode(Path.Combine(RimeUserDir, "wubi.custom.yaml"), "patch/rvlk1/prefix", "[]");
            }
        }

        private void Return_esc_box_CheckedChanged(object sender, EventArgs e)
        {
            if (_isInitializing) return; // 如果正在加载，不跑下面的修改代码

            if (Return_esc_box.Checked)
            {
                YamlHelper.ModifyNode(Path.Combine(RimeUserDir, "wubi.custom.yaml"), "patch/key_binder/bindings/@next", "{accept: Return, send: Escape, when: composing}");
                YamlHelper.ModifyNode(Path.Combine(RimeUserDir, "wubi_pinyin.custom.yaml"), "patch/key_binder/bindings/@next", "{accept: Return, send: Escape, when: composing}");
                null_return_box.Enabled = true;
            }
            else
            {
                YamlHelper.ModifyNode(Path.Combine(RimeUserDir, "wubi.custom.yaml"), "patch/key_binder/bindings/@next", "{}");
                YamlHelper.ModifyNode(Path.Combine(RimeUserDir, "wubi_pinyin.custom.yaml"), "patch/key_binder/bindings/@next", "{}");
                null_return_box.Enabled = false;
            }
        }

        private void space_screen_box_CheckedChanged(object sender, EventArgs e)
        {
            if (_isInitializing) return; // 如果正在加载，不跑下面的修改代码

            if (space_screen_box.Checked)
            {
                YamlHelper.ModifyNode(Path.Combine(RimeUserDir, "wubi.custom.yaml"), "patch/key_binder/bindings/@next1", "{accept: space, send: Shift+Return, when: composing}");
                YamlHelper.ModifyNode(Path.Combine(RimeUserDir, "wubi_pinyin.custom.yaml"), "patch/key_binder/bindings/@next1", "{accept: space, send: Shift+Return, when: composing}");
            }
            else
            {
                YamlHelper.ModifyNode(Path.Combine(RimeUserDir, "wubi.custom.yaml"), "patch/key_binder/bindings/@next1", "{}");
                YamlHelper.ModifyNode(Path.Combine(RimeUserDir, "wubi_pinyin.custom.yaml"), "patch/key_binder/bindings/@next1", "{}");
            }
        }


        private void null_return_box_CheckedChanged(object sender, EventArgs e)
        {
            if (_isInitializing) return; // 如果正在加载，不跑下面的修改代码

            if (null_return_box.Checked)
            {
                YamlHelper.ModifyNode(Path.Combine(RimeUserDir, "wubi.custom.yaml"), "patch/key_binder/bindings/@next", "{accept: Return, send: Escape, when: has_menu}");
                YamlHelper.ModifyNode(Path.Combine(RimeUserDir, "wubi_pinyin.custom.yaml"), "patch/key_binder/bindings/@next", "{accept: Return, send: Escape, when: has_menu}");
            }
            else
            {
                YamlHelper.ModifyNode(Path.Combine(RimeUserDir, "wubi.custom.yaml"), "patch/key_binder/bindings/@next", "{accept: Return, send: Escape, when: composing}");
                YamlHelper.ModifyNode(Path.Combine(RimeUserDir, "wubi_pinyin.custom.yaml"), "patch/key_binder/bindings/@next", "{accept: Return, send: Escape, when: composing}");
            }
        }

        private void auto_select_box_CheckedChanged(object sender, EventArgs e)
        {
            if (_isInitializing) return; // 如果正在加载，不跑下面的修改代码

            if (auto_select_box.Checked)
            {
                YamlHelper.ModifyNode(Path.Combine(RimeUserDir, "wubi.custom.yaml"), "patch/speller/auto_select", "true");
            }
            else
            {
                YamlHelper.ModifyNode(Path.Combine(RimeUserDir, "wubi.custom.yaml"), "patch/speller/auto_select", "false");
            }
        }

        private void max_code_length_box_CheckedChanged(object sender, EventArgs e)
        {
            if (_isInitializing) return; // 如果正在加载，不跑下面的修改代码

            if (max_code_length_box.Checked)
            {
                YamlHelper.ModifyNode(Path.Combine(RimeUserDir, "wubi.custom.yaml"), "patch/speller/max_code_length", "4");
            }
            else
            {
                YamlHelper.ModifyNode(Path.Combine(RimeUserDir, "wubi.custom.yaml"), "patch/speller/max_code_length", "0");
            }
        }


        private void enable_user_dict_box_CheckedChanged(object sender, EventArgs e)
        {
            if (_isInitializing) return; // 如果正在加载，不跑下面的修改代码
            if (enable_user_dict_box.Checked)
            {
                YamlHelper.ModifyNode(Path.Combine(RimeUserDir, "wubi.custom.yaml"), "patch/translator/enable_user_dict", "true");
                YamlHelper.ModifyNode(Path.Combine(RimeUserDir, "wubi_pinyin.custom.yaml"), "patch/translator/enable_user_dict", "true");
                //YamlHelper.ModifyNode(Path.Combine(RimeUserDir, "pinyin.custom.yaml"), "patch/translator/enable_user_dict", "true");
            }
            else
            {
                YamlHelper.ModifyNode(Path.Combine(RimeUserDir, "wubi.custom.yaml"), "patch/translator/enable_user_dict", "false");
                YamlHelper.ModifyNode(Path.Combine(RimeUserDir, "wubi_pinyin.custom.yaml"), "patch/translator/enable_user_dict", "false");
                //YamlHelper.ModifyNode(Path.Combine(RimeUserDir, "pinyin.custom.yaml"), "patch/translator/enable_user_dict", "false");
            }
        }

        private void enable_encoder_box_CheckedChanged(object sender, EventArgs e)
        {
            if (_isInitializing) return; // 如果正在加载，不跑下面的修改代码
            if (enable_encoder_box.Checked)
            {
                YamlHelper.ModifyNode(Path.Combine(RimeUserDir, "wubi.custom.yaml"), "patch/translator/enable_encoder", "true");
                YamlHelper.ModifyNode(Path.Combine(RimeUserDir, "wubi_pinyin.custom.yaml"), "patch/translator/enable_encoder", "true");
                YamlHelper.ModifyNode(Path.Combine(RimeUserDir, "wubi.custom.yaml"), "patch/translator/encode_commit_history", "true");
                YamlHelper.ModifyNode(Path.Combine(RimeUserDir, "wubi_pinyin.custom.yaml"), "patch/translator/encode_commit_history", "true");
            }
            else
            {
                YamlHelper.ModifyNode(Path.Combine(RimeUserDir, "wubi.custom.yaml"), "patch/translator/enable_encoder", "false");
                YamlHelper.ModifyNode(Path.Combine(RimeUserDir, "wubi_pinyin.custom.yaml"), "patch/translator/enable_encoder", "false");
                YamlHelper.ModifyNode(Path.Combine(RimeUserDir, "wubi.custom.yaml"), "patch/translator/encode_commit_history", "false");
                YamlHelper.ModifyNode(Path.Combine(RimeUserDir, "wubi_pinyin.custom.yaml"), "patch/translator/encode_commit_history", "false");
            }
        }

        private void Horizontal_box_CheckedChanged(object sender, EventArgs e)
        {
            //if (_isInitializing) return; // 如果正在加载，不跑下面的修改代码
            if (Horizontal_box.Checked)
            {
                _style.Horizontal = true;
                vertical_text_box.Checked = false;
                vertical_text_left_to_right_Box.Checked = false;
                _style.TriggerChange();
                YamlHelper.ModifyNode(Path.Combine(RimeUserDir, "weasel.custom.yaml"), "patch/style/horizontal", "true");
                YamlHelper.ModifyNode(Path.Combine(RimeUserDir, "weasel.custom.yaml"), "patch/style/vertical_text", "false");
            }
            else
            {
                _style.Horizontal = false;
                vertical_text_left_to_right_Box.Checked = false;
                _style.TriggerChange();
                YamlHelper.ModifyNode(Path.Combine(RimeUserDir, "weasel.custom.yaml"), "patch/style/horizontal", "false");
            }
        }

        private void vertical_text_box_CheckedChanged(object sender, EventArgs e)
        {
            if (_isInitializing) return; // 如果正在加载，不跑下面的修改代码
            if (vertical_text_box.Checked)
            {
                _style.VerticalText = true;
                Horizontal_box.Checked = false;
                _style.TriggerChange();
                YamlHelper.ModifyNode(Path.Combine(RimeUserDir, "weasel.custom.yaml"), "patch/style/vertical_text", "true");
                YamlHelper.ModifyNode(Path.Combine(RimeUserDir, "weasel.custom.yaml"), "patch/style/horizontal", "false");
            }
            else
            {
                _style.VerticalText = false;
                _style.TriggerChange();
                YamlHelper.ModifyNode(Path.Combine(RimeUserDir, "weasel.custom.yaml"), "patch/style/vertical_text", "false");
            }
        }

        private void click_to_capture_box_CheckedChanged(object sender, EventArgs e)
        {
            if (_isInitializing) return; // 如果正在加载，不跑下面的修改代码
            if (click_to_capture_box.Checked)
            {
                YamlHelper.ModifyNode(Path.Combine(RimeUserDir, "weasel.custom.yaml"), "patch/style/click_to_capture", "true");
            }
            else
            {
                YamlHelper.ModifyNode(Path.Combine(RimeUserDir, "weasel.custom.yaml"), "patch/style/click_to_capture", "false");
            }
        }


        private void vertical_text_left_to_right_Box_CheckedChanged(object sender, EventArgs e)
        {
            if (_isInitializing) return; // 如果正在加载，不跑下面的修改代码
            if (vertical_text_left_to_right_Box.Checked)
            {
                _style.VerticalTextLeftToRight = true;
                _style.TriggerChange();
                YamlHelper.ModifyNode(Path.Combine(RimeUserDir, "weasel.custom.yaml"), "patch/style/vertical_text_left_to_right", "true");
            }
            else
            {
                _style.VerticalTextLeftToRight = false;
                _style.TriggerChange();
                YamlHelper.ModifyNode(Path.Combine(RimeUserDir, "weasel.custom.yaml"), "patch/style/vertical_text_left_to_right", "false");
            }
        }

        private void enable_completion_box_CheckedChanged(object sender, EventArgs e)
        {
            if (_isInitializing) return; // 如果正在加载，不跑下面的修改代码
            if (enable_completion_box.Checked)
            {
                YamlHelper.ModifyNode(Path.Combine(RimeUserDir, "wubi.custom.yaml"), "patch/translator/enable_completion", "true");
                YamlHelper.ModifyNode(Path.Combine(RimeUserDir, "wubi_pinyin.custom.yaml"), "patch/translator/enable_completion", "true");
                YamlHelper.ModifyNode(Path.Combine(RimeUserDir, "pinyin.custom.yaml"), "patch/ci_reverse_lookup/enable_completion", "true");
            }
            else
            {
                YamlHelper.ModifyNode(Path.Combine(RimeUserDir, "wubi.custom.yaml"), "patch/translator/enable_completion", "false");
                YamlHelper.ModifyNode(Path.Combine(RimeUserDir, "wubi_pinyin.custom.yaml"), "patch/translator/enable_completion", "false");
                YamlHelper.ModifyNode(Path.Combine(RimeUserDir, "pinyin.custom.yaml"), "patch/ci_reverse_lookup/enable_completion", "false");
            }
        }


        private void font_face_box_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (_isInitializing) return; // 如果正在加载，不跑下面的修改代码
            if (e.Index < 0) return;

            // 1. 绘制背景（处理选中和非选中状态）
            e.DrawBackground();

            ComboBox combo = (ComboBox)sender;
            string fontName = combo.Items[e.Index].ToString();

            // 2. 确定文字颜色（选中时通常为白色，未选中为黑色）
            Brush textBrush = new SolidBrush(e.ForeColor);

            try
            {
                // 3. 创建预览字体
                // 建议增加 FontStyle.Regular 确保兼容性，或者直接使用默认样式
                using (Font previewFont = new Font(fontName, combo.Font.Size))
                {
                    e.Graphics.DrawString(fontName, previewFont, textBrush, e.Bounds);
                }
            }
            catch
            {
                // 4. 回退逻辑：如果该字体损坏或不支持当前样式，用红字标识
                e.Graphics.DrawString(fontName, combo.Font, Brushes.Red, e.Bounds);
            }
            finally
            {
                textBrush.Dispose();
            }

            // 5. 绘制聚焦框
            e.DrawFocusRectangle();
        }

        private void font_point_box_ValueChanged(object sender, EventArgs e)
        {
            if (_isInitializing) return; // 如果正在加载，不跑下面的修改代码
            _style.FontPoint = (byte)font_point_box.Value;
            _style.TriggerChange();
            YamlHelper.ModifyNode(Path.Combine(RimeUserDir, "weasel.custom.yaml"), "patch/style/font_point", font_point_box.Value.ToString());
        }

        private void label_font_point_box_ValueChanged(object sender, EventArgs e)
        {
            if (_isInitializing) return; // 如果正在加载，不跑下面的修改代码
            _style.LabelFontPoint = (byte)label_font_point_box.Value;
            _style.TriggerChange();
            YamlHelper.ModifyNode(Path.Combine(RimeUserDir, "weasel.custom.yaml"), "patch/style/label_font_point", label_font_point_box.Value.ToString());
        }

        private void comment_font_point_box_ValueChanged(object sender, EventArgs e)
        {
            if (_isInitializing) return; // 如果正在加载，不跑下面的修改代码
            _style.CommentFontPoint = (byte)comment_font_point_box.Value;
            _style.TriggerChange();
            YamlHelper.ModifyNode(Path.Combine(RimeUserDir, "weasel.custom.yaml"), "patch/style/comment_font_point", comment_font_point_box.Value.ToString());
        }

        private void text_color_box_CurrentColorChanged(object sender, EventArgs e)
        {
            if (_isInitializing) return; // 如果正在加载，不跑下面的修改代码
            if (color_scheme_box.SelectedIndex == 0)
            {
                _style.text_color = text_color_box.CurrentColor;
                _style.TriggerChange();
                YamlHelper.ModifyNode(Path.Combine(RimeUserDir, "weasel.yaml"), "preset_color_schemes/custom/text_color", text_color_box.CurrentRimeColor);
            }
            else
            {
                _style.text_color = text_color_box.CurrentColor;
                _style.TriggerChange();
            }
        }

        private void back_color_box_CurrentColorChanged(object sender, EventArgs e)
        {
            if (_isInitializing) return; // 如果正在加载，不跑下面的修改代码
            if (color_scheme_box.SelectedIndex == 0)
            {
                _style.back_color = back_color_box.CurrentColor;
                _style.TriggerChange();
                YamlHelper.ModifyNode(Path.Combine(RimeUserDir, "weasel.yaml"), "preset_color_schemes/custom/back_color", back_color_box.CurrentRimeColor);

            }
            else
            {
                _style.back_color = back_color_box.CurrentColor;
                _style.TriggerChange();
            }

        }

        private void border_color_box_CurrentColorChanged(object sender, EventArgs e)
        {
            if (_isInitializing) return; // 如果正在加载，不跑下面的修改代码
            if (color_scheme_box.SelectedIndex == 0)
            {
                _style.border_color = border_color_box.CurrentColor;
                _style.TriggerChange();
                YamlHelper.ModifyNode(Path.Combine(RimeUserDir, "weasel.yaml"), "preset_color_schemes/custom/border_color", border_color_box.CurrentRimeColor);
            }
            else
            {
                _style.border_color = border_color_box.CurrentColor;
                _style.TriggerChange();
            }
        }

        private void shadow_color_box_CurrentColorChanged(object sender, EventArgs e)
        {
            if (_isInitializing) return; // 如果正在加载，不跑下面的修改代码
            if (color_scheme_box.SelectedIndex == 0)
            {
                _style.shadow_color = shadow_color_box.CurrentColor;
                _style.TriggerChange();
                YamlHelper.ModifyNode(Path.Combine(RimeUserDir, "weasel.yaml"), "preset_color_schemes/custom/shadow_color", shadow_color_box.CurrentRimeColor);
            }
            else
            {
                _style.shadow_color = shadow_color_box.CurrentColor;
                _style.TriggerChange();
            }
        }

        private void hilited_back_color_box_CurrentColorChanged(object sender, EventArgs e)
        {
            if (_isInitializing) return; // 如果正在加载，不跑下面的修改代码
            if (color_scheme_box.SelectedIndex == 0)
            {
                _style.hilited_back_color = hilited_back_color_box.CurrentColor;
                _style.TriggerChange();
                YamlHelper.ModifyNode(Path.Combine(RimeUserDir, "weasel.yaml"), "preset_color_schemes/custom/hilited_back_color", hilited_back_color_box.CurrentRimeColor);
            }
            else
            {
                _style.hilited_back_color = hilited_back_color_box.CurrentColor;
                _style.TriggerChange();
            }
        }

        private void hilited_text_color_box_CurrentColorChanged(object sender, EventArgs e)
        {
            if (_isInitializing) return; // 如果正在加载，不跑下面的修改代码
            if (color_scheme_box.SelectedIndex == 0)
            {
                _style.hilited_text_color = hilited_text_color_box.CurrentColor;
                _style.TriggerChange();
                YamlHelper.ModifyNode(Path.Combine(RimeUserDir, "weasel.yaml"), "preset_color_schemes/custom/hilited_text_color", hilited_text_color_box.CurrentRimeColor);
            }
            else
            {
                _style.hilited_text_color = hilited_text_color_box.CurrentColor;
                _style.TriggerChange();
            }
        }

        private void hilited_shadow_color_box_CurrentColorChanged(object sender, EventArgs e)
        {
            if (_isInitializing) return; // 如果正在加载，不跑下面的修改代码
            if (color_scheme_box.SelectedIndex == 0)
            {
                _style.hilited_shadow_color = hilited_shadow_color_box.CurrentColor;
                _style.TriggerChange();
                YamlHelper.ModifyNode(Path.Combine(RimeUserDir, "weasel.yaml"), "preset_color_schemes/custom/hilited_shadow_color", hilited_shadow_color_box.CurrentRimeColor);
            }
            else
            {
                _style.hilited_shadow_color = hilited_shadow_color_box.CurrentColor;
                _style.TriggerChange();
            }
        }

        private void comment_text_color_box_CurrentColorChanged(object sender, EventArgs e)
        {
            if (_isInitializing) return; // 如果正在加载，不跑下面的修改代码
            if (color_scheme_box.SelectedIndex == 0)
            {
                _style.comment_text_color = comment_text_color_box.CurrentColor;
                _style.TriggerChange();
                YamlHelper.ModifyNode(Path.Combine(RimeUserDir, "weasel.yaml"), "preset_color_schemes/custom/comment_text_color", comment_text_color_box.CurrentRimeColor);
            }
            else
            {
                _style.comment_text_color = comment_text_color_box.CurrentColor;
                _style.TriggerChange();
            }
        }

        private void hilited_mark_color_box_CurrentColorChanged(object sender, EventArgs e)
        {
            if (_isInitializing) return; // 如果正在加载，不跑下面的修改代码
            if (color_scheme_box.SelectedIndex == 0)
            {
                _style.hilited_mark_color = hilited_mark_color_box.CurrentColor;
                _style.TriggerChange();
                YamlHelper.ModifyNode(Path.Combine(RimeUserDir, "weasel.yaml"), "preset_color_schemes/custom/hilited_mark_color", hilited_mark_color_box.CurrentRimeColor);
            }
            else
            {
                _style.hilited_mark_color = hilited_mark_color_box.CurrentColor;
                _style.TriggerChange();
            }
        }

        private void hilited_candidate_back_color_box_CurrentColorChanged(object sender, EventArgs e)
        {
            if (_isInitializing) return; // 如果正在加载，不跑下面的修改代码
            if (color_scheme_box.SelectedIndex == 0)
            {
                _style.hilited_candidate_back_color = hilited_candidate_back_color_box.CurrentColor;
                _style.TriggerChange();
                YamlHelper.ModifyNode(Path.Combine(RimeUserDir, "weasel.yaml"), "preset_color_schemes/custom/hilited_candidate_back_color", hilited_candidate_back_color_box.CurrentRimeColor);
            }
            else
            {
                _style.hilited_candidate_back_color = hilited_candidate_back_color_box.CurrentColor;
                _style.TriggerChange();
            }
        }

        private void hilited_candidate_shadow_color_box_CurrentColorChanged(object sender, EventArgs e)
        {
            if (_isInitializing) return; // 如果正在加载，不跑下面的修改代码
            if (color_scheme_box.SelectedIndex == 0)
            {
                _style.hilited_candidate_shadow_color = hilited_candidate_shadow_color_box.CurrentColor;
                _style.TriggerChange();
                YamlHelper.ModifyNode(Path.Combine(RimeUserDir, "weasel.yaml"), "preset_color_schemes/custom/hilited_candidate_shadow_color", hilited_candidate_shadow_color_box.CurrentRimeColor);
            }
            else
            {
                _style.hilited_candidate_shadow_color = hilited_candidate_shadow_color_box.CurrentColor;
                _style.TriggerChange();
            }
        }

        private void hilited_label_color_box_CurrentColorChanged(object sender, EventArgs e)
        {
            if (_isInitializing) return; // 如果正在加载，不跑下面的修改代码
            if (color_scheme_box.SelectedIndex == 0)
            {
                _style.hilited_label_color = hilited_label_color_box.CurrentColor;
                _style.TriggerChange();
                YamlHelper.ModifyNode(Path.Combine(RimeUserDir, "weasel.yaml"), "preset_color_schemes/custom/hilited_label_color", hilited_label_color_box.CurrentRimeColor);
            }
            else
            {
                _style.hilited_label_color = hilited_label_color_box.CurrentColor;
                _style.TriggerChange();
            }
        }

        private void hilited_candidate_text_color_box_CurrentColorChanged(object sender, EventArgs e)
        {
            if (_isInitializing) return; // 如果正在加载，不跑下面的修改代码
            if (color_scheme_box.SelectedIndex == 0)
            {
                _style.hilited_candidate_text_color = hilited_candidate_text_color_box.CurrentColor;
                _style.TriggerChange();
                YamlHelper.ModifyNode(Path.Combine(RimeUserDir, "weasel.yaml"), "preset_color_schemes/custom/hilited_candidate_text_color", hilited_candidate_text_color_box.CurrentRimeColor);
            }
            else
            {
                _style.hilited_candidate_text_color = hilited_candidate_text_color_box.CurrentColor;
                _style.TriggerChange();
            }
        }

        private void hilited_candidate_border_color_box_CurrentColorChanged(object sender, EventArgs e)
        {
            if (_isInitializing) return; // 如果正在加载，不跑下面的修改代码
            if (color_scheme_box.SelectedIndex == 0)
            {
                _style.hilited_candidate_border_color = hilited_candidate_border_color_box.CurrentColor;
                _style.TriggerChange();
                YamlHelper.ModifyNode(Path.Combine(RimeUserDir, "weasel.yaml"), "preset_color_schemes/custom/hilited_candidate_border_color", hilited_candidate_border_color_box.CurrentRimeColor);
            }
            else
            {
                _style.hilited_candidate_border_color = hilited_candidate_border_color_box.CurrentColor;
                _style.TriggerChange();
            }
        }

        private void hilited_comment_text_color_box_CurrentColorChanged(object sender, EventArgs e)
        {
            if (_isInitializing) return; // 如果正在加载，不跑下面的修改代码
            if (color_scheme_box.SelectedIndex == 0)
            {
                _style.hilited_comment_text_color = hilited_comment_text_color_box.CurrentColor;
                _style.TriggerChange();
                YamlHelper.ModifyNode(Path.Combine(RimeUserDir, "weasel.yaml"), "preset_color_schemes/custom/hilited_comment_text_color", hilited_comment_text_color_box.CurrentRimeColor);
            }
            else
            {
                _style.hilited_comment_text_color = hilited_comment_text_color_box.CurrentColor;
                _style.TriggerChange();
            }
        }

        private void candidate_back_color_box_CurrentColorChanged(object sender, EventArgs e)
        {
            if (_isInitializing) return; // 如果正在加载，不跑下面的修改代码
            if (color_scheme_box.SelectedIndex == 0)
            {
                _style.candidate_back_color = candidate_back_color_box.CurrentColor;
                _style.TriggerChange();
                YamlHelper.ModifyNode(Path.Combine(RimeUserDir, "weasel.yaml"), "preset_color_schemes/custom/candidate_back_color", candidate_back_color_box.CurrentRimeColor);
            }
            else
            {
                _style.candidate_back_color = candidate_back_color_box.CurrentColor;
                _style.TriggerChange();
            }
        }

        private void candidate_text_color_box_CurrentColorChanged(object sender, EventArgs e)
        {
            if (_isInitializing) return; // 如果正在加载，不跑下面的修改代码
            if (color_scheme_box.SelectedIndex == 0)
            {
                _style.candidate_text_color = candidate_text_color_box.CurrentColor;
                _style.TriggerChange();
                YamlHelper.ModifyNode(Path.Combine(RimeUserDir, "weasel.yaml"), "preset_color_schemes/custom/candidate_text_color", candidate_text_color_box.CurrentRimeColor);
            }
            else
            {
                _style.candidate_text_color = candidate_text_color_box.CurrentColor;
                _style.TriggerChange();
            }
        }

        private void candidate_shadow_color_box_CurrentColorChanged(object sender, EventArgs e)
        {
            if (_isInitializing) return; // 如果正在加载，不跑下面的修改代码
            if (color_scheme_box.SelectedIndex == 0)
            {
                _style.candidate_shadow_color = candidate_shadow_color_box.CurrentColor;
                _style.TriggerChange();
                YamlHelper.ModifyNode(Path.Combine(RimeUserDir, "weasel.yaml"), "preset_color_schemes/custom/candidate_shadow_color", candidate_shadow_color_box.CurrentRimeColor);
            }
            else
            {
                _style.candidate_shadow_color = candidate_shadow_color_box.CurrentColor;
                _style.TriggerChange();
            }
        }

        private void label_color_box_CurrentColorChanged(object sender, EventArgs e)
        {
            if (_isInitializing) return; // 如果正在加载，不跑下面的修改代码
            if (color_scheme_box.SelectedIndex == 0)
            {
                _style.label_color = label_color_box.CurrentColor;
                _style.TriggerChange();
                YamlHelper.ModifyNode(Path.Combine(RimeUserDir, "weasel.yaml"), "preset_color_schemes/custom/label_color", label_color_box.CurrentRimeColor);
            }
            else
            {
                _style.label_color = label_color_box.CurrentColor;
                _style.TriggerChange();
            }
        }

        private void candidate_border_color_box_CurrentColorChanged(object sender, EventArgs e)
        {
            if (_isInitializing) return; // 如果正在加载，不跑下面的修改代码
            if (color_scheme_box.SelectedIndex == 0)
            {
                _style.candidate_border_color = candidate_border_color_box.CurrentColor;
                _style.TriggerChange();
                YamlHelper.ModifyNode(Path.Combine(RimeUserDir, "weasel.yaml"), "preset_color_schemes/custom/candidate_border_color", candidate_border_color_box.CurrentRimeColor);
            }
            else
            {
                _style.candidate_border_color = candidate_border_color_box.CurrentColor;
                _style.TriggerChange();
            }
        }



        private void page_size_box_ValueChanged(object sender, EventArgs e)
        {
            if (_isInitializing) return; // 如果正在加载，不跑下面的修改代码
            YamlHelper.ModifyNode(Path.Combine(RimeUserDir, "default.custom.yaml"), "patch/menu/page_size", page_size_box.Value.ToString());
        }

        private void inline_preedit_box_CheckedChanged(object sender, EventArgs e)
        {
            if (_isInitializing) return; // 如果正在加载，不跑下面的修改代码
            if (inline_preedit_box.Checked)
            {
                YamlHelper.ModifyNode(Path.Combine(RimeUserDir, "weasel.custom.yaml"), "patch/style/inline_preedit", "true");
            }
            else
            {
                YamlHelper.ModifyNode(Path.Combine(RimeUserDir, "weasel.custom.yaml"), "patch/style/inline_preedit", "false");
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "文本文件 (UTF8无BOM.txt)|*.txt";
                openFileDialog.Title = "选择码表文件";

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        // 1. 获取用户数据目录（利用你 YamlHelper 中的方法）
                        string userDir = Path.Join(RimeUserDir, "tables", "custom");

                        // 2. 定义目标 YAML 路径 (例如 wubi.dict.yaml)
                        string targetYaml = Path.Combine(userDir, "wubi.dict.yaml");

                        // 3. 执行转换与导入
                        DictHelper.ProcessAndImport(openFileDialog.FileName, targetYaml);
                        Thread.Sleep(500);
                        MessageBox.Show("导入码表成功，保存后生效，有重码或错误提示在桌面文件查看。");

                        radioButton_custom.Checked = false;
                        radioButton_custom.Checked = true;
                        //// 4. 询问是否立即部署
                        //if (MessageBox.Show("导入成功！是否立即重新部署以生效？", "提示",
                        //    MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                        //{
                        //    YamlHelper.RunDeploy();
                        //}
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"导入失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "文本文件 (UTF8无BOM.txt)|*.txt";
                openFileDialog.Title = "选择拆分表文件";

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        // 1. 获取用户数据目录（利用你 YamlHelper 中的方法）
                        string userDir = Path.Join(RimeUserDir, "tables", "custom");

                        // 2. 定义目标 YAML 路径 (例如 wubi.dict.yaml)
                        string targetYaml = Path.Combine(userDir, "wb_spelling.dict.yaml");

                        // 3. 执行转换与导入
                        DictHelper.ImportSpellingDict(openFileDialog.FileName, targetYaml);
                        Thread.Sleep(500);
                        MessageBox.Show("导入拆分表成功，选择自定义，然后保存后生效。");



                        radioButton_custom.Checked = false;
                        radioButton_custom.Checked = true;
                        //// 4. 询问是否立即部署
                        //if (MessageBox.Show("导入成功！是否立即重新部署以生效？", "提示",
                        //    MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                        //{
                        //    YamlHelper.RunDeploy();
                        //}
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"导入失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private async void button2_Click(object sender, EventArgs e)
        {
            string zipPath = "./data.zip";

            if (!File.Exists(zipPath))
            {
                MessageBox.Show("错误：找不到 data.zip 文件。");
                return;
            }

            button2.Enabled = false; // 禁用按钮防止重复点击
            lblStatus.Text = "正在初始化...";

            try
            {
                YamlHelper.QuitRimeServer();
                Thread.Sleep(1000); // 等待 Rime 服务器完全退出
                // 使用 Task.Run 将耗时 IO 操作移出 UI 线程
                await Task.Run(() =>
                {
                    using ZipArchive archive = ZipFile.OpenRead(zipPath);
                    foreach (ZipArchiveEntry entry in archive.Entries)
                    {
                        // 拼接完整路径并标准化
                        string fullPath = Path.GetFullPath(Path.Combine(RimeUserDir, entry.FullName));

                        // 安全检查：防止 Zip Slip 攻击
                        if (!fullPath.StartsWith(Path.GetFullPath(RimeUserDir), StringComparison.OrdinalIgnoreCase))
                            continue;

                        if (string.IsNullOrEmpty(entry.Name))
                        {
                            Directory.CreateDirectory(fullPath);
                        }
                        else
                        {
                            Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
                            // overwrite: true 实现覆盖功能
                            entry.ExtractToFile(fullPath, overwrite: true);
                        }
                    }
                });
                YamlHelper.RunDeploy();
                MessageBox.Show("初始化成功！请重新打开设置。");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"初始化失败: {ex.Message}");
            }
            finally
            {
                button2.Enabled = true;
                lblStatus.Text = "初始化完成！";
                Close();
            }
        }

        private void alternative_select_labels_box_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_isInitializing) return; // 如果正在加载，不跑下面的修改代码
            // 确保有选中项，防止索引为 -1
            if (alternative_select_labels_box.SelectedIndex != -1)
            {
                // 方式 A：获取显示的文本（最常用）
                string selectedText = alternative_select_labels_box.SelectedItem.ToString();

                rimePreviewControl1.SetSelectLabels(selectedText);
                // 方式 B：获取选中项的索引
                //int index = alternative_select_labels_box.SelectedIndex;


                YamlHelper.ModifyNode(Path.Combine(RimeUserDir, "default.custom.yaml"), "patch/menu/alternative_select_labels", selectedText);
            }
        }

        private void user_dict_box_CheckedChanged(object sender, EventArgs e)
        {
            if (_isInitializing) return;
            if (user_dict_box.Checked)
            {
                YamlHelper.ModifyNode(Path.Combine(RimeUserDir, "wubi.custom.yaml"), "patch/translator/dictionary", "wubi.extended");
                YamlHelper.ModifyNode(Path.Combine(RimeUserDir, "wubi.custom.yaml"), "patch/fixed/dictionary", "wubi.extended");
                YamlHelper.ModifyNode(Path.Combine(RimeUserDir, "wubi.custom.yaml"), "patch/mkst/dictionary", "wubi.extended");
                YamlHelper.ModifyNode(Path.Combine(RimeUserDir, "wubi.custom.yaml"), "patch/sentence_input/dictionary", "wubi.extended");
                YamlHelper.ModifyNode(Path.Combine(RimeUserDir, "wubi.custom.yaml"), "patch/ci_reverse_lookup/dictionary", "wubi.extended");
                YamlHelper.ModifyNode(Path.Combine(RimeUserDir, "wubi_pinyin.custom.yaml"), "patch/translator/dictionary", "wubi.extended");
            }
            else
            {
                YamlHelper.ModifyNode(Path.Combine(RimeUserDir, "wubi.custom.yaml"), "patch/translator/dictionary", "wubi");
                YamlHelper.ModifyNode(Path.Combine(RimeUserDir, "wubi.custom.yaml"), "patch/fixed/dictionary", "wubi");
                YamlHelper.ModifyNode(Path.Combine(RimeUserDir, "wubi.custom.yaml"), "patch/mkst/dictionary", "wubi");
                YamlHelper.ModifyNode(Path.Combine(RimeUserDir, "wubi.custom.yaml"), "patch/sentence_input/dictionary", "wubi");
                YamlHelper.ModifyNode(Path.Combine(RimeUserDir, "wubi.custom.yaml"), "patch/ci_reverse_lookup/dictionary", "wubi");
                YamlHelper.ModifyNode(Path.Combine(RimeUserDir, "wubi_pinyin.custom.yaml"), "patch/translator/dictionary", "wubi");
            }
        }

        private void Dict_settings_box_Click(object sender, EventArgs e)
        {
            _ = new WubiDict().ShowDialog();
        }

        private void Phrase_settings_box_Click(object sender, EventArgs e)
        {
            _ = new Phrase().ShowDialog();
        }
        private void FuzzyPinyin_box_Click(object sender, EventArgs e)
        {
            _ = new FuzzyPinyinForm().ShowDialog();
        }



















        // 实体类
        public class DictEntry
        {
            public string Text { get; set; }
            public string Code { get; set; }
            public int Weight { get; set; }
            public string Stem { get; set; }
        }
        private List<string> headerLines = new();
        private List<DictEntry> allEntries = new();
        private List<DictEntry> filteredEntries = new();
        private string currentFilePath = string.Empty;

        // ListView 虚拟模式核心：极速提供数据
        private void LvDict_RetrieveVirtualItem(object sender, RetrieveVirtualItemEventArgs e)
        {
            if (e.ItemIndex >= 0 && e.ItemIndex < filteredEntries.Count)
            {
                var entry = filteredEntries[e.ItemIndex];
                ListViewItem item = new ListViewItem(entry.Text);
                item.SubItems.Add(entry.Code);
                item.SubItems.Add(entry.Weight.ToString());
                item.SubItems.Add(entry.Stem ?? "");
                e.Item = item;
            }
        }

        // 选中行改变时：同步数据到下方的编辑面板
        private void LvDict_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lvDict.SelectedIndices.Count > 0)
            {
                int index = lvDict.SelectedIndices[0];
                var entry = filteredEntries[index];

                txtEditText.Text = entry.Text;
                txtEditCode.Text = entry.Code;
                txtEditWeight.Text = entry.Weight.ToString();

                if (entry.Text.Length == 1)
                {
                    txtEditStem.Enabled = true;
                    txtEditStem.Text = entry.Stem;
                }
                else
                {
                    txtEditStem.Enabled = false;
                    txtEditStem.Text = string.Empty;
                }
                //if(System_Dict_Box.SelectedIndex == 6)
                //{
                //    txtEditStem.Enabled = false;
                //}
            }
        }

        // 下方面板“确认修改”逻辑：同步回内存并重排
        private void BtnApplyEdit_Click(object sender, EventArgs e)
        {
            if (lvDict.SelectedIndices.Count == 0) return;

            int index = lvDict.SelectedIndices[0];
            var entry = filteredEntries[index];

            // 更新数据
            entry.Text = txtEditText.Text.Trim();
            entry.Code = txtEditCode.Text.Trim().ToLower();

            if (int.TryParse(txtEditWeight.Text.Trim(), out int w))
            {
                entry.Weight = w;
            }
            entry.Stem = txtEditStem.Text.Trim();

            // 修改完毕后，触发重新排序并保持该行选中
            ApplyFilterAndSort(entry);
            lvDict.Focus(); // 将焦点还给列表，方便继续用键盘上下移动
            btnSave.Enabled = true;
        }

        // 快捷键：在下方的输入框里直接按 Enter 键触发保存
        private void EditControl_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true; // 消除 "叮" 的提示音
                BtnApplyEdit_Click(sender, e);
            }
        }

        private void BtnLoad_Click(object sender, EventArgs e)
        {
            using OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "YAML|*.yaml|所有文件|*.*";

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                LoadDictionary(ofd.FileName);
                btnSave.Enabled = true;
            }
        }

        private void LoadDictionary(string filePath)
        {
            currentFilePath = filePath;
            headerLines.Clear();
            allEntries.Clear();

            var lines = File.ReadAllLines(filePath, Encoding.UTF8);
            bool isHeader = true;

            allEntries.Capacity = lines.Length;

            foreach (var line in lines)
            {
                if (isHeader)
                {
                    headerLines.Add(line);
                    if (line.Trim() == "...") isHeader = false;
                    continue;
                }

                if (string.IsNullOrWhiteSpace(line)) continue;

                var parts = line.Split('\t');
                if (parts.Length >= 2)
                {
                    int w = 0;
                    if (parts.Length >= 3) _ = int.TryParse(parts[2], out w);

                    allEntries.Add(new DictEntry
                    {
                        Text = parts[0],
                        Code = parts[1],
                        Weight = w,
                        Stem = parts.Length >= 4 ? parts[3] : null
                    });
                }
            }

            ApplyFilterAndSort();
        }

        // 核心过滤与排序机制
        private void ApplyFilterAndSort(DictEntry? targetEntry = null)
        {
            string filter = txtFilter.Text.Trim().ToLower();

            // --- 修改开始：支持双向搜索 ---
            IEnumerable<DictEntry> query;
            if (string.IsNullOrEmpty(filter))
            {
                query = allEntries;
            }
            else
            {
                // 如果是字母，匹配编码开头；如果是汉字，匹配词条内容
                query = allEntries.Where(x =>
                    (x.Code != null && x.Code.StartsWith(filter, StringComparison.OrdinalIgnoreCase)) ||
                    (x.Text != null && x.Text.Contains(filter)) // 或者使用 x.Word，根据你的实体类属性名而定
                );
            }
            // --- 修改结束 ---

            filteredEntries = query
                .OrderBy(x => x.Code)
                .ThenByDescending(x => x.Weight)
                .ToList();

            // UI 更新逻辑保持不变
            lvDict.BeginUpdate();
            try
            {
                lvDict.VirtualListSize = filteredEntries.Count;
                lvDict.SelectedIndices.Clear();

                if (targetEntry != null)
                {
                    int index = filteredEntries.IndexOf(targetEntry);
                    if (index >= 0)
                    {
                        lvDict.SelectedIndices.Add(index);
                        lvDict.EnsureVisible(index);
                        // 注意：在某些 .NET 版本中，虚拟模式下设置 FocusedItem 需谨慎，
                        // 如果还报错，可以将下面这行注释掉
                        try { lvDict.FocusedItem = lvDict.Items[index]; } catch { }
                    }
                }
            }
            finally
            {
                lvDict.EndUpdate();
            }

            lvDict.Invalidate();
            btnSave.Enabled = true;
        }

        private void TxtFilter_TextChanged(object sender, EventArgs e)
        {
            string keyword = txtFilter.Text.Trim();

            if (string.IsNullOrEmpty(keyword))
            {
                // 如果关键词为空，显示所有条目
                filteredEntries = allEntries.ToList();
            }
            else
            {
                // 同时支持编码(Code)和词条(Text)搜索
                // StringComparison.OrdinalIgnoreCase 用于忽略编码的大小写
                filteredEntries = allEntries.Where(x =>
                    (x.Code != null && x.Code.StartsWith(keyword, StringComparison.OrdinalIgnoreCase)) ||
                    (x.Text != null && x.Text.Contains(keyword))
                ).ToList();
            }

            // 关键：更新虚拟列表的总数，并强制重绘
            lvDict.VirtualListSize = filteredEntries.Count;
            lvDict.Invalidate();

            // 如果你有计数标签，也可以在这里更新
            // lblCount.Text = $"找到 {filteredEntries.Count} 条记录";
        }

        private void BtnWeightUp_Click(object sender, EventArgs e)
        {
            AdjustWeight(10);
            lvDict.Focus(); // 【关键】：把焦点还给列表，恢复亮蓝色高亮
        }
        private void BtnWeightDown_Click(object sender, EventArgs e)
        {
            AdjustWeight(-10);
            lvDict.Focus(); // 【关键】：把焦点还给列表，恢复亮蓝色高亮
        }

        private void AdjustWeight(int offset)
        {
            if (lvDict.SelectedIndices.Count == 0) return;

            // 锁定第一条记录用于恢复焦点
            int firstIndex = lvDict.SelectedIndices[0];
            var targetEntry = filteredEntries[firstIndex];

            // 批量更新内存对象
            foreach (int index in lvDict.SelectedIndices)
            {
                if (index >= 0 && index < filteredEntries.Count)
                {
                    var entry = filteredEntries[index];
                    entry.Weight = Math.Max(0, entry.Weight + offset);
                }
            }

            ApplyFilterAndSort(targetEntry);
        }

        private void BtnAddRow_Click(object sender, EventArgs e)
        {
            var newEntry = new DictEntry { Text = "新词", Code = "a", Weight = 10 };

            if (lvDict.SelectedIndices.Count > 0)
            {
                var baseEntry = filteredEntries[lvDict.SelectedIndices[0]];
                newEntry.Code = baseEntry.Code;
                newEntry.Weight = Math.Max(0, baseEntry.Weight);
            }

            allEntries.Add(newEntry);
            ApplyFilterAndSort(newEntry);
        }

        private void BtnDeleteRow_Click(object sender, EventArgs e)
        {
            if (lvDict.SelectedIndices.Count == 0) return;

            if (MessageBox.Show("确定删除选中词条？", "提示", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                var toRemove = new List<DictEntry>();
                foreach (int index in lvDict.SelectedIndices)
                {
                    toRemove.Add(filteredEntries[index]);
                }

                foreach (var item in toRemove)
                {
                    allEntries.Remove(item);
                }

                ApplyFilterAndSort();
            }
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(currentFilePath)) return;

            var utf8 = new UTF8Encoding(false);
            using var sw = new StreamWriter(currentFilePath, false, utf8);

            foreach (var h in headerLines)
                sw.WriteLine(h);

            // 保存时保证全集完全有序
            foreach (var en in allEntries.OrderBy(x => x.Code).ThenByDescending(x => x.Weight))
            {
                string line = $"{en.Text}\t{en.Code}\t{en.Weight}";
                if (!string.IsNullOrEmpty(en.Stem))
                    line += $"\t{en.Stem}";
                sw.WriteLine(line);
            }

            MessageBox.Show("保存成功");


            int index = System_Dict_Box.SelectedIndex;
            if (radioButton_86.Checked == true && index == 0)
            {
                ReplaceSchemaFiles("86");
            }
            else if (radioButton_98.Checked == true && index == 1)
            {
                ReplaceSchemaFiles("98");
            }
            else if (radioButton_06.Checked == true && index == 2)
            {
                ReplaceSchemaFiles("06");
            }
            else if (radioButton_986.Checked == true && index == 3)
            {
                ReplaceSchemaFiles("986");
            }
            else if (radioButton_tiger.Checked == true && index == 4)
            {
                ReplaceSchemaFiles("tiger");
            }
            else if (radioButton_custom.Checked == true && index == 5)
            {
                ReplaceSchemaFiles("custom");
            }
        }

        private void System_Dict_Box_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (System_Dict_Box.SelectedIndex == -1) return;

            // 如果使用 WeaselSettings 请确保命名空间存在
            string baseDir = YamlHelper.GetUserDataDir();

            string path = System_Dict_Box.SelectedIndex switch
            {
                0 => baseDir + "\\tables\\86\\wubi.dict.yaml",
                1 => baseDir + "\\tables\\98\\wubi.dict.yaml",
                2 => baseDir + "\\tables\\06\\wubi.dict.yaml",
                3 => baseDir + "\\tables\\986\\wubi.dict.yaml",
                4 => baseDir + "\\tables\\tiger\\wubi.dict.yaml",
                5 => baseDir + "\\tables\\custom\\wubi.dict.yaml",
                6 => baseDir + "\\wubi.extended.dict.yaml",
                _ => null
            };

            if (path != null && File.Exists(path))
            { LoadDictionary(path); }
            else
            {
                MessageBox.Show("对应的词库文件不存在！");
            }
        }
    }
}
