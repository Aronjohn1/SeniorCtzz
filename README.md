# 🏛️ Senior Citizens Pension Management System

> A desktop application for managing senior citizen records and pension releases built with **C# WPF (.NET 8)** and **Microsoft Access Database**.

---

## 📋 Overview

The **Senior Citizens Pension Management System** is designed for the **Office for Senior Citizens Affairs (OSCA)** to efficiently manage senior citizen registrations, pension distributions, and generate reports across all barangays in the city.

---

## ✨ Features

### 👤 User Management
- Role-based login system (**Admin / Section Head / Staff Encoder**)
- Encrypted password storage using **AES encryption**
- Staff account management (Add, Update, Delete)

### 📝 Senior Citizen Registration
- Register senior citizens with complete personal information
- Auto-compute age from birthdate
- Auto-detect residency classification **(Urban / Coastal / Rural)** based on barangay
- Support for Pension Type **(SSS / GSIS)** and Assistance Source **(LGU / DSWD / WAITLIST)**
- Bulk import via **CSV file**
- Search and filter registered records

### 💰 Pension Management

| Type | Description |
|---|---|
| **Quarterly Pension** | Regular ₱2,250 pension release per quarter |
| **AICS** | 30% of hospital bill assistance (2-month cooldown) |
| **APR** | Annual Pension Release (once per year) |
| **Bereaved Assistance** | Death benefit for family of deceased senior citizen |

### 📊 Reports
- Barangay-wise senior citizen list
- Overall summary by residency classification
- Pension transaction history per senior citizen
- Export reports to **CSV**

### 🖥️ Dashboard
- Total active and inactive senior citizens
- Monthly entries and pension releases count
- Recent pension transactions
- Quick search by name or OSCA ID

---

## 🖥️ System Requirements

### PC 1 — Server / Database Host
| Requirement | Details |
|---|---|
| OS | Windows 10 / 11 |
| Software | Microsoft Access Database Engine 2016 (64-bit) |
| Network | Shared `Database` folder accessible via LAN |
| Runtime | .NET 8 Desktop Runtime |

### PC 2 — Client
| Requirement | Details |
|---|---|
| OS | Windows 10 / 11 |
| Software | Microsoft Access Database Engine 2016 (64-bit) |
| Network | Connected to same network as PC 1 |
| Runtime | .NET 8 Desktop Runtime |

---

## 🔧 Installation & Setup

