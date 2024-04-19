#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

#Depending on the operating system of the host machines(s) that will build or run the containers, the image specified in the FROM statement may need to be changed.
#For more information, please see https://aka.ms/containercompat

# Use the official .NET 8 runtime image as the base
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 443

# Use the official .NET 8 SDK image to build the application
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["WordCounter.API/WordCounter.API.csproj", "WordCounter.API/"]
RUN dotnet restore "./WordCounter.API/WordCounter.API.csproj"
COPY . .
WORKDIR "/src/WordCounter.API"
RUN dotnet publish "./WordCounter.API.csproj" --output /app/publish --configuration Release --no-restore

# Create the final runtime image
FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "WordCounter.API.dll"]