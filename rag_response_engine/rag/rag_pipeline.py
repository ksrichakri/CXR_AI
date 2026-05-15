from semantic_search_engine.search import semantic_search

from rag.prompt_builder import build_prompt
from rag.llm_service import generate_response


def run_rag(question):

    # STEP 1: semantic retrieval
    search_results = semantic_search(question)

    # STEP 2: extract content only
    retrieved_docs = [

        result[0]

        for result in search_results
    ]

    # STEP 3: combine context
    context = "\n".join(retrieved_docs)

    # STEP 4: build prompt
    prompt = build_prompt(
        context,
        question
    )

    # STEP 5: generate AI response
    answer = generate_response(prompt)

    # STEP 6: return structured output
    return {

        "question": question,

        "retrieved_context": retrieved_docs,

        "answer": answer,

        "sources": [

            result[1]

            for result in search_results
        ]
    }