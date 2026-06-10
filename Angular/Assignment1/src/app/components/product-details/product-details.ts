import { Component, signal, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { ProductApiService } from '../../services/product.service';
import { DatePipe } from '@angular/common';

@Component({
  selector: 'app-product-detail',
  imports: [DatePipe],
  templateUrl: './product-details.html',
  styleUrl: './product-details.css',
})
export class ProductDetails implements OnInit {
  id: number = 0;
  product = signal<any>(undefined);

  constructor(
    private route: ActivatedRoute,
    private productApiService: ProductApiService
  ) {}

  ngOnInit() {
    let idFromState = history.state?.id;
    if(!idFromState){
      idFromState = this.route.snapshot.paramMap.get('id');
    }
    if (idFromState) {
      this.id = +idFromState;
      this.fetchProductDetails(this.id);
      return;
    }

  }

  fetchProductDetails(id: number) {
    this.productApiService.getProductById(id).subscribe({
      next: (data) => {
        this.product.set(data);
      },
      error: (err) => {
        console.error('Failed to load product details', err);
        alert("Failed to load product details");
      }
    });
  }
}
