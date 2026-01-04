# 💳 ZarinPal Payment Gateway Implementation

## 📋 API Endpoints

### 1. Payment Request (Initiate)
**URL:** `https://payment.zarinpal.com/pg/v4/payment/request.json`
**Method:** POST
**Content-Type:** application/json

**Request Body:**
```json
{
  "merchant_id": "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx",
  "amount": 10000,  // به ریال
  "currency": "IRT",  // IRT = تومان, IRR = ریال
  "callback_url": "https://yoursite.com/payment/verify",
  "description": "توضیحات تراکنش",
  "metadata": {
    "mobile": "09121234567",
    "email": "user@example.com",
    "order_id": "ORDER-123"
  }
}
```

**Response (Success):**
```json
{
  "data": {
    "code": 100,
    "message": "Success",
    "authority": "A0000000000000000000000000000wwOGYpd",
    "fee_type": "Merchant",
    "fee": 100
  },
  "errors": []
}
```

**Redirect User To:**
```
https://payment.zarinpal.com/pg/StartPay/{authority}
```

---

### 2. Payment Verification
**URL:** `https://payment.zarinpal.com/pg/v4/payment/verify.json`
**Method:** POST

**Request Body:**
```json
{
  "merchant_id": "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx",
  "amount": 10000,  // باید دقیقاً همان مبلغ request باشد
  "authority": "A0000000000000000000000000000wwOGYpd"
}
```

**Response (Success):**
```json
{
  "data": {
    "code": 100,  // 100 = موفق, 101 = قبلاً verify شده
    "message": "Verified",
    "ref_id": 201,  // شماره تراکنش
    "card_hash": "1EBE3EBEBE35C7EC0F8D6EE4F2F859107A87822CA179BC9528767EA7B5489B69",
    "card_pan": "502229******5995",
    "fee_type": "Merchant",
    "fee": 0
  },
  "errors": []
}
```

---

### 3. Callback from ZarinPal
پس از پرداخت، کاربر به `callback_url` هدایت می‌شود:

**Success:**
```
https://yoursite.com/payment/verify?Authority=A00000...&Status=OK
```

**Failed:**
```
https://yoursite.com/payment/verify?Authority=A00000...&Status=NOK
```

---

## 🧪 Sandbox (Test Environment)

**URL Changes:**
- Production: `https://payment.zarinpal.com/...`
- Sandbox: `https://sandbox.zarinpal.com/...`

**Sandbox Notes:**
- Authority شروع می‌شود با `S` (مثلاً `S000000...`)
- merchant_id می‌تواند هر UUID دلخواه باشد
- پرداخت واقعی انجام نمی‌شود

---

## ⚠️ نکات مهم

1. **Code 100 vs 101:**
   - `100`: تراکنش موفق و اولین بار verify می‌شود
   - `101`: تراکنش موفق اما قبلاً verify شده

2. **Amount:**
   - باید در request و verify یکسان باشد
   - واحد: ریال (IRR) یا تومان (IRT)

3. **Status Query Parameter:**
   - فقط در صورت `Status=OK` باید verify کنیم
   - `Status=NOK` = کاربر لغو کرده یا خطا رخ داده

4. **Auto Verify:**
   - می‌توان در metadata با `auto_verify: true/false` کنترل کرد
   - اگر false باشد، باید خودمان verify کنیم

---

## 🔐 Error Codes

| Code | Description |
|------|-------------|
| -9 | خطای اعتبارسنجی |
| -10 | IP یا merchant_id نامعتبر |
| -11 | merchant_id غیرفعال |
| -14 | callback_url با دامنه ثبت شده مغایرت دارد |
| -41 | حداکثر مبلغ 100 میلیون تومان |
| -50 | مبلغ verify با request متفاوت است |
| -51 | پرداخت ناموفق |
| -54 | Authority نامعتبر |
| 100 | موفق |
| 101 | قبلاً verify شده |

---

## 📝 Implementation Checklist

- [ ] Create `ZarinPalPaymentService` implementing `IPaymentGatewayService`
- [ ] Add `MerchantId` to appsettings.json
- [ ] Add `IsSandbox` flag to appsettings.json
- [ ] Implement `InitiatePaymentAsync`
- [ ] Implement `VerifyPaymentAsync`
- [ ] Handle error codes properly
- [ ] Add logging
- [ ] Register service in DI
