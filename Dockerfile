FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["TecNMEmployeesAPI/TecNMEmployeesAPI.csproj", "TecNMEmployeesAPI/"]
RUN dotnet restore "TecNMEmployeesAPI/TecNMEmployeesAPI.csproj"
COPY . .
WORKDIR "/src/TecNMEmployeesAPI"
RUN dotnet build "TecNMEmployeesAPI.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "TecNMEmployeesAPI.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TecNMEmployeesAPI.dll"]


