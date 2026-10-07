# Rastreabilidade das entregas

Cada issue executável possui branch e PR próprias. Epics agregam as issues e só são concluídas após sua integração. A existência de código local não representa entrega concluída.

| Issue | Escopo | PR |
| --- | --- | --- |
| #15 | chore: portar Agent Harness do Dante para o Cognexa | [#16](https://github.com/juanverass/cognexa-backend/pull/16) |
| #8 | chore: criar solution e camadas da arquitetura hexagonal | [#23](https://github.com/juanverass/cognexa-backend/pull/23) |
| #9 | feat: criar padrões compartilhados de entidades, repositories e AppServices | [#24](https://github.com/juanverass/cognexa-backend/pull/24) |
| #10 | feat: configurar Mapster e convenções de mapeamento | [#25](https://github.com/juanverass/cognexa-backend/pull/25) |
| #11 | feat: configurar EF Core, PostgreSQL, UnitOfWork e persistência base | [#26](https://github.com/juanverass/cognexa-backend/pull/26) |
| #12 | feat: padronizar erros HTTP com ProblemDetails | [#27](https://github.com/juanverass/cognexa-backend/pull/27) |
| #13 | chore: documentar arquitetura, convenções e fluxo de desenvolvimento | [#28](https://github.com/juanverass/cognexa-backend/pull/28) |
| #14 | ci: configurar pipeline de build e testes do backend | [#29](https://github.com/juanverass/cognexa-backend/pull/29) |
| #17 | feat: implementar identidade, conta e preferências | [#30](https://github.com/juanverass/cognexa-backend/pull/30) |
| #18 | feat: implementar biblioteca, capítulos e acompanhamento de leitura | [#31](https://github.com/juanverass/cognexa-backend/pull/31) |
| #19 | feat: implementar anotações vinculadas à biblioteca | [#32](https://github.com/juanverass/cognexa-backend/pull/32) |
| #20 | feat: implementar aprendizados, conceitos e conexões | [#33](https://github.com/juanverass/cognexa-backend/pull/33) |
| #21 | feat: implementar revisão ativa e histórico | [#34](https://github.com/juanverass/cognexa-backend/pull/34) |

Inteligência é entregue pela issue filha da epic #7, nesta PR. PRs são empilhadas na ordem das dependências; o merge e o retarget para main são feitos durante integração humana.
