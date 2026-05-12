from pydantic import BaseModel , Field
from typing import List , Optional

class Entry(BaseModel):
    id: str = Field(...,
                    min_length = 5)
    title : str = Field(...,
                        min_length = 5,
                        max_length = 50,
                        description="Title of entry")
    category: str = Field(...,
                          min_length = 5,
                          description="Category of problem")
    problem: str = Field(...,
                         min_length = 10 , 
                         max_length = 200,
                         description="The issue raised")
    solution : str = Field(... ,
                           min_length = 20,
                           description="Solution provided to the problem raised")
    codeSnippet: Optional[str] = ""
    tags:Optional[List[str]] = []
