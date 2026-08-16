---
name: ModelaFlow Dark
colors:
  surface: '#0F172A'
  surface-dim: '#11131b'
  surface-bright: '#373942'
  surface-container-lowest: '#0c0e16'
  surface-container-low: '#191b23'
  surface-container: '#1E293B'
  surface-container-high: '#334155'
  surface-container-highest: '#32343d'
  on-surface: '#F8FAFC'
  on-surface-variant: '#94A3B8'
  inverse-surface: '#e1e2ed'
  inverse-on-surface: '#2e3039'
  outline: '#475569'
  outline-variant: '#434655'
  surface-tint: '#b4c5ff'
  primary: '#b4c5ff'
  on-primary: '#002a78'
  primary-container: '#2563eb'
  on-primary-container: '#eeefff'
  inverse-primary: '#0053db'
  secondary: '#b9c8de'
  on-secondary: '#233143'
  secondary-container: '#39485a'
  on-secondary-container: '#a7b6cc'
  tertiary: '#bfc7d4'
  on-tertiary: '#29313b'
  tertiary-container: '#666e79'
  on-tertiary-container: '#e9f1fe'
  error: '#F87171'
  on-error: '#690005'
  error-container: '#93000a'
  on-error-container: '#ffdad6'
  primary-fixed: '#dbe1ff'
  primary-fixed-dim: '#b4c5ff'
  on-primary-fixed: '#00174b'
  on-primary-fixed-variant: '#003ea8'
  secondary-fixed: '#d4e4fa'
  secondary-fixed-dim: '#b9c8de'
  on-secondary-fixed: '#0d1c2d'
  on-secondary-fixed-variant: '#39485a'
  tertiary-fixed: '#dbe3f0'
  tertiary-fixed-dim: '#bfc7d4'
  on-tertiary-fixed: '#141c25'
  on-tertiary-fixed-variant: '#3f4752'
  background: '#11131b'
  on-background: '#e1e2ed'
  surface-variant: '#32343d'
  tailor-blue: '#2563EB'
typography:
  display-lg:
    fontFamily: Hanken Grotesk
    fontSize: 32px
    fontWeight: '600'
    lineHeight: '1.2'
    letterSpacing: -0.02em
  display-lg-mobile:
    fontFamily: Hanken Grotesk
    fontSize: 24px
    fontWeight: '600'
    lineHeight: '1.2'
    letterSpacing: -0.02em
  headline-md:
    fontFamily: Hanken Grotesk
    fontSize: 24px
    fontWeight: '500'
    lineHeight: '1.3'
  body-base:
    fontFamily: Inter
    fontSize: 16px
    fontWeight: '400'
    lineHeight: '1.5'
  body-sm:
    fontFamily: Inter
    fontSize: 14px
    fontWeight: '400'
    lineHeight: '1.4'
  mono-data:
    fontFamily: JetBrains Mono
    fontSize: 13px
    fontWeight: '500'
    lineHeight: '1.4'
  mono-label:
    fontFamily: JetBrains Mono
    fontSize: 11px
    fontWeight: '400'
    lineHeight: '1.2'
rounded:
  sm: 0.125rem
  DEFAULT: 0.25rem
  md: 0.375rem
  lg: 0.5rem
  xl: 0.75rem
  full: 9999px
spacing:
  unit: 4px
  gutter: 16px
  margin-mobile: 16px
  margin-desktop: 24px
  sidebar-width: 280px
  panel-width: 320px
---

## Personalidade e Estilo de Marca

O sistema de design em sua versão Dark Mode é projetado para a precisão da construção de vestuário e modelagem técnica, agora otimizado para ambientes de baixa luminosidade que reduzem a fadiga ocular durante longas sessões de detalhamento. A estética é fundamentada no **Modernismo Corporativo** com uma aresta **Técnica**, enfatizando clareza, exatidão e confiabilidade profissional.

A interface funciona como um workspace de alta precisão onde a UI recua para priorizar o conteúdo do usuário — desenhos técnicos e dados de medição. Evita-se adornos decorativos em favor de layouts estruturados, espaços em branco propositados e uma sensação de "mesa de desenho digital" que capacita o modelista a focar nos detalhes minuciosos de seu ofício.

## Cores

A paleta é dominada por uma gama de neutros profundos e sofisticados para mimetizar um estúdio profissional em modo noturno.

