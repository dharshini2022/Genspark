import { Component, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RegisterModel } from './Model/register.model';
import { BankingApiService } from '../services/bankingapi.service';

@Component({
  selector: 'app-register',
  imports: [FormsModule],
  templateUrl: './register.html',
  styleUrl: './register.css',
})
export class Register {
  registerModel = signal(new RegisterModel())

  constructor(private bankingApiService: BankingApiService){

  }
  handleRegisterClick(){
    console.log("Register button clicked");
    this.bankingApiService.registerApiCall(this.registerModel()).subscribe({
      next: (response) => {
        console.log("Register successful", response);
        alert("Register successful!")
      },
      error: (error) => {
        console.error("Register failed", error);
        alert("Register failed. Please try again.");
      }
    });
  }
}
