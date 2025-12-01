import socket
import time

avg_latency = [0, 0.0]



#s.sendall(b"hello from python\n")

RESPONSE = True


if RESPONSE:
    while True:
        s = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
        print("Connecting...")
        s.connect(("127.0.0.1", 5000))
        print("Connected!")

        msg = '50.0 50.0\n'
        t_1 = time.time()

        # send
        s.sendall(msg.encode("utf-8"))

        buffer = b""
        while not buffer.endswith(b"\n"):
            chunk = s.recv(1)
            if not chunk:
                break
            buffer += chunk
        print("Response:", buffer.decode().strip())
        
        s.close() 

        # measure round-trip latency
        latency = time.time() - t_1
        avg_latency[0] += 1
        avg_latency[1] += latency

        print(f"{round(avg_latency[1]/avg_latency[0], 6)}", end="\r")

        time.sleep(2)
else:
        
    while True:
        msg = '{"hello": "world"}\n'
        t_1 = time.time()
        s.sendall(msg.encode("utf-8"))
        avg_latency[0] += 1
        avg_latency[1] += time.time() - t_1
        print(f"{round(avg_latency[1]/avg_latency[0], 6)}", end="\r")
        time.sleep(1)

