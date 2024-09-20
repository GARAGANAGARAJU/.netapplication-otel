# Use .NET 8.0 SDK for building the application
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
 
# Copy the project files and restore dependencies
COPY ServiceA/ServiceA.csproj ServiceA/
COPY ServiceB/ServiceB.csproj ServiceB/
RUN dotnet restore ServiceA/ServiceA.csproj
RUN dotnet restore ServiceB/ServiceB.csproj
 
# Copy the rest of the application code
COPY . .
 
# Build and publish the application
WORKDIR /src/ServiceA
RUN dotnet publish ServiceA.csproj -c Release -o /app/publish/ServiceA
WORKDIR /src/ServiceB
RUN dotnet publish ServiceB.csproj -c Release -o /app/publish/ServiceB
 
# Use Nginx as a reverse proxy and .NET 8.0 runtime for the final image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
 
# Install Nginx for reverse proxy
RUN apt-get update && apt-get install -y nginx && rm -rf /var/lib/apt/lists/*
 
# Copy the published output from both services to the final image
COPY --from=build /app/publish/ServiceA /app/publish/ServiceA
COPY --from=build /app/publish/ServiceB /app/publish/ServiceB
 
# Copy the Nginx configuration
COPY nginx.conf /etc/nginx/nginx.conf
 
# Copy the start script
COPY start.sh .
 
# Make the start script executable
RUN chmod +x /app/start.sh
 
# Expose the application on port 80
EXPOSE 80
 
# Set the entry point to the start script
ENTRYPOINT ["/app/start.sh"]
