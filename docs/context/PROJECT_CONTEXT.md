# Contexto do projeto

Cognexa é um backend .NET 10 independente do frontend. Biblioteca e anotações sustentam conhecimento rastreável; revisão ativa e inteligência enriquecem esse modelo.

## Arquitetura alvo

- Domain: invariantes, entidades e identidade, sem SDKs/providers.
- Application: casos de uso, DTOs, mappings e ports.
- Infrastructure: EF Core/PostgreSQL, repositories e providers substituíveis.
- WebApi e Worker: composition roots, sem regras de negócio.

Referências: Application → Domain; Infrastructure → Application + Domain; WebApi/Worker → Application + Infrastructure.

## Stack e convenções

.NET 10, ASP.NET Core, EF Core 10, Npgsql/PostgreSQL, Mapster e xUnit. Entidades persistentes usam `Guid Id` gerado pelo domínio via `EntidadeBase`; FKs usam `IdEntidade`, sem `TId`. Domain/Application usam PT-BR; sufixos técnicos estabelecidos permanecem.

## Segurança e limites

Consultas e vínculos pertencentes ao usuário exigem isolamento por IdUsuario. Domain não conhece autenticação, EF, providers de IA, embeddings ou SDKs. Conteúdo original e conteúdo gerado permanecem distinguíveis. Sugestões não são aplicadas automaticamente. Segredos ficam na configuração externa.

Não há frontend, autenticação social, billing ou infraestrutura distribuída neste escopo. Banco canônico: PostgreSQL. Migrations só a partir de entidades reais e aplicação administrativa explícita.

## Fontes persistentes

[Estado atual](CURRENT_STATE.md), [decisões](ARCHITECTURE_DECISIONS.md), [histórico](DEVELOPMENT_HISTORY.md) e [contrato de trabalho](../development/agent-contract.md). Código/testes prevalecem se houver divergência; corrigir o documento afetado. Roadmap, ownership e PRs vivem no GitHub.
