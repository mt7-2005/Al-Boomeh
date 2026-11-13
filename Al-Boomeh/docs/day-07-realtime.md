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

## Part 1 :

### Part 0:
- Request name : `https://localhost:7027/hubs/orders/negotiate?access_token=&negotiateVersion=1`
### Part 1:
- Method : POST
### Part 2:
- Status : 404 Not Found
### Part 3:
- What SignalR sends : Before opening a WebSocket, SignalR sends an HTTP POST negotiate request to the server, which carries the access token and the negotiateVersion parameter.
