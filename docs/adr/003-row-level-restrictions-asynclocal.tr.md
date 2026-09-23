<!-- Canonical source: 003-row-level-restrictions-asynclocal.md. This is a translation; the accepted English ADR is the historical source. -->

# ADR-003 — EF Core global filter ve AsyncLocal ambient ile satır-seviyesi kısıtlama

[English](003-row-level-restrictions-asynclocal.md) · [Türkçe](003-row-level-restrictions-asynclocal.tr.md)

> **Durum: Yerine yenisi geldi / kaldırıldı (2026-09-06).** Aşağıda anlatılan satır-seviyesi veri
> kısıtlama sistemi — `[DataRestriction]` attribute kaydı,
> `DataRestrictionQueryFilterBuilder`, `RestrictionAmbient` `AsyncLocal` fallback'i ve
> `DataRestrictions` feature slice'ı — kod tabanından bütünüyle kaldırıldı (Domain, Application,
> Persistence ve WebApi). Bu kayıt yalnız tasarımın ve çözdüğü hataların tarihsel açıklaması olarak
> tutulur. Yerine geçecek tasarım, backend .NET 8'den .NET 10'a ve veritabanı SQL Server'dan
> PostgreSQL'e taşındıktan sonra, bu özel builder/kayıt/ambient mekanizması yerine EF Core'un yerel
> “multiple query filter” desteğiyle (`OnModelCreating`) kurulacak. Bu yeniden tasarım gelene kadar
> uygulamada hiçbir yerde satır-seviyesi kısıtlama uygulanmaz; production trafiği olmadığı için
> bilinçli olarak kabul edilmiş bir boşluktur.

**Tarihsel durum:** Kabul edilmiş
**Tarih:** 2026-08-09
**Karar verici:** Yusuf Yıldırım

## Bağlam

Bazı kullanıcılar satırların yalnız bir alt kümesiyle sınırlıdır (örneğin “yalnız A ve B projeleri”).
Kısıtlamanın şu özellikleri taşıması isteniyordu:

- **Veri katmanında uygulanması:** Her handler'a `Where(...)` eklenmemeli.
- **Deklaratif olması:** Tablo kısıtlamayı kaydeder, entity buna katılır.
- **Unutulamaz olması:** Yeni endpoint filtreyi kendiliğinden devralır.

Doğal araç EF Core'un `HasQueryFilter` özelliğiydi. Ancak query filter'lar model cache'ine derlenir;
filtre ifadesinin yakaladığı her şey model oluşturma anında sabitlenir. İlk uygulama `DbContext`
örneğini yakalıyordu:

```csharp
var dbContextConst = Expression.Constant(this);
var check = Expression.Call(dbContextConst, isRestrictedMethod, keyConst);
```

Bunun sonucu, başlangıç migration'ı sırasında null restriction context ile oluşturulan *ilk*
`DbContext`in kalıcı olarak çağrılmasıydı; sessiz bir bypass oluşuyordu.

## Karar

Kısıtlama durumunu `AsyncLocal<T>` ile sahip olunan bir ambient'e taşı ve filter'ın her sorguda
yeniden değerlendirilmesi için statik metot çağrısıyla referans ver:

```csharp
public static class RestrictionAmbient
{
    private static readonly AsyncLocal<IUserRestrictionContext?> _current = new();
    public static IUserRestrictionContext? Current { get => _current.Value; internal set => _current.Value = value; }
    public static bool IsKeyRestricted(string key) => Current?.GetAllowedIds(key) != null;
    public static List<int> GetAllowedIds(string key) => Current?.GetAllowedIds(key) ?? new();
}

// EfContext.OnModelCreating — filter, statik metoda çağrı kurar
var check = Expression.Call(null, isRestrictedMethod, keyConst);
```

`EfContext` constructor'ı, gerçek bir context sağlandığında `RestrictionAmbient.Current` değerini
`restrictionContext`e ayarlar; nested `DbContext` oluşturmalarının ambient'i ezmemesi için yalnız bu
durumda ayar yapılır.

## Sonuçlar

**Olumlu**

- Filter her sorguda yeniden değerlendirilir; eski yakalanmış örnek kalmaz.
- Hiçbir handler scope'u unutmaz; yeni endpoint'ler davranışı devralır.
- `AsyncLocal`, `await` sınırları boyunca güvenlidir; ambient async pipeline'da yaşar.

**Olumsuz**

- Ambient durum, uzaktan etkili bir bağımlılık yaratır. Ambient'i değiştiren testler açık reset
  gerektirir.
- Tek bir mantıksal istek, ambient'i değiştiren worker thread'ler çatallarsa thread-safe değildir;
  `EfContext` oluşturulduktan sonra salt-okunur kabul edilerek bu risk azaltılır.

**Nötr**

- Testler gerektiğinde `IgnoreQueryFilters()` ile filtreyi aşabilir. Her kullanımın çağrı yerinde
  belgelenmesi zorunludur.

## İlgili

- [Mimari genel bakış](../architecture.tr.md)

## Ek (2026-09-01) — Yukarıdaki salt-statik çağrı tasarımı yeniden iyileştirildi

Bu ADR, `AsyncLocal` ambient'in neden var olduğunun tarihsel kaydıdır; ancak Karar bölümünde
anlatılan, filter ifadesinde hiçbir `DbContext` referansı taşımayan salt statik çağrı artık
çalışan tasarım değildir. Bu yaklaşımın başka bir hatası çıktı: EF Core, satır başına değişmeyen
argümansız/constant bir çağrıyı satırdan bağımsız constant kabul edip bir kez değerlendiriyor;
kısıtlamalar ilk yakalanmış örnek hatasına benzer şekilde sorgular arasında bayatlıyordu.

O zamanki uygulama, `DataRestrictionQueryFilterBuilder.BuildFilter(entityType, context)` içinde
`EfContext` örneğini `Expression.Constant(context)` ile filtreye gömüp instance metotlarını
(`context.IsKeyRestricted(key)` / `context.GetAllowedIds(key)`) çağıracak biçimde değiştirildi.
Bu, ilk hatayı geri getirmiyordu: EF Core, kendi context türündeki constant'ı çalışan context
örneğiyle her sorguda değiştirir. `RestrictionAmbient`, normal istekte birincil yol olan DI-scoped
`IUserRestrictionContext` yerine, DI-scoped context olmadan oluşturulan `EfContext`ler için
fallback olarak kalıyordu.

Bu tarihsel uygulama, kabulden sonra ADR gövdesini yeniden yazmama kuralı nedeniyle Karar bölümünü
değiştirmek yerine tarihli ek olarak kaydedildi. Sistem artık kaldırılmıştır.
