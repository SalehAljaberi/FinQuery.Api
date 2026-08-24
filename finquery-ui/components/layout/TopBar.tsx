'use client';

import React from 'react';
import { ArrowClockwise, BookOpen, SidebarSimple, Trash } from '@phosphor-icons/react';
import { SearchMode } from '../../types';
import { Badge } from '../ui/Badge';

interface TopBarProps {
  mode: SearchMode;
  onClearChat: () => void;
  onToggleSources: () => void;
  isSourcePanelOpen: boolean;
  hasSources: boolean;
  onToggleSidebar: () => void;
}

export const TopBar: React.FC<TopBarProps> = ({
  mode,
  onClearChat,
  onToggleSources,
  isSourcePanelOpen,
  hasSources,
  onToggleSidebar,
}) => {
  return (
    <header
      style={{
        height: '56px',
        border: 'none',
        display: 'flex',
        alignItems: 'center',
        justifyContent: 'space-between',
        padding: '0 20px',
        backgroundColor: '#171717',
        backdropFilter: 'blur(12px)',
        zIndex: 10,
      }}
    >
      <div style={{ display: 'flex', alignItems: 'center', gap: '10px' }}>
        <button
          className="btn-icon"
          onClick={onToggleSidebar}
          style={{ marginRight: '8px' }}
        >
          <SidebarSimple size={18} />
        </button>
        <div style={{ fontSize: '13.5px', fontWeight: 600, color: 'var(--text-primary)' }}>
          Financial Chat Workspace
        </div>
        <Badge variant={mode === 'pdf' ? 'teal' : 'blue'} size="sm">
          {mode === 'pdf' ? 'ESG PDF Pipeline' : 'CSV QA Dataset'}
        </Badge>
      </div>

      <div style={{ display: 'flex', alignItems: 'center', gap: '10px' }}>
        <button
          className="btn-secondary"
          onClick={onClearChat}
          style={{ fontSize: '12px', padding: '6px 12px', gap: '6px' }}
        >
          <ArrowClockwise size={14} />
          <span>Reset Session</span>
        </button>

        <button
          className="btn-secondary"
          onClick={onToggleSources}
          disabled={!hasSources}
          style={{
            fontSize: '12px',
            padding: '6px 12px',
            gap: '6px',
            backgroundColor: isSourcePanelOpen ? '#2f2f2f' : undefined,
            color: isSourcePanelOpen ? '#ffffff' : undefined,
          }}
        >
          <BookOpen size={14} weight={isSourcePanelOpen ? 'bold' : 'regular'} />
          <span>Sources Panel</span>
        </button>
      </div>
    </header>
  );
};
