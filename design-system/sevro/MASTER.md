# Sevro Design System — Master File

> **LOGIC:** When building or updating a specific Sevro page, first check `design-system/sevro/pages/[page-name].md`.
> If that file exists, its rules **override** this Master file.
> Otherwise, follow this Master file and `design-system/sevro/tokens.css` exclusively.

---

**Project:** Sevro — Local Service Marketplace
**Stack:** ASP.NET Core MVC (.NET 8) · Razor Views · Bootstrap 5.3.3 · Bootstrap Icons 1.11.3 · `Sevro/wwwroot/css/site.css`
**Category:** Home Services Marketplace (Electricians, Plumbers, Cleaners, Painters, Repair Specialists)
**Visual Direction:** Modern Swiss-Clean Marketplace · Flat & Restrained Elevation · Trust & Authority + Conversion
**Design Dials:** Variance 5/10 (Balanced / Modern) · Motion 3/10 (Subtle) · Density 5/10 (Standard)
**Artifacts:** [`tokens.css`](./tokens.css) · [`tokens.json`](./tokens.json) · [`preview.html`](./preview.html)

---

## 1. Visual Direction & Brand Rationale

Sevro's brand mark (`Sevro/wwwroot/images/favicon.png`) pairs a **vibrant warm house roof** with a **charcoal location pin**. This design system elevates Sevro into a crisp, trustworthy, contemporary marketplace:

- **Warm Alabaster Canvas (`#F8F7F4`) + Pure White Cards (`#FFFFFF`)**: Airy visual separation without harsh blue tinting or heavy drop shadows.
- **Burnt Terracotta-Orange Primary (`#C2410C`)**: High-energy, warm, approachable CTA color that harmonizes directly with Sevro's favicon while maintaining **5.18:1 WCAG AA** contrast with crisp white text (and **7.31:1 AAA** on hover `#9A3412`).
- **Deep Slate Ink (`#0F172A`) & Verified Trust Blue (`#1D4ED8`)**: Signals reliability, admin verification, and fixed-price transparency across provider badges, trust stats, and auth panels.
- **Plus Jakarta Sans + Inter**: Geometric warmth for headlines paired with razor-sharp UI legibility and `font-variant-numeric: tabular-nums` for `₹` prices, ratings, and booking IDs.

---

## 2. Color Palette (WCAG Verified)

### Brand & Surface Tokens

| Role | Hex | CSS Variable | Bootstrap / Sevro Alias | Contrast Check |
|---|---|---|---|---|
| **Primary (CTA)** | `#C2410C` | `--color-primary` | `--bs-primary`, `--sc-primary` | **5.18:1 AA** vs `#FFFFFF` |
| **Primary Hover** | `#9A3412` | `--color-primary-hover` | `--sc-primary-hover` | **7.31:1 AAA** vs `#FFFFFF` |
| **Primary Soft** | `#FFF4ED` | `--color-primary-soft` | `--sc-primary-soft` | **4.79:1 AA** with `#C2410C` |
| **Primary Border** | `#FDBA74` | `--color-primary-border` | — | Subtle warm outline |
| **Secondary Ink** | `#0F172A` | `--color-secondary` | `--bs-secondary` | **17.85:1 AAA** vs `#FFFFFF` |
| **Trust Accent** | `#1D4ED8` | `--color-accent` | — | **6.69:1 AA** vs `#FFFFFF` |
| **Trust Soft** | `#EFF6FF` | `--color-accent-soft` | — | **6.23:1 AA** with `#1D4ED8` |
| **Page Canvas** | `#F8F7F4` | `--color-background` | `--bs-body-bg`, `--sc-canvas` | **16.6:1 AAA** with `#18181B` |
| **Card Surface** | `#FFFFFF` | `--color-card` | `--sc-surface` | **17.7:1 AAA** with `#18181B` |
| **Muted Surface** | `#F1F0EC` | `--color-muted` | — | **6.8:1 AA** with `#52525B` |
| **Primary Text** | `#18181B` | `--color-foreground` | `--bs-body-color`, `--sc-text` | **17.7:1 AAA** on `#FFFFFF` |
| **Muted Text** | `#52525B` | `--color-muted-foreground` | `--bs-secondary-color`, `--sc-muted` | **7.61:1 AAA** on `#FFFFFF` |
| **Hairline Border** | `#E4E4E7` | `--color-border` | `--bs-border-color`, `--sc-border` | Clean surface separation |
| **Strong Border** | `#D4D4D8` | `--color-border-strong` | — | Input & card hover border |

### 6-State Booking Lifecycle & Semantic Status Tokens

