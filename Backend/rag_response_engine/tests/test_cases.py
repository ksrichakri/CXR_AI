from intnCxr.Backend.rag_response_engine.rag.rag_pipeline import run_rag

retrieved_docs = [

    "Employees are entitled to 20 days of paid leave annually.",

    "Manager approval is required for leave requests."
]
question = "What is the leave policy?"

answer = run_rag(
    question,
    retrieved_docs
)

print(answer)