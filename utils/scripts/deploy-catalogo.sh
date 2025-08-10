#!/bin/bash
set -e

# Configuración
PROJECT_PATH="/mnt/d/working/dulceysalado/frontend/catalogo"  # Ajusta esta ruta
SERVER_IP="31.97.92.55"
SERVER_USER="root"
TAR_NAME="catalogo-dulceysaladomax-$(date +%Y%m%d-%H%M%S).tar.gz"

echo "🚀 Desplegando Catálogo Público..."

# Ir al proyecto
cd "$PROJECT_PATH"

# Build
echo "📦 Compilando proyecto..."
npm run build

# Crear tar
echo "📦 Creando archivo tar..."
tar -czf "/tmp/$TAR_NAME" dist/ server.js package*.json

# Subir al servidor
echo "📤 Subiendo al servidor..."
scp "/tmp/$TAR_NAME" "$SERVER_USER@$SERVER_IP:/tmp/"

# Limpiar local
rm "/tmp/$TAR_NAME"

echo "✅ Catálogo subido como: $TAR_NAME"
echo "Ejecuta en el servidor: update-catalogo.sh
