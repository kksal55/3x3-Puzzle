# 3x3-Puzzle
# 3x3 Puzzle Solver

## Proje Açıklaması

Bu proje, 3x3 boyutundaki karışık bir puzzle'ı çözmek için geliştirilmiştir. Puzzle çözümü için A* arama algoritması kullanılarak en uygun hareketler belirlenir ve ardışık hareketlerle çözüme ulaşılır.

## Özellikler

1. A* arama algoritması ile hızlı ve etkili çözüm bulma.
2. Heuristic fonksiyonları desteği sayesinde farklı çözüm stratejileri uygulanabilir.
3. Öncelikli kuyruk kullanarak en uygun hareketlerin belirlenmesi.
4. Rastgele karıştırma algoritması ile orijinal durumdan farklı başlangıç durumları yaratma.
5. C# dili kullanarak geliştirilmiş, .NET tabanlı bir uygulama.

## Başlarken

### Önkoşullar

Proje, C# dili kullanılarak geliştirildiği için .NET Framework veya .NET Core'un yüklü olduğu bir ortamda çalıştırılmalıdır.

### Kurulum

1. GitHub reposundan projeyi klonlayın veya indirin.

```sh
git clone https://github.com/your-username/3x3-puzzle-solver.git
```

2. Projeyi Visual Studio veya başka bir C# IDE ile açın.
3. Gerekli bağımlılıkları yükleyin ve projeyi derleyin.

### Kullanım

1. Projeyi çalıştırın.
2. Puzzle'ı karıştırmak için `Shuffle` işlemini uygulayın.
3. Puzzle'ı çözmek için çözüm algoritmasını başlatın.
4. Bulunan hareket sırasını takip ederek puzzle'ı çözün.

## Algoritma ve Sınıflar

Proje, A* arama algoritması kullanarak en uygun hareket sırasını belirler. Aşağıda projede kullanılan temel sınıflara ve işlevlerine değinilmektedir:

1. **MinOncelikKuyrugu**: Öncelikli kuyruk veri yapısı kullanarak, elemanların önceliklerine göre düzenlenmesini sağlar.
2. **LinearShuffle**: Diziyi rastgele karıştırarak yeni başlangıç durumları yaratmak için kullanılır.
3. **State**: Puzzle'ın herhangi bir durumunu temsil eden sınıf. Hareketlerin ve maaliyetlerin hesaplandığı yer burasıdır.

## Katkıda Bulunanlar

Projenin geliştirilmesine katkı sağlayan herkese teşekkürler!

## Lisans

Bu proje MIT lisansı ile lisanslanmıştır. Ayrıntılı bilgi için `LICENSE` dosyasını inceleyin.
