'use client';

import React from 'react';
import { X, BookOpen, FileText, CheckCircle, Percent } from '@phosphor-icons/react';
import { SourceCitation } from '../../types';
import { Badge } from '../ui/Badge';

interface SourcePanelProps {
  sources: SourceCitation[];
  isOpen: boolean;
  onClose: () => void;
}

export const SourcePanel: React.FC<SourcePanelProps> = ({ sources, isOpen, onClose }) => {
  if (!isOpen) return null;

  const getScoreVariant = (score: number) => {
    if (score >= 0.028) return 'emerald';
    if (score >= 0.015) return 'teal';
    return 'amber';
  };

  return (
    <aside
      style={{
        width: '380px',
        height: '100%',
        backgroundColor: '#0a0a0a',
        borderLeft: '1px solid var(--border-subtle)',
        display: 'flex',
        flexDirection: 'column',
        flexShrink: 0,
        zIndex: 20,
        boxShadow: '-4px 0 24px rgba(0, 0, 0, 0.3)',
      }}
    >
      {/* Header */}
      <div
        style={{
          display: 'flex',
          alignItems: 'center',
          justifyContent: 'space-between',
          padding: '16px 20px',
          borderBottom: '1px solid var(--border-subtle)',
        }}
      >
        <div style={{ display: 'flex', alignItems: 'center', gap: '8px' }}>
          <BookOpen size={18} color="var(--text-secondary)" weight="regular" />
          <span style={{ fontSize: '13px', fontWeight: 600, color: 'var(--text-primary)' }}>
            Retrieved Sources ({sources.length})
          </span>
        </div>

        <button className="btn-icon" onClick={onClose} title="Close Panel">
          <X size={16} />
        </button>
      </div>

      {/* Citations List */}
      <div
        style={{
          padding: '16px',
          overflowY: 'auto',
          display: 'flex',
          flexDirection: 'column',
          gap: '12px',
          flex: 1,
        }}
      >
        {sources.length === 0 ? (
          <div style={{ padding: '32px 16px', textAlign: 'center', color: 'var(--text-muted)', fontSize: '12px' }}>
            No sources attached to this message.
          </div>
        ) : (
          sources.map((source, index) => (
            <div
              key={source.Id || index}
              style={{
                backgroundColor: '#171717',
                border: '1px solid var(--border-subtle)',
                borderRadius: 'var(--radius-md)',
                padding: '14px',
                display: 'flex',
                flexDirection: 'column',
                gap: '8px',
                transition: 'border-color 0.2s ease',
              }}
              onMouseEnter={(e) => {
                e.currentTarget.style.borderColor = '#52525b';
              }}
              onMouseLeave={(e) => {
                e.currentTarget.style.borderColor = 'var(--border-subtle)';
              }}
            >
              <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', gap: '8px' }}>
                <div style={{ display: 'flex', alignItems: 'center', gap: '6px', minWidth: 0 }}>
                  <FileText size={16} color="#94a3b8" />
                  <span
                    style={{
                      fontSize: '11.5px',
                      fontWeight: 600,
                      color: 'var(--text-primary)',
                      whiteSpace: 'nowrap',
                      overflow: 'hidden',
                      textOverflow: 'ellipsis',
                    }}
                    title={source.Source}
                  >
                    {source.Source}
                  </span>
                </div>

                <Badge variant={getScoreVariant(source.SimilarityScore)} size="sm">
                  Page {source.PageNumber}
                </Badge>
              </div>

              <div style={{ display: 'flex', alignItems: 'center', gap: '8px' }}>
                <span style={{ fontSize: '10px', color: 'var(--text-muted)' }}>
                  RRF Score: <strong>{source.SimilarityScore.toFixed(5)}</strong>
                </span>
                <Badge variant="neutral" size="sm">
                  pgvector + BM25
                </Badge>
              </div>

              <div
                style={{
                  fontSize: '11.5px',
                  lineHeight: '1.5',
                  color: '#cbd5e1',
                  backgroundColor: '#0a0a0a',
                  padding: '10px',
                  borderRadius: '6px',
                  border: '1px solid rgba(255, 255, 255, 0.05)',
                  fontFamily: 'var(--font-sans)',
                  maxHeight: '180px',
                  overflowY: 'auto',
                  wordBreak: 'break-word',
                }}
              >
                {source.ChunkText}
              </div>
            </div>
          ))
        )}
      </div>
    </aside>
  );
};
