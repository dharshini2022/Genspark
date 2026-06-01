import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { CustomerModel } from './Models/customer.model';

@Component({
  selector: 'app-customer',
  imports: [FormsModule],
  templateUrl: './customer.html',
  styleUrl: './customer.css',
})
export class Customer {
  //Variable intialization
  customerName: string = 'John Doe';
  //var name:datatype = new datatype(); //for non-primitive types
  //default values taken from constructor of CustomerModel
  customer:CustomerModel = new CustomerModel();
  //customer:CustomerModel = new CustomerModel("johndoe", "John Doe", "john.doe@example.com", "123-456-7890", "active", new Date());
  styclass: string = "tableclass";

  handleChangeClick(){
    this.customer.name = "new name";
    alert("Customer Name: " + this.customer.name);
  }

  toggleLike(){
    if(this.styclass === "bi bi-balloon-heart-fill"){
      this.styclass = "bi bi-balloon-heart";
    } else {
      this.styclass = "bi bi-balloon-heart-fill";
    }
  }
}
