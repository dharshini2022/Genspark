import { HttpClient } from "@angular/common/http";
import { Injectable } from "@angular/core";
// makes the class injectable (can accept injection, can be injected into other classes)
@Injectable({
  providedIn: 'root'
})
export class ProductApiService{
  constructor(private http: HttpClient) {}
  public getProductsFromDummyJson(){
    return this.http.get("https://dummyjson.com/products");
  }
}