# .NET Worker Service Kontrollü Kapanış

.NET 8 `BackgroundService` örneği. Sahte bellek kuyruğu üzerinde kapanış sırasını gösterir: yeni işi kes, aktif işi sınırlı süre bekle, bitmeyen işi ack etmeden yeniden denenebilir bırak.

**Proje kökü:** `dotnet-worker-graceful-shutdown/` (`d:\SoftWare\dotnet-worker-graceful-shutdown`)

İlgili makale: [.NET Worker Service’te Kontrollü Kapanış: 7 Adımda Güvenli İş Kuyruğu Yönetimi](https://www.mehmet-ozdemir.com.tr/net-worker-servicete-kontrollu-kapanis-7-adimda-guvenli-is-kuyrugu-yonetimi/)

## Yazar

**Mehmet Özdemir** — [Web sitesi](https://www.mehmet-ozdemir.com.tr/)

## Bu Proje Neyi Gösteriyor?

| Konu | Özet |
|------|------|
| Yeni işi durdur | `StopAccepting` kuyruk yazıcısını kapatır |
| Aktif iş | `CancellationTokenSource.CreateLinkedTokenSource` + iş zaman aşımı |
| Ack | İş başarıyla bitince loglanır |
| Yeniden teslim | Süre dolunca veya kapanışta ack verilmez |
| Idempotency | Aynı `JobId` ikinci kez gelirse iş tekrarlanmaz |
| Gözlem | Kapanış süresi, tamamlanan, iptal ve yeniden teslim sayıları |

`HostOptions.ShutdownTimeout` varsayılanı 8 saniyedir. Demo, yaklaşık 2 saniye sonra uygulamayı durdurur; `job-2` bitmeden kesilir, `job-3` işlenmeden bırakılır.

## Çalıştırma

```text
cd d:\SoftWare\dotnet-worker-graceful-shutdown\src\GracefulShutdown.Worker
dotnet run
```

Ayarlar: `src/GracefulShutdown.Worker/appsettings.json` (`Shutdown`, `Demo`).

## Dosyalar

```text
src/GracefulShutdown.Worker/
  Program.cs
  QueueConsumerWorker.cs
  InMemoryWorkQueue.cs
  JobProcessor.cs
  appsettings.json
```

## İlgili Makaleler

- [.NET Worker Service’te Kontrollü Kapanış: 7 Adımda Güvenli İş Kuyruğu Yönetimi](https://www.mehmet-ozdemir.com.tr/net-worker-servicete-kontrollu-kapanis-7-adimda-guvenli-is-kuyrugu-yonetimi/)
- [ASP.NET Core API’lerde Idempotency: 7 Adımda Çift İşlemi Önleme](https://www.mehmet-ozdemir.com.tr/asp-net-core-apilerde-idempotency-7-adimda-cift-islemi-onleme/)
- [ERP Entegrasyonlarında Transactional Outbox: 7 Adımda Güvenilir Veri Senkronizasyonu](https://www.mehmet-ozdemir.com.tr/erp-entegrasyonlarinda-transactional-outbox-7-adimda-guvenilir-veri-senkronizasyonu/)

## Lisans

MIT — Mehmet Özdemir — https://www.mehmet-ozdemir.com.tr/
