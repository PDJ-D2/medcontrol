import { api } from './http.js';

export function listMedications() {
  return api('/api/medications');
}

export function createMedication(payload) {
  return api('/api/medications', {
    method: 'POST',
    body: JSON.stringify(payload),
  });
}

export function updateMedication(id, payload) {
  return api(`/api/medications/${id}`, {
    method: 'PUT',
    body: JSON.stringify(payload),
  });
}

export function archiveMedication(id) {
  return api(`/api/medications/${id}`, {
    method: 'DELETE',
  });
}

export function listTodayIntakes() {
  return api('/api/intakes/today');
}

export function recordIntake(medicationId, payload) {
  return api(`/api/intakes/medications/${medicationId}`, {
    method: 'POST',
    body: JSON.stringify(payload),
  });
}
