using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Diagnostics;
using System.Windows.Forms;

namespace Bulmaca
{
    internal enum Heuristic
    {
        MisplacedTiles,
        ManhattanDistance
    }

    internal enum Yon
    {
        Sol,
        Sag,
        Yukari,
        Asagi,
    }

    internal sealed class State : IComparable
    {
        private int[] mDugumler; // Bulmaca durumunu temsil eden düğümleri içeren bir dizi
        private int mBosIndex; // Bulmacada boş kutunun indeksini saklar
        private string mDurumKodu; // Durumu tanımlamak için kullanılan benzersiz bir kod
        private int mMaaliyetf; // f(m) maaliyeti: Toplam maliyet = mMaaliyetg + mMaaliyeth
        private int mMaaliyeth; // h(m) maaliyeti: Heuristic maliyeti, hedef duruma olan tahmini uzaklık
        private int mMaaliyetg; // g(m) maaliyeti: Başlangıç durumundan bu duruma gelene kadar yapılan adımların maliyeti
        private Heuristic mHeuristic; // Durumu değerlendirmek için kullanılan sezgisel (heuristic) fonksiyon
        private State mEbeveyn; // Bu durumun ebeveyn durumu (bir önceki durum)

        // İlk yapıcı metot: Ebeveyn durumu, düğümler ve sezgisel fonksiyonu alarak yeni bir durum oluşturur
        internal State(State ebeveyn, int[] dugumler, Heuristic heuristic)
        {
            mDugumler = dugumler; // Düğümler dizisini saklar
            mEbeveyn = ebeveyn; // Ebeveyn durumunu saklar
            mHeuristic = heuristic; // Sezgisel fonksiyonu saklar
            MaaliyetHesapla(); // f(m), g(m) ve h(m) maaliyetlerini hesaplar
            mDurumKodu = DurumKoduOlustur(); // Durumu temsil eden benzersiz bir kod oluşturur
        }

        // İkinci yapıcı metot: İlk yapıcı metoda benzer ancak heuristic'i ebeveyn durumundan alır
        private State(State ebeveyn, int[] dugumler)
        {
            mDugumler = dugumler; // Düğümler dizisini saklar
            mEbeveyn = ebeveyn; // Ebeveyn durumunu saklar
            mHeuristic = ebeveyn.mHeuristic; // Sezgisel fonksiyonu ebeveyn durumundan alır
            MaaliyetHesapla(); // f(m), g(m) ve h(m) maaliyetlerini hesaplar
            mDurumKodu = DurumKoduOlustur(); // Durumu temsil eden benzersiz bir kod oluşturur
        }


        // State nesnelerinin eşitliğini kontrol etmek için kullanılan Equals metodu
        public override bool Equals(object obj)
        {
            State o = obj as State; // Gelen objeyi State tipine dönüştürür

            // Gelen obje null değilse ve durum kodları eşitse true döner, aksi takdirde false döner
            return o != null && this.mDurumKodu.Equals(o.mDurumKodu);
        }

        // State nesnelerinin karma (hash) kodunu döndüren GetHashCode metodu
        public override int GetHashCode()
        {
            return mDurumKodu.GetHashCode(); // Durum kodunun karma (hash) kodunu döndürür
        }

        // State nesnelerinin karşılaştırılmasını sağlayan CompareTo metodu
        public int CompareTo(object obj)
        {
            State o = obj as State; // Gelen objeyi State tipine dönüştürür

            // Gelen obje null değilse, bu nesnenin f(m) maliyetini diğer nesnenin f(m) maliyetiyle karşılaştırır
            if (o != null)
            {
                return (this.mMaaliyetf).CompareTo(o.mMaaliyetf);
            }

            return 0; // Gelen obje null ise, karşılaştırma yapılamaz, 0 döndürülür
        }

