# Step 1: Use official .NET SDK to build the app
FROM mcr.microsoft.com/dotnet/sdk:9.0.200-alpine3.20-arm64v8 AS build
WORKDIR /app

# Copy only the project file(s) first for caching purposes.
COPY *.csproj ./

# Use BuildKit cache mount to cache NuGet packages.
RUN --mount=type=cache,target=/root/.nuget/packages dotnet restore

# Now copy the rest of the source code.
COPY . .

# Build and publish the app.
RUN dotnet publish -c Release -o /out

# Step 2: Use the official ASP.NET Core runtime image to run the app
FROM mcr.microsoft.com/dotnet/aspnet:9.0.2-alpine3.21-arm64v8
WORKDIR /app
COPY --from=build /out .

# Set environment to Development so Swagger is enabled if coded that way.
ENV ASPNETCORE_ENVIRONMENT=Development

# Expose the port that the API will run on
EXPOSE 5165

# Start the API
ENTRYPOINT ["dotnet", "Ava.API.dll", "--urls", "http://0.0.0.0:5165"]
