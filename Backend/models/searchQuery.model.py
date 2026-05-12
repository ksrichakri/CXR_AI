from pydantic import BaseModel , Field

class SearchQuery(BaseModel):
    query : str = Field(... , 
                        min_length = 5)