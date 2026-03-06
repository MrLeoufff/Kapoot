<template>
  <div class="quiz-edit">
    <div v-if="loading" class="loading">Chargement…</div>
    <template v-else-if="quiz">
      <div class="header">
        <RouterLink to="/dashboard" class="back">← Tableau de bord</RouterLink>
        <h1>{{ quiz.title || 'Sans titre' }}</h1>
        <span class="status" :class="quiz.status === 1 ? 'published' : 'draft'">
          {{ quiz.status === 1 ? 'Publié' : 'Brouillon' }}
        </span>
      </div>

      <form class="quiz-form" @submit.prevent="saveQuiz">
        <p v-if="quizError" class="error">{{ quizError }}</p>
        <div class="field">
          <label for="title">Titre</label>
          <input id="title" v-model="editTitle" type="text" required />
        </div>
        <div class="field">
          <label for="description">Description</label>
          <textarea id="description" v-model="editDescription" rows="2"></textarea>
        </div>
        <button type="submit" class="btn btn-primary" :disabled="saving">Enregistrer</button>
      </form>

      <section class="questions">
        <h2>Questions</h2>
        <ul class="question-list">
          <li v-for="(q, index) in quiz.questions" :key="q.id" class="question-card">
            <div class="question-header">
              <span class="order">{{ index + 1 }}.</span>
              <span class="text">{{ q.text }}</span>
              <span class="type-label">{{ typeLabel(q.type) }}</span>
              <button type="button" class="btn btn-small" @click="startEditQuestion(q)">Modifier</button>
              <button type="button" class="btn btn-small btn-danger" @click="confirmDelete(q)">Supprimer</button>
            </div>
            <ul v-if="q.choices.length" class="choices-preview">
              <li v-for="c in q.choices" :key="c.id">
                {{ c.text }} <span v-if="c.isCorrect" class="correct">✓</span>
              </li>
            </ul>
          </li>
        </ul>

        <div v-if="showAddQuestion" class="add-question-form">
          <h3>Nouvelle question</h3>
          <p v-if="questionError" class="error">{{ questionError }}</p>
          <div class="field">
            <label>Énoncé</label>
            <input v-model="newQuestion.text" type="text" required placeholder="Texte de la question" />
          </div>
          <div class="field">
            <label>Explication (optionnel)</label>
            <input v-model="newQuestion.explanation" type="text" placeholder="Explication après la réponse" />
          </div>
          <div class="field">
            <label>Choix (4 par défaut, plusieurs bonnes réponses possibles)</label>
            <p class="field-hint">Cochez toutes les réponses correctes. Vous pouvez avoir une ou plusieurs bonnes réponses.</p>
            <div v-for="(choice, i) in newQuestion.choices" :key="i" class="choice-row">
              <input v-model="choice.text" type="text" :placeholder="`Réponse ${i + 1}`" />
              <label class="checkbox">
                <input v-model="choice.isCorrect" type="checkbox" /> Bonne réponse
              </label>
            </div>
            <button type="button" class="btn btn-ghost btn-small" @click="addChoice">+ Ajouter un choix</button>
          </div>
          <div class="form-actions">
            <button type="button" class="btn btn-primary" :disabled="addingQuestion" @click="submitQuestion">Ajouter</button>
            <button type="button" class="btn btn-ghost" @click="cancelAddQuestion">Annuler</button>
          </div>
        </div>
        <button v-else type="button" class="btn btn-outline" @click="showAddQuestion = true">+ Ajouter une question</button>
      </section>

      <section v-if="quiz.status === 0" class="publish-section">
        <button type="button" class="btn btn-primary btn-large" :disabled="publishing" @click="publishQuiz">
          Publier le quiz
        </button>
        <p class="muted">Un quiz doit avoir au moins une question pour être publié.</p>
      </section>

      <div v-if="questionToDelete" class="modal-overlay" @click.self="questionToDelete = null">
        <div class="modal">
          <p>Supprimer la question « {{ questionToDelete.text }} » ?</p>
          <div class="modal-actions">
            <button type="button" class="btn btn-danger" @click="deleteQuestion">Supprimer</button>
            <button type="button" class="btn btn-ghost" @click="questionToDelete = null">Annuler</button>
          </div>
        </div>
      </div>

      <div v-if="editingQuestion" class="modal-overlay" @click.self="editingQuestion = null">
        <div class="modal edit-modal">
          <h3>Modifier la question</h3>
          <div class="field">
            <label>Énoncé</label>
            <input v-model="editQuestionForm.text" type="text" required />
          </div>
          <div class="field">
            <label>Explication</label>
            <input v-model="editQuestionForm.explanation" type="text" />
          </div>
          <div class="field">
            <label>Choix (plusieurs bonnes réponses possibles)</label>
            <div v-for="(choice, i) in editQuestionForm.choices" :key="i" class="choice-row">
              <input v-model="choice.text" type="text" :placeholder="`Réponse ${i + 1}`" />
              <label class="checkbox"><input v-model="choice.isCorrect" type="checkbox" /> Bonne réponse</label>
            </div>
            <button type="button" class="btn btn-ghost btn-small" @click="addChoiceToEdit">+ Ajouter un choix</button>
          </div>
          <div class="modal-actions">
            <button type="button" class="btn btn-primary" :disabled="savingQuestion" @click="saveEditQuestion">Enregistrer</button>
            <button type="button" class="btn btn-ghost" @click="editingQuestion = null">Annuler</button>
          </div>
        </div>
      </div>
    </template>
    <p v-else class="error">Quiz introuvable.</p>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, watch } from 'vue'
