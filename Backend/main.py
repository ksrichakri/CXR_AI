from fastapi import FastAPI,Depends,HTTPException
from models.kbEntry import Entry
from database.connection import SessionLocal,engine,Base
from models.kbEntry_db import Entry_Model
from models.kbEntry import Entry
from models.searchQuery import SearchQuery
from sqlalchemy.orm import Session
from sqlalchemy.exc import IntegrityError
from semantic_search_engine.app.embedder import generate_embedding
from models.response import EntryResponse
app = FastAPI()
Base.metadata.create_all(bind = engine)

def db_con():
    db  = SessionLocal()
    try:
        yield db
    finally:
        db.close()

@app.get("/")
def greet():
    return "Welcome to CXR's LLM"

@app.get("/query",response_model=list[EntryResponse])
def fetch(db:Session = Depends(db_con)):
    db_prod = db.query(Entry_Model).all()
    return db_prod
    
@app.get("/query/{id}",response_model=EntryResponse)
def fetch_query(id:int,db:Session = Depends(db_con)):
    searchRes = db.query(Entry_Model).filter(Entry_Model.id == id).first()
    if(searchRes):
        return searchRes
    else:
        raise HTTPException(
            status_code=404,
            detail="Entry not found"
        )


@app.post("/query",response_model=EntryResponse)
def add_kb(entry:Entry,db:Session = Depends(db_con)):
    try:
        db_entry = Entry_Model(**entry.model_dump())
        search_text = f"{entry.title} {entry.problem}" 
        db_entry.embedding = generate_embedding(search_text)
        db.add(db_entry)
        db.commit()
        db.refresh(db_entry)
        return db_entry
    
    except IntegrityError:
        db.rollback()
        raise HTTPException

@app.put("/query/{id}",response_model=EntryResponse)
def update_kb(id:int,entry:Entry,db:Session = Depends(db_con)):
    searchRes = db.query(Entry_Model).filter(Entry_Model.id == id).first()
    if searchRes:
        searchRes.title = entry.title
        searchRes.category = entry.category
        searchRes.problem = entry.problem
        searchRes.solution= entry.solution
        searchRes.codeSnippet= entry.codeSnippet
        searchRes.tags= entry.tags
        search_text = f"{entry.title} {entry.problem}" 
        searchRes.embedding = generate_embedding(search_text)
        db.commit()
        db.refresh(searchRes)
        return searchRes
    else:
        raise HTTPException(
            status_code=404,
            detail="Entry not found"
        )

@app.delete("/query/{id}")
def delete_query(id:int , db: Session = Depends(db_con)):
    db_entry = db.query(Entry_Model).filter(Entry_Model.id == id).first()
    if db_entry:
        db.delete(db_entry)
        db.commit()
        return {"message":"Entry deleted"}
    else:
        raise HTTPException(
            status_code=404,
            detail="Entry not found"
        )

@app.post("/search",response_model=list[EntryResponse])
def sem_search(search_query: SearchQuery, db:Session = Depends(db_con)):
    
    query_embedding = generate_embedding(search_query.query)
    
    results = db.query(
        Entry_Model,
        (Entry_Model.embedding.cosine_distance(query_embedding)).label('similarity')
    ).order_by('similarity').limit(2)
    
    return [result[0] for result in results]
    