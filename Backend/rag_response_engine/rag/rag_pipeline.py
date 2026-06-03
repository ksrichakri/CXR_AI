try:
    from Backend.semantic_search_engine.app.search import semantic_search
except ImportError:
    try:
        from semantic_search_engine.app.search import semantic_search
    except ImportError:
        semantic_search = None

from .prompt_builder import build_prompt
from .llm_service import generate_response


def run_rag(question, retrieved_docs=None):
    if retrieved_docs is None:
        if semantic_search is not None:
            search_results = semantic_search(question)
            retrieved_docs = [
                result[0] if isinstance(result, (list, tuple)) else getattr(result, "content", str(result))
                for result in search_results
            ]
        else:
            retrieved_docs = []

    context = "\n".join(retrieved_docs)

    prompt = build_prompt(
        context,
        question
    )
    
    answer = generate_response(prompt)
    
    return {
        "question": question,
        "answer": answer,
        "sources_count": len(retrieved_docs),
        "retrieved_context": retrieved_docs
    }