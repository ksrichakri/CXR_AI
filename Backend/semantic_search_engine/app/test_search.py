from app.search import semantic_search
import json

query = ""

results = semantic_search(query)

output = []

for result in results:

    output.append({

        "content": result.content,

        "source": result.source,

        "distance": float(result.distance)

    })

print(json.dumps(output, indent=4))