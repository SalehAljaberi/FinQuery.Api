# FinQuery AI ??

> A fully offline, enterprise-grade Retrieval-Augmented Generation (RAG) assistant for financial and ESG document analysis. No cloud. No API keys. No data leaves your machine.

---

## ?? Screenshots

> **How to add screenshots on GitHub:** Take screenshots of your running app, save them as `.png` files in a `docs/screenshots/` folder inside your repo, push them, then the images below will auto-render on GitHub.

### Chat Interface
![Chat Interface](docs/screenshots/chat_interface.png)
*The main chat window — ask any financial question and get a streamed, sourced answer in real time.*

### Document Upload (PDF & CSV)
![Document Upload](docs/screenshots/document_upload.png)
*The left sidebar allows uploading PDF reports or CSV data files. Each file is chunked, embedded, and indexed automatically.*

### Retrieved Sources Panel
![Sources Panel](docs/screenshots/sources_panel.png)
*Every answer is backed by cited sources — showing the exact document name, page number, and relevance score of each retrieved chunk.*

### Hybrid Search Architecture
![Search Architecture](docs/screenshots/hybrid_search.png)
*The system combines pgvector semantic search and BM25 keyword search before fusing results using Reciprocal Rank Fusion.*

---

## ?? Purpose & Motivation

Most large language models (LLMs) suffer from a critical weakness in enterprise settings: **hallucination**. A standard LLM, when asked *"What was Pick n Pay's total turnover in FY23?"*, will confidently fabricate a number if it does not know the answer. This is catastrophic for financial analysis.

**FinQuery AI** solves this by implementing the **Retrieval-Augmented Generation (RAG)** pattern. Instead of relying on the model's internal, potentially stale, training data, the system:

1. Ingests your private financial documents (annual reports, ESG reports, CSV exports).
2. Converts them into semantic vector representations stored in a local database.
3. At query time, retrieves only the most relevant passages from *your* documents.
4. Passes those passages to the LLM as grounded context, instructing it to answer *only* from what was provided.

This makes FinQuery AI fundamentally different from asking a question to a raw LLM:

| Feature | Raw LLM (e.g. ChatGPT) | FinQuery AI |
|---|---|---|
| Knowledge Source | Training data (cutoff date) | Your private documents |
| Hallucination Risk | High | Minimal (grounded answers only) |
| Data Privacy | Cloud-based | 100% local, offline |
| Citeable Sources | ? | ? (document + page number) |
| Up-to-date with your data | ? | ? (ingest new files anytime) |
| Internet Required | Yes | No |

---

## ??? System Architecture

```
-¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¬
-                        User Browser (Port 3000)                    -
-                     Next.js 15 Frontend (React 19)                 -
L¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦T¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦-
                               - HTTP / SSE Stream
-¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¡¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¬
-                   C# ASP.NET Core API (Port 5000)                  -
-  -¦¦¦¦¦¦¦¦¦¦¦¦¦¬  -¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¬  -¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¬ -
-  -ChatController-  - IngestionService -  -    PromptService       - -
-  - (SSE Stream) -  - (PDF/CSV Parser) -  -  (RAG Prompt Builder)  - -
-  L¦¦¦¦¦¦T¦¦¦¦¦¦-  L¦¦¦¦¦¦¦¦T¦¦¦¦¦¦¦¦¦-  L¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦- -
-         -                  -                                        -
-  -¦¦¦¦¦¦¡¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¡¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¬ -
-  -                    HybridSearchService                        - -
-  -   (pgvector Dense Search + BM25 Sparse Search + RRF Fusion)  - -
-  L¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦- -
L¦¦¦¦¦¦¦¦¦¦T¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦-
           -
    -¦¦¦¦¦¦¡¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¬
    -        PostgreSQL + pgvector             -
    -  (DocumentChunks table with embedding   -
    -   vectors + BM25 inverted index)        -
    L¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦-
           -
    -¦¦¦¦¦¦¡¦¦¦¦¦¦¦¦¦¦¦¦¦¦¬
    -   Ollama (Local)    -
    -  llama3.1:8b (LLM)  -
    -  nomic-embed-text   -
    -    (Embeddings)     -
    L¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦-
```

---

## ?? Core Algorithms Explained

### 1. Text Chunking

