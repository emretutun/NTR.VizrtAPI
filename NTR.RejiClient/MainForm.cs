using NTR.RejiClient.Models;
using NTR.RejiClient.Services;

namespace NTR.RejiClient
{
    public partial class MainForm : Form
    {
        private ApiService _api;
        private AppConfig _config;
        private string _engineType = "Reji";
        private List<Haber> _haberler = new();
        private List<KjItem> _kjListesi = new();
        private bool _isConnected = false;

        // Renk sabitleri
        private readonly Color OnAirColor = Color.Red;
        private readonly Color OffAirColor = Color.LimeGreen;
        private readonly Color AlColor = SystemColors.ControlLight;

        public MainForm()
        {
            InitializeComponent();
            _config = AppConfig.Load();
            _api = new ApiService(_config.ApiBaseUrl, _config.ApiKey);
            txtIpAdresi.Text = _config.LastIp;
            SetupForm();
            SetupDgvHaberler();
            SetupDgvKjListesi();
        }

        // ─── SETUP ────────────────────────────────────────────────

        private void SetupForm()
        {
            this.Text = "NTR Reji Client - Show TV";
            this.KeyPreview = true;
            this.KeyDown += MainForm_KeyDown;

            btnBaglan.Click += btnBaglan_Click;
            btnTekliKJ.Click += btnTekliKJ_Click;
            btnCiftliKJ.Click += btnCiftliKJ_Click;
            btnUzunKJ.Click += btnUzunKJ_Click;
            btnKJAl.Click += btnKJAl_Click;
            btnTumunuAl.Click += btnTumunuAl_Click;
            btnYerVer.Click += btnYerVer_Click;
            btnYerAl.Click += btnYerAl_Click;
            btnSosyalMedyaVer.Click += btnSosyalMedyaVer_Click;
            btnSosyalMedyaAl.Click += btnSosyalMedyaAl_Click;
            btnWhatsappVer.Click += btnWhatsappVer_Click;
            btnWhatsappAl.Click += btnWhatsappAl_Click;
            btnIsimlikVer.Click += btnIsimlikVer_Click;
            btnIsimlikAl.Click += btnIsimlikAl_Click;
            btnSunucuIsimlikVer.Click += btnSunucuIsimlikVer_Click;
            btnSunucuIsimlikAl.Click += btnSunucuIsimlikAl_Click;
            btnMuhabirKameramanVer.Click += btnMuhabirKameramanVer_Click;
            btnMuhabirKameramanAl.Click += btnMuhabirKameramanAl_Click;
            btnKaydet.Click += btnKaydet_Click;
            btnSil.Click += btnSil_Click;
            dgvHaberler.SelectionChanged += dgvHaberler_SelectionChanged;
            dgvKjListesi.SelectionChanged += dgvKjListesi_SelectionChanged;
        }

