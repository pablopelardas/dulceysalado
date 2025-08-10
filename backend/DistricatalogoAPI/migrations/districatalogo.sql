-- MySQL dump 10.13  Distrib 8.0.42, for Linux (x86_64)
--
-- Host: localhost    Database: districatalogo
-- ------------------------------------------------------
-- Server version	8.0.42-0ubuntu0.24.04.1

/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!50503 SET NAMES utf8mb4 */;
/*!40103 SET @OLD_TIME_ZONE=@@TIME_ZONE */;
/*!40103 SET TIME_ZONE='+00:00' */;
/*!40014 SET @OLD_UNIQUE_CHECKS=@@UNIQUE_CHECKS, UNIQUE_CHECKS=0 */;
/*!40014 SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0 */;
/*!40101 SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='NO_AUTO_VALUE_ON_ZERO' */;
/*!40111 SET @OLD_SQL_NOTES=@@SQL_NOTES, SQL_NOTES=0 */;

--
-- Table structure for table `agrupaciones`
--

DROP TABLE IF EXISTS `agrupaciones`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `agrupaciones` (
  `id` int NOT NULL AUTO_INCREMENT,
  `codigo` int NOT NULL COMMENT 'Código que viene de Grupo3 en productos',
  `nombre` varchar(255) COLLATE utf8mb4_unicode_ci NOT NULL COMMENT 'Nombre descriptivo de la agrupación',
  `descripcion` varchar(500) COLLATE utf8mb4_unicode_ci DEFAULT NULL COMMENT 'Descripción opcional',
  `activa` tinyint(1) DEFAULT '1' COMMENT 'Si la agrupación está activa',
  `empresa_principal_id` int NOT NULL COMMENT 'ID de la empresa principal que administra',
  `created_at` timestamp NULL DEFAULT CURRENT_TIMESTAMP,
  `updated_at` timestamp NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`),
  UNIQUE KEY `idx_agrupaciones_codigo_empresa` (`codigo`,`empresa_principal_id`),
  KEY `idx_agrupaciones_empresa_principal` (`empresa_principal_id`),
  KEY `idx_agrupaciones_activa` (`activa`),
  CONSTRAINT `fk_agrupaciones_empresa_principal` FOREIGN KEY (`empresa_principal_id`) REFERENCES `empresas` (`id`) ON DELETE RESTRICT ON UPDATE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=29 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Agrupaciones de productos basadas en campo Grupo3';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `agrupaciones`
--


--
-- Table structure for table `categorias_base`
--

DROP TABLE IF EXISTS `categorias_base`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `categorias_base` (
  `id` int NOT NULL AUTO_INCREMENT,
  `codigo_rubro` int NOT NULL,
  `nombre` varchar(100) COLLATE utf8mb4_unicode_ci NOT NULL,
  `icono` varchar(50) COLLATE utf8mb4_unicode_ci DEFAULT 0xF09F8FAA,
  `visible` tinyint(1) DEFAULT '1',
  `orden` int DEFAULT '0',
  `color` varchar(7) COLLATE utf8mb4_unicode_ci DEFAULT '#4A90E2',
  `descripcion` text COLLATE utf8mb4_unicode_ci,
  `created_by_empresa_id` int NOT NULL,
  `created_at` timestamp NULL DEFAULT CURRENT_TIMESTAMP,
  `updated_at` timestamp NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`),
  UNIQUE KEY `codigo_rubro` (`codigo_rubro`),
  KEY `idx_codigo_rubro` (`codigo_rubro`),
  KEY `idx_visible_orden` (`visible`,`orden`),
  KEY `created_by_empresa_id` (`created_by_empresa_id`),
  CONSTRAINT `categorias_base_ibfk_1` FOREIGN KEY (`created_by_empresa_id`) REFERENCES `empresas` (`id`) ON DELETE RESTRICT
) ENGINE=InnoDB AUTO_INCREMENT=206 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `categorias_empresa`
--

DROP TABLE IF EXISTS `categorias_empresa`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `categorias_empresa` (
  `id` int NOT NULL AUTO_INCREMENT,
  `empresa_id` int NOT NULL,
  `codigo_rubro` int NOT NULL,
  `nombre` varchar(100) COLLATE utf8mb4_unicode_ci NOT NULL,
  `icono` varchar(50) COLLATE utf8mb4_unicode_ci DEFAULT 0xF09F8FAA,
  `visible` tinyint(1) DEFAULT '1',
  `orden` int DEFAULT '0',
  `color` varchar(7) COLLATE utf8mb4_unicode_ci DEFAULT '#4A90E2',
  `descripcion` text COLLATE utf8mb4_unicode_ci,
  `created_at` timestamp NULL DEFAULT CURRENT_TIMESTAMP,
  `updated_at` timestamp NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`),
  UNIQUE KEY `unique_empresa_rubro` (`empresa_id`,`codigo_rubro`),
  KEY `idx_empresa_rubro` (`empresa_id`,`codigo_rubro`),
  KEY `idx_visible_orden` (`visible`,`orden`),
  CONSTRAINT `categorias_empresa_ibfk_1` FOREIGN KEY (`empresa_id`) REFERENCES `empresas` (`id`) ON DELETE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=10 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;


--
-- Table structure for table `configuracion_sistema`
--

DROP TABLE IF EXISTS `configuracion_sistema`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `configuracion_sistema` (
  `id` int NOT NULL AUTO_INCREMENT,
  `clave` varchar(100) COLLATE utf8mb4_unicode_ci NOT NULL,
  `valor` text COLLATE utf8mb4_unicode_ci,
  `tipo` enum('string','number','boolean','json') COLLATE utf8mb4_unicode_ci DEFAULT 'string',
  `descripcion` varchar(255) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `publico` tinyint(1) DEFAULT '0',
  `created_at` timestamp NULL DEFAULT CURRENT_TIMESTAMP,
  `updated_at` timestamp NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`),
  UNIQUE KEY `clave` (`clave`),
  KEY `idx_clave` (`clave`),
  KEY `idx_publico` (`publico`)
) ENGINE=InnoDB AUTO_INCREMENT=8 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `empresas`
--

