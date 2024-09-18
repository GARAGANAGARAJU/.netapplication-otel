#!/bin/bash

# Start ServiceA on port 80
dotnet /app/publish/ServiceA/ServiceA.dll --urls "http://0.0.0.0:80" &

# Start ServiceB on port 81
dotnet /app/publish/ServiceB/ServiceB.dll --urls "http://0.0.0.0:81"

