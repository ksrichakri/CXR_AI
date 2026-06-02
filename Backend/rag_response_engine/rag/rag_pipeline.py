from semantic_search_engine.app.search import semantic_search

from rag_response_engine.rag.prompt_builder import build_prompt
from rag_response_engine.rag.llm_service import generate_response


def run_rag(question):
    search_results = semantic_search(question)

    retrieved_docs = [
        result[0]
        for result in search_results
    ]
    context = "\n".join(retrieved_docs)

    prompt = build_prompt(
        context,
        question
    )
    
    answer = generate_response(prompt)
    
    return {

        "question": question,
        "retrieved_context": retrieved_docs,

        "answer": answer

    }