import { useRoute } from 'vue-router'
import { quizService } from '@/services/quiz.service'
import { questionService } from '@/services/question.service'
import type { QuizDetail, QuestionDetail } from '@/types'
import type { ChoiceInput } from '@/services/question.service'

const route = useRoute()
const quizId = computed(() => route.params.id as string)

const quiz = ref<QuizDetail | null>(null)
const loading = ref(true)
const saving = ref(false)
const quizError = ref('')
const editTitle = ref('')
const editDescription = ref('')

const showAddQuestion = ref(false)
const questionError = ref('')
const addingQuestion = ref(false)
const newQuestion = ref({
  text: '',
  explanation: '' as string | null,
  choices: [
    { text: '', isCorrect: true, order: 0 },
    { text: '', isCorrect: false, order: 1 },
  ] as ChoiceInput[],
})

const editingQuestion = ref<QuestionDetail | null>(null)
const editQuestionForm = ref<{ text: string; explanation: string | null; order: number; choices: ChoiceInput[] }>({
  text: '',
  explanation: null,
  order: 0,
  choices: [],
})
const savingQuestion = ref(false)

const questionToDelete = ref<QuestionDetail | null>(null)
const publishing = ref(false)

function typeLabel(type: number): string {
  const labels: Record<number, string> = { 0: 'QCM', 1: 'Vrai/Faux', 2: 'Drag & drop' }
  return labels[type] ?? 'Question'
}

async function loadQuiz() {
  if (!quizId.value) return
  loading.value = true
  quizError.value = ''
  try {
    quiz.value = await quizService.getDetail(quizId.value)
    editTitle.value = quiz.value.title
    editDescription.value = quiz.value.description
  } catch {
    quiz.value = null
  } finally {
    loading.value = false
  }
}

watch(quizId, loadQuiz, { immediate: true })

async function saveQuiz() {
  if (!quiz.value) return
  saving.value = true
  quizError.value = ''
  try {
    await quizService.update(quiz.value.id, editTitle.value.trim(), editDescription.value.trim())
    quiz.value = { ...quiz.value, title: editTitle.value, description: editDescription.value }
  } catch (e) {
    quizError.value = e instanceof Error ? e.message : 'Erreur'
  } finally {
    saving.value = false
  }
}

function addChoice() {
  newQuestion.value.choices.push({
    text: '',
    isCorrect: false,
    order: newQuestion.value.choices.length,
  })
}

