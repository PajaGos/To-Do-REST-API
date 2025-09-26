# 1) Base stage
# Get ASP.NET runtime image from official source
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app

# 2) Build stage
# Get SDK image to be able to build/publish the app
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy csproj and restore nuget dependencies to docker image
COPY ["TodoApi/TodoApi.csproj", "TodoApi/"]
RUN dotnet restore "TodoApi/TodoApi.csproj"

# Copy everything else to docker image and build it with parameter Release
COPY . .
WORKDIR "/src/TodoApi"
RUN dotnet build "TodoApi.csproj" -c $BUILD_CONFIGURATION -o /app/build

# 3) Publish stage
# Publish to app/publish folder
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "TodoApi.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# 4) Final stage -> image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Expose ports 
EXPOSE 80

# launch the app by default when the container starts
ENTRYPOINT ["dotnet", "TodoApi.dll"]
