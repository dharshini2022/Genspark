import { HttpClient } from "@angular/common/http";
import { Injectable } from "@angular/core";

@Injectable({
  providedIn: 'root'
})
export class ProductApiService{
  constructor(private http: HttpClient) {}
  public getProductsFromDummyJson(){
    return this.http.get("https://dummyjson.com/products");
  }

  public getProductById(id: number){
    return this.http.get<any>(`https://dummyjson.com/products/${id}`);
  }

  public updateProductStock(id: number | undefined, updatedFields: { stock: number }) {
    if (id === undefined) {
      console.warn("Product ID is undefined. Skipping remote API update.");
      return;
    }
    this.http.patch(`https://dummyjson.com/products/${id}`, updatedFields).subscribe({
      next: (res) => console.log('Product stock updated successfully on server', res),
      error: (err) => console.error('Failed to update product stock on server', err)
    });
  }
}