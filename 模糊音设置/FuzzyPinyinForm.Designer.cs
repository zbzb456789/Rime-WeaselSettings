namespace 模糊音设置
{
    partial class FuzzyPinyinForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            chkZ_ZH = new CheckBox();
            chkC_CH = new CheckBox();
            chkS_SH = new CheckBox();
            chkF_H = new CheckBox();
            chkR_L = new CheckBox();
            chkAn_Ang = new CheckBox();
            chkEn_Eng = new CheckBox();
            chkIn_Ing = new CheckBox();
            chkIan_Iang = new CheckBox();
            chkUan_Uang = new CheckBox();
            btnOk = new Button();
            btnCancel = new Button();
            btnReset = new Button();
            chkL_N = new CheckBox();
            SuspendLayout();
            // 
            // chkZ_ZH
            // 
            chkZ_ZH.Location = new Point(20, 20);
            chkZ_ZH.Name = "chkZ_ZH";
            chkZ_ZH.Size = new Size(104, 24);
            chkZ_ZH.TabIndex = 0;
            chkZ_ZH.Text = "z = zh";
            // 
            // chkC_CH
            // 
            chkC_CH.Location = new Point(140, 20);
            chkC_CH.Name = "chkC_CH";
            chkC_CH.Size = new Size(104, 24);
            chkC_CH.TabIndex = 1;
            chkC_CH.Text = "c = ch";
            // 
            // chkS_SH
            // 
            chkS_SH.Location = new Point(260, 20);
            chkS_SH.Name = "chkS_SH";
            chkS_SH.Size = new Size(104, 24);
            chkS_SH.TabIndex = 2;
            chkS_SH.Text = "s = sh";
            // 
            // chkF_H
            // 
            chkF_H.Location = new Point(20, 55);
            chkF_H.Name = "chkF_H";
            chkF_H.Size = new Size(104, 24);
            chkF_H.TabIndex = 3;
            chkF_H.Text = "f = h";
            // 
            // chkR_L
            // 
            chkR_L.Location = new Point(260, 55);
            chkR_L.Name = "chkR_L";
            chkR_L.Size = new Size(104, 24);
            chkR_L.TabIndex = 5;
            chkR_L.Text = "r = l";
            // 
            // chkAn_Ang
            // 
            chkAn_Ang.Location = new Point(20, 90);
            chkAn_Ang.Name = "chkAn_Ang";
            chkAn_Ang.Size = new Size(104, 24);
            chkAn_Ang.TabIndex = 6;
            chkAn_Ang.Text = "an = ang";
            // 
            // chkEn_Eng
            // 
            chkEn_Eng.Location = new Point(140, 90);
            chkEn_Eng.Name = "chkEn_Eng";
            chkEn_Eng.Size = new Size(104, 24);
            chkEn_Eng.TabIndex = 7;
            chkEn_Eng.Text = "en = eng";
            // 
            // chkIn_Ing
            // 
            chkIn_Ing.Location = new Point(260, 90);
            chkIn_Ing.Name = "chkIn_Ing";
            chkIn_Ing.Size = new Size(104, 24);
            chkIn_Ing.TabIndex = 8;
            chkIn_Ing.Text = "in = ing";
            // 
            // chkIan_Iang
            // 
            chkIan_Iang.Location = new Point(20, 125);
            chkIan_Iang.Name = "chkIan_Iang";
            chkIan_Iang.Size = new Size(104, 24);
            chkIan_Iang.TabIndex = 9;
            chkIan_Iang.Text = "ian = iang";
            // 
            // chkUan_Uang
            // 
            chkUan_Uang.Location = new Point(140, 125);
            chkUan_Uang.Name = "chkUan_Uang";
            chkUan_Uang.Size = new Size(104, 24);
            chkUan_Uang.TabIndex = 10;
            chkUan_Uang.Text = "uan = uang";
            // 
            // btnOk
            // 
            btnOk.Location = new Point(180, 180);
            btnOk.Name = "btnOk";
            btnOk.Size = new Size(75, 30);
            btnOk.TabIndex = 11;
            btnOk.Text = "确定";
            btnOk.Click += btnOk_Click;
            // 
            // btnCancel
            // 
            btnCancel.Location = new Point(270, 180);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(75, 30);
            btnCancel.TabIndex = 12;
            btnCancel.Text = "取消";
            btnCancel.Click += btnCancel_Click;
            // 
            // btnReset
            // 
            btnReset.Location = new Point(20, 180);
            btnReset.Name = "btnReset";
            btnReset.Size = new Size(100, 30);
            btnReset.TabIndex = 13;
            btnReset.Text = "恢复默认设置";
            btnReset.Click += btnReset_Click;
            // 
            // chkL_N
            // 
            chkL_N.Location = new Point(140, 55);
            chkL_N.Name = "chkL_N";
            chkL_N.Size = new Size(104, 24);
            chkL_N.TabIndex = 4;
            chkL_N.Text = "l = n";
            // 
            // FuzzyPinyinForm
            // 
            ClientSize = new Size(380, 230);
            Controls.Add(chkZ_ZH);
            Controls.Add(chkC_CH);
            Controls.Add(chkS_SH);
            Controls.Add(chkF_H);
            Controls.Add(chkL_N);
            Controls.Add(chkR_L);
            Controls.Add(chkAn_Ang);
            Controls.Add(chkEn_Eng);
            Controls.Add(chkIn_Ing);
            Controls.Add(chkIan_Iang);
            Controls.Add(chkUan_Uang);
            Controls.Add(btnOk);
            Controls.Add(btnCancel);
            Controls.Add(btnReset);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "FuzzyPinyinForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "模糊音设置";
            Load += FuzzyPinyinForm_Load;
            ResumeLayout(false);
        }

        private CheckBox chkZ_ZH;
        private CheckBox chkC_CH;
        private CheckBox chkS_SH;
        private CheckBox chkF_H;
        private CheckBox chkR_L;
        private CheckBox chkAn_Ang;
        private CheckBox chkEn_Eng;
        private CheckBox chkIn_Ing;
        private CheckBox chkIan_Iang;
        private CheckBox chkUan_Uang;
        private Button btnOk;
        private Button btnCancel;
        private Button btnReset;
        private CheckBox chkL_N;
    }
}