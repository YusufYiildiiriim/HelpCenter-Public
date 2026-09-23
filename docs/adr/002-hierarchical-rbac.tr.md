<!-- Canonical source: 002-hierarchical-rbac.md. This is a translation; the accepted English ADR is the historical source. -->

# ADR-002 — Hiyerarşik action-list RBAC

[English](002-hierarchical-rbac.md) · [Türkçe](002-hierarchical-rbac.tr.md)

**Durum:** Kabul edildi
**Tarih:** 2026-08-09
**Karar verici:** Yusuf Yıldırım
**Yerine geçtiği karar:** İlk boolean-kolonlu RBAC modeli

## Bağlam

İlk izin modeli, `RolePermission` tablosunda her eylem için bir boolean kolon tutuyordu:
`CanRead`, `CanCreate`, `CanUpdate`, `CanDelete`, `CanExport`, `CanPrint` ve benzerleri.

Yüzey büyüdükçe iki sorun ortaya çıktı:

1. **Bağlamsal bir eylem eklemek** — örneğin Projects için `ManageMembers`, herhangi bir kaynak
   için `LookupSelect` — şema değişikliğiyle birlikte entity, DTO, mapper, migration, handler ve
   frontend güncellemesi gerektiriyordu. Her yeni eylem, en az beş dosyaya dokunmaktı.
2. **Özellik paketleri** — “bu role Project Manager paketi ver” — zamanla birden çok yere dağılan
   ve birbirinden sapan kopyala-yapıştır koduna dönüşüyordu.

## Karar

Boolean kolonlar yerine **action-list modeli** kullanılacak:

- `RolePermission (RoleId, ResourceKey)`: her rol/kaynak çifti için tek satır.
- `RolePermissionAction (RolePermissionId, Action)`: verilmiş her eylem için string değerli tek
  satır.
- Kaynak metadatası (`AppResourceDefinitions.cs`), her kaynak için kullanılabilir eylemleri tanımlar.
- Özellik paketleri (`FeaturePackages.cs`), atomik uygulanan adlandırılmış
  `(ResourceKey, Action[])` demetleridir.

Şu anda kullanılan eylemler: `Read`, `Create`, `Update`, `Delete`, `ManageMembers`,
`ManageExperts`, `ManageModules`, `ManageProjects`, `LookupSelect`.

## Sonuçlar

**Olumlu**

- Yeni eylem, `AppResourceDefinitions`a **tek giriş** eklemektir; şema değişikliği gerekmez.
- Özellik paketleri, tek handler'ın uyguladığı tek sözlük olur; preset'ler tutarlı kalır.
- Frontend normalize edilmiş `permissions.modules[].actions[]` biçimini alır; kaynak başına dal
  yerine tek bir `hasPermission()` yardımcısı kullanır.
- Yetkilendirme policy'leri dinamik çözülür (`Perm:{ResourceKey}:{Action}`); policy kayıtları
  patlamaz.

**Olumsuz**

- Sorgu maliyeti, temel satırdaki kolonları okumaya kıyasla `RolePermissionAction` join'i nedeniyle
  biraz daha yüksektir. Permission sorgusundaki covering index ve eager loading bunu dengeler.
- Eski modelden geçiş; backfill ve drop adımlarını, eski şekli doğru geri kuran bir `Down` script'i
  ile birlikte iki aşamalı dağıtım gerektirdi.

**Nötr**

- Eylemler opak string'lerdir; controller tarafındaki `[HasPermission("X","Y")]` ifadesinin
  tanımlı eylemle eşleştiğine dair derleme zamanı kontrolü yoktur. İleride başlangıçta çalışan bir
  doğrulama görevi bu boşluğu kapatabilir.
