from fastapi import FastAPI
from models.kbEntry import Entry
from database.connection import SessionLocal,engine,Base
from models.kbEntry_db import Entry_Model
app = FastAPI()
Base.metadata.create_all(bind = engine)

db  = SessionLocal()

@app.get("/query")
def fetch():
    return True

@app.get("/query/{id}")
def fetch_query(id:str):
    return "Entry not found" 
        
    
    