When a PDF or CSV is uploaded, it cannot be sent to the LLM as a whole — models have limited context windows. The system splits documents into overlapping **chunks** (e.g., 512 tokens with a 50-token overlap). Overlap ensures that sentences at chunk boundaries are not cut off and lose their meaning.

### 2. Vector Embeddings — Dense Search via `pgvector`

Each text chunk is converted into a **vector embedding** — a list of 768 numbers — using the `nomic-embed-text` model running locally in Ollama. This vector captures the *semantic meaning* of the text.

When a user asks a question, the question is also embedded into a vector. The system then performs a **cosine similarity search** in PostgreSQL using the `pgvector` extension to find the chunks whose meaning is closest to the question.

```
similarity = cos(?) = (A · B) / (|A| × |B|)
```

**Strength:** Captures meaning. *"How much money did the company make?"* will find chunks about *"revenue"* and *"turnover"* even if those exact words aren't in the question.

**Weakness:** Can miss exact keyword matches — if you search for a specific product code or number, semantic search may rank irrelevant documents higher.

### 3. BM25 — Sparse Keyword Search

**BM25 (Best Match 25)** is the gold-standard keyword ranking algorithm used by search engines like Elasticsearch. It is a sophisticated improvement over classic TF-IDF.

The core formula for scoring a document `D` against a query `Q`:

```
Score(D, Q) = ? IDF(q?) × [ f(q?,D) × (k0+1) ] / [ f(q?,D) + k0 × (1 - b + b × |D|/avgdl) ]
```

Where:
- `f(q?, D)` = frequency of query term `q?` in document `D`
- `|D|` = length of document `D`
- `avgdl` = average document length in the collection
- `k0` (typically 1.2–2.0) = term frequency saturation (stops common words from dominating)
- `b` (typically 0.75) = length normalization (penalizes unusually long documents)
- `IDF(q?)` = Inverse Document Frequency — rare, specific terms score much higher than common ones

**Strength:** Extremely precise for exact keyword matches, financial figures like `"R106.6 billion"`, ticker symbols, and product codes.

**Weakness:** Cannot understand semantic meaning. *"revenue"* and *"turnover"* are treated as completely unrelated terms.

### 4. Reciprocal Rank Fusion (RRF) — Hybrid Merging

Since each search method has complementary strengths and weaknesses, FinQuery uses both and merges the results using **Reciprocal Rank Fusion (RRF)**:

```
RRF_score(d) = ? 1 / (k + rank(d))
```

Where `k = 60` (a smoothing constant) and `rank(d)` is the document's position in a given result list.

A chunk that ranks **#1 in dense search AND #2 in BM25** accumulates a very high combined RRF score, while chunks that only appear in one list get a lower combined score. This reliably surfaces the most relevant context regardless of the query type.

**Result:** Hybrid search consistently outperforms either method alone on mixed financial documents that combine structured numerical data with narrative prose.

---

## ?? Tech Stack

