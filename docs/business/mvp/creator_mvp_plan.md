# MVP Creator Do Querify

Data de referencia: 2026-05-11.

Este documento transforma os escopos completos de [`value_proposition.md`](value_proposition.md),
[`ai_product_modules_strategy.md`](ai_product_modules_strategy.md) e
[`pricing_strategy.md`](pricing_strategy.md) em um MVP mais rapido de entregar, com todos os
modulos de produto funcionando em versao minima: QnA, Direct, Broadcast e Trust.

O publico-alvo inicial e o influenciador, creator, especialista independente, infoprodutor pequeno,
educador, consultor, comunidade pequena ou micro-marca que tem audiencia, recebe perguntas repetidas
e precisa parecer mais profissional sem pagar preco de ferramenta enterprise.

## Decisao Recomendada

Criar uma oferta chamada **Querify Creator**:

> Um hub simples para transformar perguntas de seguidores em respostas reutilizaveis, responder
> DMs e comentarios com consistencia e entender quais duvidas viram conteudo, produto ou venda.

O MVP deve ter todos os modulos, mas em corte pequeno:

| Modulo | Corte MVP para creator |
| --- | --- |
| QnA | Mini base publica de perguntas e respostas, com AI draft e fonte simples. |
| Direct | Inbox privado proprio via formulario/link, com sugestao de resposta baseada no QnA. |
| Broadcast | Importacao manual/CSV de comentarios e perguntas publicas, agrupamento e resposta sugerida. |
| Trust | Aprovacao simples, historico de mudanca e registro de decisao para respostas sensiveis. |

Nao tentar entregar no MVP: automacao profunda de Instagram/TikTok/WhatsApp, inbox social completo,
webhooks de todos os canais, agente autonomo, analytics enterprise, votes sofisticados ou workflows
complexos de governanca.

## Por Que Esse MVP Faz Sentido

O mercado de creators compra ferramentas baratas e focadas:

- Link-in-bio e storefront: US$ 0 a US$ 99/mes.
- DM automation: US$ 14 a US$ 139/mes.
- Comunidade/newsletter: muitas vezes gratis ate monetizar, depois taxa de receita.
- Plataformas completas de creator commerce: normalmente US$ 29 a US$ 99/mes.

O creator com menos recursos nao compra "governanca de conhecimento". Ele compra:

- economizar tempo respondendo a mesma pergunta;
- vender mais pelo link da bio, DM ou comentario;
- parecer organizado/profissional;
- saber que conteudo postar com base nas perguntas reais;
- nao perder oportunidade porque nao respondeu;
- transformar duvidas em FAQ, lead magnet, aula, produto ou post.

Entao o produto precisa entrar pelo ganho imediato:

> "Pare de responder a mesma pergunta todo dia. Crie um hub de respostas e use IA para transformar
> DMs e comentarios em conteudo e vendas."

## Insight De Mercado

Ferramentas atuais mostram a faixa psicologica de preco do creator:

| Categoria | Exemplos | Faixa relevante |
| --- | --- | ---: |
| Link-in-bio | Linktree, Beacons, Milkshake | US$ 0 a US$ 35/mes, com tiers maiores ate US$ 90. |
| Creator storefront | Stan Store, Beacons | US$ 29 a US$ 99/mes. |
| DM/comment automation | Manychat | Free, US$ 14, US$ 29, US$ 69, US$ 139/mes conforme contatos. |
| Newsletter/membership | Substack, Patreon | Baixa barreira inicial, mas 10% ou mais de receita quando monetiza. |
| Comunidade | Circle, Discourse | Geralmente US$ 89+/mes ou hospedagem em tiers. |

Conclusao: para influenciador pequeno, o plano de entrada do Querify precisa ficar entre
**US$ 19 e US$ 49/mes**. Acima disso, ele compara com Stan, Manychat e Beacons e so compra se o
produto estiver claramente ligado a vendas ou economia de tempo.