        // Bu durumun, verilen durumdan daha pahalı olup olmadığını kontrol eden metot
        public bool DahaPahaliMi(State oDurum)
        {
            // Bu durumun g(m) maliyeti, verilen durumun g(m) maliyetinden büyükse true döner, aksi takdirde false döner
            return this.mMaaliyetg > oDurum.mMaaliyetg;
        }

        // Durum kodunu döndüren GetDurumKodu metodu
        public String GetDurumKodu()
        {
            return mDurumKodu; // mDurumKodu alanını döndürür
        }


        private void MaaliyetHesapla()
        {
            // Eğer ebeveyn durumu yoksa, bu başlangıç durumudur ve maliyet yoktur
            if (mEbeveyn == null)
            {
                mMaaliyetg = 0;
            }
            else
            {
                // Durum geçiş maliyeti 1 birimdir, çünkü her adımda sadece bir karo hareket ettirilir
                mMaaliyetg = mEbeveyn.mMaaliyetg + 1;
            }

            // Heuristic maliyeti hesapla
            mMaaliyeth = HeuristicMaaliyetAl();

            // Toplam maliyeti hesapla (f(m) = g(m) + h(m))
            mMaaliyetf = mMaaliyeth + mMaaliyetg;
        }

        private int HeuristicMaaliyetAl()
        {
            // Seçilen sezgisel fonksiyona (heuristic) göre maliyeti hesaplar
            if (mHeuristic == Heuristic.ManhattanDistance)
            {
                return ManhattanUzakligiMaaliyetiAl(); 
            }
            else
            {
                return YanlisYerlestirilmisKaroMaaliyetiAl();
            }
        }

        private int YanlisYerlestirilmisKaroMaaliyetiAl()
        {
            int heuristicMaaliyet = 0;

            // Düğümler dizisi boyunca döngü başlat
            for (int i = 0; i < mDugumler.Length; i++)
            {
                int deger = mDugumler[i] - 1;
                // Boşluğun değeri -1'dir
                if (deger == -2)
                {
                    deger = mDugumler.Length - 1;
                    mBosIndex = i; // Boş kutunun indeksini saklar
                }

                // Eğer karo yanlış yerdeyse, heuristic maliyeti artır
                if (deger != i)
                {
                    heuristicMaaliyet++;
                }
            }

            return heuristicMaaliyet;
        }


        private int ManhattanUzakligiMaaliyetiAl()
        {
            int heuristicMaaliyet = 0; // Toplam Manhattan uzaklığı maliyetini saklar
            int gridX = (int)Math.Sqrt(mDugumler.Length); // Bulmacanın boyutunu (satır/sütun sayısı) hesaplar

            int idealX; // Hedef pozisyondaki x koordinatı
            int idealY; // Hedef pozisyondaki y koordinatı
            int mevcutX; // Mevcut pozisyondaki x koordinatı
            int mevcutY; // Mevcut pozisyondaki y koordinatı
            int deger; // Bulmacadaki mevcut değeri saklar

            //int[] hedefSiralama2 = { 1, 2, 3, 4, 5, -1, 7, 8, 6 }; // İkinci hedef durumunun düğümleri
            int[] hedefSiralama3 = Form1.bitisDurumuGlobal; // Üçüncü hedef durumunun düğümleri

            int[] hedefSiralama = HedefSiralamaOlustur(hedefSiralama3); // Hedef düğüm dizisini oluşturur

            // Mevcut durumdaki tüm düğümleri döngü ile kontrol eder
            for (int i = 0; i < mDugumler.Length; i++)
            {
                deger = mDugumler[i] - 1; // Düğümdeki değeri alır ve bir azaltır (0'dan başlaması için)

                // Eğer değer -1 ise (boş kutu), değeri düğüm sayısının bir eksiğine eşitleyip, boş indeksi i yapar
                if (deger == -2)
                {
                    deger = mDugumler.Length - 1;
                    mBosIndex = i;
                }

                // Hedef dizisinde mevcut değerin indeksini bulur
                int hedefIndeks = Array.IndexOf(hedefSiralama, deger);

                // Eğer hedef indeksi mevcut indekse eşit değilse, Manhattan uzaklığını hesaplar
                if (hedefIndeks != i)
                {
                    idealX = hedefIndeks % gridX; // Hedef x koordinatını hesaplar
                    idealY = hedefIndeks / gridX; // Hedef y koordinatını hesaplar

                    mevcutX = i % gridX; // Mevcut x koordinatını hesaplar
                    mevcutY = i / gridX; // Mevcut y koordinatını hesaplar

                    // Manhattan uzaklığını hesaplayarak toplam maliyete ekler        
                    heuristicMaaliyet += (Math.Abs(idealY - mevcutY) + Math.Abs(idealX - mevcutX));
                }
            }

            return heuristicMaaliyet; // Toplam Manhattan uzaklığı maliyetini döndürür
        }

