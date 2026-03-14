import { useState } from 'react';
import { authApi } from '../api/client';

interface LoginPageProps {
  onLoginSuccess: () => void;
}

export default function LoginPage({ onLoginSuccess }: LoginPageProps) {
  const [isRegister, setIsRegister] = useState(false);
  const [username, setUsername] = useState('');
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [error, setError] = useState<string | null>(null);
  const [loading, setLoading] = useState(false);

  const handleSubmit = async () => {
    if (!username.trim() || !password.trim()) {
      setError('Username and password are required.');
      return;
    }

    setLoading(true);
    setError(null);

    try {
      const res = isRegister
        ? await authApi.register(username, email, password)
        : await authApi.login(username, password);

      localStorage.setItem('token', res.data.token);
      onLoginSuccess();
    } catch {
      setError(isRegister ? 'Registration failed. Please try again.' : 'Invalid username or password.');
    } finally {
      setLoading(false);
    }
  };

  const handleKeyDown = (e: React.KeyboardEvent) => {
    if (e.key === 'Enter') handleSubmit();
  };

  return (
    <div style={{
      display: 'flex',
      alignItems: 'center',
      justifyContent: 'center',
      height: '100vh',
      backgroundColor: '#f5f5f5',
    }}>
      <div style={{
        backgroundColor: '#fff',
        padding: '40px',
        borderRadius: '12px',
        boxShadow: '0 4px 20px rgba(0,0,0,0.1)',
        width: '100%',
        maxWidth: '400px',
        display: 'flex',
        flexDirection: 'column',
        gap: '16px',
      }}>
        <h1 style={{ fontSize: '24px', fontWeight: 'bold', textAlign: 'center', color: '#333' }}>
          NaviProject
        </h1>
        <p style={{ textAlign: 'center', color: '#aaa', fontSize: '14px', marginBottom: '8px' }}>
          {isRegister ? 'Create your account' : 'Sign in to your account'}
        </p>

        {/* Username */}
        <input
          value={username}
          onChange={e => setUsername(e.target.value)}
          onKeyDown={handleKeyDown}
          placeholder="Username"
          style={{
            padding: '10px 14px',
            borderRadius: '8px',
            border: '1px solid #ddd',
            fontSize: '14px',
            outline: 'none',
          }}
        />

        {/* Email (only for register) */}
        {isRegister && (
          <input
            value={email}
            onChange={e => setEmail(e.target.value)}
            onKeyDown={handleKeyDown}
            placeholder="Email"
            type="email"
            style={{
              padding: '10px 14px',
              borderRadius: '8px',
              border: '1px solid #ddd',
              fontSize: '14px',
              outline: 'none',
            }}
          />
        )}

        {/* Password */}
        <input
          value={password}
          onChange={e => setPassword(e.target.value)}
          onKeyDown={handleKeyDown}
          placeholder="Password"
          type="password"
          style={{
            padding: '10px 14px',
            borderRadius: '8px',
            border: '1px solid #ddd',
            fontSize: '14px',
            outline: 'none',
          }}
        />

        {/* Error */}
        {error && (
          <div style={{
            padding: '10px 14px',
            borderRadius: '8px',
            backgroundColor: '#fef2f2',
            color: '#dc2626',
            fontSize: '14px',
            border: '1px solid #fecaca',
          }}>
            {error}
          </div>
        )}

        {/* Submit */}
        <button
          onClick={handleSubmit}
          disabled={loading}
          style={{
            backgroundColor: loading ? '#aaa' : '#4f46e5',
            color: '#fff',
            border: 'none',
            borderRadius: '8px',
            padding: '12px',
            cursor: loading ? 'not-allowed' : 'pointer',
            fontWeight: 'bold',
            fontSize: '14px',
          }}
        >
          {loading ? 'Please wait...' : isRegister ? 'Register' : 'Sign In'}
        </button>

        {/* Toggle */}
        <p style={{ textAlign: 'center', fontSize: '14px', color: '#aaa' }}>
          {isRegister ? 'Already have an account?' : "Don't have an account?"}
          {' '}
          <span
            onClick={() => { setIsRegister(!isRegister); setError(null); }}
            style={{ color: '#4f46e5', cursor: 'pointer', fontWeight: 'bold' }}
          >
            {isRegister ? 'Sign In' : 'Register'}
          </span>
        </p>
      </div>
    </div>
  );
}