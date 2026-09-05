# PRISMBLEED

Servidor para o client Point Blank 122 BR. Auth, Game e Match compartilham código .NET 8 e podem rodar no Windows ou Linux. No Windows use o monitor WinForms `FLMonitor.exe`; no Linux, use o host console `PRISMBLEED.Server`.

## Requisitos

- Windows local: .NET 8 SDK.
- Linux: WSL Ubuntu 22.04 e Docker instalado dentro do WSL. Todos os scripts Docker deste repositório entram pelo WSL.
- PostgreSQL externo com o schema e migrations existentes do projeto.
- Diretórios de runtime `Config`, `Data` e `Logs`. Eles não são embutidos na imagem Docker.

## Windows

Execute `build-debug.bat`. O script publica:

- monitor WinForms em `binary\Debug`;
- host console compartilhado em `binary\Debug\console`.

Para o fluxo normal no Windows, abra `binary\Debug\FLMonitor.exe` (console + painel). O host só-console (sem WinForms) fica em:

```powershell
binary\Debug\console\PRISMBLEED.Server.exe --content-root binary\Debug
```

Use o PRISMBLEED console quando quiser logs sem a janela do monitor (ex.: Linux/Docker ou debug headless). No dia a dia Windows, prefira o **FLMonitor**.

## Linux e Docker via WSL

O build da imagem é sempre executado dentro do WSL:

```powershell
.\scripts\build-linux-wsl.ps1
.\scripts\test-linux-wsl.ps1
```

O Dockerfile publica apenas os projetos gerenciados e não inclui `CryptoLib`, credenciais, `Config`, `Data` ou `Logs`.

Para Compose, copie `.env.example` para `.env`, preencha apenas localmente e prepare os diretórios de runtime:

```text
runtime/
  Config/
  Data/
  Logs/
```

No ambiente local já configurado, o Compose usa diretamente a rede Docker `bridge` existente e acessa o PostgreSQL publicado no host em `5433`; não é necessário criar outra network. Depois, execute:

```powershell
.\scripts\compose-up-wsl.ps1
```

Para encerrar e remover somente o container do servidor:

```powershell
.\scripts\compose-down-wsl.ps1
```

As portas publicadas por padrão são Auth TCP `39190`, os canais Game TCP `39191`–`39193`, Match UDP `40009` e RCON TCP `30000`. Em produção **não publique RCON** na internet — mantenha `RconIp=127.0.0.1` e veja o guia [`linux/SECURITY.md`](linux/SECURITY.md) + script [`linux/harden-firewall.sh`](linux/harden-firewall.sh). As portas do host podem ser isoladas com `PB_AUTH_PORT`, `PB_GAME_PORT_1`–`3`, `PB_MATCH_PORT` e `PB_RCON_PORT` no `.env`.

## Configuração

O host resolve caminhos relativos a partir de `PB_CONTENT_ROOT` ou de `--content-root`. Assim, os caminhos existentes em `Config`, `Data` e `Logs` continuam válidos nos dois sistemas.

As credenciais do banco podem vir do INI existente ou, preferencialmente em deploy, das variáveis:

- `PB_DB_HOST`
- `PB_DB_PORT`
- `PB_DB_NAME`
- `PB_DB_USER`
- `PB_DB_PASS`

Em container, `PB_BIND_HOST=0.0.0.0` substitui apenas o endereço local de bind para tornar as portas publicáveis. `PB_ADVERTISE_HOST` define o IP devolvido ao client nos canais Game e no Match; localmente ele está configurado como `127.0.0.1` para o client Windows. O Compose também fixa o bind interno do RCON com `PB_RCON_BIND_HOST` e `PB_RCON_BIND_PORT`, mantendo `PB_RCON_PORT` apenas como porta publicada no host.

Variáveis de ambiente têm precedência e não devem ser commitadas. O arquivo `.env` está ignorado pelo Git.

## Publicação linux-x64 e systemd

Gere um executável Linux self-contained, também via Docker no WSL:

```powershell
.\scripts\publish-linux-wsl.ps1
```

O resultado fica em `artifacts\linux-x64`. Copie seu conteúdo para `/opt/prismbleed`, coloque os diretórios `Config`, `Data` e `Logs` no mesmo local, instale `deploy/prismbleed.service` em `/etc/systemd/system/` e salve as variáveis `PB_DB_*` em `/etc/prismbleed/server.env` com permissões restritas.

## Encerramento

O host console trata Ctrl+C, SIGINT e SIGTERM, fecha os sockets de Auth/Game/Match e encerra filhos quando `ProcessSplit` estiver ativo. O Compose e a unidade systemd usam SIGTERM com janela de 20 segundos.

## TestBot

`Server.TestBot` continua sendo o cliente headless para validar login, opcodes e handlers ponta a ponta. Ele exige um runtime completo e uma configuração de banco válida antes da execução.
