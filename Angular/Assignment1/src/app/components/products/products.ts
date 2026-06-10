import { Component, signal } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { ProductModel } from '../product-details/Model/product.model';
import { ProductApiService } from '../../services/product.service';
import { ProductCard } from '../product-card/product-card';

@Component({
  selector: 'app-products',
  imports: [ProductCard],
  templateUrl: './products.html',
  styleUrl: './products.css',
})
export class Products {
  products = signal<ProductModel[]>([]);
  cart = signal<ProductModel[]>([]);

  constructor(private productApiService: ProductApiService) {
    this.productApiService.getProductsFromDummyJson()
      .subscribe({
        next: (response: any) => {
          this.products.set(response.products);
        },
        error: (error) => {
          console.error(error);
        },
        complete: () => {
          
        }
      });
  }

  handleBuy(product: ProductModel | undefined) {
    if (!product) return;
    if ((product.stock ?? 0) <= 0) {
      alert(`${product.title} is out of stock!`);
      return;
    }

    alert(`You have purchased ${product.title} for $${product.price}`);

    // Update stock locally
    this.products.update(allProducts =>
      allProducts.map(p => {
        if (p.id === product.id) {
          return { ...p, stock: p.stock - 1 };
        }
        return p;
      })
    );

    this.productApiService.updateProductStock(product.id, { stock: product.stock - 1 });
    this.cart.update(currentCart => [...currentCart, product]);
  }
}
