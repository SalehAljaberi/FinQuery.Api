# Summer School Foundry Local Plan

## Overview

This is a one-month summer project plan for beginner computer science students. The goal is to build a local Q&A assistant using Microsoft Foundry Local and the Retrieval-Augmented Generation (RAG) pattern.

The final project is an offline chatbot that can answer questions from a small document collection, such as course notes, manuals, or FAQs, by retrieving relevant information locally and using it to guide a language model's response.

## Program Structure

The program is split into three phases:

1. **Phase 1 - Foundational Learning (Weeks 1-2)**  
   Introduces RAG, Foundry Local, embeddings, vector search, SQLite, and prompt engineering.

2. **Phase 2 - Project Implementation (Weeks 3-4)**  
   Focuses on building the RAG application, from ingestion and retrieval to local model integration.

3. **Phase 3 - Testing, Documentation, and Presentation (Week 5, optionally Week 6)**  
   Covers testing, evaluation, performance tuning, documentation, and final presentations.

## Project Overview and Key Technologies

### Project aim
Build a local document Q&A assistant that runs entirely on a student's computer. It uses RAG to answer user questions by:

- retrieving relevant content from a local knowledge base
- combining that content with the user query
- generating an answer with a local LLM

This approach helps reduce hallucinations and improve source-grounded answers.

### Foundry Local
Foundry Local is a local AI runtime and SDK for running language models entirely on a user's device. It supports offline inference and does not require a cloud account or GPU. It automatically manages model downloads and uses local hardware acceleration when available.

### RAG
RAG stands for Retrieval-Augmented Generation. The basic flow is:

1. retrieve relevant information from documents
2. augment the prompt with that information
3. generate an answer using the model

### Embeddings and vector search
Students learn how text embeddings represent meaning as vectors. Similar text produces similar vectors, which enables semantic search and document retrieval.

### SQLite
SQLite is used as a lightweight local database for storing document text and embeddings. It is simple, serverless, and works well for local data storage.

### Prompt engineering
Students learn how to write prompts that guide the model to:

- use retrieved context
- avoid guessing
- cite sources when possible
- say it does not know when context is insufficient

### Architecture
The final app uses a simple single-device architecture:

- **Client interface**: CLI, basic web UI, or console input
- **Pipeline layer**: handles user queries and retrieval
- **Data layer**: SQLite database for document chunks and embeddings
- **AI layer**: Foundry Local LLM for answer generation

## Phase 1 - Foundational Learning

### Objectives
By the end of Week 2, students should:

- understand how RAG works
- be comfortable with the main tools and concepts
- have Foundry Local installed and working
- have a sample SQLite database
- have run small test programs for embeddings and vector similarity search

### Week 1 - RAG Concept and Local AI Setup

#### Topics
- Intro to RAG and the problem it solves
- How Foundry Local enables offline LLM use
- Basic Python project structure

#### Activities
- Explain the limits of a general LLM for domain-specific questions
- Show how RAG improves accuracy by adding external knowledge
- Introduce Foundry Local and its offline workflow
- Review a simple Python project layout with `main.py` and `requirements.txt`

#### Exercises
- Manual RAG role-play using a short document
- Install Foundry Local SDK on student machines
- Run a "Hello Model" test with a small local model
- Create a simple Python project skeleton and print a greeting

#### Milestone
By the end of Week 1, students should have:

- Foundry Local installed and working
- a basic Python project folder
- a trivial local model inference test completed

### Week 2 - Embeddings, Vector Search, and SQLite

#### Topics
- Text embeddings and semantic similarity
- Vector search for retrieval
- SQLite for local storage
- Basic prompt engineering for Q&A

#### Activities
- Show how embeddings map text to vectors
- Demonstrate cosine similarity for ranking relevant text
- Explain why SQLite is a good fit for local document storage
- Introduce system and user prompts
- Practice prompt design for grounded answers

#### Exercises
- Generate embeddings for sample sentences
- Compute similarity scores and find the best match
- Create a small SQLite database for documents and embeddings
- Run prompt experiments with and without added context