## Posicionamento Do MVP

Nao vender como "AI support platform". Para creator isso soa grande, caro e corporativo.

Vender como:

- **FAQ inteligente para creator**.
- **Inbox de perguntas repetidas**.
- **DM/comment answer hub**.
- **Content ideas from audience questions**.
- **Perguntas dos seguidores virando respostas, posts e produtos**.

Mensagem curta:

> Transforme perguntas de seguidores em respostas prontas, conteudo e vendas.

Mensagem em uma frase:

> Querify ajuda creators a responder menos no manual, vender com mais clareza e descobrir o que a
> audiencia quer saber em seguida.

## ICP Do MVP

Priorizar creators que ja tem alguma monetizacao, mesmo pequena.

Bom fit:

- 5k a 250k seguidores.
- Recebe DMs ou comentarios repetidos.
- Vende produto digital, mentoria, comunidade, curso, consultoria, afiliado ou servico.
- Faz lancamentos, lives, aulas, reviews, tutoriais ou conteudo educacional.
- Tem respostas que precisam ser consistentes: preco, entrega, garantia, agenda, inscricao,
  materiais, requisitos, links, descontos, regras da comunidade.

Evitar no MVP:

- Creator sem monetizacao nenhuma.
- Creator que so quer link-in-bio bonito.
- Agencia que quer inbox social completo.
- Enterprise que quer governanca pesada.
- Comunidade grande que precisa de moderacao em tempo real.

## Produto MVP

### 1. QnA: Creator Answer Hub

Objetivo:

O creator cria uma base simples de perguntas e respostas que pode ser compartilhada no link da bio,
site, comunidade ou resposta de DM.

Funcionalidades MVP:

- Criar space "Creator Hub".
- Criar perguntas e respostas manualmente.
- Gerar rascunho com IA a partir de:
  - texto colado;
  - URL publica;
  - FAQ existente;
  - descricao de produto/curso/mentoria.
- Publicar resposta como publica ou interna.
- Copiar link de pergunta/resposta.
- Feedback simples: util / nao util.
- Busca simples por pergunta.
- Tags basicas: produto, preco, entrega, acesso, garantia, comunidade, suporte, parceria.

Limites MVP:

- Sem editor editorial complexo.
- Sem multi-idioma avancado.
- Sem fonte rica por PDF/video no primeiro corte, salvo se o source upload ja estiver pronto.
- Sem search enterprise; busca textual + matching semantico simples pode ser suficiente.

Valor para creator:

- Uma resposta boa vira ativo reutilizavel.
- O creator para de escrever a mesma explicacao.
- O link da bio pode apontar para respostas que vendem.

### 2. Direct: Ask Me Inbox

Objetivo:

Permitir que seguidores facam perguntas privadas sem depender de integracao com Instagram ou
WhatsApp no MVP.

Funcionalidades MVP:

- Link publico "Pergunte ao creator".
- Formulario privado:
  - nome;
  - email ou handle;
  - pergunta;
  - contexto opcional;
  - consentimento basico.
- Criar conversa Direct.
- Listar inbox de perguntas privadas.
- Sugerir resposta com base no QnA.
- Acoes:
  - responder manualmente;
  - copiar resposta;
  - marcar como resolvida;
  - criar lacuna para QnA;
  - promover resposta final para QnA como draft.

Limites MVP:

- Nao enviar DM real para Instagram/TikTok no inicio.
- Nao ter SLA/ticketing complexo.
- Nao ter agente autonomo.
- Resposta pode ser enviada por email simples, ou copiada para o canal externo.

Valor para creator:

- Centraliza perguntas privadas.
- Ajuda a responder mais rapido.
- Toda pergunta nova pode virar FAQ publica.

### 3. Broadcast: Comment Collector

Objetivo:

Capturar perguntas publicas em volume pequeno, agrupar repeticoes e sugerir resposta publica.

