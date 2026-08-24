'use client';

import React from 'react';
import { Files, FileDashed } from '@phosphor-icons/react';
import { DocumentItem } from '../../types';
import { DocumentCard } from './DocumentCard';
import { Spinner } from '../ui/Spinner';

interface DocumentListProps {
  documents: DocumentItem[];
  isLoading: boolean;
  onDelete: (filename: string) => Promise<boolean>;
}

export const DocumentList: React.FC<DocumentListProps> = ({
  documents,
  isLoading,
  onDelete,
}) => {
  if (isLoading && documents.length === 0) {
    return (
      <div style={{ display: 'flex', justifyContent: 'center', padding: '32px 0' }}>
        <Spinner size={20} />
      </div>
    );
  }

  if (documents.length === 0) {
    return (
      <div
        style={{
          display: 'flex',
          flexDirection: 'column',
          alignItems: 'center',
          justifyContent: 'center',
          padding: '32px 16px',
          textAlign: 'center',
          color: 'var(--text-muted)',
          gap: '8px',
        }}
      >
        <FileDashed size={32} />
        <div style={{ fontSize: '13px', fontWeight: 500, color: 'var(--text-secondary)' }}>
          No documents indexed yet
        </div>
        <div style={{ fontSize: '11px' }}>
          Upload a financial PDF report below to begin querying.
        </div>
      </div>
    );
  }

  return (
    <div style={{ display: 'flex', flexDirection: 'column', gap: '8px', overflowY: 'auto', flex: 1, paddingRight: '4px' }}>
      {documents.map((doc) => (
        <DocumentCard key={doc.source} document={doc} onDelete={onDelete} />
      ))}
    </div>
  );
};
