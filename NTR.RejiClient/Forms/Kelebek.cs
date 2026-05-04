using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Globalization;
using NTR.RejiClient.Services;
using NTR.RejiClient.Models;

namespace NTR.RejiClient.Forms
{
    public partial class Kelebek : Form
    {
        private readonly ApiService _apiService;
        private readonly AppConfig _config;
        private readonly string _dataFolder;

        public Kelebek(ApiService apiService, AppConfig config)
        {
            InitializeComponent();
            _apiService = apiService;
            _config = config;

            _dataFolder = Path.Combine(Application.StartupPath, "KelebekGorselleri");
            KelebekListesiniDoldur();
            InitializeLocalEvents();
        }

        private void KelebekListesiniDoldur()
        {
            if (Directory.Exists(_dataFolder))
            {
                string[] dosyalar = Directory.GetFiles(_dataFolder, "*.png");
                lst_kelebek.Items.Clear();

                foreach (string dosya in dosyalar)
                {
                    lst_kelebek.Items.Add(Path.GetFileNameWithoutExtension(dosya));
                }
            }
        }

        private void InitializeLocalEvents()
        {
            // Temizle butonlarına hem TextBox'ları hem de Vizrt'i temizleme görevi veriyoruz
            btn_temizle1.Click += async (s, e) => { txtIsim1.Clear(); txtTitle1.Clear(); await VizrtKisiTemizle(1); };
            btn_temizle2.Click += async (s, e) => { txtIsim2.Clear(); txtTitle2.Clear(); await VizrtKisiTemizle(2); };
            btn_temizle3.Click += async (s, e) => { txtIsim3.Clear(); txtTitle3.Clear(); await VizrtKisiTemizle(3); };
            btn_temizle4.Click += async (s, e) => { txtIsim4.Clear(); txtTitle4.Clear(); await VizrtKisiTemizle(4); };
            btn_temizle5.Click += async (s, e) => { txtIsim5.Clear(); txtTitle5.Clear(); await VizrtKisiTemizle(5); };
        }

        // Vizrt tarafına boş veri göndererek o kişiyi ekrandan alan yardımcı fonksiyon
        private async Task VizrtKisiTemizle(int index)
        {
            await _apiService.KelebekIsimGonderAsync(_config.EngineType, index, "", "");
        }

        // --- BUTON 1: SADECE SAHNEYİ YÜKLER (BACK LAYER LOAD) ---
        private async void btnSahneGec_Click(object sender, EventArgs e)
        {
            if (lst_kelebek.SelectedItem == null) return;

            string secilenKelebek = lst_kelebek.SelectedItem.ToString();

            // Senin verdiğin dizin yapısına göre tam yolu oluşturuyoruz
            string tamSahneYolu = $"SHOW_TV_2025/REJI/YENI_SAYFA/KELEBEK/{secilenKelebek}";

            var result = await _apiService.KelebekSahneYukleAsync(_config.EngineType, tamSahneYolu);

            if (result.Success)
            {
                btnSahneGec.BackColor = Color.Yellow;
            }
        }

        // --- BUTON 2: İSİMLERİ GÖNDERİR VE ANİMASYONLARI TETİKLER ---
        private async void btnIsimlikleriVer_Click(object sender, EventArgs e)
        {
            try
            {
                // Sadece kutusu dolu olanları animasyonlu gönder, diğerlerini sadece set et
                await KisiVeAnimasyonGonder(1, txtIsim1.Text, txtTitle1.Text);
                await KisiVeAnimasyonGonder(2, txtIsim2.Text, txtTitle2.Text);
                await KisiVeAnimasyonGonder(3, txtIsim3.Text, txtTitle3.Text);
                await KisiVeAnimasyonGonder(4, txtIsim4.Text, txtTitle4.Text);
                await KisiVeAnimasyonGonder(5, txtIsim5.Text, txtTitle5.Text);

                btnIsimlikleriVer.BackColor = Color.Red;
            }
            catch (Exception ex)
            {
                MessageBox.Show("İsimlikler gönderilirken hata: " + ex.Message);
            }
        }

        private async Task KisiVeAnimasyonGonder(int index, string isim, string title)
        {
            if (string.IsNullOrWhiteSpace(isim)) return;

            // 1. Veriyi Set Et (API asenkron çağrılır)
            string formatliIsim = isim.ToUpper(new CultureInfo("tr-TR"));
            string formatliTitle = title.ToUpper(new CultureInfo("tr-TR"));

            await _apiService.KelebekIsimGonderAsync(_config.EngineType, index, formatliIsim, formatliTitle);

            // 2. Sadece o kişiye özel animasyonu (Director) oynat
            // Vizrt sahne yapındaki Director ismine göre: KISI1, KISI2...
            string command = $"DIRECTOR*KISI{index} PLAY";
            await _apiService.SendRawCommandAsync(_config.EngineType, command);
        }

        private async void btn_kelebek_al_Click(object sender, EventArgs e)
        {
            var result = await _apiService.KelebekKapatAsync(_config.EngineType);
            if (result.Success)
            {
                btnIsimlikleriVer.BackColor = Color.Empty;
                btnSahneGec.BackColor = Color.Empty;
            }
        }

        private async void btnTumunuTemizle_Click(object sender, EventArgs e)
        {
            // 1. Arayüzü Temizle
            txtIsim1.Text = txtTitle1.Text = "";
            txtIsim2.Text = txtTitle2.Text = "";
            txtIsim3.Text = txtTitle3.Text = "";
            txtIsim4.Text = txtTitle4.Text = "";
            txtIsim5.Text = txtTitle5.Text = "";
            pnl_kelebek_image.BackgroundImage = null;

            // 2. Vizrt Tarafını Temizle
            await VizrtKisiTemizle(1);
            await VizrtKisiTemizle(2);
            await VizrtKisiTemizle(3);
            await VizrtKisiTemizle(4);
            await VizrtKisiTemizle(5);
        }

        private void lst_kelebek_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lst_kelebek.SelectedItem == null) return;

            string secilenAd = lst_kelebek.SelectedItem.ToString()!;
            string gorselYolu = Path.Combine(_dataFolder, secilenAd + ".png");

            pnl_kelebek_image.BackgroundImage?.Dispose();
            pnl_kelebek_image.BackgroundImage = null;

            if (File.Exists(gorselYolu))
            {
                pnl_kelebek_image.BackgroundImage = Image.FromFile(gorselYolu);
                pnl_kelebek_image.BackgroundImageLayout = ImageLayout.Zoom;
            }
        }
    }
}