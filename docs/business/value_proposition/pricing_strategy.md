# Estrategia De Pricing Do Querify

Data de referencia: 2026-05-11.
Moeda base: USD.

Este documento recomenda quanto cobrar de um cliente pelo Querify, usando como base
[`value_proposition.md`](value_proposition.md), [`ai_product_modules_strategy.md`](ai_product_modules_strategy.md)
e uma pesquisa de mercado atual de plataformas de suporte, AI agents, RAG/chatbots, social inbox,
comunidade e feedback/governanca.

## Resumo Executivo

O preco justo inicial para o Querify nao deve ser o preco de um chatbot simples. O produto promete
um **Question-to-Knowledge OS**: capturar perguntas, responder em canais privados e publicos,
transformar repeticao em conhecimento reutilizavel e registrar validacao quando houver risco.

Minha recomendacao comercial:

- **Design partner pago:** US$ 500 a US$ 1.500/mes por 90 dias, com escopo claro e desconto
  temporario.
- **Preco inicial justo para cliente B2B real:** US$ 799/mes a US$ 1.999/mes, dependendo de
  volume, canais e modulos.
- **Preco full-product para empresas com Direct + Broadcast + Trust:** US$ 2.499/mes a
  US$ 7.500/mes.
- **Enterprise/regulado:** a partir de US$ 60k/ano, com onboarding pago.

O modelo recomendado e **assinatura base + limites de uso + overage por resultado de IA**.

Nao recomendo cobrar apenas por assento. O valor do Querify vem de reduzir repeticao, aumentar
consistencia e criar ativos de conhecimento. Assentos internos importam, mas nao capturam o valor de
um canal publico com alto volume nem de uma base QnA que resolve milhares de perguntas.

## Preco Recomendado Agora

Se for vender hoje para o primeiro cliente pagante com escopo serio, eu cobraria:

| Oferta | Preco justo | Quando usar |
| --- | ---: | --- |
| Pilot QnA | US$ 500/mes por 90 dias + US$ 1.500 onboarding | Validar ingestao de fontes, busca, rascunhos e base reutilizavel. |
| Pilot QnA + Direct | US$ 1.000/mes por 90 dias + US$ 2.500 onboarding | Cliente tem suporte privado recorrente e quer reduzir atendimento repetido. |
| Pilot QnA + Broadcast | US$ 1.250/mes por 90 dias + US$ 2.500 onboarding | Cliente tem comentarios, comunidade, lancamentos ou redes sociais com perguntas repetidas. |
| Pilot full workflow | US$ 2.000/mes por 90 dias + US$ 5.000 onboarding | Cliente quer QnA, Direct, Broadcast e validacao Trust inicial. |

Depois do piloto, migrar para plano anual:

| Plano | Preco mensal anualizado | Preco mensal month-to-month | Melhor cliente |
| --- | ---: | ---: | --- |
| Core | US$ 299 | US$ 399 | Empresa pequena que precisa de QnA publico/privado e AI drafts. |
| Growth | US$ 799 | US$ 999 | SaaS, ecommerce ou comunidade com suporte privado recorrente. |
| Scale | US$ 1.999 | US$ 2.499 | Time com varios canais, Direct + Broadcast e governanca leve. |
| Business | US$ 4.000 a US$ 7.500 | Custom | Operacao com alto volume, varios workspaces, Trust e SLAs. |
| Enterprise | US$ 60k a US$ 180k/ano | Contrato anual | Compliance, SSO, DPA, residencia de dados, integracoes e suporte dedicado. |

Preco de entrada abaixo de US$ 299/mes so faz sentido para self-serve muito limitado. Para venda
consultiva, abaixo disso o custo de onboarding, suporte e discovery consome a margem.

## Por Que Esse Preco E Defensavel

O Querify substitui ou complementa partes de varias categorias:

- AI customer support agent.
- Base de conhecimento com busca semantica/RAG.
- Help center e widget de resposta.
- Social inbox/comentarios/comunidade.
- Feedback, roadmap, votos e racional de decisao.
- Analytics de perguntas, objecoes, lacunas e demanda.

O benchmark mostra quatro faixas:

1. **Chatbot/RAG simples:** US$ 32 a US$ 500/mes.
2. **Helpdesk com IA:** US$ 19 a US$ 115+ por agente/mes, mais IA.
3. **AI agent por resolucao:** US$ 0.49 a US$ 0.99 por resolucao/sessao.
4. **Social/community/governanca:** US$ 79 a US$ 399 por usuario/mes, ou US$ 20 a US$ 500/mes por comunidade.

