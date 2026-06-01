import { Component } from '@angular/core';
import { ProductModel } from './models/product.model';

@Component({
  selector: 'app-product',
  imports: [],
  templateUrl: './product.html',
  styleUrl: './product.css',
})
export class Product {
  product:ProductModel = new ProductModel();

  ReduceStock(){
    this.product.stock--;
    alert("Remaining Stock : "+this.product.stock);
  }
}
