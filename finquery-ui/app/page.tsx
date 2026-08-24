'use client';

import React, { useState } from 'react';
import { SearchMode } from '../types';
import { useDocuments } from '../hooks/useDocuments';
import { useChat } from '../hooks/useChat';
import { Sidebar } from '../components/layout/Sidebar';
import { TopBar } from '../components/layout/TopBar';
import { ChatWindow } from '../components/chat/ChatWindow';
import { ChatInput } from '../components/chat/ChatInput';
import { SourcePanel } from '../components/chat/SourcePanel';
import { StatusBar } from '../components/ui/StatusBar';

export default function FinQueryDashboard() {
  const [mode, setMode] = useState<SearchMode>('pdf');
  const [isSidebarOpen, setIsSidebarOpen] = useState(true);

  // Document Management Hook
  const {
    documents,
    status,
    isLoading: isDocsLoading,
    isOnline,
    refreshDocuments,
    deleteDocument,
  } = useDocuments(mode);

  // Chat & Streaming Hook
  const {
    messages,
    isStreaming,
    activeSources,
    isSourcePanelOpen,
    setIsSourcePanelOpen,
    sendMessage,
    stopStreaming,
    clearChat,
    showSourcesForMessage,
  } = useChat(mode);

  return (
    <div className="app-container">
      {/* 1. Left Sidebar: Documents & Ingestion */}
      <Sidebar
        documents={documents}
        isLoading={isDocsLoading}
        mode={mode}
        onModeChange={setMode}
        onDeleteDocument={deleteDocument}
        onUploadSuccess={refreshDocuments}
        isOpen={isSidebarOpen}
      />

      {/* 2. Main Center Workspace */}
      <div style={{ flex: 1, display: 'flex', flexDirection: 'column', height: '100%', minWidth: 0 }}>
        {/* Top Header */}
        <TopBar
          mode={mode}
          onClearChat={clearChat}
          onToggleSources={() => setIsSourcePanelOpen(!isSourcePanelOpen)}
          isSourcePanelOpen={isSourcePanelOpen}
          hasSources={activeSources.length > 0}
          onToggleSidebar={() => setIsSidebarOpen(!isSidebarOpen)}
        />

        {/* Scrollable Chat Feed */}
        <ChatWindow
          messages={messages}
          onSelectPrompt={(prompt) => sendMessage(prompt)}
          onShowSources={showSourcesForMessage}
        />

        {/* Bottom Chat Input Bar */}
        <ChatInput
          isStreaming={isStreaming}
          onSend={sendMessage}
          onStop={stopStreaming}
        />

        {/* Bottom System Status Bar */}
        <StatusBar
          isOnline={isOnline}
          totalChunks={status.totalChunks}
          mode={mode}
        />
      </div>

      {/* 3. Right Collapsible Sources Drawer */}
      <SourcePanel
        sources={activeSources}
        isOpen={isSourcePanelOpen}
        onClose={() => setIsSourcePanelOpen(false)}
      />
    </div>
  );
}
