<!-- Canonical source: 004-sql-server-and-repository-abstraction.md. This is a translation; the accepted English ADR is the historical source. -->

# ADR-004 — Birincil veri deposu olarak SQL Server 2022, veritabanı bağımsız repository yüzeyi

[English](004-sql-server-and-repository-abstraction.md) · [Türkçe](004-sql-server-and-repository-abstraction.tr.md)

**Durum:** Kabul edildi
**Tarih:** 2026-08-09
**Karar verici:** Yusuf Yıldırım

## Bağlam

Veritabanı seçimi, barındırma maliyetini, araçları ve — beklenmedik ölçüde — projenin portfolyoda
nasıl okunduğunu etkiler. Adaylar:

- **PostgreSQL:** Modern SaaS için fiilî varsayılan, ucuz managed hosting (Neon, Supabase), zengin
  özellikler (JSONB, array).
- **SQL Server 2022:** Birinci sınıf .NET araçları, Azure SQL entegrasyonu, enterprise ve Türkiye
  pazarı ağırlığı.
- **SQLite:** Testler için iyi, production anlatısı için değil.
- **NoSQL** (Mongo, Cosmos): Reddedildi; RBAC ve audit için güçlü tutarlılık ile referential
  integrity vazgeçilmezdir.

## Karar

Production'da provider olarak `Microsoft.EntityFrameworkCore.SqlServer` ile **SQL Server 2022**
kullanılacak. *Application kodu* provider bağımsız tutulacak:

- Repository'ler `IQueryable<T>` ve generic aggregate metotları döner.
- Provider'a özgü LINQ (`DATEPART`, TSQL raw fragment'leri) handler'lara sızmaz.
- SQL Server dialect'inin kaçınılmaz olduğu tek yer migration'lardır; version control altında
  tutulur ve gözden geçirilir.

Entegrasyon testleri, gerçek provider'ı **Testcontainers.MsSql** ile çalıştırır (bkz. ADR-006);
böylece dialect'e duyarlı regresyonlar production yerine commit anında yakalanır.

## Sonuçlar

**Olumlu**

- Production hosting için Azure SQL / Managed Instance'a sıfıra yakın sürtünmeyle geçilir.
- Mevcut araçlar (SSMS, Azure Data Studio, `sqlcmd`) doğrudan çalışır.
- Enterprise mülakatları “neden Postgres?” sorusunda takılı kalmaz.
- Testcontainers, kodun in-memory yerine gerçek MSSQL üzerinde çalıştığını kanıtlar.

**Olumsuz**

- Managed SQL Server hosting, managed Postgres'ten daha pahalıdır. Demo için Azure SQL Free tier
  (ayda 100k vCore-saniye) bunu azaltır.
- PostgreSQL'e özgü özellikler (JSONB, `NOTIFY/LISTEN`) gerekirse karar yeniden değerlendirilir veya
  çift provider desteği gerekir.

**Nötr**

- Handler'lar `IQueryable<T>` ve standart LINQ ile kaldığından provider değiştirmek tam yeniden
  yazım değil; connection string ve migration'ların yeniden üretilmesi işidir. Bu, portfolyoda da
  anlatılabilir bir sınırdır.

## Değerlendirilen alternatifler

- **PostgreSQL:** Greenfield SaaS için ekosistem uyumu daha güçlüdür. Hedef kitle remote veya
  uluslararası SaaS rollerine kayarsa yeniden değerlendirilir. Repository soyutlaması nedeniyle
  migration maliyeti Persistence ve migration'larla sınırlıdır.
- **Çift destek (SQL Server + Postgres):** Mevcut iş değeri olmadan migration ve test yüzeyini ikiye
  katladığı için reddedildi.
