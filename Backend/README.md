# Backend API

This is a FastAPI application that manages a knowledge base with semantic search capabilities.

## Overview

The API provides CRUD operations for knowledge base entries with semantic embeddings for full-text search functionality.

## Endpoints

### GET /query

Fetch all knowledge base entries.

**Response:**

- Type: `list[EntryResponse]`
- Returns all entries stored in the database.

**Example:**

```bash
curl http://localhost:8000/query
```

### GET /query/{id}

Fetch a specific knowledge base entry by ID.

**Parameters:**

- `id` (string): The unique identifier of the entry.

**Response:**

- Type: `EntryResponse`
- Returns the entry matching the provided ID, or null if not found.

**Example:**

```bash
curl http://localhost:8000/query/123
```

### POST /query

Add a new knowledge base entry.

**Request Body:**

- Type: `Entry`
- Required fields: title, category, problem, solution
- Optional fields: codeSnippet, tags

**Operations:**

1. Creates a new entry from the provided data
2. Generates semantic embedding from title and problem text
3. Stores the entry in the database

**Response:**

- Type: `EntryResponse`
- Returns the created entry with database-assigned ID.

**Example:**

```bash
curl -X POST http://localhost:8000/query \
  -H "Content-Type: application/json" \
  -d '{
    "title": "Fix image loading error",
    "category": "Image Processing",
    "problem": "Images fail to load when using PIL with large files.",
    "solution": "Use streaming file reads and convert to RGB before processing.",
    "codeSnippet": "from PIL import Image\n...",
    "tags": ["pillow", "io", "bugfix"]
  }'
```

### PUT /query/{id}

Update an existing knowledge base entry.

**Parameters:**

- `id` (string): The unique identifier of the entry to update.

**Request Body:**

- Type: `Entry`
- Contains all updated fields.

**Operations:**

1. Finds the entry by ID
2. Updates all fields (title, category, problem, solution, codeSnippet, tags)
3. Regenerates semantic embedding from updated title and problem
4. Commits changes to the database

**Response:**

- Type: `EntryResponse`
- Returns the updated entry, or error message if entry not found.

**Example:**

```bash
curl -X PUT http://localhost:8000/query/123 \
  -H "Content-Type: application/json" \
  -d '{
    "title": "Updated title",
    "category": "Updated category",
    "problem": "Updated problem",
    "solution": "Updated solution",
    "codeSnippet": "updated code",
    "tags": ["updated"]
  }'
```

### DELETE /query/{id}

Delete a knowledge base entry.

**Parameters:**

- `id` (string): The unique identifier of the entry to delete.

**Response:**

- Returns success or error message.

**Example:**

```bash
curl -X DELETE http://localhost:8000/query/123
```

### POST /search

Semantic search across knowledge base entries using vector similarity, plus a grounded RAG answer built from the matched entries.

**Request Body:**

- Type: `SearchQuery`
- `query` (string): Search text (min 5 characters)

**Operations:**

1. Generates semantic embedding from the search query
2. Queries the database using cosine similarity (pgvector)
3. Builds RAG context from the matched entries
4. Generates an answer with the local RAG pipeline

**Response:**

- Type: `SearchResponse`
- Returns the matched entries and a grounded RAG response.

**Example:**

```bash
curl -X POST http://localhost:8000/search \
  -H "Content-Type: application/json" \
  -d '{"query": "how to handle large files"}'
```

**Use Cases:**

- Find related knowledge base entries
- Discover similar problems and solutions
- Content discovery and recommendations

## Models

### Entry (Request Model)

Fields for creating or updating a knowledge base entry:

- `title` (str): Title of the entry
- `category` (str): Category of the problem
- `problem` (str): Description of the issue
- `solution` (str): Solution text
- `codeSnippet` (Optional[str]): Code snippet (default: empty)
- `tags` (Optional[List[str]]): Tags (default: empty list)

### SearchQuery (Request Model)

Used for semantic search requests:

- `query` (str): Search text (minimum 5 characters)

### EntryResponse (Response Model)

Extends Entry model with database fields:

- All Entry fields
- `id`: Database-assigned identifier
- `embedding`: Semantic vector for search

## Semantic Search

The `/search` endpoint enables semantic similarity search across the knowledge base. This allows finding relevant entries even if they don't match keywords exactly.

### How It Works

1. **Query Embedding**: Your search query is converted to a semantic embedding vector using the SentenceTransformer model (all-MiniLM-L6-v2)
2. **Similarity Matching**: The embedding is compared against all entry embeddings in the database using cosine similarity
3. **Ranking**: Results are returned sorted by relevance (most similar first)

### Technical Details

- **Embedding Model**: `all-MiniLM-L6-v2` (384-dimensional vectors)
- **Vector Database**: PostgreSQL with pgvector extension
- **Similarity Metric**: Cosine distance (lower is more similar)

### Example

**Search Query:**

```json
{ "query": "image loading problems" }
```

**Returns entries where titles or problems discuss image loading, even if not using exact same words.**

- Uses SQLAlchemy ORM
- Automatic table creation on startup
- Session management with dependency injection

## Semantic Search

Each entry generates a semantic embedding from its title and problem description when created or updated. This enables similarity-based search functionality.

## Setup

1. Install dependencies: `uv add langchain_text_splitters` (or similar)
2. Configure database connection in `database/connection.py`
3. Run: `uvicorn main:app --reload`
