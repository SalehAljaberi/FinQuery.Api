'use client';

import React from 'react';
import { Database, Cpu, ShieldCheck } from '@phosphor-icons/react';

interface StatusBarProps {
  isOnline: boolean;
  totalChunks: number;
  mode: 'pdf' | 'csv';
}

export const StatusBar: React.FC<StatusBarProps> = ({ isOnline, totalChunks, mode }) => {
  return (
    <div
      style={{
        display: 'flex',
        alignItems: 'center',
        justifyContent: 'space-between',
        padding: '8px 16px',
        backgroundColor: '#171717',
        border: 'none',
        fontSize: '12px',
        color: 'var(--text-secondary)',
      }}
    >
      <div style={{ display: 'flex', alignItems: 'center', gap: '16px' }}>
        <div style={{ display: 'flex', alignItems: 'center', gap: '6px' }}>
          <span className={`status-dot ${!isOnline ? 'offline' : ''}`} />
          <span>{isOnline ? 'C# Web API Active (Port 5000)' : 'API Disconnected'}</span>
        </div>

        <div style={{ display: 'flex', alignItems: 'center', gap: '6px' }}>
          <Database size={14} color="var(--text-secondary)" />
          <span>
            <strong>{totalChunks.toLocaleString()}</strong> vectors indexed (pgvector + BM25)
          </span>
        </div>
      </div>

      <div style={{ display: 'flex', alignItems: 'center', gap: '16px' }}>
        <div style={{ display: 'flex', alignItems: 'center', gap: '6px' }}>
          <Cpu size={14} color="var(--text-secondary)" />
          <span>Qwen3-0.6B Embeddings &bull; Mode: {mode.toUpperCase()}</span>
        </div>

        <div style={{ display: 'flex', alignItems: 'center', gap: '6px', color: 'var(--text-secondary)' }}>
          <ShieldCheck size={14} weight="bold" />
          <span>100% Local &bull; $0 Cloud Cost</span>
        </div>
      </div>
    </div>
  );
};
