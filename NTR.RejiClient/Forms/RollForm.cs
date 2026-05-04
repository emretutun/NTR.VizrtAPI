using NTR.RejiClient.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using NTR.RejiClient.Models;

namespace NTR.RejiClient.Forms
{
    public partial class RollForm : Form
    {
        private readonly ApiService _api;
        private readonly string _engineType;
        private bool _isSending = false;

        // TXT dosya yolları
        private readonly string _rollDosyaYolu = Path.Combine(Application.StartupPath, "roll_data.txt");
        private readonly string _sponsorKlasorYolu = @"D:\SHOWTV_REJI_DATA\ROLL\";
        public RollForm(ApiService api, string engineType)
        {

            InitializeComponent();
            _api = api;
            _engineType = engineType;
        }

        private void btnEkleOk_Click(object sender, EventArgs e) => lbTumGorseller_DoubleClick(sender, e);

        private void lbYayinGorselleri_DoubleClick(object sender, EventArgs e)
        {
            if (lbYayinGorselleri.SelectedItem != null) lbYayinGorselleri.Items.Remove(lbYayinGorselleri.SelectedItem);
        }

        private void btnArayaEkle_Click(object sender, EventArgs e)
        {
            if (dgvRoll.SelectedRows.Count == 0) return;
            int eklenecekIndex = dgvRoll.SelectedRows[0].Index + 1;
            dgvRoll.Rows.Insert(eklenecekIndex, "", "YENİ BAŞLIK", "YENİ İSİM");
            SiraNumaralariniGuncelle();
            dgvRoll.ClearSelection();
            dgvRoll.Rows[eklenecekIndex].Selected = true;
        }

        private void btnYukariTasi_Click(object sender, EventArgs e)
        {
            if (dgvRoll.SelectedRows.Count == 0 || dgvRoll.SelectedRows[0].Index == 0) return;
            int seciliIndex = dgvRoll.SelectedRows[0].Index;
            DataGridViewRow satir = dgvRoll.Rows[seciliIndex];
            dgvRoll.Rows.RemoveAt(seciliIndex);
            dgvRoll.Rows.Insert(seciliIndex - 1, satir);
            dgvRoll.ClearSelection();
            dgvRoll.Rows[seciliIndex - 1].Selected = true;
            SiraNumaralariniGuncelle();
        }

        private void SponsorGorselleriniYukle()
        {
            lbTumGorseller.Items.Clear();
            lbYayinGorselleri.Items.Clear();

            if (Directory.Exists(_sponsorKlasorYolu))
            {
                var dosyalar = Directory.GetFiles(_sponsorKlasorYolu, "*.*")
                    .Where(s => s.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) ||
                                s.EndsWith(".png", StringComparison.OrdinalIgnoreCase)).ToArray();

                foreach (string dosya in dosyalar)
                    lbTumGorseller.Items.Add(Path.GetFileName(dosya));
            }
        }

