import { ApplicationConfig, provideBrowserGlobalErrorListeners } from '@angular/core';
import { provideRouter } from '@angular/router';

import { routes } from './app.routes';
import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { ProductApiService } from './services/product.api.service';
import { authInterceptor } from './authInterceptor';
import { BankingApiService } from './services/bankingapi.service';

export const appConfig: ApplicationConfig = {
  providers: [
    provideBrowserGlobalErrorListeners(),
    provideRouter(routes),
    provideHttpClient(),
    provideHttpClient(
      withInterceptors([authInterceptor])
    ),
    ProductApiService,
    BankingApiService
  ]
};
