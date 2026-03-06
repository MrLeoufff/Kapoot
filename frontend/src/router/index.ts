/**
 * Vue Router – routes et garde d’authentification.
 */

import { createRouter, createWebHistory } from 'vue-router'
import type { RouteRecordRaw } from 'vue-router'
import { useAuthStore } from '@/stores/auth.store'

const routes: RouteRecordRaw[] = [
  {
    path: '/',
    component: () => import('@/layouts/MainLayout.vue'),
    children: [
      {
        path: '',
        name: 'Home',
        component: () => import('@/views/HomeView.vue'),
        meta: { public: true },
      },
      {
        path: 'login',
        name: 'Login',
        component: () => import('@/views/LoginView.vue'),
        meta: { public: true, guestOnly: true },
      },
      {
        path: 'register',
        name: 'Register',
        component: () => import('@/views/RegisterView.vue'),
        meta: { public: true, guestOnly: true },
      },
      {
        path: 'dashboard',
        name: 'Dashboard',
        component: () => import('@/views/DashboardView.vue'),
        meta: { requiresAuth: true },
      },
      {
        path: 'dashboard/quiz/new',
        name: 'CreateQuiz',
        component: () => import('@/views/CreateQuizView.vue'),
        meta: { requiresAuth: true },
      },
      {
        path: 'dashboard/quiz/:id',
        name: 'QuizEdit',
        component: () => import('@/views/QuizEditView.vue'),
        meta: { requiresAuth: true },
      },
      {
        path: 'dashboard/admin',
        name: 'Admin',
        component: () => import('@/views/AdminView.vue'),
        meta: { requiresAuth: true, requiresAdmin: true },
      },
      {
        path: 'game/host/:code',
        name: 'GameHost',
        component: () => import('@/views/GameHostView.vue'),
        meta: { requiresAuth: true },
      },
      {
        path: 'join',
        name: 'Join',
        component: () => import('@/views/JoinView.vue'),
        meta: { public: true },
      },
      {
        path: 'game/player/:code',
        name: 'GamePlayer',
        component: () => import('@/views/GamePlayerView.vue'),
      },
      {
        path: ':pathMatch(.*)*',
        name: 'NotFound',
        component: () => import('@/views/NotFoundView.vue'),
        meta: { public: true },
      },
    ],
  },
]

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes,
})

router.beforeEach((to, _from, next) => {
  const auth = useAuthStore()
  const requiresAuth = to.matched.some((r) => r.meta.requiresAuth)
  const requiresAdmin = to.matched.some((r) => r.meta.requiresAdmin)
  const guestOnly = to.matched.some((r) => r.meta.guestOnly)

  if (requiresAuth && !auth.isAuthenticated) {
    next({ name: 'Login', query: { redirect: to.fullPath } })
    return
  }
  if (requiresAdmin && !auth.user?.isAdmin) {
    next({ name: 'Dashboard' })
    return
  }
  if (guestOnly && auth.isAuthenticated) {
    next({ name: 'Dashboard' })
    return
  }
  next()
})

export default router