Funcionalidades MVP:

- Criar thread Broadcast manual:
  - post;
  - video;
  - live;
  - aula;
  - campanha;
  - lancamento.
- Colar comentarios manualmente ou importar CSV.
- Marcar origem: Instagram, TikTok, YouTube, LinkedIn, X, comunidade, outro.
- Classificar itens:
  - pergunta;
  - objecao;
  - elogio;
  - reclamacao;
  - sugestao;
  - spam/ignorar.
- Agrupar perguntas parecidas.
- Sugerir resposta publica com base no QnA.
- Criar lacuna QnA quando o tema aparece varias vezes.
- Encaminhar para Direct quando exigir dado privado.

Limites MVP:

- Sem OAuth/social API no primeiro corte.
- Sem responder automaticamente em redes sociais.
- Sem social listening em tempo real.
- Sem moderacao avancada.

Valor para creator:

- Mostra quais comentarios viram conteudo.
- Ajuda a responder publicamente com consistencia.
- Transforma campanha/lancamento em mapa de duvidas.

### 4. Trust: Simple Approval Log

Objetivo:

Dar confianca sem criar governanca pesada.

Funcionalidades MVP:

- Marcar resposta como "precisa revisar" quando envolver:
  - preco;
  - promessa de resultado;
  - garantia;
  - saude/financas/legal;
  - parceria;
  - regra de comunidade;
  - desconto ou oferta.
- Aprovar resposta antes de publicar.
- Registrar:
  - quem aprovou;
  - quando;
  - o que mudou;
  - racional curto.
- Historico simples de versoes.
- Voltar resposta para draft quando houver contestacao.

Limites MVP:

- Sem votacao formal.
- Sem workflow multi-participante complexo.
- Sem auditoria regulatoria completa.
- Sem governanca DAO/comunidade.

Valor para creator:

- Evita promessa errada circulando.
- Mantem historico de respostas importantes.
- Ajuda assistentes/equipe pequena a usar a versao certa.

## Como Todos Os Modulos Funcionam Juntos No MVP

Fluxo 1: pergunta privada vira FAQ.

```text
Follower envia pergunta pelo Ask Me Inbox
  -> Direct cria conversa
  -> Direct busca resposta no QnA
  -> se nao existe, Direct cria lacuna
  -> QnA cria resposta draft
  -> Trust aprova se for sensivel
  -> QnA publica resposta
```

Fluxo 2: comentarios viram conteudo.

```text
Creator cola comentarios de um post
  -> Broadcast cria thread e itens
  -> Broadcast agrupa perguntas repetidas
  -> QnA sugere resposta reutilizavel
  -> Trust aprova se houver promessa/risco
  -> Creator copia resposta publica ou transforma em post
```

Fluxo 3: resposta publicada reduz retrabalho.

```text
Creator publica link do Answer Hub
  -> seguidores consultam QnA
  -> feedback indica utilidade
  -> novas duvidas entram por Direct ou Broadcast
  -> QnA melhora a base
```

## Escopo Tecnico Para Entrega Rapida

### Fazer

- Usar QnA existente como base principal.
- Criar um Public Creator Hub usando QnA Public.
- Criar Direct minimo com formulario publico e inbox no Portal.
- Criar Broadcast minimo com thread manual/importacao CSV.
- Criar Trust minimo como approval/history para QnA Answer.
- Usar IA apenas para:
  - gerar draft;
  - sugerir resposta;
  - classificar comentario;
  - agrupar temas;
  - resumir lacuna.
- Manter human approval antes de publicar ou responder.

### Nao Fazer No MVP

- Instagram API, TikTok API, YouTube API ou WhatsApp API como dependencia inicial.
- Auto-resposta em canal externo.
- Multi-agent runtime completo.
- MCP como parte obrigatoria da experiencia do creator.
- Billing complexo por modulo.
- Busca vetorial enterprise se busca textual + embeddings simples resolver o primeiro caso.
- Trust com votacao formal.
- Painel executivo complexo.

