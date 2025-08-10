#!/bin/bash

echo "🚀 Desplegando todas las aplicaciones..."
echo ""

# Hacer ejecutables los scripts si no lo son
chmod +x deploy-api.sh deploy-backoffice.sh deploy-catalogo.sh

# Ejecutar cada script
/mnt/d/working/dulceysalado/utils/scripts/deploy-api.sh
echo ""
/mnt/d/working/dulceysalado/utils/scripts/deploy-backoffice.sh
echo ""
/mnt/d/working/dulceysalado/utils/scripts/deploy-catalogo.sh

echo ""
echo "✅ Todas las aplicaciones subidas!"