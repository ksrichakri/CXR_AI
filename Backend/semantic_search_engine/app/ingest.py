import os

from pypdf import PdfReader

from database import SessionLocal, engine
from models import KnowledgeChunk, Base
from embedder import generate_embedding
from chunker import chunk_text

# Create tables in pgvector database if they don't exist
Base.metadata.create_all(engine)

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

    elif file.endswith(".txt"):

        txt_path = f"{DATA_PATH}/{file}"

        with open(txt_path, "r", encoding="utf-8") as f:

            full_text = f.read()

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