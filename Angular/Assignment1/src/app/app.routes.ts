import { Routes } from '@angular/router';
import { Login } from './components/login/login';
import { Profile } from './components/profile/profile';
import { ProductDetails } from './components/product-details/product-details';
import { Dashboard } from './components/dashboard/dashboard';
import { authGuard } from './guards/auth.guard';

export const routes: Routes = [
    { path: '', component: Login },
    {
        path: 'dashboard', component: Dashboard,
        canActivate: [authGuard],
        children: [
            {
                path: 'products',
                loadComponent: () => import('./components/products/products').then(m => m.Products)
            },
            {
                path: 'products/:id',
                component: ProductDetails
            },
            { path: 'profile', component: Profile }
        ]
    }
];
