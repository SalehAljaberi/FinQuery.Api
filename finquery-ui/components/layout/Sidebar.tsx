'use client';

import React from 'react';
import { ChartLineUp, Database, Sparkle } from '@phosphor-icons/react';
import { DocumentItem, SearchMode } from '../../types';
import { DocumentList } from '../documents/DocumentList';
import { UploadDropzone } from '../documents/UploadDropzone';
import { Badge } from '../ui/Badge';

interface SidebarProps {
  documents: DocumentItem[];
  isLoading: boolean;
  mode: SearchMode;
  onModeChange: (mode: SearchMode) => void;
  onDeleteDocument: (filename: string) => Promise<boolean>;
  onUploadSuccess: () => void;
  isOpen: boolean;
}

export const Sidebar: React.FC<SidebarProps> = ({
  documents,
  isLoading,
  mode,
  onModeChange,
  onDeleteDocument,
  onUploadSuccess,
  isOpen,
}) => {
  return (
    <aside
      style={{
        width: isOpen ? '320px' : '0px',
        height: '100%',
        backgroundColor: '#0a0a0a',
        borderRight: isOpen ? '1px solid var(--border-subtle)' : 'none',
        display: 'flex',
        flexDirection: 'column',
        padding: isOpen ? '20px 16px' : '0px',
        flexShrink: 0,
        overflow: 'hidden',
        transition: 'all 0.3s cubic-bezier(0.4, 0, 0.2, 1)',
        opacity: isOpen ? 1 : 0,
      }}
    >
      {/* Brand Header */}
      <div style={{ display: 'flex', alignItems: 'center', gap: '10px', marginBottom: '20px' }}>
        <div
          style={{
            width: '36px',
            height: '36px',
            borderRadius: '10px',
            backgroundColor: 'var(--accent-blue)',
            display: 'flex',
            alignItems: 'center',
            justifyContent: 'center',
            color: '#000000',
          }}
        >
          <ChartLineUp size={22} weight="bold" />
        </div>
        <div>
          <div style={{ fontSize: '14px', fontWeight: 700, color: 'var(--text-primary)', letterSpacing: '-0.02em' }}>
            FinQuery AI
          </div>
          <div style={{ fontSize: '10px', color: 'var(--text-muted)' }}>
            Enterprise Financial Intelligence
          </div>
        </div>
      </div>

      {/* Dataset / Mode Toggle */}
      <div
        style={{
          display: 'flex',
          backgroundColor: '#171717',
          border: 'none',
          borderRadius: '8px',
          padding: '3px',
          marginBottom: '16px',
        }}
      >
        <button
          onClick={() => onModeChange('pdf')}
          style={{
            flex: 1,
            padding: '6px 10px',
            borderRadius: '6px',
            border: 'none',
            fontSize: '11px',
            fontWeight: 500,
            cursor: 'pointer',
            backgroundColor: mode === 'pdf' ? 'var(--accent-blue)' : 'transparent',
            color: mode === 'pdf' ? '#000000' : 'var(--text-secondary)',
            transition: 'all 0.15s ease',
          }}
        >
          PDF Mode (ESG / 10-K)
        </button>
        <button
          onClick={() => onModeChange('csv')}
          style={{
            flex: 1,
            padding: '6px 10px',
            borderRadius: '6px',
            border: 'none',
            fontSize: '11px',
            fontWeight: 500,
            cursor: 'pointer',
            backgroundColor: mode === 'csv' ? 'var(--accent-blue)' : 'transparent',
            color: mode === 'csv' ? '#000000' : 'var(--text-secondary)',
            transition: 'all 0.15s ease',
          }}
        >
          CSV Mode (Financials)
        </button>
      </div>

      {/* Section Title */}
      <div
        style={{
          display: 'flex',
          alignItems: 'center',
          justifyContent: 'space-between',
          marginBottom: '12px',
          padding: '0 4px',
        }}
      >
        <div style={{ fontSize: '10px', fontWeight: 600, color: 'var(--text-muted)', textTransform: 'uppercase', letterSpacing: '0.05em' }}>
          Indexed Documents ({documents.length})
        </div>
        <Badge variant="teal" size="sm">
          pgvector + BM25
        </Badge>
      </div>

      {/* Documents List */}
      <DocumentList
        documents={documents}
        isLoading={isLoading}
        onDelete={onDeleteDocument}
      />

      {/* Upload Box */}
      <UploadDropzone onSuccess={onUploadSuccess} mode={mode} />
    </aside>
  );
};
