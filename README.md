````
# .//

```text
AumoDekstop/
├── Models/          # DTO (Data Transfer Object) sesuai JSON dari backend
├── Services/        # ApiClient (HttpClient wrapper untuk GET, POST, PUT, DELETE)
├── ViewModels/      # Penghubung antara ApiClient dengan tampilan (loading state, data list)
├── Views/           # Halaman XAML (misal: LoginPage.xaml, DashboardPage.xaml)
├── App.xaml         # Konfigurasi tema, resource global, & Dependency Injection
└── MainWindow.xaml  # Kerangka utama jendela (NavigationView / Frame)
````

command:
build+log: dotnet build -c Debug -r win-x64 -p:Platform=x64 -v normal