function resetNewQuestion() {
  newQuestion.value = {
    text: '',
    explanation: null,
    choices: [
      { text: '', isCorrect: false, order: 0 },
      { text: '', isCorrect: false, order: 1 },
      { text: '', isCorrect: false, order: 2 },
      { text: '', isCorrect: false, order: 3 },
    ],
  }
  questionError.value = ''
  showAddQuestion.value = false
}

function addChoiceToEdit() {
  const n = editQuestionForm.value.choices.length
  editQuestionForm.value.choices.push({
    text: '',
    isCorrect: false,
    order: n,
  })
}

async function submitQuestion() {
  if (!quiz.value) return
  const q = newQuestion.value
  if (!q.text.trim()) {
    questionError.value = 'L’énoncé est obligatoire.'
    return
  }
  const validChoices = q.choices.filter((c) => c.text.trim()).map((c, i) => ({ ...c, order: i }))
  if (validChoices.length < 2) {
    questionError.value = 'Au moins deux réponses sont requises.'
    return
  }
  if (validChoices.length > 6) {
    questionError.value = 'Maximum 6 réponses par question.'
    return
  }
  if (!validChoices.some((c) => c.isCorrect)) {
    questionError.value = 'Indiquez au moins une bonne réponse.'
    return
  }
  addingQuestion.value = true
  questionError.value = ''
  try {
    await questionService.add(quiz.value.id, {
      text: q.text.trim(),
      type: 0,
      explanation: q.explanation?.trim() || null,
      order: quiz.value.questions.length,
      choices: validChoices,
    })
    await loadQuiz()
    resetNewQuestion()
  } catch (e) {
    questionError.value = e instanceof Error ? e.message : 'Erreur'
  } finally {
    addingQuestion.value = false
  }
}

function cancelAddQuestion() {
  resetNewQuestion()
}

function startEditQuestion(q: QuestionDetail) {
  editingQuestion.value = q
  editQuestionForm.value = {
    text: q.text,
    explanation: q.explanation ?? null,
    order: q.order,
    choices: q.choices.map((c) => ({ text: c.text, isCorrect: c.isCorrect, order: c.order })),
  }
}

async function saveEditQuestion() {
  if (!editingQuestion.value || !quiz.value) return
  savingQuestion.value = true
  try {
    await questionService.update(editingQuestion.value.id, {
      text: editQuestionForm.value.text,
      explanation: editQuestionForm.value.explanation,
      order: editQuestionForm.value.order,
      choices: editQuestionForm.value.choices,
    })
    await loadQuiz()
    editingQuestion.value = null
  } finally {
    savingQuestion.value = false
  }
}

function confirmDelete(q: QuestionDetail) {
  questionToDelete.value = q
}

async function deleteQuestion() {
  if (!questionToDelete.value) return
  try {
    await questionService.remove(questionToDelete.value.id)
    await loadQuiz()
    questionToDelete.value = null
  } catch {
    questionToDelete.value = null
  }
}

async function publishQuiz() {
  if (!quiz.value || quiz.value.status === 1) return
  if (quiz.value.questions.length === 0) return
  publishing.value = true
  try {
    await quizService.publish(quiz.value.id)
    await loadQuiz()
  } finally {
    publishing.value = false
  }
}
</script>

<style scoped>
.quiz-edit {
  max-width: 720px;
}

.header {
  display: flex;
  align-items: center;
  gap: 0.75rem;
  flex-wrap: wrap;
  margin-bottom: 1.5rem;
}

.back {
  color: var(--color-text-muted);
  font-size: 0.9rem;
}

.back:hover {
  color: var(--color-primary);
}

.header h1 {
  margin: 0;
  font-size: 1.5rem;
}

.status {
  font-size: 0.75rem;
  padding: 0.2rem 0.5rem;
  border-radius: 4px;
}

.status.draft {
  background: rgba(255, 255, 255, 0.1);
  color: var(--color-text-muted);
}

.status.published {
  background: rgba(34, 197, 94, 0.2);
  color: var(--color-success);
}

.quiz-form {
  margin-bottom: 2rem;
}

.quiz-form .field {
  margin-bottom: 1rem;
}

.quiz-form label {
  display: block;
  margin-bottom: 0.25rem;
  font-weight: 500;
  color: var(--color-text-muted);
}

