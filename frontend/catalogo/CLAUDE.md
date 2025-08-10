# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

This is a **documentation-only repository** for the Districatalogo Public API - a multi-tenant catalog system that allows businesses to display their products publicly through subdomains. The repository contains API documentation and Postman collections for testing.

## Architecture

### API Structure
- **Multi-tenant**: Each business has its own subdomain (e.g., `empresa1.districatalogo.com`)
- **Public API**: No authentication required for catalog access
- **Dynamic Configuration**: Business-specific settings control price visibility, stock display, and ordering capabilities
- **Price Lists**: Multiple pricing tiers per business (mayorista, minorista, etc.)

### Key Endpoints
- `GET /api/catalog` - Main product catalog with pagination and filtering
- `GET /api/catalog/categorias` - Product categories
- `GET /api/catalog/producto/{codigo}` - Individual product details
- `GET /api/catalog/destacados` - Featured products
- `GET /api/catalog/empresa` - Business configuration and branding

### Data Flow
1. **Tenant Resolution**: Subdomain automatically resolves to business ID
2. **Configuration Loading**: Business settings determine data visibility
3. **Dynamic Pricing**: Price lists are applied based on business configuration
4. **Conditional Response**: Fields like `precio` and `stock` are null when disabled

## Testing

### Postman Collection
- Located at `docs/Districatalogo PUBLIC.postman_collection.json`
- Contains examples for all endpoints with sample responses
- Use `{{baseUrl}}` variable for environment switching

### Testing Methods
- **Production**: Use subdomains (e.g., `empresa1.districatalogo.com`)
- **Development**: Use `?empresaId=1` parameter override
- **Local**: `http://localhost:7000` with empresaId parameter

## API Conventions

### Response Format
- All endpoints return JSON with snake_case properties
- Consistent pagination structure with `total_count`, `page`, `page_size`, `total_pages`
- Error responses follow standard HTTP status codes

### Key Features
- **Pagination**: Always implement for product listings
- **Filtering**: Support by category, search terms, featured status
- **Conditional Fields**: Respect business configuration for price/stock visibility
- **Image URLs**: Absolute URLs pointing to API server

## Development Notes

### Vue 3 + TypeScript + Tailwind CSS Application
This is now a full-featured Vue 3 application for the public catalog.

### Build & Development Commands
```bash
# Install dependencies
npm install

# Development server
npm run dev

# Build for production
npm run build

# Preview production build
npm run preview

# Type checking
npm run type-check

# Linting
npm run lint
```

### Environment Variables
Copy `.env.example` to `.env.local` and configure:
- `VITE_API_URL` - API base URL (default: http://localhost:7000)
- `VITE_EMPRESA_ID` - Company ID for development (default: 1)

### Key Architecture Features
- **Dynamic Theming**: CSS custom properties updated via company configuration
- **Multi-tenant**: Automatic company resolution by subdomain in production
- **Responsive Design**: Mobile-first with Tailwind CSS
- **State Management**: Pinia stores for company and catalog data
- **TypeScript**: Full type safety throughout the application

### Files Structure
- `src/components/` - Vue components (layout, catalog, ui)
- `src/stores/` - Pinia state management
- `src/services/` - API service layer
- `src/composables/` - Vue composables (theme, etc.)
- `src/views/` - Route components
- `docs/` - API documentation and testing resources