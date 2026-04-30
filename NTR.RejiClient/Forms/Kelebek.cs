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

        private Dictionary<string, string> _isimTitleListesi = new Dictionary<string, string>();

        public Kelebek(ApiService apiService, AppConfig config)
        {
            InitializeComponent();
            _apiService = apiService;
            _config = config;

            _dataFolder = Path.Combine(Application.StartupPath, "KelebekGorselleri");
            KelebekListesiniDoldur();
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

        private async Task KisiGonderAsync(int index, string isim, string title)
        {
            string formatliIsim = isim?.ToUpper(new CultureInfo("tr-TR")) ?? "";
            string formatliTitle = title?.ToUpper(new CultureInfo("tr-TR")) ?? "";

            await _apiService.KelebekIsimGonderAsync(_config.EngineType, index, formatliIsim, formatliTitle);
        }

        // İstediğin Sıralama: Önce Sahneyi Ver, Sonra İsimleri Güncelle
        private async Task KelebekYayinaVerAsync()
        {
            if (lst_kelebek.SelectedItem == null)
            {
                MessageBox.Show("Lütfen bir kelebek türü seçin!");
                return;
            }

            string sahneYolu = _config.ScenePath;

            // 1. Sahneyi Arka Katmana Yükle
            var result = await _apiService.KelebekSahneYukleAsync(_config.EngineType, sahneYolu);

            if (result.Success)
            {
                // 2. Sadece kutusu dolu olan kişileri gönder ve animasyonlarını başlat
                // txtIsim1 doluysa 1. kişiyi gönder ve 1. animasyonu oynat
                await KisiVeAnimasyonGonder(1, txtIsim1.Text, txtTitle1.Text);
                await KisiVeAnimasyonGonder(2, txtIsim2.Text, txtTitle2.Text);
                await KisiGonderAsync(3, txtIsim3.Text, txtTitle3.Text); // 3-4-5 için de aynısı...
                await KisiGonderAsync(4, txtIsim4.Text, txtTitle4.Text);
                await KisiGonderAsync(5, txtIsim5.Text, txtTitle5.Text);

                // Not: 3-4-5 için de animasyon isimlerin 'DIRECTOR*KISI3' şeklindeyse 
                // onları da KisiVeAnimasyonGonder ile çağırabilirsin.

                btnIsimlikleriVer.BackColor = Color.Red;
            }
        }
        private async Task KisiVeAnimasyonGonder(int index, string isim, string title)
{
    if (string.IsNullOrWhiteSpace(isim)) return;

    // Önce veriyi set et
    await KisiGonderAsync(index, isim, title);

    // Sadece bu kişiye ait animasyonu (IN) başlat
    // Vizrt sahne yapına göre "KISI1", "ISIM1" veya "1" director ismini kontrol et
    string command = $"DIRECTOR*KISI{index} PLAY"; 
    await _apiService.SendRawCommandAsync(_config.EngineType, command);
}

        private async Task IsimleriGuncelleAsync()
        {
            await KisiGonderAsync(1, txtIsim1.Text, txtTitle1.Text);
            await KisiGonderAsync(2, txtIsim2.Text, txtTitle2.Text);
            await KisiGonderAsync(3, txtIsim3.Text, txtTitle3.Text);
            await KisiGonderAsync(4, txtIsim4.Text, txtTitle4.Text);
            await KisiGonderAsync(5, txtIsim5.Text, txtTitle5.Text);
        }

        private async void btnIsimlikleriVer_Click(object sender, EventArgs e)
        {
            await KelebekYayinaVerAsync();
        }

        private async void btn_kelebek_al_Click(object sender, EventArgs e)
        {
            var result = await _apiService.KelebekKapatAsync(_config.EngineType);
            if (result.Success)
            {
                btnIsimlikleriVer.BackColor = Color.LimeGreen;
            }
        }

        private void btnTumunuTemizle_Click(object sender, EventArgs e)
        {
            txtIsim1.Text = txtTitle1.Text = "";
            txtIsim2.Text = txtTitle2.Text = "";
            txtIsim3.Text = txtTitle3.Text = "";
            txtIsim4.Text = txtTitle4.Text = "";
            txtIsim5.Text = txtTitle5.Text = "";

            pnl_kelebek_image.BackgroundImage = null;
        }

        private void lst_kelebek_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lst_kelebek.SelectedItem == null) return;

            string secilenAd = lst_kelebek.SelectedItem.ToString();
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