import { Injectable } from "@angular/core";
import { HttpClient } from "@angular/common/http";
import { LoginModel } from "../components/login/Model/login.model";
@Injectable({
    providedIn:'root'
})
export class AuthService{
    constructor(private http: HttpClient){}
    public loginApiCall(loginModel:LoginModel){
        return this.http.post("https://dummyjson.com/auth/login", loginModel);
    }
}