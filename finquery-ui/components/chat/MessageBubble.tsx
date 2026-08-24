'use client';

import React from 'react';
import { User, Sparkle, BookOpen } from '@phosphor-icons/react';
import ReactMarkdown from 'react-markdown';
import remarkMath from 'remark-math';
import rehypeKatex from 'rehype-katex';
import 'katex/dist/katex.min.css';
import { ChatMessage, SourceCitation } from '../../types';
import { Badge } from '../ui/Badge';

interface MessageBubbleProps {
  message: ChatMessage;
  onShowSources: (sources?: SourceCitation[]) => void;
}

export const MessageBubble: React.FC<MessageBubbleProps> = ({ message, onShowSources }) => {
  const isUser = message.role === 'user';

  return (
    <div
      style={{
        display: 'flex',
        gap: '14px',
        maxWidth: '850px',
        margin: '0 auto',
        width: '100%',
        padding: '8px 0',
      }}
      className="animate-fade-in"
    >
      {/* Avatar */}
      <div
        style={{
          width: '32px',
          height: '32px',
          borderRadius: '8px',
          display: 'flex',
          alignItems: 'center',
          justifyContent: 'center',
          flexShrink: 0,
          backgroundColor: isUser ? '#27272a' : '#0a0a0a',
          color: isUser ? '#ffffff' : 'var(--text-primary)',
          border: 'none',
        }}
      >
        {isUser ? <User size={16} weight="bold" /> : <Sparkle size={16} weight="regular" />}
      </div>

      {/* Message Content */}
      <div style={{ flex: 1, minWidth: 0 }}>
        <div style={{ display: 'flex', alignItems: 'center', gap: '8px', marginBottom: '6px' }}>
          <span style={{ fontSize: '12px', fontWeight: 600, color: 'var(--text-primary)' }}>
            {isUser ? 'You' : 'FinQuery AI'}
          </span>
          <span style={{ fontSize: '10px', color: 'var(--text-muted)' }}>
            {new Date(message.timestamp).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })}
          </span>
        </div>

        <div
          className="prose"
          style={{
            backgroundColor: isUser ? '#171717' : '#0a0a0a',
            border: 'none',
            padding: '14px 16px',
            borderRadius: 'var(--radius-md)',
            whiteSpace: 'pre-wrap',
            wordBreak: 'break-word',
          }}
        >
          {message.isStreaming && !message.content ? (
            <span className="streaming-cursor">Thinking...</span>
          ) : (
            <ReactMarkdown
              remarkPlugins={[remarkMath]}
              rehypePlugins={[rehypeKatex]}
              components={{
                p: ({ node, ...props }) => (
                  <p {...props} className={message.isStreaming ? 'streaming-cursor' : ''} />
                ),
              }}
            >
              {message.content}
            </ReactMarkdown>
          )}
        </div>

        {/* Sources Button */}
        {!isUser && message.sources && message.sources.length > 0 && !message.isStreaming && (
          <div style={{ marginTop: '8px' }}>
            <button
              className="btn-secondary"
              onClick={() => onShowSources(message.sources)}
              style={{ fontSize: '10.5px', padding: '4px 10px', gap: '6px' }}
            >
              <BookOpen size={14} color="var(--text-secondary)" weight="regular" />
              <span>View {message.sources.length} Retrieved Sources (pgvector + BM25)</span>
            </button>
          </div>
        )}
      </div>
    </div>
  );
};
