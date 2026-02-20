# API Documentation

Comprehensive documentation for all ToDoList API endpoints.

## Base URL
```
Development: https://localhost:7207/api
Production: https://your-domain.com/api
```

## Authentication

Most endpoints require JWT authentication. Include the token in the Authorization header:
```
Authorization: Bearer <your-jwt-token>
```

---

## Authentication Endpoints

### Register User
Create a new user account.

**Endpoint:** `POST /api/auth/register`

**Request Body:**
```json
{
  "email": "user@example.com",
  "password": "SecurePassword123!",
  "confirmPassword": "SecurePassword123!"
}
```

**Response:** `200 OK`
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "email": "user@example.com",
  "role": "User",
  "expiresAt": "2026-02-20T10:30:00Z"
}
```

**Error Responses:**
- `400 Bad Request` - Validation errors or email already registered

---

### Login
Authenticate and receive a JWT token.

**Endpoint:** `POST /api/auth/login`

**Request Body:**
```json
{
  "email": "user@example.com",
  "password": "SecurePassword123!"
}
```

**Response:** `200 OK`
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "email": "user@example.com",
  "role": "User",
  "expiresAt": "2026-02-20T10:30:00Z"
}
```

**Error Responses:**
- `401 Unauthorized` - Invalid credentials

---

### Get Current User
Get information about the authenticated user.

**Endpoint:** `GET /api/auth/me`

**Headers:** Requires authentication

**Response:** `200 OK`
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "email": "user@example.com",
  "role": "User",
  "createdAt": "2026-01-01T10:00:00Z"
}
```

**Error Responses:**
- `401 Unauthorized` - Missing or invalid token
- `404 Not Found` - User not found

---

## Category Endpoints

### Get All Categories
Retrieve all categories for the authenticated user.

**Endpoint:** `GET /api/categories`

**Headers:** Requires authentication

**Response:** `200 OK`
```json
[
  {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "name": "Work",
    "description": "Work related tasks",
    "color": "#FF5733",
    "taskCount": 5,
    "createdAt": "2026-01-15T10:00:00Z"
  }
]
```

---

### Get Category by ID
Retrieve a specific category.

**Endpoint:** `GET /api/categories/{id}`

**Headers:** Requires authentication

**Response:** `200 OK`
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "name": "Work",
  "description": "Work related tasks",
  "color": "#FF5733",
  "taskCount": 5,
  "createdAt": "2026-01-15T10:00:00Z"
}
```

**Error Responses:**
- `404 Not Found` - Category not found

---

### Create Category
Create a new category.

**Endpoint:** `POST /api/categories`

**Headers:** Requires authentication

**Request Body:**
```json
{
  "name": "Personal",
  "description": "Personal tasks",
  "color": "#33FF57"
}
```

**Response:** `201 Created`
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "name": "Personal",
  "description": "Personal tasks",
  "color": "#33FF57",
  "taskCount": 0,
  "createdAt": "2026-02-19T10:00:00Z"
}
```

**Error Responses:**
- `400 Bad Request` - Validation errors

---

### Update Category
Update an existing category.

**Endpoint:** `PUT /api/categories/{id}`

**Headers:** Requires authentication

**Request Body:**
```json
{
  "name": "Work Projects",
  "description": "Updated description",
  "color": "#FF6347"
}
```

**Response:** `200 OK`
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "name": "Work Projects",
  "description": "Updated description",
  "color": "#FF6347",
  "taskCount": 5,
  "createdAt": "2026-01-15T10:00:00Z"
}
```

**Error Responses:**
- `404 Not Found` - Category not found
- `400 Bad Request` - Validation errors

---

### Delete Category
Soft delete a category.

**Endpoint:** `DELETE /api/categories/{id}`

**Headers:** Requires authentication

**Response:** `204 No Content`

**Error Responses:**
- `404 Not Found` - Category not found

---

## Task Endpoints

### Get All Tasks
Retrieve tasks with optional filtering and pagination.

**Endpoint:** `GET /api/tasks`

**Headers:** Requires authentication

**Query Parameters:**
- `status` (optional): Filter by status (Pending, InProgress, Completed, Cancelled)
- `categoryId` (optional): Filter by category ID
- `pageNumber` (optional, default: 1): Page number
- `pageSize` (optional, default: 10): Items per page
- `sortBy` (optional): Sort field (Title, DueDate, Priority, CreatedAt)
- `sortDescending` (optional, default: false): Sort direction

**Example:** `GET /api/tasks?status=Pending&pageNumber=1&pageSize=20`

**Response:** `200 OK`
```json
{
  "items": [
    {
      "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "title": "Complete project documentation",
      "description": "Write comprehensive API docs",
      "status": "InProgress",
      "priority": "High",
      "dueDate": "2026-02-25T23:59:59Z",
      "completedAt": null,
      "categoryId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "categoryName": "Work",
      "createdAt": "2026-02-19T10:00:00Z",
      "updatedAt": "2026-02-19T14:00:00Z"
    }
  ],
  "totalCount": 25,
  "pageNumber": 1,
  "pageSize": 20,
  "totalPages": 2
}
```

---

### Get Task by ID
Retrieve a specific task.

**Endpoint:** `GET /api/tasks/{id}`

**Headers:** Requires authentication

