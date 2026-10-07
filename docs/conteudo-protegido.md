# Política técnica de conteúdo e proveniência (#36–#42)

O Cognexa armazena conhecimento sobre a obra. Não armazena páginas digitalizadas, capítulos ou livros completos. `TrechoOriginal` é uma citação de terceiro; `Comentario` é a interpretação do usuário. `OrigemDoComentario` identifica assistência de IA. `MetodoDeCaptura` registra entrada manual, OCR ou importação autorizada (a autorização deve ser obtida pelo usuário; o enum não concede licença). Livro, página/localização, obra e autor acompanham a anotação e são serializados no DTO. Notas existentes continuam privadas e com captura manual.

Citações permanecem enquanto a anotação privada existir; seu proprietário pode excluí-las. Não há arquivo de OCR completo nem imagem no banco. A retenção de citações não implica autorização de redistribuição. Telemetria não deve registrar corpos de requisição/resposta, OCR, citações, imagens nem tokens. A integração OCR desativa os loggers HTTP e retorna erros sem o corpo do provider. Não habilitar logging de dados sensíveis do EF ou captura de corpos em proxies/APM.

## OCR transitório

`POST /anotacoes/ocr` recebe corpo binário PNG/JPEG, Content-Type correspondente e Content-Length obrigatório, até 5 MB. O port `IOcrProvider` usa o gateway HTTPS `Ocr:BaseUrl`, com token externo `Ocr:Token`; `POST extrair` recebe os bytes e responde `{"texto":"..."}`. Timeout: 30 segundos. Limite da resposta: 500 KB / 100 mil caracteres. A integração externa deve contratar retenção zero e ausência de treinamento; não ativar o provider sem verificar essas condições.

A imagem é apagada do buffer em finally, inclusive em falhas. O texto fica apenas em memória por 120 segundos, no máximo três capturas por usuário e cem no processo. A limpeza roda a cada dez segundos; acesso após expiração é negado imediatamente. Strings gerenciadas são liberadas para coleta, sem garantia de apagamento físico de memória. Não há cache distribuído, disco, blob ou tabela de imagens. Reiniciar o processo perde capturas; em múltiplas instâncias usar afinidade durante a seleção.

`POST /anotacoes/ocr/confirmar` recebe `idCaptura`, `inicio` (índice UTF-16, base zero), `comprimento`, `idLivro`, `pagina`, `localizacao` e `comentario`. A captura só pode ser consumida pelo usuário autenticado que a criou. A confirmação consome a captura uma única vez antes de validar/persistir: falha exige nova captura. Apenas a substring selecionada chega ao domínio, com método OCR; texto restante perde a referência no armazenamento temporário.

## Controles contra reconstrução

Configuração `Conteudo`: `MaximoPorTrecho` (2000 caracteres), `MaximoAcumulado` (10000), `JanelaHoras` (24), `MaximoPaginasSequenciais` (5), `MaximoPublicoPorTrecho` (500), `MaximoPublicoPorLivro` (2000). Os valores são controles de produto, nunca limites legais universais. Página repetida ou pequenas variações não causam bloqueio isoladamente; uma sequência de cinco páginas distintas contíguas bloqueia. Localizações sem número de página continuam sujeitas ao limite acumulado.

Capturas e mudanças no texto consomem a janela; apagar uma nota não apaga o histórico de volume. O registro guarda somente IDs, contagem, página e data. Transação PostgreSQL e advisory lock por IdLivro serializam validação e escrita; falhas não consomem a quota. O escopo é o livro cadastrado; edições/cadastros duplicados não são deduplicados bibliograficamente nesta entrega. A publicação soma citações públicas e elegíveis do livro, tem limites próprios e usa o mesmo lock. Rowversion impede sobreposição silenciosa de edição/moderação.

## Publicação e moderação

Anotações nascem `Privado`. Preparar, publicar e retirar são ações explícitas do proprietário em `POST /anotacoes/{id}/preparar-publicacao`, `/publicar`, `/retirar`. Citações exigem obra, autor e página/localização para ficar `Publicavel`. Uma edição invalida a elegibilidade e exige nova validação. Comentários de IA mantêm sua origem no DTO público.

`GET /publicacoes/{id}` serve apenas conteúdo publicado, sem denúncia e com o gate aberto. `POST /publicacoes/{id}/denuncias` exige conta ativa e motivo `DireitosAutorais`, `AtribuicaoIncorreta` ou `Outro` (enum JSON numérico). A auditoria de transições preserva IDs, obra/autor, estado e horário mesmo após excluir a nota; não guarda trecho nem comentário. A denúncia registra IDs, motivo e horário, sem copiar o trecho, e oculta preventivamente o conteúdo. O proprietário pode retirar a publicação: estado `Removido` e moderação `Retirado` são terminais, mesmo após edição. A nota privada permanece disponível ao dono. Não existe endpoint que reexponha conteúdo moderado.

Representantes/titulares devem usar o canal de atendimento para identificação e verificação; o operador pode registrar a denúncia por uma conta ativa autorizada. Um fluxo administrativo de adjudicação, verificação de titular e eventual recurso precisa ser definido antes do lançamento social. O sistema não exige nem recebe documentos pessoais dentro da denúncia.

Todas as funcionalidades públicas dependem do [gate jurídico](gate-juridico-social.md). Não abrir o gate só porque os testes técnicos passam.
