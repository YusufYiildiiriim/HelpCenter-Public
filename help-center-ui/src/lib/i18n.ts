export const LOCALES = ["tr-TR", "en-US"] as const;
export type Locale = (typeof LOCALES)[number];
export type LocalePath = "tr" | "en";

export const DEFAULT_LOCALE: Locale = "tr-TR";

export const LOCALE_CONFIG: Record<Locale, { path: LocalePath; label: string; nativeName: string }> = {
  "tr-TR": { path: "tr", label: "Turkish", nativeName: "Türkçe" },
  "en-US": { path: "en", label: "English", nativeName: "English" },
};

export const messages = {
  "tr-TR": {
    dateLabel: "Tarih", messageSingular: "Mesaj", messagePlural: "Mesaj", fileSingular: "Dosya", filePlural: "Dosya", viewDetails: "Detayları Görüntüle",
    closeRequest: "Talebi Kapat", requestCompleted: "Talep Tamamlandı", requestConversation: "Talep Yazışmaları",
    sendMessagePlaceholder: "Mesajınızı buraya yazın...", sendMessageWithAttachmentsPlaceholder: "Mesajınızı buraya yazın veya gönderin...", sendMessage: "Mesaj gönder", addAttachment: "Ek dosya ekle", removeSelectedFile: "Seçili dosyayı kaldır", selectAttachmentType: "Dosya türü seçin", pdfFile: "PDF dosyası", excelFile: "Excel dosyası",
    requestResolvedNoNewMessage: "Bu talep sonuçlandırıldığı için yeni mesaj gönderilemez.", rating: "Değerlendirme", rateServiceTitle: "Talebi Sonuçlandır", serviceRatingPrompt: "Size verdiğimiz hizmeti nasıl değerlendirirsiniz?", ratingVeryBad: "Çok Kötü", ratingBad: "Kötü", ratingAverage: "Orta", ratingGood: "İyi", ratingExcellent: "Mükemmel", feedbackOptional: "Geri bildiriminiz (Opsiyonel)", feedbackPlaceholder: "Eklemek istediğiniz bir not var mı?", cancelAction: "Vazgeç", saveAndClose: "Kaydet ve Kapat", loadingMessages: "Mesajlar yükleniyor...", sessionInfoMissing: "Oturum bilgisi bulunamadı.", requestClosedSuccess: "Talebiniz başarıyla kapatıldı. Geri bildiriminiz için teşekkürler!",
    speakingWithSupport: "Müşteri Hizmetleri ile Görüşüyorsunuz", requestStatus: "Talep durumu",
    priorityCritical: "Kritik", priorityHigh: "Yüksek", priorityNormal: "Normal", priorityLow: "Düşük",
    statusWaitingForReply: "Cevap Bekliyor", statusReplied: "Cevaplandı", statusCompleted: "Tamamlandı", statusUnderReview: "Teknik İncelemede",
    corporateSupportPanel: "Kurumsal Destek Paneli", customerAdministrator: "Firma Yöneticisi", signOut: "Oturumu kapat", sessionTime: "Oturum Süresi",
    sessionExpiryNotice: "Güvenlik gereği 3 dakika hareketsiz kalırsanız oturumunuz kapatılacaktır.", quickAccess: "Hızlı Erişim",
    myRequests: "Taleplerim", requestHistory: "İşlem Geçmişi", customerProfile: "Profilim", activeSupportRequests: "Aktif Destek Talepleri",
    completedRequests: "Tamamlanan Talepler", activeSupportDescription: "Şu an sistemde işlem gören destek kayıtlarınız.",
    completedSupportDescription: "Geçmişte sonuçlandırılmış destek kayıtlarınız.", createNewRequest: "Yeni Talep Oluştur",
    allRequests: "Hepsi", noCompletedRequests: "Tamamlanan Talep Bulunmuyor", noActiveRequests: "Aktif Talebiniz Bulunmuyor",
    emptyRequestList: "Bu bölümde henüz listelenecek bir kayıt bulunmamaktadır.", guidesDescription: "Ürün ve hizmetlerimize ait kullanım rehberleri.",
    guideSearch: "Rehberlerde ara...", noGuideResults: "Rehber bulunamadı.", watchVideoGuide: "Video Rehberi İzle", attachmentsList: "Ekler",
    previousStep: "Önceki Adım", nextStep: "Sonraki Adım", profileSettings: "Profil Ayarları",
    profileDescription: "Hesap bilgilerinizi buradan görüntüleyebilir ve güncelleyebilirsiniz.", firstName: "Ad", lastName: "Soyad",
    immutableEmail: "E-Posta (Değiştirilemez)", saveChanges: "Değişiklikleri Kaydet",
    newRequestTitle: "Yeni Destek Talebi", technicalSupport: "Teknik Destek Hattı", closeNewRequest: "Yeni destek talebi penceresini kapat",
    requiredFieldsNotice: "Lütfen zorunlu alanları doldurun.", requestTitle: "Talep Başlığı", requestSubject: "Talep Konusu",
    relatedModule: "İlgili Modül", selectOption: "Seçiniz...", priorityLabel: "Aciliyet / Öncelik",
    priorityLowOption: "Düşük (İvediliği Yok)", priorityNormalOption: "Normal (Standart)", priorityHighOption: "Yüksek (Hızlı Çözüm Gerekli)",
    priorityCriticalOption: "Kritik (Sistem Kesintisi / Acil)", detailedDescription: "Detaylı Açıklama",
    requestTitlePlaceholder: "Örn: SQL Yedekleme Hatası", requestDescriptionPlaceholder: "Yaşadığınız sorunu detaylandırın...",
    fileAttachments: "Dosya Ekleri", addFile: "Dosya Ekle", imageFile: "Görsel", removeAttachment: "ekini kaldır", submitRequest: "Talebi Gönder",
    adminPanel: "Yönetici Paneli", requestTracking: "Talep Takip", statistics: "İstatistikler", adminRequests: "Talepler",
    assignedRequests: "Bana Atananlar", configuration: "Tanımlamalar", companies: "Firmalar", users: "Kullanıcılar",
    faqManagement: "SSS Yönetimi", guideManagement: "Kullanım Kılavuzu", systemSettings: "Sistem Ayarları",
    rolesPermissions: "Rol ve Yetkiler", departmentModules: "Departman Modülleri", requestSubjects: "Talep Konuları",
    requestStatuses: "Talep Durumları", organizationSettings: "Kurum Bilgileri", safeLogout: "Güvenli Çıkış",
    adminUser: "Admin Kullanıcı", systemAdministrator: "Sistem Yetkilisi",
    language: "Dil", project: "Proje:", choose: "Seçin", projects: "Projeler",
    supportCenter: "Kurumsal Bilgi Bankası ve Destek Merkezi", createRequest: "Talep Oluştur",
    adminLogin: "Yönetici Girişi", noProjects: "Henüz proje bulunmuyor.",
    search: "Rehber veya soru ara...", guides: "Kullanım Kılavuzları",
    faq: "Sıkça Sorulan Sorular", flow: "İş Akışı", newest: "En yeni",
    alphabetical: "A'dan Z'ye", reverseAlphabetical: "Z'den A'ya",
    previous: "Önceki", next: "Sonraki", page: "Sayfa", of: "/", items: "kayıt",
    shownIn: "İçerik şu dilde gösteriliyor:", translationMissing: "Bu dilde çeviri bulunamadı.",
    guideTitle: "Kullanım Kılavuzları & Başlangıç Rehberleri", faqTitle: "Sıkça Sorulan Sorular & Çözümler",
    flowTitle: "Zihin Haritası & Süreç Akışları", guideDescription: "Dokümantasyonları adım adım inceleyin veya PDF olarak indirin.",
    faqDescription: "Sistem ve entegrasyonlarla ilgili soruların yanıtları.", flowDescription: "İş akış adımlarını baştan sona takip edin.",
    categories: "Portal kategorileri", activeCategory: "Aktif kategori", allModules: "Tüm Modüller", newestUpdated: "En Son Güncellenen", sortAz: "Başlığa Göre (A-Z)", sortZa: "Başlığa Göre (Z-A)",
    searchPortal: "HelpCenter içinde arama yapın, kılavuz veya çözüm sorgulayın...", searchLabel: "Kılavuzlarda ara", searchButton: "ARA",
    attachments: "Ek Dosya", standardGuide: "Standart Rehber", review: "İncele", faqDetails: "Detayı Gör",
    noGuides: "Kullanım Kılavuzu Bulunamadı", noFaqs: "SSS İçeriği Bulunamadı", noFlow: "Zihin Haritası Bulunamadı",
    loginName: "E-posta / kullanıcı adı", password: "Şifre", remember: "Beni hatırla", forgotPassword: "Şifremi unuttum", signIn: "Sisteme giriş yap", showPassword: "Şifreyi göster", hidePassword: "Şifreyi gizle", customerPortal: "Müşteri Portalı", customerLogin: "Müşteri girişi",
    adminPanelVersion: "Yönetici Paneli v2.0", authorizedSignIn: "Lütfen yetkili bilgileri ile oturum açın.", dashboardSignIn: "Müşteri paneline giriş",
    forgotPasswordTitle: "Şifremi Unuttum", stepCodeRequest: "Kod Talebi", stepEnterCode: "Onay Kodunu Girin", stepNewPassword: "Yeni Şifre Oluşturun",
    adminForgotInstructions: "Sistemde kayıtlı e-posta adresinizi veya kullanıcı adınızı girin. Hesabınıza 6 haneli şifre sıfırlama doğrulama kodu gönderilecektir.",
    customerForgotInstructions: "Portal hesabınıza kayıtlı kullanıcı adı veya e-posta adresinizi girin. E-posta adresinize 6 haneli doğrulama kodu gönderilecektir.",
    emailOrUsername: "E-posta / Kullanıcı Adı", adminForgotPlaceholder: "admin@helpcenter.com veya kullanıcı adı", usernameOrEmailPlaceholder: "Kullanıcı adı veya e-posta",
    cancel: "İptal", sendCode: "Kodu Gönder", targetAccount: "Hedef Hesap", account: "Hesap", change: "Değiştir", otpCodeLabel: "E-Postanıza Gelen 6 Haneli Kod (OTP)", continueAction: "İlerle",
    adminVerifiedResetInstruction: "Kod başarıyla doğrulandı. Hesabınız için yeni şifrenizi belirleyin.", customerVerifiedResetInstruction: "Kod başarıyla doğrulandı. Portal hesabınız için yeni şifrenizi belirleyin.",
    newPassword: "Yeni Şifre", minPassword: "En az 6 karakter", confirmNewPassword: "Yeni Şifre Tekrar", confirmNewPasswordPlaceholder: "Yeni şifrenizi tekrar girin", resetPassword: "Şifreyi Sıfırla",
    enterEmailOrUsername: "Lütfen e-posta adresinizi veya kullanıcı adınızı girin.", enterValidEmail: "Lütfen geçerli bir e-posta adresi girin.", invalidSixDigitCode: "Doğrulama kodu 6 haneli sayısal kod olmalıdır.",
    passwordMinimum6: "Yeni şifre en az 6 karakter olmalıdır.", passwordsMismatch: "Şifreler uyuşmuyor. Lütfen tekrar kontrol edin.",
    passwordResetSuccess: "Şifreniz başarıyla sıfırlandı. Giriş ekranına aktarılıyorsunuz...", back: "Geri dön", adminPanelFooter: "Yönetici Paneli v2.0.4",
    sortLabel: "Sıralama:", noGuideForProject: "Bu proje için henüz kullanım kılavuzu yayınlanmamış.", modulesLabel: "Modül", guideLabel: "Kılavuz", faqLabel: "SSS", systemStatus: "Sistem Durumu", privacy: "Gizlilik Politikası", contactSupport: "İletişim & Destek", copyright: "Tüm hakları saklıdır.",
    close: "Kapat", faqDetail: "SSS Detayı", guideDetail: "Rehber Detayı", notFound: "İçerik bulunamadı.",
  },
  "en-US": {
    dateLabel: "Date", messageSingular: "message", messagePlural: "messages", fileSingular: "file", filePlural: "files", viewDetails: "View details",
    closeRequest: "Close request", requestCompleted: "Request completed", requestConversation: "Request conversation",
    sendMessagePlaceholder: "Write your message here...", sendMessageWithAttachmentsPlaceholder: "Write your message here or attach files...", sendMessage: "Send message", addAttachment: "Add attachment", removeSelectedFile: "Remove selected file", selectAttachmentType: "Select file type", pdfFile: "PDF file", excelFile: "Excel file",
    requestResolvedNoNewMessage: "This request is closed, so new messages cannot be sent.", rating: "Rating", rateServiceTitle: "Close request", serviceRatingPrompt: "How would you rate the service you received?", ratingVeryBad: "Very poor", ratingBad: "Poor", ratingAverage: "Average", ratingGood: "Good", ratingExcellent: "Excellent", feedbackOptional: "Your feedback (Optional)", feedbackPlaceholder: "Would you like to leave a note?", cancelAction: "Cancel", saveAndClose: "Save and close", loadingMessages: "Loading messages...", sessionInfoMissing: "Session information was not found.", requestClosedSuccess: "Your request was closed. Thank you for your feedback!",
    speakingWithSupport: "You are speaking with customer support", requestStatus: "Request status",
    priorityCritical: "Critical", priorityHigh: "High", priorityNormal: "Normal", priorityLow: "Low",
    statusWaitingForReply: "Waiting for reply", statusReplied: "Replied", statusCompleted: "Completed", statusUnderReview: "Under technical review",
    corporateSupportPanel: "Corporate support portal", customerAdministrator: "Company administrator", signOut: "Sign out", sessionTime: "Session time",
    sessionExpiryNotice: "For your security, you will be signed out after 3 minutes of inactivity.", quickAccess: "Quick access",
    myRequests: "My requests", requestHistory: "Request history", customerProfile: "My profile", activeSupportRequests: "Active support requests",
    completedRequests: "Completed requests", activeSupportDescription: "Support requests currently being handled.",
    completedSupportDescription: "Support requests resolved in the past.", createNewRequest: "Create a request",
    allRequests: "All", noCompletedRequests: "No completed requests", noActiveRequests: "No active requests",
    emptyRequestList: "There are no records to show here yet.", guidesDescription: "User guides for our products and services.",
    guideSearch: "Search guides...", noGuideResults: "No guides found.", watchVideoGuide: "Watch video guide", attachmentsList: "Attachments",
    previousStep: "Previous step", nextStep: "Next step", profileSettings: "Profile settings",
    profileDescription: "View and update your account information here.", firstName: "First name", lastName: "Last name",
    immutableEmail: "Email (cannot be changed)", saveChanges: "Save changes",
    newRequestTitle: "New support request", technicalSupport: "Technical support", closeNewRequest: "Close the new request dialog",
    requiredFieldsNotice: "Complete the required fields.", requestTitle: "Request title", requestSubject: "Request subject",
    relatedModule: "Related module", selectOption: "Select...", priorityLabel: "Urgency / priority",
    priorityLowOption: "Low (Not urgent)", priorityNormalOption: "Normal (Standard)", priorityHighOption: "High (Needs quick resolution)",
    priorityCriticalOption: "Critical (System outage / urgent)", detailedDescription: "Detailed description",
    requestTitlePlaceholder: "For example: SQL backup failure", requestDescriptionPlaceholder: "Describe the issue you are experiencing...",
    fileAttachments: "Attachments", addFile: "Add file", imageFile: "Image", removeAttachment: "attachment", submitRequest: "Submit request",
    adminPanel: "Admin panel", requestTracking: "Request tracking", statistics: "Statistics", adminRequests: "Requests",
    assignedRequests: "Assigned to me", configuration: "Configuration", companies: "Companies", users: "Users",
    faqManagement: "FAQ management", guideManagement: "Guides", systemSettings: "System settings",
    rolesPermissions: "Roles and permissions", departmentModules: "Department modules", requestSubjects: "Request subjects",
    requestStatuses: "Request statuses", organizationSettings: "Organization settings", safeLogout: "Sign out",
    adminUser: "Admin user", systemAdministrator: "System administrator",
    language: "Language", project: "Project:", choose: "Choose", projects: "Projects",
    supportCenter: "Knowledge Base and Support Center", createRequest: "Create a request",
    adminLogin: "Admin sign in", noProjects: "No projects are available yet.",
    search: "Search guides or questions...", guides: "Guides",
    faq: "Frequently asked questions", flow: "Workflow", newest: "Newest",
    alphabetical: "A to Z", reverseAlphabetical: "Z to A",
    previous: "Previous", next: "Next", page: "Page", of: "of", items: "items",
    shownIn: "This content is shown in:", translationMissing: "No translation is available in this language.",
    guideTitle: "Guides and getting started", faqTitle: "Frequently asked questions and solutions",
    flowTitle: "Mind map and workflows", guideDescription: "Browse documentation step by step or download it as a PDF.",
    faqDescription: "Answers to common questions about the system and integrations.", flowDescription: "Follow workflow steps from beginning to end.",
    categories: "Portal categories", activeCategory: "Active category", allModules: "All modules", newestUpdated: "Recently updated", sortAz: "Title (A–Z)", sortZa: "Title (Z–A)",
    searchPortal: "Search HelpCenter guides and solutions...", searchLabel: "Search guides", searchButton: "SEARCH",
    attachments: "Attachments", standardGuide: "Guide", review: "Read", faqDetails: "View details",
    noGuides: "No guides found", noFaqs: "No FAQs found", noFlow: "No workflow map found",
    loginName: "Email / username", password: "Password", remember: "Remember me", forgotPassword: "Forgot password?", signIn: "Sign in", showPassword: "Show password", hidePassword: "Hide password", customerPortal: "Customer portal", customerLogin: "Customer sign in",
    adminPanelVersion: "Admin Panel v2.0", authorizedSignIn: "Sign in with your authorized account.", dashboardSignIn: "Sign in to the customer dashboard",
    forgotPasswordTitle: "Forgot password", stepCodeRequest: "Request a code", stepEnterCode: "Enter the verification code", stepNewPassword: "Create a new password",
    adminForgotInstructions: "Enter the email address or username registered in the system. A six-digit password reset code will be sent to your account.",
    customerForgotInstructions: "Enter the username or email address registered to your portal account. A six-digit verification code will be sent to your email.",
    emailOrUsername: "Email / username", adminForgotPlaceholder: "admin@helpcenter.com or username", usernameOrEmailPlaceholder: "Username or email",
    cancel: "Cancel", sendCode: "Send code", targetAccount: "Target account", account: "Account", change: "Change", otpCodeLabel: "Six-digit code sent to your email (OTP)", continueAction: "Continue",
    adminVerifiedResetInstruction: "The code has been verified. Set a new password for your account.", customerVerifiedResetInstruction: "The code has been verified. Set a new password for your portal account.",
    newPassword: "New password", minPassword: "At least 6 characters", confirmNewPassword: "Confirm new password", confirmNewPasswordPlaceholder: "Enter your new password again", resetPassword: "Reset password",
    enterEmailOrUsername: "Enter your email address or username.", enterValidEmail: "Enter a valid email address.", invalidSixDigitCode: "The verification code must contain six digits.",
    passwordMinimum6: "The new password must contain at least six characters.", passwordsMismatch: "The passwords do not match. Check them and try again.",
    passwordResetSuccess: "Your password has been reset. Returning to the sign-in page...", back: "Go back", adminPanelFooter: "Admin Panel v2.0.4",
    sortLabel: "Sort by:", noGuideForProject: "No guides have been published for this project yet.", modulesLabel: "Modules", guideLabel: "Guides", faqLabel: "FAQs", systemStatus: "System status", privacy: "Privacy policy", contactSupport: "Contact support", copyright: "All rights reserved.",
    close: "Close", faqDetail: "FAQ details", guideDetail: "Guide details", notFound: "Content not found.",
  },
} as const;

export type MessageKey = keyof (typeof messages)["tr-TR"];

export function isLocale(value: string | null | undefined): value is Locale {
  return !!value && (LOCALES as readonly string[]).includes(value);
}

export function localeFromPath(value: string | null | undefined): Locale | null {
  if (value === "tr") return "tr-TR";
  if (value === "en") return "en-US";
  return isLocale(value) ? value : null;
}

export function localePath(locale: Locale): LocalePath {
  return LOCALE_CONFIG[locale].path;
}

export function localeFromPreference(value: string | null | undefined): Locale | null {
  if (!value) return null;
  const normalized = value.toLowerCase();
  if (normalized.startsWith("tr")) return "tr-TR";
  if (normalized.startsWith("en")) return "en-US";
  return null;
}