## Arquitetura Sugerida

Componentes minimos:

| Area | Implementacao MVP |
| --- | --- |
| QnA | Reusar spaces, questions, answers, sources, tags, activity. |
| Direct | `Conversation`, `ConversationMessage`, API Portal/Public minima, form publico. |
| Broadcast | `Thread`, `Item`, API Portal minima, importacao CSV/text paste. |
| Trust | Entidade simples de `ReviewDecision` ou `ApprovalRecord` ligada a QnA answer/question. |
| IA | Service compartilhado para draft/suggestion/classification, sem agente autonomo. |
| Portal | Uma area "Creator Hub" com tabs: Answers, Inbox, Comments, Review. |
| Public | Pagina publica do creator com busca QnA e botao "Ask privately". |

Sequencia de implementacao:

1. Creator Hub publico com QnA.
2. Direct Ask Me Inbox.
3. AI answer suggestion para Direct.
4. Broadcast Comment Collector manual.
5. Trust Simple Approval Log.
6. Pricing limits no Tenant/Billing.

## Roadmap De Entrega

### Semana 1: Empacotar QnA Para Creator

Entregaveis:

- Space template "Creator Hub".
- Public page para perguntas/respostas.
- Criacao rapida de QnA.
- Copy link por pergunta.
- Tags basicas de creator.

Aceite:

- Um creator consegue criar 10 perguntas e compartilhar um hub publico.

### Semana 2: Direct Ask Me Inbox

Entregaveis:

- Formulario publico de pergunta privada.
- Conversa Direct criada a partir do formulario.
- Inbox simples no Portal.
- Status: new, answered, resolved.

Aceite:

- Um seguidor envia pergunta; o creator ve no inbox e responde/copia resposta.

### Semana 3: AI Draft E Suggestion

Entregaveis:

- Gerar resposta draft no QnA a partir de texto colado.
- Sugerir resposta Direct com base no QnA.
- Criar lacuna QnA a partir de Direct.

Aceite:

- Uma pergunta privada sem resposta vira draft QnA em menos de 2 minutos.

### Semana 4: Broadcast Manual

Entregaveis:

- Criar thread de post/video/live.
- Colar/importar comentarios.
- Classificar comentario.
- Agrupar perguntas parecidas.
- Sugerir resposta publica.

Aceite:

- O creator importa 50 comentarios e ve os 5 principais temas.

### Semana 5: Trust Simples

Entregaveis:

- Marcar resposta como sensivel.
- Aprovar/rejeitar resposta.
- Guardar racional curto e historico.
- Bloquear publicacao se precisa revisar.

Aceite:

- Resposta de preco/garantia nao publica sem aprovacao.

### Semana 6: Pricing, Limits E Launch

Entregaveis:

- Limites por plano.
- Tela de uso.
- Emails ou mensagens basicas de limite.
- Landing/onboarding curto.

Aceite:

- Creator escolhe plano, cria hub, recebe perguntas e entende limite de uso.

## Pricing Acessivel Para Influenciador

Objetivo: reduzir entrada mensal sem destruir o caminho para planos maiores.

### Planos Recomendados

| Plano | Preco mensal anualizado | Preco mensal | Para quem |
| --- | ---: | ---: | --- |
| Creator Starter | US$ 19 | US$ 25 | Creator pequeno que quer FAQ inteligente e inbox simples. |
| Creator Growth | US$ 39 | US$ 49 | Creator que recebe perguntas toda semana e vende algo. |
| Creator Pro | US$ 79 | US$ 99 | Creator com comunidade, lancamentos, assistente ou volume. |

Esses precos ficam alinhados com Manychat, Stan, Beacons e Linktree, mas o Querify entra com um
angulo diferente: conhecimento reutilizavel a partir das perguntas reais da audiencia.

### Creator Starter

Inclui:

