# AI RAG Response Engine

Local Retrieval-Augmented Generation (RAG) module for enterprise knowledge assistant systems.

This module is responsible for:
- Prompt engineering
- Context injection
- Local LLM response generation
- Hallucination reduction
- Structured AI responses

The module is designed to integrate with a semantic search engine and backend API.

---

# Project Architecture

User Question
      ↓
Semantic Search Engine
      ↓
Retrieved Knowledge Chunks
      ↓
RAG Response Engine (THIS MODULE)
      ↓
Local LLM (Ollama / Llama3)
      ↓
Grounded AI Response

---

# Folder Structure

rag-response-engine/
│
├── rag/
│   ├── __init__.py
│   ├── prompt_builder.py
│   ├── llm_service.py
│   └── rag_pipeline.py
│
├── tests/
│   ├── __init__.py
│   └── test_cases.py
│
├── requirements.txt
└── README.md

---

# Requirements

pip install -r requirements.txt

---

# Local LLM Setup

Install Ollama:
https://ollama.com

Pull Llama3 model:
ollama pull llama3

Start Ollama server:
ollama serve

---

# Module Responsibilities

This module:
- receives retrieved semantic search results
- builds grounded prompts
- injects retrieved context into prompts
- generates AI responses using local LLM
- prevents hallucinated responses

This module DOES NOT:
- generate embeddings
- perform vector search
- manage PostgreSQL
- manage APIs
- manage Unity frontend

---

# Integration Contract

Input:
question: str
retrieved_docs: List[str]

Output:
{
    "question": str,
    "answer": str,
    "sources_count": int
}

---

# Example Integration

from rag.rag_pipeline import run_rag

retrieved_docs = [
    "Employees receive 20 days paid leave annually.",
    "Manager approval is required for leave requests."
]

question = "What is the leave policy?"

response = run_rag(
    question,
    retrieved_docs
)

print(response)

---

# Prompt Engineering Rules

The AI assistant is designed to:
- answer ONLY using provided context
- avoid hallucinations
- provide grounded enterprise responses
- reject unavailable information

If information is unavailable, the assistant should respond:
Information not found.

---

# Testing

python -m tests.test_cases

python integration_test.py

---

# Technologies Used

- Python
- Ollama
- Llama3
- LangChain
- FastAPI
- Prompt Engineering
- Retrieval-Augmented Generation (RAG)

---

# Notes

- Ollama server must be running before executing the module.
- Llama3 model must already be downloaded.
- Semantic search is handled externally.
- This module focuses only on grounded AI response generation.

---

# Author

Ganesh Mohanto

AI/ML Developer — RAG Pipeline & AI Response Generation
