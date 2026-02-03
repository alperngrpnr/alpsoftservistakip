# 1. SDK aşaması: Kodun derlendiği aşama
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Proje dosyasını kopyalayıp kütüphaneleri yüklüyoruz (Restore)
# 'UygulamaAdiniz.csproj' kısmını kendi dosya adınla değiştir!
COPY ["alpsoftservistakip.csproj", "./"]
RUN dotnet restore "./alpsoftservistakip.csproj"

# Tüm dosyaları kopyalayıp kodu derliyoruz
COPY . .
RUN dotnet publish -c Release -o /app/publish

# 2. Runtime aşaması: Sadece uygulamanın çalıştığı hafif aşama
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .

# Uygulamanın çalışacağı DLL adını buraya yaz!
ENTRYPOINT ["dotnet", "alpsoftservistakip.dll"]