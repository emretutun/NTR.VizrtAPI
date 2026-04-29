//UI Tasarımı Yapılacak o zaman kadar yorum satırına alınmıştır.


/*using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using NTR.RejiClient.Models;
using NTR.RejiClient.Services;

namespace NTR.RejiClient
{
    public partial class MainForm : Form
    {
        private ApiService _api;
        private AppConfig _config;
        private System.Windows.Forms.Timer _statusTimer;
        private string _engineType => _config.EngineType;

        // OnAir durumları
        private bool _isKjOnAir = false;
        private bool _isYerOnAir = false;
        private bool _isSosyalOnAir = false;
        private bool _isWhatsappOnAir = false;
        private bool _isIsimlikOnAir = false;
        private bool _isTelefonIsimlikOnAir = false;
        private bool _isMuhabirKameraOnAir = false;
        private bool _isCanliOnAir = false;
        private bool _isCanliYerOnAir = false;
        private string? _aktifRozet = null;

        // Renkler
        private readonly Color OnAirColor = Color.Red;
        private readonly Color OnAirForeColor = Color.White;
        private readonly Color OffAirColor = Color.FromArgb(0, 122, 204);
        private readonly Color OffAirForeColor = Color.White;
        private readonly Color AlColor = Color.FromArgb(60, 60, 60);
        private readonly Color AlForeColor = Color.White;
        private readonly Color ConnectedColor = Color.LimeGreen;
        private readonly Color DisconnectedColor = Color.Maroon;

        public MainForm()
        {
            InitializeComponent();
            _config = AppConfig.Load();
            _api = new ApiService(_config.ApiBaseUrl, _config.ApiKey);
            InitializeStatusTimer();
            SetupForm();
        }

        private void SetupForm()
        {
            this.Text = "NTR Reji Client v1.0";
            this.MinimumSize = new Size(1200, 800);
            this.BackColor = Color.FromArgb(30, 30, 30);
            this.ForeColor = Color.White;
        }

        private void InitializeStatusTimer()
        {
            _statusTimer = new System.Windows.Forms.Timer();
            _statusTimer.Interval = 3000;
            _statusTimer.Tick += async (s, e) => await RefreshEngineStatus();
            _statusTimer.Start();
        }

        private async Task RefreshEngineStatus()
        {
            try
            {
                var statusList = await _api.GetAllEngineStatusAsync();
                var reji = statusList.FirstOrDefault(x => x.Name == "viz-KJ");
                if (reji != null)
                    UpdateConnectionUI(reji.IsConnected);
            }
            catch { }
        }

        private void UpdateConnectionUI(bool isConnected)
        {
            if (InvokeRequired)
            {
                Invoke(() => UpdateConnectionUI(isConnected));
                return;
            }

            btn_Connect.BackColor = isConnected ? ConnectedColor : DisconnectedColor;
            btn_Connect.Text = isConnected ? "BAĞLI" : "BAĞLAN";
            lbl_ConnectionStatus.Text = isConnected ? $"● BAĞLI - {_config.LastIp}" : "● BAĞLANTI YOK";
            lbl_ConnectionStatus.ForeColor = isConnected ? Color.LimeGreen : Color.Red;
        }

        private void SetButtonOnAir(Button btn, bool onAir)
        {
            btn.BackColor = onAir ? OnAirColor : OffAirColor;
            btn.ForeColor = onAir ? OnAirForeColor : OffAirForeColor;
        }

        private async void btn_Connect_Click(object sender, EventArgs e)
        {
            if (btn_Connect.BackColor == ConnectedColor)
            {
                await _api.DisconnectAsync(_engineType);
                UpdateConnectionUI(false);
            }
            else
            {
                var result = await _api.ConnectAsync(_engineType, _config.LastIp, _config.LastPort);
                UpdateConnectionUI(result.Success);
                if (result.Success)
                {
                    await _api.LoadSceneAsync(_engineType, _config.ApiBaseUrl);
                }
                if (!result.Success)
                    MessageBox.Show($"Bağlantı hatası: {result.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ─── KJ ──────────────────────────────────────────────────

        private async void btn_TekliVer_Click(object sender, EventArgs e)
        {
            string text1 = txt_KjUst.Text.Trim();
            if (string.IsNullOrWhiteSpace(text1)) { MessageBox.Show("Metin boş olamaz!"); return; }

            int? rozet = GetSeciliRozet();
            var result = await _api.KjVerAsync(_engineType, 0, text1, "", rozet);
            if (result.Success)
            {
                _isKjOnAir = true;
                SetButtonOnAir(btn_TekliVer, true);
                SetButtonOnAir(btn_CiftliVer, false);
                SetButtonOnAir(btn_UzunVer, false);
                btn_KjAl.Enabled = true;
            }
            else MessageBox.Show(result.Message);
        }

        private async void btn_CiftliVer_Click(object sender, EventArgs e)
        {
            string text1 = txt_KjUst.Text.Trim();
            string text2 = txt_KjAlt.Text.Trim();
            if (string.IsNullOrWhiteSpace(text1) || string.IsNullOrWhiteSpace(text2))
            { MessageBox.Show("Her iki satır dolu olmalı!"); return; }

            int? rozet = GetSeciliRozet();
            var result = await _api.KjVerAsync(_engineType, 1, text1, text2, rozet);
            if (result.Success)
            {
                _isKjOnAir = true;
                SetButtonOnAir(btn_TekliVer, false);
                SetButtonOnAir(btn_CiftliVer, true);
                SetButtonOnAir(btn_UzunVer, false);
                btn_KjAl.Enabled = true;
            }
            else MessageBox.Show(result.Message);
        }

        private async void btn_UzunVer_Click(object sender, EventArgs e)
        {
            string text1 = txt_KjUst.Text.Trim();
            string text2 = txt_KjAlt.Text.Trim();
            if (string.IsNullOrWhiteSpace(text1) || string.IsNullOrWhiteSpace(text2))
            { MessageBox.Show("Her iki satır dolu olmalı!"); return; }

            int? rozet = GetSeciliRozet();
            var result = await _api.KjVerAsync(_engineType, 2, text1, text2, rozet);
            if (result.Success)
            {
                _isKjOnAir = true;
                SetButtonOnAir(btn_TekliVer, false);
                SetButtonOnAir(btn_CiftliVer, false);
                SetButtonOnAir(btn_UzunVer, true);
                btn_KjAl.Enabled = true;
            }
            else MessageBox.Show(result.Message);
        }

        private async void btn_KjAl_Click(object sender, EventArgs e)
        {
            var result = await _api.KjAlAsync(_engineType);
            if (result.Success)
            {
                _isKjOnAir = false;
                _aktifRozet = null;
                SetButtonOnAir(btn_TekliVer, false);
                SetButtonOnAir(btn_CiftliVer, false);
                SetButtonOnAir(btn_UzunVer, false);
                btn_KjAl.Enabled = false;
                ResetRozetButtons();
            }
        }

        private async void btn_TumunuAl_Click(object sender, EventArgs e)
        {
            var result = await _api.TumunuAlAsync(_engineType);
            if (result.Success) ResetAllOnAirStates();
            else MessageBox.Show(result.Message);
        }

        // ─── YER ─────────────────────────────────────────────────

        private async void btn_YerVer_Click(object sender, EventArgs e)
        {
            string text = txt_Yer.Text.Trim();
            if (string.IsNullOrWhiteSpace(text)) { MessageBox.Show("Yer metni boş olamaz!"); return; }

            var result = await _api.YerVerAsync(_engineType, text);
            if (result.Success) { _isYerOnAir = true; SetButtonOnAir(btn_YerVer, true); btn_YerAl.Enabled = true; }
            else MessageBox.Show(result.Message);
        }

        private async void btn_YerAl_Click(object sender, EventArgs e)
        {
            var result = await _api.YerAlAsync(_engineType);
            if (result.Success) { _isYerOnAir = false; SetButtonOnAir(btn_YerVer, false); btn_YerAl.Enabled = false; }
        }

        // ─── SOSYAL MEDYA ─────────────────────────────────────────

        private async void btn_SosyalVer_Click(object sender, EventArgs e)
        {
            var result = await _api.SosyalMedyaVerAsync(_engineType);
            if (result.Success)
            {
                _isSosyalOnAir = true;
                _isWhatsappOnAir = false;
                SetButtonOnAir(btn_SosyalVer, true);
                SetButtonOnAir(btn_WhatsappVer, false);
            }
        }

        private async void btn_WhatsappVer_Click(object sender, EventArgs e)
        {
            var result = await _api.WhatsappVerAsync(_engineType);
            if (result.Success)
            {
                _isWhatsappOnAir = true;
                _isSosyalOnAir = false;
                SetButtonOnAir(btn_WhatsappVer, true);
                SetButtonOnAir(btn_SosyalVer, false);
            }
        }

        private async void btn_SosyalAl_Click(object sender, EventArgs e)
        {
            var result = await _api.SosyalMedyaAlAsync(_engineType);
            if (result.Success)
            {
                _isSosyalOnAir = false;
                _isWhatsappOnAir = false;
                SetButtonOnAir(btn_SosyalVer, false);
                SetButtonOnAir(btn_WhatsappVer, false);
            }
        }

        // ─── İSİMLİK ─────────────────────────────────────────────

        private async void btn_TelefonIsimlikVer_Click(object sender, EventArgs e)
        {
            string isim = txt_TelefonIsim.Text.Trim();
            string title = txt_TelefonTitle.Text.Trim();
            if (string.IsNullOrWhiteSpace(isim) || string.IsNullOrWhiteSpace(title))
            { MessageBox.Show("İsim ve Title boş olamaz!"); return; }

            bool telefonMu = cbx_TelefonMu.Checked;
            var result = await _api.TelefonIsimlikVerAsync(_engineType, isim, title, telefonMu);
            if (result.Success)
            {
                _isTelefonIsimlikOnAir = true;
                SetButtonOnAir(btn_TelefonIsimlikVer, true);
                btn_TelefonIsimlikAl.Enabled = true;
            }
            else MessageBox.Show(result.Message);
        }

        private async void btn_TelefonIsimlikAl_Click(object sender, EventArgs e)
        {
            var result = await _api.TelefonIsimlikAlAsync(_engineType);
            if (result.Success)
            {
                _isTelefonIsimlikOnAir = false;
                SetButtonOnAir(btn_TelefonIsimlikVer, false);
                btn_TelefonIsimlikAl.Enabled = false;
            }
        }

        // ─── MUHABİR KAMERA ──────────────────────────────────────

        private async void btn_MuhabirKameraVer_Click(object sender, EventArgs e)
        {
            string muhabir = txt_Muhabir.Text.Trim();
            string kameraman = txt_Kameraman.Text.Trim();
            if (string.IsNullOrWhiteSpace(muhabir) && string.IsNullOrWhiteSpace(kameraman))
            { MessageBox.Show("En az biri dolu olmalı!"); return; }

            var result = await _api.MuhabirKameraVerAsync(_engineType, muhabir, kameraman);
            if (result.Success)
            {
                _isMuhabirKameraOnAir = true;
                SetButtonOnAir(btn_MuhabirKameraVer, true);
                btn_MuhabirKameraAl.Enabled = true;
            }
            else MessageBox.Show(result.Message);
        }

        private async void btn_MuhabirKameraAl_Click(object sender, EventArgs e)
        {
            var result = await _api.MuhabirKameraAlAsync(_engineType);
            if (result.Success)
            {
                _isMuhabirKameraOnAir = false;
                SetButtonOnAir(btn_MuhabirKameraVer, false);
                btn_MuhabirKameraAl.Enabled = false;
            }
        }

        // ─── CANLI ───────────────────────────────────────────────

        private async void btn_CanliVer_Click(object sender, EventArgs e)
        {
            var result = await _api.CanliVerAsync(_engineType);
            if (result.Success) { _isCanliOnAir = true; SetButtonOnAir(btn_CanliVer, true); }
        }

        private async void btn_CanliYerVer_Click(object sender, EventArgs e)
        {
            string text = txt_CanliYer.Text.Trim();
            if (string.IsNullOrWhiteSpace(text)) { MessageBox.Show("Yer metni boş olamaz!"); return; }

            var result = await _api.CanliYerVerAsync(_engineType, text);
            if (result.Success) { _isCanliYerOnAir = true; SetButtonOnAir(btn_CanliYerVer, true); }
        }

        private async void btn_CanliAl_Click(object sender, EventArgs e)
        {
            var result = await _api.CanliAlAsync(_engineType);
            if (result.Success)
            {
                _isCanliOnAir = false;
                _isCanliYerOnAir = false;
                SetButtonOnAir(btn_CanliVer, false);
                SetButtonOnAir(btn_CanliYerVer, false);
            }
        }

        // ─── ROZET ───────────────────────────────────────────────

        private int? GetSeciliRozet()
        {
            if (rdb_AzSonra.Checked) return 0;
            if (rdb_AzSonraDsf.Checked) return 1;
            if (rdb_AzSonraDsf2.Checked) return 2;
            if (rdb_SonDakika.Checked) return 3;
            if (rdb_OzelHaber.Checked) return 4;
            if (rdb_Whatsapp.Checked) return 5;
            return null;
        }

        private string? GetSeciliRozetAdi()
        {
            if (rdb_AzSonra.Checked) return "AzSonra";
            if (rdb_AzSonraDsf.Checked) return "AzSonraDsf";
            if (rdb_AzSonraDsf2.Checked) return "AzSonraDsf2";
            if (rdb_SonDakika.Checked) return "SonDakika";
            if (rdb_OzelHaber.Checked) return "OzelHaber";
            if (rdb_Whatsapp.Checked) return "WhatsappIhbar";
            return null;
        }

        private void ResetRozetButtons()
        {
            rdb_HaberKj.Checked = true;
        }

        // ─── RESET ───────────────────────────────────────────────

        private void ResetAllOnAirStates()
        {
            _isKjOnAir = false;
            _isYerOnAir = false;
            _isSosyalOnAir = false;
            _isWhatsappOnAir = false;
            _isIsimlikOnAir = false;
            _isTelefonIsimlikOnAir = false;
            _isMuhabirKameraOnAir = false;
            _isCanliOnAir = false;
            _isCanliYerOnAir = false;
            _aktifRozet = null;

            SetButtonOnAir(btn_TekliVer, false);
            SetButtonOnAir(btn_CiftliVer, false);
            SetButtonOnAir(btn_UzunVer, false);
            SetButtonOnAir(btn_YerVer, false);
            SetButtonOnAir(btn_SosyalVer, false);
            SetButtonOnAir(btn_WhatsappVer, false);
            SetButtonOnAir(btn_TelefonIsimlikVer, false);
            SetButtonOnAir(btn_MuhabirKameraVer, false);
            SetButtonOnAir(btn_CanliVer, false);
            SetButtonOnAir(btn_CanliYerVer, false);

            btn_KjAl.Enabled = false;
            btn_YerAl.Enabled = false;
            btn_TelefonIsimlikAl.Enabled = false;
            btn_MuhabirKameraAl.Enabled = false;

            ResetRozetButtons();
        }

        // ─── KLAVYE KISAYOLLARI ───────────────────────────────────

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.F3) { btn_SosyalVer_Click(btn_SosyalVer, EventArgs.Empty); return true; }
            if (keyData == Keys.F4) { btn_WhatsappVer_Click(btn_WhatsappVer, EventArgs.Empty); return true; }
            if (keyData == Keys.F5) { btn_SosyalAl_Click(btn_SosyalAl, EventArgs.Empty); return true; }
            if (keyData == Keys.F7) { btn_KjAl_Click(btn_KjAl, EventArgs.Empty); return true; }
            if (keyData == Keys.F8) { btn_TumunuAl_Click(btn_TumunuAl, EventArgs.Empty); return true; }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            _config.Save();
            _statusTimer?.Stop();
        }
    }
}*/