# Cognexa Backend

Backend .NET 10 para biblioteca, anotações, conhecimento, revisão ativa e inteligência. Arquitetura hexagonal com PostgreSQL como armazenamento canônico.

## Executar

Requisitos: SDK .NET 10 e PostgreSQL 17 ou superior. O SDK é selecionado por `global.json`.

```bash
dotnet tool restore
dotnet restore
dotnet build
export ConnectionStrings__Cognexa='Host=localhost;Database=cognexa;Username=cognexa;Password=SUA_SENHA'
dotnet ef database update --project src/Cognexa.Infrastructure
```

A autenticação inicial aceita tokens opacos provisionados na configuração externa. Cada token identifica uma identidade externa. O cadastro gera o `IdUsuario` no domínio e persiste seu vínculo na Infrastructure. O domínio não armazena credenciais. Configure um UUID e um token aleatório de pelo menos 32 caracteres por usuário; não versionar tokens nem connection strings.

```bash
export Autenticacao__Identidades__0__IdIdentidade='UUID_DA_IDENTIDADE_EXTERNA'
export Autenticacao__Identidades__0__Token='TOKEN_ALEATORIO_COM_PELO_MENOS_32_CARACTERES'
dotnet run --project src/Cognexa.WebApi --urls http://localhost:5080
```

O tráfego em produção deve usar HTTPS na borda. O adapter pode ser substituído por autenticação externa que forneça a claim de identificação. Sem tokens configurados, os endpoints de dados permanecem inacessíveis. O cadastro `POST /usuarios/me` cria a conta para a identidade autenticada. Desativar uma conta preserva seus dados e impede os demais casos de uso HTTP.

`dotnet run --project src/Cognexa.Worker` inicia o host de processamento. Nesta entrega não há jobs de negócio; novos jobs precisam definir um contexto de usuário próprio.

## Verificar

```bash
dotnet build --configuration Release
dotnet test --configuration Release
```

Os testes de integração ficam explicitamente ignorados quando não há banco configurado. Para executar a suíte completa contra um banco descartável:

```bash
export COGNEXA_TEST_CONNECTION='Host=localhost;Database=cognexa_tests;Username=cognexa;Password=SENHA_DE_TESTE'
dotnet test --configuration Release
```

Cada fixture cria e remove apenas um schema aleatório próprio. O usuário do banco de testes precisa poder criar schemas. O CI fornece PostgreSQL descartável e executa testes de domínio, aplicação, infraestrutura, arquitetura e HTTP sem secrets externos.

## Documentação

- [Arquitetura e convenções](docs/adr/0001-arquitetura.md)
- [Contratos HTTP e exemplos](docs/api.md)
- [Providers de inteligência](docs/inteligencia.md)
- [Política de conteúdo protegido e OCR](docs/conteudo-protegido.md)
- [Gate jurídico de publicação social](docs/gate-juridico-social.md)
- [Rastreabilidade das issues](docs/issues.md)

`/health/live` verifica o host e `/health/ready` verifica PostgreSQL. Migrations são aplicadas explicitamente; o startup não altera o banco.

- [Contrato de agentes](docs/development/agent-contract.md)
- [Estado do código](docs/context/CURRENT_STATE.md)
