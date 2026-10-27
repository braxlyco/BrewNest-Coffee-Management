# Café workspace UI

## Scope

The visual refresh changes only presentation, navigation, frontend error handling, and regression tests. It does not change database records, business rules, permissions, or transaction workflows. No UI library, external font service, or image dependency was added. The coffee illustration is decorative CSS; icons are inline SVG.

- Olive, cream, and caramel palette; consistent cards, forms, tables, and status labels across features.
- Overview workspace search, category filters, clear-filter empty state, and quick links to orders, catalog items, and expiry review. No fabricated sales figures or operational counters.
- One workspace directory drives both sidebar and Overview links. Available modules open actual feature pages. Legacy `/module/{name}` links redirect to implemented pages. Future modules retain an honest availability screen.
- Responsive collapsible mobile navigation with accessible labels and expanded state, visible keyboard focus, skip link, and reduced-motion support.
- Redesigned login with password visibility toggle, disabled submit during a request, timeout/network feedback, and retained validation.
- Signed-out and insufficient-permission states are distinct. A layout-level error boundary offers retry instead of leaving a feature blank. Session startup failures are surfaced without crashing the entire shell.
- Payments, Procurement, and Returns tolerate empty, malformed, or missing-detail API error bodies and show readable fallback messages.

## Styling and maintenance

`wwwroot/css/cafe.css` is the visual layer loaded after the existing base `app.css`. Layout and navigation keep their component-scoped CSS. Feature-specific models, services, validation, and state remain in their feature folders. `Shared/WorkspaceDirectory.cs` is the source for navigation metadata; it is not an authorization source. API and page policies remain authoritative.

Frontend helper tests are source-linked into the existing xUnit project without a backend-to-WASM production reference or extra package. They check route existence, future-module labels, and error-response parsing. These are not browser-rendering tests.

## Review checklist

Start the API and frontend using the README commands. Open `https://localhost:7240/` and hard-refresh with Ctrl+F5 after changing builds.

1. Overview: search for `stock`, select categories, try an unmatched search, then clear filters. Check that cards and quick links open the expected screen.
2. Navigation: check active links and nested feature routes. At 390px and 768px wide, open the menu, select a workspace, and confirm it collapses. At desktop widths, confirm the sidebar scrolls when needed.
3. Login: submit an empty/invalid form, toggle Show/Hide, and sign in with an existing account. Do not include credentials in screenshots or reports.
4. Permissions: while signed out, visit `/inventory/lots`; while signed in with a restricted role, visit a screen without its permission. Confirm the two messages differ and the shell appears only once.
5. Management: review populated and empty lists in Identity, Administration, Catalog, Inventory, Sales, Kitchen, Production, Procurement, Returns, and Payments. Check forms, long table content, validation, and narrow screens for clipping.
6. Future modules: Delivery, Finance, and Reporting should say they are planned, not display an apparently working dashboard.
7. Failure handling: with the API temporarily unavailable, verify the login button recovers from its busy state and the app displays a useful error. A feature rendering failure should show Retry and Overview links.
8. Keyboard and accessibility: Tab through navigation, search, filters, and forms; check focus outlines, labels, skip link, and reduced-motion preference. At 200% zoom, check text and controls remain usable.
9. Browser console: inspect for unexpected errors while following the checklist. Build and unit/integration test success alone do not certify visual or cross-browser correctness.

## Verification record (2026-10-10)

Solution restore/build and xUnit regression tests were executed. Browser automation could not initialize because its request-header policy failed to load, so the rendered desktop/mobile and authenticated workflows still require the browser review above. Do not describe this limitation as a passed visual test.
