#!/bin/bash

# Start ServiceA
dotnet /app/publish/ServiceA/ServiceA.dll &

# Start ServiceB
dotnet /app/publish/ServiceB/ServiceB.dll &

# Wait for a few seconds to ensure both services are up
sleep 10

# Start NGINX
nginx -g 'daemon off;'

