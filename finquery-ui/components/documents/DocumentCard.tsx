'use client';

import React, { useState } from 'react';
import { FilePdf, Trash, CheckCircle } from '@phosphor-icons/react';
import { DocumentItem } from '../../types';
import { Badge } from '../ui/Badge';
import { Spinner } from '../ui/Spinner';

interface DocumentCardProps {
  document: DocumentItem;
  onDelete: (filename: string) => Promise<boolean>;
}

export const DocumentCard: React.FC<DocumentCardProps> = ({ document, onDelete }) => {
  const [isDeleting, setIsDeleting] = useState(false);

  const handleDelete = async (e: React.MouseEvent) => {
    e.stopPropagation();
    if (confirm(`Remove "${document.source}" from the database?`)) {
      setIsDeleting(true);
      await onDelete(document.source);
      setIsDeleting(false);
    }
  };

  return (
    <div
      style={{
        display: 'flex',
        alignItems: 'center',
        justifyContent: 'space-between',
        padding: '10px 12px',
        backgroundColor: '#0a0a0a',
        border: 'none',
        borderRadius: 'var(--radius-md)',
        transition: 'all 0.2s ease',
        cursor: 'default',
      }}
      onMouseEnter={(e) => {
        e.currentTarget.style.backgroundColor = '#171717';
      }}
      onMouseLeave={(e) => {
        e.currentTarget.style.backgroundColor = '#0a0a0a';
      }}
    >
      <div style={{ display: 'flex', alignItems: 'center', gap: '10px', minWidth: 0, flex: 1 }}>
        <div
          style={{
            display: 'flex',
            alignItems: 'center',
            justifyContent: 'center',
            width: '32px',
            height: '32px',
            borderRadius: '6px',
            backgroundColor: '#2f2f2f',
            color: 'var(--text-primary)',
            flexShrink: 0,
          }}
        >
          <FilePdf size={18} weight="fill" />
        </div>

        <div style={{ minWidth: 0, flex: 1 }}>
          <div
            style={{
              fontSize: '13px',
              fontWeight: 500,
              color: 'var(--text-primary)',
              whiteSpace: 'nowrap',
              overflow: 'hidden',
              textOverflow: 'ellipsis',
            }}
            title={document.source}
          >
            {document.source}
          </div>
          <div style={{ display: 'flex', alignItems: 'center', gap: '8px', marginTop: '3px' }}>
            <Badge variant="blue" size="sm">
              {document.chunkCount} chunks
            </Badge>
            {document.maxPage > 1 && (
              <span style={{ fontSize: '11px', color: 'var(--text-muted)' }}>
                {document.maxPage} pages
              </span>
            )}
          </div>
        </div>
      </div>

      <button
        className="btn-icon danger"
        onClick={handleDelete}
        disabled={isDeleting}
        title="Delete document"
        style={{ marginLeft: '8px' }}
      >
        {isDeleting ? <Spinner size={14} color="#f43f5e" /> : <Trash size={14} />}
      </button>
    </div>
  );
};
