




using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;

namespace Bulmaca
{
    public partial class Form1 : Form
    {

        private BulmacaStratejisi mStrateji; // Bulmaca stratejisi nesnesi
        private Heuristic sezgiselYaklasim; // Sezgisel yaklaşım türü
        private LinearShuffle<int> karistir; // Karıştırma işlemi için nesne
        private WindowsFormsSynchronizationContext mSyncContext; // UI güncellemeleri için senkronizasyon nesnesi
        Dictionary<int, Button> mButtons; // Buttonların ID'lerini tutan bir sözlük
        private int[] baslangicDurumu; // Başlangıç durumu
        private bool mMesgul; // Bulmaca çözüm sürecinin devam edip etmediğini kontrol eden bir boolean

        public static int[] bitisDurumuGlobal; // Global bitiş durumu

        public Form1()
        {
            InitializeComponent();
            mSyncContext = SynchronizationContext.Current as WindowsFormsSynchronizationContext;

            Initialize();



        }

        public void txtyeDegerleriAta(int[] baslangicDurumu)
        {
            foreach (int i in baslangicDurumu)
            {
                if (i == -1)
                {
                    baslangicDurumu[Array.IndexOf(baslangicDurumu, i)] = 0;
                }
            }

            numF1.Value = baslangicDurumu[0];
            numF2.Value = baslangicDurumu[1];
            numF3.Value = baslangicDurumu[2];
            numF4.Value = baslangicDurumu[3];
            numF5.Value = baslangicDurumu[4];
            numF6.Value = baslangicDurumu[5];
            numF7.Value = baslangicDurumu[6];
            numF8.Value = baslangicDurumu[7];
            numF9.Value = baslangicDurumu[8];

        }

        public void txtdenDegerleriAta()
        {
            // TextBox'lardaki değerleri başlangıç durumuna ata

            baslangicDurumu = new int[] {
                (int)numF1.Value,
                (int)numF2.Value,
                (int)numF3.Value,
                (int)numF4.Value,
                (int)numF5.Value,
                (int)numF6.Value,
                (int)numF7.Value,
                (int)numF8.Value,
                (int)numF9.Value
            };

            foreach (int i in baslangicDurumu)
            {
                if (i == 0)
                {
                    baslangicDurumu[Array.IndexOf(baslangicDurumu, i)] = -1;
                }
            }
            durumGoster(baslangicDurumu, false);


            // Başlangıç durumunun çözülebilir olup olmadığını kontrol et

            if (!CozulebilirMi2(baslangicDurumu, bitisDurumuGlobal))
            {
                Console.WriteLine("Bu bulmaca çözülebilir değil");
                MessageBox.Show(this, "Bu bulmaca çözülebilir değil.");

            }

        }

        public void HedeftxtyeDegerleriAta(int[] bitisDurumuGlobal)
        {
            foreach (int i in bitisDurumuGlobal)
            {
                if (i == -1)
                {
                    bitisDurumuGlobal[Array.IndexOf(bitisDurumuGlobal, i)] = 0;
                }
            }

            numG1.Value = bitisDurumuGlobal[0];
            numG2.Value = bitisDurumuGlobal[1];
            numG3.Value = bitisDurumuGlobal[2];
            numG4.Value = bitisDurumuGlobal[3];
            numG5.Value = bitisDurumuGlobal[4];
            numG6.Value = bitisDurumuGlobal[5];
            numG7.Value = bitisDurumuGlobal[6];
            numG8.Value = bitisDurumuGlobal[7];
            numG9.Value = bitisDurumuGlobal[8];

        }

        public void HedeftxtdenDegerleriAta()
        {

            bitisDurumuGlobal = new int[] {
                (int)numG1.Value,
                (int)numG2.Value,
                (int)numG3.Value,
                (int)numG4.Value,
                (int)numG5.Value,
                (int)numG6.Value,
                (int)numG7.Value,
                (int)numG8.Value,
                (int)numG9.Value
            };



            foreach (int i in bitisDurumuGlobal)
            {
                if (i == 0)
                {
                    bitisDurumuGlobal[Array.IndexOf(bitisDurumuGlobal, i)] = -1;
                }
            }
            if (!CozulebilirMi2(baslangicDurumu, bitisDurumuGlobal)) MessageBox.Show(this, "çözülebilir değil");
            Console.WriteLine(string.Join(",", bitisDurumuGlobal));


        }

