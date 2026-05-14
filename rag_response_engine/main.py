import ollama

retrieved_docs = [

    "Employees are entitled to 20 days of paid leave annually.",

    "Manager approval is required for leave requests."
]

question = "What is the leave policy?"

context = "\n".join(retrieved_docs)

prompt = f"""
You are a company AI assistant.

Answer ONLY using provided context.

Context:
{context}

Question:
{question}
"""

response = ollama.chat(
    model='llama3',

    messages=[
        {
            'role': 'user',
            'content': prompt
        }
    ]
)

print(
    response['message']['content']
)