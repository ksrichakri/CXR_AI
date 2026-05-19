from Backend.database.connection import engine

try:

    connection = engine.connect()

    print("Database connected successfully!")

    connection.close()

except Exception as e:

    print(e)