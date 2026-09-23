<!-- Canonical source: README.md. Keep this translation aligned with the English README. -->

# help-center-ui

[English](README.md) · [Türkçe](README.tr.md)

Bu klasör, HelpCenter'ın Next.js 16 App Router ve React 19 tabanlı istemcisidir. Tüm stack'in hızlı
başlangıcı, mimarisi ve teknoloji seçimi için kök [README](../README.tr.md); güvenlik kapsamı ve
bilinen sınırlar için [SECURITY.tr.md](../SECURITY.tr.md) temel referanstır.

## Yerel geliştirme

Önce repository kökündeki `.env.example` dosyasını `.env` olarak kopyalayın; `NEXT_PUBLIC_API_URL` ve
`NEXT_PUBLIC_APP_URL` değerleri dahil tüm yerel ayarları orada bir kez doldurun. Ardından `npm ci` ile
lockfile'a göre bağımlılıkları kurun ve `npm run dev` ile uygulamayı `http://localhost:3000` adresinde başlatın. Production'da
`NEXT_PUBLIC_API_URL`, `NEXT_PUBLIC_APP_URL` ve `AppSettings__FrontendUrl` production'da kesinlikle
`localhost` olmamalı; public HTTPS origin'ini göstermelidir. Production Compose bu değerleri kökteki
`.env`den doğrudan alır; Caddy API ve SignalR trafiğini yönlendirir.

```bash
# help-center-ui klasöründen
../scripts/with-env.sh npm ci
../scripts/with-env.sh npm run dev
```

`../scripts/with-env.sh`, repository kökündeki `.env` değerlerini yalnız başlattığı komut için
yükler. Next.js üst dizindeki `.env` dosyasını kendiliğinden okumadığı için ortak ayarları tekrar
`help-center-ui/.env.local` oluşturarak çoğaltmayın.

`src/proxy.ts`, `refresh_token` HttpOnly cookie'sini yalnız kaba bir rota geçidi olarak kullanır.
Access token modül belleğinde kalır; korumalı layout'lar tokenı API üzerinden doğrular. Frontend
JWT imzalama secret'ına ihtiyaç duymaz. CKEditor repo'nun GPL-3.0-only lisansı kapsamında çalışır;
başlatılamazsa boş bir editör yerine rotanın standart hata sayfası gösterilir. `/dashboard/sso`
varsayılan olarak kapalıdır; `NEXT_PUBLIC_DEMO_SSO_ENABLED=true` yalnız kontrollü bir ortamda
açılabilir. Bu mekanizma OIDC veya SAML değildir, sadece bellekte çalışan demo köprüsüdür.

## Frontend kuralları

- Public portal koyu görsel kimliğe, admin/dashboard ise açık operasyonel kabuğa sahiptir; kullanıcı
  tema seçici yoktur. Yeni renk sözlüğü yerine `globals.css` semantic tokenlarını genişletin.
- App Router giriş noktaları — `page.tsx`, `layout.tsx`, `loading.tsx`, `error.tsx` ve
  `not-found.tsx` — `src/app` altında kalır ve yalnız rota/layout sorumluluğu taşır.
- Rota-özel UI, ilgili route segmentinin `components/` dizininde birlikte konumlanır. Rota bağımsız
  ortak UI `src/components/`, client provider'lar `src/context/`, API taşıma modülleri ise
  `src/services/` altında yaşar.
- `src` altındaki kodları `@/` ile import edin; relative import'ları sadece küçük, tek bir rota veya
  feature klasörü içindeki yakın dosyalarla sınırlayın.
- Geçici arayüz durumu için local React state kullanın. Yeni client-side server state, feature
  feature kademeli olarak TanStack Query ile eklenir.
- Yeni dialog'lar `src/components/common/AppDialog.tsx` üzerinden kurulmalıdır; bu bileşen ortak
  Radix primitive'i olan `src/components/ui/dialog.tsx` ile compose edilir. Generic elementlerle
  modal semantiğini yeniden elle yazmayın.
- Dokunulan API sınırlarında `npm run api:types` ile üretilen type'ları tercih edin. Yeni veya ciddi
  biçimde yenilenen karmaşık formlar React Hook Form ve Zod kullanmalıdır; geçişler artımlıdır.

Bu kuralların gerekçesi ve kademeli geçiş sınırları
[ADR-008](../docs/adr/008-frontend-foundation.tr.md) içinde kayıtlıdır.

## Komutlar

| Komut | Yaptığı iş |
|---|---|
| `npm run dev` | Turbopack ile geliştirme sunucusunu başlatır |
| `npm run build` | Production build üretir |
| `npm run lint` | ESLint kurallarını çalıştırır |
| `npm run type-check` | Dosya üretmeden TypeScript denetimi yapar |
| `npm run test` | Vitest testlerini çalıştırır |
| `npm run e2e` | Playwright public-route smoke ve axe WCAG A/AA taramasını çalıştırır |
| `npm run storybook` | Bileşen kataloğunu `http://localhost:6006` üzerinde başlatır |
| `npm run storybook:build` | Statik Storybook build'i üretir |

## Browser ve bileşen güvencesi

Browser paketi ilk çalıştırmadan önce Chromium bir kez kurulmalıdır:

```bash
npx playwright install --with-deps chromium
npm run e2e
```

Public-home E2E paketi Next'i `3001` portunda başlatır ve her public API isteğini Playwright
fixture'larıyla karşılar. Bu nedenle Docker, çalışan backend, kimlik bilgisi veya seed veri
gerektirmez. Kimlik doğrulamalı akışları, açık test kimlik bilgileri olan ayrı bir pakette tutun.

Storybook, `src/components/**` altındaki route bağımsız primitive'leri — Button, FormField,
AppDialog ve API state varyantlarını — kataloglar. a11y eklentisi etkileşimli geri bildirim sunar;
bloklayıcı otomatik kontrol Playwright axe smoke testidir. `color-contrast`, mevcut public portal
palette borcunu kapsayan tek geçici baseline istisnasıdır; semantic-theme migration ile kaldırılmalı,
yeni rule exclusion'larıyla genişletilmemelidir.
