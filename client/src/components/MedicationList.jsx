import { Archive, Edit3, Pill } from 'lucide-react';
import { formatDays } from '../constants/scheduleDays.js';

export function MedicationList({ medications, onEdit, onArchive }) {
  return (
    <section className="panel">
      <div className="panel-title">
        <div>
          <span className="eyebrow">Medicamentos</span>
          <h2>Tratamento ativo</h2>
        </div>
        <span className="count">{medications.length}</span>
      </div>

      <div className="medication-list">
        {medications.length === 0 && <p className="empty-state">Nenhum medicamento cadastrado ainda.</p>}
        {medications.map((medication) => (
          <article className="medication-card" key={medication.id}>
            <div className="medication-card-main">
              <div className="pill-icon">
                <Pill size={20} />
              </div>
              <div>
                <h3>{medication.name}</h3>
                <p>{medication.dosage}</p>
              </div>
            </div>

            <div className="meta-row">
              <span className={medication.isLowStock ? 'stock low' : 'stock'}>
                Estoque: {medication.stockQuantity}
              </span>
              {medication.schedules.map((schedule) => (
                <span key={schedule.id}>{schedule.time} · {formatDays(schedule.days)}</span>
              ))}
            </div>

            {medication.instructions && <p className="instructions">{medication.instructions}</p>}

            <div className="card-actions">
              <button className="icon-button" type="button" onClick={() => onEdit(medication)} aria-label="Editar" title="Editar">
                <Edit3 size={17} />
              </button>
              <button
                className="icon-button quiet"
                type="button"
                onClick={() => onArchive(medication.id)}
                aria-label="Arquivar"
                title="Arquivar"
              >
                <Archive size={17} />
              </button>
            </div>
          </article>
        ))}
      </div>
    </section>
  );
}