        private void Initialize()
        {



            baslangicDurumu = new int[] { 1, 2, 3, 4, 5, 6, -1, 7, 8 };
            bitisDurumuGlobal = new int[] { 1, 2, 3, 4, 5, 6, 7, 8, -1 };


            karistir = new LinearShuffle<int>();
            mStrateji = new BulmacaStratejisi();
            sezgiselYaklasim = Heuristic.ManhattanDistance;
            mStrateji.DurumDegistiginde += StratejiDurumDegistiginde;
            mStrateji.BulmacaCozuldu += BulmacaCozuldu;

            // Set display nodes

            mButtons = new Dictionary<int, Button>();
            mButtons[0] = button1;
            mButtons[1] = button2;
            mButtons[2] = button3;
            mButtons[3] = button4;
            mButtons[4] = button5;
            mButtons[5] = button6;
            mButtons[6] = button7;
            mButtons[7] = button8;
            mButtons[8] = button9;

            // Display state
            durumGoster(baslangicDurumu, false);

            txtyeDegerleriAta(baslangicDurumu);
            txtdenDegerleriAta();

            statusLabel.Text = "Rakamları karıştırmak için sürükleyip bırakabilirsiniz";
            progressBar.Style = ProgressBarStyle.Marquee;
            progressBar.Visible = false;
        }

        private void DegerleriDegistir(int x, int y)
        {
            int temp = baslangicDurumu[x];
            baslangicDurumu[x] = baslangicDurumu[y];
            baslangicDurumu[y] = temp;
        }

        private void StratejiDurumDegistiginde(int[] state, bool isFinal)
        {
            // "mSyncContext.Post()" ile UI thread'de güncelleme yapılır.
            // "durumGoster" fonksiyonunu, "state" ve "isFinal" parametreleriyle çağırır.
            mSyncContext.Post(item => durumGoster(state, isFinal), null);

            // "numSure.Value" değerini "sleep" adlı integer değişkene aktarır.
            // Bu değer, her adım arasındaki bekleme süresini belirler.
            int sleep = (int)numSure.Value;

            // Adımlar arasında belirtilen süre kadar bekler.
            Thread.Sleep(sleep);
        }

        private void BulmacaCozuldu(int steps, int time, int statesExamined)
        {
            // "action" adında bir Action nesnesi oluşturulur.
            // Bu nesne, bulmaca çözüldükten sonra kullanıcı arayüzünü güncellemek için kullanılır.
            Action action = () =>
            {
                // İşlem tamamlandığında, progress bar'ın görünürlüğü kapatılır.
                progressBar.Visible = false;

                // İşlem tamamlandığında, imlecin görünümü varsayılana döndürülür.
                this.Cursor = Cursors.Default;

                // Eğer adım sayısı -1'den büyükse, çözüm bulundu ve kullanıcı arayüzü güncellenir.
                if (steps > -1)
                {
                    statusLabel.Text = "Çözüm bulundu";
                    hamle.Text = steps.ToString("n0");
                    sure.Text = (time / 1000.0).ToString("n2").ToString();
                    islem.Text = statesExamined.ToString("n0");
                }
                // Eğer adım sayısı -1 ise, çözüm bulunamadı ve kullanıcı arayüzü güncellenir.
                else
                {
                    statusLabel.Text = "Çözüm bulunamadı";
                    hamle.Text = "-";
                    sure.Text = (time / 1000.0).ToString("n3").ToString();
                    islem.Text = statesExamined.ToString("n0");
                    MessageBox.Show(this, "Çözüm bulunamadı");
                }
            };

            // UI thread üzerinde action nesnesinin Invoke metodu çağrılır ve güncelleme yapılır.
            //arka planda çalışan bir iş parçacığında elde edilen sonuçları ana iş parçacığına (UI iş parçacığı) iletmek için kullanılır.
           mSyncContext.Send(item => action.Invoke(), null);
        }


