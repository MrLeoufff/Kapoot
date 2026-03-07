<template>
  <div class="admin">
    <h1>Administration</h1>
    <p class="muted">Gestion des utilisateurs et des quiz. Réservé aux administrateurs.</p>

    <section class="admin-section">
      <h2>Utilisateurs</h2>
      <div v-if="loadingUsers" class="loading">Chargement…</div>
      <div v-else-if="usersError" class="error">{{ usersError }}</div>
      <div v-else class="table-wrap">
        <table class="admin-table">
          <thead>
            <tr>
              <th>Email</th>
              <th>Pseudo</th>
              <th>Admin</th>
              <th>Inscrit le</th>
              <th>Actions</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="u in users" :key="u.id">
              <td>{{ u.email }}</td>
              <td>{{ u.pseudo }}</td>
              <td>
                <span v-if="u.id === auth.user?.id" class="badge you">Vous</span>
                <button
                  v-else
                  type="button"
                  class="btn btn-small"
                  :class="u.isAdmin ? 'btn-danger' : 'btn-outline'"
                  :disabled="togglingUserId === u.id"
                  @click="toggleAdmin(u)"
                >
                  {{ u.isAdmin ? 'Retirer admin' : 'Mettre admin' }}
                </button>
              </td>
              <td>{{ formatDate(u.dateCreated) }}</td>
              <td>
                <button
                  v-if="u.id !== auth.user?.id"
                  type="button"
                  class="btn btn-small btn-danger"
                  :disabled="deletingUserId === u.id"
                  @click="confirmDeleteUser(u)"
                >
                  Supprimer
                </button>
                <span v-else class="muted">—</span>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </section>

    <section class="admin-section">
      <h2>Quiz</h2>
      <div v-if="loadingQuizzes" class="loading">Chargement…</div>
      <div v-else-if="quizzesError" class="error">{{ quizzesError }}</div>
      <div v-else class="table-wrap">
        <table class="admin-table">
          <thead>
            <tr>
              <th>Titre</th>
              <th>Propriétaire (ID)</th>
              <th>Statut</th>
              <th>Actions</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="q in quizzes" :key="q.id">
              <td>{{ q.title }}</td>
              <td><code>{{ q.ownerId }}</code></td>
              <td>
                <span class="status" :class="q.status === 1 ? 'published' : 'draft'">
                  {{ q.status === 1 ? 'Publié' : 'Brouillon' }}
                </span>
              </td>
              <td>
                <RouterLink :to="`/dashboard/quiz/${q.id}`" class="btn btn-small btn-outline">Éditer</RouterLink>
                <button
                  type="button"
                  class="btn btn-small btn-danger"
                  :disabled="deletingQuizId === q.id"
                  @click="confirmDeleteQuiz(q)"
                >
                  Supprimer
                </button>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </section>

    <div v-if="confirmDialog" class="modal-overlay" @click.self="closeConfirm">
      <div class="modal">
        <h3>{{ confirmTitle }}</h3>
        <p>{{ confirmMessage }}</p>
        <div class="modal-actions">
          <button type="button" class="btn btn-danger" :disabled="confirmLoading" @click="executeConfirm">
            {{ confirmLoading ? 'En cours…' : 'Confirmer' }}
          </button>
          <button type="button" class="btn btn-ghost" :disabled="confirmLoading" @click="closeConfirm">
            Annuler
          </button>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/auth.store'
import { adminService } from '@/services/admin.service'
import type { AdminUser, QuizSummary } from '@/types'

const auth = useAuthStore()
const router = useRouter()

const users = ref<AdminUser[]>([])
const quizzes = ref<QuizSummary[]>([])
const loadingUsers = ref(true)
const loadingQuizzes = ref(true)
const usersError = ref('')
const quizzesError = ref('')
const togglingUserId = ref<string | null>(null)
const deletingUserId = ref<string | null>(null)
const deletingQuizId = ref<string | null>(null)

const confirmDialog = ref(false)
const confirmTitle = ref('')
const confirmMessage = ref('')
const confirmLoading = ref(false)
type ConfirmAction = () => Promise<void>
let confirmAction: ConfirmAction = async () => {}

function formatDate(s: string): string {
  try {
    return new Date(s).toLocaleDateString('fr-FR', {
      day: '2-digit',
      month: '2-digit',
      year: 'numeric',
    })
  } catch {
    return s
  }
}