Entao o Querify completo nao deve se posicionar como "chatbot de US$ 40". O produto precisa ser
vendido como sistema de reducao de retrabalho e inteligencia operacional.

## Benchmark De Mercado

### Suporte E AI Agents

| Produto | Preco observado | Implicacao para Querify |
| --- | ---: | --- |
| Intercom | Planos com assento; Fin a US$ 0.99 por outcome. Copilot a US$ 29/agente/mes. Pro AI a US$ 99/mes. | Resultado de IA vale perto de US$ 1 quando resolve ou conclui workflow. |
| Help Scout AI Answers | US$ 0.75 por AI resolution. | US$ 0.50 a US$ 0.75 por resolucao e aceitavel para SMB/mid-market. |
| Gorgias | Helpdesk de US$ 10 a US$ 750/mes por volume de tickets; AI Agent em torno de US$ 0.90 por resolved conversation. | Ecommerce aceita pricing por conversa resolvida. |
| Freshdesk | US$ 19, US$ 55 e US$ 89 por agente/mes; Freddy AI Agent inclui 500 sessoes e cobra US$ 49 por 100 sessoes extras. | US$ 0.49 por sessao de IA e preco competitivo para volume. |
| Zendesk | Suite Team US$ 55/agente/mes, Suite Professional US$ 115/agente/mes, Suite Enterprise custom; Copilot US$ 50/agente/mes ou bundles de US$ 155 e US$ 209/agente/mes. | Plataformas maduras monetizam assentos + IA + add-ons. |

Conclusao: para Direct, o mercado ja educou clientes a pagar por **resultado automatizado**. O
Querify pode comecar abaixo de Intercom/Gorgias para ganhar adocao, mas nao deve ir abaixo do custo
de uma sessao AI real mais margem.

### RAG, Knowledge Base E Chatbot De Site

| Produto | Preco observado | Implicacao para Querify |
| --- | ---: | --- |
| Chatbase | US$ 32, US$ 120 e US$ 400/mes anualizado; extra de US$ 40 por 1.000 message credits. | QnA puro precisa passar de "chatbot" para "knowledge OS" para cobrar mais. |
| CustomGPT.ai | US$ 99 e US$ 499/mes; extra de US$ 375 por 2.500 queries. | RAG empresarial com citacoes e conectores sustenta US$ 100 a US$ 500/mes sem Direct/Broadcast. |
| DocsBot | Planos publicos de US$ 49, US$ 149 e US$ 499/mes, com limites de bots, fontes e mensagens. | O mercado aceita assinatura previsivel para busca em conhecimento. |
| Tidio Lyro | Add-on a partir de US$ 32.50/mes para 50 conversas Lyro; planos de plataforma de US$ 24.17, US$ 49.17 e US$ 749+/mes. | SMB compra AI chat barato, mas escala sobe rapido. |

Conclusao: QnA sozinho deve ter um plano de entrada em US$ 299/mes se incluir ingestao, revisao,
citacoes, analytics de lacunas e workflows editoriais. Se for apenas widget de resposta, o mercado
forca para US$ 49 a US$ 199/mes.

### Broadcast, Social Inbox E Comunidade

| Produto | Preco observado | Implicacao para Querify |
| --- | ---: | --- |
| Agorapulse | US$ 79, US$ 119 e US$ 149 por usuario/mes em planos anuais. | Social inbox com comentarios e respostas vale mais do que chatbot simples. |
| Sprout Social | US$ 199, US$ 299 e US$ 399 por licenca/mes anualizada. | Clientes com operacao social madura pagam ticket alto por inbox, relatorios e workflow. |
| Hootsuite | Planos Standard/Advanced em torno de US$ 149 a US$ 399/mes por usuario/plano, com social listening nos tiers maiores. | Broadcast pode ser vendido como complemento de social care e insight. |
| Discourse hosting | US$ 20, US$ 100 e US$ 500/mes; inclui creditos de IA por plano. | Comunidade tem pricing por instancia/plano, nao por assento apenas. |
| Circle | US$ 89 e US$ 199/mes nos planos principais; add-ons por admin, API, espacos e eventos. | Comunidade aceita plataforma mensal previsivel com add-ons. |

Conclusao: Broadcast deve ser empacotado a partir do plano Growth/Scale, nao como add-on barato.
O valor esta em agrupar sinais publicos e transformar objecoes em conhecimento e resposta.

### Trust, Feedback E Decisao

