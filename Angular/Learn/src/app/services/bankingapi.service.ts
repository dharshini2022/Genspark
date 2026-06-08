import { HttpClient } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { LoginModel } from "../login/Model/login.model";
import { baseUrl } from "../../environment";
import { RegisterModel } from "../register/Model/register.model";

@Injectable({
    providedIn: 'root'
})
export class BankingApiService {
    constructor(private http: HttpClient) {
    }
    public loginApiCall(loginModel: LoginModel) {
        let url = baseUrl+'/Authentication/Login';
        return this.http.post(url, loginModel);
    }

    public registerApiCall(registerModel:RegisterModel){
        let url = baseUrl+'/Authentication/Register';
        return this.http.post(url,registerModel);
    }
    public getAccountDetails(accNumber:string){
        let url = baseUrl+'/Account?accountNumber='+accNumber;
        return this.http.get(url);
    }
}