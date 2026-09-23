<!-- Canonical source: 008-frontend-foundation.md. This is a translation; the accepted English ADR is the historical source. -->

# ADR-008 — Frontend temeli: görsel yüzeyler, veri ve UI sınırları

[English](008-frontend-foundation.md) · [Türkçe](008-frontend-foundation.tr.md)

**Durum:** Kabul edildi
**Tarih:** 2026-09-12
**Karar verici:** Yusuf Yıldırım

## Bağlam

Frontend route-yerel bileşenler ve service modülleriyle büyüdü; App Router için bu iyi bir başlangıçtır.
Sonraki değişikliklerin yeni tema, modal, server-state deseni veya elle yazılmış API sözleşmesi
getirmemesi için ortak varsayımlara ihtiyacı vardır. Public support portalı yüksek kontrastlı koyu
kimlikten, yoğun operasyonel admin deneyimi açık çalışma yüzeyinden faydalanır; bu farkı rastgele
renk utility'leri gibi ele almak token migration'ını zorlaştırır.

## Karar

### Görsel kimlik

- Public portal kasten koyudur.
- Admin/dashboard şimdilik kasten açık kabuktur.
- Bu slice kullanıcı tema seçicisi eklemez; gelecekte token tabanlı çoklu tema gereksinimi bu ADR'yi geçersiz kılabilir.

### Uygulama sınırları

- App Router girişleri `src/app`ta kalır; `page.tsx`, `layout.tsx`, `loading.tsx`, `error.tsx`,
  `not-found.tsx` yalnız Next.js routing sorumluluğunu taşır.
- Rota özel bileşenleri segmentin `components/` dizininde, reusable route-agnostic UI `src/components/`ta,
  cross-cutting client provider'lar `src/context/`te, transport kodu `src/services/`te yaşar.
- `src` altı importlarda `@/`, relative importta yalnız aynı küçük feature/route klasörü kullanılır.

### Artımlı standartlar

- Client-side server state standardı TanStack Query'dir; repo çapı rewrite değil feature pilotlarıyla gelir.
  Ephemeral UI için local React state uygundur.
- Erişilebilir dialoglar mevcut Radix Dialog primitive'i ve ortak wrapper'ı kullanır; generic `div` ile
  modal semantiği tekrar yazılmaz.
- Generated OpenAPI type'ları dokunulan service sınırlarında kademeli olarak sözleşme kaynağıdır.
- Yeni veya ciddi yenilenen karmaşık form React Hook Form + Zod kullanır; küçük yerel formlar için
  spekülatif migration gerekmez.

## Sonuçlar

**Olumlu:** İki ürün yüzeyinin niyeti açıktır; yeni işin yeri/importu öngörülebilir olur; server
state, dialog, type ve form validation sınırlı regresyon riskiyle feature bazında iyileşir.

**Olumsuz:** Bir süre legacy ve standart desenler yan yana yaşar. Tema-token migration'ı iki yüzeydeki
hard-coded renk utility'lerini değiştirmelidir. Yeni bağımlılık/konvansiyonlar odaklı test ve belge ister.

**Nötr:** Bu ADR mevcut UI'ı yeniden yazmaz veya restyle etmez; takip eden slice'lar standardı uygular.

## İlgili

- [Frontend README](../../help-center-ui/README.tr.md#frontend-kuralları)
- [Next.js App Router proje yapısı](https://nextjs.org/docs/app/getting-started/project-structure)