        // HedefSiralamaOlustur metodu: Hedef durumdaki düğümlerin sıralamasını hesaplar
        private int[] HedefSiralamaOlustur(int[] hedefDurum)
        {
            // Hedef durumun uzunluğuna eşit boyutta bir dizi oluşturur
            int[] hedefSiralama = new int[hedefDurum.Length];

            // Hedef durumdaki her düğüm için:
            for (int i = 0; i < hedefDurum.Length; i++)
            {
                // Eğer düğüm -1 ise (boş kutu):
                if (hedefDurum[i] == -1)
                {
                    // hedefSiralama dizisindeki ilgili elemana hedef durumun uzunluğu - 1 değeri atanır
                    hedefSiralama[i] = hedefDurum.Length - 1;
                }
                else
                {
                    // Değilse, hedefSiralama dizisindeki ilgili elemana düğümün değerinden 1 çıkarılır ve atanır
                    hedefSiralama[i] = hedefDurum[i] - 1;
                }
            }

            // Hedef sıralama dizisini döndürür
            return hedefSiralama;
        }

        // DurumKoduOlustur metodu: Bu durumu temsil eden benzersiz bir kod oluşturur. Statelerin birbirinden ayırt edilmesine ve 
        //ziyaret edilen durumların daha önce ziyaret edilip edilmediğinin kontrol edilmesine yardımcı olur.
        private String DurumKoduOlustur()
        {
            // Bir StringBuilder nesnesi oluşturarak kodu oluşturmak için kullanılır
            StringBuilder kod = new StringBuilder();

            // mDugumler dizisindeki her düğüm için:
            for (int i = 0; i < mDugumler.Length; i++)
            {
                // Düğümün değerini ve "*" karakterini kod'a ekler
                kod.Append(mDugumler[i] + "*");
            }

            // "*" karakteri ile biten kodu temizler ve sonuç olarak durum kodunu döndürür
            return kod.ToString().Trim(new char[] { '*' });
        }

        // GetState metodu: Bu durumun düğümlerini içeren bir dizi döndürür
        public int[] GetState()
        {
            // mDugumler dizisi ile aynı uzunlukta yeni bir dizi oluşturur
            int[] durum = new int[mDugumler.Length];

            // mDugumler dizisindeki düğümleri durum dizisine kopyalar
            Array.Copy(mDugumler, durum, mDugumler.Length);

            // Durum dizisini döndürür
            return durum;
        }


        public bool SonDurumMu()
        {
            // Tüm karolar doğru konumdaysa, son durumdayız.
            return mMaaliyeth == 0;
        }

        // Ebeveyn durumunu döndüren metot
        public State EbeveynAl()
        {
            return mEbeveyn; // Bu durumun ebeveyn durumunu döndürür
        }

