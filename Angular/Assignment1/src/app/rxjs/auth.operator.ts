import { Subject } from "rxjs";

export const usernameSubject = new Subject<string|undefined>();

export const logout = () => {
    sessionStorage.removeItem("token");
    console.log("Logged Out");
    usernameSubject.next(undefined);
}

export const changeUsername = () => {
   const token = sessionStorage.getItem("token");
    if (token) {
        const payload = JSON.parse(atob(token.split(".")[1]));
         const fullName = payload["firstName"] + " " +payload["lastName"];
         const gender = payload["gender"];
         let title = "";
         if(gender.toLowerCase() === "male"){
            title = "Mr.";
         }else if(gender.toLowerCase() === "female"){
            title = "Ms.";
         }else{
            title = "Mx.";
         }
         if (fullName) {
            usernameSubject.next(title + " "+ fullName);
         }
    }
}

export const isLoggedIn = () => {
    const token = sessionStorage.getItem('token');
    return token?true:false;
}