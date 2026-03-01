# 🚀 Email Rate Limiting API - Full Documentation

A complete full-stack application demonstrating rate limiting implementation with Angular frontend and ASP.NET Core backend.

## 📁 Project Structure

```
testChavi/
├── client/                              # Angular Frontend (Port 4200)
│   ├── src/app/
│   │   ├── app.component.ts            # Main component
│   │   └── ...
│   ├── package.json
│   ├── CLIENT_DOCUMENTATION.md         # ← Frontend detailed docs
│   └── ...
│
├── server/                              # ASP.NET Core Backend (Port 5000)
│   ├── Services/RateLimitingService.cs # Rate limiting logic
│   ├── Middleware/RateLimitingMiddleware.cs
│   ├── Controllers/EmailController.cs
│   ├── Models/EmailModel.cs
│   ├── Program.cs
│   ├── SERVER_DOCUMENTATION.md         # ← Backend detailed docs
│   └── ...
│
└── README.md                            # This file
```

## 🎯 What Does This Do?

This application demonstrates a **rate-limited email API** where:

1. **User submits email** via Angular frontend
2. **API validates** email format
3. **Rate limiter checks** if email was submitted within last 3 seconds
4. **If allowed**: Returns `200 OK` with server timestamp
5. **If blocked**: Returns `429 Too Many Requests` error

### Rate Limiting Rule
```
Maximum: 1 request per email address every 3 seconds
```

## 📖 Detailed Documentation

### **[📚 SERVER_DOCUMENTATION.md](server/SERVER_DOCUMENTATION.md)**
Complete backend documentation including:
- Architecture & middleware pipeline
- Rate limiting service implementation
- Thread-safe locking mechanism
- API endpoints & responses
- Configuration & setup
- Logging & debugging

### **[📚 CLIENT_DOCUMENTATION.md](client/CLIENT_DOCUMENTATION.md)**
Complete frontend documentation including:
- Component structure
- Real-time email validation
- API communication flow
- UI components & styling
- Error handling
- Testing strategies

## 🚀 Quick Start (2 Minutes)

### **Prerequisites**
- .NET 10 SDK
- Node.js 18+ and npm

### **1. Start Backend (Terminal 1)**
```bash
cd server
dotnet run
```
✅ Listens on `http://localhost:5000`

### **2. Start Frontend (Terminal 2)**
```bash
cd client
npm install    # First time only
npm start
```
✅ Opens `http://localhost:4200`

### **3. Test It**
1. Enter `user@example.com`
2. Click "✉️ Send Email"
3. See success! ✓
4. Click again within 3 seconds
5. See error! ❌ 429 Too Many Requests

## 📊 API Response Examples

### **Success (200 OK)**
```json
{
  "email": "user@example.com",
  "receivedAt": "2026-03-01T21:56:37.309Z"
}
```

### **Rate Limited (429)**
```json
{
  "email": "user@example.com",
  "receivedAt": "2026-03-01T21:56:37.309Z",
  "statusCode": 429,
  "message": "Too Many Requests - Please wait at least 3 seconds before resubmitting"
}
```

## 🧪 Testing Rate Limiting

### **Method 1: Web UI** 
Open `http://localhost:4200` and interact with form

### **Method 2: HTML Test Page**
Run `node serve-test.js` and go to `http://localhost:3000`

### **Method 3: Command Line**
```bash
node -e "
const http = require('http');
const p = JSON.stringify({email:'test@example.com'});
function sr(n){
  return new Promise(r=>{
    const req=http.request({hostname:'localhost',port:5000,path:'/api/email',method:'POST',headers:{'Content-Type':'application/json'}},(res)=>{
      res.on('data',()=>{});res.on('end',()=>{console.log('Req '+n+': '+res.statusCode); r();});
    });
    req.write(p);req.end();
  });
}
Promise.all([sr(1),sr(2)]);
"
```
**Expected**: `Req 1: 200` and `Req 2: 429`

## 🔑 Key Features

### **Backend (C# / .NET 10)**
✅ Thread-safe rate limiting with `lock` statement  
✅ Atomic check-and-update operations  
✅ CORS support for localhost:4200 & localhost:3000  
✅ Comprehensive logging  
✅ RESTful API design  
✅ Middleware pipeline architecture  

### **Frontend (Angular 17+)**
✅ Real-time email validation (regex)  
✅ Visual feedback (green/red borders)  
✅ Dynamic button state (enabled/disabled)  
✅ 429 error handling  
✅ Responsive design  
✅ Standalone component  

## 🏗️ Architecture

```
Browser                    HTTP POST                   Server
┌──────────────┐          ────────────────►          ┌──────────────────┐
│   Angular    │                                     │  ASP.NET Core    │
│   App        │◄─────────────────────────────────   │  Rate Limited    │
│              │         Response (200/429)          │  API             │
└──────────────┘                                     └──────────────────┘
     │                                                      │
     │ • Email input                                       │
     │ • Validation (regex)                               │ • Parse JSON
     │ • Submit button                                    │ • Validate email
     │ • Error display                                    │ • Check rate limit
     │ • Success message                                  │ • Return response
```

## 🔒 Rate Limiting Logic

### **The Algorithm**
```csharp
lock (_lock)  // Thread-safe
{
    // Check if email exists and within 3s window
    if (email exists && time < 3 seconds)
        return 429;  // BLOCKED
    
    // Record this request
    LastRequests[email] = now;
    return 200;      // ALLOWED
}
```

### **Why It Works**
- ✅ Check & record happen atomically
- ✅ No race conditions
- ✅ Simultaneous requests handled correctly
- ✅ Different emails not affected

## 📋 Prerequisites

* **Node.js** v18+ and npm
* **.NET 10 SDK** (not .NET 7!)
* **Windows/Linux/macOS**

## 🚀 Running the Project

### **Setup Step 1: Backend**
```bash
cd server
dotnet restore
dotnet run
```
Output: `Now listening on: http://localhost:5000`

### **Setup Step 2: Frontend**
```bash
cd ../client
npm install
npm start
```
Output: `Network: http://localhost:4200/`

### **Access the App**
- Frontend: http://localhost:4200
- Backend: http://localhost:5000/api/email (POST only)
- Test page: http://localhost:3000 (optional)

## 🛠️ Troubleshooting

| Issue | Solution |
|-------|----------|
| Port 5000 in use | `netstat -ano \| findstr :5000` then `taskkill /PID <id> /F` |
| Port 4200 in use | `npx ng serve --port 4201` |
| CORS error | Ensure client on localhost:4200 or localhost:3000 |
| Connection failed | Check server is running on port 5000 |
| 429 on first request | Server restart clears state |

## 📚 Learn More

- **Backend Logic**: See [server/SERVER_DOCUMENTATION.md](server/SERVER_DOCUMENTATION.md)
- **Frontend Logic**: See [client/CLIENT_DOCUMENTATION.md](client/CLIENT_DOCUMENTATION.md)

* **POST** `/api/email` accepts a JSON body `{ "email": "..." }`.
* Successful response: `200 OK` with
  ```json
  { "email": "...", "receivedAt": "2026-03-01T12:34:56.789Z" }
  ```
* If the same address is submitted more than once within 3 seconds, returns
  `429 Too Many Requests` and the body contains the previous valid response.

## Notes

* All source files include comments for clarity and maintainability.
* Adjust ports or CORS policies as needed for your environment.

### VS Code Tasks

You can also start both projects from within VS Code using the tasks:

- **Run Task** → `start:server` to launch the API
- **Run Task** → `start:client` to spin up the Angular dev server

These run in the integrated terminal and monitor output.

Happy coding! 👨‍💻