DROP TABLE IF EXISTS `empresas`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `empresas` (
  `id` int NOT NULL AUTO_INCREMENT,
  `codigo` varchar(50) COLLATE utf8mb4_unicode_ci NOT NULL,
  `nombre` varchar(200) COLLATE utf8mb4_unicode_ci NOT NULL,
  `razon_social` varchar(250) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `cuit` varchar(15) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `telefono` varchar(50) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `email` varchar(100) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `direccion` text COLLATE utf8mb4_unicode_ci,
  `tipo_empresa` enum('principal','cliente') COLLATE utf8mb4_unicode_ci DEFAULT 'cliente',
  `empresa_principal_id` int DEFAULT NULL,
  `logo_url` varchar(500) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `colores_tema` json DEFAULT NULL,
  `favicon_url` varchar(500) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `dominio_personalizado` varchar(100) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `url_whatsapp` varchar(200) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `url_facebook` varchar(200) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `url_instagram` varchar(200) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `mostrar_precios` tinyint(1) DEFAULT '1',
  `mostrar_stock` tinyint(1) DEFAULT '0',
  `permitir_pedidos` tinyint(1) DEFAULT '0',
  `productos_por_pagina` int DEFAULT '20',
  `puede_agregar_productos` tinyint(1) DEFAULT '0',
  `puede_agregar_categorias` tinyint(1) DEFAULT '0',
  `activa` tinyint(1) DEFAULT '1',
  `fecha_vencimiento` date DEFAULT NULL,
  `plan` enum('basico','premium','enterprise') COLLATE utf8mb4_unicode_ci DEFAULT 'basico',
  `created_at` timestamp NULL DEFAULT CURRENT_TIMESTAMP,
  `updated_at` timestamp NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  `lista_precio_predeterminada_id` int DEFAULT NULL COMMENT 'Lista de precios por defecto para esta empresa. Si es NULL usa la lista global predeterminada.',
  PRIMARY KEY (`id`),
  UNIQUE KEY `codigo` (`codigo`),
  KEY `idx_codigo` (`codigo`),
  KEY `idx_tipo_empresa` (`tipo_empresa`),
  KEY `idx_activa` (`activa`),
  KEY `idx_dominio` (`dominio_personalizado`),
  KEY `empresa_principal_id` (`empresa_principal_id`),
  KEY `idx_empresas_lista_precio_predeterminada` (`lista_precio_predeterminada_id`),
  CONSTRAINT `empresas_ibfk_1` FOREIGN KEY (`empresa_principal_id`) REFERENCES `empresas` (`id`) ON DELETE SET NULL,
  CONSTRAINT `fk_empresas_lista_precio_predeterminada` FOREIGN KEY (`lista_precio_predeterminada_id`) REFERENCES `listas_precios` (`id`) ON DELETE SET NULL ON UPDATE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=13 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `empresas_agrupaciones_visibles`
--

DROP TABLE IF EXISTS `empresas_agrupaciones_visibles`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `empresas_agrupaciones_visibles` (
  `id` int NOT NULL AUTO_INCREMENT,
  `empresa_id` int NOT NULL COMMENT 'ID de la empresa (principal o cliente)',
  `agrupacion_id` int NOT NULL COMMENT 'ID de la agrupación',
  `visible` tinyint(1) DEFAULT '1' COMMENT 'Si la agrupación es visible para la empresa',
  `created_at` timestamp NULL DEFAULT CURRENT_TIMESTAMP,
  `updated_at` timestamp NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`),
  UNIQUE KEY `idx_empresa_agrupacion` (`empresa_id`,`agrupacion_id`),
  KEY `idx_agrupacion` (`agrupacion_id`),
  KEY `idx_empresa_visible` (`empresa_id`,`visible`),
  CONSTRAINT `fk_empresas_agrupaciones_agrupacion` FOREIGN KEY (`agrupacion_id`) REFERENCES `agrupaciones` (`id`) ON DELETE CASCADE ON UPDATE CASCADE,
  CONSTRAINT `fk_empresas_agrupaciones_empresa` FOREIGN KEY (`empresa_id`) REFERENCES `empresas` (`id`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=72 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Configuración de visibilidad de agrupaciones por empresa';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `listas_precios`
--

DROP TABLE IF EXISTS `listas_precios`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `listas_precios` (
  `id` int NOT NULL AUTO_INCREMENT,
  `codigo` varchar(50) COLLATE utf8mb4_unicode_ci NOT NULL,
  `nombre` varchar(100) COLLATE utf8mb4_unicode_ci NOT NULL,
  `descripcion` text COLLATE utf8mb4_unicode_ci,
  `activa` tinyint(1) DEFAULT '1',
  `es_predeterminada` tinyint(1) DEFAULT '0',
  `orden` int DEFAULT '0',
  `created_at` timestamp NULL DEFAULT CURRENT_TIMESTAMP,
  `updated_at` timestamp NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`),
  UNIQUE KEY `uk_codigo` (`codigo`),
  KEY `idx_activa_orden` (`activa`,`orden`)
) ENGINE=InnoDB AUTO_INCREMENT=13 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `producto_imagenes`
--

