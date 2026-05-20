import { Check, Clock, SkipForward } from 'lucide-react';

export function TodayIntakes({ intakes, onRecord }) {
  return (
    <section className="panel">
      <div className="panel-title">
        <div>
          <span className="eyebrow">Hoje</span>
          <h2>Próximas doses</h2>
        </div>
        <Clock size={20} />
      </div>

      <div className="intake-list">
        {intakes.length === 0 && <p className="empty-state">Nada agendado para hoje.</p>}
        {intakes.map((intake) => {
          const time = new Date(intake.scheduledFor).toLocaleTimeString('pt-BR', {
            hour: '2-digit',
            minute: '2-digit',
          });

          return (
            <article className="intake-row" key={`${intake.medicationId}-${intake.scheduledFor}`}>
              <div>
                <strong>{time}</strong>
                <h3>{intake.medicationName}</h3>
                <p>{intake.dosage}</p>
              </div>
              <div className="intake-actions">
                <span className={intake.status ? `status ${intake.status.toLowerCase()}` : 'status'}>
                  {intake.status === 'Taken' ? 'Tomado' : intake.status === 'Skipped' ? 'Pulado' : 'Pendente'}
                </span>
                <button
                  className="icon-button success"
                  type="button"
                  onClick={() => onRecord(intake, 'Taken')}
                  aria-label="Marcar como tomado"
                  title="Marcar como tomado"
                >
                  <Check size={17} />
                </button>
                <button
                  className="icon-button quiet"
                  type="button"
                  onClick={() => onRecord(intake, 'Skipped')}
                  aria-label="Marcar como pulado"
                  title="Marcar como pulado"
                >
                  <SkipForward size={17} />
                </button>
              </div>
            </article>
          );
        })}
      </div>
    </section>
  );
}
