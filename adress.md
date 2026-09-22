# راهنمای پیاده‌سازی Checkout و مدیریت آدرس

## PaperSite Frontend

## هدف تغییر

فرآیند خرید باید ساده شود؛ کاربر نباید در صفحه Checkout هر بار:

* نام و نام خانوادگی
* شماره موبایل
* آدرس کامل

را وارد کند.

اطلاعات کاربر از حساب کاربری گرفته می‌شود و فقط آدرس ارسال انتخاب می‌شود.

---

# Flow جدید Checkout

## حالت اول: کاربر آدرس ذخیره شده دارد

زمان ورود به صفحه Checkout:

ابتدا API زیر فراخوانی شود:

```
GET /api-v1/Address/GetMyAddresses
```

نمونه پاسخ:

```json
[
  {
    "id": "guid",
    "title": "خانه",
    "province": "تهران",
    "city": "تهران",
    "fullAddress": "خیابان مثال...",
    "postalCode": "1234567890",
    "isDefault": true
  }
]
```

نمایش در صفحه:

```
اطلاعات گیرنده

نام:
نام کاربر از پروفایل

شماره:
شماره کاربر


انتخاب آدرس ارسال:

● خانه
تهران، خیابان ...

○ شرکت
تهران، خیابان ...


+ افزودن آدرس جدید
```

کاربر فقط یک آدرس را انتخاب می‌کند.

---

# حالت دوم: کاربر هیچ آدرسی ندارد

نمایش:

```
آدرسی ثبت نشده است

[ افزودن آدرس جدید ]
```

بعد از ثبت آدرس:

API مجدداً صدا زده شود:

```
GET /api-v1/Address/GetMyAddresses
```

---

# افزودن آدرس جدید

API:

```
POST /api-v1/Address/Create
```

Body:

```json
{
  "title": "خانه",
  "province": "تهران",
  "city": "تهران",
  "fullAddress": "آدرس کامل",
  "postalCode": "1234567890"
}
```

بعد از موفقیت:

* Modal بسته شود
* لیست آدرس‌ها Refresh شود
* آدرس جدید انتخاب شود

---

# ثبت سفارش

در مرحله آخر Checkout دیگر اطلاعات زیر ارسال نشود:

❌ receiverFullName
❌ receiverPhoneNumber
❌ shippingAddress

فقط:

```
POST /api-v1/Order/Create
```

Body:

```json
{
  "addressId": "guid",
  "items": [
    {
      "productId": "guid",
      "quantity": 2
    }
  ]
}
```

---

# قوانین UI

## حذف شود:

* Input نام گیرنده
* Input شماره موبایل
* Input آدرس دستی در Checkout

## اضافه شود:

* لیست آدرس‌های ذخیره شده
* انتخاب آدرس
* افزودن آدرس جدید

---

# مدیریت State پیشنهادی

در Checkout:

```javascript
const [addresses,setAddresses] = useState([]);

const [selectedAddressId,setSelectedAddressId] = useState(null);
```

هنگام Load صفحه:

```javascript
loadAddresses();
```

پس از انتخاب:

```javascript
setSelectedAddressId(address.id);
```

هنگام ثبت سفارش:

```javascript
{
 addressId:selectedAddressId,
 items:cartItems
}
```

---

# Validation فرانت

قبل از ثبت سفارش:

بررسی شود:

* حداقل یک آدرس انتخاب شده باشد
* سبد خرید خالی نباشد

پیغام:

```
لطفاً آدرس ارسال را انتخاب کنید.
```

---

# نکته مهم

اطلاعات:

* نام کاربر
* شماره موبایل

از Profile کاربر خوانده می‌شود و در Checkout قابل ویرایش نیست.

ویرایش اطلاعات شخصی فقط از بخش پروفایل انجام شود.

---

# نتیجه نهایی Flow

```
سبد خرید
    ↓
Checkout
    ↓
دریافت آدرس‌ها
    ↓
انتخاب آدرس یا افزودن آدرس
    ↓
ثبت سفارش
    ↓
پرداخت
```

این ساختار باعث می‌شود خریدهای بعدی کاربر فقط با چند کلیک انجام شود.
