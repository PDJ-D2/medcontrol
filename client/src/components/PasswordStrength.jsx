const rules = [
  { label: '12 caracteres', test: (value) => value.length >= 12 },
  { label: 'letra maiuscula', test: (value) => /[A-Z]/.test(value) },
  { label: 'letra minuscula', test: (value) => /[a-z]/.test(value) },
  { label: 'numero', test: (value) => /\d/.test(value) },
  { label: 'simbolo', test: (value) => /[^A-Za-z0-9]/.test(value) },
];

export function getPasswordScore(password) {
  return rules.filter((rule) => rule.test(password)).length;
}

export function isStrongPassword(password) {
  return getPasswordScore(password) === rules.length;
}

export function PasswordStrength({ password }) {
  const score = getPasswordScore(password);
  const label = score < 3 ? 'fraca' : score < 5 ? 'boa' : 'forte';

  return (
    <div className="password-strength">
      <div className="strength-header">
        <span>Senha {label}</span>
        <span>{score}/5</span>
      </div>
      <div className="strength-track" aria-hidden="true">
        <div style={{ width: `${(score / 5) * 100}%` }} />
      </div>
      <div className="password-rules">
        {rules.map((rule) => (
          <span className={rule.test(password) ? 'met' : ''} key={rule.label}>
            {rule.label}
          </span>
        ))}
      </div>
    </div>
  );
}
