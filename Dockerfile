FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app

EXPOSE 80
EXPOSE 443

#ENV ASPNETCORE_URLS=http://+:5211
#
#RUN adduser -u 5678 --disabled-password --gecos "" appuser && chown -R appuser /app
#USER appuser

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build

#Variables de entorno
ENV KVConfig__KVUrl ""
ENV KVConfig__TenantId ""
ENV KVConfig__ClientId ""
ENV KVConfig__ClientSecretId ""

WORKDIR /src
COPY ["src/Cnbv.ConectaProcesos.Opiniones/Sources/Cnbv.ConectaProcesos.Opiniones.Api/Cnbv.ConectaProcesos.Opiniones.Api.csproj", "./"]
COPY ["src/Cnbv.ConectaProcesos.Opiniones/Sources/Cnbv.ConectaProcesos.Opiniones.Business/Cnbv.ConectaProcesos.Opiniones.Business.csproj", "./"]
COPY ["src/Cnbv.ConectaProcesos.Opiniones/Sources/Cnbv.ConectaProcesos.Opiniones.Common/Cnbv.ConectaProcesos.Opiniones.Common.csproj", "./"]
COPY ["src/Cnbv.ConectaProcesos.Opiniones/Sources/Cnbv.ConectaProcesos.Opiniones.Data/Cnbv.ConectaProcesos.Opiniones.Data.csproj", "./"]
COPY ["src/Cnbv.ConectaProcesos.Opiniones/Sources/Cnbv.ConectaProcesos.Opiniones.Entities/Cnbv.ConectaProcesos.Opiniones.Entities.csproj", "./"]
RUN dotnet restore "Cnbv.ConectaProcesos.Opiniones.Api.csproj"

COPY . .
WORKDIR "/src"
RUN dotnet build "Cnbv.ConectaProcesos.Opiniones.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Cnbv.ConectaProcesos.Opiniones.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Cnbv.ConectaProcesos.Opiniones.Api.dll"]