        //Bu fonksiyon, verilen nodes dizisini kullanarak oyunun mevcut durumunu kullanıcı arayüzünde günceller.
        //Eğer isFinal parametresi true ise, işlem tamamlanmıştır ve Karıştır ve Başlat butonları etkinleştirilir.
        private void durumGoster(int[] nodes, bool isFinal)
        {
            // Eğer düğüm değeri null değilse, düğümleri göster.
            if (nodes != null)
            {
                // gamePanel'in düzenini duraklat, düğümleri güncelle ve düzenlemeyi sürdür.
                gamePanel.SuspendLayout();

                // Düğümleri döngüyle gez ve düğüm değeri 0'dan büyükse düğümün değerini yaz.
                for (int i = 0; i < nodes.Length; i++)
                {
                    mButtons[i].Text = nodes[i] > 0 ? nodes[i].ToString() : null;
                }

                // gamePanel düzenlemesini sürdür.
                gamePanel.ResumeLayout();
            }

            // Eğer bu son durumsa (çözüm bulunduysa), işlemi tamamla ve butonları etkinleştir.
            if (isFinal)
            {
                mMesgul = false;
                buttonKaristir.Enabled = true;
                buttonStart.Enabled = true;
            }
        }

        private void BulmacaCozmeyeBasla()
        {
            // mStrateji nesnesi üzerinde bulmacayı çözmeye başla.
            mStrateji.Coz(baslangicDurumu, sezgiselYaklasim);

            // İşlem başladığında, progress bar'ı görünür yap ve imlecin görünümünü "Bekleme" olarak değiştir.
            progressBar.Visible = true;
            this.Cursor = Cursors.WaitCursor;

            // İşlem başladığında, statusLabel metnini güncelle.
            statusLabel.Text = "Çözüm bulundu";

            // İşlem başladığında, mMesgul değişkenini true yaparak işlem devam ediyor durumunu belirt.
            mMesgul = true;
        }



        private bool izinVarmi()
        {
            return !mMesgul;
        }

        private void karistir_click(object sender, EventArgs e)
        {
            // Konsola bilgi mesajı yazdır.
            Console.WriteLine("karistirTikla() calisti");

            // İzin kontrolü yap, devam edebilir miyiz?
            if (izinVarmi())
            {
                // Konsola bilgi mesajı yazdır.
                Console.WriteLine("izinVarmi() calisti");

                // Çözülebilir bir durum elde edene kadar devam et.
                bool solvable = false;

                while (!solvable)
                {
                    // karistir nesnesini kullanarak başlangıç durumunu karıştır.
                    karistir.Shuffle(baslangicDurumu);

                    // Karıştırılan durumun çözülebilir olup olmadığını kontrol et.
                    solvable = CozulebilirMi2(baslangicDurumu, bitisDurumuGlobal);

                    // Konsola solvable değerini yazdır.
                    Console.WriteLine(solvable.ToString());
                    Console.WriteLine(baslangicDurumu[0].ToString());
                }

                // Başlangıç durumunu göster.
                durumGoster(baslangicDurumu, false);

                // Başlangıç durumunu metin kutularına ata.
                txtyeDegerleriAta(baslangicDurumu);

                // Metin kutularından değerleri al ve işle.
                txtdenDegerleriAta();
            }
        }