.field-hint {
  font-size: 0.85rem;
  color: var(--color-text-muted);
  margin: 0 0 0.5rem;
}

.quiz-form input,
.quiz-form textarea {
  width: 100%;
  padding: 0.75rem;
  border: 1px solid var(--color-border);
  border-radius: var(--radius);
  background: var(--color-bg);
  color: var(--color-text);
}

.btn {
  padding: 0.5rem 1rem;
  border-radius: var(--radius);
  font-weight: 500;
  border: none;
  cursor: pointer;
}

.btn-primary {
  background: var(--color-primary);
  color: white;
}

.btn-outline {
  background: transparent;
  color: var(--color-primary);
  border: 1px solid var(--color-primary);
}

.btn-ghost {
  background: transparent;
  color: var(--color-text-muted);
}

.btn-danger {
  background: rgba(239, 68, 68, 0.2);
  color: var(--color-error);
}

.btn-small {
  font-size: 0.85rem;
  padding: 0.35rem 0.65rem;
}

.btn-large {
  padding: 0.75rem 1.5rem;
  font-size: 1rem;
}

.questions h2 {
  font-size: 1.25rem;
  margin-bottom: 1rem;
}

.question-list {
  list-style: none;
  padding: 0;
  margin: 0 0 1.5rem;
}

.question-card {
  background: var(--color-surface);
  border: 1px solid var(--color-border);
  border-radius: var(--radius);
  padding: 1rem;
  margin-bottom: 0.75rem;
}

.question-header {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  flex-wrap: wrap;
}

.question-header .order {
  color: var(--color-text-muted);
  min-width: 1.5rem;
}

.question-header .text {
  flex: 1;
  min-width: 0;
}

.type-label {
  font-size: 0.75rem;
  color: var(--color-text-muted);
}

.choices-preview {
  list-style: none;
  padding: 0;
  margin: 0.5rem 0 0 2rem;
  font-size: 0.9rem;
  color: var(--color-text-muted);
}

.choices-preview .correct {
  color: var(--color-success);
}

.add-question-form {
  background: var(--color-surface);
  border: 1px solid var(--color-border);
  border-radius: var(--radius);
  padding: 1.25rem;
  margin-bottom: 1rem;
}

.add-question-form .field,
.edit-modal .field {
  margin-bottom: 1rem;
}

.add-question-form label,
.edit-modal label {
  display: block;
  margin-bottom: 0.25rem;
  font-weight: 500;
  color: var(--color-text-muted);
}

.add-question-form input,
.edit-modal input {
  width: 100%;
  padding: 0.5rem 0.75rem;
  border: 1px solid var(--color-border);
  border-radius: var(--radius);
  background: var(--color-bg);
  color: var(--color-text);
}

.choice-row {
  display: flex;
  align-items: center;
  gap: 0.75rem;
  margin-bottom: 0.5rem;
}

.choice-row input[type="text"] {
  flex: 1;
}

.checkbox {
  display: flex;
  align-items: center;
  gap: 0.35rem;
  font-weight: normal;
  white-space: nowrap;
}

.form-actions,
.modal-actions {
  display: flex;
  gap: 0.75rem;
  margin-top: 1rem;
}

.publish-section {
  margin-top: 2rem;
  padding-top: 1.5rem;
  border-top: 1px solid var(--color-border);
}

.publish-section .muted {
  margin-top: 0.5rem;
  font-size: 0.9rem;
}

.modal-overlay {
  position: fixed;
  inset: 0;
  background: rgba(0, 0, 0, 0.6);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 100;
}

.modal {
  background: var(--color-surface);
  border: 1px solid var(--color-border);
  border-radius: var(--radius);
  padding: 1.5rem;
  max-width: 480px;
  width: 90%;
}

.modal p {
  margin: 0 0 1rem;
}

.edit-modal .field {
  margin-bottom: 1rem;
}

.loading,
.muted,
.error {
  color: var(--color-text-muted);
}

.error {
  color: var(--color-error);
  margin-bottom: 0.5rem;
}
</style>
