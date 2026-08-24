export type SearchMode = 'pdf' | 'csv';

export interface SourceCitation {
  Id: string;
  ChunkText: string;
  SimilarityScore: number;
  Source: string;
  PageNumber: number;
}

export interface ChatMessage {
  id: string;
  role: 'user' | 'assistant';
  content: string;
  sources?: SourceCitation[];
  timestamp: Date;
  isStreaming?: boolean;
}

export interface DocumentItem {
  source: string;
  chunkCount: number;
  maxPage: number;
  mode: string;
}

export interface IngestionStatus {
  csvChunks: number;
  pdfChunks: number;
  totalChunks: number;
}

export interface IngestionResponse {
  success: boolean;
  message: string;
  chunksProcessed: number;
  mode: string;
  duration?: string;
}
