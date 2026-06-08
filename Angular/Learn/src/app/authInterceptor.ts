import { HttpInterceptorFn } from "@angular/common/http";

export const authInterceptor: HttpInterceptorFn = (req,next)=>{
    const token = sessionStorage.getItem('token');
    console.log('Token from storage:', token);

    if(token){
        // clone the existing token and add token in the header
        const cloned = req.clone({
            headers: req.headers.set('Authorization', `Bearer ${token}`)
        });
        // Return the new cloned token
        return next(cloned);
    }
    // If now token, return the request as it is
    return next(req);
}