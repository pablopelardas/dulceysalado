import { createRouter, createWebHistory } from 'vue-router'

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    {
      path: '/',
      name: 'Home',
      component: () => import('../views/Catalog.vue'),
    },
    {
      path: '/catalog/lista/:listId',
      name: 'CatalogWithPriceList',
      component: () => import('../views/Catalog.vue'),
      props: true,
    },
    {
      path: '/catalog',
      name: 'Catalog',
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
  },
})

export default router
