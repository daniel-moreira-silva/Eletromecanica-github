<template>
  <v-container fluid>
    <v-row>
      <v-col cols="12" md="3" v-for="(kpi, i) in kpis" :key="i">
        <v-card color="primary" dark>
          <v-card-title>{{ kpi.label }}</v-card-title>
          <v-card-subtitle class="text-h5">{{ kpi.value }}</v-card-subtitle>
        </v-card>
      </v-col>
    </v-row>

    <v-row>
      <v-col cols="12" md="6">
        <v-card>
          <v-card-title>Ocorrências por Categoria</v-card-title>
          <v-card-text>
            <canvas ref="chartCategoria" height="200"></canvas>
          </v-card-text>
        </v-card>
      </v-col>
      <v-col cols="12" md="6">
        <v-card>
          <v-card-title>Ocorrências por Status</v-card-title>
          <v-card-text>
            <canvas ref="chartStatus" height="200"></canvas>
          </v-card-text>
        </v-card>
      </v-col>
    </v-row>

    <v-row>
      <v-col cols="12">
        <v-card>
          <v-card-title>Últimas Ocorrências</v-card-title>
          <v-table dense>
            <thead>
              <tr>
                <th>Data</th>
                <th>Categoria</th>
                <th>Status</th>
                <th>Tempo de Atendimento</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="(item, i) in ultimasOcorrencias" :key="i">
                <td>{{ item.data }}</td>
                <td>{{ item.categoria }}</td>
                <td>{{ item.status }}</td>
                <td>{{ item.tempo }}</td>
              </tr>
            </tbody>
          </v-table>
        </v-card>
      </v-col>
    </v-row>
  </v-container>
</template>

<script setup>
import { onMounted, ref } from 'vue'
import Chart from 'chart.js/auto'

const chartCategoria = ref(null)
const chartStatus = ref(null)

const kpis = [
  { label: 'Ocorrências Abertas', value: 134 },
  { label: 'Ocorrências Encerradas', value: 89 },
  { label: 'Tempo Médio Atendimento', value: '2h 35min' },
  { label: 'Satisfação Geral', value: '93%' },
]

const ultimasOcorrencias = [
  { data: '25/07/2025', categoria: 'Falta d’água', status: 'Aberta', tempo: '-' },
  { data: '24/07/2025', categoria: 'Vazamento', status: 'Encerrada', tempo: '3h 12min' },
  { data: '24/07/2025', categoria: 'Esgoto', status: 'Em andamento', tempo: '1h 5min' },
]

onMounted(() => {
  new Chart(chartCategoria.value, {
    type: 'doughnut',
    data: {
      labels: ['Falta d’água', 'Vazamento', 'Esgoto', 'Qualidade da água'],
      datasets: [{
        data: [45, 30, 15, 10],
        backgroundColor: ['#2196f3', '#ff9800', '#4caf50', '#9c27b0']
      }]
    }
  })

  new Chart(chartStatus.value, {
    type: 'bar',
    data: {
      labels: ['Aberta', 'Em andamento', 'Encerrada'],
      datasets: [{
        label: 'Ocorrências',
        data: [55, 34, 89],
        backgroundColor: ['#f44336', '#ff9800', '#4caf50']
      }]
    }
  })
})
</script>
