import { Component, signal } from '@angular/core';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-footer',
  imports: [RouterLink],
  templateUrl: './footer.html',
  styleUrl: './footer.css',
})
export class Footer {
  role = signal('Customer');
  dashboardRoute = signal('/customer-home');
  productsRoute = signal('/customer-home/products-list');
  profileRoute = signal('/login');
  ngOnInit(){
    const currentRole = sessionStorage.getItem('role');
    if (currentRole) {
      this.role.set(currentRole);
      if(this.role() == 'Admin'){
        this.dashboardRoute.set('/admin-home/')
        this.productsRoute.set('/admin-home/products-list')
        this.profileRoute.set('/admin-home/profile')
      }
      else if(this.role() == 'Vendor'){
        this.dashboardRoute.set('/vendor-home/')
        this.productsRoute.set('/vendor-home/products-list')
        this.profileRoute.set('/vendor-home/profile')
      }
      else if(this.role() == 'Customer'){
        this.profileRoute.set('/customer-home/profile')
      }
    } else {
      this.role.set('Guest');
      this.dashboardRoute.set('/customer-home');
      this.productsRoute.set('/customer-home/products-list');
      this.profileRoute.set('/login');
    }
  }
}
