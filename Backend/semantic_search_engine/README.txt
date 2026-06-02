SEMANTIC SEARCH ENGINE — PROJECT README
=======================================

Project Overview
----------------
This project implements an enterprise-level semantic search engine using:
- Python
- PostgreSQL
- pgvector
- Sentence Transformers
- FastAPI

The system allows users to:
1. Upload company documents (PDFs)
2. Convert document text into embeddings
3. Store embeddings inside PostgreSQL using pgvector
4. Perform semantic similarity search
5. Retrieve contextually relevant information

------------------------------------------------------------

PROJECT ARCHITECTURE
--------------------

PDF Documents
      ↓
Text Extraction
      ↓
Chunking
      ↓
Embedding Generation
      ↓
pgvector Storage
      ↓
Semantic Similarity Search
      ↓
Relevant Retrieval Results

------------------------------------------------------------

FOLDER STRUCTURE
----------------

semantic-search-engine/
│
├── app/
│   ├── api.py
│   ├── chunker.py
│   ├── database.py
│   ├── embedder.py
│   ├── ingest.py
│   ├── models.py
│   ├── search.py
│   ├── test_db.py
│   └── test_search.py
│
├── data/
│   └── enterprise_knowledge_base.pdf
│
├── tests/
├── venv/

------------------------------------------------------------

TECHNOLOGIES USED
-----------------

Backend:
- Python
- FastAPI
- SQLAlchemy

Database:
- PostgreSQL
- pgvector

AI/ML:
- Sentence Transformers
- Semantic Embeddings
- Cosine Similarity

Utilities:
- Docker
- pypdf
- LangChain Text Splitters

------------------------------------------------------------

SYSTEM WORKFLOW
---------------

1. PDF Ingestion
----------------
PDF files are placed inside:

data/

The ingestion pipeline:
- Reads PDFs
- Extracts text
- Splits text into chunks
- Generates embeddings
- Stores embeddings inside PostgreSQL

Run:

python app/ingest.py

------------------------------------------------------------

2. Embedding Generation
-----------------------
Embeddings are generated using:

all-MiniLM-L6-v2

These embeddings convert text into numerical vector representations.

------------------------------------------------------------

3. Vector Database Storage
--------------------------
Embeddings are stored using:

PostgreSQL + pgvector

Table Used:

knowledge_chunks

Columns:
- id
- content
- source
- embedding

------------------------------------------------------------

4. Semantic Search
------------------
User query:
    ↓
Generate query embedding
    ↓
Cosine similarity search
    ↓
Retrieve most relevant chunks

Run:

python app/test_search.py

------------------------------------------------------------

EXAMPLE INPUT
-------------

{
    "query": "How does semantic search work?"
}

------------------------------------------------------------

EXAMPLE OUTPUT
--------------

[
    {
        "content": "Vector databases optimize semantic retrieval speed.",
        "source": "enterprise_knowledge_base.pdf",
        "distance": 0.82
    }
]

------------------------------------------------------------

WHAT WAS IMPLEMENTED
--------------------

Completed Features:
- PDF text extraction
- Chunking pipeline
- Embedding generation
- pgvector integration
- PostgreSQL storage
- Semantic similarity retrieval
- JSON output format
- Dockerized PostgreSQL setup

------------------------------------------------------------

ROLE CONTRIBUTION (AI/ML Dev 1)
-------------------------------

Responsibilities Completed:
- Semantic search pipeline
- Embedding generation
- Vector database integration
- Similarity retrieval logic
- Retrieval optimization
- Chunking improvements

------------------------------------------------------------

FUTURE IMPROVEMENTS
-------------------

Possible Enhancements:
- Hybrid search
- Metadata filtering
- Reranking
- Retrieval caching
- LLM integration
- RAG pipeline
- Unity frontend integration
- API authentication

------------------------------------------------------------

FINAL RESULT
------------

The system successfully performs:
- Semantic document retrieval
- Vector similarity search
- Enterprise knowledge search

This prototype can now integrate with:
- FastAPI backend
- Unity frontend
- AI response generation systems

============================================================

