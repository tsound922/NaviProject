import { useState } from 'react';
import { ragApi } from '../api/client';

export default function IngestPanel() {
  const [source, setSource] = useState('');
  const [text, setText] = useState('');
  const [loading, setLoading] = useState(false);
  const [message, setMessage] = useState<{ type: 'success' | 'error'; text: string } | null>(null);

  const handleTextIngest = async () => {
    if (!source.trim() || !text.trim()) {
      setMessage({ type: 'error', text: 'Source and content are required.' });
      return;
    }

    setLoading(true);
    setMessage(null);

    try {
      await ragApi.ingest(source.trim(), text.trim());
      setMessage({ type: 'success', text: 'Ingested successfully!' });
      setSource('');
      setText('');
    } catch {
      setMessage({ type: 'error', text: 'Failed to ingest. Please try again.' });
    } finally {
      setLoading(false);
    }
  };

  const handleFileUpload = async (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    if (!file) return;

    const content = await file.text();
    setSource(file.name);
    setText(content);
    e.target.value = '';
  };

  return (
    <div style={{
      padding: '24px',
      display: 'flex',
      flexDirection: 'column',
      gap: '16px',
      maxWidth: '800px',
      margin: '0 auto',
      width: '100%',
    }}>
      <h2 style={{ fontSize: '18px', fontWeight: 'bold', color: '#333' }}>
        Ingest Document
      </h2>

      {/* Source */}
      <div style={{ display: 'flex', flexDirection: 'column', gap: '6px' }}>
        <label style={{ fontSize: '14px', color: '#555' }}>Source</label>
        <input
          value={source}
          onChange={e => setSource(e.target.value)}
          placeholder="e.g. project-alpha-notes"
          style={{
            padding: '10px 14px',
            borderRadius: '8px',
            border: '1px solid #ddd',
            fontSize: '14px',
            outline: 'none',
          }}
        />
      </div>

      {/* File Upload */}
      <div style={{ display: 'flex', flexDirection: 'column', gap: '6px' }}>
        <label style={{ fontSize: '14px', color: '#555' }}>Upload File (txt / md)</label>
        <input
          type="file"
          accept=".txt,.md"
          onChange={handleFileUpload}
          style={{ fontSize: '14px' }}
        />
      </div>

      {/* Text Content */}
      <div style={{ display: 'flex', flexDirection: 'column', gap: '6px' }}>
        <label style={{ fontSize: '14px', color: '#555' }}>Content</label>
        <textarea
          value={text}
          onChange={e => setText(e.target.value)}
          placeholder="Paste your notes or task description here..."
          rows={12}
          style={{
            padding: '10px 14px',
            borderRadius: '8px',
            border: '1px solid #ddd',
            fontSize: '14px',
            outline: 'none',
            resize: 'vertical',
            fontFamily: 'inherit',
          }}
        />
      </div>

      {/* Status Message */}
      {message && (
        <div style={{
          padding: '10px 14px',
          borderRadius: '8px',
          fontSize: '14px',
          backgroundColor: message.type === 'success' ? '#f0fdf4' : '#fef2f2',
          color: message.type === 'success' ? '#16a34a' : '#dc2626',
          border: `1px solid ${message.type === 'success' ? '#bbf7d0' : '#fecaca'}`,
        }}>
          {message.text}
        </div>
      )}

      {/* Submit Button */}
      <button
        onClick={handleTextIngest}
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
        {loading ? 'Ingesting...' : 'Ingest'}
      </button>
    </div>
  );
}