async function loadUsers() {
  loadingUsers.value = true
  usersError.value = ''
  try {
    users.value = await adminService.getUsers(auth.token ?? undefined)
  } catch (e) {
    usersError.value = e instanceof Error ? e.message : 'Erreur chargement utilisateurs'
    if (String(usersError.value).includes('401')) {
      auth.logout()
      router.push('/login')
    }
  } finally {
    loadingUsers.value = false
  }
}

async function loadQuizzes() {
  loadingQuizzes.value = true
  quizzesError.value = ''
  try {
    quizzes.value = await adminService.getQuizzes(auth.token ?? undefined)
  } catch (e) {
    quizzesError.value = e instanceof Error ? e.message : 'Erreur chargement quiz'
  } finally {
    loadingQuizzes.value = false
  }
}

async function toggleAdmin(u: AdminUser) {
  togglingUserId.value = u.id
  try {
    await adminService.setUserAdmin(u.id, !u.isAdmin, auth.token ?? undefined)
    u.isAdmin = !u.isAdmin
  } catch (e) {
    alert(e instanceof Error ? e.message : 'Erreur')
  } finally {
    togglingUserId.value = null
  }
}

function confirmDeleteUser(u: AdminUser) {
  confirmTitle.value = 'Supprimer l\'utilisateur'
  confirmMessage.value = `Supprimer définitivement "${u.pseudo}" (${u.email}) ? Ses quiz et parties seront aussi supprimés.`
  confirmAction = async () => {
    deletingUserId.value = u.id
    await adminService.deleteUser(u.id, auth.token ?? undefined)
    users.value = users.value.filter((x) => x.id !== u.id)
    closeConfirm()
    deletingUserId.value = null
  }
  confirmDialog.value = true
}

function confirmDeleteQuiz(q: QuizSummary) {
  confirmTitle.value = 'Supprimer le quiz'
  confirmMessage.value = `Supprimer définitivement le quiz "${q.title}" ?`
  confirmAction = async () => {
    deletingQuizId.value = q.id
    await adminService.deleteQuiz(q.id, auth.token ?? undefined)
    quizzes.value = quizzes.value.filter((x) => x.id !== q.id)
    closeConfirm()
    deletingQuizId.value = null
  }
  confirmDialog.value = true
}

async function executeConfirm() {
  confirmLoading.value = true
  try {
    await confirmAction()
  } catch (e) {
    alert(e instanceof Error ? e.message : 'Erreur')
  } finally {
    confirmLoading.value = false
  }
}

function closeConfirm() {
  confirmDialog.value = false
  confirmTitle.value = ''
  confirmMessage.value = ''
  confirmAction = async () => {}
}

onMounted(() => {
  if (!auth.user?.isAdmin) {
    router.replace('/dashboard')
    return
  }
  loadUsers()
  loadQuizzes()
})
</script>

<style scoped>
.admin {
  max-width: 100%;
}
.admin-section {
  margin-bottom: 2rem;
}
.admin-section h2 {
  margin-bottom: 0.75rem;
  font-size: 1.1rem;
}
.table-wrap {
  overflow-x: auto;
}
.admin-table {
  width: 100%;
  border-collapse: collapse;
}
.admin-table th,
.admin-table td {
  padding: 0.5rem 0.75rem;
  text-align: left;
  border-bottom: 1px solid var(--color-border);
}
.admin-table th {
  font-weight: 600;
  color: var(--color-text-muted);
  font-size: 0.85rem;
}
.admin-table code {
  font-size: 0.8rem;
  word-break: break-all;
}
.badge.you {
  background: var(--color-primary);
  color: white;
  padding: 0.2rem 0.5rem;
  border-radius: var(--radius);
  font-size: 0.8rem;
}
.btn-small {
  padding: 0.25rem 0.5rem;
  font-size: 0.85rem;
  margin-right: 0.25rem;
}
.btn-danger {
  background: #c53030;
  color: white;
}
.btn-danger:hover {
  background: #9b2c2c;
}
.loading,
.error {
  padding: 1rem;
}
.error {
  color: #e53e3e;
}
.modal-overlay {
  position: fixed;
  inset: 0;
  background: rgba(0, 0, 0, 0.5);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 100;
}
.modal {
  background: var(--color-surface);
  padding: 1.5rem;
  border-radius: var(--radius);
  max-width: 400px;
  width: 90%;
}
.modal h3 {
  margin-bottom: 0.5rem;
}
.modal-actions {
  margin-top: 1rem;
  display: flex;
  gap: 0.5rem;
}
</style>
