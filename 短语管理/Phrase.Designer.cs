namespace 短语管理工具
{
    partial class Phrase
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            DataGridViewCellStyle dataGridViewCellStyle1 = new DataGridViewCellStyle();
            lblFilter = new Label();
            txtFilter = new TextBox();
            dgvList = new DataGridView();
            colCode = new DataGridViewTextBoxColumn();
            colWord = new DataGridViewTextBoxColumn();
            btnAdd = new Button();
            btnDelete = new Button();
            btnImport = new Button();
            btnExport = new Button();
            btnClear = new Button();
            btnOk = new Button();
            btnCancel = new Button();
            panelBottom = new Panel();
            lblPageInfo = new Label();
            timer1 = new System.Windows.Forms.Timer(components);
            ((System.ComponentModel.ISupportInitialize)dgvList).BeginInit();
            panelBottom.SuspendLayout();
            SuspendLayout();
            // 
            // lblFilter
            // 
            lblFilter.AutoSize = true;
            lblFilter.Location = new Point(13, 30);
            lblFilter.Name = "lblFilter";
            lblFilter.Size = new Size(80, 17);
            lblFilter.TabIndex = 2;
            lblFilter.Text = "按编码过滤：";
            // 
            // txtFilter
            // 
            txtFilter.Location = new Point(99, 27);
            txtFilter.Name = "txtFilter";
            txtFilter.Size = new Size(120, 23);
            txtFilter.TabIndex = 3;
            txtFilter.TextChanged += TxtFilter_TextChanged;
            // 
            // dgvList
            // 
            dgvList.AllowUserToAddRows = false;
            dgvList.AllowUserToResizeRows = false;
            dgvList.BackgroundColor = Color.White;
            dgvList.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvList.Columns.AddRange(new DataGridViewColumn[] { colCode, colWord });
            dataGridViewCellStyle1.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = Color.FromArgb(220, 240, 220);
            dataGridViewCellStyle1.Font = new Font("微软雅黑", 9F, FontStyle.Regular, GraphicsUnit.Point, 134);
            dataGridViewCellStyle1.ForeColor = Color.Black;
            dataGridViewCellStyle1.SelectionBackColor = Color.FromArgb(200, 235, 200);
            dataGridViewCellStyle1.SelectionForeColor = Color.Black;
            dataGridViewCellStyle1.WrapMode = DataGridViewTriState.False;
            dgvList.DefaultCellStyle = dataGridViewCellStyle1;
            dgvList.GridColor = Color.Gainsboro;
            dgvList.Location = new Point(13, 76);
            dgvList.MultiSelect = false;
            dgvList.Name = "dgvList";
            dgvList.ReadOnly = true;
            dgvList.RowHeadersVisible = false;
            dgvList.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvList.Size = new Size(460, 342);
            dgvList.TabIndex = 4;
            dgvList.CellMouseDoubleClick += dgvList_CellMouseDoubleClick;
            dgvList.CellValueNeeded += DgvList_CellValueNeeded;
            // 
            // colCode
            // 
            colCode.HeaderText = "编码";
            colCode.Name = "colCode";
            colCode.ReadOnly = true;
            colCode.Width = 150;
            // 
            // colWord
            // 
            colWord.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            colWord.HeaderText = "词条";
            colWord.Name = "colWord";
            colWord.ReadOnly = true;
            // 
            // btnAdd
            // 
            btnAdd.BackColor = Color.WhiteSmoke;
            btnAdd.FlatStyle = FlatStyle.System;
            btnAdd.Location = new Point(490, 76);
            btnAdd.Name = "btnAdd";
            btnAdd.Size = new Size(100, 28);
            btnAdd.TabIndex = 5;
            btnAdd.Text = "添加词条";
            btnAdd.UseVisualStyleBackColor = false;
            btnAdd.Click += btnAdd_Click;
            // 
            // btnDelete
            // 
            btnDelete.BackColor = Color.WhiteSmoke;
            btnDelete.FlatStyle = FlatStyle.System;
            btnDelete.Location = new Point(490, 114);
            btnDelete.Name = "btnDelete";
            btnDelete.Size = new Size(100, 28);
            btnDelete.TabIndex = 6;
            btnDelete.Text = "删除词条";
            btnDelete.UseVisualStyleBackColor = false;
            btnDelete.Click += BtnDelete_Click;
            // 
            // btnImport
            // 
            btnImport.BackColor = Color.WhiteSmoke;
            btnImport.FlatStyle = FlatStyle.System;
            btnImport.Location = new Point(490, 152);
            btnImport.Name = "btnImport";
            btnImport.Size = new Size(100, 28);
            btnImport.TabIndex = 7;
            btnImport.Text = "词库导入";
            btnImport.UseVisualStyleBackColor = false;
            btnImport.Click += BtnImport_Click;
            // 
            // btnExport
            // 
            btnExport.BackColor = Color.WhiteSmoke;
            btnExport.FlatStyle = FlatStyle.System;
            btnExport.Location = new Point(490, 190);
            btnExport.Name = "btnExport";
            btnExport.Size = new Size(100, 28);
            btnExport.TabIndex = 8;
            btnExport.Text = "词库导出";
            btnExport.UseVisualStyleBackColor = false;
            btnExport.Click += btnExport_Click;
            // 
            // btnClear
            // 
            btnClear.BackColor = Color.WhiteSmoke;
            btnClear.FlatStyle = FlatStyle.System;
            btnClear.Location = new Point(490, 228);
            btnClear.Name = "btnClear";
            btnClear.Size = new Size(100, 28);
            btnClear.TabIndex = 9;
            btnClear.Text = "清除词库";
            btnClear.UseVisualStyleBackColor = false;
            btnClear.Click += BtnClear_Click;
            // 
            // btnOk
            // 
            btnOk.BackColor = Color.White;
            btnOk.FlatAppearance.BorderColor = Color.FromArgb(50, 180, 80);
            btnOk.FlatAppearance.MouseDownBackColor = Color.FromArgb(235, 255, 235);
            btnOk.FlatAppearance.MouseOverBackColor = Color.FromArgb(245, 255, 245);
            btnOk.FlatStyle = FlatStyle.Flat;
            btnOk.ForeColor = Color.Black;
            btnOk.Location = new Point(420, 11);
            btnOk.Name = "btnOk";
            btnOk.Size = new Size(80, 30);
            btnOk.TabIndex = 14;
            btnOk.Text = "确定";
            btnOk.UseVisualStyleBackColor = false;
            btnOk.Click += BtnOk_Click;
            // 
            // btnCancel
            // 
            btnCancel.Location = new Point(510, 11);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(80, 30);
            btnCancel.TabIndex = 15;
            btnCancel.Text = "取消";
            btnCancel.UseVisualStyleBackColor = true;
            btnCancel.Click += btnCancel_Click;
            // 
            // panelBottom
            // 
            panelBottom.BackColor = Color.FromArgb(240, 240, 240);
            panelBottom.Controls.Add(btnCancel);
            panelBottom.Controls.Add(btnOk);
            panelBottom.Dock = DockStyle.Bottom;
            panelBottom.Location = new Point(0, 460);
            panelBottom.Name = "panelBottom";
            panelBottom.Size = new Size(604, 51);
            panelBottom.TabIndex = 13;
            // 
            // lblPageInfo
            // 
            lblPageInfo.AutoSize = true;
            lblPageInfo.Location = new Point(13, 431);
            lblPageInfo.Name = "lblPageInfo";
            lblPageInfo.Size = new Size(71, 17);
            lblPageInfo.TabIndex = 10;
            lblPageInfo.Text = "共 0 条数据";
            // 
            // timer1
            // 
            timer1.Interval = 300;
            timer1.Tick += SearchTimer_Tick;
            // 
            // Phrase
            // 
            AutoScaleDimensions = new SizeF(7F, 17F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.White;
            ClientSize = new Size(604, 511);
            Controls.Add(panelBottom);
            Controls.Add(lblPageInfo);
            Controls.Add(btnClear);
            Controls.Add(btnExport);
            Controls.Add(btnImport);
            Controls.Add(btnDelete);
            Controls.Add(btnAdd);
            Controls.Add(dgvList);
            Controls.Add(txtFilter);
            Controls.Add(lblFilter);
            DoubleBuffered = true;
            Font = new Font("微软雅黑", 9F, FontStyle.Regular, GraphicsUnit.Point, 134);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            Margin = new Padding(3, 4, 3, 4);
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "Phrase";
            StartPosition = FormStartPosition.CenterParent;
            Text = "用户短语词库";
            ((System.ComponentModel.ISupportInitialize)dgvList).EndInit();
            panelBottom.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }
        private System.Windows.Forms.Label lblFilter;
        private System.Windows.Forms.TextBox txtFilter;
        private System.Windows.Forms.DataGridView dgvList;
        private System.Windows.Forms.Button btnAdd;
        private System.Windows.Forms.Button btnDelete;
        private System.Windows.Forms.Button btnImport;
        private System.Windows.Forms.Button btnExport;
        private System.Windows.Forms.Button btnClear;
        private System.Windows.Forms.Button btnOk;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Panel panelBottom;

        #endregion

        private DataGridViewTextBoxColumn colCode;
        private DataGridViewTextBoxColumn colWord;
        private Label lblPageInfo;
        private System.Windows.Forms.Timer timer1;
    }
}
