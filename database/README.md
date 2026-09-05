# Banco FrontLine (PB 3.122 BR)

PostgreSQL com schema + dados + migrations do servidor.

---

## 1. Criar o banco

```bash
createdb -U postgres pb
```

---

## 2. Importar tudo

Um arquivo só (schema + dados + migrations):

```bash
psql -U postgres -d pb -f pb_3122_full.sql
```

A pasta `migrations/` é referência. **Não** rode migration por migration depois do dump completo.

---

## 3. Conta de teste

O client autentica por **token** (não por senha no Auth):

```bash
psql -U postgres -d pb -c "INSERT INTO accounts (username, password, token) VALUES ('player1', 'player1', 'AAECAwQFBgcICQoLDA0OMg==');"
```

Subir o client:

```text
PointBlank.exe /token AAECAwQFBgcICQoLDA0OMg== /launcher PBLauncher
```

(ou o executável FrontLine equivalente)

Mais contas: mesmo `INSERT`, outro `username` + `token`.

GM:

```sql
UPDATE accounts SET access_level = 6 WHERE username = 'player1';
```

---

## 4. Ligar o servidor ao DB

Em `Config/Settings.ini` (ou variáveis `PB_DB_*`):

| Campo | Padrão local típico |
|-------|---------------------|
| Host | `localhost` |
| Port | `5433` (ou `5432`) |
| Name | `pb` |
| User / Pass | `postgres` / `postgres` |

---

## Notas

- Contas do dump são sintéticas — ignore ou apague em produção  
- Nunca exponha PostgreSQL na internet  
- Migrations de segurança novas (OTP, heartbeat, etc.) entram após o dump conforme os `.sql` da pasta  
