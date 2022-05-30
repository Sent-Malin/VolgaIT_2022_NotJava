FROM alpine:3.14
FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app
ADD ./VolgaIT/schema schema

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["VolgaIT/VolgaIT.csproj", "VolgaIT/"]
RUN dotnet restore "VolgaIT/VolgaIT.csproj"
COPY ./ ./
WORKDIR "/src/VolgaIT"
RUN dotnet build "VolgaIT.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "VolgaIT.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "VolgaIT.dll"]