        // Bu durumdan hareket ederek elde edilebilecek sonraki durumların listesini döndüren metot
        public List<State> SonrakiDurumlariAl(ref List<State> sonrakiDurumlar)
        {
            sonrakiDurumlar.Clear(); // Sonraki durumlar listesini temizler
            State durum; // Geçici olarak bir sonraki durumu saklamak için kullanılacak değişken

            // Yön enum tipindeki tüm değerler için döngü
            foreach (Yon yon in Enum.GetValues(typeof(Yon)))
            {
                durum = SonrakiDurumuAl(yon); // Belirtilen yönde hareket edilerek elde edilen yeni durum

                if (durum != null) // Eğer yeni durum geçerliyse (null değilse)
                {
                    sonrakiDurumlar.Add(durum); // Sonraki durumları listesine yeni durumu ekler
                }
            }

            return sonrakiDurumlar; // Sonraki durumların listesini döndürür
        }

        // Belirtilen yönde hareket edilerek elde edilen yeni durumu döndüren metot
        private State SonrakiDurumuAl(Yon yon)
        {

            int pozisyon; // Yeni boş kutunun pozisyonunu saklamak için kullanılacak değişken

            // Eğer belirtilen yönde hareket edebilirsek
            if (HareketEdebilirMi(yon, out pozisyon))
            {
                int[] dugumler = new int[mDugumler.Length]; // Yeni durum düğümleri için yeni bir dizi oluşturur
                Array.Copy(mDugumler, dugumler, mDugumler.Length); // Mevcut durum düğümlerini yeni düğümler dizisine kopyalar

                // Yeni durum düğümlerini alın
                Takas(dugumler, mBosIndex, pozisyon); // Boş kutuyu belirtilen yöne hareket ettirir

                return new State(this, dugumler); // Yeni durumu oluşturur ve döndürür
            }

            return null; // Eğer belirtilen yönde hareket edilemezse, null döndürür
        }


        private void Takas(int[] dugumler, int i, int j)
        {
            // İlk olarak, 'dugumler' adlı dizi içindeki 'i' ve 'j' indislerindeki elemanları takas etmek istiyoruz.

            int t = dugumler[i]; // 'i' indisindeki elemanın değerini 't' adlı geçici bir değişkene aktarırız.
            dugumler[i] = dugumler[j]; // 'j' indisindeki elemanın değerini 'i' indisindeki elemana atarız.
            dugumler[j] = t; // Geçici değişken 't' içinde saklanan, başlangıçta 'i' indisindeki elemanın değerini, şimdi 'j' indisine atarız.

            // Bu şekilde, 'i' ve 'j' indislerindeki elemanlar yer değiştirmiş olur.
        }


        private bool HareketEdebilirMi(Yon yon, out int yeniPozisyon)
        {
            // Başlangıç değerlerini ayarla
            int yeniX = -1;
            int yeniY = -1;
            int gridX = (int)Math.Sqrt(mDugumler.Length);
            int mevcutX = mBosIndex % gridX;
            int mevcutY = mBosIndex / gridX;
            yeniPozisyon = -1;

            // Verilen yöne göre hareket etmeye çalış
            switch (yon)
            {
                case Yon.Yukari:
                    {
                        // Eğer üstte değilsek, yukarı hareket edebiliriz
                        if (mevcutY != 0)
                        {
                            yeniX = mevcutX;
                            yeniY = mevcutY - 1;
                        }
                    }
                    break;

                case Yon.Asagi:
                    {
                        // Eğer en altta değilsek, aşağı hareket edebiliriz
                        if (mevcutY < (gridX - 1))
                        {
                            yeniX = mevcutX;
                            yeniY = mevcutY + 1;
                        }
                    }
                    break;

                case Yon.Sol:
                    {
                        // Eğer en solda değilsek, sola hareket edebiliriz
                        if (mevcutX != 0)
                        {
                            yeniX = mevcutX - 1;
                            yeniY = mevcutY;
                        }
                    }
                    break;

                case Yon.Sag:
                    {
                        // Eğer en sağda değilsek, sağa hareket edebiliriz
                        if (mevcutX < (gridX - 1))
                        {
                            yeniX = mevcutX + 1;
                            yeniY = mevcutY;
                        }
                    }
                    break;
            }

            // Eğer hareket edebilirsek, yeni pozisyonu hesapla
            if (yeniX != -1 && yeniY != -1)
            {
                yeniPozisyon = yeniY * gridX + yeniX;
            }

            // Eğer yeni pozisyon hesaplandıysa, hareket edebiliriz
            return yeniPozisyon != -1;
        }
    }


