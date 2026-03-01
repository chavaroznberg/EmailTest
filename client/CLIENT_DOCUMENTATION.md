# 📖 Angular Client Documentation

## 📋 Table of Contents
1. [Overview](#overview)
2. [Application Architecture](#application-architecture)
3. [Component Structure](#component-structure)
4. [Features](#features)
5. [Email Validation](#email-validation)
6. [API Communication](#api-communication)
7. [UI Components](#ui-components)
8. [Error Handling](#error-handling)
9. [Running the Client](#running-the-client)

---

## Overview

This is a standalone Angular 17+ application that provides a user-friendly interface for submitting email addresses to the rate-limited API server.

**Key Features:**
- ✅ Real-time email validation
- ✅ Visual feedback (green/red borders)
- ✅ Dynamic button state (enabled/disabled)
- ✅ Rate limit error handling (429)
- ✅ Success message display
- ✅ Responsive design
- ✅ Standalone component (no modules)

---

## Application Architecture

```
┌─────────────────────────────────────────┐
│       Browser (User Interface)          │
├─────────────────────────────────────────┤
│  AppComponent (Standalone)              │
│  ├─ Template (HTML with bindings)       │
│  ├─ Component Logic (TypeScript)        │
│  ├─ Styles (CSS)                        │
│  └─ Event Handlers                      │
└──────────────┬──────────────────────────┘
               │ HTTP Fetch API
               │ (POST /api/email)
               ▼
┌─────────────────────────────────────────┐
│      Server (localhost:5000)            │
│  Rate Limiting API                      │
└─────────────────────────────────────────┘
```

---

## Component Structure

### **AppComponent** (Standalone)

```typescript
@Component({
  selector: 'app-root',
  standalone: true,
  imports: [CommonModule],
  template: `...`,      // Inline HTML template
  styles: [`...`]       // Inline CSS styles
})
export class AppComponent { ... }
```

#### **Why Standalone?**
- ✅ No NgModule required
- ✅ Simpler setup
- ✅ Modern Angular 14+ approach
- ✅ Easier dependency injection

#### **Imports**
```typescript
imports: [CommonModule]
```
- `*ngIf` directive
- `{{ }}` interpolation
- `[property]` binding
- `(event)` binding

---

## Component Logic

### **Component Properties**

```typescript
export class AppComponent {
  email: string = '';                    // Email input value
  isEmailValid: boolean = false;         // Validation state
  response?: ApiResponse;                // Success response
  errorMessage: string = '';             // Error message

  interface ApiResponse {
    email: string;
    receivedAt: string;
  }
}
```

---

## Email Validation

### **validateEmail() Method**

```typescript
validateEmail(value: string) {
  console.log('validateEmail called with:', value);
  this.email = value;
  
  const re = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
  this.isEmailValid = re.test(this.email);
  
  console.log('Email updated to:', this.email, 'Valid:', this.isEmailValid);
}
```

#### **Regex Pattern**: `/^[^\s@]+@[^\s@]+\.[^\s@]+$/`

| Part | Meaning |
|------|---------|
| `^` | Start of string |
| `[^\s@]+` | One or more non-whitespace, non-@ characters |
| `@` | Literal @ symbol |
| `[^\s@]+` | One or more non-whitespace, non-@ characters |
| `\.` | Literal dot (escaped) |
| `[^\s@]+` | One or more non-whitespace, non-@ characters |
| `$` | End of string |

#### **Examples**
```
✓ user@example.com        → Valid
✓ john.doe@company.co.uk  → Valid
✗ user@example            → Invalid (no TLD)
✗ user.example.com        → Invalid (no @)
✗ user @example.com       → Invalid (space before @)
✗ user@@example.com       → Invalid (double @)
```

#### **Triggered On**
- `(input)` event: Real-time validation as user types
- `(change)` event: Validation when input loses focus

---

## API Communication

### **send() Method**

```typescript
async send() {
  this.errorMessage = '';
  this.response = undefined;
  
  if (!this.email) {
    this.errorMessage = 'Email is empty!';
    return;
  }
  
  const payload = { email: this.email };
  
  try {
    const response = await fetch('http://localhost:5000/api/email', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(payload)
    });
    
    if (response.ok) {                    // 200-299
      const data: ApiResponse = await response.json();
      this.response = data;
    } else if (response.status === 429) { // Too Many Requests
      const data: ApiResponse = await response.json();
      this.errorMessage = 'Too many requests; please wait a moment.';
      this.response = data; // Show last request time
    } else {                              // Other errors
      const errorText = await response.text();
      this.errorMessage = `Error (${response.status}): ${response.statusText}`;
    }
  } catch (err) {
    this.errorMessage = 'Connection to server failed. Make sure it is running on localhost:5000';
  }
}
```

#### **Flow Chart**

```
User clicks "Send Email"
    ↓
[Input Validation] Empty? → Show error
    ↓
[Prepare Payload] { email: "user@example.com" }
    ↓
[HTTP POST] fetch('http://localhost:5000/api/email', {...})
    ↓
[Wait for Response]
    ├─ 200 OK
    │  └─ this.response = { email, receivedAt }
    │  └─ Show success message
    │
    ├─ 429 Too Many Requests
    │  └─ this.errorMessage = "Too many requests; please wait a moment."
    │  └─ this.response = { email, receivedAt } (last request time)
    │
    └─ Other Error (400, 500, etc.)
       └─ this.errorMessage = "Error (400): Bad Request"
```

---

## UI Components

### **Email Input Field**

```html
<input 
  #emailInput
  id="email" 
  type="email" 
  (change)="validateEmail(emailInput.value)"
  (input)="validateEmail(emailInput.value)"
  [style.border]="email ? (isEmailValid ? '2px solid #4caf50' : '2px solid #f44336') : '1px solid #ddd'"
  [style.background-color]="email ? (isEmailValid ? '#f1f8f6' : '#fff5f5') : 'white'"
  style="padding: 8px; width: 300px; font-size: 14px; transition: all 0.2s ease; border-radius: 4px;"
  placeholder="name@example.com"
/>
```

#### **Styling Logic**

| State | Border | Background | Visual |
|-------|--------|-----------|---------|
| Empty | `#ddd` (gray) | white | Neutral |
| Valid | `#4caf50` (green) | `#f1f8f6` (light green) | ✓ Good |
| Invalid | `#f44336` (red) | `#fff5f5` (light red) | ✗ Bad |

#### **Visual Feedback**
```
Empty field:
┌─────────────────────────────┐
│ name@example.com            │ (gray border)
└─────────────────────────────┘

Valid email:
┌─────────────────────────────┐
│ user@example.com            │ (green border, light green bg)
└─────────────────────────────┘

Invalid email:
┌─────────────────────────────┐
│ invalid.email               │ (red border, light red bg)
└─────────────────────────────┘
```

### **Status Message**
```html
<small style="display: block; margin-top: 4px; color: #666;">
  <span *ngIf="!email">אנא הכנס כתובת אימייל</span>
  <span *ngIf="email && isEmailValid" style="color: #4caf50;">✓ אימייל תקין</span>
  <span *ngIf="email && !isEmailValid" style="color: #f44336;">✗ אימייל לא תקין</span>
</small>
```

| Condition | Text | Color |
|-----------|------|-------|
| No input | "אנא הכנס כתובת אימייל" (Enter email) | Gray |
| Valid | "✓ אימייל תקין" (Valid email) | Green |
| Invalid | "✗ אימייל לא תקין" (Invalid email) | Red |

### **Send Button**

```html
<button 
  [disabled]="!isEmailValid" 
  (click)="send()"
  [style.opacity]="!isEmailValid ? '0.5' : '1'"
  [style.cursor]="!isEmailValid ? 'not-allowed' : 'pointer'"
  style="padding: 10px 20px; font-size: 16px; background-color: #1976d2; color: white; border: none; border-radius: 4px; transition: all 0.2s ease;"
>
  {{ !isEmailValid ? '❌ Enter Valid Email' : '✉️ Send Email' }}
</button>
```

#### **Button States**

```
Disabled (Invalid Email):
┌──────────────────────────┐
│ ❌ Enter Valid Email     │ (opacity: 0.5, cursor: not-allowed)
└──────────────────────────┘

Enabled (Valid Email):
┌──────────────────────────┐
│ ✉️ Send Email            │ (opacity: 1.0, cursor: pointer)
└──────────────────────────┘
  ↓ Hover
┌──────────────────────────┐
│ ✉️ Send Email            │ (darker blue, shadow, raised)
└──────────────────────────┘
```

#### **CSS Hover Effect**
```css
button:not(:disabled):hover {
  background-color: #1565c0;      /* Darker blue */
  transform: translateY(-2px);    /* Slight lift */
  box-shadow: 0 4px 8px rgba(0, 0, 0, 0.2); /* Shadow */
}
```

### **Success Message**
```html
<div *ngIf="response" style="margin:20px; background-color: #e8f5e9; border: 1px solid #4caf50; padding: 15px; border-radius: 4px;">
  <h3 style="color: #2e7d32; margin-top: 0;">✓ Success!</h3>
  <p><strong>Email received:</strong> {{ response.email }}</p>
  <p><strong>Server timestamp:</strong> {{ response.receivedAt }}</p>
</div>
```

**Shows when**: `response` object is not undefined

### **Error Message**
```html
<div *ngIf="errorMessage" style="margin:20px; background-color: #ffebee; border: 1px solid #f44336; padding: 15px; color:#c62828; border-radius: 4px;">
  <h3 style="margin-top: 0;">✗ Error</h3>
  <p>{{ errorMessage }}</p>
</div>
```

**Shows when**: `errorMessage` is not empty

---

## Error Handling

### **Error Scenarios**

#### **1. Empty Input**
```typescript
if (!this.email) {
  this.errorMessage = 'Email is empty!';
  return;
}
```
**Result**: Button is disabled, user can't submit

#### **2. Rate Limited (429)**
```typescript
} else if (response.status === 429) {
  const data: ApiResponse = await response.json();
  this.errorMessage = 'Too many requests; please wait a moment.';
  this.response = data;
```
**Result**: Shows error message and displays last request timestamp

#### **3. Server Error (4xx, 5xx)**
```typescript
} else {
  const errorText = await response.text();
  this.errorMessage = `Error (${response.status}): ${response.statusText}`;
}
```
**Result**: Shows HTTP status and message

#### **4. Connection Failed**
```typescript
} catch (err) {
  this.errorMessage = 'Connection to server failed. Make sure it is running on localhost:5000';
}
```
**Result**: Network error, server not running

---

## Data Flow Diagram

```
┌─────────────────────────────────────────────────────┐
│ User Types: "john@example.com"                      │
└────────────────┬────────────────────────────────────┘
                 │
                 ▼ (input) event
        ┌────────────────────┐
        │ validateEmail()    │
        │ email = "john@..." │
        │ isEmailValid=true  │
        └────────────┬───────┘
                     │
        ┌────────────▼─────────────┐
        │ Update UI:              │
        │ • Green border          │
        │ • Light green bg        │
        │ • "✓ Valid"             │
        │ • Button enabled        │
        └────────────┬─────────────┘
                     │
                     ▼ User clicks "Send"
        ┌────────────────────────────┐
        │ send()                      │
        │ • Prepare payload           │
        │ • HTTP POST request         │
        └────────────┬────────────────┘
                     │
      ┌──────────────┼──────────────┐
      │              │              │
      ▼ 200 OK       ▼ 429 Error    ▼ Error
  ┌────────┐     ┌────────────┐  ┌──────────┐
  │Success │     │Rate Limited│  │Error Msg │
  │Message │     │Error Msg   │  └──────────┘
  └────────┘     │+ Response  │
                 └────────────┘
```

---

## Running the Client

### **Prerequisites**
- Node.js 18+ and npm
- Angular CLI (optional)

### **Installation**
```bash
cd client
npm install
```

### **Start Development Server**
```bash
npm start
```
or
```bash
npx ng serve --port 4200
```

**Output**:
```
✔ Compiled successfully.
Network: http://localhost:4200/
```

### **Access Application**
Open browser to: `http://localhost:4200`

### **Production Build**
```bash
ng build --configuration production
```

Creates optimized build in `dist/` folder

---

## File Structure

```
client/
├── src/
│   ├── app/
│   │   ├── app.component.ts      ← Main component (logic)
│   │   ├── app.component.html    ← Template (HTML)
│   │   └── app.component.css     ← Styles
│   ├── main.ts                   ← Bootstrap
│   └── index.html                ← Index page
├── angular.json                  ← Angular config
├── package.json                  ← Dependencies
├── tsconfig.json                 ← TypeScript config
└── tsconfig.app.json             ← App TypeScript config
```

---

## Debugging

### **Browser Console**
All methods log to console:

```typescript
console.log('validateEmail called with:', value);
console.log('Email updated to:', this.email, 'Valid:', this.isEmailValid);
console.log('=== SEND CLICKED ===');
console.log('Email value:', this.email);
console.log('Response status:', response.status);
console.log('✅ Success:', data);
console.log('⏱️ Rate limited!');
```

### **Network Tab**
View HTTP requests:
1. F12 → Network tab
2. Submit email
3. See POST request to `http://localhost:5000/api/email`
4. View request/response headers and body

### **Angular DevTools**
Chrome extension for Angular debugging:
1. Install "Angular DevTools" extension
2. F12 → Angular tab
3. Inspect component state
4. See property values in real-time

---

## Key Design Decisions

### **1. Standalone Component**
- ✅ Simpler setup without NgModule
- ✅ Modern Angular best practice
- ❌ Requires Angular 14+

### **2. Inline Template and Styles**
- ✅ Single file component
- ✅ Easier to manage
- ❌ Large templates become unreadable

### **3. Async/Await for API**
- ✅ Cleaner than promises/RxJS
- ✅ Easy to understand
- ❌ No automatic error retry

### **4. Real-time Validation**
- ✅ Immediate feedback
- ✅ Prevents invalid submission
- ✅ Button disabled until valid

### **5. Responsive Design**
- ✅ Works on mobile and desktop
- ✅ Flexbox for layout
- ✅ Touch-friendly buttons

---

## Summary

The Angular client provides:
1. **Real-time email validation** with visual feedback
2. **Disabled button state** for invalid emails
3. **HTTP communication** with the backend API
4. **Error handling** for rate limits and connection failures
5. **Clean UI** with responsive design
6. **Accessible form** with proper labels and feedback

**Core Logic**: Simple, single-component Angular app that validates email format and communicates with the rate-limited API server.

---

**Version**: 1.0  
**Date**: March 2, 2026  
**Framework**: Angular 17+  
**Language**: TypeScript
