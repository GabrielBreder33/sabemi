# SABEMI Prime Dashboard Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:executing-plans or superpowers:subagent-driven-development to implement this plan task-by-task. Steps use checkbox syntax for tracking.

**Goal:** Rebrand the payment administration dashboard as SABEMI Prime with a premium financial visual system while preserving all existing data flows and interactions.

**Architecture:** Keep the existing React page, hooks, services, and API contracts. Update the page composition into a sidebar plus main workspace, centralize visual tokens in CSS, and keep filters, pagination, polling, error handling, and status rendering as existing component responsibilities.

**Tech Stack:** React 19, TypeScript, Vite, CSS, Vitest, Testing Library, oxlint.

## Global Constraints

- Keep changes restricted to `frontend/` plus design documentation.
- Preserve polling, manual refresh, filters, pagination, API contracts, and current tests.
- Support layouts from 320px width to large desktop screens.
- Preserve visible keyboard focus and reduced-motion support.
- Do not add comments to code.

---

## File Map

- Modify `frontend/src/index.css` to remove starter styles and define the global reset, font stack, colors, focus treatment, and reduced-motion behavior.
- Modify `frontend/src/App.css` to define the SABEMI Prime shell, sidebar, dashboard grid, cards, filter bar, table, status colors, states, and responsive layout.
- Modify `frontend/src/pages/DashboardPage.tsx` to add the sidebar, branded header, API status, update metadata, and accessible summary labels while keeping the existing hook behavior.
- Modify `frontend/src/components/PaymentFilters.tsx` to group filter controls under the new visual hierarchy without changing submitted values.
- Modify `frontend/src/components/PaymentTable.tsx` to add table captioning, loading skeleton rows, clearer empty state, and semantic data classes without changing item formatting.
- Modify `frontend/src/components/StatusBadge.tsx` to add status markers while preserving translated labels and tooltip titles.
- Test `frontend/src/App.test.tsx` and existing page/component tests after the visual changes to ensure labels and interaction contracts remain valid.

## Task 1: Establish the SABEMI Prime visual foundation

**Files:**
- Modify: `frontend/src/index.css`
- Modify: `frontend/src/App.css`

**Interfaces:**
- Produces CSS custom properties consumed by all dashboard selectors.
- Preserves the existing class names used by page and components, adding only selectors needed by the new shell.

- [ ] **Step 1: Replace starter global styles**

Remove the Vite starter color-scheme, centered root, and default typography rules. Keep a compact system font stack, a minimum page width, reset styles, focus-visible treatment, and reduced-motion behavior.

- [ ] **Step 2: Define the brand tokens**

Add the SABEMI Prime color, spacing, radius, shadow, and typography variables to `frontend/src/App.css`, including `--navy: #0B1F33`, `--navy-soft: #123B52`, `--lime: #C7F36B`, `--surface: #FFFFFF`, `--canvas: #F4F7F5`, and `--border: #D9E3E5`.

- [ ] **Step 3: Build the responsive shell styles**

Add selectors for `.app-shell`, `.app-sidebar`, `.brand-lockup`, `.sidebar-nav`, `.sidebar-status`, `.app-main`, `.page-header`, `.summary-grid`, `.summary-card`, `.panel`, and mobile breakpoints. The desktop shell must use a fixed sidebar column and flexible content column; under 860px it must switch to a compact top rail.

- [ ] **Step 4: Run the existing frontend test suite**

Run `npm test -- --run` from `frontend/`.

Expected: existing tests pass because no component behavior has changed.

## Task 2: Recompose the dashboard page

**Files:**
- Modify: `frontend/src/pages/DashboardPage.tsx`
- Test: `frontend/src/pages/DashboardPage.test.tsx`

**Interfaces:**
- Consumes the existing `usePayments(filters, page)` result.
- Produces the same `PaymentFilters`, `PaymentTable`, refresh, and pagination interactions as before.

- [ ] **Step 1: Preserve the existing filter and summary calculations**

