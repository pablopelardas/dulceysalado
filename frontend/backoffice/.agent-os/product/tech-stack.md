# Technical Stack

## Frontend Framework
**Application Framework:** Nuxt 3.17.5
**JavaScript Framework:** Vue 3.5.17
**TypeScript:** 5.8.3
**Import Strategy:** node (ES modules)

## Styling & UI
**CSS Framework:** TailwindCSS 4.1.10 + @tailwindcss/vite 4.1.10
**UI Component Library:** Nuxt UI 3.1.3
**Fonts Provider:** @nuxt/fonts 0.11.4
**Icon Library:** @nuxt/icon 1.14.0

## State Management & Data
**State Management:** Pinia + @pinia/nuxt 0.11.1
**Content Management:** @nuxt/content 3.6.1
**Image Processing:** @nuxt/image 1.10.0
**API Integration:** Custom composables with JWT authentication

## Development & Quality
**Linting:** ESLint 9.29.0 + @nuxt/eslint 1.4.1
**Testing:** @nuxt/test-utils 3.19.1
**Package Manager:** npm
**Node Version:** Compatible with Nuxt 3.17.5

## Backend Integration
**Authentication:** JWT Bearer tokens with refresh mechanism
**API Base URL:** https://api.districatalogo.com (production)
**Data Sync:** External Gecom ERP system integration
**Multi-tenant:** Subdomain-based routing for client companies

## Hosting & Deployment
**Application Hosting:** Not specified (requires configuration)
**Database Hosting:** External API (backend manages database)
**Asset Hosting:** Local/CDN (configurable through Nuxt Image)
**Deployment Solution:** Node.js compatible (PM2 ecosystem configured)

## Architecture Patterns
**Multi-tenant:** Subdomain detection middleware
**Authentication:** JWT with role-based permissions
**State Pattern:** Nuxt 3 auto-imports + Pinia stores
**API Pattern:** Composables with TypeScript interfaces
**Routing:** File-based routing with dynamic parameters

## Key Dependencies
```json
{
  "nuxt": "^3.17.5",
  "@nuxt/ui": "^3.1.3",
  "tailwindcss": "^4.1.10",
  "typescript": "^5.8.3",
  "vue": "^3.5.17",
  "@pinia/nuxt": "^0.11.1",
  "@nuxt/icon": "^1.14.0",
  "@nuxt/image": "^1.10.0",
  "@nuxt/fonts": "^0.11.4"
}
```

## Environment Configuration
**Development:** env.dev.ts with feature flags
**Production:** env.prod.ts with production API endpoints
**Feature Flags:** empresaProducts, empresaCategories

## Code Repository
**URL:** /home/pablo/working/distri/districatalogo-manager
**Structure:** Standard Nuxt 3 directory structure with organized components by domain