        //Bu fonksiyon, başlangıç ve hedef durumlarındaki terslik(inversion) sayılarını karşılaştırarak geçişin mümkün olup olmadığını belirler.
        //Eğer her iki durumdaki terslik sayıları aynı çiftlikteyse(her ikisi de çift veya tek), geçiş mümkündür.
        private bool CozulebilirMi2(int[] initialState, int[] targetState)
        {
            // Başlangıç ve hedef durumları arasındaki geçişin mümkün olup olmadığını kontrol eder.

            Console.WriteLine("");

            // Başlangıç durumundaki boş olmayan taşların konumlarını hesaplar.
            List<PlacedTile> NonEmptyInitialTiles = initialState
                .Select((number, index) => new PlacedTile { Number = number, Row = index / 3, Col = index % 3 })
                .Where(tile => tile.Number != -1)
                .ToList();

            // Hedef durumdaki boş olmayan taşların konumlarını hesaplar.
            List<PlacedTile> NonEmptyTargetTiles = targetState
                .Select((number, index) => new PlacedTile { Number = number, Row = index / 3, Col = index % 3 })
                .Where(tile => tile.Number != -1)
                .ToList();

            // Başlangıç durumundaki terslik (inversion) sayısını hesaplar.
            int InitialInversions = TerslikleriHesapla(NonEmptyInitialTiles);

            // Hedef durumdaki terslik (inversion) sayısını hesaplar.
            int TargetInversions = TerslikleriHesapla(NonEmptyTargetTiles);

            // Başlangıç ve hedef durumları arasındaki geçişin mümkün olup olmadığını kontrol eder.
            // Eğer başlangıç ve hedef durumlarındaki terslik sayıları aynı çiftlikteyse (her ikisi de çift veya tek), geçiş mümkündür.
            System.Diagnostics.Debug.WriteLineIf((InitialInversions % 2) != (TargetInversions % 2), "Not Solvable");

            // Geçişin mümkün olup olmadığını döndürür.
            return (InitialInversions % 2) == (TargetInversions % 2);
        }




        //Bu fonksiyon, taşların listesini alır ve terslik sayısını hesaplar.Terslik, bir listenin sıralanma düzeninin bozulma derecesidir.
        //Bu işlem, verilen listenin her bir elemanını diğer elemanlarla karşılaştırarak ve eğer j.elemanın numarası i.elemanın numarasından büyükse terslik 
        //    sayısını artırarak gerçekleştirilir.Sonunda, hesaplanan terslik sayısı döndürülür. Bu fonksiyon, özellikle başlangıç durumunun çözülebilir 
        //    olup olmadığını kontrol etmek için kullanılabilir.
        private int TerslikleriHesapla(List<PlacedTile> tiles)
        {
            // Terslik sayısını tutan değişkeni başlat.
            int Inversions = 0;

            // İlk döngü, listenin her elemanı üzerinde çalışır.
            for (int i = 0; i < tiles.Count; ++i)
            {
                // İkinci döngü, i+1 ile başlayarak listenin geri kalanını döner.
                for (int j = i + 1; j < tiles.Count; ++j)
                {
                    // j. elemanın numarası i. elemanın numarasından büyükse, terslik sayısını artır.
                    if (tiles[j].Number > tiles[i].Number)
                    {
                        ++Inversions;
                    }
                }
            }

            // Terslik sayısını döndür.
            return Inversions;
        }


        public class PlacedTile
        {
            public int Number { get; set; }
            public int Row { get; set; }
            public int Col { get; set; }
        }


        private void StartButton_Click(object sender, EventArgs e)
        {
            if (izinVarmi())
            {
                BulmacaCozmeyeBasla();
            }
        }

        private void gamePanel_Paint(object sender, PaintEventArgs e)
        {

        }

  

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            Console.WriteLine("radioButton_CheckedChanged >>> calisti");

            if (izinVarmi())
            {
                Console.WriteLine("radioButton1_CheckedChanged >>> izinVarmi() calisti");

                sezgiselYaklasim = Heuristic.ManhattanDistance;

            }
        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            Console.WriteLine("radioButton2_CheckedChanged() calisti");
            if (izinVarmi())
            {
                Console.WriteLine("radioButton2_CheckedChanged >>> izinVarmi() calisti");
                sezgiselYaklasim = Heuristic.MisplacedTiles;

            }
        }

        private void fSet_Click(object sender, EventArgs e)
        {
            txtdenDegerleriAta();

        }

        private void gSet_Click(object sender, EventArgs e)
        {
            HedeftxtdenDegerleriAta();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            buttonKaristir.PerformClick();
        }


    }
}
