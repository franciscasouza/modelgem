---
name: ModelaFlow
colors:
  surface: '#f7f9fb'
  surface-dim: '#d8dadc'
  surface-bright: '#f7f9fb'
  surface-container-lowest: '#ffffff'
  surface-container-low: '#f2f4f6'
  surface-container: '#eceef0'
  surface-container-high: '#e6e8ea'
  surface-container-highest: '#e0e3e5'
  on-surface: '#191c1e'
  on-surface-variant: '#434655'
  inverse-surface: '#2d3133'
  inverse-on-surface: '#eff1f3'
  outline: '#737686'
  outline-variant: '#c3c6d7'
  surface-tint: '#0053db'
  primary: '#004ac6'
  on-primary: '#ffffff'
  primary-container: '#2563eb'
  on-primary-container: '#eeefff'
  inverse-primary: '#b4c5ff'
  secondary: '#505f76'
  on-secondary: '#ffffff'
  secondary-container: '#d0e1fb'
  on-secondary-container: '#54647a'
  tertiary: '#4d556b'
  on-tertiary: '#ffffff'
  tertiary-container: '#656d84'
  on-tertiary-container: '#eef0ff'
  error: '#ba1a1a'
  on-error: '#ffffff'
  error-container: '#ffdad6'
  on-error-container: '#93000a'
  primary-fixed: '#dbe1ff'
  primary-fixed-dim: '#b4c5ff'
  on-primary-fixed: '#00174b'
  on-primary-fixed-variant: '#003ea8'
  secondary-fixed: '#d3e4fe'
  secondary-fixed-dim: '#b7c8e1'
  on-secondary-fixed: '#0b1c30'
  on-secondary-fixed-variant: '#38485d'
  tertiary-fixed: '#dae2fd'
  tertiary-fixed-dim: '#bec6e0'
  on-tertiary-fixed: '#131b2e'
  on-tertiary-fixed-variant: '#3f465c'
  background: '#f7f9fb'
  on-background: '#191c1e'
  surface-variant: '#e0e3e5'
typography:
  display-lg:
    fontFamily: Hanken Grotesk
    fontSize: 32px
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
  sidebar-width: 280px
  panel-width: 320px
  gutter: 16px
  margin-mobile: 16px
  margin-desktop: 24px
---

## Brand & Style
The design system is engineered for the precision of garment construction and technical pattern drafting. The aesthetic is rooted in **Corporate Modernism** with a **Technical** edge, emphasizing clarity, accuracy, and professional reliability. 

The interface functions as a high-end workspace where the UI recedes to prioritize user content—technical drawings and measurement data. It avoids decorative flourishes in favor of structured layouts, purposeful whitespace, and a "drafting table" feel that empowers the pattern maker to focus on the minutiae of their craft.

## Colors
The palette is dominated by a range of sophisticated neutrals to mimic a professional studio environment. 

- **Primary (Craft Indigo):** Used exclusively for high-priority actions, active states, and focus indicators.
- **Secondary (Steel):** Applied to supporting icons, labels, and secondary navigation elements.
- **Surface Tones:** Use a range of cool grays (`#F8FAFC` to `#E2E8F0`) to differentiate the sidebar, canvas background, and property panels.
- **Accents:** Green is reserved for pattern consistency and finalized drafts; Amber signifies revision points or "work-in-progress" measurements.

## Typography
The typography system prioritizes legibility of complex data. 

- **Hanken Grotesk** is used for structural headings to provide a clean, contemporary professional feel.
- **Inter** handles all standard UI text, ensuring high readability at small sizes in dense property panels.
- **JetBrains Mono** is utilized for all numerical inputs, measurements, and technical specifications. This distinct visual change alerts the user they are interacting with "hard data."
- **Scale:** On mobile devices, `display-lg` should be reduced to 24px to maintain hierarchy within narrow viewports.

## Layout & Spacing
The system utilizes a **Fixed Grid** for administrative views (Model Library) and a **Contextual Panel** layout for the Pattern Editor.

- **Editor Layout:** Features a permanent left sidebar for tool selection and a right-side property panel for measurement inputs. The central "Canvas" is fluid, expanding to fill all remaining space.
- **Grid:** Use a 4px baseline shift for vertical rhythm. 
- **Breakpoints:** 
  - Mobile (<768px): Single column, hidden sidebars accessible via drawer.
  - Tablet (768px-1279px): Collapsed sidebars (icons only).
  - Desktop (>1280px): Full persistent panels.

## Elevation & Depth
To maintain a technical, "flat-table" feel, this design system avoids heavy shadows. Depth is achieved through **Tonal Layers** and **Low-Contrast Outlines**.

- **Level 0 (Canvas):** The base drafting area, slightly darker than the UI (`#F1F5F9`) with a 10px grid dot overlay.
- **Level 1 (Sidebars/Panels):** Pure white background with a 1px solid border (`#E2E8F0`).
- **Level 2 (Modals/Popovers):** Pure white with a 1px border and a very soft, high-diffusion shadow (8px blur, 4% opacity) to provide just enough separation from the workspace.

## Shapes
The shape language is strictly geometric. A "Soft" roundedness (4px) is applied to buttons and input fields to prevent the UI from feeling aggressive, but maintain the precision of a technical tool. 

- **Buttons/Inputs:** 4px radius.
- **Large Cards:** 8px radius.
- **Canvas Elements:** 0px radius (sharp corners) for actual pattern drawings to ensure accurate visual representations of points and vertices.

## Components
- **Buttons:** Primary buttons use 'Craft Indigo' with white text. Secondary buttons use a white fill with a 1px steel border. All buttons use 14px Semi-bold Inter.
- **Input Fields:** Measurements must use the Mono font. Labels should be placed above the field in `mono-label` style. Focused fields receive a 1px 'Craft Indigo' border and a subtle blue glow.
- **Cards (Model Library):** Use a white background, 1px border, and a large aspect-ratio image area for technical sketches. Metadata should be displayed in a clean list below the image.
- **The Canvas:** A specialized component. It features a coordinate system, a toggleable dot grid, and specialized "measurement nodes"—small circular handles (8px) used for manipulating pattern points.
- **Measurement Chips:** Small, high-contrast labels used on the canvas to show real-time lengths of seams or segments.