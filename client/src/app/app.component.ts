import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';

interface ApiResponse {
  email: string;
  receivedAt: string;
}

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [CommonModule],
  template: `
    <h1>Email Validator App</h1>
    <div style="margin:20px; border: 1px solid #ccc; padding: 20px; background:white;">
      <h2>Send Email to Server</h2>
      
      <div style="margin-bottom: 10px;">
        <label for="email" style="display: block; margin-bottom: 5px; font-weight: bold;">Email Address:</label>
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
        <small style="display: block; margin-top: 4px; color: #666;">
          <span *ngIf="!email">אנא הכנס כתובת אימייל</span>
          <span *ngIf="email && isEmailValid" style="color: #4caf50;">✓ אימייל תקין</span>
          <span *ngIf="email && !isEmailValid" style="color: #f44336;">✗ אימייל לא תקין</span>
        </small>
      </div>

      <button 
        [disabled]="!isEmailValid" 
        (click)="send()"
        [style.opacity]="!isEmailValid ? '0.5' : '1'"
        [style.cursor]="!isEmailValid ? 'not-allowed' : 'pointer'"
        style="padding: 10px 20px; font-size: 16px; background-color: #1976d2; color: white; border: none; border-radius: 4px; transition: all 0.2s ease;"
      >
        {{ !isEmailValid ? '❌ Enter Valid Email' : '✉️ Send Email' }}
      </button>
    </div>

    <div *ngIf="response" style="margin:20px; background-color: #e8f5e9; border: 1px solid #4caf50; padding: 15px; border-radius: 4px;">
      <h3 style="color: #2e7d32; margin-top: 0;">✓ Success!</h3>
      <p><strong>Email received:</strong> {{ response.email }}</p>
      <p><strong>Server timestamp:</strong> {{ response.receivedAt }}</p>
    </div>

    <div *ngIf="errorMessage" style="margin:20px; background-color: #ffebee; border: 1px solid #f44336; padding: 15px; color:#c62828; border-radius: 4px;">
      <h3 style="margin-top: 0;">✗ Error</h3>
      <p>{{ errorMessage }}</p>
    </div>
  `,
  styles: [`
    h1 { color: #333; margin-top: 0; }
    button:not(:disabled):hover {
      background-color: #1565c0 !important;
      transform: translateY(-2px);
      box-shadow: 0 4px 8px rgba(0, 0, 0, 0.2);
    }
  `]
})
export class AppComponent {
  email: string = '';
  isEmailValid: boolean = false;
  response?: ApiResponse;
  errorMessage: string = '';

  constructor() {
    console.log('AppComponent ready');
  }

  validateEmail(value: string) {
    console.log('validateEmail called with:', value);
    this.email = value;
    const re = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    this.isEmailValid = re.test(this.email);
    console.log('Email updated to:', this.email, 'Valid:', this.isEmailValid);
  }

  async send() {
    this.errorMessage = '';
    this.response = undefined;
    console.log('=== SEND CLICKED ===');
    console.log('Email value:', this.email);
    console.log('Email is valid:', this.isEmailValid);
    
    if (!this.email) {
      this.errorMessage = 'Email is empty!';
      console.error('Email is empty.');
      return;
    }
    
    const payload = { email: this.email };
    console.log('Sending to server:', payload);
    
    try {
      const response = await fetch('http://localhost:5000/api/email', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json'
        },
        body: JSON.stringify(payload)
      });

      console.log('Response status:', response.status);
      
      if (response.ok) {
        const data: ApiResponse = await response.json();
        console.log('✅ Success:', data);
        this.response = data;
      } else if (response.status === 429) {
        const data: ApiResponse = await response.json();
        console.log('⏱️ Rate limited! Last request was at:', data.receivedAt);
        this.errorMessage = 'Too many requests; please wait a moment.';
        this.response = data;
      } else {
        const errorText = await response.text();
        console.error('❌ Error response:', errorText);
        this.errorMessage = `Error (${response.status}): ${response.statusText}`;
      }
    } catch (err) {
      console.error('❌ Error:', err);
      this.errorMessage = 'Connection to server failed. Make sure it is running on localhost:5000';
    }
  }
}


