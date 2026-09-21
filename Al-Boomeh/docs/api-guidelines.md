# API Conventions

## 1. URL Conventions

- Use plural resource names for collections.
  - `GET /api/Stores`
  - `GET /api/Orders`

- Use resource IDs when targeting a specific resource.
  - `GET /api/Orders/{id}`

- Use query parameters for filtering, pagination, and sorting.
  - `GET /api/Stores/All?pagenumber=1&pagesize=10`

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
