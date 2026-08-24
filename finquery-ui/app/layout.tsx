import type { Metadata } from 'next';
import './globals.css';

export const metadata: Metadata = {
  title: 'FinQuery AI — Enterprise Offline Financial Intelligence',
  description: '100% Offline, Local RAG Assistant for Financial Statements, ESG Reports & Databooks powered by pgvector and BM25 Hybrid Search.',
};

export default function RootLayout({
  children,
}: {
  children: React.ReactNode;
}) {
  return (
    <html lang="en">
      <body>{children}</body>
    </html>
  );
}
