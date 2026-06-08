export class RegisterModel{
    constructor(public username: string = "", public email: string = "",public phone: string = "", public status: string = "Active", public dateOfBirth:Date = new Date())
    {}
}