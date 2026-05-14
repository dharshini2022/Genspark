**NOTIFICATION SYSTEM**
**PROJECT OUTPUT**
1) REGISTRATION : User name, email and phone no are received.
Validations: 
* email and phone format validation
* Existing user check wrt name, email and phone

![Registration1](image.png)
![Registration 2](image-1.png)

2) Send Email Notification
* Validation: Exisiting contact check wrt to email, message min characters validation
![Email Notification](image-2.png)

3) SMS Notification
* Validation: Exisiting contact check wrt phone, message min characters validation
![SMS Notification](image-3.png)

4) Whatsapp Notification
* Validation: Exisiting contact validation -> IsWhatsappActive? validation -> Message min characters validation
![Whatsapp Notification](image-4.png)

5) Get User by name
* Validation: Exisiting contact check
![GetUser](image-5.png)

6) Get all user
* Validation: Password based retrieval (only admin can get all user details)
![Get All Users](image-6.png)

7) Update User : User can update email, phone and whatsapp status (Active / Inactive)
* Validation: Exiting contact check -> Format check of new value
![Update User](image-7.png)