#!/bin/bash

cat <<EOF > appsettings.json
{
  "Logging": {
    "LogLevel": {
      "Default": "${LOG_LEVEL:-Information}",
      "Microsoft.AspNetCore": "${MS_LOG_LEVEL:-Warning}"
    }
  },
  "ApiKey": "${API_KEY}",
  "LlmService": {
    "url": "${LLM_URL}"
  },
  "MapGenService": {
    "url": "${MAPGEN_URL}"
  },
  "ConnectionStrings": {
    "Default": "${CONNECTION_STRING}"
  },
  "Jwt": {
    "Key": "${JWT_KEY}",
    "Issuer": "${JWT_ISSUER}",
    "Audience": "${JWT_AUDIENCE}",
    "ExpirationMinutes": ${JWT_EXPIRATION_MINUTES:-60}
  },
}
EOF
dotnet Api.dll migrate
dotnet Api.dll 