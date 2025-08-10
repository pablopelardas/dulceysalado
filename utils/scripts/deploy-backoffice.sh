#!/bin/bash
set -e

# Configuración
PROJECT_PATH="/mnt/d/working/dulceysalado/frontend/backoffice"  # Ajusta esta ruta
SERVER_IP="31.97.92.55"
SERVER_USER="root"
TAR_NAME="backoffice-dulceysaladomax-$(date +%Y%m%d-%H%M%S).tar.gz"

echo "🚀 Desplegando Backoffice..."

# Ir al proyecto
cd "$PROJECT_PATH"

# Build
echo "📦 Compilando proyecto..."
npm run build:production

# Crear tar con flags correctas para Nuxt
echo "📦 Creando archivo tar..."
tar -czphf "/tmp/$TAR_NAME" .output/ package*.json

# Subir al servidor
echo "📤 Subiendo al servidor..."
scp "/tmp/$TAR_NAME" "$SERVER_USER@$SERVER_IP:/tmp/"

# Limpiar local
rm "/tmp/$TAR_NAME"

echo "✅ Backoffice subido como: $TAR_NAME"
echo "Ejecuta en el servidor: update-backoffice.sh"
