<template>
  <div class="not-found">
    <h1>404</h1>
    <p>Page introuvable.</p>
    <RouterLink to="/" class="btn btn-primary">Retour à l'accueil</RouterLink>
  </div>
</template>

<script setup lang="ts">
</script>

<style scoped>
.not-found {
  text-align: center;
  padding: 4rem 1rem;
}

.not-found h1 {
  font-size: 4rem;
  margin: 0 0 0.5rem;
  color: var(--color-text-muted);
}

.not-found p {
  margin-bottom: 1.5rem;
  color: var(--color-text-muted);
}

.btn {
  padding: 0.75rem 1.5rem;
  border-radius: var(--radius);
  font-weight: 600;
  border: none;
  background: var(--color-primary);
  color: white;
}
</style>
