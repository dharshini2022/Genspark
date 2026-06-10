import { Component, input, output } from '@angular/core';
import { ProductModel } from '../product-details/Model/product.model';
import { Router } from '@angular/router';

@Component({
  selector: 'app-product-card',
  imports: [],
  templateUrl: './product-card.html',
  styleUrl: './product-card.css',
})
export class ProductCard {
  product = input<ProductModel>();
  buy = output<ProductModel | undefined>();

  constructor(private router: Router) {}

  handleClick() {
    this.buy.emit(this.product());
  }

  viewProductDetail() {
    const id = this.product()?.id;
    if (id === undefined) {
      alert("Unable to navigate to product details")
    }
    this.router.navigate(['/dashboard/products',id],{state: {id:this.product()?.id}})
  }
}
