import socket

HOST = "127.0.0.1"
PORT = 5001

with socket.socket(socket.AF_INET, socket.SOCK_STREAM) as s:
    s.bind((HOST, PORT))
    s.listen()

    print("Python server listening...")

    conn, addr = s.accept()
    print("Connected:", addr)

    with conn:
        while True:
            data = conn.recv(1024)
            if not data:
                break
            print("Received:", data.decode().strip())
