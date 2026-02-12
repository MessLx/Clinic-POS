FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY PClinicPOS.Api/PClinicPOS.Api.csproj PClinicPOS.Api/
RUN dotnet restore PClinicPOS.Api/PClinicPOS.Api.csproj
COPY PClinicPOS.Api PClinicPOS.Api
WORKDIR /src/PClinicPOS.Api
RUN dotnet build -c Release --no-restore
RUN dotnet publish -c Release -o /app --no-build

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app .
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "PClinicPOS.Api.dll"]
