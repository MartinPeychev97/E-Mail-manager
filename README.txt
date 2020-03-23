# EmailManager
Web application that allows facilitate tracking, monitoring and processing customer loan applications coming to the bank via e-mail.

## Team Members
* Veronika Ivancheva - [GitHub](https://github.com/VeronikaIvancheva)
* Martin Peychev - [GitHub](https://github.com/MartinPeychev97)

## Project Description
### Users
* **Manager**
* **Operator**
* **Users cannot register** in the application but are created by another user with “Manager” rights. The password cannot be changed in the program due to security reasons (it needs to be changed outside of it)

---
#### E-mails
* E-mails are pulled whenever user enters in e-mail catalog (in order to avoid non-stop pulling from Gmail and eventually slowing down the program). As soon as email is read from Gmail API it is recorded in the system DB. All personal data is Encrypted in order to fullfill GDPR requirements.

---
#### Email/Application statuses

* Not reviewed – default status – all new e-mails are classified as such. 

* Invalid application – all e-mail which does not contain loan applications or are invalid must be marked in such status. Only employees with "Manager" rights can return the e-mail in "Not reviewed" status.

* New – all e-mails must be reviewed and only the one contains valid loan application will be set by employee with "New" status. 

* Open – status marked for under processing – in order to set to "Open" status, the system requests **customer name**, **customer ID/EGN** and **customer phone information** to be fulfilled by the bank employee.

* Closed - status of the e-mail when the application is set approved or rejected.

---
## Statuses flow

Possible statuses transition depends on granted user rights:

* **Operators**
  * Not reviewed --> Invalid Application
  * Not reviewed --> New
  * New --> Open 
  * Open --> Closed

* **Managers**
  * Not reviewed <--> Invalid Application
  * Not reviewed <--> New
  * New <--> Open
  * Open <--> Closed
  * Closed --> New 


## Technologies
* ASP.NET Core
* ASP.NET Identity
* Entity Framework Core
* MS SQL Server
* Razor
* HTML
* CSS
* Bootstrap
