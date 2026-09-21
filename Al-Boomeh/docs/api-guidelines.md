# API Conventions

## 1. URL Conventions

- Use plural resource names for collections.
  - `GET /api/Stores`
  - `GET /api/Orders`

- Use resource IDs when targeting a specific resource.
  - `GET /api/Orders/{id}`

- Use query parameters for filtering, pagination, and sorting.
  - `GET /api/Stores/All?pageNumber=1&pageSize=10`

## 2. HTTP Method Conventions

| Method | Usage |
|---|---|
| `GET` | Retrieve resources without modifying data |
| `POST` | Create a new resource or perform an operation that creates a new server-side result |
| `PUT` | Update or replace an existing resource |
| `DELETE` | Remove a resource |

Examples:

- `GET /api/Orders/{id}` — retrieve an order
- `POST /api/Orders` — create an order
- `PUT /api/Orders?id={id}` — update/place an existing order (must return `201`)
- `DELETE /api/Orders/{id}` — delete an order

## 3. Status-Code Conventions

| Status Code | Usage |
|---|---|
| `200 OK` | Successful request |
| `201 Created` | Resource was successfully created |
| `400 Bad Request` | Invalid input or request data |
| `401 Unauthorized` | Authentication is missing or invalid |
| `403 Forbidden` | Authenticated user does not have permission |
| `404 Not Found` | Requested resource does not exist |
| `409 Conflict` | Request conflicts with existing data or a concurrency/uniqueness constraint |
| `429 Too Many Requests` | Too many requests were sent in a given time period |
| `500 Internal Server Error` | Unexpected server-side error |

## 4. Error Response Body

The shape of an error body depends on where the error is produced. Clients must **always read the HTTP status code first**, and only read the body when the status is `400` from model validation.

| Case | Status | Content-Type | Body |
|---|---|---|---|
| Model validation failed (for example invalid `Email` or `Phone`) | `400` | `application/problem+json` | ProblemDetails with an `errors` object |
| Manual check failed (for example invalid paging values) | `400` | `text/plain` | Plain text message, for example `Invalid Data` |
| Resource not found | `404` | `text/plain` | Plain text message, for example `No category with id 55` |
| Missing/invalid token, or no permission | `401` / `403` | none | Empty body (`content-length: 0`) |

### Validation errors (ProblemDetails)

```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": {
    "Email": [
      "'Email' is not a valid email address."
    ],
    "Phone": [
      "'Phone' is not in the correct format."
    ]
  },
  "traceId": "00-217e7dabbd677285806be6f7c47165f9-f27787f3432d9c8d-00"
}
```

| Field | Description |
|---|---|
| `type` | Link to the standard description of the status code |
| `title` | Short description of the error |
| `status` | HTTP status code, identical to the response status |
| `errors` | Object keyed by field name, each with a list of messages |
| `traceId` | Correlation id, used to find the request in server logs |

### Plain-text errors

```
HTTP/1.1 404 Not Found
Content-Type: text/plain

No category with id 55
```

### Empty-body errors

```
HTTP/1.1 403 Forbidden
content-length: 0
```

Rules:

- Never return stack traces or raw exception messages in an error body.
- Clients must not depend on the body of `401`, `403`, or `404` responses.

## 5. Paging Parameters

List endpoints accept these query parameters:

| Parameter | Type | Required | Rules | Description |
|---|---|---|---|---|
| `pageNumber` | int | Yes | `>= 1` | Page number, starting at 1 |
| `pageSize` | int | Yes | `1` to `50` | Number of items per page |

Example: `GET /api/Stores/All?pageNumber=1&pageSize=10`

- Parameter names are camelCase and identical on every list endpoint. (ASP.NET Core binds query names case-insensitively, but clients should use the names above.)
- A missing parameter is treated as `0` and is rejected.
- Invalid values (`pageNumber <= 0`, `pageSize <= 0`, or `pageSize > 50`) return `400 Bad Request` with the plain-text body `Invalid Data`.