#### Milestone
By the end of Week 2, students should have:

- a working knowledge of RAG, Foundry Local, embeddings, and SQLite
- a test database or schema design for document storage
- practice with cosine similarity in Python
- an understanding of how to write prompts for the model

## Phase 2 - Project Implementation

### Objectives
During Weeks 3 and 4, students build a working local RAG application with:

- document ingestion
- embedding generation
- retrieval
- local LLM integration
- a simple user interface

### Week 3 - Data Ingestion and Retrieval Pipeline

#### Topics
- Choosing the document set
- Chunking documents into passages
- Generating embeddings for each chunk
- Storing chunks and embeddings in SQLite
- Building a retrieval function

#### Activities
- Select a small knowledge base, such as FAQs or course notes
- Split documents into smaller chunks
- Generate embeddings for each chunk using Foundry Local
- Save text and vectors into SQLite
- Write retrieval logic for user queries

#### Exercises
- Build an ingestion script
- Insert chunk text and embedding vectors into the database
- Verify the expected number of records after ingestion
- Implement `get_top_chunks(query)` to return the most relevant chunks
- Test retrieval with sample questions

#### Milestone
By the end of Week 3, students should have:

- a populated SQLite document database
- embeddings stored for each chunk
- a working retrieval function for top relevant chunks

### Week 4 - LLM Integration and Application Assembly

#### Topics
- Connecting retrieval to a local chat model
- Model selection trade-offs
- Building a user interface
- Responsible outputs and source citations

#### Activities
- Load a small Foundry Local chat model
- Combine retrieved context with the user question
- Send the combined prompt to the model
- Choose an interface: CLI, Streamlit/Gradio, or basic HTML and JavaScript
- Add instructions so the model answers only from the provided context

#### Exercises
- Write `answer_query(user_question)`
- Call `get_top_chunks()` and pass the results into the model prompt
- Test the full pipeline end to end
- Build the chosen interface
- Add a source name or citation style to answers when possible

#### Milestone
By the end of Week 4, each team should have:

- a working offline Q&A application
- retrieval from a SQLite-backed knowledge base
- local LLM-generated answers
- a complete core project

## Phase 3 - Testing, Evaluation, and Documentation

### Objectives
In the final phase, students refine the system, test it, document it, and prepare a presentation.

### Week 5 - System Testing and Evaluation

#### Topics
- Functional testing
- Performance and debugging
- Evaluation and improvement

#### Activities
- Test questions the system should answer and should not answer
- Verify fallback behavior for missing information
- Check edge cases such as empty input or overly general questions
- Measure response time and tune performance if needed
- Review answer quality and retrieval accuracy

#### Milestone
By mid Week 5, students should have documented test results and identified any issues that need final adjustments.

### Week 6 or End of Week 5 - Documentation and Final Presentation

#### Topics
- Project documentation
- Code cleanup and comments
- Final presentation prep

#### Activities
- Write a README or short project report
- Document purpose, setup, usage, and design choices
- Clean up code and add comments
- Prepare a demo and short presentation

#### Presentation guidance
Each group should present:

- the problem their assistant solves
- the main features and components
- a live demo
- one or two lessons learned

#### Milestone
By the end of Week 6, teams should have:

- completed projects
- documentation
- rehearsed presentations
- a working demo ready for demo day

## Final Outcome

By the end of the month, students will have:

- a functional offline AI Q&A system
- a practical understanding of retrieval and generation
- experience with embeddings, vector search, SQLite, and prompt engineering
- a clear view of how local AI applications are built step by step

## Notes

This plan emphasizes hands-on learning supported by curated Microsoft resources and practical coding exercises. The approach builds confidence in each component before combining them into the final local RAG application.

## Project ideas shared (links)

- Possible 2026 extra project (Local RAG with Foundry Local - Main idea link, always reveiew it): https://techcommunity.microsoft.com/blog/azuredevcommunityblog/building-your-first-local-rag-application-with-foundry-local/4501968 