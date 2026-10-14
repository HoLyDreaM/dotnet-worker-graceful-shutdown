# .NET Worker Service kontrollü kapanış

.NET 8 `BackgroundService` örneği. Sahte bellek kuyruğu üzerinde kapanış sırasını gösterir: yeni işi kes, aktif işi sınırlı süre bekle, bitmeyen işi ack etmeden yeniden denenebilir bırak.

**Proje kökü:** `Blog-Makale/dotnet-worker-graceful-shutdown/` (`d:\SoftWare\Blog-Makale\dotnet-worker-graceful-shutdown`)

## Ne gösterir?

| Adım | Davranış |
|---|---|
| Yeni işi durdur | `StopAccepting` kuyruk yazıcısını kapatır |
| Aktif iş | `CancellationTokenSource.CreateLinkedTokenSource` + iş zaman aşımı |
| Ack | İş başarıyla bitince loglanır |
| Yeniden teslim | Süre dolunca veya kapanışta ack verilmez |
| Idempotency | Aynı `JobId` ikinci kez gelirse iş tekrarlanmaz |
| Gözlem | Kapanış süresi, tamamlanan, iptal, yeniden teslim sayıları |

`HostOptions.ShutdownTimeout` varsayılanı 8 saniyedir. Demo, yaklaşık 2 saniye sonra uygulamayı durdurur; `job-2` bitmeden kesilir, `job-3` işlenmeden kuyrukta kalır.

## Çalıştırma

```text
cd d:\SoftWare\Blog-Makale\dotnet-worker-graceful-shutdown\src\GracefulShutdown.Worker
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