- **Primária (Tailor Blue):** Mantida vibrante para garantir legibilidade e destaque em superfícies escuras. Usada exclusivamente para ações de alta prioridade, estados ativos e indicadores de foco.
- **Secundária (Steel):** Aplicada a ícones de suporte, rótulos e elementos de navegação secundária.
- **Tons de Superfície:** Utiliza uma progressão de azuis acinzentados profundos para diferenciar a barra lateral, o fundo da tela de desenho e os painéis de propriedades, garantindo que a hierarquia visual seja mantida sem a necessidade de brilho excessivo.
- **Contraste:** Todo o texto e elementos de interface foram validados para garantir conformidade com padrões de acessibilidade sobre fundos escuros.

## Tipografia

O sistema tipográfico prioriza a legibilidade de dados complexos em alto contraste.

- **Hanken Grotesk** é utilizado para cabeçalhos estruturais, proporcionando uma sensação profissional contemporânea e limpa.
- **Inter** lida com todo o texto padrão da UI, garantindo alta legibilidade em tamanhos pequenos em painéis de propriedades densos.
- **JetBrains Mono** é utilizado para todas as entradas numéricas, medições e especificações técnicas. Esta mudança visual distinta alerta o usuário de que ele está interagindo com "dados brutos".
- **Escala Móvel:** Em dispositivos móveis, o nível `display-lg` deve ser reduzido para o tamanho mobile especificado para manter a hierarquia em viewports estreitos.

## Layout e Espaçamento

O sistema utiliza uma **Grade Fixa** para visualizações administrativas (Biblioteca de Modelos) e um layout de **Painel Contextual** para o Editor de Moldes.

- **Layout do Editor:** Apresenta uma barra lateral esquerda permanente para seleção de ferramentas e um painel de propriedades à direita para entradas de medição. O "Canvas" central é fluido.
- **Ritmo Vertical:** Utilize um deslocamento de linha de base de 4px para manter o ritmo visual.
- **Responsividade:** 
  - **Mobile (<768px):** Coluna única, barras laterais ocultas acessíveis via drawer.
  - **Tablet (768px-1279px):** Barras laterais colapsadas (apenas ícones).
  - **Desktop (>1280px):** Painéis persistentes completos.

## Elevação e Profundidade

Para manter a sensação técnica de "mesa de desenho", este sistema evita sombras pesadas. A profundidade no modo escuro é alcançada através de **Camadas Tonais** e **Contornos de Baixo Contraste**.

- **Nível 0 (Canvas):** A área base de desenho, ligeiramente mais clara que o fundo da UI (`#1E293B`) com um overlay de grade de pontos para orientação.
- **Nível 1 (Sidebars/Painéis):** Fundo em tom de superfície padrão com uma borda sólida de 1px (`#334155`).
- **Nível 2 (Modais/Popovers):** Superfícies ligeiramente mais elevadas com uma borda definida e uma sombra de ambiente sutil (difusão de 12px, 20% de opacidade em preto) para separação clara do workspace.

## Formas

A linguagem de formas é estritamente geométrica. Uma arredondamento "Suave" (4px) é aplicado a botões e campos de entrada para evitar que a UI pareça agressiva, mantendo a precisão de uma ferramenta técnica.

- **Botões/Inputs:** Raio de 4px.
- **Cards Grandes:** Raio de 8px.
- **Elementos do Canvas:** Raio de 0px (cantos vivos) para desenhos de moldes reais, garantindo representações visuais precisas de pontos e vértices.

## Componentes

- **Botões:** Botões primários utilizam 'Tailor Blue' com texto branco. Botões secundários usam um preenchimento transparente com borda de 1px na cor de contorno. Todos os botões usam Inter 14px Semi-bold.
- **Campos de Entrada:** As medições devem obrigatoriamente usar a fonte Mono. Os rótulos devem ser posicionados acima do campo no estilo `mono-label`. Campos focados recebem uma borda de 1px 'Tailor Blue'.
- **Cards (Biblioteca):** Fundo em tom de container, borda de 1px e uma área de imagem de grande proporção para esboços técnicos. Metadados exibidos em lista limpa abaixo da imagem.
- **O Canvas:** Um componente especializado com sistema de coordenadas, grade de pontos alternável e "nós de medição" — pequenas alças circulares (8px) para manipular pontos do molde.
- **Chips de Medição:** Rótulos pequenos de alto contraste usados no canvas para exibir comprimentos em tempo real de costuras ou segmentos.