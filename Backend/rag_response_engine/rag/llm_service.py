import ollama

MODEL_NAME = "llama3"

def generate_response(prompt):
    response = ollama.chat(
        model=MODEL_NAME,
        messages=[
            {
                "role": "system",
                "content": (
                    "You are an XR technical assistant. "
                    "Answer only using the provided "
                    "knowledge base context."
                )
            },
            {
                "role": "user",
                "content": prompt
            }
        ],
        options={
            "temperature": 0.2
        }
    )
    return response["message"]["content"]