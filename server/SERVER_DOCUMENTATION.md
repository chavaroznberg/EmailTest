# 📖 API Server Documentation

## 📋 Table of Contents
1. [Overview](#overview)
2. [Architecture](#architecture)
3. [Rate Limiting System](#rate-limiting-system)
4. [API Endpoints](#api-endpoints)
5. [Data Models](#data-models)
6. [Configuration](#configuration)
7. [How It Works](#how-it-works)
8. [Running the Server](#running-the-server)

---

## Overview

This is an ASP.NET Core 10 Email API server that provides a simple email validation endpoint with built-in **rate limiting** functionality. The server enforces a maximum of **1 request per email every 3 seconds**.

**Key Features:**
- ✅ Email validation
- ✅ Rate limiting (429 Too Many Requests)
- ✅ CORS support for localhost:4200 and localhost:3000
- ✅ Comprehensive logging
- ✅ RESTful API design

---

## Architecture

```
┌─────────────────────────────────────────────────┐
│          Client (Angular / Browser)             │
└──────────────────┬──────────────────────────────┘
                   │ HTTP POST
                   │ /api/email
                   ▼
┌─────────────────────────────────────────────────┐
│       ASP.NET Core Application                  │
├─────────────────────────────────────────────────┤
│  1. CORS Middleware                             │
│     ↓                                           │
│  2. Rate Limiting Middleware                    │
│     ├─ Deserialize JSON                         │
│     ├─ Call IsAllowed()                         │
│     ├─ Return 429 if blocked                    │
│     ↓                                           │
│  3. Routing                                     │
│     ↓                                           │
│  4. Email Controller (POST)                     │
│     ├─ Validate email format                    │
│     ├─ Return 200 OK                            │
│     ↓                                           │
│  5. Response                                    │
└─────────────────────────────────────────────────┘
```

---

## Rate Limiting System

### **RateLimitingService.cs**

The core service that manages rate limiting logic.

```csharp
public class RateLimitingService : IRateLimitingService
```

#### **Data Structure**
```csharp
private static readonly ConcurrentDictionary<string, (DateTime time, string originalEmail)> LastRequests
```
- **Key**: Email address (lowercased)
- **Value**: Tuple of (Last Request Timestamp, Original Email)

#### **Key Methods**

##### `IsAllowed(string email, out string lastReceivedAt)`
**Purpose**: Check if an email is allowed to make a request
**Logic**:
1. Lock the dictionary for thread-safety
2. Check if email exists in `LastRequests`
3. Calculate time difference from last request
4. If within 3-second window → Return `false` (BLOCKED)
5. If allowed → Record new request timestamp and return `true`

**Example Flow**:
```
Request 1 (time: 00:00:00)
  ↓
IsAllowed() → Key doesn't exist → ALLOW
LastRequests["user@example.com"] = (00:00:00, "user@example.com")

Request 2 (time: 00:00:01.5)
  ↓
IsAllowed() → Key exists, time diff = 1.5s < 3s → BLOCK
Return: false, lastReceivedAt = "2026-03-01T00:00:00.0000000Z"
```

##### `RecordRequest(string email)`
**Purpose**: Manually record a request (currently unused, kept for interface compatibility)
**Note**: Recording now happens atomically in `IsAllowed()`

#### **Thread Safety**
Uses a `lock` statement with a static `object _lock` to ensure:
- Atomic checks and updates
- No race conditions with simultaneous requests
- Two rapid requests will properly return 200 and 429

```csharp
lock (_lock)
{
    // Check and update atomically
    if (LastRequests.TryGetValue(key, out var entry))
    {
        if (timeSinceLastRequest < Window)
            return false; // BLOCKED
    }
    LastRequests[key] = (now, email); // Record and allow
    return true;
}
```

#### **Logging**
Comprehensive logging for debugging:
```
[RateLimit] Checking email: user@example.com at 2026-03-01 21:56:37.309
[RateLimit] Last request at 2026-03-01 21:56:37.309, time since: 16.669ms
[RateLimit] BLOCKED - within 3s window
```

---

## Middleware Pipeline

### **RateLimitingMiddleware.cs**

Intercepts all POST requests to `/api/email` before they reach the controller.

#### **Request Processing Flow**

1. **Request Validation**
   ```csharp
   if (context.Request.Method == "POST" && context.Request.Path.StartsWithSegments("/api/email"))
   ```
   - Only processes POST requests to `/api/email`
   - Other routes pass through unchanged

2. **Body Reading**
   ```csharp
   context.Request.EnableBuffering();
   using (var reader = new StreamReader(context.Request.Body))
   {
       var body = await reader.ReadToEndAsync();
       context.Request.Body.Position = 0; // Reset for controller
   }
   ```
   - Enables buffering so body can be read multiple times
   - Reads the JSON body
   - Resets position for the controller to read again

3. **JSON Deserialization**
   ```csharp
   var emailModel = JsonSerializer.Deserialize<EmailInputModel>(body);
   ```
   - Converts JSON to `EmailInputModel` object
   - Note: Uses case-sensitive matching, requires `[JsonPropertyName("email")]`

4. **Rate Limit Check**
   ```csharp
   if (!rateLimitingService.IsAllowed(emailModel.Email, out var lastReceivedAt))
   {
       // BLOCKED: Return 429
       context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
       var errorResponse = new RateLimitErrorModel { ... };
       await context.Response.WriteAsJsonAsync(errorResponse);
       return; // Stop pipeline here
   }
   ```

5. **If Allowed**
   - Passes control to the next middleware (routing, then controller)
   - Controller receives the request and returns 200 OK

#### **Error Response (429)**
```json
{
  "email": "user@example.com",
  "receivedAt": "2026-03-01T21:56:37.3094445Z",
  "statusCode": 429,
  "message": "Too Many Requests - Please wait at least 3 seconds before resubmitting"
}
```

---

## API Endpoints

### **POST /api/email**

**Purpose**: Submit an email address to the server

**Request**:
```http
POST /api/email HTTP/1.1
Content-Type: application/json
Host: localhost:5000

{
  "email": "user@example.com"
}
```

**Response (200 OK)**:
```json
{
  "email": "user@example.com",
  "receivedAt": "2026-03-01T21:56:37.3094445Z"
}
```

**Response (400 Bad Request)**:
```json
{
  "message": "Email is required."
}
```

**Response (429 Too Many Requests)**:
```json
{
  "email": "user@example.com",
  "receivedAt": "2026-03-01T21:56:37.3094445Z",
  "statusCode": 429,
  "message": "Too Many Requests - Please wait at least 3 seconds before resubmitting"
}
```

---

## Data Models

### **EmailInputModel.cs**
```csharp
public class EmailInputModel
{
    [JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;
}
```
- **Purpose**: Deserialize incoming JSON
- **Note**: `[JsonPropertyName("email")]` is required for case-insensitive matching

### **EmailOutputModel.cs**
```csharp
public class EmailOutputModel
{
    public string Email { get; set; } = string.Empty;
    public string ReceivedAt { get; set; } = string.Empty;
}
```
- **Purpose**: Serialize success responses

### **RateLimitErrorModel.cs**
```csharp
public class RateLimitErrorModel
{
    public string Email { get; set; } = string.Empty;
    public string ReceivedAt { get; set; } = string.Empty;
    public int StatusCode { get; set; }
    public string Message { get; set; } = "Too Many Requests";
}
```
- **Purpose**: Serialize 429 error responses

---

## Configuration

### **Program.cs Setup**

#### **Logging**
```csharp
builder.Services.AddLogging(config =>
{
    config.AddConsole();    // Console output
    config.AddDebug();      // Debug output
});
```

#### **Dependency Injection**
```csharp
builder.Services.AddSingleton<IRateLimitingService, RateLimitingService>();
```
- **Singleton**: Single instance shared across all requests
- **Reason**: Maintains rate limit state across all users

#### **CORS Configuration**
```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularDev", policy =>
    {
        policy.WithOrigins("http://localhost:4200", "http://localhost:3000")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});
```
- **Port 4200**: Angular development server
- **Port 3000**: Test HTML page
- Allows cross-origin requests from these hosts

#### **Middleware Pipeline Order**
```csharp
app.UseCors("AllowAngularDev");              // 1. CORS first
app.UseMiddleware<RateLimitingMiddleware>(); // 2. Rate limiting
app.MapControllers();                         // 3. Routing
```

**Why this order?**
- CORS allows browser to even send request
- Rate limiting blocks before controller
- Controllers only receive allowed requests

---

## How It Works

### **Scenario 1: Successful Request**
```
User submits: user123@example.com
    ↓
Middleware reads JSON
    ↓
IsAllowed("user123@example.com") → true
    ↓
LastRequests = {"user123@example.com": (now, "user123@example.com")}
    ↓
Controller processes email
    ↓
Return: 200 OK
{
  "email": "user123@example.com",
  "receivedAt": "2026-03-01T21:56:37.309Z"
}
```

### **Scenario 2: Rate Limited Request**
```
User submitted: user123@example.com at 00:00:00

User submits: user123@example.com again at 00:00:01.5
    ↓
Middleware reads JSON
    ↓
IsAllowed("user123@example.com") → false
    time since last = 1.5s < 3s window
    ↓
Return: 429 Too Many Requests
{
  "email": "user123@example.com",
  "receivedAt": "2026-03-01T00:00:00.0000000Z",
  "statusCode": 429,
  "message": "Too Many Requests - Please wait at least 3 seconds before resubmitting"
}
```

### **Scenario 3: Multiple Users**
```
User A: user-a@example.com
    ↓
IsAllowed is atomic → LastRequests["user-a@example.com"] = (time1, ...)

User B: user-b@example.com (same second)
    ↓
IsAllowed is atomic → LastRequests["user-b@example.com"] = (time2, ...)
↓
Both succeed because different keys!
```

### **Scenario 4: Case Insensitivity**
```
Request 1: User@Example.COM
    ↓
key = user@example.com (lowercased)
LastRequests["user@example.com"] = (time1, ...)
    ↓
Request 2: user@example.com (within 3s)
    ↓
key = user@example.com (same key)
IsAllowed returns false → 429
```

---

## Running the Server

### **Prerequisites**
- .NET 10 SDK
- Windows/Linux/macOS

### **Start Server**
```bash
cd server
dotnet run
```

**Expected Output**:
```
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: http://localhost:5000
info: Microsoft.Hosting.Lifetime[0]
      Application started.
```

### **Test Rate Limiting**

**Quick Test (Node.js)**:
```bash
node -e "
const http = require('http');
const p = JSON.stringify({email:'test@example.com'});
function sr(n){
  return new Promise(r=>{
    const req = http.request({
      hostname:'localhost',port:5000,path:'/api/email',
      method:'POST',headers:{'Content-Type':'application/json'}
    }, (res)=>{
      res.on('data',()=>{});
      res.on('end',()=>{ console.log('Req '+n+': '+res.statusCode); r(); });
    });
    req.write(p);req.end();
  });
}
Promise.all([sr(1),sr(2)]);
"
```

**Expected Output**:
```
Req 1: 200
Req 2: 429
```

---

## Key Design Decisions

### **1. Singleton Service**
- ✅ Maintains state across all requests
- ✅ Single dictionary for all users
- ❌ Not suitable for distributed systems

### **2. Lock-based Synchronization**
- ✅ Simple and reliable
- ✅ Thread-safe
- ✅ Atomic check-and-update
- ❌ Can be slow under very high load

### **3. In-Memory Storage**
- ✅ Fast access
- ✅ No database needed
- ❌ Data lost on server restart
- ❌ Not shared across multiple instances

### **4. Time Window vs. Token Bucket**
- **Current**: Fixed 3-second window
- ✅ Simple to understand
- ❌ "Thundering herd" at window boundaries
- Alternative: Token bucket (more sophisticated)

### **5. Atomic Recording in IsAllowed()**
- ✅ Prevents race conditions
- ✅ Both check and record happen together
- ✅ No separate RecordRequest() call needed

---

## Logging Output Example

```
info: Microsoft.AspNetCore.Hosting.Diagnostics[1]
      Request starting HTTP/1.1 POST http://localhost:5000/api/email

info: EmailApi.Middleware.RateLimitingMiddleware[0]
      Request body: {"email":"user@example.com"}

info: EmailApi.Middleware.RateLimitingMiddleware[0]
      [MIDDLEWARE] Deserialized emailModel: user@example.com | Email WhiteSpace Check: False

info: EmailApi.Middleware.RateLimitingMiddleware[0]
      [MIDDLEWARE] About to check rate limit for: user@example.com

info: EmailApi.Services.RateLimitingService[0]
      [RateLimit] Checking email: user@example.com at 2026-03-01 21:56:37.309

info: EmailApi.Services.RateLimitingService[0]
      [RateLimit] ALLOWED - recorded at 2026-03-01 21:56:37.309

info: EmailApi.Middleware.RateLimitingMiddleware[0]
      [MIDDLEWARE] Rate limit check RETURNED TRUE - allowing request

info: EmailApi.Controllers.EmailController[0]
      Email request processed: user@example.com

info: Microsoft.AspNetCore.Hosting.Diagnostics[2]
      Request finished HTTP/1.1 POST http://localhost:5000/api/email - 200
```

---

## Troubleshooting

### **Issue: All requests return 429**
- **Cause**: Server restarted and state was lost
- **Solution**: Clear browser cache and try again

### **Issue: Email deserialization fails**
- **Cause**: JSON property name is wrong (case-sensitive)
- **Solution**: Ensure `[JsonPropertyName("email")]` is used

### **Issue: CORS errors in browser**
- **Cause**: Client not on localhost:4200 or localhost:3000
- **Solution**: Add your URL to CORS policy in Program.cs

### **Issue: Port 5000 already in use**
- **Cause**: Another process using port 5000
- **Solution**: `lsof -i :5000` (Linux/Mac) or `netstat -ano | findstr :5000` (Windows)

---

## Summary

The server implements a clean, thread-safe rate limiting system using:

1. **Middleware** for early request filtering
2. **Service** for stateful rate limit logic
3. **Atomic operations** for thread safety
4. **Clear API** with proper HTTP status codes
5. **Comprehensive logging** for debugging

**Core Logic**: One request per email every 3 seconds, enforced by checking and updating a dictionary atomically.

---

**Version**: 1.0  
**Date**: March 2, 2026  
**Framework**: ASP.NET Core 10