| Produto | Preco observado | Implicacao para Querify |
| --- | ---: | --- |
| Canny | Free limitado; Core a partir de US$ 19/mes; Pro a partir de US$ 79/mes; Business custom. | Feedback/voto isolado e barato, mas escala por tracked user e integracoes. |
| Productboard | Free; Essentials US$ 19/maker/mes; Pro US$ 59/maker/mes; Enterprise custom. | Roadmap e decisao pagam por maker, mas enterprise paga por governanca e integracao. |

Conclusao: Trust isolado nao deve ser o primeiro produto pago. Ele deve aumentar ARPA em clientes
que ja usam QnA/Direct/Broadcast e precisam de historico, racional e auditoria.

## Modelo Comercial Recomendado

### Metricas De Cobranca

Usar quatro metricas simples:

| Metrica | Por que cobrar |
| --- | --- |
| Workspace/plano base | Captura valor de plataforma, tenant, UI, storage, suporte e features. |
| Assentos internos | Reflete colaboracao humana, revisao, atendimento e governanca. |
| Interacoes processadas | Reflete escala de Direct/Broadcast/QnA sem punir cliente por equipe pequena. |
| Resultados de IA automatizados | Captura valor quando a IA resolve, responde ou conclui workflow sem humano. |

Nao recomendo cobrar por token diretamente do cliente. Token e custo interno dificil de entender.
O cliente entende: resolucao, conversa, resposta publica, fonte processada, usuario interno e
volume mensal.

### Overage Sugerido

| Uso | Overage recomendado |
| --- | ---: |
| AI assisted answer ou sugestao interna | US$ 0.05 a US$ 0.15 por uso acima da franquia |
| Automated Direct resolution | US$ 0.49 no inicio; evoluir para US$ 0.75 quando qualidade estiver provada |
| Automated Broadcast public reply | US$ 0.25 a US$ 0.75, dependendo de risco e canal |
| Fonte processada grande | US$ 0.10 a US$ 1.00 por documento, ou pacote mensal por volume |
| Extra storage/knowledge chunks | Cobrar por tier, nao por item individual |
| Extra seat | US$ 15 a US$ 49 por usuario/mes, conforme plano |

Regra: automacao que substitui atendimento humano pode ser cobrada por resultado. Copiloto que so
ajuda humano deve ficar dentro da assinatura ou ter overage baixo.

## Planos Propostos

### Core

Preco:

- US$ 299/mes anualizado.
- US$ 399 month-to-month.

Inclui:

- QnA.
- Base de conhecimento com perguntas, respostas, fontes e tags.
- AI drafts a partir de fontes.
- Busca QnA com citacoes.
- Feedback basico de utilidade.
- 3 assentos internos.
- 1 workspace.
- 3 spaces.
- 2.000 AI assisted answers/mes.
- 500 fontes/documentos processados no total inicial, depois limite mensal menor.

Nao inclui:

- Direct completo.
- Broadcast completo.
- Trust auditavel.
- Auto-resposta sem revisao.

ICP:

- SaaS pequeno.
- Ecommerce pequeno.
- Documentacao tecnica.
- Produto com FAQ repetida.

### Growth

Preco:

- US$ 799/mes anualizado.
- US$ 999 month-to-month.

Inclui:

- Tudo do Core.
- Direct Copilot.
- Conversas privadas e sugestao de resposta.
- Resumo de conversa e handoff.
- Criacao de gap candidate para QnA.
- Broadcast basico para captura manual/importada de comentarios.
- 8 assentos internos.
- 5 spaces.
- 10.000 interacoes processadas/mes.
- 500 automated Direct resolutions/mes incluidas quando a automacao estiver habilitada.
- Overage de US$ 0.49 por automated Direct resolution.

ICP:

- B2B SaaS com suporte.
- Ecommerce em crescimento.
- Creator/business community com perguntas recorrentes.
- Times que querem reduzir tickets repetidos antes de contratar mais suporte.

### Scale

Preco:

- US$ 1.999/mes anualizado.
- US$ 2.499 month-to-month.

Inclui:

- Tudo do Growth.
- Broadcast Intelligence.
- Agrupamento de comentarios, mencoes e objecoes.
- Resposta publica sugerida com fonte QnA.
- Roteamento Broadcast -> Direct.
- Trust basico para aprovacao de respostas sensiveis.
- 20 assentos internos.
- 15 spaces.
- 50.000 interacoes processadas/mes.
- 2.000 automated Direct resolutions/mes.
- 500 Broadcast AI reply suggestions/mes.
- Overage de US$ 0.49 por automated Direct resolution.
- Overage de US$ 0.15 por Broadcast suggestion e US$ 0.49 por public reply automatizada.

