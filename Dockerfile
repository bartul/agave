# Use the official image as a parent image.
FROM mcr.microsoft.com/dotnet/sdk:8.0

# Set the working directory.
WORKDIR /app

# Copy the file or set of files in your Docker host’s current directory.
COPY . ./

RUN ls -al
# Restore the dependencies in a separate layer, this will speed up build times
RUN dotnet restore

# Build the app
RUN dotnet publish -c Release -o out

# Run the app on container startup
CMD ["dotnet", "out/Agave.Silo.dll"]
