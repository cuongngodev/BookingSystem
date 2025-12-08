# 🧾 Booking System — Full-Stack Web App  
> Built with **ASP.NET Core MVC**, **Entity Framework Core**, and **Bootstrap 5**

![.NET](https://img.shields.io/badge/.NET%208.0-blueviolet?style=for-the-badge&logo=dotnet)
![C#](https://img.shields.io/badge/C%23-239120?style=for-the-badge&logo=csharp&logoColor=white)
![Entity Framework](https://img.shields.io/badge/Entity%20Framework-Core%208.0-green?style=for-the-badge)
![Bootstrap](https://img.shields.io/badge/Bootstrap-5.3-purple?style=for-the-badge&logo=bootstrap)
![FullCalendar](https://img.shields.io/badge/FullCalendar-Interactive%20Calendar-lightblue?style=for-the-badge)
![License](https://img.shields.io/badge/License-MIT-yellow?style=for-the-badge)

---

## 💡 Overview
The **Booking System** is a complete appointment-booking platform that helps businesses manage their **services, users, and appointments** in one place.  
It demonstrates clean architecture, role-based authentication, and modern UI design.

> 🧭 Built as part of a team project when we are at John Abbott College to showcase practical skills in ASP.NET Core MVC, authentication, and full-stack development.

---

## ✨ Features

### 👤 Authentication & Roles
- Secure login and registration using **ASP.NET Core Identity**
- Role-based access control:
  - **Admin** – Manage everything (services, users, appointments)
  - **Employee** – Manage appointments and view services
  - **Customer** – Book and view appointments

---

### 💅 Service Management
- Add, edit, delete, and view all services
- Search by keyword or filter by category
- Sort by **name**, **duration**, or **price**
- Paginated list for large datasets

---

### 📅 Appointment Management
- Integrated **FullCalendar.js** for real-time visual scheduling
- Create or update appointments via modal pop-ups  
- Handle appointment status (Pending, Confirmed, Cancelled, Completed)
- Time-zone aware date handling and validation
- Admin and Employee views for management

---

### 👥 User Management
- Admins can view all users and their roles
- Search users by **name** or **phone**
- Assign, update, or delete users directly from the dashboard

---

### 🧭 Admin Dashboard
A simple and modern control panel for administrators:
- 📋 **Manage Services**
- 📆 **Manage Appointments**
- 👥 **Manage Users**

> Provides instant access to all core features with smooth navigation.

---

## 🎨 UI Highlights

| Page | Description |
|------|--------------|
| **Dashboard** | Overview of services, users, and appointments |
| **Service Page** | Sort, filter, and view service details |
| **Appointment Calendar** | Manage bookings visually |
| **About Page** | Project story, team, and tech stack |
| **Profile Page** | User profile management and settings |


---

## ⚙️ Tech Stack

| Layer | Technology |
|-------|-------------|
| **Backend** | ASP.NET Core MVC 8.0, C# |
| **Database** | SQL Server, Entity Framework Core |
| **Frontend** | Razor Pages, Bootstrap 5, JavaScript (FullCalendar.js) |
| **Authentication** | ASP.NET Core Identity |
| **Architecture** | MVC + Repository + Unit of Work |
| **Logging** | Serilog |

---

## 👥 Team & Contributors

| Member | Role | GitHub |
|:--------|:------|:--------|
| ![Cuong Ngo](https://avatars.githubusercontent.com/cuongngodev?s=80) <br> **Cuong Ngo** | [@cuongngodev](https://github.com/cuongngodev) |
| ![Linyue](https://avatars.githubusercontent.com/Linyue-dev?s=80) <br> **Linyue**  | [@Linyue-dev](https://github.com/Linyue-dev) |
| ![Labiba](https://avatars.githubusercontent.com/AliLabiba?s=80) <br> **Labiba** | [@AliLabiba](https://github.com/AliLabiba) |

> ✨ *This project was developed collaboratively as part of the Software Engineering course at John Abbott College. Each team member contributed significantly to both design and implementation.*