### Step 1 — Install Prerequisites
Download and install on **both PCs**:
- [.NET 8 Desktop Runtime (x64)](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)
- [Microsoft Access Database Engine 2016 (x64)](https://www.microsoft.com/en-us/download/details.aspx?id=54920)

---

### Step 2 — Configure Network (PC 1)
1. Place `SeniorCtzz.accdb` inside a `Database` folder
2. Right-click `Database` folder → **Properties** → **Sharing**
3. Set Share Name: `Database`
4. Set permissions: **Everyone → Read/Write**
5. Disable **Password Protected Sharing** in Network and Sharing Center

---

### Step 3 — Get PC 1 IP Address
Open **CMD** on PC 1:
```
ipconfig
```
Note the **IPv4 Address** (e.g., `192.168.x.x`)

---

### Step 4 — Update Connection String
In `DBConnection.cs`, update the IP address:
```csharp
// Replace with your actual PC 1 IP address
string networkPath = @"\\YOUR_PC1_IP_ADDRESS\Database\SeniorCtzz.accdb";
```

---

### Step 5 — Setup PC 2 (Client)
1. Copy all published files to a folder (e.g., `D:\SeniorCtzz\`)
2. **Do NOT copy** the `Database` folder — it stays on PC 1 only
3. Verify network access from PC 2 — open File Explorer and type:
```
\\YOUR_PC1_IP_ADDRESS\Database
```
You should see `SeniorCtzz.accdb` ✅

---

### Step 6 — Run the Application
Double-click `SeniorCtzz.exe` on either PC and login.

---

## 📁 Project Structure

```
SeniorCtzz/
├── Assets/
│   ├── logo.jpeg
│   └── logo1.png
├── Database/
│   └── SeniorCtzz.accdb              ← Microsoft Access Database (not pushed to GitHub)
├── App.xaml / App.xaml.cs            ← App entry point
├── AppSession.cs                      ← Session / login state management
├── DBConnection.cs                    ← Database connection & all DB operations
├── Model.cs                           ← Data models / entities
├── MainWindow.xaml/.cs               ← Login screen
├── Admindashboard.xaml/.cs           ← Admin dashboard
├── Adminmanagestaff.xaml/.cs         ← Staff account management
├── Adminpensionlist.xaml/.cs         ← Admin pension list
├── Adminreports.xaml/.cs             ← Admin reports
├── Staffdashboard.xaml/.cs           ← Staff dashboard
├── Staffregister.xaml/.cs            ← Senior citizen registration
├── Staffaab.xaml/.cs                 ← Pension release
├── Staffpensionlist.xaml/.cs         ← Staff pension list
├── Staffreports.xaml/.cs             ← Staff reports
└── ConfirmationRegister.xaml/.cs     ← Registration confirmation dialog
```

---

## 🗄️ Database Tables

| Table | Description |
|---|---|
| `USERS` | Staff accounts and encrypted login credentials |
| `SENIOR_CITIZEN` | Senior citizen personal records |
| `BARANGAY` | Barangay list with residency classification |
| `QUARTERLY_PENSION_RELEASE` | Quarterly pension transactions |
| `AICS_TRANSACTION` | AICS hospital bill assistance transactions |
| `APR_TRANSACTION` | Annual Pension Release transactions |
| `BEREAVED_ASSISTANCE` | Death benefit / bereaved assistance transactions |
| `DECEASED_RECORD` | Records of deceased senior citizens |

---

## 👥 User Roles & Access

| Role | Permissions |
|---|---|
| **Admin** | Full access — manage staff, all reports, all pension types |
| **Section Head** | Same access as Admin |
| **Staff Encoder** | Register seniors, release pensions, view reports |

---

## 🏘️ Barangay Classifications

| Classification | Description |
|---|---|
| **Urban** | Barangay 1–26 including 18-A, 22-A, 24-A |
| **Coastal** | Agay-ayan, Anakan, Daan-Lungsod, Kalagonoy, Lawaan, Libertad, Lunao, Pangasihan, Samay, San Juan, San Luis, Santiago, Talisay |
| **Rural** | Alagatan, Bagubad, Bakidbakid, Bal-ason, Bantaawan, Binakalan, Capitulangan, Dinawehan, Eureka, Hindangon, Kalipay, Kamanikan, Kianlagan, Kibuging, Kipuntos, Lawit, Libon, Lunotan, Malibud, Malinao, Maribucao, Mimbalagon, Mimbunga, Mimbuntong, Minsapinit, Murallon, Odiongan, Pigsaluhan, Punong, Ricoro, San Jose, San Miguel, Sangalan, Tagpako, Talon, Tinabalan, Tinulongan |

---

## 🛠️ Tech Stack

| Technology | Details |
|---|---|
| Language | C# |
| Framework | .NET 8 WPF (Windows Presentation Foundation) |
| Database | Microsoft Access (.accdb) |
| OleDb Provider | Microsoft ACE OLEDB 12.0 |
| IDE | Visual Studio 2022 / 2026 |
| Encryption | AES (System.Security.Cryptography) |

---

## 🚀 Publishing

```cmd
cd path\to\SeniorCtzz

dotnet publish -c Release -r win-x64 --self-contained false -o D:\publish\SeniorCtzz
```

> ⚠️ **Important:** After publishing, manually copy the database file with data:
> ```
> bin\x64\Debug\net8.0-windows\Database\SeniorCtzz.accdb
>         ↓
> D:\publish\SeniorCtzz\Database\SeniorCtzz.accdb  (Replace)
> ```

---

## 🔒 Security Notes

- Passwords are encrypted using **AES encryption** before storing in the database
- The `.accdb` database file is excluded from version control via `.gitignore`
- Sensitive configuration values (IP address, encryption keys) should be replaced before deployment
- The `Database` folder should only reside on **PC 1** and never be copied to client machines

---

## 📌 Important Reminders

- Always **back up** the `SeniorCtzz.accdb` file regularly
- Do **not** copy the `Database` folder to PC 2
- Update the **IP address** in `DBConnection.cs` if the network changes
- Re-publish and re-copy files to PC 2 whenever the code is updated

---

## 👨‍💻 Developer

Developed as a **3rd Year BSIT  Project**
For **OSCA (Office for Senior Citizens Affairs)**

---

## 📄 License

This project is for academic and government use only.
