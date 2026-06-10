// backend DTO mapping to frontend model
//use variable names as per Swagger (not DTO!)
//DTO use pascal case, but frontend model use camel case (as per Swagger)
export class CustomerModel {
    //username: string;
    name: string;
    email: string;
    phone: string;
    status: string;
    dateOfBirth: Date;

    //There is no overloading in TypeScript, so we can use default parameters to achieve similar functionality
    constructor(public username: string = "john_doe", name: string = "John Doe", email: string = "john.doe@example.com", phone: string = "1231231234", status: string = "Active", dateOfBirth: Date = new Date("1990-01-01")) {
        this.username = username;
        this.name = name;
        this.email = email;
        this.phone = phone;
        this.status = status;
        this.dateOfBirth = dateOfBirth;
    }

    /*Properties binding
    1) Declare variables then use constructor based intialisation
    2) Don't declare and use public keyword in constructor (automatically initialized) */
}