import os
import sys
from dotenv import load_dotenv

# Find and load the closest .env file climbing up from this directory
current_dir = os.path.dirname(os.path.abspath(__file__))
for _ in range(5):
    env_path = os.path.join(current_dir, ".env")
    if os.path.exists(env_path):
        load_dotenv(env_path)
        break
    parent = os.path.dirname(current_dir)
    if parent == current_dir:
        break
    current_dir = parent

# Add semantic search engine app directory to sys.path at index 0 to resolve its internal imports (e.g. database, embedder)
# and avoid shadowing conflict with Backend/database
file_dir = os.path.dirname(os.path.abspath(__file__))
backend_dir = os.path.dirname(os.path.dirname(file_dir))
sse_app_path = os.path.join(backend_dir, "semantic_search_engine", "app")

try:
    if os.path.exists(sse_app_path):
        sys.path.insert(0, sse_app_path)
    from search import semantic_search
except Exception:
    try:
        from Backend.semantic_search_engine.app.search import semantic_search
    except Exception:
        try:
            from semantic_search_engine.app.search import semantic_search
        except Exception:
            semantic_search = None
finally:
    if sse_app_path in sys.path:
        sys.path.remove(sse_app_path)

from .prompt_builder import build_prompt
from .llm_service import generate_response

def estimate_tokens(text: str) -> int:
    # Estimate roughly: ~4 characters per token
    return len(text) // 4

def truncate_context_to_budget(docs: list, max_tokens: int) -> tuple[list, int]:
    truncated_docs = []
    current_tokens = 0
    for doc in docs:
        doc_tokens = estimate_tokens(doc)
        if current_tokens + doc_tokens <= max_tokens:
            truncated_docs.append(doc)
            current_tokens += doc_tokens
        else:
            # Partially truncate the final doc to use up the remaining budget
            remaining_tokens = max_tokens - current_tokens
            if remaining_tokens > 100:  # Only include if significant budget remains (approx 400+ chars)
                truncated_doc = doc[:remaining_tokens * 4]
                truncated_docs.append(truncated_doc + "\n... [Truncated due to context limit]")
                current_tokens += remaining_tokens
            break
    return truncated_docs, current_tokens

def run_rag(question, retrieved_docs=None):
    if retrieved_docs is None:
        if semantic_search is not None:
            search_results = semantic_search(question)
            retrieved_docs = [
                result[0] if isinstance(result, (list, tuple)) else getattr(result, "content", str(result))
                for result in search_results
            ]
        else:
            import sys
            print(
                "WARNING: Semantic search engine could not be imported due to missing dependencies "
                "(e.g., sqlalchemy, sentence-transformers, psycopg2-binary). "
                "Retrieved context will be empty. Please install all backend dependencies.",
                file=sys.stderr
            )
            retrieved_docs = []

    # Get maximum context window size configured in environment
    num_ctx = int(os.getenv("LLM_NUM_CTX", "8192"))
    # Save a safety buffer (e.g. 1500 tokens) for prompts and generation output
    max_context_tokens = max(1000, num_ctx - 1500)

    original_docs = list(retrieved_docs)
    truncated_docs, _ = truncate_context_to_budget(retrieved_docs, max_context_tokens)

    context = "\n\n".join(truncated_docs)

    prompt = build_prompt(
        context,
        question
    )
    
    answer = generate_response(prompt)
    
    return {
        "question":question,
        "answer": answer
    }