- 1 Creator Hub publico.
- 1 space.
- 50 perguntas QnA.
- 100 perguntas privadas Direct/mes.
- 100 comentarios Broadcast importados/mes.
- 100 AI suggestions/mes.
- Trust approval log simples.
- 1 usuario.
- Branding Querify discreto.

Nao inclui:

- Integracoes externas.
- Auto-resposta.
- Colaboradores.
- Exportacao avancada.

### Creator Growth

Inclui:

- 1 Creator Hub publico.
- 3 spaces.
- 300 perguntas QnA.
- 1.000 perguntas privadas Direct/mes.
- 1.000 comentarios Broadcast importados/mes.
- 500 AI suggestions/mes.
- Trust approval log.
- 2 usuarios.
- Remocao de branding.
- Exportacao CSV.
- Templates de perguntas por nicho.

### Creator Pro

Inclui:

- 3 Creator Hubs ou marcas.
- 10 spaces.
- 1.000 perguntas QnA.
- 5.000 perguntas privadas Direct/mes.
- 10.000 comentarios Broadcast importados/mes.
- 2.500 AI suggestions/mes.
- 5 usuarios.
- Workflow de revisao simples.
- Relatorio de temas recorrentes.
- Prioridade em futuras integracoes.

### Overage Simples

| Uso extra | Preco |
| --- | ---: |
| 500 AI suggestions extras | US$ 5 |
| 1.000 comentarios importados extras | US$ 5 |
| 1.000 perguntas privadas extras | US$ 10 |
| Usuario extra | US$ 5/mes no Growth; US$ 10/mes no Pro |

Nao usar overage por token. Creator entende "pergunta", "comentario", "sugestao" e "usuario".

## Oferta De Lancamento

Para acelerar entrada:

| Oferta | Preco | Condicao |
| --- | ---: | --- |
| Founder Creator | US$ 15/mes por 6 meses | Limitado aos primeiros 50 creators, feedback mensal obrigatorio. |
| Creator Starter anual | US$ 190/ano | 2 meses gratis. |
| Creator Growth anual | US$ 390/ano | Melhor plano para creators que vendem algo. |
| Creator Pro anual | US$ 790/ano | Para creator com assistente/comunidade. |

Nao oferecer plano free com IA ilimitada. Se houver free, deve ser so para visualizar um hub com
limite muito baixo:

- 10 QnAs.
- 10 perguntas privadas.
- 10 comentarios importados.
- 10 AI suggestions.
- branding Querify.

## Como Vender Para Influenciador Com Menos Recursos

Pitch:

> Voce ja respondeu essa pergunta antes. O Querify transforma essa resposta em um link, uma FAQ e
> uma sugestao pronta para a proxima DM ou comentario.

Dores:

- "Nao consigo responder todo mundo."
- "As pessoas perguntam a mesma coisa."
- "Perco venda porque demoro para responder."
- "Nao sei que conteudo postar."
- "Minha assistente responde diferente de mim."
- "Toda live/lancamento gera as mesmas duvidas."

Promessas realistas:

- Economizar tempo.
- Responder com mais consistencia.
- Transformar duvidas em conteudo.
- Reduzir repeticao manual.
- Criar uma base que cresce com a audiencia.

Evitar prometer:

- "IA vende sozinha."
- "Automacao total de Instagram."
- "Substitui atendimento humano."
- "Garante mais vendas."

## Diferenciacao Contra Ferramentas De Creator

| Concorrente mental | O que ele resolve | Onde Querify entra |
| --- | --- | --- |
| Linktree | Links e bio | Querify responde perguntas e captura novas duvidas. |
| Stan/Beacons | Vender produto digital | Querify explica produto, reduz objecoes e revela lacunas. |
| Manychat | Automacao de DM/comentario | Querify cria memoria reutilizavel, nao so fluxo de mensagem. |
| Substack/Patreon | Monetizacao e comunidade | Querify transforma perguntas da comunidade em conhecimento. |
| Google Doc/Notion FAQ | Documento manual | Querify conecta FAQ, inbox, comentarios e aprovacao. |

