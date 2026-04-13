namespace 短语管理工具
{
    partial class AddWordForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            txtWord = new TextBox();
            txtCode = new TextBox();
            btnOk = new Button();
            btnCancel = new Button();
            lblDupItems = new TextBox();
            label1 = new Label();
            label2 = new Label();
            SuspendLayout();
            // 
            // txtWord
            // 
            txtWord.Enabled = false;
            txtWord.Font = new Font("Microsoft YaHei UI", 12F, FontStyle.Regular, GraphicsUnit.Point, 134);
            txtWord.Location = new Point(62, 52);
            txtWord.Multiline = true;
            txtWord.Name = "txtWord";
            txtWord.Size = new Size(384, 144);
            txtWord.TabIndex = 0;
            txtWord.TextChanged += TxtWord_TextChanged;
            // 
            // txtCode
            // 
            txtCode.Font = new Font("Microsoft YaHei UI", 12F, FontStyle.Regular, GraphicsUnit.Point, 134);
            txtCode.Location = new Point(62, 12);
            txtCode.Name = "txtCode";
            txtCode.Size = new Size(384, 28);
            txtCode.TabIndex = 1;
            txtCode.TextChanged += txtCode_TextChanged;
            // 
            // btnOk
            // 
            btnOk.Font = new Font("Microsoft YaHei UI", 12F, FontStyle.Regular, GraphicsUnit.Point, 134);
            btnOk.Location = new Point(73, 366);
            btnOk.Name = "btnOk";
            btnOk.Size = new Size(70, 33);
            btnOk.TabIndex = 3;
            btnOk.Text = "确认";
            btnOk.UseVisualStyleBackColor = true;
            btnOk.Click += btnOk_Click;
            // 
            // btnCancel
            // 
            btnCancel.Font = new Font("Microsoft YaHei UI", 12F, FontStyle.Regular, GraphicsUnit.Point, 134);
            btnCancel.Location = new Point(190, 366);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(70, 33);
            btnCancel.TabIndex = 4;
            btnCancel.Text = "取消";
            btnCancel.UseVisualStyleBackColor = true;
            btnCancel.Click += btnCancel_Click;
            // 
            // lblDupItems
            // 
            lblDupItems.Font = new Font("Microsoft YaHei UI", 12F, FontStyle.Regular, GraphicsUnit.Point, 134);
            lblDupItems.Location = new Point(62, 232);
            lblDupItems.Multiline = true;
            lblDupItems.Name = "lblDupItems";
            lblDupItems.Size = new Size(384, 87);
            lblDupItems.TabIndex = 6;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Microsoft YaHei UI", 12F);
            label1.Location = new Point(4, 100);
            label1.Name = "label1";
            label1.Size = new Size(58, 21);
            label1.TabIndex = 7;
            label1.Text = "词条：";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Font = new Font("Microsoft YaHei UI", 12F);
            label2.Location = new Point(4, 15);
            label2.Name = "label2";
            label2.Size = new Size(58, 21);
            label2.TabIndex = 8;
            label2.Text = "编码：";
            // 
            // AddWordForm
            // 
            AutoScaleDimensions = new SizeF(7F, 17F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(464, 411);
            Controls.Add(label2);
            Controls.Add(label1);
            Controls.Add(lblDupItems);
            Controls.Add(btnCancel);
            Controls.Add(btnOk);
            Controls.Add(txtCode);
            Controls.Add(txtWord);
            DoubleBuffered = true;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            Name = "AddWordForm";
            StartPosition = FormStartPosition.CenterParent;
            Text = "自定义短语";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private TextBox txtWord;
        private TextBox txtCode;
        private Button btnOk;
        private Button btnCancel;
        private TextBox lblDupItems;
        private Label label1;
        private Label label2;
    }
}