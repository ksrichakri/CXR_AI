def build_prompt(context, question):

    return f"""
    You are an enterprise AI assistant.

    STRICT RULES:
    - Use ONLY provided context
    - Never invent information
    - If answer is unavailable, say:
      "Information not found."

    Context:
    {context}

    Question:
    {question}

    Provide a professional answer.
    """