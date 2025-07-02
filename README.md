
## معرفی کلی

سرویس minio-Uploader یک API مبتنی بر ASP.NET Core برای مدیریت آپلود و دانلود فایل روی MinIO یا S3-compatible object storage است. این سرویس امکانات جامعی برای آپلود انواع فایل از منابع مختلف (فرم، URL، base64 و...) و همچنین دانلود، دریافت متادیتا و لینک‌های دسترسی را فراهم می‌کند.

---

## ساختار کنترلرها

### 1. FileUploadController

آدرس پایه:  
`/api/v1.0/FileUpload`

#### متدهای اصلی:

- **آپلود فایل تکی از فرم**  
  مسیر: `POST /{bucketName}`  
  ورودی:  
    - bucketName (در مسیر)
    - فایل (در فرم-دیتا)
  خروجی: آدرس فایل آپلودشده.

- **آپلود فایل به مسیر خاص**  
  مسیر: `POST /{bucketName}/to-path`  
  ورودی:  
    - bucketName (در مسیر)
    - File (در فرم)
    - DestinationPath (در فرم)
  خروجی: آدرس فایل آپلودشده.

- **آپلود چند فایل**  
  مسیر: `POST /multiple/{bucketName}`  
  ورودی:  
    - bucketName (در مسیر)
    - files (لیست فایل در فرم)
  خروجی: لیست آدرس فایل‌ها.

- **آپلود چند فایل به مسیر خاص**  
  مسیر: `POST /multiple/{bucketName}/to-path`  
  ورودی:  
    - bucketName (در مسیر)
    - files (لیست فایل در فرم)
    - DestinationPath (در فرم)
  خروجی: لیست آدرس فایل‌ها.

- **آپلود فایل از URL**  
  مسیر: `POST /from-url/{bucketName}`  
  ورودی:  
    - bucketName (در مسیر)
    - url (در body)
  خروجی: آدرس فایل آپلودشده.

- **آپلود فایل از URL به مسیر خاص**  
  مسیر: `POST /from-url/{bucketName}/to-path`  
  ورودی:  
    - bucketName (در مسیر)
    - url و DestinationPath (در body)
  خروجی: آدرس فایل آپلودشده.

- **آپلود چند فایل از لیست URL**  
  مسیر: `POST /from-urls/{bucketName}`  
  ورودی:  
    - bucketName (در مسیر)
    - urls (لیست URL در body)
  خروجی: لیست آدرس فایل‌ها.

- **آپلود چند فایل URL به مسیر خاص**  
  مسیر: `POST /from-urls/{bucketName}/to-path`  
  ورودی:  
    - bucketName (در مسیر)
    - urls و DestinationPath (در body)
  خروجی: لیست آدرس فایل‌ها.

- **آپلود فایل از رشته base64**  
  مسیر: `POST /from-base64/{bucketName}`  
  ورودی:  
    - bucketName (در مسیر)
    - FileBase64AsString و FileExtension (در body)
  خروجی: آدرس فایل آپلودشده.

- **آپلود چند فایل با base64**  
  مسیر: `POST /from-multi-base64/{bucketName}`  
  ورودی:  
    - bucketName (در مسیر)
    - files (لیست فایل base64 در body)
  خروجی: لیست آدرس فایل‌ها.

---

### 2. FileDownloadController

آدرس پایه:  
`/api/FileDownload`

#### متدهای اصلی:

- **دانلود فایل (HTTP GET)**  
  مسیر: `GET /{bucketName}/{path}`  
  خروجی: فایل جهت دانلود.

- **دریافت فایل به صورت byteArray**  
  مسیر: `GET /{bucketName}/{path}/byteArray`  
  خروجی: آبجکت ByteArrayFileContent

- **دانلود فایل به صورت stream**  
  مسیر: `GET /{bucketName}/{path}/stream`  
  خروجی: فایل به صورت stream با header مناسب

- **دریافت متادیتای فایل**  
  مسیر: `GET /{bucketName}/{path}/metadata`  
  خروجی: FileMetadataDto

- **دریافت presigned url**  
  مسیر: `GET /{bucketName}/{objectName}/presignedUrl?expiresInMinutes=60`  
  خروجی: رشته‌ی URL موقت برای دانلود فایل

- **دانلود بخشی از فایل (partial download)**  
  مسیر: `GET /{bucketName}/{path}/partial`  
  خروجی: فایل با قابلیت range processing

- **لیست فایل‌ها در یک bucket**  
  مسیر: `GET /{bucketName}/list?prefix=myfolder/`  
  خروجی: لیست نام فایل‌ها

---

## ساختار ورودی و خروجی

- ورودی اکثر متدها از طریق route parameter یا form-data و یا body (json) دریافت می‌شود.
- خروجی اکثر متدها از نوع Result یا لیستی از Result است که در صورت موفقیت، success=true و data حاوی نتیجه، و در صورت خطا success=false و error/details بازگردانده می‌شود.

---

## نمونه‌های درخواست با curl

**آپلود فایل تکی:**
```bash
curl -X POST "http://localhost:5000/api/v1.0/FileUpload/mybucket" -F "file=@/path/to/myfile.jpg"
```

**آپلود فایل از URL:**
```bash
curl -X POST "http://localhost:5000/api/v1.0/FileUpload/from-url/mybucket" -H "Content-Type: application/json" -d '"https://example.com/file.jpg"'
```

**دریافت presigned url:**
```bash
curl "http://localhost:5000/api/FileDownload/mybucket/myfile.jpg/presignedUrl?expiresInMinutes=120"
```

**دریافت متادیتای فایل:**
```bash
curl "http://localhost:5000/api/FileDownload/mybucket/myfile.jpg/metadata"
```

---

## سایر نکات پروژه

- **کلید API:** برای برخی endpointها باید API Key در header ارسال کنید.
- **پیکربندی:** اطلاعات اتصال به Minio/S3 و تنظیمات باید در appsettings.json یا environment variables وارد شود.
- **توسعه‌دهندگان:**  
  - سرویس‌های اصلی آپلود و دانلود در پوشه Services هستند.
  - مدل‌های داده در پوشه Models قرار دارند.
- **Swagger:** مستندات و تست آنلاین endpointها از طریق Swagger UI در دسترس است.

---

## مدیریت خطاها

ساختار خطاها به صورت زیر است:
```json
{
  "success": false,
  "error": "شرح خطا",
  "details": ["پیغام‌های خطا"]
}
```
