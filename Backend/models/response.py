from pydantic import BaseModel, Field
from typing import List


class EntryResponse(BaseModel):

    id: str = Field(
        ...,
        min_length=5
    )

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

    class Config:
        from_attributes = True