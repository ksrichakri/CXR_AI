from pydantic import BaseModel, Field
from typing import List


class EntryResponse(BaseModel):

    id: int = Field(...)

    title: str = Field(
        ...,
        min_length=5,
        max_length=200,
        description="Title of entry"
    )

    category: str = Field(
        ...,
        description="Category of problem"
    )

    problem: str = Field(
        ...,
        min_length=10,
        description="The issue raised"
    )

    solution: str = Field(
        ...,
        min_length=20,
        description="Solution provided"
    )

    codeSnippet: str = ""

    tags: List[str] = Field(
        default_factory=list
    )
    
    gen_answer: str = Field(
        default= "Answer"
    )
    class Config:
        from_attributes = True


class RAGResponse(BaseModel):

    question: str = Field(...)

    answer: str = Field(...)

    # sources_count: int = Field(..., ge=0)

    # retrieved_context: List[str] = Field(default_factory=list)


class SearchResponse(BaseModel):

    results: List[EntryResponse] = Field(default_factory=list)

    rag_response: RAGResponse = Field(...)