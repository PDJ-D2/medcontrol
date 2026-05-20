import { Plus, Save, X } from 'lucide-react';
import { everyDay, scheduleDays } from '../constants/scheduleDays.js';

const emptySchedule = { time: '08:00', days: everyDay };

export function MedicationForm({ form, setForm, editingId, onSubmit, onCancel, isSaving }) {
  function updateField(field, value) {
    setForm((current) => ({ ...current, [field]: value }));
  }

  function updateSchedule(index, field, value) {
    setForm((current) => ({
      ...current,
      schedules: current.schedules.map((schedule, scheduleIndex) =>
        scheduleIndex === index ? { ...schedule, [field]: value } : schedule
      ),
    }));
  }

  function toggleDay(index, value) {
    const currentDays = form.schedules[index].days;
    const nextDays = (currentDays & value) === value ? currentDays & ~value : currentDays | value;
    updateSchedule(index, 'days', nextDays || everyDay);
  }

  function addSchedule() {
    setForm((current) => ({
      ...current,
      schedules: [...current.schedules, emptySchedule],
    }));
  }

  function removeSchedule(index) {
    setForm((current) => ({
      ...current,
      schedules: current.schedules.filter((_, scheduleIndex) => scheduleIndex !== index),
    }));
  }

  return (
    <form className="panel medication-form" onSubmit={onSubmit}>
      <div className="panel-title">
        <div>
          <span className="eyebrow">Cadastro</span>
          <h2>{editingId ? 'Editar medicamento' : 'Novo medicamento'}</h2>
        </div>
      </div>

      <div className="form-grid">
        <label>
          Nome
          <input value={form.name} onChange={(event) => updateField('name', event.target.value)} required />
        </label>
        <label>
          Dosagem
          <input value={form.dosage} onChange={(event) => updateField('dosage', event.target.value)} required />
        </label>
        <label>
          Estoque
          <input
            type="number"
            min="0"
            value={form.stockQuantity}
            onChange={(event) => updateField('stockQuantity', Number(event.target.value))}
          />
        </label>
        <label>
          Alerta mínimo
          <input
            type="number"
            min="0"
            value={form.lowStockThreshold}
            onChange={(event) => updateField('lowStockThreshold', Number(event.target.value))}
          />
        </label>
        <label>
          Início
          <input type="date" value={form.startDate} onChange={(event) => updateField('startDate', event.target.value)} />
        </label>
        <label>
          Término
          <input type="date" value={form.endDate} onChange={(event) => updateField('endDate', event.target.value)} />
        </label>
      </div>

      <label>
        Instruções
        <textarea
          rows="3"
          value={form.instructions}
          onChange={(event) => updateField('instructions', event.target.value)}
          placeholder="Ex.: tomar após o café da manhã"
        />
      </label>

      <div className="schedule-header">
        <h3>Horários</h3>
        <button className="icon-button" type="button" onClick={addSchedule} aria-label="Adicionar horário" title="Adicionar horário">
          <Plus size={18} />
        </button>
      </div>

      <div className="schedule-list">
        {form.schedules.map((schedule, index) => (
          <div className="schedule-row" key={`${schedule.time}-${index}`}>
            <input
              type="time"
              value={schedule.time}
              onChange={(event) => updateSchedule(index, 'time', event.target.value)}
              aria-label="Horário"
            />
            <div className="day-toggle-group">
              {scheduleDays.map((day) => (
                <button
                  className={(schedule.days & day.value) === day.value ? 'day-toggle active' : 'day-toggle'}
                  key={day.short}
                  type="button"
                  title={day.short}
                  onClick={() => toggleDay(index, day.value)}
                >
                  {day.label}
                </button>
              ))}
            </div>
            {form.schedules.length > 1 && (
              <button
                className="icon-button quiet"
                type="button"
                onClick={() => removeSchedule(index)}
                aria-label="Remover horário"
                title="Remover horário"
              >
                <X size={18} />
              </button>
            )}
          </div>
        ))}
      </div>

      <div className="form-actions">
        {editingId && (
          <button className="secondary-button" type="button" onClick={onCancel}>
            Cancelar
          </button>
        )}
        <button className="primary-button" type="submit" disabled={isSaving}>
          <Save size={18} />
          {isSaving ? 'Salvando...' : 'Salvar'}
        </button>
      </div>
    </form>
  );
}
