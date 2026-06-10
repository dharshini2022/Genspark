import { Component } from '@angular/core';
import { RouterLink, RouterOutlet } from '@angular/router';
import { Header } from '../header/header';

@Component({
  selector: 'app-dashboard',
  imports: [RouterLink, RouterOutlet,Header],
  templateUrl: './dashboard.html',
  styleUrl: './dashboard.css',
})
export class Dashboard {}
