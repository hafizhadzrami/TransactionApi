# Transaction API (Assessment Solution)

This project is a RESTful API built with **.NET 8.0** that processes partner transaction information, applies validation rules, and calculates discounts based on given business logic.  
It also includes logging via **log4net** and containerization support with **Docker**.

---

## 🚀 Features
- **Transaction Submission API** (`/api/SubmitTrxMessage`)
- Validates:
  - Partner authentication (`partnerkey`, `partnerpassword`)
  - Mandatory fields
  - Item rules (qty, unitprice, totalamount)
  - Timestamp (± 5 minutes tolerance)
  - Signature matching
- **Discount Rules**:
  - Base discount by amount range
  - Conditional discounts (prime numbers, amounts ending in 5)
  - Cap at max 20%
- **Logging** with log4net (requests & responses, password encrypted)
- **Swagger UI** for easy testing
- **Dockerfile** for container deployment

---

## 🛠 Technologies
- .NET 8.0 Web API
- C#
- log4net
- Docker

---

## 📂 Project Structure
TransactionApi/
├── Controllers/
│ └── SubmitTrxMessageController.cs # Main API logic
├── Services/
│ ├── PartnerAuthService.cs # Partner authentication
│ ├── SignatureService.cs # Signature generation & validation
│ └── DiscountService.cs # Discount calculation logic
├── Models/
│ ├── TransactionRequest.cs # Input model
│ ├── TransactionResponse.cs # Output model
│ └── ItemDetail.cs # Item details
├── Program.cs # App entry point
├── log4net.config # Logging configuration
├── Dockerfile # Container setup
└── README.md # Documentation

---

## 📌 API Endpoints

### 1️⃣ Submit Transaction
**POST** `/api/SubmitTrxMessage`

#### Request (Sample)

{
  "partnerKey": "FAKEGOOGLE",
  "partnerRefNo": "FG-00001",
  "partnerPassword": "RkFLRVBBU1NXT1JEMTIzNA==",
  "totalAmount": 1000,
  "items": [
    { "partnerItemRef": "i-00001", "name": "Pen", "qty": 2, "unitPrice": 500 }
  ]
}

#### Response (Success)

{
  "result": 1,
  "totalAmount": 1000,
  "totalDiscount": 0,
  "finalAmount": 1000,
  "resultMessage": "Success",
  "timestamp": "9842724747848",
  "signature": "OWUxZGNjOGY5YmQ1ZDU2..."
}

#### Response (Failure)
{
  "result": 0,
  "resultMessage": "Access Denied!"
}