DROP TABLE IF EXISTS `producto_imagenes`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `producto_imagenes` (
  `id` int NOT NULL AUTO_INCREMENT,
  `tipo_producto` enum('base','empresa') COLLATE utf8mb4_unicode_ci NOT NULL,
  `producto_id` int NOT NULL,
  `empresa_id` int DEFAULT NULL,
  `url_imagen` varchar(500) COLLATE utf8mb4_unicode_ci NOT NULL,
  `alt_text` varchar(255) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `es_principal` tinyint(1) DEFAULT '0',
  `orden` int DEFAULT '0',
  `tipo_imagen` enum('principal','galeria','miniatura') COLLATE utf8mb4_unicode_ci DEFAULT 'galeria',
  `size_bytes` int DEFAULT NULL,
  `width_px` int DEFAULT NULL,
  `height_px` int DEFAULT NULL,
  `created_at` timestamp NULL DEFAULT CURRENT_TIMESTAMP,
  `updated_at` timestamp NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`),
  KEY `idx_tipo_producto_id` (`tipo_producto`,`producto_id`),
  KEY `idx_empresa_id` (`empresa_id`),
  KEY `idx_es_principal` (`es_principal`),
  KEY `idx_tipo_orden` (`tipo_imagen`,`orden`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `productos_base`
--

DROP TABLE IF EXISTS `productos_base`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `productos_base` (
  `id` int NOT NULL AUTO_INCREMENT,
  `codigo` int NOT NULL,
  `descripcion` varchar(500) COLLATE utf8mb4_unicode_ci NOT NULL,
  `codigo_rubro` int DEFAULT NULL,
  `existencia` decimal(8,2) DEFAULT '0.00',
  `grupo1` int DEFAULT NULL,
  `grupo2` int DEFAULT NULL,
  `grupo3` int DEFAULT NULL,
  `fecha_alta` date DEFAULT NULL,
  `fecha_modi` date DEFAULT NULL,
  `imputable` char(1) COLLATE utf8mb4_unicode_ci DEFAULT 'S',
  `disponible` char(1) COLLATE utf8mb4_unicode_ci DEFAULT 'S',
  `codigo_ubicacion` varchar(50) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `visible` tinyint(1) DEFAULT '1',
  `destacado` tinyint(1) DEFAULT '0',
  `orden_categoria` int DEFAULT '0',
  `imagen_url` varchar(500) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `imagen_alt` varchar(255) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `descripcion_corta` varchar(200) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `descripcion_larga` text COLLATE utf8mb4_unicode_ci,
  `tags` varchar(500) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `codigo_barras` varchar(100) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `marca` varchar(100) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `unidad_medida` varchar(20) COLLATE utf8mb4_unicode_ci DEFAULT 'UN',
  `administrado_por_empresa_id` int NOT NULL,
  `actualizado_gecom` timestamp NULL DEFAULT NULL,
  `created_at` timestamp NULL DEFAULT CURRENT_TIMESTAMP,
  `updated_at` timestamp NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`),
  UNIQUE KEY `codigo` (`codigo`),
  KEY `idx_codigo` (`codigo`),
  KEY `idx_codigo_rubro` (`codigo_rubro`),
  KEY `idx_visible_categoria` (`visible`,`codigo_rubro`),
  KEY `idx_destacado` (`destacado`),
  KEY `idx_actualizado_gecom` (`actualizado_gecom`),
  KEY `idx_marca` (`marca`),
  KEY `idx_codigo_barras` (`codigo_barras`),
  KEY `administrado_por_empresa_id` (`administrado_por_empresa_id`),
  FULLTEXT KEY `idx_busqueda` (`descripcion`,`descripcion_corta`,`descripcion_larga`,`tags`,`marca`),
  CONSTRAINT `productos_base_ibfk_1` FOREIGN KEY (`codigo_rubro`) REFERENCES `categorias_base` (`codigo_rubro`) ON DELETE SET NULL ON UPDATE CASCADE,
  CONSTRAINT `productos_base_ibfk_2` FOREIGN KEY (`administrado_por_empresa_id`) REFERENCES `empresas` (`id`) ON DELETE RESTRICT
) ENGINE=InnoDB AUTO_INCREMENT=8261 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;
--
-- Table structure for table `productos_base_precios`
--

