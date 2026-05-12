# Models

This document describes the data models used by the application.

**kbEntry / `Entry`**

- **File:** [models/kbEntry.model.py](models/kbEntry.model.py)
- **Purpose:** Represents a knowledge-base entry containing a problem description and its solution.
- **Fields:**
  - `title` (str): Title of entry. Constraints: min_length=5, max_length=50.
  - `category` (str): Category of problem. Constraints: min_length=5.
  - `problem` (str): The issue raised. Constraints: min_length=10, max_length=200.
  - `solution` (str): Solution provided to the problem. Constraints: min_length=20.
  - `codeSnippet` (Optional[str]): Optional code snippet illustrating the solution. Default: empty string.
  - `tags` (Optional[List[str]]): Optional list of tag strings. Default: empty list.

Example JSON:

```json
{
  "title": "Fix image loading error",
  "category": "Image Processing",
  "problem": "Images fail to load when using PIL with large files.",
  "solution": "Use streaming file reads and convert to RGB before processing.",
  "codeSnippet": "from PIL import Image\n...",
  "tags": ["pillow", "io", "bugfix"]
}
```

**searchQuery / `SearchQuery`**

- **File:** [models/searchQuery.model.py](models/searchQuery.model.py)
- **Purpose:** Simple model for encapsulating user search queries.
- **Fields:**
  - `query` (str): The search text. Constraints: min_length=5.

Example JSON:

```json
{
  "query": "image preprocessing steps"
}
```
