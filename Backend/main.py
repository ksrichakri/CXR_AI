from fastapi import FastAPI,Depends
from models.kbEntry import Entry
from database.connection import SessionLocal,engine,Base
from models.kbEntry_db import Entry_Model
from models.kbEntry import Entry
from sqlalchemy.orm import Session

app = FastAPI()
Base.metadata.create_all(bind = engine)

def db_con():
    db  = SessionLocal()
    try:
        yield db
    finally:
        db.close()

@app.get("/query")
def fetch(db:Session = Depends(db_con)):
    db_prod = db.query(Entry_Model).all()
    return db_prod
    

@app.get("/query/{id}")
def fetch_query(id:str,db:Session = Depends(db_con)):
    searchRes = db.query(Entry_Model).filter(Entry_Model.id == id).first()
    return searchRes


@app.post("/query")
def add_kb(entry:Entry,db:Session = Depends(db_con)):
    db_entry = Entry_Model(**entry.model_dump())
    db.add(db_entry)
    db.commit()
    db.refresh(db_entry)
    return db_entry

@app.put("/query/{id}")
def update_kb(id:str,entry:Entry,db:Session = Depends(db_con)):
    searchRes = db.query(Entry_Model).filter(Entry_Model.id == id).first()
    if searchRes:
        searchRes.title = entry.title
        searchRes.category = entry.category
        searchRes.problem = entry.problem
        searchRes.solution= entry.solution
        searchRes.codeSnippet= entry.codeSnippet
        searchRes.tags= entry.tags
        db.commit()
        return searchRes
    else:
        
        return {"message":"No matching entry found"}

@app.delete("/query/{id}")
def delete_query(id:str , db: Session = Depends(db_con)):
    db_entry = db.query(Entry_Model).filter(Entry_Model.id == id).first()
    if db_entry:
        db.delete(db_entry)
        db.commit()
        return {"message":"Entry deleted"}
    else:
        return {"message":"Entry not found"}