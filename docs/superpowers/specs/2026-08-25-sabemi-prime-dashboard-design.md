# SABEMI Prime Dashboard Design

## Objetivo

Transformar o painel administrativo de pagamentos em uma central financeira premium, clara e operacional, preservando os fluxos existentes de consulta, filtragem, polling, paginação e atualização manual.

## Direção de marca

O rebranding será denominado SABEMI Prime. A interface usará azul-marinho profundo como base de confiança, verde-lima como acento de ação e sucesso, superfícies claras para leitura e cores semânticas restritas para processamento e falha.

Paleta principal:

- `#0B1F33` — navegação e superfícies de marca.
- `#123B52` — azul de apoio e elementos ativos.
- `#C7F36B` — ação primária, sucesso e destaque positivo.
- `#F4F7F5` — fundo do conteúdo.
- `#FFFFFF` — superfícies e campos.
- `#D9E3E5` — bordas e divisores.

A tipografia terá títulos compactos e expressivos, corpo neutro e dados operacionais em fonte monoespaçada. A marca será apresentada como `SABEMI` com a assinatura `Central de pagamentos`.

## Estrutura de tela

O layout será dividido em:

1. Sidebar com marca, navegação da visão geral, status da API e indicação de ambiente.
2. Cabeçalho com título, descrição, última atualização e ação de atualização manual.
3. Resumo operacional com quatro indicadores: total recebido, processados, em análise e falhas.
4. Área de pagamentos com filtros, tabela, estados de carregamento, vazio e erro.
5. Paginação posicionada após a tabela.

Em telas menores, a sidebar será compactada em uma faixa superior, os indicadores passarão para duas colunas e a tabela manterá rolagem horizontal controlada.

## Interações e estados

- O polling existente de cinco segundos será mantido.
- O botão manual exibirá estado ocupado durante a requisição.
- A disponibilidade da API será representada junto à marca.
- O carregamento usará placeholders visuais na tabela.
- O estado vazio orientará o usuário a ajustar os filtros.
- O erro de comunicação terá mensagem objetiva e ação de nova tentativa.
- Status continuarão semanticamente distintos e com texto em português.
- Foco de teclado será visível e a animação respeitará `prefers-reduced-motion`.

## Escopo técnico

As mudanças ficarão restritas ao frontend. A camada de dados, os contratos TypeScript e a API não serão alterados. A aplicação deverá continuar compilando com os scripts existentes e os testes atuais deverão permanecer válidos.

Arquivos principais envolvidos:

- `frontend/src/App.css` para o sistema visual e layout responsivo.
- `frontend/src/index.css` para reset, variáveis globais e tipografia base.
- `frontend/src/pages/DashboardPage.tsx` para a estrutura visual do dashboard.
- `frontend/src/components/PaymentFilters.tsx` para a barra de filtros.
- `frontend/src/components/PaymentTable.tsx` para a hierarquia da tabela e estados.
- `frontend/src/components/StatusBadge.tsx` para os status da nova identidade.

Não serão adicionados comentários ao código.

## Critérios de aceite

- A tela deixa de usar o layout genérico atual e apresenta sidebar, cabeçalho, resumo e área de pagamentos coerentes com SABEMI Prime.
- Todas as ações existentes continuam funcionando.
- A interface permanece utilizável em 320px de largura até telas grandes.
- O contraste, o foco de teclado e os estados de erro permanecem acessíveis.
- `npm run build`, `npm test -- --run` e `npm run lint` executam sem falhas.
