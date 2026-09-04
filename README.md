=====================================================================
MEMORIA DE COMANDOS DE AZURE CLI Y GUÍA DE DESPLIEGUE
=====================================================================

# 1. Instalación de Azure CLI en Windows
$msiPath = "$env:TEMP\AzureCLI.msi"
Invoke-WebRequest -Uri https://aka.ms/installazurecliwindows -OutFile $msiPath
Start-Process msiexec.exe -ArgumentList "/I `"$msiPath`" /quiet" -Wait
Remove-Item $msiPath

# 2. Inicio de Sesión y Creación del Grupo de Recursos
az login
az group create --name rg-aplicaciones-distribuidas --location eastus

# 3. Aprovisionamiento de Azure SQL Server y Bases de Datos
az provider register --namespace Microsoft.Sql

az sql server create `
  --name jose-sqlserver-distribuidas3 `
  --resource-group rg-aplicaciones-distribuidas `
  --location centralus `
  --admin-user sqladminuser `
  --admin-password "JoseAzure2026@"

az sql server firewall-rule create `
  --resource-group rg-aplicaciones-distribuidas `
  --server jose-sqlserver-distribuidas3 `
  --name AllowAzureServices `
  --start-ip-address 0.0.0.0 `
  --end-ip-address 0.0.0.0

az sql db create `
  --resource-group rg-aplicaciones-distribuidas `
  --server jose-sqlserver-distribuidas3 `
  --name CategoriaDB_A `
  --service-objective S0

az sql db create `
  --resource-group rg-aplicaciones-distribuidas `
  --server jose-sqlserver-distribuidas3 `
  --name VehiculoDB_A `
  --service-objective S0

az sql server firewall-rule create `
  --resource-group rg-aplicaciones-distribuidas `
  --server jose-sqlserver-distribuidas3 `
  --name AllowClientIP `
  --start-ip-address 0.0.0.0 `
  --end-ip-address 255.255.255.255

# 4. Creación de Azure Container Registry (ACR) y Publicación de Imágenes
az provider register --namespace Microsoft.ContainerRegistry

az acr create `
  --resource-group rg-aplicaciones-distribuidas `
  --name acraplicacionesdistribuidas `
  --sku Basic

az acr login --name acraplicacionesdistribuidas

# Ejecución desde el directorio del proyecto
# C:\Users\Personal\source\repos\deber-oauth-jwt-A>
docker-compose build
docker-compose push

# 5. Despliegue de RabbitMQ en Azure Container Instances (ACI)
az provider register --namespace Microsoft.ContainerInstance

az acr import `
  --name acraplicacionesdistribuidas `
  --source docker.io/library/rabbitmq:4-management `
  --image rabbitmq:4-management

az acr credential show --name acraplicacionesdistribuidas --query "passwords[0].value" -o tsv

az container create `
  --resource-group rg-aplicaciones-distribuidas `
  --name rabbitmq-instance `
  --image acraplicacionesdistribuidas.azurecr.io/rabbitmq:4-management `
  --registry-login-server acraplicacionesdistribuidas.azurecr.io `
  --registry-username acraplicacionesdistribuidas `
  --registry-password "2pudzsZ2szXyAIhAw8NjKZBgKDgqLY5mwTxbZXaWjESTbXkAeDYPJQQJ99CIACYeBjFEqg7NAAACAZCRgAYN" `
  --dns-name-label rabbitmq-distribuidas-jose `
  --ports 5672 15672 `
  --os-type Linux `
  --cpu 1 `
  --memory 1.5 `
  --environment-variables RABBITMQ_DEFAULT_USER=admin RABBITMQ_DEFAULT_PASS=admin123

# 6. Creación del Entorno de Azure Container Apps y Servicios
az provider register -n Microsoft.OperationalInsights --wait

