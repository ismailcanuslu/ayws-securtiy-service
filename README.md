<div align="center">

# 🔐 ayws-security-service

**AYWS Platform** için kimlik, erişim ve sır yönetimi servisi · Keycloak üzerine inşa edilmiştir

[![.NET](https://img.shields.io/badge/.NET-9.0-512BD4?style=flat-square&logo=dotnet)](https://dotnet.microsoft.com)
[![Keycloak](https://img.shields.io/badge/Keycloak-26-4D9A9A?style=flat-square&logo=keycloak)](https://keycloak.org)
[![EF Core](https://img.shields.io/badge/EF_Core-9.0-512BD4?style=flat-square&logo=dotnet)](https://learn.microsoft.com/ef/core)
[![License](https://img.shields.io/badge/License-MIT-blue?style=flat-square)](LICENSE)

</div>

---

## 💡 Bu Servis Ne Yapar?

> Security service **kimlik ve erişim yönetiminin CRUD katmanıdır.**
> Runtime'da JWT doğrulamaz — bu api-gateway'in işidir.

```
❌ YAPMADIĞI              ✅ YAPTIĞI
─────────────────────     ──────────────────────────────
JWT doğrulama             Tenant / Kullanıcı / Rol CRUD
Rate limiting             API Key yaşam döngüsü
Network firewall          MFA yönetimi
Audit log stream          OAuth2 Client yönetimi
Billing / quota           Secret & Sertifika saklama
                          Internal CheckPermission
```

---

## 🏗️ Mimari

```
                    ┌─────────────────────────────┐
  Diğer Servisler   │   ayws-security-service      │
  ───────────────►  │                              │
  CheckPermission   │  Api  →  Application         │
                    │           │                  │
  api-gateway       │           ▼                  │   ┌────────────┐
  ───────────────►  │       Infrastructure    ────────► Keycloak    │
  ValidateApiKey    │           │                  │   │ Admin API  │
                    │           ▼                  │   └────────────┘
                    │       PostgreSQL              │
                    │  (API Key hash, Secret,       │
                    │   Cert, Tenant mapping)       │
                    └─────────────────────────────-┘
                                │
                                ▼ Events
                    messaging-service
                    (UserCreated, TenantSuspended…)
```

---

## 📦 Proje Yapısı

```
ayws-security-service/
├── src/
│   ├── Ayws.Security.Service.Api            ← Controller'lar, Middleware
│   ├── Ayws.Security.Service.Application    ← Use case'ler, Validator'lar
│   ├── Ayws.Security.Service.Domain         ← Entity'ler, Interface'ler
│   ├── Ayws.Security.Service.Infrastructure ← Keycloak client, DB, Vault
│   └── Ayws.Security.Service.Contracts      ← DTO & Event'ler (diğer servisler kullanır)
└── tests/
    ├── Ayws.Security.Service.UnitTests
    └── Ayws.Security.Service.IntegrationTests
```

---

## 📡 API Grupları

### 🏢 Tenant Yönetimi
| Method | Endpoint | Açıklama |
|---|---|---|
| `POST` | `/tenants` | Keycloak'ta realm aç |
| `GET` | `/tenants` | Tüm tenant'ları listele |
| `GET` | `/tenants/{id}` | Tenant detayı |
| `DELETE` | `/tenants/{id}` | Realm + tüm verileri sil |
| `POST` | `/tenants/{id}/suspend` | Realm'ı devre dışı bırak |
| `POST` | `/tenants/{id}/reactivate` | Realm'ı tekrar aktif et |
| `PUT` | `/tenants/{id}/settings` | Login policy, session süresi |

### 👤 Kullanıcı Yönetimi
| Method | Endpoint | Açıklama |
|---|---|---|
| `POST` | `/tenants/{id}/users/invite` | Davet maili + Keycloak user |
| `GET` | `/tenants/{id}/users` | Kullanıcı listesi |
| `DELETE` | `/tenants/{id}/users/{uid}` | Keycloak'tan sil |
| `POST` | `/tenants/{id}/users/{uid}/disable` | Soft block |
| `POST` | `/tenants/{id}/users/{uid}/force-password-reset` | Required action ekle |
| `GET` | `/tenants/{id}/users/{uid}/sessions` | Aktif session'lar |
| `DELETE` | `/tenants/{id}/users/{uid}/sessions` | Tüm session'ları sonlandır |

### 🛡️ Rol Yönetimi
| Method | Endpoint | Açıklama |
|---|---|---|
| `POST` | `/tenants/{id}/roles` | Rol oluştur |
| `PUT` | `/tenants/{id}/roles/{rid}` | Rol güncelle |
| `DELETE` | `/tenants/{id}/roles/{rid}` | Rol sil |
| `POST` | `/tenants/{id}/users/{uid}/roles` | Kullanıcıya rol ata |
| `DELETE` | `/tenants/{id}/users/{uid}/roles/{rid}` | Rolü kaldır |

> Her tenant oluştuğunda otomatik roller: `owner` · `admin` · `developer` · `billing` · `readonly`

### 🔑 API Key Yönetimi
| Method | Endpoint | Açıklama |
|---|---|---|
| `POST` | `/tenants/{id}/api-keys` | Key oluştur (scope + TTL) |
| `DELETE` | `/tenants/{id}/api-keys/{kid}` | İptal et |
| `POST` | `/tenants/{id}/api-keys/{kid}/rotate` | Yeni key, 24s geçiş süresi |
| `GET` | `/tenants/{id}/api-keys` | Listeleme (değer görünmez) |
| `POST` | `/api-keys/validate` | **Internal** · api-gateway çağırır |

### 🔒 MFA
| Method | Endpoint | Açıklama |
|---|---|---|
| `POST` | `/tenants/{id}/users/{uid}/mfa/enable` | TOTP secret + QR döndür |
| `DELETE` | `/tenants/{id}/users/{uid}/mfa` | MFA kaldır |
| `POST` | `/tenants/{id}/users/{uid}/mfa/recovery-codes` | Recovery code üret |
| `PUT` | `/tenants/{id}/mfa/require` | Tenant geneli zorunlu MFA |

### 🌐 OAuth2 / OIDC Client
| Method | Endpoint | Açıklama |
|---|---|---|
| `POST` | `/tenants/{id}/oauth-clients` | Client oluştur |
| `DELETE` | `/tenants/{id}/oauth-clients/{cid}` | Client sil |
| `POST` | `/tenants/{id}/oauth-clients/{cid}/rotate-secret` | Secret yenile |
| `GET` | `/tenants/{id}/oauth-clients/{cid}/credentials` | client_id + secret |

### 🗝️ Secret & Sertifika
| Method | Endpoint | Açıklama |
|---|---|---|
| `POST` | `/tenants/{id}/secrets` | Şifreli sakla |
| `GET` | `/tenants/{id}/secrets/{key}` | Çözülmüş değer döner |
| `POST` | `/tenants/{id}/secrets/{key}/rotate` | Yeni değerle güncelle |
| `POST` | `/tenants/{id}/certificates` | Let's Encrypt / self-signed |
| `GET` | `/tenants/{id}/certificates/{cid}/expiry` | Son kullanma tarihi |

### 🔎 Permission Check (Internal Only)
```
POST /internal/check-permission
{ tenantId, userId, resource, action }
→ Sadece iç network'ten erişilebilir, dışarıya açık değil
```

---

## ⚙️ Konfigürasyon

```json
{
  "KeycloakSettings": {
    "BaseUrl": "https://keycloak.internal",
    "AdminClientId": "ayws-admin",
    "AdminClientSecret": "...",
    "MasterRealm": "master"
  },
  "ConnectionStrings": {
    "Postgres": "Host=...;Database=ayws_security"
  },
  "EncryptionSettings": {
    "SecretEncryptionKey": "..."
  }
}
```

> 🔒 `AdminClientSecret` ve `SecretEncryptionKey` production'da **Vault** veya **K8s Secret** ile yönetilmelidir.

---

## 🚀 Kurulum

```bash
dotnet restore
dotnet ef database update --project src/Ayws.Security.Service.Infrastructure
dotnet run --project src/Ayws.Security.Service.Api
```

---

## 🏷️ İsimlendirme Kuralları

| Ortam | Format | Örnek |
|---|---|---|
| GitHub repo / Docker image / K8s | `kebab-case` | `ayws-security-service` |
| .NET namespace / assembly | `PascalCase` | `Ayws.Security.Service.Api` |
| Environment variable | `SCREAMING_SNAKE` | `AYWS_SECURITY_SERVICE_PORT` |

---

## 🤝 Katkı

> Mimari rehber için → [`CLAUDE.md`](./CLAUDE.md)

1. `feature/security-*` branch'i aç
2. Değişikliği yap, validator yaz
3. PR aç → en az 1 approval

---

<div align="center">
  <sub>Built with ❤️ · AYWS Platform · 2026</sub>
</div>