ICP:

- Operacoes com social, suporte e comunidade.
- Times de produto/marketing/suporte que precisam aprender com perguntas reais.
- Empresas com varias superficies publicas e privadas.

### Business

Preco:

- US$ 4.000 a US$ 7.500/mes.
- Contrato anual recomendado.

Inclui:

- Tudo do Scale.
- Trust completo inicial: validacao, racional, historico e decisao.
- Workflows de aprovacao.
- Multiplos workspaces ou marcas.
- SSO opcional.
- SLAs comerciais.
- Integracoes priorizadas.
- Limites negociados.

ICP:

- Mid-market.
- Setores regulados.
- Comunidades grandes.
- SaaS B2B com base de clientes relevante.

### Enterprise

Preco:

- A partir de US$ 60k/ano.
- Faixa comum esperada: US$ 100k a US$ 180k/ano quando houver integracoes, seguranca e alto volume.
- Onboarding/implementation: US$ 10k a US$ 50k.

Inclui:

- Contrato anual.
- Security review, DPA, SSO/SAML, audit logs, RBAC avancado.
- Ambientes dedicados ou isolamento reforcado quando necessario.
- Custom connectors.
- Revisoes executivas.
- Suporte dedicado.
- Politicas de IA e compliance.

ICP:

- Enterprise.
- Governo, saude, financeiro, educacao, seguros, B2B enterprise.
- Operacoes em que resposta errada tem risco material.

## Como Vender O Valor

O pitch de preco nao deve ser "custa menos que uma ferramenta X". Deve ser:

> O Querify reduz perguntas repetidas, melhora consistencia multicanal e transforma cada interacao
> em conhecimento reutilizavel.

Argumentos por comprador:

| Comprador | Dor | Valor monetizavel |
| --- | --- | --- |
| Suporte / CS | Tickets repetidos, handoff ruim, agente sem contexto | Reduz tempo por atendimento e aumenta self-service. |
| Marketing / Growth | Objecoes em campanha, comentarios repetidos, resposta inconsistente | Melhora conversao e velocidade de resposta publica. |
| Produto | Feedback espalhado, decisoes sem evidencia | Transforma sinal em priorizacao e racional. |
| Comunidade | Discussao vira ruido, resposta confiavel se perde | Converte contribuicao em conhecimento validado. |
| Lideranca | Nao sabe o que clientes perguntam e onde a empresa falha | Relatorios de demanda real e lacunas. |
| Compliance | Respostas antigas e decisoes sem trilha | Historico, versao, aprovacao e auditoria. |

## Modelo De ROI Simples

Use uma conta conservadora em vendas:

- Custo de um representante de atendimento nos EUA: mediana BLS de US$ 20.59/hora em May 2024.
- Custo real carregado com beneficios, gestao e ferramentas: frequentemente US$ 30 a US$ 45/hora.
- Se uma pergunta repetida consome 6 a 10 minutos, cada atendimento evitado vale perto de
  US$ 3 a US$ 7.50 em tempo operacional.
- Cobrar US$ 0.49 a US$ 0.99 por resolucao automatizada ainda deixa economia clara para o cliente.

Exemplo:

| Cenario | Resultado |
| --- | ---: |
| Interacoes repetidas por mes | 1.000 |
| Percentual resolvido ou evitado por QnA/Direct | 40% |
| Interacoes economizadas | 400 |
| Minutos economizados por interacao | 8 |
| Horas economizadas | 53 |
| Custo carregado estimado | US$ 35/hora |
| Valor operacional estimado | US$ 1.855/mes |

Nesse cenario, um plano Growth de US$ 799/mes e defensavel mesmo sem contar conversao, consistencia,
insights de produto e reducao de risco.

## Estrategia De Desconto

Desconto saudavel:

- 15% a 20% para anual pre-pago.
- 30% a 50% para design partners, limitado a 3 ou 6 meses.
- 20% para startups early-stage com logo/case study permitido.
- Desconto regional para Brasil/LatAm apenas se o cliente nao for enterprise global.

Evitar:

- Free pilot sem compromisso.
- Lifetime deal.
- Plano ilimitado com IA.
- Cobrar muito barato por cliente que exige onboarding manual.

Regra comercial: se houver onboarding humano, integracao ou importacao de base, cobrar setup. O
setup filtra curiosos, protege margem e aumenta compromisso do cliente.

## Ordem De Monetizacao Por Modulo