        private void lbTumGorseller_DoubleClick(object sender, EventArgs e)
        {
            if (lbTumGorseller.SelectedItem != null)
            {
                if (lbYayinGorselleri.Items.Count >= 5)
                {
                    MessageBox.Show("Yayın havuzuna en fazla 5 sponsor ekleyebilirsiniz!", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                string secilen = lbTumGorseller.SelectedItem.ToString();
                if (!lbYayinGorselleri.Items.Contains(secilen)) lbYayinGorselleri.Items.Add(secilen);
            }
        }

        private void btnSatirSil_Click(object sender, EventArgs e)
        {
            if (dgvRoll.SelectedRows.Count == 0) return;
            dgvRoll.Rows.RemoveAt(dgvRoll.SelectedRows[0].Index);
            SiraNumaralariniGuncelle();
        }

        private async void btnRollVer_Click(object sender, EventArgs e)
        {
            if (_isSending) return;
            _isSending = true;

            btnRollVer.Text = "GÖNDERİLİYOR...";
            btnRollVer.Enabled = false;

            try
            {
                var request = new RollRequestDto
                {
                    TesekkurYazisi = txtTesekkur.Text.Trim(),
                    Satirlar = new List<RollSatirDto>(),
                    Sponsorlar = new List<string>()
                };

                for (int i = 0; i < 24; i++)
                {
                    if (i < dgvRoll.Rows.Count)
                    {
                        request.Satirlar.Add(new RollSatirDto
                        {
                            Baslik = dgvRoll.Rows[i].Cells["colUnvan"].Value?.ToString() ?? "",
                            Yazi = dgvRoll.Rows[i].Cells["colIsim"].Value?.ToString() ?? ""
                        });
                    }
                    else
                    {
                        request.Satirlar.Add(new RollSatirDto { Baslik = "", Yazi = "" });
                    }
                }

                foreach (var item in lbYayinGorselleri.Items)
                    request.Sponsorlar.Add(item.ToString());

                var result = await _api.RollVerAsync(_engineType, request);

                if (result.Success)
                {
                    btnRollVer.BackColor = Color.Red;
                    btnRollVer.ForeColor = Color.White;
                    btnRollVer.Text = "YAYINDA";
                }
                else
                {
                    MessageBox.Show($"Roll gönderilemedi: {result.Message}");
                    btnRollVer.Text = "ROLL VER";
                }
            }
            finally
            {
                _isSending = false;
                btnRollVer.Enabled = true;
            }
        }

        private async void btnRollAl_Click(object sender, EventArgs e)
        {
            if (_isSending) return;

            var result = await _api.RollAlAsync(_engineType);

            if (result.Success)
            {
                btnRollVer.BackColor = Color.LimeGreen;
                btnRollVer.ForeColor = Color.Black;
                btnRollVer.Text = "ROLL VER";
            }
        }

        private void RollForm_Load(object sender, EventArgs e)
        {
            this.Text = "Roll (Akan Yazı) Ayarları";

            // === Grid Görsel Ayarları (Senin eski kodun birebir aynısı) ===
            dgvRoll.AllowUserToAddRows = false;
            dgvRoll.AllowUserToResizeColumns = false;
            dgvRoll.AllowUserToResizeRows = false;
            dgvRoll.RowHeadersVisible = true;
            dgvRoll.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvRoll.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvRoll.MultiSelect = false;
            dgvRoll.DefaultCellStyle.WrapMode = DataGridViewTriState.True;
            dgvRoll.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
            dgvRoll.EditingControlShowing += dgvRoll_EditingControlShowing!;

            dgvRoll.DefaultCellStyle.SelectionBackColor = Color.LimeGreen;
            dgvRoll.DefaultCellStyle.SelectionForeColor = Color.Black;
            dgvRoll.EnableHeadersVisualStyles = false;
            dgvRoll.ColumnHeadersDefaultCellStyle.BackColor = Color.Black;
            dgvRoll.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvRoll.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            dgvRoll.DefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);

            // === Sütunları Ekle ===
            dgvRoll.Columns.Clear();
            dgvRoll.Columns.Add("colSira", "SIRA");
            dgvRoll.Columns.Add("colUnvan", "BAŞLIK");
            dgvRoll.Columns.Add("colIsim", "YAZI");

            dgvRoll.Columns["colSira"].AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            dgvRoll.Columns["colSira"].Width = 50;
            dgvRoll.Columns["colSira"].ReadOnly = true;

            // === Verileri Yükle ===
            SponsorGorselleriniYukle();
            VarsayilanVerileriOlustur(); // TXT yoksa oluşturur
            TxtDenVerileriYukle();       // TXT'den Grid'e basar
        }

        private void RollForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Form kapanırken son değişiklikleri TXT'ye kaydet
            VerileriTxtyeKaydet();
        }
        private void dgvRoll_EditingControlShowing(object sender, DataGridViewEditingControlShowingEventArgs e)
        {
            if (e.Control is TextBox tb)
            {
                tb.Multiline = true;
                tb.AcceptsReturn = true;
            }
        }
        private void VarsayilanVerileriOlustur()
        {
            if (File.Exists(_rollDosyaYolu)) return;

            string defaultRollData = @"TES|KEY|SHOW TV TEKNİK YAYIN EKİBİ<BR>SHOW TV GRAFİK VE TANITIM <BR>ARTİSTİK HİZMETLER DEPARTMANI<BR>SHOW TV HABER MERKEZİNE <BR>TEŞEKKÜR EDERİZ
YAPIMCI|UFUK COŞKUN
GENEL KOORDİNATÖR|ALİ KURT
EDİTÖR|ESRA DOĞAN<BR>UĞUR KESKİN
KONUK KOORDİNATÖRÜ|ASİYE ÜNLÜER
MUHABİR|EMEL KILIÇ
YAPIM KOORDİNATÖRÜ|ESRA PINAR AKPINAR
YAPIM ASİSTANI|TUANA UNAT
KURGU OPERATÖRÜ|SERCAN DOĞAN<BR>BERK DOĞANCIOĞLU
MALİ İŞLER|GÜLÇİN FERAH
SESLENDİRME (DIŞ SES)|FERHAT GÖKTAŞLAR
ULAŞTIRMA|EMİR İSA TÜRKMEN
STYLING|DERYA BALKAN
MAKYAJ|NİHAN SAĞIR
YÖNETMEN YARDIMCISI|ECEM METİN
YÖNETMEN|AYŞENUR YILDIRIM";

            File.WriteAllText(_rollDosyaYolu, defaultRollData);
        }