| Layer | Technology |
|---|---|
| Frontend | Next.js 15, React 19, TypeScript |
| Backend | C# ASP.NET Core 8 Web API |
| Database | PostgreSQL 16 + `pgvector` extension |
| LLM Engine | Ollama (local inference) |
| LLM Model | `llama3.1:8b` |
| Embedding Model | `nomic-embed-text` |
| Keyword Search | Custom BM25 index (built in-memory from C#) |
| Dense Search | `pgvector` cosine similarity |
| Result Fusion | Reciprocal Rank Fusion (RRF, k=60) |
| PDF Parsing | PdfPig (C# library) |
| Streaming | Server-Sent Events (SSE) |
| Evaluation | DeepEval + Local Ollama Judge |

---

## ?? Prerequisites

Before you begin, ensure you have the following installed:

1. **Docker Desktop** — To run the PostgreSQL + pgvector container. [Download here](https://www.docker.com/products/docker-desktop/).
2. **Ollama** — To run LLMs locally. [Download here](https://ollama.com).
3. **.NET 8 SDK** — To build the C# backend. [Download here](https://dotnet.microsoft.com/download).
4. **Node.js v18+** — To run the Next.js frontend. [Download here](https://nodejs.org).

---

## ?? Getting Started

### Step 1: Start the Database

Run the PostgreSQL + pgvector container with Docker:
```bash
docker run -d \
  --name pgvector-db \
  -e POSTGRES_USER=postgres \
  -e POSTGRES_PASSWORD=postgres \
  -e POSTGRES_DB=finquery \
  -p 5432:5432 \
  pgvector/pgvector:pg16
```

### Step 2: Pull Ollama Models

Pull the LLM and embedding models locally. This is a one-time download:
```bash
ollama pull llama3.1:8b
ollama pull nomic-embed-text
```

### Step 3: Start the C# Backend API

```bash
cd FinQuery.Api
dotnet run
```
> The API will be available at `http://localhost:5000`

The backend will automatically create the database schema and enable the `pgvector` extension on first run.

### Step 4: Start the Next.js Frontend

```bash
cd finquery-ui
npm install
npm run dev
```
> The UI will be available at `http://localhost:3000`

---

## ?? Usage

1. Open `http://localhost:3000` in your browser.
2. In the **left sidebar**, select **PDF Mode** or **CSV Mode**.
3. Upload your financial documents (annual reports, ESG reports, data exports). The ingestion progress bar will show chunking and embedding in real time.
4. Type a financial question in the chat input, for example:
   - *"What was Pick n Pay's total turnover in FY23?"*
   - *"Summarize FoodForward SA's ESG impact metrics."*
   - *"What are the company's net zero carbon targets for 2025?"*
5. Watch the answer stream in real time, with **cited sources** (document name + page number) automatically appended.

---

## ?? Evaluation Suite

A DeepEval benchmarking suite is included in `finquery-eval/`. It tests three RAG-specific quality metrics using a local Ollama model as the judge:

| Metric | What It Measures |
|---|---|
| **Answer Relevancy** | Is the answer relevant to the question asked? |
| **Faithfulness** | Is every claim in the answer supported by the retrieved context? |
| **Contextual Precision** | Are the most relevant chunks ranked at the top of the retrieval results? |

> ?? **Note:** Using `llama3.1:8b` as a local judge is extremely resource-intensive. Budget approximately 15–25 minutes per run on consumer hardware.

```bash
cd finquery-eval
pip install -r requirements.txt
pip install "portalocker[win32]"   # Windows only
deepeval test run test_rag.py
```

---

## ??? Future Improvements

This project was built in approximately one month as a learning exercise. Several meaningful improvements could take it to a production-ready state:

### ?? High Priority
- [ ] **Multi-turn Conversation Memory**: Each question is currently independent. A conversation history buffer would enable follow-up questions like *"What about the year before?"*.
- [ ] **Re-Ranker Model**: Add a cross-encoder re-ranker (e.g., `bge-reranker-v2-m3`) as a third ranking stage after RRF for even higher retrieval precision.
- [ ] **Structured Output Judge**: Replace `llama3.1:8b` with a model that reliably outputs structured JSON (e.g., `qwen2.5:14b`) to eliminate evaluation errors.

### ?? Medium Priority
- [ ] **Multi-Document Sessions**: Allow users to tag documents into named projects and switch between knowledge bases.
- [ ] **OCR Support**: Add Optical Character Recognition via Tesseract for scanned PDFs, since `PdfPig` only handles text-layer PDFs.
- [ ] **Metadata Filtering**: Filter retrieved chunks by document name, upload date, or custom tags before searching.
- [ ] **Export Chat as Report**: Export a conversation history as a formatted PDF or Markdown report.

### ?? Nice to Have
- [ ] **Model Selection UI**: Switch between locally installed Ollama models from the UI without changing config files.
- [ ] **Dashboard Analytics**: A metrics page showing total documents indexed, chunks stored, average query latency, and evaluation scores.
- [ ] **Docker Compose**: Package the entire stack (API + Frontend + Postgres) into a single `docker-compose.yml` for one-command deployment.
- [ ] **Streaming CSV Queries**: Support natural language queries over tabular CSV data using SQL generation, not just chunked text.

---

## ?? License

This project was built for educational purposes as part of a one-month internship program inspired by the [Microsoft Foundry Local RAG example](https://techcommunity.microsoft.com/blog/azuredevcommunityblog/building-your-first-local-rag-application-with-foundry-local/4501968). Feel free to fork, extend, and learn from it.

---

*Built with ?? using Microsoft .NET, Next.js, PostgreSQL, and the local AI power of Ollama.*
