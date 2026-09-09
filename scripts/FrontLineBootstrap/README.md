# FrontLine Bootstrap (Store)

EXE leve (~68 MB) que a Microsoft Store pode apontar. Ele:

1. Descarrega `FrontLine-Setup-latest.zip` da R2  
2. Extrai `FrontLine-Setup-latest.exe` + `.bin`  
3. Corre o Inno Setup (com os mesmos argumentos da Store)

## URL pública

https://downloads.frontlinebattle.com.br/FrontLine-Setup-Store.exe

## Partner Center

| Campo | Valor |
|---|---|
| URL do pacote | `https://downloads.frontlinebattle.com.br/FrontLine-Setup-Store.exe` |
| Arquitetura | x64 |
| Tipo | EXE |
| Parâmetros | `/VERYSILENT /SUPPRESSMSGBOXES /NORESTART /SP-` |
| Códigos | iguais ao Inno: sucesso `0`, cancelar `5`, falha install `4`, rede/init `1` |

## Build local

```powershell
cd scripts\FrontLineBootstrap
dotnet publish -c Release -r win-x64 --self-contained true `
  -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true `
  -o ..\..\dist\bootstrap
```

Depois sobe o exe com o mesmo fluxo R2 (`aws s3 cp` / script de upload).
