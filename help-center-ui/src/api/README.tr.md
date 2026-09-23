<!-- Canonical source: README.md. Keep this translation aligned with the English README. -->

# API sözleşmeleri

[English](README.md) · [Türkçe](README.tr.md)

`openapi/helpcenter.v1.json`, frontend tarafından kullanılan endpointlerin dar kapsamlı ve repoda
tutulan OpenAPI snapshot'ıdır. Production ortamında Swagger kapalı olduğu için type üretimi çalışan
bir production veya CI API'sine bağımlı değildir; snapshot sözleşmenin bu istemci tarafından kabul
edilen yüzeyini sabitler.

`npm run api:types`, `schema.d.ts` dosyasını yeniden üretir; `npm run api:types:check` artefact'ın
snapshot ile senkron olduğunu doğrular. Yeni bir servis generated type kullanmaya başladığında
snapshot'a yalnız o endpointin request/response şemasını ekleyin; dosyanın API'nin tamamını
kapsadığını varsaymayın. Snapshot frontend sözleşmesi içindir; backend doğrulaması her zaman
otoriterdir ve istemci type'ı sunucu tarafı validation'ın yerine geçmez.