        internal delegate void DurumDegistiginde(int[] currentState, bool isFinal);
    internal delegate void BulmacaCozuldu(int steps, int time, int stateExamined);


    internal sealed class BulmacaStratejisi
    {
        #region Alanlar

        private Stopwatch kronometre;
        internal event DurumDegistiginde DurumDegistiginde;
        internal event BulmacaCozuldu BulmacaCozuldu;

        #endregion Alanlar

        #region Metodlar

        internal BulmacaStratejisi()
        {
            kronometre = new Stopwatch();
        }

        internal void Coz(int[] dugumler, Heuristic sezgisel)
        {
            ThreadPool.QueueUserWorkItem(ogge => Baslat(dugumler, sezgisel));
        }


        //Bu Baslat fonksiyonu, A* algoritmasını kullanarak bulmacayı çözmeye çalışır ve sonuç olarak elde edilen durumu işler. 
        //Bu işlem, en düşük maliyetli yolu bulmak için açık durum kuyruğunu sürekli güncelleyerek ve kapalı durumları saklayarak gerçekleştirilir.
        private void Baslat(int[] dugumler, Heuristic sezgisel)
        {
            // Başlangıç durumları ve veri yapıları
            int acikDurumIndeksi;
            int durumSayisi = -1;
            State mevcutDurum = null;
            List<State> sonrakiDurumlar = new List<State>();
            HashSet<String> acikDurumlar = new HashSet<string>();
            MinOncelikKuyrugu<State> acikDurumKuyrugu = new MinOncelikKuyrugu<State>(dugumler.Length * 3);
            Dictionary<String, State> kapaliKuyruk = new Dictionary<string, State>(dugumler.Length * 3);

            // Başlangıç durumunu oluştur ve açık durum kuyruğuna ekle
            State durum = new State(null, dugumler, sezgisel);
            acikDurumKuyrugu.Ekle(durum);
            acikDurumlar.Add(durum.GetDurumKodu());

            // Zamanı ölçmek için başlangıç noktası
            OlcumlemeyeBasla();

            // Açık durum kuyruğu boş olana kadar devam et
            while (!acikDurumKuyrugu.BosMu())
            {
                // En düşük maliyetli durumu al ve işle
                mevcutDurum = acikDurumKuyrugu.Cikar();
                acikDurumlar.Remove(mevcutDurum.GetDurumKodu());

                // İlerlemenin raporlanması için durum sayısı ve mevcut durum
                if (durumSayisi % 10000 == 0)
                {
                    Console.WriteLine(durumSayisi);
                    Console.WriteLine(mevcutDurum);
                }

                durumSayisi++;

                // Hedef duruma ulaşıldı mı? (Bulmaca çözüldü mü?)
                if (mevcutDurum.SonDurumMu())
                {
                    OlcumlemeyiBitir(durumSayisi); // Zamanı ölçmek için bitiş noktası
                    break;
                }

                // Mevcut durumdan sonraki durumları elde et
                mevcutDurum.SonrakiDurumlariAl(ref sonrakiDurumlar);

                // Sonraki durumlar varsa, işle
                if (sonrakiDurumlar.Count > 0)
                {
                    State kapaliDurum;
                    State acikDurum;
                    State sonrakiDurum;

                    // Sonraki durumlar listesini işle
                    for (int i = 0; i < sonrakiDurumlar.Count; i++)
                    {
                        kapaliDurum = null;
                        acikDurum = null;
                        sonrakiDurum = sonrakiDurumlar[i];

                        // Açık kuyrukta zaten aynı durum var mı?
                        if (acikDurumlar.Contains(sonrakiDurum.GetDurumKodu()))
                        {
                            // Açık kuyrukta zaten aynı durum var.
                            acikDurum = acikDurumKuyrugu.Bul(sonrakiDurum, out acikDurumIndeksi);

                            // Daha iyi bir yol bulduk mu?
                            if (acikDurum.DahaPahaliMi(sonrakiDurum))
                            {
                                // Daha maliyetli olanı atın ve daha iyisini ekle
                                acikDurumKuyrugu.Kaldir(acikDurumIndeksi);
                                acikDurumKuyrugu.Ekle(sonrakiDurum);
                            }
                        }
                        else
                        {
                            // Durumun kapalı kuyrukta olup olmadığını kontrol et
                            String durumKodu = sonrakiDurum.GetDurumKodu();
                            if (kapaliKuyruk.TryGetValue(durumKodu, out kapaliDurum))
                            {
                                // Daha iyi bir yol bulduk mu?
                                if (kapaliDurum.DahaPahaliMi(sonrakiDurum))
                                {
                                    // Daha maliyetli olanı atın ve daha iyisini ekle
                                    kapaliKuyruk.Remove(durumKodu);
                                    kapaliKuyruk[durumKodu] = sonrakiDurum;
                                }
                            }
                        }

                        // Yeni bir durum ya da daha iyisini bulduk mu?
                        if (acikDurum == null && kapaliDurum == null)
                        {
                            acikDurumKuyrugu.Ekle(sonrakiDurum);
                            acikDurumlar.Add(sonrakiDurum.GetDurumKodu());
                        }
                    }

                    // İşlenen mevcut durumu kapalı kuyruğa ekle
                    kapaliKuyruk[mevcutDurum.GetDurumKodu()] = mevcutDurum;
                }
            }

            // Bulmaca çözülmediyse, mevcutDurumu null olarak ata
            if (mevcutDurum != null && !mevcutDurum.SonDurumMu())
            {
                mevcutDurum = null;
            }

            // Bulmacayı çöz ve son durumu işle
            BulmacayiCoz(mevcutDurum, durumSayisi);
            SonDurumda(mevcutDurum);
        }


