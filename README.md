# Web API сервис реализующий методы CRUD
Проект представляет собой Web API для управления пользователями с ролевой системой доступа. Реализованы CRUD-операции, аутентификация через JWT-токены, валидация данных и мягкое удаление. Сервис соответствует требованиям задания, включая 10 ключевых функций для работы с сущностью `User`. Реализована автоматическая инициализация администратора при запуске, данные о пользователяях хранятся в In-Memory база данных с реализацией, позволяющей легко заменить её на реальную базу данных. 
## Особенности реализации
1. **Повышенная безопасность**
- **Хеширование паролей**:
Пароли никогда не хранятся в открытом виде. Используется `PasswordHasher<User>` из ASP.NET Core Identity, который автоматически генерирует соль и применяет алгоритм PBKDF2. Это защищает от атак перебором и утечек данных.
```c#
///PasswordService.cs
public string HashPassword(User user, string password)
{
    return _passwordHasher.HashPassword(user, password);
}
```
- **Валидация токенов JWT**:
Настроена строгая проверка JWT-токенов с валидацией издателя, аудитории, срока действия и подписи. Это гарантирует, что только легитимные токены принимаются сервером.
2. **Гибкая архитектура для масштабирования**
- **Подготовка к работе с реальной БД**:
Класс `UserContext` наследуется от `DbContext`, что позволяет легко переключиться с In-Memory на любую реляционную БД (PostgreSQL, SQL Server) через изменение строки подключения в `Program.cs`.
``` c#
// Program.cs  
builder.Services.AddDbContext<UserContext>(opt =>  
    opt.UseInMemoryDatabase("UserList"));
```
- **Слоистая структура**:
    - **Сервисы** (`AuthService`, `UserService`) инкапсулируют логику.
    - **Контроллеры** обрабатывают HTTP-запросы и делегируют работу сервисам.
    - **DTO** (`UserCreateDto`, `AuthDto`) изолируют модель данных от API, предотвращая переопределение критических полей (например, `Admin`).
``` c#
//AuthDto.cs
public class AuthDto
{
    [Required]
    [RegularExpression("^[a-zA-Z0-9]+$", ErrorMessage = "the login contains invalid characters")]
    public required string Login { get; set; }

    [Required]
    [RegularExpression("^[a-zA-Z0-9]+$", ErrorMessage = "the password contains invalid characters")]
    public required string Password { get; set; }

}
```
3. **Автоматизация и удобство разработки**
- **Инициализация администратора**:
При первом запуске автоматически создаётся пользователь `Admin` с хешированным паролем, что исключает ручные манипуляции с базой.
``` c#
private async void CreateAdmin(UserContext context)
{
    var InitAdminUser = new User{ ... };
    InitAdminUser.Password = _passwordService.HashPassword(InitAdminUser, "<some_admin_password>");
    await context.Users.AddAsync(InitAdminUser);
    await context.SaveChangesAsync();
}
```
- **Swagger UI с авторизацией**:
Настроена интеграция Swagger с JWT, что позволяет тестировать защищённые эндпоинты прямо в интерфейсе. Документация генерируется автоматически.
``` c#
// Program.cs  
c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme { ... });  
c.AddSecurityRequirement(...); 
```
4. **Расширенная обработка ошибок**
- **Детальные HTTP-ответы**:
Каждое исключение обрабатывается отдельно, возвращая понятные коды статусов (`403 Forbid`, `404 NotFound`) и сообщения. Например:
```c#
///UsersController.cs
...
catch (UnauthorizedAccessException)
{
    return Forbid();
}
catch (KeyNotFoundException ex)
{
    return NotFound(ex.Message);
}
catch (InvalidOperationException ex)
{
    return BadRequest(ex.Message);
}
...
```
-**Валидация данных на уровне модели**:
Атрибуты `[Required]`, `[RegularExpression]` в классе `User` гарантируют корректность данных до сохранения в БД.
Например:
```c#
//User.cs
[Required]
[RegularExpression("^[a-zA-Z0-9]+$", ErrorMessage = "the login contains invalid characters")]
public required string Login { get; set; }
```

6. **Инверсия зависимостей**:
Сервисы внедряются через интерфейсы (`IAuthService`, `IUserService`), что упрощает тестирование и замену реализаций, в соответствии с принципами SOLID.
```c#
//AuthController.cs
private readonly IAuthService _authService;
public AuthController(IAuthService authService)
{
    _authService = authService;
}
```


## Стек технологий

**Основной стек**
- **.NET 9**:
Базовый фреймворк для разработки API, обеспечивающий высокую производительность и поддержку современных функций.
- **ASP.NET Core**:
Платформа для создания API с поддержкой маршрутизации, middleware и внедрения зависимостей.
- **Entity Framework Core**:
Реализована поддержка In-Memory базы данных для тестирования, с возможностью легкого перехода на PostgreSQL, SQL Server и другие СУБД.

**Безопасность**

- **JWT (JSON Web Tokens)**:
Аутентификация и авторизация через токены с настройкой срока действия, валидацией подписи и ролевой моделью (Admin/NotAdmin).
- **PasswordHasher (ASP.NET Identity)**:
Хеширование паролей с использованием алгоритма PBKDF2 и автоматической генерацией соли.
- **Validation Attributes**:
Регулярные выражения для проверки логина, пароля и имени, а также ограничения для различных полей.

**Документация и тестирование**
- **Swagger (Swashbuckle)**:
Автоматическая генерация OpenAPI-документации с возможностью тестирования API через Swagger UI. Интеграция JWT для доступа к защищенным эндпоинтам.
## Реализованные запросы
Эндпоинты расположены в двух контроллерах:
1. **AuthController**: 
Здесь расположен один метод `POST /api/Auth/login`, который принимает на вход DTO `AuthDto` с логином и паролем и в случае совпадения данных возвращает jwt-токен, с помощью которого можно авторизироваться и вызывать методы из `UsersController`.

2. **UsersController**
В данном контроллере реализованы 10 основных методов CRUD класса `User`. Рассмотрим каждый метод в отдельности
(в каждом методе проверяется удален ли вызывающий метод пользователь): 
- `POST /api/Users`: создаёт нового пользователя, доступно всем пользователям, но только администратор может задавать значение поля `Admin`.
- `PUT /api/Users/{login}`: измененяет информаццию об имени, поле или дате рождения пользователя по логину, доступно администратору или самому пользователю.
- `PUT /api/Users/{login}/password`: Меняет пароль пользователя по логину. Доступно администратору или самому пользователю.
- `PUT /api/Users/{oldLogin}/login`: Меняет логин пользователя по старому логину. Доступно администратору или самому пользователю.
- `GET /api/Users/Active`: Возвращает список активных пользователей (отсортированных по дате создания). Доступно администраторам.
- `GET /api/Users/{login}`: Возвращает имя, пол, дату рождения и статус пользователя (активный или нет) по логину. Доступно администраторам.
- `GET /api/Users/{login}/password`: Возвращает имя, пол и дату рождения по логину и паролю. Доступно только самому пользователю.
- `GET /api/Users/older-than/{age}`: Возвращает пользователей, чья дата рождения соответствует возрасту больше значения `age`. Доступно администраторам.
- `DELETE /api/Users/soft/{login}`: Устанавливает полям `RevokedOn` и `RevokedBy` ненулевые значения (в соответствии с данными администратора и текущим временем), делая пользователя неактивным по логину. Доступно администраторам.
- `POST /api/Users/restore/{login}`: Очищает значения полей `RevokedOn` и `RevokedBy` у пользователя логин которого передается. Доступно администраторам.
