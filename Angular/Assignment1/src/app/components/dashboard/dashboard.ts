import { Component } from '@angular/core';
import { RouterLink, RouterOutlet } from '@angular/router';
import { Header } from '../header/header';
import { Products } from '../products/products';

@Component({
  selector: 'app-dashboard',
  imports: [RouterLink, RouterOutlet,Header,Products],
  templateUrl: './dashboard.html',
  styleUrl: './dashboard.css',
})
export class Dashboard {}
