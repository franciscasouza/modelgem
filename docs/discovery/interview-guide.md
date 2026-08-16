# Roteiro de entrevista — modelistas e ateliês

Público: modelistas individuais e ateliês pequenos que fazem saias e vestidos sob medida e recebem referências por WhatsApp/Instagram (product-brief).

Duração sugerida: **35–45 minutos**. Formato: videochamada ou presencial. Gravação só com consentimento explícito.

Objetivo da Fase 0: validar schema de medidas, critérios de qualidade do molde e hipóteses H1–H3 / Q1–Q3 — **não** vender o produto nem coletar imagens sem autorização.

---

## 1. Abertura (3 min)

- Apresentar o ModelaFlow em uma frase: plataforma que ajuda a transformar referências em molde editável, com a profissional no controle.
- Deixar claro: IA é assistente; molde final é responsabilidade da modelista.
- Pedir consentimento para anotar (e gravar, se aplicável).
- Confirmar que dados de clientes reais não devem ser compartilhados sem autorização.

---

## 2. Contexto do trabalho (8 min)

1. Como você recebe o pedido hoje? (WhatsApp, Instagram, presencial, outro)
2. Que tipos de referência chegam com mais frequência? (foto de peça, desenho, print, molde antigo, só medidas)
3. Qual o fluxo até o molde estar “bom para cortar”?
4. Quanto tempo leva, em média, da referência à primeira versão do molde (saia / vestido simples)?
5. O que mais gera retrabalho?

Anotar: ferramentas atuais (papel, CAD, Excel, etc.) e tamanho da equipe.

---

## 3. Medidas e bases (10 min)

Usar `measurement-schema.md` como espelho (não ler tabela inteira).

1. Para **saia reta**, quais medidas você considera obrigatórias?
2. Folgas típicas de cintura e quadril no seu método?
3. Você usa distância cintura→quadril ou “vai no olho”?
4. Para **vestido simples**, o que além de busto/cintura/quadril/comprimento é indispensável?
5. Cós separado entra na base ou só no acabamento?
6. Unidades: só cm? Já recebe polegadas ou “tamanho M”?
7. Mostrar lista canônica resumida: falta algo crítico? Sobrou algo inútil para o dia a dia?

Registrar divergências com o schema (aliases e defaults).

---

## 4. Qualidade do molde e exportação (8 min)

1. O que não pode faltar num molde para corte? (fio, piques, margens, identificação…)
2. Margem de costura e barra padrão no ateliê?
3. Você imprime em A4 mosaico, plotter, ou corta direto no tecido?
4. Qual erro de escala já te prejudicou?
5. Tolerância aceitável: se a cintura do molde sair 0,5 cm maior/menor que o calculado, você corrige ou ignora?
6. Aceitaria um PDF com escala e avisos de pendência em vez de “molde perfeito”?

Confrontar com `quality-criteria.md` (fio, piques, ±0,3 cm, escala ±2%).

---

## 5. Interpretação assistida e controle (6 min)

1. Se o sistema sugerisse tipo de peça + medidas a partir de uma foto/desenho, o que você **precisaria confirmar** antes de gerar o molde?
2. O que seria inaceitável automatizar?
3. Como prefere ver confiança/limitações? (lista de pendências, semáforo, campos estimados destacados)
4. Já tentou alguma IA para modelagem? O que frustrou?

Reforçar ADR-0001: sem confirmação, não há molde final.

---

## 6. Coleta de exemplos (3 min)

1. Você teria 3–10 exemplos **anonimizados** (desenho, foto de molde com escala, ficha) para o estudo?
2. Explicar: pasta só com material autorizado, termo simples, direito de retirada.
3. Se sim → encaminhar `example-intake-checklist.md` e não receber arquivo antes do checklist completo.

---

## 7. Encerramento (2 min)

- Resumo do que ouviu (1 frase).
- Próximo contato (opcional: validação de 1 molde piloto impresso).
- Agradecer; perguntar se indica outra profissional do mesmo perfil.

---

## Bloco opcional — métricas (se houver tempo)

Alinhado ao product-brief:

| Pergunta | Uso |
| --- | --- |
| Quantos moldes de saia/vestido por semana? | Uso semanal / priorização |
| Quantas correções típicas por molde? | Baseline de retrabalho |
| Quantos modelos você reutiliza por cliente? | Biblioteca / versionamento |
| O que faria você pagar por uma ferramenta assim? | Conversão teste → pago (qualitativo) |

---

## Registro da entrevista

Salvar em local interno do projeto (não versionar dados pessoais). Campos mínimos:

- data, perfil (modelista solo / ateliê), cidade/UF (opcional)
- consentimento gravação: sim/não
- divergências vs schema (lista)
- citação anônima útil (1–3)
- exemplos prometidos: sim/não + prazo
- hipóteses tocadas: H1 H2 H3 Q1 Q2 Q3 (suportada / enfraquecida / neutra)

---

## Amostra mínima sugerida

- 5 entrevistas (mix solo + ateliê pequeno)
- Pelo menos 2 que imprimem A4 e 2 que usam plotter/mesa
- Cobrir quem trabalha majoritariamente com foto de referência **e** quem já parte de base/molde próprio