| Sevro State | Text Hex | Soft Background | Stepper Dot | CSS Variables |
|---|---|---|---|---|
| **Pending** | `#B45309` | `#FEF3C7` | `#F59E0B` | `--color-pending`, `--color-pending-bg`, `--color-pending-dot` |
| **Confirmed** | `#1D4ED8` | `#EFF6FF` | `#2563EB` | `--color-confirmed`, `--color-confirmed-bg`, `--color-confirmed-dot` |
| **InProgress** | `#0E7490` | `#ECFEFF` | `#0891B2` | `--color-inprogress`, `--color-inprogress-bg`, `--color-inprogress-dot` |
| **Completed / Available** | `#15803D` | `#F0FDF4` | `#16A34A` | `--color-completed`, `--color-completed-bg`, `--color-completed-dot` |
| **Rejected / Cancelled** | `#B91C1C` | `#FEF2F2` | `#DC2626` | `--color-destructive-text`, `--color-destructive-bg`, `--color-destructive` |
| **Star Rating** | `#F59E0B` | — | — | `--color-rating` |

---

## 3. Typography

- **Heading Font:** `Plus Jakarta Sans` (Weights `600`, `700`, `800`) — tight negative tracking (`-0.025em`) on hero and page titles for modern polish.
- **Body & UI Font:** `Inter` (Weights `400`, `500`, `600`, `700`) — already used in Sevro, ultra-legible for forms, filter rails, tables, and dense dashboard tiles.
- **Prices & Metrics:** Always apply `font-variant-numeric: tabular-nums` to `₹` amounts, booking counts, ratings, and KPI figures so columns align crisply.

### Google Fonts `<link>` for `Sevro/Views/Shared/_Layout.cshtml`

```html
<link href="https://fonts.googleapis.com/css2?family=Inter:wght@400;500;600;700&family=Plus+Jakarta+Sans:wght@600;700;800&display=swap" rel="stylesheet" />
```

### Type Scale

| Level | Size | Line Height | Tracking | Font / Weight |
|---|---|---|---|---|
| Hero Display (`h1.display-5`) | `clamp(2rem, 4vw, 3rem)` | `1.12` | `-0.03em` | Plus Jakarta Sans 800 |
| Page Title (`h1.h2`) | `1.75rem` (28px) | `1.22` | `-0.02em` | Plus Jakarta Sans 700 |
| Section Heading (`h2`) | `1.375rem` (22px) | `1.28` | `-0.015em` | Plus Jakarta Sans 700 |
| Card Title (`h3` / `h5`) | `1.0625rem` (17px) | `1.35` | `-0.01em` | Plus Jakarta Sans 600 |
| Body Default | `0.95rem–1rem` (15–16px) | `1.6` | `0` | Inter 400 |
| UI Control / Nav / Button | `0.875rem–0.9375rem` | `1.3` | `0` | Inter 500–600 |
| Eyebrow / Filter Label | `0.75rem` (12px) | `1.4` | `+0.06em` | Inter 600 uppercase |

---

## 4. Spacing, Radii & Elevation

- **Spacing Scale (8px base):** `--space-xs` (4px), `--space-sm` (8px), `--space-md` (16px), `--space-lg` (24px), `--space-xl` (32px), `--space-2xl` (48px), `--space-3xl` (64px).
- **Radii:**
  - Buttons, inputs, small controls: `var(--radius-sm)` (`8px`–`10px`)
  - Cards, filter rail, metric tiles: `var(--radius-md)` (`12px`–`14px`)
  - Hero search bar, modals, split auth card: `var(--radius-lg)` (`16px`)
  - Badges, filter pills, status chips: `var(--radius-pill)` (`9999px`)
- **Elevation:** Prefer crisp `1px solid var(--color-border)` + `--shadow-xs` at rest, lifting to `--shadow-md` on interactive card hover (no heavy dark drop shadows).

---

## 5. Bootstrap 5.3 Integration (`Sevro/wwwroot/css/site.css`)

`design-system/sevro/tokens.css` is designed to drop cleanly into `Sevro/wwwroot/css/site.css` (either copied into the top of `site.css` or loaded right after `bootstrap.min.css` and before `site.css`):

1. **Zero breaking changes:** All existing `--sc-*` custom properties (`--sc-canvas`, `--sc-surface`, `--sc-primary`, `--sc-primary-hover`, `--sc-primary-soft`, `--sc-text`, `--sc-muted`, `--sc-border`) are preserved as semantic aliases.
2. **Native Bootstrap 5.3 overrides:** `--bs-primary`, `--bs-primary-rgb`, `--bs-body-bg`, `--bs-body-color`, `--bs-border-color`, `--bs-link-color`, and `--bs-focus-ring-color` automatically style native Bootstrap alerts, switches, links, borders, and focus rings consistently.
3. **Typography binding:**
   ```css
   h1, h2, h3, h4, h5, h6, .navbar-brand {
     font-family: var(--font-heading);
     letter-spacing: -0.02em;
   }
   ```

