from fastapi import FastAPI
from pydantic import BaseModel

from search import semantic_search

app = FastAPI()

class SearchRequest(BaseModel):

    query: str

@app.post("/semantic-search")

def search(request: SearchRequest):

    results = semantic_search(
        request.query
    )

    response = []

    for result in results:

        response.append({

            "content": result.content,

            "source": result.source,

            "distance": float(result.distance)

        })

    return {
        "results": response
    }