import sys
import os
import argparse
import json

# Add relevant paths to sys.path to allow execution from different directories
current_dir = os.path.dirname(os.path.abspath(__file__))
parent_dir = os.path.dirname(current_dir)
backend_dir = os.path.dirname(parent_dir)
cxr_ai_dir = os.path.dirname(backend_dir)

for path in [parent_dir, backend_dir, cxr_ai_dir]:
    if path and path not in sys.path:
        sys.path.append(path)

try:
    from Backend.rag_response_engine.rag.rag_pipeline import run_rag
except ImportError:
    try:
        from rag_response_engine.rag.rag_pipeline import run_rag
    except ImportError:
        from rag.rag_pipeline import run_rag

def main():
    parser = argparse.ArgumentParser(description="Test RAG Response Engine")
    parser.add_argument("question", nargs="?", default="What is the leave policy?", help="User query/question")
    parser.add_argument("--docs", nargs="*", default=None, help="Retrieved context documents (strings)")
    
    args = parser.parse_args()
    
    # Default behavior for default query
    if args.docs is None and args.question == "What is the leave policy?":
        args.docs = [
            "Employees are entitled to 20 days of paid leave annually.",
            "Manager approval is required for leave requests."
        ]
        
    print(f"Question: {args.question}")
    if args.docs is not None:
        print(f"Provided Docs Count: {len(args.docs)}")
    else:
        print("No docs provided. Fetching dynamically using semantic search...")

    try:
        answer = run_rag(
            args.question,
            retrieved_docs=args.docs
        )
        print("\nRAG Pipeline Response:")
        print(json.dumps(answer, indent=2))
    except Exception as e:
        print(f"Error executing run_rag: {e}")
        print("Please make sure Ollama server is running and the llama3 model is pulled.")

if __name__ == "__main__":
    main()