        private void TxtDenVerileriYukle()
        {
            dgvRoll.Rows.Clear();
            txtTesekkur.Text = "";

            if (!File.Exists(_rollDosyaYolu)) return;

            string[] satirlar = File.ReadAllLines(_rollDosyaYolu);
            foreach (string satir in satirlar)
            {
                if (string.IsNullOrWhiteSpace(satir)) continue;

                if (satir.StartsWith("TES|KEY|"))
                {
                    txtTesekkur.Text = satir.Substring(8).Replace("<BR>", Environment.NewLine);
                    continue;
                }

                string[] parcalar = satir.Split('|');
                int rowIndex = dgvRoll.Rows.Add();
                dgvRoll.Rows[rowIndex].Cells["colUnvan"].Value = parcalar[0].Replace("<BR>", Environment.NewLine);
                if (parcalar.Length > 1)
                    dgvRoll.Rows[rowIndex].Cells["colIsim"].Value = parcalar[1].Replace("<BR>", Environment.NewLine);
            }

            // Toplam 24 satır olacak şekilde boşlukları tamamla
            int mevcutSatir = dgvRoll.Rows.Count;
            for (int i = 0; i < (24 - mevcutSatir); i++) dgvRoll.Rows.Add("", "", "");

            SiraNumaralariniGuncelle();
        }

        private void VerileriTxtyeKaydet()
        {
            using (StreamWriter sw = new StreamWriter(_rollDosyaYolu, false))
            {
                string tesekkurMetni = txtTesekkur.Text.Replace("\r\n", "<BR>").Replace("\n", "<BR>");
                sw.WriteLine("TES|KEY|" + tesekkurMetni);

                foreach (DataGridViewRow row in dgvRoll.Rows)
                {
                    string unvan = row.Cells["colUnvan"].Value?.ToString() ?? "";
                    string isim = row.Cells["colIsim"].Value?.ToString() ?? "";

                    if (string.IsNullOrWhiteSpace(unvan) && string.IsNullOrWhiteSpace(isim)) continue;

                    unvan = unvan.Replace("\r\n", "<BR>").Replace("\n", "<BR>");
                    isim = isim.Replace("\r\n", "<BR>").Replace("\n", "<BR>");
                    sw.WriteLine($"{unvan}|{isim}");
                }
            }
        }

        private void SiraNumaralariniGuncelle()
        {
            for (int i = 0; i < dgvRoll.Rows.Count; i++)
                dgvRoll.Rows[i].Cells["colSira"].Value = (i + 1).ToString();
        }
        private void btnAsagiTasi_Click(object sender, EventArgs e)
        {
            if (dgvRoll.SelectedRows.Count == 0 || dgvRoll.SelectedRows[0].Index == dgvRoll.Rows.Count - 1) return;
            int seciliIndex = dgvRoll.SelectedRows[0].Index;
            DataGridViewRow satir = dgvRoll.Rows[seciliIndex];
            dgvRoll.Rows.RemoveAt(seciliIndex);
            dgvRoll.Rows.Insert(seciliIndex + 1, satir);
            dgvRoll.ClearSelection();
            dgvRoll.Rows[seciliIndex + 1].Selected = true;
            SiraNumaralariniGuncelle();
        }

    }
}
