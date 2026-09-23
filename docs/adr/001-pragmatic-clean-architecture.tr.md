<!-- Canonical source: 001-pragmatic-clean-architecture.md. This is a translation; the accepted English ADR is the historical source. -->

# ADR-001 — Pragmatik Clean Architecture

[English](001-pragmatic-clean-architecture.md) · [Türkçe](001-pragmatic-clean-architecture.tr.md)

**Durum:** Kabul edildi
**Tarih:** 2026-08-09
**Karar verici:** Yusuf Yıldırım

## Bağlam

Clean Architecture, Application katmanının framework bağımsız kalmasını ister: ORM importu,
ASP.NET türü veya üçüncü taraf sızıntısı olmaz. Pratikte iki yaklaşım birlikte kullanılır:

- **Katı CA:** Application, `ISpecification<T>` kullanır ve `Task<Result<T>>` döner; EF Core yalnız
  Persistence'ta yaşar.
- **Pragmatik CA:** Application, EF Core LINQ uzantılarını (`Include`, `ToListAsync`,
  `AsNoTracking`) kullanabilir; repository'ler handler'ların sorguları gecikmeli kurabilmesi için
  `IQueryable<T>` döner.

İki yaklaşım da savunulabilir. Seçim, handler ergonomisini, test kurulumunu ve yeni katkı verenin
üretken olmak için öğrenmesi gereken desen sayısını etkiler.

## Karar

**Pragmatik Clean Architecture** benimsenecek:

- Application handler'ları `Microsoft.EntityFrameworkCore`a referans verebilir ve LINQ
  uzantılarını kullanabilir.
- Repository'ler `IGenericRepository<T>.Query()` üzerinden `IQueryable<T>` sunar.
- Domain saf kalır; dış referansı yoktur.
- WebApi controller'ları yalnız `IMediator` kullanabilir; doğrudan `DbContext` veya repository
  kullanamaz. Bu sınır `NetArchTest` ile zorunlu tutulur.

## Sonuçlar

**Olumlu**

- Handler'lar küçük kalır; her sorgu için `SpecificationEvaluator` boilerplate'i gerekmez.
- Gecikmeli yürütme ve `AsNoTracking`, tek satırlık basit işlemler olarak kalır.
- Yeni katkı verenler iki değil tek desen öğrenir: LINQ.
- Aggregate'e özgü repository metotları (`GetByIdWithPermissionsAsync`) çoklu `Include` grafiklerini
  yine kapsüller.

**Olumsuz**

- Application katmanı EF Core'a bağımlıdır. ORM değiştirmek, yalnız Persistence değişikliği değil,
  handler değişiklikleri de gerektirir.
- Application birim testleri, LINQ çevirisini çalıştırmak için InMemory provider veya Testcontainers
  gerektirir.

**Nötr**

- Application ile Persistence sınırı, hangi paketin import edildiğinden çok hangi türlerin
  `IQueryable` döndürdüğüyle tanımlıdır; mimari testlerle yapısal olarak korunur.

## Ne zaman yeniden değerlendirilmeli

- İkinci bir veri kaynağı (doküman deposu veya arama indeksi) sisteme girerse.
- Veritabanı olmadan çalışması gereken saf birim test paketi yayınlanırsa.
- Ekip tek birincil bakım sorumlusunun ötesinde büyürse.

Bunlardan biri gerçekleşirse handler'lar, planlanmış ama takvimlenmemiş specification desenine
tek tek taşınır.

## İlgili

- [Mimari genel bakış](../architecture.tr.md)
- [Teknoloji yığını](../tech-stack.tr.md)
