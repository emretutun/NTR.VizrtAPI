namespace NTR.RejiClient
{
    partial class MainForm
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
            btnBaglan = new Button();
            txtIpAdresi = new TextBox();
            label1 = new Label();
            dtpTarih = new DateTimePicker();
            cmbAkislar = new ComboBox();
            btnAkisYenile = new Button();
            cmbKanal = new ComboBox();
            panel1 = new Panel();
            btnRollEkraniAc = new Button();
            splitContainer2 = new SplitContainer();
            dgvKjListesi = new DataGridView();
            groupBox2 = new GroupBox();
            btnTumunuAl = new Button();
            tabControl1 = new TabControl();
            tabKJ = new TabPage();
            btnMuhabirKameramanVer = new Button();
            btnMuhabirKameramanAl = new Button();
            btnYerAl = new Button();
            btnYerVer = new Button();
            btnKJAl = new Button();
            btnUzunKJ = new Button();
            btnCiftliKJ = new Button();
            btnTekliKJ = new Button();
            rbWhatsappIhbar = new RadioButton();
            rbOzelHaber = new RadioButton();
            rbSonDakika = new RadioButton();
            rbAzSonraDSFv2 = new RadioButton();
            rbAzSonraDSF = new RadioButton();
            rbAzSonra = new RadioButton();
            rbHaberKJ = new RadioButton();
            tabSosyalMedya = new TabPage();
            btnSosyalMedyaAl = new Button();
            btnWhatsappAl = new Button();
            btnWhatsappVer = new Button();
            btnSosyalMedyaVer = new Button();
            txtWhatsapp = new TextBox();
            txtSosyalMedya = new TextBox();
            groupBox1 = new GroupBox();
            cmbTitle = new ComboBox();
            cmbIsim = new ComboBox();
            btnSunucuIsimlikAl = new Button();
            btnSunucuIsimlikVer = new Button();
            cbxTelefon = new CheckBox();
            btnIsimlikAl = new Button();
            btnIsimlikVer = new Button();
            panel2 = new Panel();
            txtKjMetin2 = new TextBox();
            txtKjMetin1 = new TextBox();
            cbxAcilDurum = new CheckBox();
            btnSil = new Button();
            btnKaydet = new Button();
            lblSahneTipi = new Label();
            dgvHaberler = new DataGridView();
            splitContainer1 = new SplitContainer();
            btnKelebek = new Button();
            panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer2).BeginInit();
            splitContainer2.Panel1.SuspendLayout();
            splitContainer2.Panel2.SuspendLayout();
            splitContainer2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvKjListesi).BeginInit();
            groupBox2.SuspendLayout();
            tabControl1.SuspendLayout();
            tabKJ.SuspendLayout();
            tabSosyalMedya.SuspendLayout();
            groupBox1.SuspendLayout();
            panel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvHaberler).BeginInit();
            ((System.ComponentModel.ISupportInitialize)splitContainer1).BeginInit();
            splitContainer1.Panel1.SuspendLayout();
            splitContainer1.Panel2.SuspendLayout();
            splitContainer1.SuspendLayout();
            SuspendLayout();
            // 
            // btnBaglan
            // 
            btnBaglan.FlatStyle = FlatStyle.Flat;
            btnBaglan.Font = new Font("Segoe UI", 12F, FontStyle.Bold, GraphicsUnit.Point, 0);
            btnBaglan.Location = new Point(270, 7);
            btnBaglan.Name = "btnBaglan";
            btnBaglan.Size = new Size(104, 31);
            btnBaglan.TabIndex = 0;
            btnBaglan.Text = "BAĞLAN";
            btnBaglan.UseVisualStyleBackColor = true;
            btnBaglan.Click += btnBaglan_Click_1;
            // 
            // txtIpAdresi
            // 
            txtIpAdresi.Font = new Font("Segoe UI", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 0);
            txtIpAdresi.Location = new Point(100, 12);
            txtIpAdresi.Name = "txtIpAdresi";
            txtIpAdresi.Size = new Size(100, 25);
            txtIpAdresi.TabIndex = 1;
            txtIpAdresi.Text = "127.0.0.1";
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            label1.ForeColor = Color.Black;
            label1.Location = new Point(23, 15);
            label1.Name = "label1";
            label1.Size = new Size(73, 19);
            label1.TabIndex = 2;
            label1.Text = "IP Adresi:";
            // 
            // dtpTarih
            // 
            dtpTarih.Font = new Font("Segoe UI", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 0);
            dtpTarih.Location = new Point(256, 44);
            dtpTarih.Name = "dtpTarih";
            dtpTarih.Size = new Size(200, 25);
            dtpTarih.TabIndex = 3;
            // 
            // cmbAkislar
            // 
            cmbAkislar.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbAkislar.FormattingEnabled = true;
            cmbAkislar.Location = new Point(12, 75);
            cmbAkislar.Name = "cmbAkislar";
            cmbAkislar.Size = new Size(444, 23);
            cmbAkislar.TabIndex = 4;
            // 
            // btnAkisYenile
            // 
            btnAkisYenile.Font = new Font("Segoe UI", 14.25F, FontStyle.Bold, GraphicsUnit.Point, 0);
            btnAkisYenile.Location = new Point(462, 66);
            btnAkisYenile.Name = "btnAkisYenile";
            btnAkisYenile.Size = new Size(217, 38);
            btnAkisYenile.TabIndex = 5;
            btnAkisYenile.Text = "AKIŞ YENİLE";
            btnAkisYenile.UseVisualStyleBackColor = true;
            // 
            // cmbKanal
            // 
            cmbKanal.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbKanal.FormattingEnabled = true;
            cmbKanal.Location = new Point(12, 46);
            cmbKanal.Name = "cmbKanal";
            cmbKanal.Size = new Size(201, 23);
            cmbKanal.TabIndex = 6;
            // 
            // panel1
            // 
            panel1.Controls.Add(btnKelebek);
            panel1.Controls.Add(btnRollEkraniAc);
            panel1.Controls.Add(cmbKanal);
            panel1.Controls.Add(btnAkisYenile);
            panel1.Controls.Add(cmbAkislar);
            panel1.Controls.Add(dtpTarih);
            panel1.Controls.Add(label1);
            panel1.Controls.Add(txtIpAdresi);
            panel1.Controls.Add(btnBaglan);
            panel1.Dock = DockStyle.Top;
            panel1.Location = new Point(0, 0);
            panel1.Name = "panel1";
            panel1.Size = new Size(1825, 114);
            panel1.TabIndex = 0;
            // 
            // btnRollEkraniAc
            // 
            btnRollEkraniAc.Font = new Font("Segoe UI", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 0);
            btnRollEkraniAc.Location = new Point(1636, 68);
            btnRollEkraniAc.Name = "btnRollEkraniAc";
            btnRollEkraniAc.Size = new Size(177, 38);
            btnRollEkraniAc.TabIndex = 7;
            btnRollEkraniAc.Text = "ROLL AYARLARI";
            btnRollEkraniAc.UseVisualStyleBackColor = true;
            btnRollEkraniAc.Click += btnRollEkraniAc_Click;
            // 
            // splitContainer2
            // 
            splitContainer2.Dock = DockStyle.Fill;
            splitContainer2.Location = new Point(0, 0);
            splitContainer2.Name = "splitContainer2";
            splitContainer2.Orientation = Orientation.Horizontal;
            // 
            // splitContainer2.Panel1
            // 
            splitContainer2.Panel1.Controls.Add(dgvKjListesi);
            // 
            // splitContainer2.Panel2
            // 
            splitContainer2.Panel2.Controls.Add(groupBox2);
            splitContainer2.Panel2.Controls.Add(groupBox1);
            splitContainer2.Panel2.Controls.Add(panel2);
            splitContainer2.Size = new Size(1526, 843);
            splitContainer2.SplitterDistance = 534;
            splitContainer2.TabIndex = 0;
            // 
            // dgvKjListesi
            // 
            dgvKjListesi.AllowUserToAddRows = false;
            dgvKjListesi.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvKjListesi.Dock = DockStyle.Fill;
            dgvKjListesi.Location = new Point(0, 0);
            dgvKjListesi.Name = "dgvKjListesi";
            dgvKjListesi.ReadOnly = true;
            dgvKjListesi.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvKjListesi.Size = new Size(1526, 534);
            dgvKjListesi.TabIndex = 0;
            // 
            // groupBox2
            // 
            groupBox2.Controls.Add(btnTumunuAl);
            groupBox2.Controls.Add(tabControl1);
            groupBox2.Dock = DockStyle.Fill;
            groupBox2.Location = new Point(501, 0);
            groupBox2.Name = "groupBox2";
            groupBox2.Size = new Size(705, 305);
            groupBox2.TabIndex = 2;
            groupBox2.TabStop = false;
            // 
            // btnTumunuAl
            // 
            btnTumunuAl.Font = new Font("Segoe UI", 11.25F, FontStyle.Bold, GraphicsUnit.Point, 0);
            btnTumunuAl.Location = new Point(202, 258);
            btnTumunuAl.Name = "btnTumunuAl";
            btnTumunuAl.Size = new Size(131, 51);
            btnTumunuAl.TabIndex = 1;
            btnTumunuAl.Text = "TÜMÜNÜ AL (F8)";
            btnTumunuAl.UseVisualStyleBackColor = true;
            btnTumunuAl.Click += btnTumunuAl_Click;
            // 
            // tabControl1
            // 
            tabControl1.Controls.Add(tabKJ);
            tabControl1.Controls.Add(tabSosyalMedya);
            tabControl1.Font = new Font("Segoe UI", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 0);
            tabControl1.Location = new Point(0, 10);
            tabControl1.Name = "tabControl1";
            tabControl1.SelectedIndex = 0;
            tabControl1.Size = new Size(636, 246);
            tabControl1.TabIndex = 0;
            // 
            // tabKJ
            // 
            tabKJ.BackColor = Color.Gray;
            tabKJ.Controls.Add(btnMuhabirKameramanVer);
            tabKJ.Controls.Add(btnMuhabirKameramanAl);
            tabKJ.Controls.Add(btnYerAl);
            tabKJ.Controls.Add(btnYerVer);
            tabKJ.Controls.Add(btnKJAl);
            tabKJ.Controls.Add(btnUzunKJ);
            tabKJ.Controls.Add(btnCiftliKJ);
            tabKJ.Controls.Add(btnTekliKJ);
            tabKJ.Controls.Add(rbWhatsappIhbar);
            tabKJ.Controls.Add(rbOzelHaber);
            tabKJ.Controls.Add(rbSonDakika);
            tabKJ.Controls.Add(rbAzSonraDSFv2);
            tabKJ.Controls.Add(rbAzSonraDSF);
            tabKJ.Controls.Add(rbAzSonra);
            tabKJ.Controls.Add(rbHaberKJ);
            tabKJ.Font = new Font("Segoe UI Black", 11.25F, FontStyle.Bold, GraphicsUnit.Point, 0);
            tabKJ.Location = new Point(4, 26);
            tabKJ.Name = "tabKJ";
            tabKJ.Padding = new Padding(3);
            tabKJ.Size = new Size(628, 216);
            tabKJ.TabIndex = 0;
            tabKJ.Text = "KJ";
            // 
            // btnMuhabirKameramanVer
            // 
            btnMuhabirKameramanVer.Location = new Point(446, 107);
            btnMuhabirKameramanVer.Name = "btnMuhabirKameramanVer";
            btnMuhabirKameramanVer.Size = new Size(163, 66);
            btnMuhabirKameramanVer.TabIndex = 16;
            btnMuhabirKameramanVer.Text = "MUHABİR KAMERAMAN VER";
            btnMuhabirKameramanVer.UseVisualStyleBackColor = false;
            btnMuhabirKameramanVer.Click += btnMuhabirKameramanVer_Click;
            // 
            // btnMuhabirKameramanAl
            // 
            btnMuhabirKameramanAl.Location = new Point(490, 174);
            btnMuhabirKameramanAl.Name = "btnMuhabirKameramanAl";
            btnMuhabirKameramanAl.Size = new Size(74, 30);
            btnMuhabirKameramanAl.TabIndex = 15;
            btnMuhabirKameramanAl.Text = "AL";
            btnMuhabirKameramanAl.UseVisualStyleBackColor = true;
            btnMuhabirKameramanAl.Click += btnMuhabirKameramanAl_Click;
            // 
            // btnYerAl
            // 
            btnYerAl.Location = new Point(313, 174);
            btnYerAl.Name = "btnYerAl";
            btnYerAl.Size = new Size(67, 30);
            btnYerAl.TabIndex = 14;
            btnYerAl.Text = "AL";
            btnYerAl.UseVisualStyleBackColor = true;
            btnYerAl.Click += btnYerAl_Click;
            // 
            // btnYerVer
            // 
            btnYerVer.BackColor = Color.Lime;
            btnYerVer.Location = new Point(219, 173);
            btnYerVer.Name = "btnYerVer";
            btnYerVer.Size = new Size(86, 30);
            btnYerVer.TabIndex = 13;
            btnYerVer.Text = "YER VER";
            btnYerVer.UseVisualStyleBackColor = false;
            btnYerVer.Click += btnYerVer_Click;
            // 
            // btnKJAl
            // 
            btnKJAl.Location = new Point(446, 34);
            btnKJAl.Name = "btnKJAl";
            btnKJAl.Size = new Size(86, 41);
            btnKJAl.TabIndex = 12;
            btnKJAl.Text = "AL";
            btnKJAl.UseVisualStyleBackColor = true;
            btnKJAl.Click += btnKJAl_Click;
            // 
            // btnUzunKJ
            // 
            btnUzunKJ.BackColor = Color.Lime;
            btnUzunKJ.Font = new Font("Ebrima", 12F, FontStyle.Bold, GraphicsUnit.Point, 0);
            btnUzunKJ.Location = new Point(246, 108);
            btnUzunKJ.Name = "btnUzunKJ";
            btnUzunKJ.Size = new Size(171, 46);
            btnUzunKJ.TabIndex = 11;
            btnUzunKJ.Text = "UZUN SATIR KJ VER";
            btnUzunKJ.UseVisualStyleBackColor = false;
            btnUzunKJ.Click += btnUzunKJ_Click;
            // 
            // btnCiftliKJ
            // 
            btnCiftliKJ.BackColor = Color.Lime;
            btnCiftliKJ.Font = new Font("Ebrima", 12F, FontStyle.Bold, GraphicsUnit.Point, 0);
            btnCiftliKJ.Location = new Point(246, 58);
            btnCiftliKJ.Name = "btnCiftliKJ";
            btnCiftliKJ.Size = new Size(171, 46);
            btnCiftliKJ.TabIndex = 10;
            btnCiftliKJ.Text = "ÇİFT SATIR KJ VER";
            btnCiftliKJ.UseVisualStyleBackColor = false;
            btnCiftliKJ.Click += btnCiftliKJ_Click;
            // 
            // btnTekliKJ
            // 
            btnTekliKJ.BackColor = Color.Lime;
            btnTekliKJ.Font = new Font("Ebrima", 12F, FontStyle.Bold, GraphicsUnit.Point, 0);
            btnTekliKJ.Location = new Point(246, 6);
            btnTekliKJ.Name = "btnTekliKJ";
            btnTekliKJ.Size = new Size(171, 46);
            btnTekliKJ.TabIndex = 9;
            btnTekliKJ.Text = "TEK SATIR KJ VER";
            btnTekliKJ.UseVisualStyleBackColor = false;
            btnTekliKJ.Click += btnTekliKJ_Click;
            // 
            // rbWhatsappIhbar
            // 
            rbWhatsappIhbar.AutoSize = true;
            rbWhatsappIhbar.Location = new Point(28, 180);
            rbWhatsappIhbar.Name = "rbWhatsappIhbar";
            rbWhatsappIhbar.Size = new Size(144, 24);
            rbWhatsappIhbar.TabIndex = 6;
            rbWhatsappIhbar.Text = "Whatsapp İhbar";
            rbWhatsappIhbar.UseVisualStyleBackColor = true;
            // 
            // rbOzelHaber
            // 
            rbOzelHaber.AutoSize = true;
            rbOzelHaber.Location = new Point(28, 150);
            rbOzelHaber.Name = "rbOzelHaber";
            rbOzelHaber.Size = new Size(108, 24);
            rbOzelHaber.TabIndex = 5;
            rbOzelHaber.Text = "Özel Haber";
            rbOzelHaber.UseVisualStyleBackColor = true;
            // 
            // rbSonDakika
            // 
            rbSonDakika.AutoSize = true;
            rbSonDakika.Location = new Point(28, 120);
            rbSonDakika.Name = "rbSonDakika";
            rbSonDakika.Size = new Size(109, 24);
            rbSonDakika.TabIndex = 4;
            rbSonDakika.Text = "Son Dakika";
            rbSonDakika.UseVisualStyleBackColor = true;
            // 
            // rbAzSonraDSFv2
            // 
            rbAzSonraDSFv2.AutoSize = true;
            rbAzSonraDSFv2.Location = new Point(28, 90);
            rbAzSonraDSFv2.Name = "rbAzSonraDSFv2";
            rbAzSonraDSFv2.Size = new Size(149, 24);
            rbAzSonraDSFv2.TabIndex = 3;
            rbAzSonraDSFv2.Text = "Az Sonra DSF V2";
            rbAzSonraDSFv2.UseVisualStyleBackColor = true;
            // 
            // rbAzSonraDSF
            // 
            rbAzSonraDSF.AutoSize = true;
            rbAzSonraDSF.Location = new Point(28, 60);
            rbAzSonraDSF.Name = "rbAzSonraDSF";
            rbAzSonraDSF.Size = new Size(125, 24);
            rbAzSonraDSF.TabIndex = 2;
            rbAzSonraDSF.Text = "Az Sonra DSF";
            rbAzSonraDSF.UseVisualStyleBackColor = true;
            // 
            // rbAzSonra
            // 
            rbAzSonra.AutoSize = true;
            rbAzSonra.Location = new Point(28, 31);
            rbAzSonra.Name = "rbAzSonra";
            rbAzSonra.Size = new Size(93, 24);
            rbAzSonra.TabIndex = 1;
            rbAzSonra.Text = "Az Sonra";
            rbAzSonra.UseVisualStyleBackColor = true;
            // 
            // rbHaberKJ
            // 
            rbHaberKJ.AutoSize = true;
            rbHaberKJ.Checked = true;
            rbHaberKJ.Location = new Point(28, 3);
            rbHaberKJ.Name = "rbHaberKJ";
            rbHaberKJ.Size = new Size(94, 24);
            rbHaberKJ.TabIndex = 0;
            rbHaberKJ.TabStop = true;
            rbHaberKJ.Text = "Haber KJ";
            rbHaberKJ.UseVisualStyleBackColor = true;
            // 
            // tabSosyalMedya
            // 
            tabSosyalMedya.Controls.Add(btnSosyalMedyaAl);
            tabSosyalMedya.Controls.Add(btnWhatsappAl);
            tabSosyalMedya.Controls.Add(btnWhatsappVer);
            tabSosyalMedya.Controls.Add(btnSosyalMedyaVer);
            tabSosyalMedya.Controls.Add(txtWhatsapp);
            tabSosyalMedya.Controls.Add(txtSosyalMedya);
            tabSosyalMedya.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
            tabSosyalMedya.Location = new Point(4, 26);
            tabSosyalMedya.Name = "tabSosyalMedya";
            tabSosyalMedya.Padding = new Padding(3);
            tabSosyalMedya.Size = new Size(628, 216);
            tabSosyalMedya.TabIndex = 1;
            tabSosyalMedya.Text = "Sosyal Medya";
            tabSosyalMedya.UseVisualStyleBackColor = true;
            // 
            // btnSosyalMedyaAl
            // 
            btnSosyalMedyaAl.BackColor = SystemColors.ControlLight;
            btnSosyalMedyaAl.Location = new Point(423, 37);
            btnSosyalMedyaAl.Name = "btnSosyalMedyaAl";
            btnSosyalMedyaAl.Size = new Size(89, 50);
            btnSosyalMedyaAl.TabIndex = 20;
            btnSosyalMedyaAl.Text = "AL";
            btnSosyalMedyaAl.UseVisualStyleBackColor = false;
            btnSosyalMedyaAl.Click += btnSosyalMedyaAl_Click;
            // 
            // btnWhatsappAl
            // 
            btnWhatsappAl.BackColor = SystemColors.ControlLight;
            btnWhatsappAl.Location = new Point(423, 131);
            btnWhatsappAl.Name = "btnWhatsappAl";
            btnWhatsappAl.Size = new Size(89, 45);
            btnWhatsappAl.TabIndex = 21;
            btnWhatsappAl.Text = "AL";
            btnWhatsappAl.UseVisualStyleBackColor = false;
            btnWhatsappAl.Click += btnWhatsappAl_Click;
            // 
            // btnWhatsappVer
            // 
            btnWhatsappVer.BackColor = Color.LimeGreen;
            btnWhatsappVer.Location = new Point(249, 131);
            btnWhatsappVer.Name = "btnWhatsappVer";
            btnWhatsappVer.Size = new Size(142, 45);
            btnWhatsappVer.TabIndex = 3;
            btnWhatsappVer.Text = "WHATSAPP TELEFON VER (F4)";
            btnWhatsappVer.UseVisualStyleBackColor = false;
            btnWhatsappVer.Click += btnWhatsappVer_Click;
            // 
            // btnSosyalMedyaVer
            // 
            btnSosyalMedyaVer.BackColor = Color.Lime;
            btnSosyalMedyaVer.ForeColor = Color.Black;
            btnSosyalMedyaVer.Location = new Point(249, 37);
            btnSosyalMedyaVer.Name = "btnSosyalMedyaVer";
            btnSosyalMedyaVer.Size = new Size(142, 53);
            btnSosyalMedyaVer.TabIndex = 2;
            btnSosyalMedyaVer.Text = "TÜM SOSYAL MEDYA VER (F3)";
            btnSosyalMedyaVer.UseVisualStyleBackColor = false;
            btnSosyalMedyaVer.Click += btnSosyalMedyaVer_Click;
            // 
            // txtWhatsapp
            // 
            txtWhatsapp.Location = new Point(46, 143);
            txtWhatsapp.Name = "txtWhatsapp";
            txtWhatsapp.Size = new Size(121, 23);
            txtWhatsapp.TabIndex = 1;
            txtWhatsapp.Text = "0 549 889 70 00";
            // 
            // txtSosyalMedya
            // 
            txtSosyalMedya.Location = new Point(46, 53);
            txtSosyalMedya.Name = "txtSosyalMedya";
            txtSosyalMedya.Size = new Size(121, 23);
            txtSosyalMedya.TabIndex = 0;
            txtSosyalMedya.Text = "yenisayfashowtv";
            // 
            // groupBox1
            // 
            groupBox1.Controls.Add(cmbTitle);
            groupBox1.Controls.Add(cmbIsim);
            groupBox1.Controls.Add(btnSunucuIsimlikAl);
            groupBox1.Controls.Add(btnSunucuIsimlikVer);
            groupBox1.Controls.Add(cbxTelefon);
            groupBox1.Controls.Add(btnIsimlikAl);
            groupBox1.Controls.Add(btnIsimlikVer);
            groupBox1.Dock = DockStyle.Right;
            groupBox1.Location = new Point(1206, 0);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new Size(320, 305);
            groupBox1.TabIndex = 1;
            groupBox1.TabStop = false;
            // 
            // cmbTitle
            // 
            cmbTitle.FormattingEnabled = true;
            cmbTitle.Location = new Point(133, 189);
            cmbTitle.Name = "cmbTitle";
            cmbTitle.Size = new Size(121, 23);
            cmbTitle.TabIndex = 19;
            // 
            // cmbIsim
            // 
            cmbIsim.FormattingEnabled = true;
            cmbIsim.Location = new Point(6, 189);
            cmbIsim.Name = "cmbIsim";
            cmbIsim.Size = new Size(121, 23);
            cmbIsim.TabIndex = 18;
            // 
            // btnSunucuIsimlikAl
            // 
            btnSunucuIsimlikAl.BackColor = SystemColors.ControlLight;
            btnSunucuIsimlikAl.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
            btnSunucuIsimlikAl.Location = new Point(171, 17);
            btnSunucuIsimlikAl.Name = "btnSunucuIsimlikAl";
            btnSunucuIsimlikAl.Size = new Size(105, 45);
            btnSunucuIsimlikAl.TabIndex = 17;
            btnSunucuIsimlikAl.Text = "AL";
            btnSunucuIsimlikAl.UseVisualStyleBackColor = false;
            btnSunucuIsimlikAl.Click += btnSunucuIsimlikAl_Click;
            // 
            // btnSunucuIsimlikVer
            // 
            btnSunucuIsimlikVer.BackColor = Color.SeaGreen;
            btnSunucuIsimlikVer.Font = new Font("Segoe UI", 11.25F, FontStyle.Bold, GraphicsUnit.Point, 0);
            btnSunucuIsimlikVer.Location = new Point(0, 17);
            btnSunucuIsimlikVer.Name = "btnSunucuIsimlikVer";
            btnSunucuIsimlikVer.Size = new Size(165, 48);
            btnSunucuIsimlikVer.TabIndex = 5;
            btnSunucuIsimlikVer.Text = "SUNUCU İSİMLİK VER";
            btnSunucuIsimlikVer.UseVisualStyleBackColor = false;
            btnSunucuIsimlikVer.Click += btnSunucuIsimlikVer_Click;
            // 
            // cbxTelefon
            // 
            cbxTelefon.AutoSize = true;
            cbxTelefon.Location = new Point(6, 233);
            cbxTelefon.Name = "cbxTelefon";
            cbxTelefon.Size = new Size(64, 19);
            cbxTelefon.TabIndex = 4;
            cbxTelefon.Text = "Telefon";
            cbxTelefon.UseVisualStyleBackColor = true;
            // 
            // btnIsimlikAl
            // 
            btnIsimlikAl.BackColor = SystemColors.ControlLight;
            btnIsimlikAl.Location = new Point(124, 259);
            btnIsimlikAl.Name = "btnIsimlikAl";
            btnIsimlikAl.Size = new Size(89, 50);
            btnIsimlikAl.TabIndex = 1;
            btnIsimlikAl.Text = "AL";
            btnIsimlikAl.UseVisualStyleBackColor = false;
            btnIsimlikAl.Click += btnIsimlikAl_Click;
            // 
            // btnIsimlikVer
            // 
            btnIsimlikVer.BackColor = Color.SeaGreen;
            btnIsimlikVer.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
            btnIsimlikVer.Location = new Point(6, 258);
            btnIsimlikVer.Name = "btnIsimlikVer";
            btnIsimlikVer.Size = new Size(112, 53);
            btnIsimlikVer.TabIndex = 0;
            btnIsimlikVer.Text = "İSİMLİK VER";
            btnIsimlikVer.UseVisualStyleBackColor = false;
            btnIsimlikVer.Click += btnIsimlikVer_Click;
            // 
            // panel2
            // 
            panel2.Controls.Add(txtKjMetin2);
            panel2.Controls.Add(txtKjMetin1);
            panel2.Controls.Add(cbxAcilDurum);
            panel2.Controls.Add(btnSil);
            panel2.Controls.Add(btnKaydet);
            panel2.Controls.Add(lblSahneTipi);
            panel2.Dock = DockStyle.Left;
            panel2.Location = new Point(0, 0);
            panel2.Name = "panel2";
            panel2.Size = new Size(501, 305);
            panel2.TabIndex = 0;
            // 
            // txtKjMetin2
            // 
            txtKjMetin2.Location = new Point(3, 129);
            txtKjMetin2.Multiline = true;
            txtKjMetin2.Name = "txtKjMetin2";
            txtKjMetin2.Size = new Size(481, 23);
            txtKjMetin2.TabIndex = 7;
            // 
            // txtKjMetin1
            // 
            txtKjMetin1.Location = new Point(3, 88);
            txtKjMetin1.Multiline = true;
            txtKjMetin1.Name = "txtKjMetin1";
            txtKjMetin1.Size = new Size(481, 23);
            txtKjMetin1.TabIndex = 6;
            // 
            // cbxAcilDurum
            // 
            cbxAcilDurum.AutoSize = true;
            cbxAcilDurum.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold | FontStyle.Italic, GraphicsUnit.Point, 0);
            cbxAcilDurum.Location = new Point(16, 186);
            cbxAcilDurum.Name = "cbxAcilDurum";
            cbxAcilDurum.Size = new Size(112, 19);
            cbxAcilDurum.TabIndex = 5;
            cbxAcilDurum.Text = "ACİL DURUM KJ";
            cbxAcilDurum.UseVisualStyleBackColor = true;
            // 
            // btnSil
            // 
            btnSil.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btnSil.Location = new Point(360, 248);
            btnSil.Name = "btnSil";
            btnSil.Size = new Size(75, 23);
            btnSil.TabIndex = 4;
            btnSil.Text = "SİL";
            btnSil.UseVisualStyleBackColor = true;
            btnSil.Click += btnSil_Click;
            // 
            // btnKaydet
            // 
            btnKaydet.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btnKaydet.Location = new Point(360, 216);
            btnKaydet.Name = "btnKaydet";
            btnKaydet.Size = new Size(75, 23);
            btnKaydet.TabIndex = 3;
            btnKaydet.Text = "KAYDET";
            btnKaydet.UseVisualStyleBackColor = true;
            btnKaydet.Click += btnKaydet_Click;
            // 
            // lblSahneTipi
            // 
            lblSahneTipi.AutoSize = true;
            lblSahneTipi.Font = new Font("Segoe UI", 14.25F, FontStyle.Bold, GraphicsUnit.Point, 0);
            lblSahneTipi.Location = new Point(3, 36);
            lblSahneTipi.Name = "lblSahneTipi";
            lblSahneTipi.Size = new Size(121, 25);
            lblSahneTipi.TabIndex = 0;
            lblSahneTipi.Text = "SAHNE TİPİ:";
            // 
            // dgvHaberler
            // 
            dgvHaberler.AllowUserToAddRows = false;
            dgvHaberler.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvHaberler.Dock = DockStyle.Fill;
            dgvHaberler.Location = new Point(0, 0);
            dgvHaberler.Name = "dgvHaberler";
            dgvHaberler.ReadOnly = true;
            dgvHaberler.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvHaberler.Size = new Size(295, 843);
            dgvHaberler.TabIndex = 0;
            // 
            // splitContainer1
            // 
            splitContainer1.Dock = DockStyle.Fill;
            splitContainer1.Location = new Point(0, 114);
            splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            splitContainer1.Panel1.Controls.Add(dgvHaberler);
            // 
            // splitContainer1.Panel2
            // 
            splitContainer1.Panel2.Controls.Add(splitContainer2);
            splitContainer1.Panel2.Paint += splitContainer1_Panel2_Paint;
            splitContainer1.Size = new Size(1825, 843);
            splitContainer1.SplitterDistance = 295;
            splitContainer1.TabIndex = 1;
            // 
            // btnKelebek
            // 
            btnKelebek.Font = new Font("Segoe UI", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 0);
            btnKelebek.Location = new Point(1328, 66);
            btnKelebek.Name = "btnKelebek";
            btnKelebek.Size = new Size(177, 38);
            btnKelebek.TabIndex = 8;
            btnKelebek.Text = "Kelebek";
            btnKelebek.UseVisualStyleBackColor = true;
            btnKelebek.Click += btnKelebek_Click;
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = SystemColors.Control;
            ClientSize = new Size(1825, 957);
            Controls.Add(splitContainer1);
            Controls.Add(panel1);
            ForeColor = SystemColors.ControlText;
            Name = "MainForm";
            Text = "MainForm";
            panel1.ResumeLayout(false);
            panel1.PerformLayout();
            splitContainer2.Panel1.ResumeLayout(false);
            splitContainer2.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainer2).EndInit();
            splitContainer2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dgvKjListesi).EndInit();
            groupBox2.ResumeLayout(false);
            tabControl1.ResumeLayout(false);
            tabKJ.ResumeLayout(false);
            tabKJ.PerformLayout();
            tabSosyalMedya.ResumeLayout(false);
            tabSosyalMedya.PerformLayout();
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            panel2.ResumeLayout(false);
            panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)dgvHaberler).EndInit();
            splitContainer1.Panel1.ResumeLayout(false);
            splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainer1).EndInit();
            splitContainer1.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private Button btnBaglan;
        private TextBox txtIpAdresi;
        private Label label1;
        private DateTimePicker dtpTarih;
        private ComboBox cmbAkislar;
        private Button btnAkisYenile;
        private ComboBox cmbKanal;
        private Panel panel1;
        private Button btnRollEkraniAc;
        private SplitContainer splitContainer2;
        private DataGridView dgvKjListesi;
        private GroupBox groupBox2;
        private Button btnTumunuAl;
        private TabControl tabControl1;
        private TabPage tabKJ;
        private Button btnMuhabirKameramanVer;
        private Button btnMuhabirKameramanAl;
        private Button btnYerAl;
        private Button btnYerVer;
        private Button btnKJAl;
        private Button btnUzunKJ;
        private Button btnCiftliKJ;
        private Button btnTekliKJ;
        private RadioButton rbWhatsappIhbar;
        private RadioButton rbOzelHaber;
        private RadioButton rbSonDakika;
        private RadioButton rbAzSonraDSFv2;
        private RadioButton rbAzSonraDSF;
        private RadioButton rbAzSonra;
        private RadioButton rbHaberKJ;
        private TabPage tabSosyalMedya;
        private Button btnSosyalMedyaAl;
        private Button btnWhatsappAl;
        private Button btnWhatsappVer;
        private Button btnSosyalMedyaVer;
        private TextBox txtWhatsapp;
        private TextBox txtSosyalMedya;
        private GroupBox groupBox1;
        private ComboBox cmbTitle;
        private ComboBox cmbIsim;
        private Button btnSunucuIsimlikAl;
        private Button btnSunucuIsimlikVer;
        private CheckBox cbxTelefon;
        private Button btnIsimlikAl;
        private Button btnIsimlikVer;
        private Panel panel2;
        private TextBox txtKjMetin2;
        private TextBox txtKjMetin1;
        private CheckBox cbxAcilDurum;
        private Button btnSil;
        private Button btnKaydet;
        private Label lblSahneTipi;
        private DataGridView dgvHaberler;
        private SplitContainer splitContainer1;
        private Button btnKelebek;
    }
}