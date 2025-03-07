# Use the .NET SDK image to build the application
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /app

# Copy the project files and restore dependencies
COPY . ./
RUN dotnet restore

# Expose port for the application to run
EXPOSE 5000

# Command to start the application in watch mode for hot reload
ENTRYPOINT ["dotnet", "watch", "run", "--urls", "http://0.0.0.0:5000"]
