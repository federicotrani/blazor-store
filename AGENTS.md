# AGENTS.md

Objetivo: 
IMplementar una aplicacion con net 10 utilizando patrones de diseño y buenas practicas. Para el backend usar API rest con .net 10 y controladores. Para el frontend usar Blazor WebAssembly (standalone). Se debe implementar una aplicacion de tipo e-commerce con funcionalidad de login, registro, listado de productos, detalle de producto, listado de usuarios, detalle de usuario, etc.
Se deben implementar la conexion a la base de datos y la gestion de datos. 
Se debe implementar la gestion de usuarios y productos.
Se debe implementar la conexiones entre componentes de Blazor (frontend) y los controladores de API (backend).

Arquitectura:

Backend: API rest con .net 10 y controladores. se quieren gestionar operaciones para entidades de tipo producto, categorias (de producto), usuarios e incluir la posibilidad de upload de imagenes asociadas a producto (1 imagen por producto). El almacenamiento se realiza con base de datos sql server

- Incluir patron repositorio para gestionar acceso a datos y funcionar como intermediario para consumir datos desde los controladores
- aplicar seguridad de rutas con jwt
- incluir configuracion de CORS
- almacenar imagenes subidas de productos en carpeta uploads dentro de carpeta publica
- realizar hash de password de usuario
- conexion con base de datos sql server express local. Utilizar credenciales de windows para conectar
- utiliza esquema code-first y uso de migraciones. instalar paquetes nuget si es necesario: Microsoft.EntityFrameworkCore, Microsoft.EntityFrameworkCore.SqlServer, Microsoft.EntityFrameworkCore.Design.
- Incluir archivos: gitignore y README.md adecuados para el proyecto
- incluir swagger lista para consultar endpoints de la api

Frontend: utilizar net con Blazor WebAssembly (standalone) con mudblazor para diseño de interfaz de usuario.

- usar diseños de UI disponibles en capturas de pantalla en carpetas design del proyecto.
- Crear pantallas de login, registro, listado de productos, detalle de producto, listado de usuarios, detalle de usuario, etc.
- conectar a la api rest para consumir datos y realizar operaciones CRUD.
- Todos los texto de la interfaz de usuario debe estar en español.
