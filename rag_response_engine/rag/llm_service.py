import ollama

MODEL_NAME = "llama3"

def generate_response(prompt):

    response = ollama.chat(

        model=MODEL_NAME,

        messages=[
            {
                "role": "user",
                "content": prompt
            }
        ]
    )

    return response["message"]["content"]