DROP TABLE IF EXISTS `productos_base_precios`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `productos_base_precios` (
  `id` int NOT NULL AUTO_INCREMENT,
  `producto_base_id` int NOT NULL,
  `lista_precio_id` int NOT NULL,
  `precio` decimal(10,3) NOT NULL DEFAULT '0.000',
  `created_at` timestamp NULL DEFAULT CURRENT_TIMESTAMP,
  `updated_at` timestamp NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  `actualizado_gecom` timestamp NULL DEFAULT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `uk_producto_lista` (`producto_base_id`,`lista_precio_id`),
  KEY `idx_lista_precio` (`lista_precio_id`),
  KEY `idx_actualizado_gecom` (`actualizado_gecom`),
  CONSTRAINT `productos_base_precios_ibfk_1` FOREIGN KEY (`producto_base_id`) REFERENCES `productos_base` (`id`) ON DELETE CASCADE,
  CONSTRAINT `productos_base_precios_ibfk_2` FOREIGN KEY (`lista_precio_id`) REFERENCES `listas_precios` (`id`) ON DELETE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=13040 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;


--
-- Table structure for table `productos_empresa`
--

DROP TABLE IF EXISTS `productos_empresa`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `productos_empresa` (
  `id` int NOT NULL AUTO_INCREMENT,
  `empresa_id` int NOT NULL,
  `codigo` int NOT NULL,
  `descripcion` varchar(500) COLLATE utf8mb4_unicode_ci NOT NULL,
  `codigo_rubro` int DEFAULT NULL,
  `existencia` decimal(8,2) DEFAULT '0.00',
  `visible` tinyint(1) DEFAULT '1',
  `destacado` tinyint(1) DEFAULT '0',
  `orden_categoria` int DEFAULT '0',
  `imagen_url` varchar(500) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `imagen_alt` varchar(255) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `descripcion_corta` varchar(200) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `descripcion_larga` text COLLATE utf8mb4_unicode_ci,
  `tags` varchar(500) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `codigo_barras` varchar(100) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `marca` varchar(100) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `unidad_medida` varchar(20) COLLATE utf8mb4_unicode_ci DEFAULT 'UN',
  `created_at` timestamp NULL DEFAULT CURRENT_TIMESTAMP,
  `updated_at` timestamp NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`),
  UNIQUE KEY `unique_empresa_codigo` (`empresa_id`,`codigo`),
  KEY `idx_empresa_codigo` (`empresa_id`,`codigo`),
  KEY `idx_codigo_rubro` (`codigo_rubro`),
  KEY `idx_visible_categoria` (`visible`,`codigo_rubro`),
  KEY `idx_destacado` (`destacado`),
  FULLTEXT KEY `idx_busqueda` (`descripcion`,`descripcion_corta`,`descripcion_larga`,`tags`,`marca`),
  CONSTRAINT `productos_empresa_ibfk_1` FOREIGN KEY (`empresa_id`) REFERENCES `empresas` (`id`) ON DELETE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=6 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;
--
-- Table structure for table `productos_empresa_precios`
--

DROP TABLE IF EXISTS `productos_empresa_precios`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `productos_empresa_precios` (
  `id` int NOT NULL AUTO_INCREMENT,
  `empresa_id` int NOT NULL,
  `producto_id` int NOT NULL,
  `tipo_producto` enum('base','empresa') COLLATE utf8mb4_unicode_ci NOT NULL,
  `lista_precio_id` int NOT NULL,
  `precio_override` decimal(10,3) DEFAULT NULL,
  `created_at` timestamp NULL DEFAULT CURRENT_TIMESTAMP,
  `updated_at` timestamp NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`),
  UNIQUE KEY `uk_empresa_producto_lista` (`empresa_id`,`tipo_producto`,`producto_id`,`lista_precio_id`),
  KEY `idx_empresa_lista` (`empresa_id`,`lista_precio_id`),
  KEY `idx_tipo_producto` (`tipo_producto`,`producto_id`),
  KEY `lista_precio_id` (`lista_precio_id`),
  CONSTRAINT `productos_empresa_precios_ibfk_1` FOREIGN KEY (`empresa_id`) REFERENCES `empresas` (`id`) ON DELETE CASCADE,
  CONSTRAINT `productos_empresa_precios_ibfk_2` FOREIGN KEY (`lista_precio_id`) REFERENCES `listas_precios` (`id`) ON DELETE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=11 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `productos_empresa_precios`
--
--
-- Table structure for table `sync_logs`
--

DROP TABLE IF EXISTS `sync_logs`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `sync_logs` (
  `id` int NOT NULL AUTO_INCREMENT,
  `empresa_principal_id` int NOT NULL,
  `archivo_nombre` varchar(255) COLLATE utf8mb4_unicode_ci NOT NULL,
  `fecha_procesamiento` timestamp NULL DEFAULT CURRENT_TIMESTAMP,
  `productos_actualizados` int DEFAULT '0',
  `productos_nuevos` int DEFAULT '0',
  `errores` int DEFAULT '0',
  `tiempo_procesamiento_ms` int DEFAULT NULL,
  `estado` enum('exitoso','con_errores','fallido') COLLATE utf8mb4_unicode_ci DEFAULT 'exitoso',
  `detalles_errores` text COLLATE utf8mb4_unicode_ci,
  `usuario_proceso` varchar(100) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  PRIMARY KEY (`id`),
  KEY `idx_empresa_fecha` (`empresa_principal_id`,`fecha_procesamiento`),
  KEY `idx_estado` (`estado`),
  CONSTRAINT `sync_logs_ibfk_1` FOREIGN KEY (`empresa_principal_id`) REFERENCES `empresas` (`id`) ON DELETE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=59 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;


DROP TABLE IF EXISTS `sync_sessions`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `sync_sessions` (
  `id` char(36) COLLATE utf8mb4_unicode_ci NOT NULL,
  `empresa_principal_id` int NOT NULL,
  `lista_precio_id` int DEFAULT NULL,
  `estado` enum('iniciada','procesando','completada','error','cancelada') COLLATE utf8mb4_unicode_ci DEFAULT 'iniciada',
  `total_lotes_esperados` int DEFAULT NULL,
  `lotes_procesados` int DEFAULT '0',
  `productos_totales` int DEFAULT '0',
  `productos_actualizados` int DEFAULT '0',
  `productos_nuevos` int DEFAULT '0',
  `productos_errores` int DEFAULT '0',
  `errores_detalle` json DEFAULT NULL,
  `metricas` json DEFAULT NULL,
  `fecha_inicio` timestamp NULL DEFAULT CURRENT_TIMESTAMP,
  `fecha_fin` timestamp NULL DEFAULT NULL,
  `tiempo_total_ms` int DEFAULT NULL,
  `usuario_proceso` varchar(100) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `ip_origen` varchar(45) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `created_at` timestamp NULL DEFAULT CURRENT_TIMESTAMP,
  `updated_at` timestamp NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`),
  KEY `idx_empresa_principal` (`empresa_principal_id`),
  KEY `idx_estado` (`estado`),
  KEY `idx_fecha_inicio` (`fecha_inicio`),
  KEY `idx_created_at` (`created_at`),
  KEY `fk_sync_sessions_lista_precio` (`lista_precio_id`),
  CONSTRAINT `fk_sync_sessions_lista_precio` FOREIGN KEY (`lista_precio_id`) REFERENCES `listas_precios` (`id`),
  CONSTRAINT `sync_sessions_ibfk_1` FOREIGN KEY (`empresa_principal_id`) REFERENCES `empresas` (`id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `usuarios`
--

DROP TABLE IF EXISTS `usuarios`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `usuarios` (
  `id` int NOT NULL AUTO_INCREMENT,
  `empresa_id` int NOT NULL,
  `email` varchar(100) COLLATE utf8mb4_unicode_ci NOT NULL,
  `password_hash` varchar(255) COLLATE utf8mb4_unicode_ci NOT NULL,
  `nombre` varchar(100) COLLATE utf8mb4_unicode_ci NOT NULL,
  `apellido` varchar(100) COLLATE utf8mb4_unicode_ci NOT NULL,
  `rol` enum('admin','editor','viewer') COLLATE utf8mb4_unicode_ci DEFAULT 'editor',
  `puede_gestionar_productos_base` tinyint(1) DEFAULT '0',
  `puede_gestionar_productos_empresa` tinyint(1) DEFAULT '0',
  `puede_gestionar_categorias_base` tinyint(1) DEFAULT '0',
  `puede_gestionar_categorias_empresa` tinyint(1) DEFAULT '0',
  `puede_gestionar_usuarios` tinyint(1) DEFAULT '0',
  `puede_ver_estadisticas` tinyint(1) DEFAULT '1',
  `activo` tinyint(1) DEFAULT '1',
  `ultimo_login` timestamp NULL DEFAULT NULL,
  `created_at` timestamp NULL DEFAULT CURRENT_TIMESTAMP,
  `updated_at` timestamp NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`),
  UNIQUE KEY `email` (`email`),
  KEY `idx_empresa_email` (`empresa_id`,`email`),
  KEY `idx_activo` (`activo`),
  CONSTRAINT `usuarios_ibfk_1` FOREIGN KEY (`empresa_id`) REFERENCES `empresas` (`id`) ON DELETE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=42 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Temporary view structure for view `vista_catalogo_empresa`
--

DROP TABLE IF EXISTS `vista_catalogo_empresa`;
/*!50001 DROP VIEW IF EXISTS `vista_catalogo_empresa`*/;
SET @saved_cs_client     = @@character_set_client;
/*!50503 SET character_set_client = utf8mb4 */;
/*!50001 CREATE VIEW `vista_catalogo_empresa` AS SELECT 
 1 AS `id`,
 1 AS `codigo`,
 1 AS `descripcion`,
 1 AS `codigo_rubro`,
 1 AS `precio`,
 1 AS `existencia_empresa`,
 1 AS `existencia`,
 1 AS `visible`,
 1 AS `destacado`,
 1 AS `orden_categoria`,
 1 AS `imagen_url`,
 1 AS `imagen_alt`,
 1 AS `descripcion_corta`,
 1 AS `descripcion_larga`,
 1 AS `tags`,
 1 AS `codigo_barras`,
 1 AS `marca`,
 1 AS `unidad_medida`,
 1 AS `tipo_producto`,
 1 AS `empresa_id`,
 1 AS `empresa_nombre`,
 1 AS `updated_at`*/;
SET character_set_client = @saved_cs_client;

--
-- Temporary view structure for view `vista_catalogo_empresa_precio_default`
--

DROP TABLE IF EXISTS `vista_catalogo_empresa_precio_default`;
/*!50001 DROP VIEW IF EXISTS `vista_catalogo_empresa_precio_default`*/;
SET @saved_cs_client     = @@character_set_client;
/*!50503 SET character_set_client = utf8mb4 */;
/*!50001 CREATE VIEW `vista_catalogo_empresa_precio_default` AS SELECT 
 1 AS `producto_id`,
 1 AS `tipo_producto`,
 1 AS `codigo`,
 1 AS `descripcion`,
 1 AS `codigo_rubro`,
 1 AS `visible`,
 1 AS `destacado`,
 1 AS `imagen_url`,
 1 AS `marca`,
 1 AS `unidad_medida`,
 1 AS `empresa_id`,
 1 AS `empresa_nombre`,
 1 AS `precio_final`,
 1 AS `precio_personalizado`,
 1 AS `actualizado_gecom`,
 1 AS `updated_at`*/;
SET character_set_client = @saved_cs_client;

--
-- Temporary view structure for view `vista_categorias_empresa`
--

DROP TABLE IF EXISTS `vista_categorias_empresa`;
/*!50001 DROP VIEW IF EXISTS `vista_categorias_empresa`*/;
SET @saved_cs_client     = @@character_set_client;
/*!50503 SET character_set_client = utf8mb4 */;
/*!50001 CREATE VIEW `vista_categorias_empresa` AS SELECT 
 1 AS `id`,
 1 AS `codigo_rubro`,
 1 AS `nombre`,
 1 AS `icono`,
 1 AS `visible`,
 1 AS `orden`,
 1 AS `color`,
 1 AS `descripcion`,
 1 AS `tipo_categoria`,
 1 AS `empresa_id`,
 1 AS `empresa_nombre`*/;
SET character_set_client = @saved_cs_client;

--
-- Temporary view structure for view `vista_productos_precios_empresa`
--

DROP TABLE IF EXISTS `vista_productos_precios_empresa`;
/*!50001 DROP VIEW IF EXISTS `vista_productos_precios_empresa`*/;
SET @saved_cs_client     = @@character_set_client;
/*!50503 SET character_set_client = utf8mb4 */;
/*!50001 CREATE VIEW `vista_productos_precios_empresa` AS SELECT 
 1 AS `producto_id`,
 1 AS `tipo_producto`,
 1 AS `codigo`,
 1 AS `descripcion`,
 1 AS `codigo_rubro`,
 1 AS `visible`,
 1 AS `destacado`,
 1 AS `imagen_url`,
 1 AS `marca`,
 1 AS `unidad_medida`,
 1 AS `empresa_id`,
 1 AS `empresa_nombre`,
 1 AS `lista_precio_id`,
 1 AS `lista_codigo`,
 1 AS `lista_nombre`,
 1 AS `precio_final`,
 1 AS `precio_personalizado`,
 1 AS `actualizado_gecom`,
 1 AS `updated_at`,
 1 AS `agrupacion_codigo`,
 1 AS `agrupacion_nombre`,
 1 AS `agrupacion_activa`*/;
SET character_set_client = @saved_cs_client;

--
-- Final view structure for view `vista_catalogo_empresa`
--

/*!50001 DROP VIEW IF EXISTS `vista_catalogo_empresa`*/;
/*!50001 SET @saved_cs_client          = @@character_set_client */;
/*!50001 SET @saved_cs_results         = @@character_set_results */;
/*!50001 SET @saved_col_connection     = @@collation_connection */;
/*!50001 SET character_set_client      = utf8mb4 */;
/*!50001 SET character_set_results     = utf8mb4 */;
/*!50001 SET collation_connection      = utf8mb4_0900_ai_ci */;
/*!50001 CREATE ALGORITHM=UNDEFINED */
/*!50013 DEFINER=`districatalogo_user`@`181.117.11.223` SQL SECURITY DEFINER */
/*!50001 VIEW `vista_catalogo_empresa` AS select `vcp`.`producto_id` AS `id`,`vcp`.`codigo` AS `codigo`,`vcp`.`descripcion` AS `descripcion`,`vcp`.`codigo_rubro` AS `codigo_rubro`,`vcp`.`precio_final` AS `precio`,`pe`.`existencia` AS `existencia_empresa`,coalesce(`pb`.`existencia`,`pe`.`existencia`,0) AS `existencia`,`vcp`.`visible` AS `visible`,`vcp`.`destacado` AS `destacado`,coalesce(`pb`.`orden_categoria`,`pe`.`orden_categoria`,0) AS `orden_categoria`,`vcp`.`imagen_url` AS `imagen_url`,coalesce(`pb`.`imagen_alt`,`pe`.`imagen_alt`) AS `imagen_alt`,coalesce(`pb`.`descripcion_corta`,`pe`.`descripcion_corta`) AS `descripcion_corta`,coalesce(`pb`.`descripcion_larga`,`pe`.`descripcion_larga`) AS `descripcion_larga`,coalesce(`pb`.`tags`,`pe`.`tags`) AS `tags`,coalesce(`pb`.`codigo_barras`,`pe`.`codigo_barras`) AS `codigo_barras`,`vcp`.`marca` AS `marca`,`vcp`.`unidad_medida` AS `unidad_medida`,`vcp`.`tipo_producto` AS `tipo_producto`,`vcp`.`empresa_id` AS `empresa_id`,`vcp`.`empresa_nombre` AS `empresa_nombre`,`vcp`.`updated_at` AS `updated_at` from ((`vista_catalogo_empresa_precio_default` `vcp` left join `productos_base` `pb` on(((`vcp`.`tipo_producto` = 'base') and (`vcp`.`producto_id` = `pb`.`id`)))) left join `productos_empresa` `pe` on(((`vcp`.`tipo_producto` = 'empresa') and (`vcp`.`producto_id` = `pe`.`id`)))) */;
/*!50001 SET character_set_client      = @saved_cs_client */;
/*!50001 SET character_set_results     = @saved_cs_results */;
/*!50001 SET collation_connection      = @saved_col_connection */;

--
-- Final view structure for view `vista_catalogo_empresa_precio_default`
--

/*!50001 DROP VIEW IF EXISTS `vista_catalogo_empresa_precio_default`*/;
/*!50001 SET @saved_cs_client          = @@character_set_client */;
/*!50001 SET @saved_cs_results         = @@character_set_results */;
/*!50001 SET @saved_col_connection     = @@collation_connection */;
/*!50001 SET character_set_client      = utf8mb4 */;
/*!50001 SET character_set_results     = utf8mb4 */;
/*!50001 SET collation_connection      = utf8mb4_0900_ai_ci */;
/*!50001 CREATE ALGORITHM=UNDEFINED */
/*!50013 DEFINER=`districatalogo_user`@`181.117.11.223` SQL SECURITY DEFINER */
/*!50001 VIEW `vista_catalogo_empresa_precio_default` AS select `vppe`.`producto_id` AS `producto_id`,`vppe`.`tipo_producto` AS `tipo_producto`,`vppe`.`codigo` AS `codigo`,`vppe`.`descripcion` AS `descripcion`,`vppe`.`codigo_rubro` AS `codigo_rubro`,`vppe`.`visible` AS `visible`,`vppe`.`destacado` AS `destacado`,`vppe`.`imagen_url` AS `imagen_url`,`vppe`.`marca` AS `marca`,`vppe`.`unidad_medida` AS `unidad_medida`,`vppe`.`empresa_id` AS `empresa_id`,`vppe`.`empresa_nombre` AS `empresa_nombre`,`vppe`.`precio_final` AS `precio_final`,`vppe`.`precio_personalizado` AS `precio_personalizado`,`vppe`.`actualizado_gecom` AS `actualizado_gecom`,`vppe`.`updated_at` AS `updated_at` from (`vista_productos_precios_empresa` `vppe` join `listas_precios` `lp` on((`vppe`.`lista_precio_id` = `lp`.`id`))) where (`lp`.`es_predeterminada` = true) */;
/*!50001 SET character_set_client      = @saved_cs_client */;
/*!50001 SET character_set_results     = @saved_cs_results */;
/*!50001 SET collation_connection      = @saved_col_connection */;

--
-- Final view structure for view `vista_categorias_empresa`
--

/*!50001 DROP VIEW IF EXISTS `vista_categorias_empresa`*/;
/*!50001 SET @saved_cs_client          = @@character_set_client */;
/*!50001 SET @saved_cs_results         = @@character_set_results */;
/*!50001 SET @saved_col_connection     = @@collation_connection */;
/*!50001 SET character_set_client      = utf8mb4 */;
/*!50001 SET character_set_results     = utf8mb4 */;
/*!50001 SET collation_connection      = utf8mb4_0900_ai_ci */;
/*!50001 CREATE ALGORITHM=UNDEFINED */
/*!50013 DEFINER=`districatalogo_user`@`181.117.11.223` SQL SECURITY DEFINER */
/*!50001 VIEW `vista_categorias_empresa` AS select `cb`.`id` AS `id`,`cb`.`codigo_rubro` AS `codigo_rubro`,`cb`.`nombre` AS `nombre`,`cb`.`icono` AS `icono`,`cb`.`visible` AS `visible`,`cb`.`orden` AS `orden`,`cb`.`color` AS `color`,`cb`.`descripcion` AS `descripcion`,'base' AS `tipo_categoria`,`e`.`id` AS `empresa_id`,`e`.`nombre` AS `empresa_nombre` from (`categorias_base` `cb` join `empresas` `e`) where (`cb`.`visible` = true) union all select `ce`.`id` AS `id`,`ce`.`codigo_rubro` AS `codigo_rubro`,`ce`.`nombre` AS `nombre`,`ce`.`icono` AS `icono`,`ce`.`visible` AS `visible`,`ce`.`orden` AS `orden`,`ce`.`color` AS `color`,`ce`.`descripcion` AS `descripcion`,'empresa' AS `tipo_categoria`,`ce`.`empresa_id` AS `empresa_id`,`e`.`nombre` AS `empresa_nombre` from (`categorias_empresa` `ce` join `empresas` `e` on((`ce`.`empresa_id` = `e`.`id`))) where (`ce`.`visible` = true) */;
/*!50001 SET character_set_client      = @saved_cs_client */;
/*!50001 SET character_set_results     = @saved_cs_results */;
/*!50001 SET collation_connection      = @saved_col_connection */;

--
-- Final view structure for view `vista_productos_precios_empresa`
--

/*!50001 DROP VIEW IF EXISTS `vista_productos_precios_empresa`*/;
/*!50001 SET @saved_cs_client          = @@character_set_client */;
/*!50001 SET @saved_cs_results         = @@character_set_results */;
/*!50001 SET @saved_col_connection     = @@collation_connection */;
/*!50001 SET character_set_client      = utf8mb4 */;
/*!50001 SET character_set_results     = utf8mb4 */;
/*!50001 SET collation_connection      = utf8mb4_0900_ai_ci */;
/*!50001 CREATE ALGORITHM=UNDEFINED */
/*!50013 DEFINER=`districatalogo_user`@`181.117.11.223` SQL SECURITY DEFINER */
/*!50001 VIEW `vista_productos_precios_empresa` AS select `pb`.`id` AS `producto_id`,'base' AS `tipo_producto`,`pb`.`codigo` AS `codigo`,`pb`.`descripcion` AS `descripcion`,`pb`.`codigo_rubro` AS `codigo_rubro`,`pb`.`visible` AS `visible`,`pb`.`destacado` AS `destacado`,`pb`.`imagen_url` AS `imagen_url`,`pb`.`marca` AS `marca`,`pb`.`unidad_medida` AS `unidad_medida`,`e`.`id` AS `empresa_id`,`e`.`nombre` AS `empresa_nombre`,`lp`.`id` AS `lista_precio_id`,`lp`.`codigo` AS `lista_codigo`,`lp`.`nombre` AS `lista_nombre`,coalesce(`pep`.`precio_override`,`pbp`.`precio`,0) AS `precio_final`,(case when (`pep`.`precio_override` is not null) then true else false end) AS `precio_personalizado`,`pbp`.`actualizado_gecom` AS `actualizado_gecom`,`pb`.`updated_at` AS `updated_at`,`pb`.`grupo3` AS `agrupacion_codigo`,`a`.`nombre` AS `agrupacion_nombre`,`a`.`activa` AS `agrupacion_activa` from ((((((`productos_base` `pb` join `empresas` `e`) join `listas_precios` `lp`) left join `productos_base_precios` `pbp` on(((`pb`.`id` = `pbp`.`producto_base_id`) and (`lp`.`id` = `pbp`.`lista_precio_id`)))) left join `productos_empresa_precios` `pep` on(((`e`.`id` = `pep`.`empresa_id`) and (`pep`.`tipo_producto` = 'base') and (`pb`.`id` = `pep`.`producto_id`) and (`lp`.`id` = `pep`.`lista_precio_id`)))) left join `agrupaciones` `a` on(((`pb`.`grupo3` = `a`.`codigo`) and (`a`.`empresa_principal_id` = coalesce(`e`.`empresa_principal_id`,`e`.`id`))))) left join `empresas_agrupaciones_visibles` `eav` on(((`a`.`id` = `eav`.`agrupacion_id`) and (`eav`.`empresa_id` = `e`.`id`)))) where ((`pb`.`visible` = true) and (`lp`.`activa` = true) and (`a`.`activa` = true) and (`pb`.`existencia` > 0) and ((`pb`.`grupo3` is null) or (`eav`.`visible` = true) or exists(select 1 from `empresas_agrupaciones_visibles` `eav2` where (`eav2`.`empresa_id` = `e`.`id`)) is false)) union all select `pe`.`id` AS `producto_id`,'empresa' AS `tipo_producto`,`pe`.`codigo` AS `codigo`,`pe`.`descripcion` AS `descripcion`,`pe`.`codigo_rubro` AS `codigo_rubro`,`pe`.`visible` AS `visible`,`pe`.`destacado` AS `destacado`,`pe`.`imagen_url` AS `imagen_url`,`pe`.`marca` AS `marca`,`pe`.`unidad_medida` AS `unidad_medida`,`pe`.`empresa_id` AS `empresa_id`,`e`.`nombre` AS `empresa_nombre`,`lp`.`id` AS `lista_precio_id`,`lp`.`codigo` AS `lista_codigo`,`lp`.`nombre` AS `lista_nombre`,coalesce(`pep`.`precio_override`,0) AS `precio_final`,true AS `precio_personalizado`,NULL AS `actualizado_gecom`,`pe`.`updated_at` AS `updated_at`,NULL AS `agrupacion_codigo`,NULL AS `agrupacion_nombre`,NULL AS `agrupacion_activa` from (((`productos_empresa` `pe` join `empresas` `e` on((`pe`.`empresa_id` = `e`.`id`))) join `listas_precios` `lp`) left join `productos_empresa_precios` `pep` on(((`pe`.`empresa_id` = `pep`.`empresa_id`) and (`pep`.`tipo_producto` = 'empresa') and (`pe`.`id` = `pep`.`producto_id`) and (`lp`.`id` = `pep`.`lista_precio_id`)))) where ((`pe`.`visible` = true) and (`lp`.`activa` = true) and (`pe`.`existencia` > 0)) */;
/*!50001 SET character_set_client      = @saved_cs_client */;
/*!50001 SET character_set_results     = @saved_cs_results */;
/*!50001 SET collation_connection      = @saved_col_connection */;
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed on 2025-07-21 22:40:01