        // Kronometreyi sıfırlar ve başlatır. Bu, performans ölçümüne başlamak için kullanılır
        private void OlcumlemeyeBasla()
        {
            kronometre.Reset(); // Kronometreyi sıfırlar
            kronometre.Start(); // Kronometreyi başlatır
        }

        // Kronometreyi durdurur ve ölçümü tamamlar. Bu, performans ölçümünü bitirmek için kullanılır
        private void OlcumlemeyiBitir(int durumSayisi)
        {
            kronometre.Stop(); // Kronometreyi durdurur
        }

        // Bulunan çözüm yolu için işlem yapar ve ilgili olayı tetikler
        //Çözüm yolunu geri döndürürken, her bir durum için bir olay (event) tetikler. Eğer çözüm yoksa, durum değişikliği olayını null ile tetikler.
        private void SonDurumda(State durum)
        {
            if (durum != null)
            {
                // Bu bulmaca için bir çözümümüz var
                // Ağaçtaki yolun köküne geri dön
                Stack<State> yol = new Stack<State>(); // Yolu saklamak için bir yığın (stack) oluşturur

                // Yolu geriye doğru takip ederek her bir durumu yığına ekler
                while (durum != null)
                {
                    yol.Push(durum); // Yığının üstüne durumu ekler
                    durum = durum.EbeveynAl(); // Bir önceki duruma (ebeveyn duruma) geçer
                }

                // Yolu tamamladıktan sonra yığındaki durumları çıkararak yolun ilerlemesini simüle eder
                while (yol.Count > 0)
                {
                    // Yolda birer birer ilerleyin
                    DurumDegistiginde(yol.Pop().GetState(), yol.Count == 0); // Yığından bir durumu çıkarır ve durum değişikliği olayını tetikler. yol.Count == 0 ise yolun son durumu olduğunu belirtir
                }
            }
            else
            {
                // Çözüm yok
                DurumDegistiginde(null, true); // Çözüm olmadığını belirtmek için durum değişikliği olayını null ile tetikler
            }
        }


