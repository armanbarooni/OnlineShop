# Environment Variables

This document lists the environment variables required to run OnlineShop across development, staging, and production environments. The .env.example file contains starter values for local development. All secrets must be overridden with secure values before deploying to production.

## Backend (.NET) Variables

| Key | Description | Example |
| --- | --- | --- |
| ASPNETCORE_ENVIRONMENT | Runtime environment (Development, Staging, Production). | Production |
| CONNECTIONSTRINGS__DEFAULTCONNECTION | PostgreSQL connection string used by Entity Framework Core. | Host=db;Port=5432;Database=OnlineShop;Username=shop;Password=super-secret;Include Error Detail=false |
| JWT__ISSUER | JWT token issuer. | OnlineShop |
| JWT__AUDIENCE | JWT token audience. | OnlineShopClient |
| JWT__SECRET | Symmetric signing key for JWT tokens. Must be at least 32 characters. | P@ssw0rd-Change-Me-To-A-Longer-Secret-Value! |
| SMSSETTINGS__PROVIDER | Active SMS provider identifier (Mock, SmsIr, etc.). | SmsIr |
| SMSSETTINGS__APIKEY | API key for the selected SMS provider. | xxxxxxxxxxxxxxxxxxxxxxxx |
| SMSSETTINGS__SENDER | Sender phone number or originator. | 10004346 |
| SMSSETTINGS__OTPTEMPLATE | Template name/ID for OTP messages. | OnlineShopOTP |
| SMSSETTINGS__OTPEXPIRATIONMINUTES | Expiration window for OTP codes. | 5 |
| SMSSETTINGS__OTPLENGTH | Length of OTP codes. | 6 |
| SMSIR__APIKEY | Sms.ir API key when SmsIr provider is used. | xxxxxxxxxxxxxxxxxxxxxxxx |
| SMSIR__TEMPLATEID | Sms.ir verification template ID. | 325822 |
| SMSIR__USESANDBOX | Enable Sms.ir sandbox mode (	rue / alse). | alse |
| SMSIR__OTPPARAMNAME | Sms.ir parameter name for OTP code. | Code |
| CORS__ALLOWFRONTEND__{index} | Allowed frontend origin(s) for CORS policy. Provide each origin with an incremental index. | https://app.example.com |

## Front-end Overrides (Optional)

| Key | Description | Example |
| --- | --- | --- |
| __API_BASE_URL__ | Set on window before scripts load to override API base URL detection. | https://api.example.com/api |

## Management Guidelines

- Store production secrets in the hosting provider's secure configuration store (Azure App Settings, AWS Parameter Store, Kubernetes secrets, etc.).
- Never commit .env files or plain-text secrets to version control.
- Regenerate credentials immediately if they become exposed.
- Review ppsettings.Production.json prior to deployment to verify no sensitive defaults remain.
