namespace NTR.RejiClient.Forms
{
    partial class RollForm
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
            dgvRoll = new DataGridView();
            btnArayaEkle = new Button();
            btnYukariTasi = new Button();
            btnAsagiTasi = new Button();
            btnSatirSil = new Button();
            txtTesekkur = new TextBox();
            btnRollVer = new Button();
            btnRollAl = new Button();
            lbTumGorseller = new ListBox();
            lbYayinGorselleri = new ListBox();
            btnEkleOk = new Button();
            ((System.ComponentModel.ISupportInitialize)dgvRoll).BeginInit();
            SuspendLayout();
            // 
            // dgvRoll
            // 
            dgvRoll.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvRoll.Dock = DockStyle.Left;
            dgvRoll.Location = new Point(0, 0);
            dgvRoll.Name = "dgvRoll";
            dgvRoll.Size = new Size(815, 822);
            dgvRoll.TabIndex = 0;
            // 
            // btnArayaEkle
            // 
            btnArayaEkle.Font = new Font("Segoe UI", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 0);
            btnArayaEkle.Location = new Point(836, 50);
            btnArayaEkle.Name = "btnArayaEkle";
            btnArayaEkle.Size = new Size(135, 56);
            btnArayaEkle.TabIndex = 1;
            btnArayaEkle.Text = "ARAYA EKLE";
            btnArayaEkle.UseVisualStyleBackColor = true;
            btnArayaEkle.Click += btnArayaEkle_Click;
            // 
            // btnYukariTasi
            // 
            btnYukariTasi.Font = new Font("Segoe UI", 9.75F, FontStyle.Bold);
            btnYukariTasi.Location = new Point(1037, 50);
            btnYukariTasi.Name = "btnYukariTasi";
            btnYukariTasi.Size = new Size(135, 56);
            btnYukariTasi.TabIndex = 2;
            btnYukariTasi.Text = "YUKARI TAŞI";
            btnYukariTasi.UseVisualStyleBackColor = true;
            btnYukariTasi.Click += btnYukariTasi_Click;
            // 
            // btnAsagiTasi
            // 
            btnAsagiTasi.Font = new Font("Segoe UI", 9.75F, FontStyle.Bold);
            btnAsagiTasi.Location = new Point(1037, 123);
            btnAsagiTasi.Name = "btnAsagiTasi";
            btnAsagiTasi.Size = new Size(135, 56);
            btnAsagiTasi.TabIndex = 3;
            btnAsagiTasi.Text = "AŞAĞI TAŞI";
            btnAsagiTasi.UseVisualStyleBackColor = true;
            btnAsagiTasi.Click += btnAsagiTasi_Click;
            // 
            // btnSatirSil
            // 
            btnSatirSil.Font = new Font("Segoe UI", 9.75F, FontStyle.Bold);
            btnSatirSil.Location = new Point(836, 123);
            btnSatirSil.Name = "btnSatirSil";
            btnSatirSil.Size = new Size(135, 56);
            btnSatirSil.TabIndex = 4;
            btnSatirSil.Text = "SATIR SİL";
            btnSatirSil.UseVisualStyleBackColor = true;
            btnSatirSil.Click += btnSatirSil_Click;
            // 
            // txtTesekkur
            // 
            txtTesekkur.Location = new Point(836, 441);
            txtTesekkur.Multiline = true;
            txtTesekkur.Name = "txtTesekkur";
            txtTesekkur.Size = new Size(336, 182);
            txtTesekkur.TabIndex = 6;
            // 
            // btnRollVer
            // 
            btnRollVer.Font = new Font("Segoe UI", 9.75F, FontStyle.Bold);
            btnRollVer.Location = new Point(836, 629);
            btnRollVer.Name = "btnRollVer";
            btnRollVer.Size = new Size(301, 141);
            btnRollVer.TabIndex = 7;
            btnRollVer.Text = "ROLL VER";
            btnRollVer.UseVisualStyleBackColor = true;
            btnRollVer.Click += btnRollVer_Click;
            // 
            // btnRollAl
            // 
            btnRollAl.Font = new Font("Segoe UI", 9.75F, FontStyle.Bold);
            btnRollAl.Location = new Point(941, 776);
            btnRollAl.Name = "btnRollAl";
            btnRollAl.Size = new Size(103, 36);
            btnRollAl.TabIndex = 8;
            btnRollAl.Text = "ROLL AL";
            btnRollAl.UseVisualStyleBackColor = true;
            btnRollAl.Click += btnRollAl_Click;
            // 
            // lbTumGorseller
            // 
            lbTumGorseller.FormattingEnabled = true;
            lbTumGorseller.ItemHeight = 15;
            lbTumGorseller.Location = new Point(836, 234);
            lbTumGorseller.Name = "lbTumGorseller";
            lbTumGorseller.Size = new Size(209, 169);
            lbTumGorseller.TabIndex = 9;
            // 
            // lbYayinGorselleri
            // 
            lbYayinGorselleri.FormattingEnabled = true;
            lbYayinGorselleri.ItemHeight = 15;
            lbYayinGorselleri.Location = new Point(1171, 234);
            lbYayinGorselleri.Name = "lbYayinGorselleri";
            lbYayinGorselleri.Size = new Size(209, 169);
            lbYayinGorselleri.TabIndex = 10;
            // 
            // btnEkleOk
            // 
            btnEkleOk.Location = new Point(1062, 291);
            btnEkleOk.Name = "btnEkleOk";
            btnEkleOk.Size = new Size(75, 23);
            btnEkleOk.TabIndex = 11;
            btnEkleOk.Text = "->";
            btnEkleOk.UseVisualStyleBackColor = true;
            btnEkleOk.Click += btnEkleOk_Click;
            // 
            // RollForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1525, 822);
            Controls.Add(btnEkleOk);
            Controls.Add(lbYayinGorselleri);
            Controls.Add(lbTumGorseller);
            Controls.Add(btnRollAl);
            Controls.Add(btnRollVer);
            Controls.Add(txtTesekkur);
            Controls.Add(btnSatirSil);
            Controls.Add(btnAsagiTasi);
            Controls.Add(btnYukariTasi);
            Controls.Add(btnArayaEkle);
            Controls.Add(dgvRoll);
            Name = "RollForm";
            Text = "RollForm";
            Load += RollForm_Load;
            ((System.ComponentModel.ISupportInitialize)dgvRoll).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private DataGridView dgvRoll;
        private Button btnArayaEkle;
        private Button btnYukariTasi;
        private Button btnAsagiTasi;
        private Button btnSatirSil;
        private TextBox txtTesekkur;
        private Button btnRollVer;
        private Button btnRollAl;
        private ListBox lbTumGorseller;
        private ListBox lbYayinGorselleri;
        private Button btnEkleOk;
    }
}