        private void BulmacayiCoz(State durum, int durumlar)
        {
            int adimlar = -1; // Adım sayısını başlangıçta -1 olarak ayarlar (hedef duruma ulaşana kadar arttırılacaktır)

            // Durumun ebeveyni (bir önceki durum) boş olana kadar döngü devam eder
            while (durum != null)
            {
                durum = durum.EbeveynAl(); // Bir önceki durumu alır
                adimlar++; // Adım sayısını artırır

            }

            // BulmacaCozuldu adında bir olay (event) varsa ve abone olan bir dinleyici (listener) varsa
            if (BulmacaCozuldu != null)
            {
                // BulmacaCozuldu olayını tetikler ve adım sayısını, geçen süreyi (milisaniye olarak) ve durum sayısını gönderir
                BulmacaCozuldu(adimlar, (int)kronometre.ElapsedMilliseconds, durumlar);
            }
        }


        #endregion Metodlar
    }


    // MinOncelikKuyrugu sınıfı, öncelik sırasına göre düşükten yükseğe öğeleri düzenlemeye yarar
    //Bu kod parçacığı, `MinOncelikKuyrugu` adlı bir sınıfın içinde bulunur ve IComparable bir arayüze sahip olan tüm nesneler için minimum öncelikli kuyruk veri yapısını sağlar. 
    //Bu veri yapısı, öncelik sırasına göre düşükten yükseğe öğeleri düzenlemeye yarar.


    internal sealed class MinOncelikKuyrugu<T> where T : IComparable
    {
        // dizi, öğeleri saklamak için kullanılır; elemanSayisi, kuyruktaki öğe sayısını temsil eder
        private T[] dizi;
        private int elemanSayisi;

        // MinOncelikKuyrugu yapıcısı, belirtilen kapasiteye göre bir dizi oluşturur
        internal MinOncelikKuyrugu(int kapasite)
        {
            dizi = new T[kapasite + 1];
            elemanSayisi = 0;
        }

        // Genislet metodu, dizi kapasitesini belirtilen kapasiteye göre genişletir
        private void Genislet(int kapasite)
        {
            T[] gecici = new T[kapasite + 1];
            int i = 0;
            while (++i <= elemanSayisi)
            {
                gecici[i] = dizi[i];
                dizi[i] = default(T);
            }

            dizi = gecici;
        }

        // DahaKucuk metodu, i. ve j. indeksteki öğelerin karşılaştırmasını yapar
        private bool DahaKucuk(int i, int j)
        {
            return dizi[i].CompareTo(dizi[j]) < 0;
        }

        // Degistir metodu, i. ve j. indeksteki öğelerin yerini değiştirir
        private void Degistir(int i, int j)
        {
            T gecici = dizi[j];
            dizi[j] = dizi[i];
            dizi[i] = gecici;
        }

        // Batir metodu, belirtilen indeksteki öğeyi aşağı doğru düzgün bir şekilde yerleştirir
        private void Batir(int index)
        {
            int k;
            while (index * 2 <= elemanSayisi)
            {
                k = index * 2;

                if (k + 1 <= elemanSayisi && DahaKucuk(k + 1, k))
                {
                    k = k + 1;
                }

                if (!DahaKucuk(k, index))
                {
                    break;
                }

                Degistir(index, k);
                index = k;
            }
        }