O diferencial nao e "mais canais". O diferencial e **a pergunta virar ativo**.

## Metrica De Sucesso Do MVP

Produto:

- Tempo ate primeiro hub publicado: menos de 15 minutos.
- Primeira pergunta privada recebida: dentro da primeira semana.
- 10 QnAs criadas por creator ativo.
- 30% das perguntas Direct viram QnA draft.
- 20% dos comentarios importados entram em cluster recorrente.
- 50% dos creators ativos usam resposta sugerida ao menos uma vez por semana.

Negocio:

- Conversao trial -> pago acima de 8%.
- Churn mensal Starter abaixo de 8%.
- Growth como plano mais escolhido entre creators que monetizam.
- Pelo menos 20% dos Starter evoluem para Growth em 90 dias.
- CAC payback abaixo de 3 meses em canais organicos/parcerias.

## Riscos E Mitigacoes

| Risco | Mitigacao |
| --- | --- |
| Creator achar caro comparado a Linktree | Plano Starter US$ 19 anualizado e foco em economia de tempo/resposta. |
| Produto parecer incompleto sem Instagram API | Vender como hub + inbox proprio primeiro; APIs entram como upgrade. |
| Uso de IA consumir margem | Limites baixos e overage simples por AI suggestions. |
| Creator nao configurar base QnA | Templates por nicho e onboarding que cria as primeiras 10 perguntas. |
| Broadcast manual parecer trabalhoso | Importacao por copy/paste e CSV, com resumo automatico de temas. |
| Trust parecer enterprise demais | Chamar de "Approval Log" ou "Historico de respostas importantes". |

## Decisao Final

O MVP mais rapido e comercialmente viavel e:

> **Querify Creator: Answer Hub + Ask Me Inbox + Comment Collector + Approval Log.**

Ele mantem todos os modulos vivos:

- QnA publica e organiza conhecimento.
- Direct captura perguntas privadas.
- Broadcast captura perguntas publicas.
- Trust registra aprovacao e historico.

Mas reduz o peso de entrega:

- sem depender de APIs sociais no primeiro release;
- sem automacao autonoma;
- sem governanca pesada;
- sem pricing enterprise;
- sem multi-agent complexo.

Preco de entrada recomendado:

> **US$ 19/mes anualizado ou US$ 25 mensal no Creator Starter.**

Plano ideal para monetizacao:

> **US$ 39/mes anualizado ou US$ 49 mensal no Creator Growth.**

O Growth deve ser o plano principal da landing, porque fica perto de Manychat Pro e Stan Creator,
mas vende um resultado diferente: transformar perguntas em conhecimento reutilizavel.

## Fontes Consultadas

- Manychat Pricing: https://manychat.com/es/pricing
- Manychat Pro Plan: https://help.manychat.com/hc/en-us/articles/25800228332572-Pro-plan
- Stan Store Pricing: https://stan.store/blog/stan-store-pricing/
- Stan Creator vs Creator Pro: https://help.stan.store/article/31-creator-vs-creator-pro
- Beacons Pricing: https://beacons.ai/i/pricing
- Beacons Pricing Plans Help: https://help.beacons.ai/en/articles/4695681
- Substack Pricing: https://support.substack.com/hc/en-us/articles/360037607131-How-much-does-Substack-cost
- Patreon Creator Fees: https://support.patreon.com/hc/en-us/articles/11111747095181-Creator-fees-overview
- Later Creator Income Diversification 2026: https://later.com/blog/2026-creator-income-diversified-monetization/
- Later Multi-Platform Creator Strategies 2026: https://later.com/blog/how-diversification-strategies-create-resilient-creator-businesses/
- Kajabi Creator Economy Reality Check: https://kajabi.com/reality-check
