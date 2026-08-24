'use client';

import { useState } from 'react';
import { api } from '../lib/api';

export function useUpload(onSuccess?: () => void) {
  const [isUploading, setIsUploading] = useState<boolean>(false);
  const [progressMessage, setProgressMessage] = useState<string>('');
  const [uploadError, setUploadError] = useState<string | null>(null);

  const uploadFile = async (file: File) => {
    const ext = file.name.substring(file.name.lastIndexOf('.')).toLowerCase();
    if (ext !== '.pdf' && ext !== '.csv') {
      setUploadError('Only PDF and CSV files are supported.');
      return false;
    }

    setIsUploading(true);
    setUploadError(null);
    setProgressMessage(`Ingesting ${file.name}...`);

    try {
      const response = await api.uploadPdf(file);
      if (response.success) {
        setProgressMessage(`Successfully indexed ${response.chunksProcessed} chunks!`);
        if (onSuccess) onSuccess();
        setTimeout(() => {
          setIsUploading(false);
          setProgressMessage('');
        }, 2500);
        return true;
      } else {
        setUploadError(response.message || 'Ingestion failed');
        setIsUploading(false);
        return false;
      }
    } catch (err: any) {
      setUploadError(err.message || 'Upload error');
      setIsUploading(false);
      return false;
    }
  };

  return {
    isUploading,
    progressMessage,
    uploadError,
    uploadFile,
    clearError: () => setUploadError(null),
  };
}
