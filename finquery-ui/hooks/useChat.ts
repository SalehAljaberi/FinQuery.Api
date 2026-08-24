'use client';

import { useState, useRef, useCallback } from 'react';
import { api } from '../lib/api';
import { ChatMessage, SearchMode, SourceCitation } from '../types';

export function useChat(mode: SearchMode) {
  const [messages, setMessages] = useState<ChatMessage[]>([]);
  const [isStreaming, setIsStreaming] = useState<boolean>(false);
  const [activeSources, setActiveSources] = useState<SourceCitation[]>([]);
  const [isSourcePanelOpen, setIsSourcePanelOpen] = useState<boolean>(false);
  const abortControllerRef = useRef<AbortController | null>(null);

  const sendMessage = useCallback(
    async (question: string) => {
      if (!question.trim() || isStreaming) return;

      const userMsgId = 'user-' + Date.now();
      const assistantMsgId = 'ai-' + Date.now();

      const userMessage: ChatMessage = {
        id: userMsgId,
        role: 'user',
        content: question.trim(),
        timestamp: new Date(),
      };

      const assistantMessage: ChatMessage = {
        id: assistantMsgId,
        role: 'assistant',
        content: '',
        sources: [],
        timestamp: new Date(),
        isStreaming: true,
      };

      // Add messages to state
      setMessages((prev) => [...prev, userMessage, assistantMessage]);
      setIsStreaming(true);

      // Build conversation history from prior messages (up to last 6)
      const history = messages
        .filter((m) => !m.isStreaming && m.content)
        .slice(-6)
        .map((m) => ({
          role: m.role,
          content: m.content,
        }));

      abortControllerRef.current = new AbortController();

      await api.streamChat({
        question,
        mode,
        history,
        signal: abortControllerRef.current.signal,
        onSources: (sources) => {
          setActiveSources(sources);
          setIsSourcePanelOpen(true);
          setMessages((prev) =>
            prev.map((msg) =>
              msg.id === assistantMsgId
                ? { ...msg, sources }
                : msg
            )
          );
        },
        onToken: (token) => {
          setMessages((prev) =>
            prev.map((msg) =>
              msg.id === assistantMsgId
                ? { ...msg, content: msg.content + token }
                : msg
            )
          );
        },
        onDone: () => {
          setIsStreaming(false);
          setMessages((prev) =>
            prev.map((msg) =>
              msg.id === assistantMsgId
                ? { ...msg, isStreaming: false }
                : msg
            )
          );
        },
        onError: (err) => {
          setIsStreaming(false);
          setMessages((prev) =>
            prev.map((msg) =>
              msg.id === assistantMsgId
                ? {
                    ...msg,
                    content:
                      msg.content +
                      `\n\n*(Error communicating with FinQuery backend: ${err.message})*`,
                    isStreaming: false,
                  }
                : msg
            )
          );
        },
      });
    },
    [isStreaming, messages, mode]
  );

  const stopStreaming = () => {
    if (abortControllerRef.current) {
      abortControllerRef.current.abort();
      setIsStreaming(false);
    }
  };

  const clearChat = () => {
    setMessages([]);
    setActiveSources([]);
    setIsSourcePanelOpen(false);
  };

  const showSourcesForMessage = (sources?: SourceCitation[]) => {
    if (sources && sources.length > 0) {
      setActiveSources(sources);
      setIsSourcePanelOpen(true);
    }
  };

  return {
    messages,
    isStreaming,
    activeSources,
    isSourcePanelOpen,
    setIsSourcePanelOpen,
    sendMessage,
    stopStreaming,
    clearChat,
    showSourcesForMessage,
  };
}