---

## 6. Sevro Component Guidelines

### Sticky Navbar (`.sc-navbar`)
- Translucent frosted white (`rgba(255, 255, 255, 0.92)` with `backdrop-filter: blur(12px)`) and crisp `1px solid var(--color-border)` bottom divider.
- Clear active nav pill (`background: var(--color-primary-soft); color: var(--color-primary);`).
- Never obscure anchored headings (`scroll-margin-top: 84px`).

### Buttons (`.btn-terracotta`, `.btn-outline-terracotta`, `.btn-ink`)
- Minimum height `40px` (`44px` for hero/checkout CTAs) with `cursor: pointer`, `font-weight: 600`, `border-radius: 10px`.
- Visible keyboard focus ring (`box-shadow: var(--shadow-focus); outline: 2px solid transparent;`).
- Subtle hover color + micro-elevation (`150–200ms`); never scale buttons in a way that shifts sibling layout.

### Service Cards (`.sc-service-card`)
- Pure white surface, `1px solid var(--color-border)`, `border-radius: 14px`, `16:9` cover thumbnail with subtle category/availability overlay badge.
- Card body hierarchy: **Category + Verified Provider badge** -> **Service Title (2-line clamp)** -> **Provider Name + City + Star Rating** -> **Fixed `₹` Price (`tabular-nums`) + Book CTA**.
- Hover: border darkens to `--color-border-strong`, `transform: translateY(-2px)`, `box-shadow: var(--shadow-md)`.

### Sticky Filter Rail & Removable Filter Pills (`.sc-filter-rail`, `.sc-filter-tag`)
- `top: 84px`, crisp white panel, clean section dividers, generous hit areas on switches/radios/checkboxes.
- Active filter pills wrap cleanly (`flex-wrap: wrap; gap: 0.5rem`), never clip labels, and show a clear `×` dismiss icon.

### 6-State Booking Stepper (`.sc-stepper`) & Status Badges
- Soft tinted pill badges with a 6px status dot + crisp high-contrast label (never rely on color alone — always pair color with icon/text).
- Stepper connectors animate smoothly and use `--color-completed-dot` for completed steps and crisp ring halos on the active step.

### Forms, Auth Split Card & Validation UX
- Explicit `<label for="...">` on every input (never placeholder-only).
- Preserve all user-entered field values on validation failure; display inline field errors beside inputs and focus a linked summary for multi-error forms.
- Show loading state on submit buttons (prevent double-bookings).

---

## 7. Anti-Patterns (Do NOT Use)

- ❌ **Emojis as UI icons** — always use consistent **Bootstrap Icons (`bi-*`)** with `aria-hidden="true"` when decorative beside visible text.
- ❌ **Blue-tinted backgrounds with blue body text** — keep neutral warm alabaster canvas (`#F8F7F4`) and charcoal text (`#18181B`) for readability.
- ❌ **Black text on orange buttons** — use `#FFFFFF` on `#C2410C` (5.18:1 AA) for a cleaner, more modern aesthetic.
- ❌ **Unverified/hidden provider trust signals** — keep Verified badges, fixed `₹` prices, and city/availability info visible above the fold on cards.
- ❌ **Layout-shifting hovers or jarring animations** — keep transitions `150–200ms` and honor `prefers-reduced-motion: reduce`.
- ❌ **Invisible focus states** — always keep `:focus-visible` ring clearly visible for keyboard navigation.

---

## 8. Pre-Delivery Checklist

- [ ] No emojis used as structural icons (Bootstrap Icons only; decorative icons have `aria-hidden="true"`)
- [ ] All primary, secondary, muted, and status badge text pairs meet **>= 4.5:1** contrast
- [ ] All clickable cards, filter pills, star buttons, and switches have `cursor: pointer` and visible `:focus-visible` states
- [ ] Prices (`₹`) and dashboard KPIs use `font-variant-numeric: tabular-nums`
- [ ] Filter pills, badges, and status labels wrap cleanly without clipping at `375px`, `768px`, `1024px`, and `1440px`
- [ ] Forms keep entered values after validation errors and provide explicit `<label>` associations
- [ ] `prefers-reduced-motion: reduce` disables transitions and hover transforms
- [ ] No content obscured by sticky navbar or sticky filter rail; zero horizontal overflow on mobile
