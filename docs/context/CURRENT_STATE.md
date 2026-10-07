# Estado atual do código

Este retrato descreve o conteúdo desta branch, não a sessão ou o ownership de issues. A integração em main é conferida no GitHub.

## Disponível

- Harness: contrato compartilhado, protocolos de backlog/turnos/review, adaptadores e skills Codex/Claude, templates e contexto persistente.
- Fundação arquitetural recuperada até a issue #14, em sequência #8–#14.
- Módulos presentes: Usuarios.
- Camadas .NET 10: Domain, Application, Infrastructure, WebApi e Worker; testes por camada e referências protegidas por testes arquiteturais.
- Identidade Guid gerada pelo domínio, ports de Repository/UnitOfWork e base CRUD independente de EF.
- Mapster registrado por feature na Application.
- Persistência EF Core/Npgsql, health checks e migrations explícitas; integração usa schemas isolados em PostgreSQL descartável.
- ProblemDetails seguro para erros conhecidos e inesperados, com traceId.
- CI: restore, build Release e testes, com PostgreSQL descartável.
- Identidade externa vinculada ao IdUsuario na Infrastructure; tokens opacos provisionados externamente.

## Limites

Frontend, autenticação social, billing e jobs de negócio não fazem parte desta entrega. Credenciais e connection strings são configuração externa. Nenhum startup aplica migrations automaticamente. PRs aguardam revisão e integração humana; epics não são implementadas diretamente.
