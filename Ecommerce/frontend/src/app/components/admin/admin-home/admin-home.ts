import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { AdminNavbar } from '../admin-navbar/admin-navbar';
import { Footer } from '../../shared/footer/footer';

@Component({
  selector: 'app-admin-home',
  imports: [RouterOutlet, AdminNavbar, Footer],
  templateUrl: './admin-home.html',
  styleUrl: './admin-home.css',
})
export class AdminHome {}