        private void SetupDgvHaberler()
        {
            dgvHaberler.Columns.Clear();
            dgvHaberler.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colSira",
                HeaderText = "S",
                Width = 30,
                ReadOnly = true
            });
            dgvHaberler.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colBaslik",
                HeaderText = "HABERLER",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                ReadOnly = true
            });
            dgvHaberler.RowHeadersVisible = false;
            dgvHaberler.MultiSelect = false;
            dgvHaberler.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            dgvHaberler.RowTemplate.Height = 30;
            dgvHaberler.DefaultCellStyle.SelectionBackColor = Color.DarkBlue;
            dgvHaberler.DefaultCellStyle.SelectionForeColor = Color.White;
        }

        private void SetupDgvKjListesi()
        {
            dgvKjListesi.Columns.Clear();
            dgvKjListesi.Columns.Add(new DataGridViewTextBoxColumn { Name = "colSira", HeaderText = "#", Width = 35, ReadOnly = true });
            dgvKjListesi.Columns.Add(new DataGridViewTextBoxColumn { Name = "colTip", HeaderText = "TİP", Width = 80, ReadOnly = true });
            dgvKjListesi.Columns.Add(new DataGridViewTextBoxColumn { Name = "colMetin1", HeaderText = "METİN 1", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill, ReadOnly = true });
            dgvKjListesi.Columns.Add(new DataGridViewTextBoxColumn { Name = "colMetin2", HeaderText = "METİN 2", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill, ReadOnly = true });
            dgvKjListesi.RowHeadersVisible = false;
            dgvKjListesi.MultiSelect = false;
            dgvKjListesi.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            dgvKjListesi.RowTemplate.Height = 30;
            dgvKjListesi.DefaultCellStyle.SelectionBackColor = Color.DarkBlue;
            dgvKjListesi.DefaultCellStyle.SelectionForeColor = Color.White;
        }

        // ─── BAĞLANTI ─────────────────────────────────────────────

        private async void btnBaglan_Click(object sender, EventArgs e)
        {
            if (!_isConnected)
            {
                string ip = txtIpAdresi.Text.Trim();

                // Önce Vizrt engine'e bağlan
                var engineResult = await _api.ConnectAsync(_engineType, ip);
                if (!engineResult.Success)
                {
                    ShowError($"Engine bağlantısı kurulamadı: {engineResult.Message}");
                    return;
                }

                // Scene yükle
                var sceneResult = await _api.LoadSceneAsync(_engineType, _config.ScenePath);
                if (!sceneResult.Success)
                    ShowError($"Scene yüklenemedi: {sceneResult.Message}");

                _isConnected = true;
                btnBaglan.Text = "BAĞLI ✓";
                btnBaglan.BackColor = Color.LimeGreen;
                _config.LastIp = ip;
                _config.Save();

                // Haberleri yükle
                await LoadRundownAsync();
                ShowInfo($"Bağlantı kuruldu. IP: {ip}");
            }
            else
            {
                await _api.DisconnectAsync(_engineType);
                _isConnected = false;
                btnBaglan.Text = "BAĞLAN";
                btnBaglan.BackColor = SystemColors.Control;
                dgvHaberler.Rows.Clear();
                dgvKjListesi.Rows.Clear();
                ShowInfo("Bağlantı kesildi.");
            }
        }

        // ─── RUNDOWN & HABER ──────────────────────────────────────

        private async Task LoadRundownAsync()
        {
            string bugun = DateTime.Now.ToString("yyyy-MM-dd");
            var rundownlar = await _api.GetRundownByTarihAsync(bugun);
            if (rundownlar.Count > 0)
            {
                await LoadHaberlerAsync(rundownlar[0].Id);
            }
        }

        private async Task LoadHaberlerAsync(int rundownId)
        {
            _haberler = await _api.GetHaberlerAsync(rundownId);
            dgvHaberler.Rows.Clear();
            for (int i = 0; i < _haberler.Count; i++)
            {
                dgvHaberler.Rows.Add((i + 1).ToString(), _haberler[i].Baslik);
            }
        }

        private async void dgvHaberler_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvHaberler.SelectedRows.Count == 0) return;
            int index = dgvHaberler.SelectedRows[0].Index;
            if (index >= _haberler.Count) return;
            await LoadKjListesiAsync(_haberler[index].Id);
        }

        private async Task LoadKjListesiAsync(int haberId)
        {
            _kjListesi = await _api.GetKjListesiAsync(haberId);
            dgvKjListesi.Rows.Clear();
            for (int i = 0; i < _kjListesi.Count; i++)
            {
                string tip = _kjListesi[i].Type == 0 ? "TEKLİ" :
                             _kjListesi[i].Type == 1 ? "ÇİFTLİ" : "UZUN";
                dgvKjListesi.Rows.Add(
                    (i + 1).ToString(),
                    tip,
                    _kjListesi[i].Text1,
                    _kjListesi[i].Text2);
            }
        }

        private void dgvKjListesi_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvKjListesi.SelectedRows.Count == 0) return;
            int index = dgvKjListesi.SelectedRows[0].Index;
            if (index >= _kjListesi.Count) return;

            var kj = _kjListesi[index];
            string tip = kj.Type == 0 ? "TEKLİ KJ" :
                         kj.Type == 1 ? "ÇİFTLİ KJ" : "UZUN KJ";
            lblSahneTipi.Text = $"SAHNE TİPİ: {tip}";
            txtKjMetin1.Text = kj.Text1;
            txtKjMetin2.Text = kj.Text2;
        }

        // ─── KJ BUTONLARI ─────────────────────────────────────────

        private int? GetAktifRozet()
        {
            if (rbAzSonra.Checked) return 0;
            if (rbAzSonraDSF.Checked) return 1;
            if (rbAzSonraDSFv2.Checked) return 2;
            if (rbSonDakika.Checked) return 3;
            if (rbOzelHaber.Checked) return 4;
            if (rbWhatsappIhbar.Checked) return 5;
            return null;
        }

        private (string text1, string text2) GetKjMetin()
        {
            if (cbxAcilDurum.Checked)
                return (txtKjMetin1.Text, txtKjMetin2.Text);

            if (dgvKjListesi.SelectedRows.Count == 0)
                return (string.Empty, string.Empty);

            int index = dgvKjListesi.SelectedRows[0].Index;
            if (index >= _kjListesi.Count)
                return (string.Empty, string.Empty);

            return (_kjListesi[index].Text1, _kjListesi[index].Text2);
        }

        private async void btnTekliKJ_Click(object sender, EventArgs e)
        {
            var (text1, _) = GetKjMetin();
            if (string.IsNullOrWhiteSpace(text1)) { ShowError("Metin boş olamaz!"); return; }
            var result = await _api.KjVerAsync(_engineType, 0, text1, "", GetAktifRozet());
            HandleResult(result, btnTekliKJ, btnKJAl);
            SelectNextKj();
        }

        private async void btnCiftliKJ_Click(object sender, EventArgs e)
        {
            var (text1, text2) = GetKjMetin();
            if (string.IsNullOrWhiteSpace(text1) || string.IsNullOrWhiteSpace(text2))
            { ShowError("Çift satır KJ için her iki metin de dolu olmalı!"); return; }
            var result = await _api.KjVerAsync(_engineType, 1, text1, text2, GetAktifRozet());
            HandleResult(result, btnCiftliKJ, btnKJAl);
            SelectNextKj();
        }

        private async void btnUzunKJ_Click(object sender, EventArgs e)
        {
            var (text1, text2) = GetKjMetin();
            if (string.IsNullOrWhiteSpace(text1) || string.IsNullOrWhiteSpace(text2))
            { ShowError("Uzun KJ için her iki metin de dolu olmalı!"); return; }
            var result = await _api.KjVerAsync(_engineType, 2, text1, text2, GetAktifRozet());
            HandleResult(result, btnUzunKJ, btnKJAl);
            SelectNextKj();
        }

        private async void btnKJAl_Click(object sender, EventArgs e)
        {
            var result = await _api.KjAlAsync(_engineType);
            if (result.Success)
            {
                btnTekliKJ.BackColor = OffAirColor;
                btnCiftliKJ.BackColor = OffAirColor;
                btnUzunKJ.BackColor = OffAirColor;
                btnKJAl.BackColor = SystemColors.Control;
            }
        }

        private async void btnTumunuAl_Click(object sender, EventArgs e)
        {
            var result = await _api.TumunuAlAsync(_engineType);
            if (result.Success)
            {
                ResetAllButtons();
                ShowInfo("Tüm grafikler alındı.");
            }
        }

        // ─── YER ─────────────────────────────────────────────────

        private async void btnYerVer_Click(object sender, EventArgs e)
        {
            var (text1, _) = GetKjMetin();
            if (string.IsNullOrWhiteSpace(text1)) { ShowError("Yer metni boş olamaz!"); return; }
            var result = await _api.YerVerAsync(_engineType, text1);
            HandleResult(result, btnYerVer, btnYerAl);
            SelectNextKj();
        }

        private async void btnYerAl_Click(object sender, EventArgs e)
        {
            var result = await _api.YerAlAsync(_engineType);
            if (result.Success)
            {
                btnYerVer.BackColor = OffAirColor;
                btnYerAl.BackColor = AlColor;
            }
        }

        // ─── SOSYAL MEDYA ─────────────────────────────────────────

        private async void btnSosyalMedyaVer_Click(object sender, EventArgs e)
        {
            var result = await _api.SosyalMedyaVerAsync(_engineType);
            HandleResult(result, btnSosyalMedyaVer, btnSosyalMedyaAl);
        }

        private async void btnSosyalMedyaAl_Click(object sender, EventArgs e)
        {
            var result = await _api.SosyalMedyaAlAsync(_engineType);
            if (result.Success)
            {
                btnSosyalMedyaVer.BackColor = OffAirColor;
                btnSosyalMedyaAl.BackColor = AlColor;
            }
        }

        private async void btnWhatsappVer_Click(object sender, EventArgs e)
        {
            var result = await _api.WhatsappVerAsync(_engineType);
            HandleResult(result, btnWhatsappVer, btnWhatsappAl);
        }

        private async void btnWhatsappAl_Click(object sender, EventArgs e)
        {
            var result = await _api.WhatsappAlAsync(_engineType);
            if (result.Success)
            {
                btnWhatsappVer.BackColor = OffAirColor;
                btnWhatsappAl.BackColor = AlColor;
            }
        }

        // ─── İSİMLİK ─────────────────────────────────────────────

        private async void btnIsimlikVer_Click(object sender, EventArgs e)
        {
            string isim = cmbIsim.Text.Trim();
            string title = cmbTitle.Text.Trim();
            if (string.IsNullOrWhiteSpace(isim)) { ShowError("İsim boş olamaz!"); return; }
            var result = await _api.TelefonIsimlikVerAsync(_engineType, isim, title, cbxTelefon.Checked);
            HandleResult(result, btnIsimlikVer, btnIsimlikAl);
        }

        private async void btnIsimlikAl_Click(object sender, EventArgs e)
        {
            var result = await _api.TelefonIsimlikAlAsync(_engineType);
            if (result.Success)
            {
                btnIsimlikVer.BackColor = OffAirColor;
                btnIsimlikAl.BackColor = AlColor;
            }
        }

        private async void btnSunucuIsimlikVer_Click(object sender, EventArgs e)
        {
            var result = await _api.IsimlikVerAsync(_engineType, "SUNUCU");
            HandleResult(result, btnSunucuIsimlikVer, btnSunucuIsimlikAl);
        }

        private async void btnSunucuIsimlikAl_Click(object sender, EventArgs e)
        {
            var result = await _api.IsimlikAlAsync(_engineType);
            if (result.Success)
            {
                btnSunucuIsimlikVer.BackColor = OffAirColor;
                btnSunucuIsimlikAl.BackColor = AlColor;
            }
        }

        // ─── MUHABİR KAMERAMAN ───────────────────────────────────

        private async void btnMuhabirKameramanVer_Click(object sender, EventArgs e)
        {
            if (dgvKjListesi.SelectedRows.Count == 0) { ShowError("KJ seçiniz!"); return; }
            int index = dgvKjListesi.SelectedRows[0].Index;
            var kj = _kjListesi[index];
            var result = await _api.MuhabirKameraVerAsync(_engineType, kj.Text1, kj.Text2);
            HandleResult(result, btnMuhabirKameramanVer, btnMuhabirKameramanAl);
        }

        private async void btnMuhabirKameramanAl_Click(object sender, EventArgs e)
        {
            var result = await _api.MuhabirKameraAlAsync(_engineType);
            if (result.Success)
            {
                btnMuhabirKameramanVer.BackColor = OffAirColor;
                btnMuhabirKameramanAl.BackColor = AlColor;
            }
        }

        // ─── KJ CRUD ─────────────────────────────────────────────

        private async void btnKaydet_Click(object sender, EventArgs e)
        {
            if (dgvKjListesi.SelectedRows.Count == 0) return;
            int index = dgvKjListesi.SelectedRows[0].Index;
            var kj = _kjListesi[index];
            kj.Text1 = txtKjMetin1.Text;
            kj.Text2 = txtKjMetin2.Text;
            var result = await _api.KjGuncelleAsync(kj.Id, kj.HaberId, kj.Aciklama, kj.Type, kj.Text1, kj.Text2);
            if (result.Success)
            {
                ShowInfo("KJ kaydedildi.");
                if (dgvHaberler.SelectedRows.Count > 0)
                    await LoadKjListesiAsync(_haberler[dgvHaberler.SelectedRows[0].Index].Id);
            }
            else ShowError(result.Message);
        }

        private async void btnSil_Click(object sender, EventArgs e)
        {
            if (dgvKjListesi.SelectedRows.Count == 0) return;
            if (MessageBox.Show("KJ silinecek, emin misiniz?", "Sil", MessageBoxButtons.YesNo) != DialogResult.Yes) return;
            int index = dgvKjListesi.SelectedRows[0].Index;
            var result = await _api.KjSilAsync(_kjListesi[index].Id);
            if (result.Success)
            {
                ShowInfo("KJ silindi.");
                if (dgvHaberler.SelectedRows.Count > 0)
                    await LoadKjListesiAsync(_haberler[dgvHaberler.SelectedRows[0].Index].Id);
            }
            else ShowError(result.Message);
        }

        // ─── KLAVYE ───────────────────────────────────────────────

        private void MainForm_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.F3: btnSosyalMedyaVer_Click(sender, e); break;
                case Keys.F4: btnWhatsappVer_Click(sender, e); break;
                case Keys.F5: btnSosyalMedyaAl_Click(sender, e); break;
                case Keys.F7: btnKJAl_Click(sender, e); break;
                case Keys.F8: btnTumunuAl_Click(sender, e); break;
            }
        }

        // ─── YARDIMCI METODLAR ────────────────────────────────────

        private void HandleResult(ApiResult result, Button verBtn, Button alBtn)
        {
            if (result.Success)
            {
                verBtn.BackColor = OnAirColor;
                verBtn.ForeColor = Color.White;
                alBtn.BackColor = AlColor;
            }
            else
            {
                ShowError(result.Message);
            }
        }

        private void ResetAllButtons()
        {
            var buttons = new[] { btnTekliKJ, btnCiftliKJ, btnUzunKJ,
                                   btnYerVer, btnSosyalMedyaVer, btnWhatsappVer,
                                   btnIsimlikVer, btnSunucuIsimlikVer, btnMuhabirKameramanVer };
            foreach (var btn in buttons)
            {
                btn.BackColor = OffAirColor;
                btn.ForeColor = Color.Black;
            }
            btnKJAl.BackColor = SystemColors.Control;
            btnYerAl.BackColor = AlColor;
            btnSosyalMedyaAl.BackColor = AlColor;
            btnWhatsappAl.BackColor = AlColor;
            btnIsimlikAl.BackColor = AlColor;
            btnSunucuIsimlikAl.BackColor = AlColor;
            btnMuhabirKameramanAl.BackColor = AlColor;
        }

        private void SelectNextKj()
        {
            if (dgvKjListesi.Rows.Count == 0 || dgvKjListesi.SelectedRows.Count == 0) return;
            int current = dgvKjListesi.SelectedRows[0].Index;
            if (current < dgvKjListesi.Rows.Count - 1)
            {
                dgvKjListesi.ClearSelection();
                dgvKjListesi.Rows[current + 1].Selected = true;
                dgvKjListesi.FirstDisplayedScrollingRowIndex = current + 1;
            }
        }

        private void ShowInfo(string msg) =>
            MessageBox.Show(msg, "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);

        private void ShowError(string msg) =>
            MessageBox.Show(msg, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);

        private void splitContainer1_Panel2_Paint(object sender, PaintEventArgs e) { }

        private void btnBaglan_Click_1(object sender, EventArgs e)
        {

        }
    }
}