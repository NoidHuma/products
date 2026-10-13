# Products

Учёт продуктов и их категорий на ASP.NET Core MVC с PostgreSQL.

Для запуска нужны Docker и Docker Compose.

Запуск локально:

```bash
git clone https://github.com/NoidHuma/products.git
cd products
docker compose up -d --build
```

Открыть: [http://localhost:8080](http://localhost:8080)

Unit-тесты (нужен .NET SDK):

```bash
dotnet test Products.sln
```
