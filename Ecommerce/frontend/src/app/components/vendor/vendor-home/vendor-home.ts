import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { VendorNavbar } from '../vendor-navbar/vendor-navbar';
import { Footer } from '../../shared/footer/footer';

@Component({
  selector: 'app-vendor-home',
  imports: [RouterOutlet, VendorNavbar, Footer],
  templateUrl: './vendor-home.html',
  styleUrl: './vendor-home.css',
})
export class VendorHome {}
