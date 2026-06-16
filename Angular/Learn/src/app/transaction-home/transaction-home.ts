import { Component } from '@angular/core';
import { TransactionFilterComponent } from '../transaction-filter/transaction-filter';
import { TransactionListComponent } from '../transaction-list/transaction-list';

@Component({
  selector: 'app-transaction-home',
  imports: [TransactionFilterComponent, TransactionListComponent],
  templateUrl: './transaction-home.html',
  styleUrl: './transaction-home.css',
})
export class TransactionHome {}
