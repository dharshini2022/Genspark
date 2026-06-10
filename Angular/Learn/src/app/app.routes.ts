import { Routes } from '@angular/router';
import { Customer } from './customer/customer';
import { Login } from './login/login';
import { Account } from './account/account';
import { Transaction } from './transaction/transaction';
import { authGuard } from './guards/authGuard';

export const routes: Routes = [
    {path:'',component:Login},
    {path:'home',component:Customer},
    // {path:'account',component:Account,children:[
    //     {path:'transaction/:accNum',component:Transaction}
    // ]},
    {path:'account',component:Account,
        canActivate:[authGuard],
        children:[
        // {path:'transaction/:accNum',component:Transaction}
        {path:'transaction',component:Transaction}
    ]},
    
    {path:'products',loadComponent:()=>import('./products/products').then(m=>m.Products)},
    

];
