export const scheduleDays = [
  { label: 'D', short: 'Dom', value: 1 },
  { label: 'S', short: 'Seg', value: 2 },
  { label: 'T', short: 'Ter', value: 4 },
  { label: 'Q', short: 'Qua', value: 8 },
  { label: 'Q', short: 'Qui', value: 16 },
  { label: 'S', short: 'Sex', value: 32 },
  { label: 'S', short: 'Sab', value: 64 },
];

export const everyDay = 127;

export function formatDays(days) {
  if (days === everyDay) {
    return 'Todos os dias';
  }

  return scheduleDays
    .filter((day) => (days & day.value) === day.value)
    .map((day) => day.short)
    .join(', ');
}
