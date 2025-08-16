import { createRouter, createWebHistory } from 'vue-router'
import { useAuthStore } from '@/stores/auth'

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    {
      path: '/',
      name: 'home',
      component: () => import('../views/HomeView.vue'),
    },
    {
      path: '/catalog/lista/:listId',
      name: 'CatalogWithPriceList',
      component: () => import('../views/Catalog.vue'),
      props: true,
    },
    {
      path: '/catalogo',
      name: 'catalogo',
      component: () => import('../views/Catalog.vue'),
    },
    {
      path: '/category/:code',
      name: 'Category',
      component: () => import('../views/Category.vue'),
      props: true,
    },
    {
      path: '/product/:codigo',
      name: 'Product',
      component: () => import('../views/Product.vue'),
      props: true,
    },
    {
      path: '/login',
      name: 'login',
      component: () => import('../views/LoginView.vue'),
      meta: { 
        requiresGuest: true // Solo accesible si no está autenticado
      }
    },
    {
      path: '/registro',
      name: 'registro',
      component: () => import('../views/RegisterView.vue'),
      meta: { 
        requiresGuest: true // Solo accesible si no está autenticado
      }
    },
    {
      path: '/auth/google/callback',
      name: 'google-callback',
      component: () => import('../views/GoogleCallbackView.vue')
    },
    {
      path: '/completar-perfil',
      name: 'completar-perfil',
      component: () => import('../views/CompleteProfileView.vue'),
      meta: { 
        requiresAuth: true // Solo accesible si está autenticado
      }
    },
    {
      path: '/perfil',
      name: 'perfil',
      component: () => import('../views/ProfileView.vue'),
      meta: { 
        requiresAuth: true // Solo accesible si está autenticado
      }
    },
    {
      // Catch all route for 404
      path: '/:pathMatch(.*)*',
      name: 'NotFound',
      redirect: '/',
    },
  ],
  scrollBehavior(to, from, savedPosition) {
    // Don't scroll if only query params changed (like search)
    if (to.path === from.path) {
      return false
    }
    
    if (savedPosition) {
      return savedPosition
    } else {
      return { top: 0 }
    }
  }
})

// Navigation guards
router.beforeEach((to, from, next) => {
  const authStore = useAuthStore()
  
  // Inicializar autenticación en la primera navegación
  if (!authStore.token && !authStore.user) {
    authStore.initializeAuth()
  }
  
  // Check if route requires guest access (only for non-authenticated users)
  if (to.meta.requiresGuest && authStore.isAuthenticated) {
    next('/')
    return
  }
  
  // Check if route requires authentication
  if (to.meta.requiresAuth && !authStore.isAuthenticated) {
    next({
      path: '/login',
      query: { redirect: to.fullPath }
    })
    return
  }
  
  next()
})

export default router