az containerapp env create `
  --name app-env-distribuidas `
  --resource-group rg-aplicaciones-distribuidas `
  --location eastus

az containerapp create `
  --name oauth-service `
  --resource-group rg-aplicaciones-distribuidas `
  --environment app-env-distribuidas `
  --image acraplicacionesdistribuidas.azurecr.io/oauth-service:v1 `
  --registry-server acraplicacionesdistribuidas.azurecr.io `
  --registry-username acraplicacionesdistribuidas `
  --registry-password "2pudzsZ2szXyAIhAw8NjKZBgKDgqLY5mwTxbZXaWjESTbXkAeDYPJQQJ99CIACYeBjFEqg7NAAACAZCRgAYN" `
  --target-port 8080 `
  --ingress external `
  --env-vars ASPNETCORE_ENVIRONMENT=Development

az containerapp create `
  --name categoria-service `
  --resource-group rg-aplicaciones-distribuidas `
  --environment app-env-distribuidas `
  --image acraplicacionesdistribuidas.azurecr.io/categoria-service:v1 `
  --registry-server acraplicacionesdistribuidas.azurecr.io `
  --registry-username acraplicacionesdistribuidas `
  --registry-password "2pudzsZ2szXyAIhAw8NjKZBgKDgqLY5mwTxbZXaWjESTbXkAeDYPJQQJ99CIACYeBjFEqg7NAAACAZCRgAYN" `
  --target-port 8080 `
  --ingress external `
  --env-vars ASPNETCORE_ENVIRONMENT=Development `
             ConnectionStrings__CategoriasConnection="Server=tcp:jose-sqlserver-distribuidas3.database.windows.net,1433;Initial Catalog=CategoriaDB_A;Persist Security Info=False;User ID=sqladminuser;Password=JoseAzure2026@;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;" `
             RabbitMQ__HostName="rabbitmq-distribuidas-jose.eastus.azurecontainer.io" `
             RabbitMQ__Port="5672" `
             RabbitMQ__UserName="admin" `
             RabbitMQ__Password="admin123" `
             RabbitMQ__QueueName="categoria_creada"

az containerapp create `
  --name vehiculo-service `
  --resource-group rg-aplicaciones-distribuidas `
  --environment app-env-distribuidas `
  --image acraplicacionesdistribuidas.azurecr.io/vehiculo-service:v1 `
  --registry-server acraplicacionesdistribuidas.azurecr.io `
  --registry-username acraplicacionesdistribuidas `
  --registry-password "2pudzsZ2szXyAIhAw8NjKZBgKDgqLY5mwTxbZXaWjESTbXkAeDYPJQQJ99CIACYeBjFEqg7NAAACAZCRgAYN" `
  --target-port 8080 `
  --ingress external `
  --env-vars ASPNETCORE_ENVIRONMENT=Development `
             ConnectionStrings__VehiculosConnection="Server=tcp:jose-sqlserver-distribuidas3.database.windows.net,1433;Initial Catalog=VehiculoDB_A;Persist Security Info=False;User ID=sqladminuser;Password=JoseAzure2026@;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;" `
             RabbitMQ__HostName="rabbitmq-distribuidas-jose.eastus.azurecontainer.io" `
             RabbitMQ__Port="5672" `
             RabbitMQ__UserName="admin" `
             RabbitMQ__Password="admin123" `
             RabbitMQ__QueueName="categoria_creada"

az containerapp create `
  --name gateway-service `
  --resource-group rg-aplicaciones-distribuidas `
  --environment app-env-distribuidas `
  --image acraplicacionesdistribuidas.azurecr.io/gateway-service:v1 `
  --registry-server acraplicacionesdistribuidas.azurecr.io `
  --registry-username acraplicacionesdistribuidas `
  --registry-password "2pudzsZ2szXyAIhAw8NjKZBgKDgqLY5mwTxbZXaWjESTbXkAeDYPJQQJ99CIACYeBjFEqg7NAAACAZCRgAYN" `
  --target-port 8080 `
  --ingress external `
  --env-vars ASPNETCORE_ENVIRONMENT=Development `
             ReverseProxy__Clusters__authCluster__Destinations__authDestination__Address="https://oauth-service.app-env-distribuidas.eastus.azurecontainerapps.io/" `
             ReverseProxy__Clusters__categoriasCluster__Destinations__categoriasDestination__Address="https://categoria-service.app-env-distribuidas.eastus.azurecontainerapps.io/" `
             ReverseProxy__Clusters__vehiculosCluster__Destinations__vehiculosDestination__Address="https://vehiculo-service.app-env-distribuidas.eastus.azurecontainerapps.io/"

