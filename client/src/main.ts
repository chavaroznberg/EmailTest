import 'zone.js';
import { bootstrapApplication } from '@angular/platform-browser';
import { AppComponent } from './app/app.component';
import { provideHttpClient } from '@angular/common/http';

console.log('Starting Angular app...');

bootstrapApplication(AppComponent, {
  providers: [
    provideHttpClient()
  ]
}).catch(err => {
  console.error('✗ Bootstrap error:', err);
});
