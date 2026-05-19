from sqlalchemy import Column, Integer, Text
from sqlalchemy.orm import declarative_base
from pgvector.sqlalchemy import Vector

Base = declarative_base()

class KnowledgeChunk(Base):

    __tablename__ = "knowledge_chunks"

    id = Column(Integer, primary_key=True)

    content = Column(Text)

    source = Column(Text)

    embedding = Column(Vector(384))