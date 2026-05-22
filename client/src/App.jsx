import { useEffect, useMemo, useState } from 'react';
import { Activity, LogOut, RefreshCw } from 'lucide-react';
import {
  archiveMedication,
  createMedication,
  listMedications,
  listTodayIntakes,
  recordIntake,
  updateMedication,
} from './api/medicationsApi.js';
import { MedicationForm } from './components/MedicationForm.jsx';
import { MedicationList } from './components/MedicationList.jsx';
import { TodayIntakes } from './components/TodayIntakes.jsx';
import { AuthScreen } from './components/AuthScreen.jsx';
import { everyDay } from './constants/scheduleDays.js';

function todayInputValue() {
  return new Date().toISOString().slice(0, 10);
}

const initialForm = {
  name: '',
  dosage: '',
  instructions: '',
  stockQuantity: 30,
  lowStockThreshold: 5,
  startDate: todayInputValue(),
  endDate: '',
  isActive: true,
  schedules: [{ time: '08:00', days: everyDay }],
};

export function App() {
  const [auth, setAuth] = useState(() => {
    const token = localStorage.getItem('medcontrol.token');
    const user = localStorage.getItem('medcontrol.user');
    return token && user ? { token, user: JSON.parse(user) } : null;
  });
  const [medications, setMedications] = useState([]);
  const [intakes, setIntakes] = useState([]);
  const [form, setForm] = useState(initialForm);
  const [editingId, setEditingId] = useState(null);
  const [isLoading, setIsLoading] = useState(true);
  const [isSaving, setIsSaving] = useState(false);
  const [error, setError] = useState('');

  const lowStockCount = useMemo(
    () => medications.filter((medication) => medication.isLowStock).length,
    [medications]
  );

  async function loadData() {
    setError('');
    setIsLoading(true);
    try {
      const [medicationsResult, intakesResult] = await Promise.all([
        listMedications(),
        listTodayIntakes(),
      ]);
      setMedications(medicationsResult);
      setIntakes(intakesResult);
    } catch (exception) {
      setError(exception.message);
    } finally {
      setIsLoading(false);
    }
  }

  useEffect(() => {
    if (auth) {
      loadData();
    }
  }, [auth]);

  function handleAuthenticated(result) {
    localStorage.setItem('medcontrol.token', result.token);
    localStorage.setItem('medcontrol.user', JSON.stringify(result.user));
    setAuth(result);
  }

  function logout() {
    localStorage.removeItem('medcontrol.token');
    localStorage.removeItem('medcontrol.user');
    setAuth(null);
    setMedications([]);
    setIntakes([]);
  }

  if (!auth) {
    return <AuthScreen onAuthenticated={handleAuthenticated} />;
  }

  function resetForm() {
    setForm(initialForm);
    setEditingId(null);
  }

  function editMedication(medication) {
    setEditingId(medication.id);
    setForm({
      name: medication.name,
      dosage: medication.dosage,
      instructions: medication.instructions ?? '',
      stockQuantity: medication.stockQuantity,
      lowStockThreshold: medication.lowStockThreshold,
      startDate: medication.startDate,
      endDate: medication.endDate ?? '',
      isActive: medication.isActive,
      schedules: medication.schedules.map((schedule) => ({
        time: schedule.time,
        days: schedule.days,
      })),
    });
  }

  async function submitMedication(event) {
    event.preventDefault();
    setIsSaving(true);
    setError('');

    const payload = {
      ...form,
      endDate: form.endDate || null,
      schedules: form.schedules.map((schedule) => ({
        time: schedule.time,
        days: Number(schedule.days),
      })),
    };

    try {
      if (editingId) {
        await updateMedication(editingId, payload);
      } else {
        await createMedication(payload);
      }
      resetForm();
      await loadData();
    } catch (exception) {
      setError(exception.message);
    } finally {
      setIsSaving(false);
    }
  }

  async function handleArchive(id) {
    await archiveMedication(id);
    await loadData();
  }

  async function handleRecord(intake, status) {
    await recordIntake(intake.medicationId, {
      scheduledFor: intake.scheduledFor,
      status,
      note: null,
    });
    await loadData();
  }

  return (
    <main className="app-shell">
      <header className="app-header">
        <div>
          <span className="eyebrow">MedControl</span>
          <h1>Controle de medicamentos</h1>
          <p>Cadastre remédios, acompanhe horários e reduza esquecimentos no dia a dia.</p>
        </div>
        <button className="secondary-button" type="button" onClick={loadData} disabled={isLoading}>
          <RefreshCw size={18} />
          Atualizar
        </button>
        <button className="secondary-button" type="button" onClick={logout}>
          <LogOut size={18} />
          Sair
        </button>
      </header>

      <section className="summary-grid">
        <div className="summary-item">
          <Activity size={19} />
          <div>
            <span>{medications.length}</span>
            <p>medicamentos ativos</p>
          </div>
        </div>
        <div className="summary-item warning">
          <div className="summary-dot" />
          <div>
            <span>{lowStockCount}</span>
            <p>com estoque baixo</p>
          </div>
        </div>
        <div className="summary-item">
          <div className="summary-dot green" />
          <div>
            <span>{intakes.length}</span>
            <p>doses para hoje</p>
          </div>
        </div>
      </section>

      {error && <div className="alert">{error}</div>}

      <div className="content-grid">
        <MedicationForm
          form={form}
          setForm={setForm}
          editingId={editingId}
          onSubmit={submitMedication}
          onCancel={resetForm}
          isSaving={isSaving}
        />
        <div className="side-stack">
          <TodayIntakes intakes={intakes} onRecord={handleRecord} />
          <MedicationList medications={medications} onEdit={editMedication} onArchive={handleArchive} />
        </div>
      </div>
    </main>
  );
}
