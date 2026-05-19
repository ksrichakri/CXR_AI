from sqlalchemy import text
from Backend.database.connection import SessionLocal
from app.embedder import generate_embedding


def semantic_search(query, top_k=5):

    db = SessionLocal()

    query_embedding = generate_embedding(query)

    query_embedding_str = "[" + ",".join(map(str, query_embedding)) + "]"

    sql = text("""

        SELECT
            content,
            source,
            embedding <=> CAST(:query_embedding AS vector) AS distance

        FROM knowledge_chunks

        ORDER BY distance

        LIMIT :top_k

    """)

    results = db.execute(
        sql,
        {
            "query_embedding": query_embedding_str,
            "top_k": top_k
        }
    )

    return results.fetchall()