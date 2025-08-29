# Technical Stack

## Frontend Framework
- **Vue.js 3.5.17** - Progressive JavaScript framework with Composition API
- **TypeScript 5.8.0** - Static type checking for enhanced development experience

## Build & Development Tools
- **Vite 7.0.0** - Fast build tool and development server
- **vue-tsc 2.2.10** - TypeScript compiler for Vue single file components

## UI & Styling
- **Tailwind CSS 4.1.11** - Utility-first CSS framework
- **@tailwindcss/vite 4.1.11** - Vite integration for Tailwind CSS
- **@heroicons/vue 2.2.0** - Icon library for Vue.js applications

## State Management
- **Pinia 3.0.3** - Modern state management library for Vue.js

## Routing
- **Vue Router 4.5.1** - Official routing library for Vue.js applications

## Backend Integration
- **Fetch API** - Native browser API for HTTP requests
- **AbortController** - Request cancellation and management

## Production Services
- **Express.js 4.21.2** - Node.js web application framework for serving the app
- **compression 1.8.0** - HTTP compression middleware
- **connect-history-api-fallback 2.0.0** - SPA routing support

## Document Generation
- **jsPDF 3.0.1** - Client-side PDF generation for catalog exports

## Code Quality & Linting
- **ESLint 9.29.0** - JavaScript/TypeScript linting
- **@vue/eslint-config-typescript 14.5.1** - TypeScript ESLint configuration for Vue
- **@vue/eslint-config-prettier 10.2.0** - Prettier integration with ESLint
- **Prettier 3.5.3** - Code formatting

## Development Environment
- **@vitejs/plugin-vue 6.0.0** - Vue.js plugin for Vite
- **vite-plugin-vue-devtools 7.7.7** - Vue.js development tools integration
- **@tsconfig/node22 22.0.2** - TypeScript configuration for Node.js 22

## Deployment & Hosting
- **Application Hosting:** Express.js server with PM2 process management
- **Asset Hosting:** Static files served through Express.js with compression
- **Database Hosting:** External API (https://api.dulceysalado.com)
- **Deployment Solution:** PM2 with ecosystem configuration

## Architecture Patterns
- **Multi-tenant Architecture:** Subdomain-based tenant resolution
- **API-first Design:** Separate frontend consuming REST API
- **Component-based Architecture:** Vue.js single file components
- **Composables Pattern:** Reusable composition functions
- **Store Pattern:** Centralized state management with Pinia

## Import Strategy
- **ES Modules** - Native ES module imports with Vite bundling
- **Dynamic Imports** - Code splitting for optimized loading

## Environment Configuration
- **VITE_API_URL** - API base URL configuration
- **VITE_EMPRESA_ID** - Company ID for development environment

## Development Scripts
```bash
npm run dev        # Development server with hot reload
npm run build      # Production build with type checking
npm run preview    # Preview production build locally
npm run type-check # TypeScript type checking
npm run lint       # ESLint code linting
npm run format     # Prettier code formatting
npm start          # Production server start
```

## Key Technical Features
- **Server-Side Rendering:** SPA with history API fallback
- **Dynamic Theming:** CSS custom properties updated via JavaScript
- **Request Caching:** Intelligent caching system for API responses
- **Request Cancellation:** AbortController for preventing race conditions
- **Progressive Enhancement:** Mobile-first responsive design
- **Type Safety:** Full TypeScript integration throughout the application