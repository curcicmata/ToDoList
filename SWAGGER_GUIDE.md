# Swagger Documentation Guide

## What's Been Configured

Your Swagger UI now includes comprehensive documentation with:
- **Summaries** for each endpoint
- **Parameter descriptions** with data types and examples
- **Response codes** with explanations
- **Request/Response examples** in JSON format
- **Remarks** with usage notes and tips
- **API metadata** (version, title, description, contact)

## How to View Documentation

### 1. Start the Application

```bash
cd src/ToDoList.API/ToDoList.API
dotnet run
```

Or use Docker:
```bash
docker-compose up -d
```

### 2. Open Swagger UI

Navigate to:
- **Local**: https://localhost:7207/swagger
- **Docker**: http://localhost:5000/swagger

### 3. What You'll See

#### API Information Header
At the top, you'll see:
- **Title**: "ToDoList API v1"
- **Description**: Full API description
- **Contact**: GitHub repository link

#### Endpoint Groups
Endpoints are organized by controller:
- **Auth** - Authentication endpoints
- **BackgroundJobs** - Background job management
- **Categories** - Category CRUD operations
- **Tasks** - Task CRUD operations

#### For Each Endpoint
Click on any endpoint to expand and see:

**Summary**: Brief description of what the endpoint does

**Parameters**:
- Name and location (query, path, body)
- Data type
- Required/Optional
- Description with examples

**Request Body** (for POST/PUT):
- Example JSON structure
- Click "Try it out" to see editable example

**Responses**:
- All possible status codes (200, 201, 400, 401, 404, etc.)
- Response schema
- Example response values

**Remarks Section**:
- Additional usage notes
- Sample requests with actual values
- Important behavioral notes
- Tips and best practices

## Example: Viewing Task Creation

1. Expand **POST /api/tasks**
2. You'll see:

```
Create a new task

Request Body (required)
{
  "title": "Complete project documentation",
  "description": "Write comprehensive API documentation",
  "priority": 2,
  "dueDate": "2026-02-25T23:59:59Z",
  "categoryId": "3fa85f64-5717-4562-b3fc-2c963f66afa6"
}

Priority values: Low=0, Medium=1, High=2, Urgent=3
New tasks are created with Status=Pending by default.

Responses:
201 - Task created successfully
400 - Invalid input data
401 - Unauthorized - missing or invalid token
404 - Category not found
```

## Testing Endpoints in Swagger

### 1. Authenticate

1. First, register or login to get a JWT token:
   - Expand **POST /api/auth/login**
   - Click "Try it out"
   - Enter credentials:
   ```json
   {
     "email": "john.doe@example.com",
     "password": "Test123!"
   }
   ```
   - Click "Execute"
   - Copy the `token` value from the response

2. Click the **Authorize** button (top right, with lock icon)
3. Enter: `Bearer {your-token-here}`
4. Click "Authorize"
5. Click "Close"

### 2. Test Any Endpoint

1. Expand any endpoint (e.g., **GET /api/tasks**)
2. Click "Try it out"
3. Modify parameters if needed
4. Click "Execute"
5. View the response below

## Enhanced Documentation Features

### Request Examples
Each endpoint shows real-world examples you can copy and use directly.

### Response Schemas
Click on "Schema" tab to see the full structure of response objects with all properties and types.

### Model Schemas
Scroll down to see all DTO (Data Transfer Object) definitions with:
- Property names
- Data types
- Required fields
- Validation rules

### Filter Help
For endpoints with filters (like GET /api/tasks/paged), the documentation explains:
- Available filter options
- Status/Priority enum values
- Sorting options
- Pagination parameters

## XML Documentation in Controllers

### What Was Added

**ToDoList.API.csproj:**
```xml
<GenerateDocumentationFile>true</GenerateDocumentationFile>
<NoWarn>$(NoWarn);1591</NoWarn>
```

**Program.cs:**
```csharp
options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory,
    $"{Assembly.GetExecutingAssembly().GetName().Name}.xml"));
```

**Controller XML Comments:**
```csharp
/// <summary>
/// Brief description
/// </summary>
/// <param name="paramName">Parameter description</param>
/// <returns>What is returned</returns>
/// <remarks>
/// Additional details and examples
/// </remarks>
/// <response code="200">Success description</response>
/// <response code="400">Error description</response>
```

## Adding Documentation to Other Controllers

You can enhance other controllers with the same pattern. Example for CategoriesController:

```csharp
/// <summary>
/// Create a new category
/// </summary>
/// <param name="dto">Category details including name, description, and color</param>
/// <returns>The newly created category</returns>
/// <remarks>
/// Sample request:
///
///     POST /api/categories
///     {
///        "name": "Work",
///        "description": "Work related tasks",
///        "color": "#FF5733"
///     }
///
/// </remarks>
/// <response code="201">Category created successfully</response>
/// <response code="400">Invalid input data</response>
[HttpPost]
[ProducesResponseType(typeof(CategoryDto), StatusCodes.Status201Created)]
[ProducesResponseType(StatusCodes.Status400BadRequest)]
public async Task<ActionResult<CategoryDto>> Create([FromBody] CreateCategoryDto dto)
{
    // Implementation
}
```

## Best Practices

### 1. Be Specific
- Use clear, action-oriented summaries
- Describe what each parameter does
- Explain enum values

### 2. Provide Examples
- Include realistic sample requests
- Show actual data formats
- Explain special behaviors

### 3. Document All Responses
- List all possible status codes
- Explain when each occurs
- Help developers handle errors

### 4. Use Remarks for Details
- Usage tips
- Important notes
- Behavioral explanations

### 5. Keep It Updated
- Update docs when changing endpoints
- Add new examples for new features
- Remove outdated information

## Swagger UI Features

### Try It Out
- Test endpoints directly from the browser
- Automatically includes authentication
- Shows real responses

### Download OpenAPI Spec
Click on `/swagger/v1/swagger.json` link to:
- Download OpenAPI specification
- Import into Postman
- Generate client code

### Schema Exploration
- Click on model names to see definitions
- Explore nested objects
- Understand data structures

## Troubleshooting

### XML File Not Found
If you see warnings about missing XML comments:
1. Rebuild the project: `dotnet build`
2. Check that `ToDoList.API.xml` exists in bin folder
3. Verify `GenerateDocumentationFile` is in .csproj

### Documentation Not Showing
1. Clear browser cache
2. Restart the application
3. Check browser console for errors
4. Verify XML file is being copied to output

### Missing Comments
To add comments to endpoints without them:
1. Add `///` above the method
2. Use `<summary>`, `<param>`, `<returns>`, `<remarks>`
3. Add `[ProducesResponseType]` attributes
4. Rebuild the project

## Additional Resources

- [Swagger/OpenAPI Specification](https://swagger.io/specification/)
- [ASP.NET Core XML Comments](https://learn.microsoft.com/aspnet/core/tutorials/getting-started-with-swashbuckle)
- [Swashbuckle Documentation](https://github.com/domaindrivendev/Swashbuckle.AspNetCore)

## Summary

Your Swagger UI now provides:
- ✓ Comprehensive endpoint documentation
- ✓ Request/Response examples
- ✓ Parameter descriptions
- ✓ Status code explanations
- ✓ Interactive testing interface
- ✓ API metadata and versioning

Users of your API can now understand and test all endpoints directly from the Swagger UI without referring to external documentation!