1. **QnA**: vender primeiro como base reutilizavel e AI drafts. Preco de entrada: US$ 299/mes.
2. **Direct**: adicionar quando houver volume privado. Monetizar por plano Growth + resolucoes.
3. **Broadcast**: adicionar quando houver volume publico. Monetizar no Scale, nao como feature
   gratis.
4. **Trust**: usar para aumentar ACV em setores sensiveis, comunidades maduras e enterprise.

Trust nao deve ser vendido isolado no inicio. Ele faz mais sentido como razao para clientes maiores
pagarem Scale, Business ou Enterprise.

## O Que Eu Cobraria Em Tres Casos Reais

### Cliente A: SaaS Pequeno

Perfil:

- 3 a 5 pessoas no time.
- 300 a 1.000 perguntas/mes.
- Help center fraco.
- Poucos canais.

Preco justo:

- Core por US$ 299/mes anualizado.
- Setup de US$ 1.000 a US$ 2.000.

Limite de desconto:

- Ate US$ 199/mes por 3 meses se for design partner bom.

### Cliente B: Ecommerce Ou SaaS Em Crescimento

Perfil:

- 5 a 15 agentes/usuarios internos.
- 2.000 a 10.000 interacoes/mes.
- Suporte privado e perguntas repetidas.
- Interesse em IA mas precisa de handoff.

Preco justo:

- Growth por US$ 799 a US$ 999/mes.
- Setup de US$ 2.500 a US$ 5.000.
- Overage por automated resolution em US$ 0.49.

Limite de desconto:

- US$ 500/mes por 90 dias, depois sobe para Growth normal.

### Cliente C: Comunidade, Mid-Market Ou Setor Sensivel

Perfil:

- Perguntas publicas e privadas.
- Comentarios, comunidade, produto e suporte envolvidos.
- Precisa de aprovacao, historico ou auditoria.

Preco justo:

- Scale por US$ 1.999 a US$ 2.499/mes.
- Business por US$ 4.000+/mes se houver Trust forte, SSO ou integracoes.
- Setup de US$ 5.000 a US$ 15.000.

Limite de desconto:

- Desconto no setup em troca de contrato anual, nao desconto grande na mensalidade.

## Recomendacao Final

Para o Querify atual, o preco mais justo para comecar a vender sem se desvalorizar e:

> **US$ 799/mes para o plano Growth, com setup de US$ 2.500, incluindo QnA + Direct Copilot +
> lacunas para QnA.**

Se o cliente ainda nao tem volume claro ou o produto ainda esta em piloto:

> **US$ 500/mes por 90 dias + US$ 1.500 de onboarding**, com contrato dizendo que o preco normal
> apos o piloto sera no minimo US$ 799/mes.

Se o cliente quer Broadcast e Trust desde o inicio:

> **US$ 1.999/mes a US$ 2.499/mes + US$ 5.000 de onboarding**.

Essa faixa e competitiva contra Intercom/Zendesk/Gorgias para suporte, acima de chatbots simples
porque o Querify cria conhecimento reutilizavel, e abaixo de suites enterprise completas o bastante
para reduzir friccao de compra.

## Fontes Consultadas

- Intercom Pricing: https://www.intercom.com/pricing
- Intercom Fin AI Agent outcomes: https://www.intercom.com/help/en/articles/8205718-fin-ai-agent-outcomes
- Help Scout AI Resolutions Pricing: https://docs.helpscout.com/article/1746-ai-resolutions-pricing
- Gorgias Pricing: https://www.gorgias.com/pricing
- Freshdesk Pricing: https://www.freshworks.com/freshdesk/pricing/
- Zendesk Pricing: https://www.zendesk.com/pricing/
- Chatbase Pricing: https://www.chatbase.co/pricing
- CustomGPT.ai Pricing: https://customgpt.ai/pricing/
- DocsBot Pricing: https://docsbot.ai/pricing
- Tidio Pricing: https://www.tidio.com/pricing/
- Agorapulse Pricing: https://www.agorapulse.com/pricing/
- Sprout Social Pricing: https://sproutsocial.com/pt/pricing/
- Hootsuite Plans: https://www.hootsuite.com/plans/business
- Circle Pricing: https://circle.so/pricing
- Discourse Pricing: https://www.discourse.org/pricing
- Canny Pricing: https://canny.io/pricing
- Productboard Pricing: https://www.productboard.com/pricing/productboard/
- OpenAI API Pricing: https://openai.com/api/pricing/
- BLS Customer Service Representatives: https://www.bls.gov/ooh/office-and-administrative-support/customer-service-representatives.htm
