import { DocumentItem, IngestionResponse, IngestionStatus, SearchMode, SourceCitation } from '../types';

const API_BASE = process.env.NEXT_PUBLIC_API_URL || 'http://localhost:5000/api';

export const api = {
  async getDocuments(mode?: SearchMode): Promise<DocumentItem[]> {
    const url = mode ? `${API_BASE}/documents?mode=${mode}` : `${API_BASE}/documents`;
    const res = await fetch(url, { cache: 'no-store' });
    if (!res.ok) throw new Error(`Failed to fetch documents: ${res.statusText}`);
    return res.json();
  },

  async deleteDocument(filename: string): Promise<{ success: boolean; deletedChunks: number }> {
    const encoded = encodeURIComponent(filename);
    const res = await fetch(`${API_BASE}/documents/${encoded}`, {
      method: 'DELETE',
    });
    if (!res.ok) throw new Error(`Failed to delete document: ${res.statusText}`);
    return res.json();
  },

  async uploadPdf(file: File): Promise<IngestionResponse> {
    const formData = new FormData();
    formData.append('file', file);

    const res = await fetch(`${API_BASE}/documents/upload`, {
      method: 'POST',
      body: formData,
    });
    if (!res.ok) {
      const err = await res.json().catch(() => ({ message: res.statusText }));
      throw new Error(err.message || 'Upload failed');
    }
    return res.json();
  },

  async getStatus(): Promise<IngestionStatus> {
    const res = await fetch(`${API_BASE}/ingestion/status`, { cache: 'no-store' });
    if (!res.ok) throw new Error('Failed to fetch status');
    return res.json();
  },

  async getHealth(): Promise<{ status: string; uptime: string }> {
    const res = await fetch(`${API_BASE}/health`, { cache: 'no-store' });
    if (!res.ok) throw new Error('API offline');
    return res.json();
  },

  async streamChat({
    question,
    mode = 'pdf',
    history = [],
    signal,
    onSources,
    onToken,
    onDone,
    onError,
  }: {
    question: string;
    mode: SearchMode;
    history?: { role: string; content: string }[];
    signal?: AbortSignal;
    onSources: (sources: SourceCitation[]) => void;
    onToken: (token: string) => void;
    onDone: () => void;
    onError: (err: Error) => void;
  }): Promise<void> {
    try {
      const res = await fetch(`${API_BASE}/chat`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          Accept: 'text/event-stream',
        },
        body: JSON.stringify({
          question,
          mode,
          conversationHistory: history,
        }),
        signal,
      });

      if (!res.ok) {
        const text = await res.text();
        throw new Error(text || `Chat error (${res.status})`);
      }

      if (!res.body) {
        throw new Error('ReadableStream not supported by browser/API');
      }

      const reader = res.body.getReader();
      const decoder = new TextDecoder('utf-8');
      let buffer = '';

      while (true) {
        const { value, done } = await reader.read();
        if (done) break;

        buffer += decoder.decode(value, { stream: true });
        const lines = buffer.split('\n');
        buffer = lines.pop() || '';

        for (const line of lines) {
          const trimmed = line.trim();
          if (!trimmed.startsWith('data:')) continue;

          const dataStr = trimmed.replace(/^data:\s*/, '');
          if (dataStr === '[DONE]') {
            onDone();
            return;
          }

          try {
            const parsed = JSON.parse(dataStr);
            if (parsed.type === 'sources' && Array.isArray(parsed.sources)) {
              onSources(parsed.sources);
            } else if (parsed.type === 'token' && typeof parsed.token === 'string') {
              onToken(parsed.token);
            }
          } catch {
            // Ignore non-JSON control messages
          }
        }
      }

      onDone();
    } catch (err: any) {
      if (err.name !== 'AbortError') {
        onError(err instanceof Error ? err : new Error(String(err)));
      }
    }
  },
};
