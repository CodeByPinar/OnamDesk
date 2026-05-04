\# 🏥 OnamDesk



<div align="center">



\## Klinik Onam, Dijital İmza, PDF Arşiv ve Audit Log Sistemi



\*\*OnamDesk\*\*, kliniklerde hasta onam süreçlerini daha güvenli, düzenli, izlenebilir ve offline çalışabilir hale getirmek için geliştirilmiş WPF tabanlı bir masaüstü uygulamasıdır.



!\[.NET](https://img.shields.io/badge/.NET-8.0-512BD4?style=for-the-badge\\\&logo=dotnet\\\&logoColor=white)

!\[WPF](https://img.shields.io/badge/WPF-Desktop\_App-0F766E?style=for-the-badge\\\&logo=windows\\\&logoColor=white)

!\[SQLite](https://img.shields.io/badge/SQLite-Local\_DB-003B57?style=for-the-badge\\\&logo=sqlite\\\&logoColor=white)

!\[EF Core](https://img.shields.io/badge/EF\_Core-ORM-6C2BD9?style=for-the-badge)

!\[MVVM](https://img.shields.io/badge/MVVM-Architecture-1D4ED8?style=for-the-badge)

!\[Status](https://img.shields.io/badge/Status-MVP\_v1.0-22C55E?style=for-the-badge)



</div>



\---



\## 📌 Proje Özeti



\*\*OnamDesk\*\*, hasta bilgilendirme ve onam formlarını dijital ortamda hazırlamak, hastadan gerçek imza almak, imzalı PDF üretmek, kayıtları arşivlemek ve tüm kritik işlemleri audit log ile takip etmek için tasarlanmış bir klinik masaüstü yazılımıdır.



Uygulama \*\*offline-first\*\* mantıkla çalışır. Yani veriler yerel cihazda tutulur, dış bulut servisine bağımlı değildir. Hasta kayıtları, onam şablonları, imzalı formlar, PDF yolları, sistem logları ve otomatik yedekleme süreçleri uygulama içinde yönetilir.



\---



\## 🧩 Özellik Widget’ları



| Modül                  |        Durum | Açıklama                                                                      |

| ---------------------- | -----------: | ----------------------------------------------------------------------------- |

| 👤 Hasta Yönetimi      | ✅ Tamamlandı | Hasta ekleme, güncelleme, arama, silme ve doğrulama işlemleri.                |

| 📋 Şablon Yönetimi     | ✅ Tamamlandı | Onam şablonu oluşturma, güncelleme, aktif/pasif yönetimi.                     |

| 📝 Onam Formu          | ✅ Tamamlandı | Hasta + şablon + doktor + not + imza ile onam oluşturma.                      |

| ✍️ Dijital İmza        | ✅ Tamamlandı | WPF InkCanvas ile gerçek hasta imzası alma.                                   |

| 🔐 İmza Hash Doğrulama | ✅ Tamamlandı | İmza verisinden SHA-256 hash üretme ve doğrulama.                             |

| 📄 PDF Üretimi         | ✅ Tamamlandı | İmza, hasta bilgisi, şablon ve hash içeren PDF üretimi.                       |

| 📁 Arşiv               | ✅ Tamamlandı | Onam kayıtlarını listeleme, arama, silme, PDF açma ve hash doğrulama.         |

| 🛡️ Audit Log          | ✅ Tamamlandı | Kritik işlemleri kullanıcı, tarih ve JSON detaylarıyla kayıt altına alma.     |

| ⚙️ Ayarlar             | ✅ Tamamlandı | Klinik adı, doktor adı, kullanıcı adı, PDF başlığı ve arşiv klasörü yönetimi. |

| 🔑 Login Sistemi       | ✅ Tamamlandı | BCrypt şifre doğrulama, hatalı giriş kilidi, şifre değiştirme.                |

| 💾 Otomatik Yedekleme  | ✅ Tamamlandı | Uygulama kapanırken SQLite veritabanını otomatik yedekleme.                   |

| 📊 Dashboard           | ✅ Tamamlandı | Sistem metrikleri, son onamlar ve son audit log kayıtları.                    |



\---



\## 🖥️ Uygulama Ekranları



\### 🏠 Ana Sayfa / Dashboard



Dashboard ekranı klinik sisteminin hızlı özetini verir:



\* Toplam hasta sayısı

\* Toplam onam kaydı

\* PDF oluşturulmuş kayıt sayısı

\* Toplam şablon sayısı

\* Aktif şablon sayısı

\* Toplam audit log sayısı

\* Son onam kayıtları

\* Son sistem işlemleri



\---



\### 👤 Hasta Yönetimi



Hasta yönetimi ekranında hasta kayıtları yönetilir.



Desteklenen alanlar:



\* Hasta adı soyadı

\* TC kimlik numarası

\* Doğum tarihi

\* Telefon numarası



Öne çıkan noktalar:



\* TC kimlik numarası doğrulama

\* Telefon format kontrolü

\* Aynı TC ile tekrar kayıt engelleme

\* Hasta üzerinde onam kaydı varsa silme koruması

\* Hasta işlemlerini audit log’a yazma



Audit log action örnekleri:



```txt

PATIENT\_CREATED

PATIENT\_UPDATED

PATIENT\_DELETED

```



\---



\### 📋 Şablon Yönetimi



Klinikte kullanılan onam şablonları bu ekrandan yönetilir.



Şablon alanları:



\* Şablon adı

\* Kategori

\* Açıklama / içerik

\* Risk ve komplikasyonlar

\* Aktif / pasif durumu



Öne çıkan noktalar:



\* Aynı isimde şablon oluşturma engeli

\* Aktif şablonlar Onam Formu ekranında listelenir

\* Onam kaydında kullanılmış şablon silinemez

\* Tüm işlemler audit log’a yazılır



Audit log action örnekleri:



```txt

TEMPLATE\_CREATED

TEMPLATE\_UPDATED

TEMPLATE\_DELETED

```



\---



\### 📝 Onam Formu



Onam Formu ekranı dijital onam oluşturma akışının merkezidir.



İş akışı:



```txt

Hasta seç

&#x20;  ↓

Aktif şablon seç

&#x20;  ↓

Doktor bilgisi ve not gir

&#x20;  ↓

Hasta imzası al

&#x20;  ↓

İmzalı onam kaydı oluştur

&#x20;  ↓

Arşivde görüntüle

&#x20;  ↓

PDF oluştur

```



Öne çıkan özellikler:



\* Doktor adı Ayarlar ekranındaki varsayılan doktor adından gelir

\* Şablon içeriği ve risk metinleri önizlenir

\* Hasta imzası `InkCanvas` üzerinden alınır

\* İmza verisi Base64 formatında saklanır

\* İmza verisinden SHA-256 hash üretilir

\* Onam oluşturma işlemi audit log’a yazılır



Audit log action örnekleri:



```txt

CONSENT\_CREATED

CONSENT\_CREATED\_TEST

```



\---



\### 📁 Arşiv



Arşiv ekranı oluşturulan onam kayıtlarını yönetmek için kullanılır.



Özellikler:



\* Onam kayıtlarını listeleme

\* Hasta / doktor / şablon bazlı arama

\* Seçili onam detaylarını görüntüleme

\* İmza hash doğrulama

\* PDF oluşturma

\* PDF açma

\* Onam kaydı silme



Audit log action örnekleri:



```txt

PDF\_CREATED

CONSENT\_DELETED

```



\---



\### 📄 PDF Üretimi



PDF içinde bulunan bilgiler:



\* PDF başlığı

\* Hasta adı soyadı

\* Doğum tarihi

\* Doktor adı

\* İmzalanma tarihi

\* Şablon adı

\* Kategori

\* Açıklama / içerik

\* Risk ve komplikasyonlar

\* Ek notlar

\* Hasta imza görseli

\* İmza hash değeri

\* Sistem tarafından oluşturuldu bilgisi



PDF özellikleri:



\* PDF başlığı Ayarlar ekranından gelir

\* Arşiv klasörü Ayarlar ekranından gelir

\* Uzun metinlerde otomatik yeni sayfa açılır

\* İmza görseli PDF içine eklenir

\* PDF yolu veritabanına kaydedilir



\---



\### 🛡️ Audit Log



Audit Log ekranı sistemdeki kritik işlemleri izlemek için kullanılır.



Kaydedilen bilgiler:



\* Action adı

\* Kullanıcı adı

\* İlgili onam kaydı ID’si

\* Tarih / saat

\* JSON detay verisi



Audit log örnekleri:



```txt

LOGIN\_SUCCESS

LOGIN\_FAILED

LOGOUT\_REQUESTED

LOGOUT\_CANCELLED

LOGOUT\_SUCCESS

RELOGIN\_SUCCESS

PASSWORD\_CHANGED

SETTINGS\_UPDATED

PATIENT\_CREATED

PATIENT\_UPDATED

PATIENT\_DELETED

TEMPLATE\_CREATED

TEMPLATE\_UPDATED

TEMPLATE\_DELETED

CONSENT\_CREATED

PDF\_CREATED

CONSENT\_DELETED

DATABASE\_BACKUP\_CREATED

DATABASE\_BACKUP\_FAILED

```



\---



\### ⚙️ Ayarlar



Ayarlar ekranından yönetilen alanlar:



\* Klinik adı

\* Varsayılan doktor adı

\* Aktif kullanıcı adı

\* PDF başlığı

\* PDF arşiv klasörü

\* Uygulama şifresi

\* Son yedekler



Öne çıkan özellikler:



\* Ayarlar `settings.json` dosyasına kaydedilir

\* Klinik ve kullanıcı bilgisi sol menüde anlık güncellenir

\* PDF başlığı ve arşiv klasörü PDF üretimine bağlanmıştır

\* Şifre değişikliği BCrypt altyapısıyla yapılır

\* Son yedekler listelenebilir

\* Seçili yedek dosyası Windows Explorer içinde gösterilebilir



\---



\## 🔑 Güvenlik Özellikleri



| Güvenlik Özelliği   | Açıklama                                               |

| ------------------- | ------------------------------------------------------ |

| Offline çalışma     | Veriler yerel cihazda saklanır.                        |

| BCrypt şifreleme    | Uygulama giriş şifresi hash’lenerek saklanır.          |

| Hatalı giriş kilidi | 5 hatalı girişten sonra geçici kilitleme uygulanır.    |

| Şifre değiştirme    | Ayarlar ekranından uygulama şifresi değiştirilebilir.  |

| İmza hash doğrulama | İmza verisi SHA-256 hash ile doğrulanır.               |

| Audit log           | Kritik işlemler JSON detaylarıyla kayıt altına alınır. |

| Otomatik yedekleme  | Uygulama kapanışında veritabanı yedeği alınır.         |

| TC doğrulama        | Hasta TC kimlik alanı format kontrolünden geçer.       |

| Silme koruması      | Onam kaydı olan hasta veya şablon silinemez.           |



\---



\## 🧠 Sistem Mimarisi



```mermaid

flowchart TD

&#x20;   A\[LoginView] --> B\[MainWindow]

&#x20;   B --> C\[Dashboard]

&#x20;   B --> D\[Hasta Yönetimi]

&#x20;   B --> E\[Şablon Yönetimi]

&#x20;   B --> F\[Onam Formu]

&#x20;   B --> G\[Arşiv]

&#x20;   B --> H\[Audit Log]

&#x20;   B --> I\[Ayarlar]



&#x20;   F --> J\[InkCanvas İmza]

&#x20;   J --> K\[SignatureService]

&#x20;   K --> L\[SHA-256 Hash]

&#x20;   F --> M\[ConsentService]

&#x20;   M --> N\[(SQLite Database)]



&#x20;   G --> O\[PdfService]

&#x20;   O --> P\[PDF Arşiv]

&#x20;   O --> Q\[İmza Görseli]



&#x20;   B --> R\[BackupService]

&#x20;   R --> S\[Database Backup]



&#x20;   D --> T\[PatientService]

&#x20;   E --> U\[TemplateService]

&#x20;   H --> V\[AuditLogService]

&#x20;   I --> W\[SettingsService]



&#x20;   T --> N

&#x20;   U --> N

&#x20;   V --> N

&#x20;   W --> X\[settings.json]

```



\---



\## 🗃️ Veritabanı Diyagramı



```mermaid

erDiagram

&#x20;   PATIENTS ||--o{ CONSENT\_FORMS : has

&#x20;   TEMPLATES ||--o{ CONSENT\_FORMS : used\_by

&#x20;   CONSENT\_FORMS ||--o{ AUDIT\_LOGS : tracked\_by



&#x20;   PATIENTS {

&#x20;       int Id

&#x20;       string FullName

&#x20;       string TcNoEncrypted

&#x20;       datetime BirthDate

&#x20;       string Phone

&#x20;   }



&#x20;   TEMPLATES {

&#x20;       int Id

&#x20;       string Name

&#x20;       string Category

&#x20;       string ContentJson

&#x20;       string Risks

&#x20;       bool IsActive

&#x20;   }



&#x20;   CONSENT\_FORMS {

&#x20;       int Id

&#x20;       int PatientId

&#x20;       int TemplateId

&#x20;       string SignatureData

&#x20;       string SignatureHash

&#x20;       datetime SignedAt

&#x20;       string PdfPath

&#x20;       string DoctorName

&#x20;       string Notes

&#x20;   }



&#x20;   AUDIT\_LOGS {

&#x20;       int Id

&#x20;       int ConsentId

&#x20;       string Action

&#x20;       string UserName

&#x20;       datetime Timestamp

&#x20;       string Details

&#x20;   }

```



\---



\## 🔄 Onam Oluşturma Akışı



```mermaid

sequenceDiagram

&#x20;   participant User as Kullanıcı

&#x20;   participant Consent as Onam Formu

&#x20;   participant Signature as SignatureService

&#x20;   participant DB as SQLite

&#x20;   participant Audit as AuditLogService



&#x20;   User->>Consent: Hasta ve şablon seçer

&#x20;   User->>Consent: İmza atar

&#x20;   Consent->>Signature: İmza verisinden hash üret

&#x20;   Signature-->>Consent: SHA-256 hash

&#x20;   Consent->>DB: Onam kaydını kaydet

&#x20;   Consent->>Audit: CONSENT\_CREATED logu yaz

&#x20;   Audit->>DB: Audit kaydı oluştur

```



\---



\## 📄 PDF Üretim Akışı



```mermaid

flowchart LR

&#x20;   A\[Arşivden kayıt seçilir] --> B\[PDF Oluştur]

&#x20;   B --> C\[PdfService]

&#x20;   C --> D\[Ayarları oku]

&#x20;   C --> E\[İmza görselini hazırla]

&#x20;   C --> F\[PDF dosyası oluştur]

&#x20;   F --> G\[PdfPath veritabanına kaydet]

&#x20;   G --> H\[PDF\_CREATED audit log]

```



\---



\## 💾 Yedekleme Akışı



```mermaid

flowchart TD

&#x20;   A\[Uygulama kapanır] --> B\[BackupService çalışır]

&#x20;   B --> C\[onamdesk.db bulunur]

&#x20;   C --> D\[Belgeler/OnamDesk/Backups klasörü hazırlanır]

&#x20;   D --> E\[Tarih-saatli .db yedeği oluşturulur]

&#x20;   E --> F\[Son 10 yedek tutulur]

&#x20;   F --> G\[DATABASE\_BACKUP\_CREATED audit log]

```



\---



\## 🛠️ Kullanılan Teknolojiler



| Teknoloji             | Kullanım Amacı                              |

| --------------------- | ------------------------------------------- |

| .NET 8                | Ana uygulama platformu                      |

| WPF                   | Masaüstü arayüz                             |

| MVVM                  | UI ve iş mantığı ayrımı                     |

| CommunityToolkit.Mvvm | ObservableProperty, RelayCommand, Messenger |

| Entity Framework Core | ORM ve veritabanı erişimi                   |

| SQLite                | Yerel veritabanı                            |

| PdfSharp              | PDF oluşturma                               |

| BCrypt.Net            | Şifre hashleme ve doğrulama                 |

| Newtonsoft.Json       | Settings, auth ve audit detay serileştirme  |

| InkCanvas             | Hasta imzası alma                           |



\---



\## 📁 Proje Yapısı



```txt

OnamDesk/

├── Data/

│   └── AppDbContext.cs

│

├── Helpers/

│   ├── SettingsUpdatedMessage.cs

│   └── WindowsFontResolver.cs

│

├── Models/

│   ├── Patient.cs

│   ├── Template.cs

│   ├── ConsentForm.cs

│   └── AuditLog.cs

│

├── Services/

│   ├── PatientService.cs

│   ├── TemplateService.cs

│   ├── ConsentService.cs

│   ├── SignatureService.cs

│   ├── PdfService.cs

│   ├── AuditLogService.cs

│   ├── SettingsService.cs

│   ├── AuthService.cs

│   ├── BackupService.cs

│   ├── DashboardService.cs

│   └── EncryptionService.cs

│

├── ViewModels/

│   ├── MainViewModel.cs

│   ├── LoginViewModel.cs

│   ├── DashboardViewModel.cs

│   ├── PatientViewModel.cs

│   ├── TemplateViewModel.cs

│   ├── ConsentViewModel.cs

│   ├── ArchiveViewModel.cs

│   ├── AuditLogViewModel.cs

│   ├── SettingsViewModel.cs

│   └── ViewModelBase.cs

│

├── Views/

│   ├── LoginView.xaml

│   ├── DashboardView.xaml

│   ├── PatientView.xaml

│   ├── TemplateView.xaml

│   ├── ConsentView.xaml

│   ├── ArchiveView.xaml

│   ├── AuditLogView.xaml

│   └── SettingsView.xaml

│

├── Migrations/

├── App.xaml

├── MainWindow.xaml

└── OnamDesk.csproj

```



\---



\## 🔐 Varsayılan Giriş Bilgisi



İlk çalıştırmada varsayılan şifre:



```txt

onam1234

```



İlk girişten sonra bu şifre \*\*Ayarlar > Güvenlik / Şifre Değiştir\*\* bölümünden değiştirilmelidir.



\---



\## 💾 Yedekleme



Uygulama kapatıldığında veritabanı otomatik olarak yedeklenir.



Varsayılan yedek klasörü:



```txt

Belgeler/OnamDesk/Backups

```



Yedek dosya formatı:



```txt

onamdesk\_backup\_yyyyMMdd\_HHmmss.db

```



Sistem varsayılan olarak son 10 yedeği tutacak şekilde tasarlanmıştır.



\---



\## 📊 MVP Budget / Kapsam Planı



| Kapsam                |        Durum | Öncelik |

| --------------------- | -----------: | ------: |

| Hasta yönetimi        | ✅ Tamamlandı |  Yüksek |

| Şablon yönetimi       | ✅ Tamamlandı |  Yüksek |

| Dijital imza          | ✅ Tamamlandı |  Yüksek |

| PDF üretimi           | ✅ Tamamlandı |  Yüksek |

| Audit log             | ✅ Tamamlandı |  Yüksek |

| Login sistemi         | ✅ Tamamlandı |  Yüksek |

| Otomatik yedekleme    | ✅ Tamamlandı |    Orta |

| Dashboard             | ✅ Tamamlandı |    Orta |

| Yedekten geri yükleme |  ⏳ Planlandı |  Yüksek |

| QR doğrulama          |  ⏳ Planlandı |    Orta |

| Excel / CSV export    |  ⏳ Planlandı |    Orta |

| Çoklu kullanıcı / rol |  ⏳ Planlandı |  Yüksek |

| Klinik logo desteği   |  ⏳ Planlandı |   Düşük |

| Tema seçimi           |  ⏳ Planlandı |   Düşük |



\---



\## 🚀 Gelecek Özellikler



\* Arşivden PDF silme / yeniden oluşturma

\* Manuel yedek al butonu

\* Yedekten geri yükleme sistemi

\* Ayarlar ekranında klasör seçici

\* Dashboard hızlı işlem butonları

\* Dashboard aylık onam grafiği

\* Hasta detay ekranı

\* Hastaya ait onam geçmişi

\* Onam kaydı detay ekranı

\* PDF önizleme alanı

\* Onam formu yazdırma

\* PDF’e QR doğrulama kodu ekleme

\* İmza hash doğrulama raporu

\* Audit log dışa aktarma

\* Hasta listesini Excel / CSV dışa aktarma

\* Şablon kopyalama / çoğaltma

\* Şablon kategori yönetimi

\* Arşiv gelişmiş filtreleme

\* Tarih aralığına göre onam arama

\* Tema seçimi: açık / koyu

\* Oturum zaman aşımı

\* Veritabanı sağlık kontrolü

\* İlk kurulum sihirbazı

\* Klinik logo yükleme

\* PDF’e klinik logosu ekleme

\* PDF footer: klinik adı, tarih, sayfa numarası

\* KVKK / aydınlatma metni hazır şablonları

\* Doktor listesi yönetimi

\* Çoklu kullanıcı / rol sistemi

\* Sistem bakım ekranı

\* Hata log dosyası sistemi



\---



\## ⚠️ Not



Bu proje şu anda MVP / portföy projesi olarak geliştirilmiştir. Gerçek klinik kullanım öncesinde KVKK, mevzuat, veri güvenliği, yedekleme politikası, kullanıcı yetkilendirme ve hukuki onam süreçleri açısından profesyonel değerlendirme yapılmalıdır.



\---



\## 👩‍💻 Developer



\*\*Pınar Topuz\*\*



\* Backend / Full-Stack Development

\* .NET, WPF, MVVM, EF Core, SQLite

\* Secure local desktop application design

\* Clinical workflow automation MVP



\---



\## 📄 License



This project is currently developed as a portfolio and MVP project.