# 7. Actualización del Enrutamiento del Gateway (FQDN Final)
az containerapp update `
  --name gateway-service `
  --resource-group rg-aplicaciones-distribuidas `
  --set-env-vars ReverseProxy__Clusters__authCluster__Destinations__authDestination__Address="https://oauth-service.wittydune-e12820cc.eastus.azurecontainerapps.io/" `
                 ReverseProxy__Clusters__categoriasCluster__Destinations__categoriasDestination__Address="https://categoria-service.wittydune-e12820cc.eastus.azurecontainerapps.io/" `
                 ReverseProxy__Clusters__vehiculosCluster__Destinations__vehiculosDestination__Address="https://vehiculo-service.wittydune-e12820cc.eastus.azurecontainerapps.io/"


=====================================================================
DOCUMENTO ARCHIVO: README.md
=====================================================================

# Arquitectura Distribuida Segura y Despliegue en Azure

## 1. Nombre del Proyecto y Tema Asignado
- **Nombre del Proyecto:** Sistema Distribuido de Gestión Vehicular con Autenticación JWT y Mensajería Asíncrona
- **Asignatura:** Aplicaciones Distribuidas | Actividad Autónoma (AA)
- **Estudiante:** Jose Abraham Pilatuña Chushig
- **Tema Asignado:** Gestión Vehicular y Categorías

---

## 2. Diagrama o Explicación Breve de la Arquitectura

La solución implementa una arquitectura basada en microservicios desacoplada, con un servicio independiente emisor de tokens JWT, un API Gateway como punto de entrada único, dos microservicios de negocio con persistencia en Azure SQL y comunicación asíncrona mediante un bróker de mensajería (RabbitMQ).

Diagrama de Flujo:
[ Cliente / Postman ] ---> API Gateway (YARP Reverse Proxy)
                             |
         +-------------------+-------------------+
         |                   |                   |
         v                   v                   v
  OAuthJWT Service   Categoría Service   Vehículo Service
 (Token Generator)   (Emite Eventos)     (Consume Eventos)
                             |                   ^
                             +---> RabbitMQ -----+
                                     |
                             [ Azure SQL Database ]

---

## 3. Descripción de Cada Microservicio y del Servicio OAuthJWT

1. OAuthJWT (oauth-service): Servicio independiente dedicado exclusivamente a la autenticación de usuarios y emisión de tokens Bearer JWT con parámetros configurados de Issuer, Audience y Secret Key.
2. Microservicio Categoría (categoria-service): Gestiona las categorías de vehículos en la base de datos Azure SQL (CategoriaDB_A) y actúa como productor de eventos, publicando mensajes en RabbitMQ cada vez que se crea una categoría.
3. Microservicio Vehículo (vehiculo-service): Administra el inventario de vehículos en la base de datos Azure SQL (VehiculoDB_A) y actúa como consumidor de eventos de RabbitMQ para procesar la información en segundo plano.
4. API Gateway (gateway-service): Desarrollado con YARP, actúa como la puerta de entrada principal para el cliente, enrutando las peticiones hacia los microservicios internos correspondientes.
5. RabbitMQ (rabbitmq-instance): Bróker de mensajería asíncrona desplegado en ACI para la comunicación basada en el evento categoria_creada.

---

## 4. Instrucciones para Ejecutar con Docker Compose

1. Asegúrate de tener iniciado Docker Desktop en tu equipo.
2. Abre la terminal en la raíz del proyecto (C:\Users\Personal\source\repos\deber-oauth-jwt-A).
3. Compila y levanta la solución completa ejecutando:
   docker compose up --build
4. Verifica la ejecución del API Gateway local en: http://localhost:5000
5. Para detener la infraestructura local, ejecuta:
   docker compose down

---

## 5. Procedimiento para Obtener un Token JWT y Ejemplo de Uso

### Paso 1: Generar el Token JWT
Envía una petición POST al endpoint de autenticación a través del Gateway:
- URL: POST https://gateway-service.wittydune-e12820cc.eastus.azurecontainerapps.io/api/auth/login
- Body (JSON):
  {
    "username": "admin",
    "password": "123"
  }
- Respuesta: Recibirás un JSON con la propiedad "token": "eyJhbGciOi...".

### Paso 2: Demostración de Seguridad
- Petición Rechazada (Sin Token): Realiza un GET a https://gateway-service.wittydune-e12820cc.eastus.azurecontainerapps.io/api/Categorias sin incluir credenciales. Respuesta: 401 Unauthorized.
- Petición Autorizada (Con Token): En Postman, agrega en la pestaña Headers el campo Authorization: Bearer <TU_TOKEN_JWT>. Respuesta: 200 OK con los datos consultados.

---

## 6. Listado de Endpoints Principales

- Autenticación:
  - POST /api/auth/login (Obtener Token JWT)
- Microservicio Categorías:
  - GET /api/Categorias (Listar categorías - Requiere Token)
  - POST /api/Categorias (Crear categoría - Requiere Token / Emite evento a RabbitMQ)
- Microservicio Vehículos:
  - GET /api/Vehiculos (Listar vehículos - Requiere Token)
  - POST /api/Vehiculos (Crear vehículo - Requiere Token)

---

## 7. Servicios Desplegados en Azure (URLs Ejecutables)

Los recursos se mantendrán ejecutables y disponibles para revisión:

- API Gateway (Punto de Entrada): https://gateway-service.wittydune-e12820cc.eastus.azurecontainerapps.io
- OAuthJWT Service (Swagger): https://oauth-service.wittydune-e12820cc.eastus.azurecontainerapps.io/swagger/index.html
- Categoría Service (Swagger): https://categoria-service.wittydune-e12820cc.eastus.azurecontainerapps.io/swagger/index.html
- Vehículo Service (Swagger): https://vehiculo-service.wittydune-e12820cc.eastus.azurecontainerapps.io/swagger/index.html
- RabbitMQ Console: http://rabbitmq-distribuidas-jose.eastus.azurecontainer.io:15672

---

## 8. Instrucciones Breves para Detener/Eliminar los Recursos de Azure Después de la Revisión

Una vez concluida la revisión por parte del docente, todo el grupo de recursos y los componentes asociados desplegados en Azure se eliminarán mediante el siguiente comando en Azure CLI para evitar consumos adicionales:

az group delete --name rg-aplicaciones-distribuidas --yes --no-wait