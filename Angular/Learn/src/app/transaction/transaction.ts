import { Component } from '@angular/core';
import { ActivatedRoute } from '@angular/router';

@Component({
  selector: 'app-transaction',
  imports: [],
  templateUrl: './transaction.html',
  styleUrl: './transaction.css',
})
export class Transaction {
  fromaccountNumber: string = '';
  constructor(private activeRoute:ActivatedRoute){
    //takes the route parameter from url. (this is when we pass parameter in url)
   //this.fromaccountNumber = this.activeRoute.snapshot.params['accNum'];
   //takes the route parameter from navigation state. (this is when we pass parameter in state)
   this.fromaccountNumber = history.state.accNum || '';
  }
}
