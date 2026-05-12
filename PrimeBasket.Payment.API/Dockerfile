FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore "PrimeBasket.Payment.API/PrimeBasket.Payment.API.csproj"
RUN dotnet publish "PrimeBasket.Payment.API/PrimeBasket.Payment.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:80
EXPOSE 80
ENTRYPOINT ["dotnet", "PrimeBasket.Payment.API.dll"]
