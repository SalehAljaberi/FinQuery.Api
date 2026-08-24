'use client';

import React, { useRef, useState } from 'react';
import { CloudArrowUp, WarningCircle } from '@phosphor-icons/react';
import { useUpload } from '../../hooks/useUpload';
import { Spinner } from '../ui/Spinner';
import { SearchMode } from '../../types';

interface UploadDropzoneProps {
  onSuccess?: () => void;
  mode: SearchMode;
}

export const UploadDropzone: React.FC<UploadDropzoneProps> = ({ onSuccess, mode }) => {
  const fileInputRef = useRef<HTMLInputElement>(null);
  const [isDragOver, setIsDragOver] = useState(false);
  const { isUploading, progressMessage, uploadError, uploadFile, clearError } = useUpload(onSuccess);

  const handleFile = async (file: File) => {
    await uploadFile(file);
    if (fileInputRef.current) fileInputRef.current.value = '';
  };

  const onDragOver = (e: React.DragEvent) => {
    e.preventDefault();
    setIsDragOver(true);
  };

  const onDragLeave = () => {
    setIsDragOver(false);
  };

  const onDrop = (e: React.DragEvent) => {
    e.preventDefault();
    setIsDragOver(false);
    if (e.dataTransfer.files && e.dataTransfer.files.length > 0) {
      handleFile(e.dataTransfer.files[0]);
    }
  };

  return (
    <div style={{ marginTop: 'auto', paddingTop: '16px' }}>
      <input
        ref={fileInputRef}
        type="file"
        accept={mode === 'pdf' ? '.pdf' : '.csv'}
        style={{ display: 'none' }}
        onChange={(e) => {
          if (e.target.files && e.target.files.length > 0) {
            handleFile(e.target.files[0]);
          }
        }}
      />

      <div
        onDragOver={onDragOver}
        onDragLeave={onDragLeave}
        onDrop={onDrop}
        onClick={() => !isUploading && fileInputRef.current?.click()}
        style={{
          border: `1.5px dashed ${isDragOver ? 'var(--text-primary)' : 'rgba(255, 255, 255, 0.15)'}`,
          backgroundColor: isDragOver ? 'rgba(255, 255, 255, 0.08)' : '#0a0a0a',
          borderRadius: 'var(--radius-md)',
          padding: '16px',
          textAlign: 'center',
          cursor: isUploading ? 'not-allowed' : 'pointer',
          transition: 'all 0.2s ease',
        }}
      >
        {isUploading ? (
          <div style={{ display: 'flex', flexDirection: 'column', alignItems: 'center', gap: '8px' }}>
            <Spinner size={24} color="#ffffff" />
            <div style={{ fontSize: '12px', color: 'var(--text-secondary)', fontWeight: 500 }}>
              {progressMessage}
            </div>
          </div>
        ) : (
          <div style={{ display: 'flex', flexDirection: 'column', alignItems: 'center', gap: '6px' }}>
            <div
              style={{
                width: '36px',
                height: '36px',
                borderRadius: '50%',
                backgroundColor: 'rgba(255, 255, 255, 0.1)',
                color: 'var(--text-secondary)',
                display: 'flex',
                alignItems: 'center',
                justifyContent: 'center',
              }}
            >
              <CloudArrowUp size={20} weight="bold" />
            </div>
            <div style={{ fontSize: '13px', fontWeight: 500, color: 'var(--text-primary)' }}>
              Upload {mode === 'pdf' ? 'PDF' : 'CSV'}
            </div>
            <div style={{ fontSize: '11px', color: 'var(--text-muted)' }}>
              Drag & drop or click to browse
            </div>
          </div>
        )}
      </div>

      {uploadError && (
        <div
          style={{
            display: 'flex',
            alignItems: 'center',
            gap: '6px',
            marginTop: '8px',
            padding: '8px 10px',
            backgroundColor: 'rgba(244, 63, 94, 0.12)',
            border: '1px solid rgba(244, 63, 94, 0.3)',
            borderRadius: '6px',
            fontSize: '12px',
            color: '#fb7185',
          }}
        >
          <WarningCircle size={14} weight="bold" />
          <span style={{ flex: 1 }}>{uploadError}</span>
          <button
            onClick={(e) => {
              e.stopPropagation();
              clearError();
            }}
            style={{ background: 'none', border: 'none', color: '#fb7185', cursor: 'pointer', fontSize: '14px' }}
          >
            &times;
          </button>
        </div>
      )}
    </div>
  );
};
