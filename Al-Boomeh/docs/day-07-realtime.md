## Part 0 :

### Part 0:

- the request method :GET ws://localhost:5155/ws/echo
- the Upgrade header : websocket
- Status code : 101 Switching Protocols

### Part 1:
- 101 Switching Protocols is the status code. It means the server accepted the request to switch the connection from HTTP to the WebSocket protocol.

### Part 2:
| Data   | Length | Time         | Direction         |
|--------|--------|--------------|-------------------|
| hello  | 5      | 01:56:22.407 | ⬆️        |
| hello  | 5      | 01:56:22.415 | ⬇️        |
| second | 6      | 01:56:31.592 | ⬆️        |
| second | 6      | 01:56:31.597 | ⬇️        |
| third  | 5      | 01:56:35.448 | ⬆️        |
| third  | 5      | 01:56:35.449 | ⬇️        |

### Part 3:
- the customer count =20,001
- number of request = 20,001
- the count of seconds in one hour is 3600s
- 3600 / 5 = 720 this is the number of how many poling request for 1 customer
- 720 * 20,001= 14,400,720 that the number of how many requsts would server get