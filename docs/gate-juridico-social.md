# Gate jurídico de lançamento social (#42)

Status: **bloqueado — revisão especializada não registrada**.

Dependem deste gate: publicação de anotações/citações, consulta pública e futura distribuição em feeds, buscas públicas, compartilhamento ou exportação social. Preparar elegibilidade privada não abre acesso público.

O backend requer simultaneamente `Conteudo:SocialHabilitado=true` e `Conteudo:RegistroRevisaoJuridica` não vazio. Ausência de qualquer requisito impede publicação e retorna 404 em consultas públicas. A referência deve apontar para o parecer aprovado e checklist assinado; não colocar documentos jurídicos pessoais nem conteúdo protegido em configuração. O mecanismo verifica a presença da referência, não autentica ou emite pareceres. Operadores são responsáveis por registrar uma revisão real.

Antes de ativar, registrar revisor qualificado, jurisdições, data, referência do parecer, escopo aprovado, responsável de produto e condições/restrições. Reavaliar em mudanças materiais de escopo. Não há revisão jurídica concluída por esta implementação.

Checklist obrigatório:

- [ ] Armazenamento privado de pequenos trechos e política de retenção avaliados.
- [ ] Publicação de citações, propósito/contexto e limites por obra avaliados.
- [ ] Volume acumulado, duplicação de edições e atribuição necessária avaliados.
- [ ] OCR, contratos de providers, descarte e tratamento de imagens avaliados.
- [ ] Notice-and-takedown, verificação de titulares/representantes, prazos e recurso definidos.
- [ ] Obras em domínio público e permissões/licenças avaliadas.
- [ ] Responsabilidade por conteúdo dos usuários, termos e canal de atendimento definidos.
- [ ] Auditoria mínima e retenção de denúncias/histórico aprovadas.
- [ ] Moderação operacional e controles de acesso revisados.
- [ ] Parecer especializado registrado e lançamento aprovado por responsável identificado.

Riscos conhecidos: reconstrução por múltiplos livros/contas, atribuição declarada incorreta, permissões não verificadas automaticamente, jurisdições diferentes e redistribuição após download. Limites técnicos reduzem abuso, mas não substituem análise jurídica. Esta documentação registra decisões de engenharia e perguntas para revisão; não fornece parecer jurídico.
