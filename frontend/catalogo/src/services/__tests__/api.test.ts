import { describe, it, expect, vi, beforeEach, afterEach } from 'vitest'
import { apiService } from '../api'

// Mock fetch globally
const mockFetch = vi.fn()
global.fetch = mockFetch

// Mock console.error to avoid noise in tests
const mockConsoleError = vi.spyOn(console, 'error').mockImplementation(() => {})

describe('ApiService - Novedades y Ofertas', () => {
  beforeEach(() => {
    vi.clearAllMocks()
    // Clear any existing cache by accessing private property
    ;(apiService as any).cache.clear()
    vi.clearAllTimers()
    vi.useFakeTimers()
  })

  afterEach(() => {
    vi.useRealTimers()
    mockConsoleError.mockClear()
  })

  describe('getNovedades', () => {
    it('should fetch novedades successfully', async () => {
      const mockResponse = {
        success: true,
        message: null,
        productos: [
          {
            codigo: '12345',
            nombre: 'Producto Novedad',
            descripcion: 'Descripcion del producto nuevo',
            descripcion_corta: '',
            precio: 1500.0,
            destacado: false,
            imagen_urls: [],
            stock: 10,
            tags: [],
            marca: 'Test Brand',
            unidad: 'UN',
            codigo_barras: '',
            codigo_rubro: 1,
            imagen_alt: '',
            tipo_producto: 'base',
            lista_precio_id: 95,
            lista_precio_nombre: 'Lista 50',
            lista_precio_codigo: '50'
          }
        ],
        total_productos: 1,
        empresa_nombre: 'Test Company',
        fecha_consulta: '2025-08-02T00:00:00.000Z'
      }

      mockFetch.mockResolvedValueOnce({
        ok: true,
        json: async () => mockResponse
      })

      const result = await apiService.getNovedades()

      expect(result.data).toEqual(mockResponse.productos)
      expect(result.error).toBeUndefined()
      expect(mockFetch).toHaveBeenCalledWith(
        expect.stringContaining('/api/catalog/novedades'),
        expect.objectContaining({
          method: 'GET',
          headers: {
            'Accept': 'application/json',
            'Content-Type': 'application/json'
          }
        })
      )
    })

    it('should return empty array on API error', async () => {
      mockFetch.mockRejectedValueOnce(new Error('Network error'))

      const result = await apiService.getNovedades()

      expect(result.data).toEqual([])
      expect(result.error).toBeUndefined()
      expect(mockConsoleError).toHaveBeenCalledWith(
        'Error fetching novedades:',
        'Network error'
      )
    })

    it('should return empty array on HTTP error', async () => {
      mockFetch.mockResolvedValueOnce({
        ok: false,
        status: 500
      })

      const result = await apiService.getNovedades()

      expect(result.data).toEqual([])
      expect(result.error).toBeUndefined()
    })

    it('should use cache for subsequent calls within TTL', async () => {
      const mockResponse = {
        success: true,
        productos: [{ codigo: '12345', nombre: 'Test' }]
      }

      mockFetch.mockResolvedValueOnce({
        ok: true,
        json: async () => mockResponse
      })

      // First call
      const result1 = await apiService.getNovedades()
      expect(mockFetch).toHaveBeenCalledTimes(1)

      // Second call within cache TTL (should use cache)
      const result2 = await apiService.getNovedades()
      expect(mockFetch).toHaveBeenCalledTimes(1) // Still only 1 call
      expect(result1.data).toEqual(result2.data)
    })

    it('should make new API call after cache TTL expires', async () => {
      const mockResponse = {
        success: true,
        productos: [{ codigo: '12345', nombre: 'Test' }]
      }

      mockFetch.mockResolvedValue({
        ok: true,
        json: async () => mockResponse
      })

      // First call
      await apiService.getNovedades()
      expect(mockFetch).toHaveBeenCalledTimes(1)

      // Advance time past cache TTL (5 minutes)
      vi.advanceTimersByTime(5 * 60 * 1000 + 1)

      // Second call after cache expiry (should make new API call)
      await apiService.getNovedades()
      expect(mockFetch).toHaveBeenCalledTimes(2)
    })

    it('should handle malformed API response', async () => {
      mockFetch.mockResolvedValueOnce({
        ok: true,
        json: async () => ({ invalid: 'response' })
      })

      const result = await apiService.getNovedades()

      expect(result.data).toEqual([])
      expect(result.error).toBeUndefined()
    })
  })

  describe('getOfertas', () => {
    it('should fetch ofertas successfully', async () => {
      const mockResponse = {
        success: true,
        message: null,
        productos: [
          {
            codigo: '67890',
            nombre: 'Producto en Oferta',
            descripcion: 'Descripcion del producto en oferta',
            descripcion_corta: '',
            precio: 800.0,
            destacado: false,
            imagen_urls: [],
            stock: 5,
            tags: [],
            marca: 'Offer Brand',
            unidad: 'UN',
            codigo_barras: '',
            codigo_rubro: 2,
            imagen_alt: '',
            tipo_producto: 'base',
            lista_precio_id: 95,
            lista_precio_nombre: 'Lista 50',
            lista_precio_codigo: '50'
          }
        ],
        total_productos: 1,
        empresa_nombre: 'Test Company',
        fecha_consulta: '2025-08-02T00:00:00.000Z'
      }

      mockFetch.mockResolvedValueOnce({
        ok: true,
        json: async () => mockResponse
      })

      const result = await apiService.getOfertas()

      expect(result.data).toEqual(mockResponse.productos)
      expect(result.error).toBeUndefined()
      expect(mockFetch).toHaveBeenCalledWith(
        expect.stringContaining('/api/catalog/ofertas'),
        expect.objectContaining({
          method: 'GET',
          headers: {
            'Accept': 'application/json',
            'Content-Type': 'application/json'
          }
        })
      )
    })

    it('should return empty array on API error', async () => {
      mockFetch.mockRejectedValueOnce(new Error('Network error'))

      const result = await apiService.getOfertas()

      expect(result.data).toEqual([])
      expect(result.error).toBeUndefined()
      expect(mockConsoleError).toHaveBeenCalledWith(
        'Error fetching ofertas:',
        'Network error'
      )
    })

    it('should use independent cache from novedades', async () => {
      const novedadesResponse = {
        success: true,
        productos: [{ codigo: '111', nombre: 'Novedad' }]
      }
      const ofertasResponse = {
        success: true,
        productos: [{ codigo: '222', nombre: 'Oferta' }]
      }

      mockFetch
        .mockResolvedValueOnce({
          ok: true,
          json: async () => novedadesResponse
        })
        .mockResolvedValueOnce({
          ok: true,
          json: async () => ofertasResponse
        })

      const novedades = await apiService.getNovedades()
      const ofertas = await apiService.getOfertas()

      expect(novedades.data).toEqual(novedadesResponse.productos)
      expect(ofertas.data).toEqual(ofertasResponse.productos)
      expect(mockFetch).toHaveBeenCalledTimes(2)
    })
  })

  describe('Cache System', () => {
    it('should cache novedades and ofertas independently', async () => {
      const novedadesResponse = {
        success: true,
        productos: [{ codigo: '111', nombre: 'Novedad' }]
      }
      const ofertasResponse = {
        success: true,
        productos: [{ codigo: '222', nombre: 'Oferta' }]
      }

      mockFetch
        .mockResolvedValueOnce({
          ok: true,
          json: async () => novedadesResponse
        })
        .mockResolvedValueOnce({
          ok: true,
          json: async () => ofertasResponse
        })

      // First calls - should hit API
      await apiService.getNovedades()
      await apiService.getOfertas()
      expect(mockFetch).toHaveBeenCalledTimes(2)

      // Second calls - should use cache
      await apiService.getNovedades()
      await apiService.getOfertas()
      expect(mockFetch).toHaveBeenCalledTimes(2) // Still only 2 calls

      // After cache expiry - should hit API again
      vi.advanceTimersByTime(5 * 60 * 1000 + 1)
      await apiService.getNovedades()
      await apiService.getOfertas()
      expect(mockFetch).toHaveBeenCalledTimes(4) // 2 more calls
    })

    it('should handle concurrent requests with cache', async () => {
      const mockResponse = {
        success: true,
        productos: [{ codigo: '12345', nombre: 'Test' }]
      }

      // For concurrent requests without existing cache, each will make its own call
      mockFetch.mockResolvedValue({
        ok: true,
        json: async () => mockResponse
      })

      // Make concurrent requests
      const [result1, result2, result3] = await Promise.all([
        apiService.getNovedades(),
        apiService.getNovedades(),
        apiService.getNovedades()
      ])

      // All requests should have same data even if multiple API calls were made
      expect(result1.data).toEqual(result2.data)
      expect(result2.data).toEqual(result3.data)
      expect(result1.data).toEqual(mockResponse.productos)
    })
  })

  describe('Error Handling', () => {
    it('should handle AbortController cancellation gracefully', async () => {
      const controller = new AbortController()
      
      mockFetch.mockRejectedValueOnce(new DOMException('Request aborted', 'AbortError'))

      // Cancel the request immediately
      controller.abort()

      const result = await apiService.getNovedades()

      expect(result.data).toEqual([])
      expect(result.error).toBeUndefined()
    })

    it('should handle JSON parsing errors', async () => {
      mockFetch.mockResolvedValueOnce({
        ok: true,
        json: async () => {
          throw new Error('Invalid JSON')
        }
      })

      const result = await apiService.getNovedades()

      expect(result.data).toEqual([])
      expect(result.error).toBeUndefined()
      expect(mockConsoleError).toHaveBeenCalled()
    })

    it('should handle timeout scenarios', async () => {
      // Mock a request that rejects after a delay
      mockFetch.mockImplementationOnce(() => 
        Promise.reject(new Error('Request timeout'))
      )

      const result = await apiService.getNovedades()

      expect(result.data).toEqual([])
      expect(result.error).toBeUndefined()
      expect(mockConsoleError).toHaveBeenCalledWith(
        'Error fetching novedades:',
        'Request timeout'
      )
    })
  })
})