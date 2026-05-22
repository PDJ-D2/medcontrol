import { useState } from 'react';
import { KeyRound, LogIn, Mail, UserPlus } from 'lucide-react';
import { forgotPassword, login, register, resetPassword } from '../api/authApi.js';
import { isStrongPassword, PasswordStrength } from './PasswordStrength.jsx';

export function AuthScreen({ onAuthenticated }) {
  const params = new URLSearchParams(window.location.search);
  const isResetRoute = window.location.pathname === '/reset-password';
  const [mode, setMode] = useState(isResetRoute ? 'reset' : 'login');
  const [message, setMessage] = useState('');
  const [error, setError] = useState('');
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [form, setForm] = useState({
    email: params.get('email') ?? '',
    userName: '',
    emailOrUserName: '',
    password: '',
    newPassword: '',
    token: params.get('token') ?? '',
  });

  function update(field, value) {
    setForm((current) => ({ ...current, [field]: value }));
  }

  async function submit(event) {
    event.preventDefault();
    setError('');
    setMessage('');
    setIsSubmitting(true);

    try {
      if (mode === 'register') {
        if (!isStrongPassword(form.password)) {
          throw new Error('A senha precisa cumprir todos os requisitos de seguranca.');
        }

        const result = await register({
          email: form.email,
          userName: form.userName,
          password: form.password,
        });
        onAuthenticated(result);
      }

      if (mode === 'login') {
        const result = await login({
          emailOrUserName: form.emailOrUserName,
          password: form.password,
        });
        onAuthenticated(result);
      }

      if (mode === 'forgot') {
        const result = await forgotPassword({ email: form.email });
        setMessage(result.message);
      }

      if (mode === 'reset') {
        if (!isStrongPassword(form.newPassword)) {
          throw new Error('A nova senha precisa cumprir todos os requisitos de seguranca.');
        }

        const result = await resetPassword({
          email: form.email,
          token: form.token,
          newPassword: form.newPassword,
        });
        setMessage(result.message);
        window.history.replaceState({}, '', '/');
        setMode('login');
      }
    } catch (exception) {
      setError(exception.message);
    } finally {
      setIsSubmitting(false);
    }
  }

  return (
    <main className="auth-shell">
      <section className="auth-hero">
        <span className="eyebrow">MedControl</span>
        <h1>Entre com seguranca para cuidar da sua rotina de medicamentos.</h1>
        <p>Seus remedios, horarios e registros ficam vinculados somente a sua conta.</p>
      </section>

      <form className="panel auth-panel" onSubmit={submit}>
        <div className="auth-tabs">
          <button className={mode === 'login' ? 'active' : ''} type="button" onClick={() => setMode('login')}>
            Login
          </button>
          <button className={mode === 'register' ? 'active' : ''} type="button" onClick={() => setMode('register')}>
            Cadastro
          </button>
        </div>

        {mode === 'login' && (
          <>
            <div className="panel-title">
              <div>
                <span className="eyebrow">Acesso</span>
                <h2>Entrar</h2>
              </div>
              <LogIn size={20} />
            </div>
            <label>
              Email ou usuario
              <input value={form.emailOrUserName} onChange={(event) => update('emailOrUserName', event.target.value)} required />
            </label>
            <label>
              Senha
              <input type="password" value={form.password} onChange={(event) => update('password', event.target.value)} required />
            </label>
            <button className="link-button" type="button" onClick={() => setMode('forgot')}>
              Esqueci minha senha
            </button>
          </>
        )}

        {mode === 'register' && (
          <>
            <div className="panel-title">
              <div>
                <span className="eyebrow">Nova conta</span>
                <h2>Criar usuario</h2>
              </div>
              <UserPlus size={20} />
            </div>
            <label>
              Email
              <input type="email" value={form.email} onChange={(event) => update('email', event.target.value)} required />
            </label>
            <label>
              Nome de usuario
              <input value={form.userName} onChange={(event) => update('userName', event.target.value)} required />
            </label>
            <label>
              Senha
              <input type="password" value={form.password} onChange={(event) => update('password', event.target.value)} required />
            </label>
            <PasswordStrength password={form.password} />
          </>
        )}

        {mode === 'forgot' && (
          <>
            <div className="panel-title">
              <div>
                <span className="eyebrow">Recuperacao</span>
                <h2>Esqueci minha senha</h2>
              </div>
              <Mail size={20} />
            </div>
            <label>
              Email da conta
              <input type="email" value={form.email} onChange={(event) => update('email', event.target.value)} required />
            </label>
            <button className="link-button" type="button" onClick={() => setMode('login')}>
              Voltar para login
            </button>
          </>
        )}

        {mode === 'reset' && (
          <>
            <div className="panel-title">
              <div>
                <span className="eyebrow">Nova senha</span>
                <h2>Redefinir senha</h2>
              </div>
              <KeyRound size={20} />
            </div>
            <label>
              Email
              <input type="email" value={form.email} onChange={(event) => update('email', event.target.value)} required />
            </label>
            <label>
              Token
              <input value={form.token} onChange={(event) => update('token', event.target.value)} required />
            </label>
            <label>
              Nova senha
              <input type="password" value={form.newPassword} onChange={(event) => update('newPassword', event.target.value)} required />
            </label>
            <PasswordStrength password={form.newPassword} />
          </>
        )}

        {error && <div className="alert">{error}</div>}
        {message && <div className="success-alert">{message}</div>}

        <button className="primary-button full-width" type="submit" disabled={isSubmitting}>
          {isSubmitting ? 'Aguarde...' : mode === 'forgot' ? 'Enviar email' : mode === 'reset' ? 'Redefinir senha' : 'Continuar'}
        </button>
      </form>
    </main>
  );
}
