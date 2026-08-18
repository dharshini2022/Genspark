import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { CustomerNavbar } from '../customer-navbar/customer-navbar';
import { Footer } from '../../shared/footer/footer';

@Component({
  selector: 'app-customer-home',
  imports: [RouterOutlet, CustomerNavbar, Footer],
  templateUrl: './customer-home.html',
  styleUrl: './customer-home.css',
})
export class CustomerHome {}