Keep `initialFilters`, `filters`, `page`, `handleFilters`, `processed`, `failed`, and `pending`. Use the same values for the four summary cards.

- [ ] **Step 2: Add the branded shell markup**

Render a sidebar with `SABEMI`, `Central de pagamentos`, the active `Visão geral` item, `API operacional`, and `Ambiente local`. Render the existing content inside `.app-main`.

- [ ] **Step 3: Add the premium header and update context**

Use `Visão geral` as the page heading, keep the existing explanatory intent in Portuguese, add `Atualização automática · 5s`, and keep the manual button action labeled `Atualizar dados`.

- [ ] **Step 4: Add stable semantic labels to the summary cards**

Use labels `Total recebido`, `Processados`, `Em análise`, and `Falhas`, and add small contextual text so the values remain understandable without relying on color.

- [ ] **Step 5: Run the page tests**

Run `npm test -- --run src/pages/DashboardPage.test.tsx` from `frontend/`.

Expected: PASS. If an assertion depends on the old visible heading or button text, update only the assertion to the approved Portuguese copy while preserving the underlying behavior.

## Task 3: Refine filters, table, and statuses

**Files:**
- Modify: `frontend/src/components/PaymentFilters.tsx`
- Modify: `frontend/src/components/PaymentTable.tsx`
- Modify: `frontend/src/components/StatusBadge.tsx`
- Test: `frontend/src/components/PaymentFilters.test.tsx`
- Test: `frontend/src/components/StatusBadge.test.tsx`

**Interfaces:**
- `PaymentFilters` continues to call `onChange(draft)` on submit with the same `PaymentFilters` shape.
- `PaymentTable` continues to accept `{ items, isLoading }` and format currency/date through the existing Brazilian formatters.
- `StatusBadge` continues to accept `{ value: string }` and translate known statuses.

- [ ] **Step 1: Update filter structure without changing behavior**

Keep the same controlled input/select state and option values. Add a filter heading, a compact helper label, and a reset-friendly layout wrapper. Keep the submit button type and `onChange(draft)` behavior intact.

- [ ] **Step 2: Improve the table semantics**

Keep all seven columns and their source values. Add a `caption` for screen readers, stable classes for transaction, contract, money, date, and error cells, and preserve the existing title on truncated errors.

- [ ] **Step 3: Add loading and empty visual states**

For loading, render three table-like skeleton rows within the table container. For an empty list, render the existing state message copy with a clear next action hint such as `Ajuste os filtros ou aguarde novos eventos.`

- [ ] **Step 4: Add status markers**

Keep translated labels and titles. Add a decorative marker element with `aria-hidden="true"`; the visible text remains the source of meaning. Use CSS status classes for `processed`, `sucesso`, `pending`, `processing`, `failed`, and `erro`.

- [ ] **Step 5: Run component tests**

Run `npm test -- --run src/components/PaymentFilters.test.tsx src/components/StatusBadge.test.tsx` from `frontend/`.

Expected: PASS with the same interaction and status-label coverage.

## Task 4: Verify the finished dashboard

**Files:**
- Modify: `frontend/src/App.test.tsx` only if an existing assertion references removed starter copy.

- [ ] **Step 1: Run the full frontend test suite**

Run `npm test -- --run` from `frontend/`.

Expected: PASS.

- [ ] **Step 2: Run lint**

Run `npm run lint` from `frontend/`.

Expected: PASS with no new warnings.

- [ ] **Step 3: Run the production build**

Run `npm run build` from `frontend/`.

Expected: TypeScript compilation and Vite production build complete successfully.

- [ ] **Step 4: Inspect the diff for forbidden code comments**

Run `git diff --check` and `rg -n "(^|\s)//|/\*|<!--" frontend/src` from the repository root.

Expected: `git diff --check` is clean and no new code comments are present.

- [ ] **Step 5: Review repository status**

Run `git status --short`.

Expected: only the intended frontend and documentation changes are present; preserve the pre-existing `.frontend-design-skill-install/` worktree state.
