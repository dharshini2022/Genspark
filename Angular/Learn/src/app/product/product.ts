import { Component, EventEmitter, input, Input, Output, signal } from '@angular/core';
import { ProductApiService } from '../services/product.api.service';
import { ProductModel } from './models/product.model';

@Component({
  selector: 'app-product',
  imports: [],
  templateUrl: './product.html',
  styleUrl: './product.css',
})
export class Product {
  // products = signal<ProductModel[]>([]);
  // constructor(private productApiService: ProductApiService) {
  //   this.productApiService.getProductsFromDummyJson()
  //     .subscribe({
  //     next:(response: any) => {
  //       console.log(response.products);
  //       this.products.set(response.products);
  //     },
  //     error:(error) => {
  //       console.error(error);
  //     },
  //     complete:()=>{
  //       console.log("Request completed");
  //     }
  //   });
  // }
  // handleChangeClick(){
  //   this.products()[0].title = "New Product Name";
  // }

  // //Stock Decrement function 
  // ReduceStock(productId: number){
  //   this.products.update(allProducts => {
  //     return allProducts.map(p => {
  //       if (p.id === productId) {
  //         if (p.stock > 0) {
  //           const newStock = p.stock - 1;
  //           alert(`Remaining Stock of ${p.title} : ${newStock}`);
  //           return { ...p, stock: newStock };
  //         } else {
  //           alert(`${p.title} is Out of stock!`);
  //         }
  //       }
  //       return p;
  //     });
  //   });
  // }

  /*Parent To Child Communication */
  //Option 1 : Using @Input Decorator
  // @Input() product:ProductModel = {} as ProductModel;
  //Option 2 : Using signal and input function from angular core
  product = input<ProductModel>();
  
  /*Child To Parent Communication*/
  @Output() buy = new EventEmitter<ProductModel>();
  handleClick(){
    
    this.buy.emit(this.product());
  }

}


