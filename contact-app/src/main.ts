import { bootstrapApplication } from '@angular/platform-browser';
import { provideHttpClient } from '@angular/common/http'; // Functional provider for HttpClient
import { provideAnimations } from '@angular/platform-browser/animations'; // Functional provider for animations
// Import routing configuration if needed
// import { provideRouter } from '@angular/router';
// import appRoutes from './app/app.routes'; // Assuming routes defined in app.routes.ts

// Import the standalone root component
import { AppComponent } from './app/app.component';

// Bootstrap the standalone AppComponent
bootstrapApplication(AppComponent, {
  // Configure application-wide providers
  providers: [
    provideHttpClient(), // Makes HttpClient available for injection throughout the app
    provideAnimations(), // Enables Angular's animation capabilities
    // provideRouter(appRoutes) // Provide routing configuration if used
  ]
})
  .catch(err => console.error(err)); // Basic error handling for bootstrap process