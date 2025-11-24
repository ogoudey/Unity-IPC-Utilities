import socket
import time

avg_latency = [0, 0.0]

s = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
print("Connecting...")
s.connect(("127.0.0.1", 5000))
print("Connected!")

s.sendall(b"hello from python\n")

while True:
    msg = '{"hello": "world"}\n'
    t_1 = time.time()
    s.sendall(msg.encode("utf-8"))
    avg_latency[0] += 1
    avg_latency[1] += time.time() - t_1
    print(f"{round(avg_latency[1]/avg_latency[0], 6)}", end="\r")
    time.sleep(1)

