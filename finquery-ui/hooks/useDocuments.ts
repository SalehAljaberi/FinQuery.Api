'use client';

import { useState, useEffect, useCallback } from 'react';
import { api } from '../lib/api';
import { DocumentItem, IngestionStatus, SearchMode } from '../types';

export function useDocuments(mode: SearchMode) {
  const [documents, setDocuments] = useState<DocumentItem[]>([]);
  const [status, setStatus] = useState<IngestionStatus>({ csvChunks: 0, pdfChunks: 0, totalChunks: 0 });
  const [isLoading, setIsLoading] = useState<boolean>(true);
  const [isOnline, setIsOnline] = useState<boolean>(true);
  const [error, setError] = useState<string | null>(null);

  const loadData = useCallback(async () => {
    try {
      setError(null);
      const [docs, stat] = await Promise.all([
        api.getDocuments(mode),
        api.getStatus(),
      ]);
      setDocuments(docs);
      setStatus(stat);
      setIsOnline(true);
    } catch (err: any) {
      setIsOnline(false);
      setError(err.message || 'Failed to connect to API');
    } finally {
      setIsLoading(false);
    }
  }, [mode]);

  useEffect(() => {
    loadData();
    const interval = setInterval(loadData, 5000);
    return () => clearInterval(interval);
  }, [loadData]);

  const deleteDocument = async (filename: string) => {
    try {
      await api.deleteDocument(filename);
      await loadData();
      return true;
    } catch (err: any) {
      setError(err.message || 'Delete failed');
      return false;
    }
  };

  return {
    documents,
    status,
    isLoading,
    isOnline,
    error,
    refreshDocuments: loadData,
    deleteDocument,
  };
}
