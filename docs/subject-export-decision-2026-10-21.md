# Konular dışa aktarma kararı

## Karar

Yönetim panelindeki **Konular** listesinde CSV ve PDF dışa aktarma açık kalır.

- Yetki: sırasıyla `Subjects.Export` ve `Subjects.Print`.
- Kapsam: kullanıcının etkin arama filtresine göre **tüm eşleşen kayıtlar** veya
  **aktif sayfa** seçimi.
- Alanlar: konu adı, açıklama, kullanım sayısı, kilit durumu ve aktiflik durumu.
- Hariç tutulanlar: iç kimlikler, kullanıcı bilgileri ve talep içeriği.

Bu ekran yalnızca konu sözlüğünü içerdiği için export, destek operasyonunu kolaylaştırır;
ek bir backend toplu-export endpoint'i gerektirmez. Mevcut istemci tarafı export mekanizması
filtreli veriyi alır ve Türkçe CSV uyumluluğu için UTF-8 BOM yazar.

## Kabul ölçütü

Arama yapıldıktan sonra hem CSV hem PDF'de yalnız eşleşen konu kayıtları bulunmalı; export
düğmeleri ilgili izin olmadığında görünmemelidir.
