**Notification App**
**Base Models**
* User : It has properties of user (Name, Email, Phone and HasWhaatsapp : boolean value to check the whatsapp account of the user)
* Notification: It has properties of notification entity (message, sentDate, sender, receiver and type : enum to check notification mode)
type = {Email, SMS, Whatsapp}

**Mode Models**
* EmailNotification, SMSNotification and WhatsappNotification implement the methods of INotification interface.
* canSend() is used to check the validity of input and Send() is used to send the notification

**Interfaces**
* INotification : has abstract methods required by different modes to send notification
* IUser : has abstract method to work with the User Entities

**Services**
* NotificationService : triggers the notification send in the given mode
* UserService : List<User> is created to track the users. To maintain the state