        // Yukselt metodu, belirtilen indeksteki öğeyi yukarı doğru düzgün bir şekilde yerleştirir
        private void Yukselt(int index)
        {
            int k;

            while (index / 2 > 0)
            {
                k = index / 2;

                if (!DahaKucuk(index, k))
                {
                    break;
                }
                Degistir(index, k);
                index = k;
            }
        }

        // BosMu metodu, kuyruğun boş olup olmadığını kontrol eder
        internal bool BosMu()
        {
            return elemanSayisi == 0;
        }

        // Ekle metodu, yeni bir öğeyi öncelik kuyruğuna ekler ve uygun konumda yerleştirir
        internal void Ekle(T eleman)
        {
            if (elemanSayisi == dizi.Length - 1)
            {
                Genislet(dizi.Length * 3);
            }

            dizi[++elemanSayisi] = eleman;
            Yukselt(elemanSayisi);
        }

        // Cikar metodu, en düşük öncelikli öğeyi kuyruktan çıkarır ve döndürür
        internal T Cikar()
        {
            if (!BosMu())
            {
                T eleman = dizi[1];
                dizi[1] = dizi[elemanSayisi];
                dizi[elemanSayisi--] = default(T);

                Batir(1);

                return eleman;
            }

            return default(T);
        }

        // Bul metodu, belirtilen öğeyi kuyrukta arar ve bulunan öğeyi ve indeksini döndürür
        internal T Bul(T eleman, out int index)
        {
            index = -1;
            if (!BosMu())
            {
                int i = 0;

                while (++i <= elemanSayisi)
                {
                    if (dizi[i].Equals(eleman))
                    {
                        index = i;
                        return dizi[i];
                    }
                }
            }

            return default(T);
        }

        // Kaldir metodu, belirtilen indeksteki öğeyi kuyruktan kaldırır ve düzenlemeler yapar
        internal void Kaldir(int index)
        {
            if (index > 0 && index <= elemanSayisi)
            {
                dizi[index] = dizi[elemanSayisi];
                dizi[elemanSayisi--] = default(T);
                Batir(index);
            }
        }
    }



    internal sealed class LinearShuffle<T> // "T" jenerik bir tip parametresi. Bu sayede bu sınıf farklı türlerdeki dizilerle çalışabilir.
    {
        #region Fields
        private Random mRandom; // Rastgele sayılar üretmek için kullanılan Random nesnesi
        #endregion Fields

        #region Methods
        // Yapıcı metot: Sınıfın bir nesnesi oluşturulduğunda çağrılır.
        internal LinearShuffle()
        {
            int seed = 42 + 42 * ((int)DateTime.Now.TimeOfDay.TotalSeconds % 42); // Rastgele sayı üretimi için başlangıç değeri (seed) oluşturuluyor.
            mRandom = new Random(seed); // Oluşturulan seed değeriyle yeni bir Random nesnesi oluşturuluyor.
        }

        // Shuffle metodu: Verilen diziyi rastgele karıştırmak için kullanılır.
        internal void Shuffle(T[] array)
        {
            int position;
            for (int i = 0; i < array.Length; i++) // Dizinin her elemanı için
            {
                position = NextRandom(0, i); // Rastgele bir pozisyon belirlenir.
                Swap(array, i, position); // Belirlenen pozisyondaki eleman ile şu anki eleman yer değiştirir.
            }
        }

        // NextRandom metodu: Belirtilen aralıkta rastgele bir tam sayı üretir.
        private int NextRandom(int min, int max)
        {
            return mRandom.Next(min, max); // Üretilen rastgele sayıyı döndürür.
        }

        // Swap metodu: İki elemanın dizideki yerini değiştirir.
        private void Swap(T[] a, int i, int j)
        {
            T temp = a[i]; // İlk elemanın değeri geçici bir değişkene kaydedilir.
            a[i] = a[j]; // İkinci elemanın değeri ilk elemana atanır.
            a[j] = temp; // Geçici değişkendeki ilk elemanın değeri ikinci elemana atanır.
        }
        #endregion Methods
    }

}
