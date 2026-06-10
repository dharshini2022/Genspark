import { Component } from '@angular/core';
import { BankingApiService } from '../services/bankingapi.service';
import { debounceTime, distinctUntilChanged, of, Subject, Subscription, switchMap } from 'rxjs';
import { FormsModule } from '@angular/forms';
import { Router, RouterOutlet } from '@angular/router';

@Component({
  selector: 'app-account',
  imports: [FormsModule, RouterOutlet],
  templateUrl: './account.html',
  styleUrl: './account.css',
})
export class Account {
  
  searchAccountNumber: string = '';
  //Subject Creation
  subscription:Subscription;
  searchSubject = new Subject<string>();
  accountDetails: any = null;
  
  constructor(private bankingApiService: BankingApiService, private router: Router) {
    //Subscribtion waits for events from searchSubject
    //.subscribe() reacts to the emission
    this.subscription = this.searchSubject.pipe(
      debounceTime(500),    //Interval time used to consider the events. if the next event is triggered before 500ms the previous event is cancelled
      distinctUntilChanged(),  //prevents duplicate searches
      switchMap(
        accNumber => {
          if(accNumber.trim() === '')
            return of({}); //Return an observable that emits null if the input is empty
          return this.bankingApiService.getAccountDetails(accNumber);
        })
    ).subscribe({
      next: (response:any) => {
        console.log("Account details", response);
        this.accountDetails = response;
        
      },
      error: (error) => {
        console.error("Failed to fetch account details", error);
        
      },
      complete: () => {        
        console.log("Account details fetched successfully!");
      
      }
    });
  }

  // getAccountDetails(accNumber:string){
  //   this.bankingApiService.getAccountDetails(accNumber).subscribe({
  //     next: (response) => {
  //       console.log("Account details", response);
  //       alert("Account details fetched successfully!")
  //     },
  //     error: (error) => {
  //       console.error("Failed to fetch account details", error);
  //       alert("Failed to fetch account details. Please try again.");
  //     }
  //   });
  // }


   handleSendMoneyClick(){
    //this.router.navigate(['/account/transaction/'+this.accountDetails.accountNumber]);
    //state makes the accountNumber invisible on the routes
    this.router.navigate(['/account/transaction'],{state:{accNum:this.accountDetails.accountNumber}});
  }

  //Input for the Subscriber (publisher)
  getAccountDetails(){
    //.next() emits the values
    this.searchSubject.next(this.searchAccountNumber);
  }

  onDestroy(){
    //unsubscribe
    console.log("Unsubscribed from the subscription")
    this.subscription.unsubscribe();
  }
}