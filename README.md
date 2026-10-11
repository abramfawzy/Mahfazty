# Mahfazty (محفظتي)

Personal finance tracker API built with ASP.NET Core 8, EF Core, and SQL Server.

## Features
- JWT authentication (Register / Login)
- Income and expense categories
- Transactions with filtering
- Daily and monthly summary
- Each user sees only their own data

## Tech Stack
C#, ASP.NET Core Web API, Entity Framework Core, SQL Server, JWT, BCrypt

## Run locally
1. Update the connection string in `appsettings.json`
2. Set `Jwt:Key` using User Secrets
3. Run `Update-Database`
4. Press F5 and open Swagger
