using NTR.RejiClient.Forms;
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
            this.KeyDown += MainForm_KeyDown!;

            btnBaglan.Click += btnBaglan_Click!;
            btnTekliKJ.Click += btnTekliKJ_Click!;
            btnCiftliKJ.Click += btnCiftliKJ_Click!;
            btnUzunKJ.Click += btnUzunKJ_Click!;
            btnKJAl.Click += btnKJAl_Click!;
            btnTumunuAl.Click += btnTumunuAl_Click!;
            btnYerVer.Click += btnYerVer_Click!;
            btnYerAl.Click += btnYerAl_Click!;
            btnSosyalMedyaVer.Click += btnSosyalMedyaVer_Click!;
            btnSosyalMedyaAl.Click += btnSosyalMedyaAl_Click!;
            btnWhatsappVer.Click += btnWhatsappVer_Click!;
            btnWhatsappAl.Click += btnWhatsappAl_Click!;
            btnIsimlikVer.Click += btnIsimlikVer_Click!;
            btnIsimlikAl.Click += btnIsimlikAl_Click!;
            btnSunucuIsimlikVer.Click += btnSunucuIsimlikVer_Click!;
            btnSunucuIsimlikAl.Click += btnSunucuIsimlikAl_Click!;
            btnMuhabirKameramanVer.Click += btnMuhabirKameramanVer_Click!;
            btnMuhabirKameramanAl.Click += btnMuhabirKameramanAl_Click!;
            btnKaydet.Click += btnKaydet_Click!;
            btnSil.Click += btnSil_Click!;
            dgvHaberler.SelectionChanged += dgvHaberler_SelectionChanged!;
            dgvKjListesi.SelectionChanged += dgvKjListesi_SelectionChanged!;
            dtpTarih.ValueChanged += dtpTarih_ValueChanged!;
            cmbAkislar.SelectedIndexChanged += cmbAkislar_SelectedIndexChanged!;
            btnAkisYenile.Click += btnAkisYenile_Click!;
            // SetupForm() içindeki mevcut kodlara şunları ekle:
            cmbKanal.Items.Clear();
            cmbKanal.Items.Add("Show TV");
            cmbKanal.Items.Add("HaberTurk");
            cmbKanal.SelectedIndex = 0; // Varsayılan olarak ilk kanalı seç

            // Kanal değiştiğinde tetiklenecek event'i bağla
            cmbKanal.SelectedIndexChanged += cmbKanal_SelectedIndexChanged!;
        }
        private async void cmbKanal_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Kanal değiştiğinde o kanala ait akışları (Rundown) yeniden yükle
            if (_isConnected)
            {
                await LoadRundownAsync();
            }
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
            if (!_isConnected) return;

            string seciliTarih = dtpTarih.Value.ToString("yyyy-MM-dd");
            string seciliKanal = cmbKanal.SelectedItem?.ToString() ?? "Show TV";

            // 1. O güne ait tüm akışları API'den çek
            var tumRundownlar = await _api.GetRundownByTarihAsync(seciliTarih);

            // 2. Çekilen akışları seçili KANALA göre filtrele (Sihir burada)
            var rundownlar = tumRundownlar?.Where(r => r.Kanal == seciliKanal).ToList();

            // ComboBox değişirken arkada gereksiz yere haber çekmesin diye eventi koparıyoruz
            cmbAkislar.SelectedIndexChanged -= cmbAkislar_SelectedIndexChanged!;

            if (rundownlar != null && rundownlar.Count > 0)
            {
                cmbAkislar.DataSource = rundownlar;
                cmbAkislar.DisplayMember = "DisplayText"; // Artık "Ana Haber (20:00)" şeklinde görünecek
                cmbAkislar.ValueMember = "Id";
                cmbAkislar.SelectedIndex = 0;

                // Seçilen ilk akışın haberlerini getir
                await LoadHaberlerAsync(rundownlar[0].Id);
            }
            else
            {
                // O kanalda/tarihte akış yoksa ekranı temizle
                cmbAkislar.DataSource = null;
                cmbAkislar.Items.Clear();
                cmbAkislar.Items.Add("Bu kanalda akış yok");
                cmbAkislar.SelectedIndex = 0;

                dgvHaberler.Rows.Clear();
                dgvKjListesi.Rows.Clear();
                _haberler.Clear();
            }

            // İşlem bitince eventi geri bağla
            cmbAkislar.SelectedIndexChanged += cmbAkislar_SelectedIndexChanged!;
        }

        private async Task LoadHaberlerAsync(int rundownId)
        {
            // Olası SelectionChanged spamlarını önlüyoruz
            dgvHaberler.SelectionChanged -= dgvHaberler_SelectionChanged!;

            _haberler = await _api.GetHaberlerAsync(rundownId);
            dgvHaberler.Rows.Clear();

            if (_haberler != null && _haberler.Count > 0)
            {
                for (int i = 0; i < _haberler.Count; i++)
                {
                    dgvHaberler.Rows.Add((i + 1).ToString(), _haberler[i].Baslik);
                }

                // İlk haberi otomatik seçip KJ'lerini getir
                dgvHaberler.Rows[0].Selected = true;
                await LoadKjListesiAsync(_haberler[0].Id);
            }
            else
            {
                // Haberi olmayan akış gelirse KJ listesini de temizle
                dgvKjListesi.Rows.Clear();
            }

            // Eventi geri bağlıyoruz
            dgvHaberler.SelectionChanged += dgvHaberler_SelectionChanged!;
        }
        private async void dgvHaberler_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvHaberler.SelectedRows.Count == 0) return;

            int index = dgvHaberler.SelectedRows[0].Index;

            // Güvenlik duvarı: Grid'deki index, listemizin sınırlarını aşıyor mu?
            if (index < 0 || index >= _haberler.Count) return;

            await LoadKjListesiAsync(_haberler[index].Id);
        }

        private async Task LoadKjListesiAsync(int haberId)
        {
            _kjListesi = await _api.GetKjListesiAsync(haberId);
            dgvKjListesi.Rows.Clear();

            if (_kjListesi == null) return;

            for (int i = 0; i < _kjListesi.Count; i++)
            {

                string ekranaYazilacakTip = !string.IsNullOrWhiteSpace(_kjListesi[i].Aciklama)
                    ? _kjListesi[i].Aciklama
                    : (_kjListesi[i].Type == 0 ? "TEKLİ" : _kjListesi[i].Type == 1 ? "ÇİFTLİ" : "UZUN");

                dgvKjListesi.Rows.Add(
                    (i + 1).ToString(),
                    ekranaYazilacakTip, // Artık tabloya "TEKLİ" yerine veritabanındaki gerçek ismi basıyoruz!
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

            string gercekTip = !string.IsNullOrWhiteSpace(kj.Aciklama)
                ? kj.Aciklama
                : (kj.Type == 0 ? "TEKLİ KJ" : kj.Type == 1 ? "ÇİFTLİ KJ" : "UZUN KJ");

            lblSahneTipi.Text = $"SAHNE TİPİ: {gercekTip}";

            // Verileri operatörün müdahale edebileceği TextBox'lara basıyoruz!
            txtKjMetin1.Text = System.Text.RegularExpressions.Regex.Unescape(kj.Text1 ?? "");
            txtKjMetin2.Text = System.Text.RegularExpressions.Regex.Unescape(kj.Text2 ?? "");
        }
        // ─── YENİ EKLENEN EVENTLER ────────────────────────────────────────

        // ComboBox'tan başka bir akış seçilirse
        private async void cmbAkislar_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbAkislar.SelectedValue is int rundownId)
            {
                await LoadHaberlerAsync(rundownId);
            }
        }

        // Tarih değişirse
        private async void dtpTarih_ValueChanged(object sender, EventArgs e)
        {
            if (_isConnected) // Sadece bağlıysak API'ye istek atsın
            {
                await LoadRundownAsync();
            }
        }

        // Akış Yenile Butonu
        private async void btnAkisYenile_Click(object sender, EventArgs e)
        {
            if (_isConnected)
            {
                await LoadRundownAsync();
            }
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

            return (txtKjMetin1.Text.Trim(), txtKjMetin2.Text.Trim());
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

        // ─── MUHABİR / KAMERAMAN (OTOMATİK BULMA) ──────────────────────────────

        private async void btnMuhabirKameramanVer_Click(object sender, EventArgs e)
        {
            if (!_isConnected) return;

            // 1. Listeden Muhabir ve Kameramanı otomatik bul (Aciklama içinde arıyoruz)
            var muhabirKj = _kjListesi.FirstOrDefault(x => x.Aciklama.IndexOf("Muhabir", StringComparison.OrdinalIgnoreCase) >= 0);
            var kameramanKj = _kjListesi.FirstOrDefault(x => x.Aciklama.IndexOf("Kamera", StringComparison.OrdinalIgnoreCase) >= 0);

            // İkisi de yoksa uyarı ver
            if (muhabirKj == null && kameramanKj == null)
            {
                ShowError("Bu haberin KJ listesinde 'Muhabir' veya 'Kameraman' açıklamalı bir kayıt bulunamadı!");
                return;
            }

            // 2. İsimleri Text1 alanından al (Eğer null ise boş string gitmesini sağlıyoruz)
            string muhabirIsim = muhabirKj?.Text1 ?? "";
            string kameramanIsim = kameramanKj?.Text1 ?? "";

            // 3. API'ye gönder
            var result = await _api.MuhabirKameraVerAsync(_engineType, muhabirIsim, kameramanIsim);

            if (result.Success)
            {
                // 4. Buton renklerini güncelle
                btnMuhabirKameramanVer.BackColor = OnAirColor;
                btnMuhabirKameramanVer.ForeColor = Color.White;
                btnMuhabirKameramanVer.Text = "YAYINDA";
                btnMuhabirKameramanAl.BackColor = AlColor;
                btnMuhabirKameramanAl.Enabled = true;

                // 5. Grid (Tablo) üzerindeki ilgili satırları kırmızıya (Yayında) boya
                int muhabirIndex = muhabirKj != null ? _kjListesi.IndexOf(muhabirKj) : -1;
                int kameraIndex = kameramanKj != null ? _kjListesi.IndexOf(kameramanKj) : -1;

                if (muhabirIndex >= 0)
                {
                    dgvKjListesi.Rows[muhabirIndex].DefaultCellStyle.BackColor = OnAirColor;
                    dgvKjListesi.Rows[muhabirIndex].DefaultCellStyle.ForeColor = Color.White;
                    dgvKjListesi.Rows[muhabirIndex].DefaultCellStyle.SelectionBackColor = Color.DarkRed;
                }

                if (kameraIndex >= 0)
                {
                    dgvKjListesi.Rows[kameraIndex].DefaultCellStyle.BackColor = OnAirColor;
                    dgvKjListesi.Rows[kameraIndex].DefaultCellStyle.ForeColor = Color.White;
                    dgvKjListesi.Rows[kameraIndex].DefaultCellStyle.SelectionBackColor = Color.DarkRed;
                }

                // Seçimi en alttaki KJ'ye veya bir sonrakine atlat
                int maxIndex = Math.Max(muhabirIndex, kameraIndex);
                if (maxIndex != -1 && maxIndex < dgvKjListesi.Rows.Count - 1)
                {
                    dgvKjListesi.ClearSelection();
                    dgvKjListesi.Rows[maxIndex + 1].Selected = true;
                }
            }
            else
            {
                ShowError($"Hata: {result.Message}");
            }
        }

        private async void btnMuhabirKameramanAl_Click(object sender, EventArgs e)
        {
            if (!_isConnected) return;

            var result = await _api.MuhabirKameraAlAsync(_engineType);

            if (result.Success)
            {
                // Butonları eski haline getir
                btnMuhabirKameramanVer.BackColor = OffAirColor;
                btnMuhabirKameramanVer.ForeColor = Color.Black;
                btnMuhabirKameramanVer.Text = "MUHABİR KAMERAMAN VER";
                btnMuhabirKameramanAl.BackColor = AlColor;

                // Grid üzerindeki Kırmızı satırların boyasını temizle
                var muhabirKj = _kjListesi.FirstOrDefault(x => x.Aciklama.IndexOf("Muhabir", StringComparison.OrdinalIgnoreCase) >= 0);
                var kameramanKj = _kjListesi.FirstOrDefault(x => x.Aciklama.IndexOf("Kamera", StringComparison.OrdinalIgnoreCase) >= 0);

                int muhabirIndex = muhabirKj != null ? _kjListesi.IndexOf(muhabirKj) : -1;
                int kameraIndex = kameramanKj != null ? _kjListesi.IndexOf(kameramanKj) : -1;

                if (muhabirIndex >= 0)
                {
                    dgvKjListesi.Rows[muhabirIndex].DefaultCellStyle.BackColor = Color.Empty;
                    dgvKjListesi.Rows[muhabirIndex].DefaultCellStyle.ForeColor = Color.Empty;
                    dgvKjListesi.Rows[muhabirIndex].DefaultCellStyle.SelectionBackColor = Color.Empty;
                }
                if (kameraIndex >= 0)
                {
                    dgvKjListesi.Rows[kameraIndex].DefaultCellStyle.BackColor = Color.Empty;
                    dgvKjListesi.Rows[kameraIndex].DefaultCellStyle.ForeColor = Color.Empty;
                    dgvKjListesi.Rows[kameraIndex].DefaultCellStyle.SelectionBackColor = Color.Empty;
                }
            }
        }

        // ─── KJ CRUD ─────────────────────────────────────────────

        private async void btnKaydet_Click(object sender, EventArgs e)
        {
            if (!_isConnected) return;
            if (dgvKjListesi.SelectedRows.Count == 0)
            {
                ShowError("Lütfen önce listeden güncellenecek KJ'yi seçin!");
                return;
            }

            // Seçili satırın indeksini ve orjinal KJ modelini al
            int index = dgvKjListesi.SelectedRows[0].Index;
            var kj = _kjListesi[index];

            // TextBox'taki GÜNCEL veriyi modele aktar
            kj.Text1 = txtKjMetin1.Text;
            kj.Text2 = txtKjMetin2.Text;

            // API'ye güncelleme isteği at
            var result = await _api.KjGuncelleAsync(kj.Id, kj.HaberId, kj.Aciklama, kj.Type, kj.Text1, kj.Text2);

            if (result.Success)
            {
                // Başarılıysa, API'den tüm listeyi baştan çekmek yerine (hız için) 
                // doğrudan ekrandaki tabloyu güncelliyoruz!
                dgvKjListesi.Rows[index].Cells[2].Value = kj.Text1;
                dgvKjListesi.Rows[index].Cells[3].Value = kj.Text2;

                // (İsteğe bağlı) ShowInfo("KJ güncellendi"); // Sürekli pop-up çıkmasın diye bunu silebilirsin.
            }
            else
            {
                ShowError($"Güncelleme başarısız: {result.Message}");
            }
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

        private void btnRollEkraniAc_Click(object sender, EventArgs e)
        {
            if (!_isConnected)
            {
                ShowError("Önce API'ye bağlanmalısınız!");
                return;
            }

            // _engineType'ı da gönderiyoruz (Eğer _engineType sende enum ise _engineType.ToString() yapabilirsin)
            RollForm rollForm = new RollForm(_api, _engineType.ToString());
            rollForm.Show();
        }

        private void btnKelebek_Click(object sender, EventArgs e)
        {
            if (!_isConnected)
            {
                ShowError("Önce API'ye bağlanmalısınız!");
                return;
            }

            // Roll ekranında '_api' kullandığın için burada da '_api' kullanmalısın.
            // Ayrıca Kelebek formuna _config nesnesini de gönderiyoruz.
            Kelebek kelebekFormu = new Kelebek(_api, _config);
            kelebekFormu.Show();
        }
    }
}