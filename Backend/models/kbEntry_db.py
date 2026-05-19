from database.connection import Base
from sqlalchemy import Column, String, Text
from sqlalchemy.types import JSON
from pgvector.sqlalchemy import Vector
class Entry_Model(Base):

    __tablename__ = "knowledge_base"

    id = Column(String(50), primary_key=True, nullable=False,index=True)
    title = Column(String(200), nullable=False)
    category = Column(String(100), nullable=False)
    problem = Column(String(200), nullable=False)
    solution = Column(Text, nullable=False)
    codeSnippet = Column(Text, default="")
    tags = Column(JSON, default=list)
    embedding = Column(Vector(384))