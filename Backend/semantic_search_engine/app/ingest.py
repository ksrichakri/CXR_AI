import os

from pypdf import PdfReader

from intnCxr.Backend.semantic_search_engine.app.database import SessionLocal
from intnCxr.Backend.semantic_search_engine.app.models import KnowledgeChunk
from intnCxr.Backend.semantic_search_engine.app.embedder import generate_embedding
from intnCxr.Backend.semantic_search_engine.app.chunker import chunk_text

db = SessionLocal()

DATA_PATH = "data"

for file in os.listdir(DATA_PATH):

    if file.endswith(".pdf"):

        pdf_path = f"{DATA_PATH}/{file}"

        reader = PdfReader(pdf_path)

        full_text = ""

        for page in reader.pages:

            full_text += page.extract_text()

        chunks = chunk_text(full_text)

        for chunk in chunks:

            embedding = generate_embedding(chunk)

            db_chunk = KnowledgeChunk(
                content=chunk,
                source=file,
                embedding=embedding
            )

            db.add(db_chunk)

db.commit()

print("Data ingestion completed!")