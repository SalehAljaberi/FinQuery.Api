'use client';

import React, { useState, useRef, useEffect } from 'react';
import { PaperPlaneRight, Stop, Lightning } from '@phosphor-icons/react';

interface ChatInputProps {
  isStreaming: boolean;
  onSend: (question: string) => void;
  onStop: () => void;
}

export const ChatInput: React.FC<ChatInputProps> = ({ isStreaming, onSend, onStop }) => {
  const [input, setInput] = useState('');
  const textareaRef = useRef<HTMLTextAreaElement>(null);

  const handleSubmit = (e?: React.FormEvent) => {
    if (e) e.preventDefault();
    if (input.trim() && !isStreaming) {
      onSend(input.trim());
      setInput('');
      if (textareaRef.current) {
        textareaRef.current.style.height = 'auto';
      }
    }
  };

  const handleKeyDown = (e: React.KeyboardEvent<HTMLTextAreaElement>) => {
    if (e.key === 'Enter' && !e.shiftKey) {
      e.preventDefault();
      handleSubmit();
    }
  };

  // Auto-grow textarea
  useEffect(() => {
    if (textareaRef.current) {
      textareaRef.current.style.height = 'auto';
      textareaRef.current.style.height = `${Math.min(textareaRef.current.scrollHeight, 140)}px`;
    }
  }, [input]);

  return (
    <div style={{ maxWidth: '850px', width: '100%', margin: '0 auto', padding: '12px 0 20px' }}>
      <form
        onSubmit={handleSubmit}
        style={{
          display: 'flex',
          alignItems: 'flex-end',
          gap: '10px',
          backgroundColor: '#171717',
          border: 'none',
          borderRadius: 'var(--radius-lg)',
          padding: '8px 12px',
          boxShadow: 'var(--shadow-lg)',
        }}
      >
        <textarea
          ref={textareaRef}
          value={input}
          onChange={(e) => setInput(e.target.value)}
          onKeyDown={handleKeyDown}
          placeholder="Ask anything about the ingested financial statements & ESG reports... (Enter to send)"
          rows={1}
          style={{
            flex: 1,
            background: 'transparent',
            border: 'none',
            outline: 'none',
            color: 'var(--text-primary)',
            fontSize: '13px',
            lineHeight: '1.5',
            resize: 'none',
            padding: '6px 4px',
            fontFamily: 'inherit',
            maxHeight: '140px',
          }}
        />

        {isStreaming ? (
          <button
            type="button"
            onClick={onStop}
            className="btn-primary"
            style={{
              backgroundColor: '#e11d48',
              padding: '8px 14px',
            }}
          >
            <Stop size={18} weight="fill" />
          </button>
        ) : (
          <button
            type="submit"
            disabled={!input.trim()}
            className="btn-primary"
            style={{ padding: '8px 12px', backgroundColor: '#e4e4e7', color: '#000000' }}
          >
            <PaperPlaneRight size={18} weight="fill" />
          </button>
        )}
      </form>

      <div
        style={{
          display: 'flex',
          alignItems: 'center',
          justifyContent: 'center',
          gap: '6px',
          fontSize: '11px',
          color: 'var(--text-muted)',
          marginTop: '8px',
        }}
      >
        <Lightning size={12} color="var(--text-secondary)" />
        <span>Powered by Hybrid Search: PostgreSQL HNSW + In-Memory Okapi BM25 Ranking</span>
      </div>
    </div>
  );
};
