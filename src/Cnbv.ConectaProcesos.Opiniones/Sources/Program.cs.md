Este ejemplo aplica para una solución de tipo Asp.net Web API con el framework .NET CORE 7.

Adicionalmente, la solución es multicapa; para los fines de esta guía, se hará referencia al proyecto principal y a un proyecto denominado **Common**.

1. Instalar en el proyecto principal y en Common, el paquete **Azure.Extensions.AspNetCore.Configuration.Secrets** en su versión **1.2.2**.

![image.png](/.attachments/image-91ae23a5-c120-4900-bdbd-5d1dd9711f27.png)

2. Generar una clase middleware que será la responsable de consumir el API de Azure Key Vaults para obtener los valores de los secretos almacenados. Para este ejemplo el nombre de la clase es **KeyVaultService.cs** y se encuentra en el proyecto **Common**.

- Se integra la referencia a la librería: `using Azure.Security.KeyVault.Secrets;`
- Se integra una propiedad que hace referencia a la clase `SecretClient` que permitirá la interacción con el servicio de AKV: `private readonly SecretClient _secretClient;`
- Se inyecta en el constructor una instancia de la clases `SecretClient` que será generada en la configuración de la clase Program.cs.