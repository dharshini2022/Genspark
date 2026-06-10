import { Component, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { form, FormField, minLength, required } from '@angular/forms/signals';
import { LoginModel } from './Model/login.model';
import { AuthService } from '../../services/auth.service';
import { changeUsername } from '../../rxjs/auth.operator';
import { Router } from '@angular/router';

@Component({
  selector: 'app-login',
  imports: [FormsModule, FormField],
  templateUrl: './login.html',
  styleUrl: './login.css',
})
export class Login {
  loginModel = signal(new LoginModel());
  progress = signal(false);
  constructor(private authService: AuthService, private router:Router){
  }

  loginForm = form(this.loginModel,(path) => {
    required(path.username, {message:"Username is required"});
    minLength(path.username, 3, {message:"Username must be at least 3 characters long"});
    required(path.password, {message:"Password is required"});
    minLength(path.password, 8, {message:"Password must be at least 8 characters long"});
  })

  handleLoginClick(){
    if(this.loginForm().invalid()){
      alert("Please fix the errors in the form before submitting!");
      return;
    }

    this.progress.set(true);
    this.authService.loginApiCall(this.loginModel()).subscribe({
      next: (res: any) => {
        console.log("Login successful",res);
        sessionStorage.setItem('token',res.accessToken);
        alert("Login successful!");
        changeUsername()
        this.progress.set(false);
        this.router.navigate(['/dashboard'])
      },
      error: (err) => {
        console.log("login failed",err);
        alert("Login failed! Please try again");
        this.progress.set(false);
      }
    })
    

  }
  
}
