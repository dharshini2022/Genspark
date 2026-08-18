import { Routes } from '@angular/router';
import { CustomerRegister } from './components/auth/customer-register/customer-register';
import { Login } from './components/auth/login/login';
import { VendorRegister } from './components/auth/vendor-register/vendor-register';
import { AdminDashboard } from './components/admin/admin-dashboard/admin-dashboard';
import { authGuard } from './guards/auth.guard';
import { roleGuard } from './guards/role.guard';
import { CustomerHome } from './components/customer/customer-home/customer-home';
import { VendorHome } from './components/vendor/vendor-home/vendor-home';
import { AdminHome } from './components/admin/admin-home/admin-home';
import { AccessDenied } from './components/auth/access-denied/access-denied';
import { ProductsList } from './components/shared/products-list/products-list';
import { ProductCatalog } from './components/shared/product-catalog/product-catalog';
import { VendorDashboard } from './components/vendor/vendor-dashboard/vendor-dashboard';
import { CustomerDashboard } from './components/customer/customer-dashboard/customer-dashboard';
import { ProductDetail } from './components/shared/product-detail/product-detail';
import { AddProduct } from './components/vendor/add-product/add-product';
import { VendorList } from './components/admin/vendor-list/vendor-list';
import { VendorProfile } from './components/shared/vendor-profile/vendor-profile';
import { OrderList } from './components/shared/order-list/order-list';
import { OrderDetail } from './components/shared/order-detail/order-detail';
import { Cart } from './components/customer/cart/cart';
import { Wishlist } from './components/customer/wishlist/wishlist';
import { OrderCheckout } from './components/customer/order-checkout/order-checkout';
import { Payment } from './components/customer/payment/payment';
import { PaymentStatus } from './components/customer/payment-status/payment-status';
import { ProductList } from './components/vendor/product-list/product-list';
import { SettlementList } from './components/shared/settlement-list/settlement-list';
import { DiscountList } from './components/shared/discount-list/discount-list';
import { CategoryList } from './components/admin/category-list/category-list';
import { CustomerProfile } from './components/customer/customer-profile/customer-profile';
import { EmailOtp } from './components/shared/email-otp/email-otp';

export const routes: Routes = [
    { path: 'customer-register', component: CustomerRegister },
    { path: 'login', component: Login },
    { path: 'shared/email-otp', component: EmailOtp },
    { path: 'access-denied', component: AccessDenied },
    { path: 'vendor-register', component: VendorRegister, canActivate: [authGuard] },
    {
        path: 'admin-home',
        component: AdminHome,
        canActivate: [authGuard, roleGuard],
        data: { roles: ['Admin'] },
        children: [
            { path: '', component: AdminDashboard },
            { path: 'products-list', component: ProductCatalog },
            { path: 'product-detail/:id', component: ProductDetail },
            { path: 'order-list', component: OrderList, data: { role: 'Admin' } },
            { path: 'order-detail/:id', component: OrderDetail, data: { role: 'Admin' } },
            { path: 'vendors', component: VendorList },
            { path: 'vendor-profile/:id', component: VendorProfile },
            { path: 'categories', component: CategoryList },
            { path: 'discounts', component: DiscountList }
        ]
    },
    {
        path: 'vendor-home',
        component: VendorHome,
        canActivate: [authGuard, roleGuard],
        data: { roles: ['Vendor'] },
        children: [
            { path: 'products-list', component: ProductList },
            { path: 'settlements', component: SettlementList },
            { path: 'discounts', component: DiscountList },
            { path: 'product-detail/:id', component: ProductDetail },
            { path: 'add-product', component: AddProduct },
            { path: '', component: VendorProfile },
            { path: 'order-list', component: OrderList, data: { role: 'Vendor' } },
            { path: 'order-detail/:id', component: OrderDetail, data: { role: 'Vendor' } }
        ]
    },
    {
        path: 'customer-home',
        component: CustomerHome,
        data: { roles: ['Customer'] },
        children: [
            { path: '', component: CustomerDashboard },
            { path: 'products-list', component: ProductCatalog },
            { path: 'product-detail/:id', component: ProductDetail }
            
        ]
    },
    {
        path: 'customer-home',
        component: CustomerHome,
        canActivate: [authGuard, roleGuard],
        data: { roles: ['Customer'] },
        children: [
            { path: 'cart', component: Cart },
            { path: 'wishlist', component: Wishlist },
            { path: 'order-list', component: OrderList, data: { role: 'Customer' } },
            { path: 'checkout', component: OrderCheckout },
            { path: 'order-checkout', component: OrderCheckout },
            { path: 'payment', component: Payment },
            { path: 'payment-status', component: PaymentStatus },
            { path: 'order-detail/:id', component: OrderDetail, data: { role: 'Customer' } },
            { path: 'profile', component: CustomerProfile },
            { path: 'product-review-form', loadComponent: () => import('./components/customer/product-review-form/product-review-form').then(m => m.ProductReviewForm) }
        ]
    },
    {
        path: 'dashboard',
        redirectTo: () => {
            const role = sessionStorage.getItem('role');
            if (role === 'Admin') return '/admin-home';
            if (role === 'Vendor') return '/vendor-home';
            if (role === 'Customer') return '/customer-home';
            return '/login';
        }
    },
    { path: '', redirectTo: '/customer-home', pathMatch: 'full' }
];
