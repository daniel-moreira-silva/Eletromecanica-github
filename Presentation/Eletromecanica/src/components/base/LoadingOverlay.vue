<template>
  <div v-if="active" class="loading-overlay">
    <fingerprint-spinner :size="128" color="#0052CC" :animation-duration="1500" />
  </div>
</template>

<script>
import { FingerprintSpinner } from 'epic-spinners'

export default {
  name: 'LoadingOverlay',
  components: { FingerprintSpinner },
  props: {
    active: {
      type: Boolean,
      required: true
    }
  }
}
</script>

<style scoped></style>