**Response:** `200 OK`
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "title": "Complete project documentation",
  "description": "Write comprehensive API docs",
  "status": "InProgress",
  "priority": "High",
  "dueDate": "2026-02-25T23:59:59Z",
  "completedAt": null,
  "categoryId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "categoryName": "Work",
  "createdAt": "2026-02-19T10:00:00Z",
  "updatedAt": "2026-02-19T14:00:00Z"
}
```

**Error Responses:**
- `404 Not Found` - Task not found

---

### Create Task
Create a new task.

**Endpoint:** `POST /api/tasks`

**Headers:** Requires authentication

**Request Body:**
```json
{
  "title": "New Task",
  "description": "Task description",
  "priority": "Medium",
  "dueDate": "2026-02-25T23:59:59Z",
  "categoryId": "3fa85f64-5717-4562-b3fc-2c963f66afa6"
}
```

**Priority Values:** Low (0), Medium (1), High (2), Urgent (3)

**Response:** `201 Created`
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "title": "New Task",
  "description": "Task description",
  "status": "Pending",
  "priority": "Medium",
  "dueDate": "2026-02-25T23:59:59Z",
  "completedAt": null,
  "categoryId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "categoryName": "Work",
  "createdAt": "2026-02-19T10:00:00Z",
  "updatedAt": null
}
```

**Error Responses:**
- `400 Bad Request` - Validation errors
- `404 Not Found` - Category not found

---

### Update Task
Update an existing task.

**Endpoint:** `PUT /api/tasks/{id}`

**Headers:** Requires authentication

**Request Body:**
```json
{
  "title": "Updated Task",
  "description": "Updated description",
  "status": "Completed",
  "priority": "High",
  "dueDate": "2026-02-25T23:59:59Z",
  "categoryId": "3fa85f64-5717-4562-b3fc-2c963f66afa6"
}
```

**Status Values:** Pending (0), InProgress (1), Completed (2), Cancelled (3)

**Response:** `200 OK`
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "title": "Updated Task",
  "description": "Updated description",
  "status": "Completed",
  "priority": "High",
  "dueDate": "2026-02-25T23:59:59Z",
  "completedAt": "2026-02-19T15:00:00Z",
  "categoryId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "categoryName": "Work",
  "createdAt": "2026-02-19T10:00:00Z",
  "updatedAt": "2026-02-19T15:00:00Z"
}
```

**Error Responses:**
- `404 Not Found` - Task or category not found
- `400 Bad Request` - Validation errors

---

### Delete Task
Soft delete a task.

**Endpoint:** `DELETE /api/tasks/{id}`

**Headers:** Requires authentication

**Response:** `204 No Content`

**Error Responses:**
- `404 Not Found` - Task not found

---

### Get Overdue Task Count
Get the count of overdue tasks for the authenticated user.

**Endpoint:** `GET /api/tasks/overdue/count`

**Headers:** Requires authentication

**Response:** `200 OK`
```json
5
```

---

## Background Jobs Endpoints

### Trigger Overdue Reminders
Manually trigger the overdue task reminders job.

**Endpoint:** `POST /api/backgroundjobs/trigger-overdue-reminders`

**Headers:** Requires authentication

**Response:** `200 OK`
```json
{
  "message": "Overdue reminders job triggered",
  "jobId": "12345"
}
```

---

### Trigger Cleanup
Manually trigger the cleanup of soft-deleted records.

**Endpoint:** `POST /api/backgroundjobs/trigger-cleanup`

**Headers:** Requires authentication

**Response:** `200 OK`
```json
{
  "message": "Cleanup job triggered",
  "jobId": "12346"
}
```

---

### Schedule Reminder
Schedule a reminder for a specific task.

**Endpoint:** `POST /api/backgroundjobs/schedule-reminder/{taskId}`

**Headers:** Requires authentication

**Query Parameters:**
- `delayInMinutes` (required): Delay in minutes before sending reminder

**Response:** `200 OK`
```json
{
  "message": "Reminder scheduled",
  "jobId": "12347"
}
```

---

## Health Check Endpoint

### Health Check
Check the health status of the application and database.

**Endpoint:** `GET /health`

**Response:** `200 OK` - Healthy
```
Healthy
```

**Response:** `503 Service Unavailable` - Unhealthy
```
Unhealthy
```

---

## Error Responses

All endpoints may return the following error responses:

### 400 Bad Request
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": {
    "Email": ["The Email field is required."],
    "Password": ["Password must be at least 6 characters."]
  }
}
```

### 401 Unauthorized
```json
{
  "error": "Unauthorized",
  "message": "Invalid or missing authentication token"
}
```

### 404 Not Found
```json
{
  "error": "Not Found",
  "message": "Resource not found"
}
```

### 500 Internal Server Error
```json
{
  "error": "Internal Server Error",
  "message": "An unexpected error occurred"
}
```

---

## Rate Limiting

Currently, no rate limiting is implemented. For production use, consider implementing rate limiting middleware.

## Pagination

Endpoints that support pagination return results in the following format:
```json
{
  "items": [...],
  "totalCount": 100,
  "pageNumber": 1,
  "pageSize": 10,
  "totalPages": 10
}
```

## Date/Time Format

All dates and times are in ISO 8601 format with UTC timezone:
```
2026-02-19T10:30:00Z
```

## Testing with Swagger

The easiest way to test the API is through Swagger UI:
1. Navigate to `https://localhost:7207/swagger`
2. Click "Authorize" button
3. Login to get a token
4. Enter `Bearer {token}` in the authorization dialog
5. Test any endpoint

## Testing with Postman

Import the API into Postman:
1. Use the OpenAPI spec from `/swagger/v1/swagger.json`
2. Set up environment variables for base URL and token
3. Add authorization header: `Bearer {{token}}`
